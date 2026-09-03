using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using JN.Client;
using JN.Client.Config;
using JN.Client.Messages;
using JN.Client.Model;
using JN.Client.UI;
using Newtonsoft.Json;
using QFramework;
using UnityEngine;
using ApiProtocols = global::JN.Client.Protocols.Protocols;

namespace JN.Client.Manager
{
    /// <summary>
    /// 负责数据相关的运行时逻辑。
    /// </summary>
    public partial class DataManager : MonoSingleton<DataManager>
    {
        private const int SaveVersion = 1;
        /// <summary>每位玩家在大地图可拥有的地块数（引导期仅允许建 1 店）。</summary>
        private const int DefaultLandCount = 1;
        public const int MaxTableSlotCount = 12;
        /// <summary>满该数量桌位后，每次开业菜价递增。</summary>
        public const int PostTenTablePriceGrowthTableCount = 10;
        /// <summary>满 10 桌后每次开业的价格增幅（累计，+20%）。</summary>
        public const float PostTenTablePriceGrowthPerOpen = 0.20f;
        private const int DefaultTableCount = MaxTableSlotCount;
        private const int MaxLoanCount = 4;
        private const int NextLoanStepAmount = 0;
        private const int CounterEquipmentId = 0;
        private const int StoveEquipmentId = 3;
        private const int ShopkeeperStaffId = 1;
        private const int ChefStaffId = 4;
        private const int WaiterStaffId = 5;
        private const int TownTileCount = 8;
        private const int SelfPlayerBuildingId = LocalSaveMode.DefaultPlayerId;
        private const int TownLandPurchaseCost = 3000;
        private const string CoinLogColor = "#FFA500";

        private static string SavePath => LocalSaveStore.ActiveSavePath;
        private static string SaveDirectoryPath => LocalSaveStore.SaveDirectoryPath;

        public GameSaveData SaveData { get; private set; }
        public PlayerModel PlayerData { get; private set; }
        public string AuthToken { get; private set; }

        public int tableNum { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool HasCreatedPlayer => PlayerData != null && !string.IsNullOrWhiteSpace(PlayerData.playerName?.Trim());

        public LocalGameplaySaveData GameplayData
        {
            get
            {
                EnsureInitialized();
                return SaveData.gameplay;
            }
        }

        public TavernSaveData TavernData
        {
            get
            {
                EnsureInitialized();
                return SaveData.tavern;
            }
        }

        public TownSaveData TownData
        {
            get
            {
                EnsureInitialized();
                return SaveData.town;
            }
        }

        public GameplayGuideSaveData GameplayGuideData
        {
            get
            {
                EnsureGameplayDefaults();
                SyncGameplayGuideProgress();
                return SaveData.gameplay.gameplayGuide;
            }
        }

        /// <summary>
        /// 初始化模块依赖和默认状态。
        /// </summary>
        public void Init()
        {
            EnsureInitialized();
        }

        public void SetAuthToken(string token)
        {
            AuthToken = string.IsNullOrWhiteSpace(token) ? null : token.Trim();
        }

        /// <summary>
        /// 获取当前玩家剩余可贷款次数。
        /// </summary>
        /// <returns>返回计算后的数值。</returns>
        public int GetRemainingLoanCount()
        {
            EnsureInitialized();
            return Mathf.Max(0, MaxLoanCount - GameplayData.loanCount);
        }

        /// <summary>
        /// 获取下一次贷款金额。
        /// </summary>
        /// <returns>返回计算后的数值。</returns>
        public int GetNextLoanAmount()
        {
            EnsureInitialized();
            return TbConfigRuntime.GetOpeningLoanAmount(500) + (GameplayData.loanCount * NextLoanStepAmount);
        }

        /// <summary>
        /// 判断是否已经领取开局贷款。
        /// </summary>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        public bool HasClaimedOpeningLoan()
        {
            EnsureInitialized();
            return GameplayData.openingLoanClaimed;
        }

        /// <summary>
        /// 判断是否需要显示开局贷款窗口。
        /// </summary>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        public bool ShouldShowOpeningLoanWindow()
        {
            EnsureInitialized();
            return !GameplayData.openingLoanClaimed;
        }

        /// <summary>
        /// 修改铜钱数量。
        /// </summary>
        /// <param name="change数量">参数值。</param>
        public void ChangeCoinNum(int changeNum)
        {
            EnsureInitialized();

            var beforeNum = PlayerData.coinNum;
            var afterNum = PlayerData.coinNum + changeNum;
            if (afterNum < 0)
            {
                return;
            }

            PlayerData.coinNum = afterNum;
            if (changeNum < 0 && SaveData.gameplay.shopOpened)
            {
                SaveData.gameplay.pendingSettlementCosts += -changeNum;
            }

            Debug.Log($"<color={CoinLogColor}>[Coin Change] Change={changeNum:+#;-#;0} Before={beforeNum} After={afterNum}</color>");
            Signals.Get<UpdateCoinNumSignal>().Dispatch(changeNum);
            SaveGame();

            // 本地存档模式或无鉴权令牌时，不再同步服务器金币。
            if (LocalSaveMode.Enabled || string.IsNullOrWhiteSpace(AuthToken))
            {
                return;
            }

            ApiProtocols.Instance.UpdatePlayerCoins(
                AuthToken,
                afterNum,
                onSuccess: response => { PlayerData.coinNum = response.coins; },
                onError: error => { Debug.LogWarning($"[DataManager] 保存当前金币数量失败：{error}"); });
        }

        /// <summary>
        /// 为当前营业结算追加一笔不立即扣钱的支出。
        /// </summary>
        /// <param name="cost">支出金额。</param>
        public void AddPendingSettlementCost(int cost)
        {
            EnsureInitialized();
            if (cost <= 0)
            {
                return;
            }

            SaveData.gameplay.pendingSettlementCosts += cost;
            SaveGame();
        }

        /// <summary>
        /// 创建新玩家本地数据并写入初始存档。
        /// </summary>
        /// <param name="playerName">名称。</param>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        public bool CreatePlayer(string playerName, int userId)
        {
            return LoginOrCreatePlayer(playerName, userId);
        }

        /// <summary>
        /// 登录或创建玩家。
        /// </summary>
        /// <param name="playerName">名称。</param>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        public bool LoginOrCreatePlayer(string playerName, int userId)
        {
            EnsureInitialized();

            var trimmedName = NormalizePlayerName(playerName);
            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                return false;
            }

            var namedSavePath = GetNamedSavePath(trimmedName);
            if (File.Exists(namedSavePath) && TryLoadSaveFromPath(namedSavePath, out var existingSave))
            {
                SaveData = existingSave;
            }
            else
            {
                SaveData = CreateDefaultSave();
                SaveData.player.createdAtUtcTicks = DateTime.UtcNow.Ticks;
            }

            SaveData.player ??= new PlayerModel();
            SaveData.gameplay ??= new LocalGameplaySaveData();
            SaveData.town ??= new TownSaveData();
            SaveData.tavern ??= new TavernSaveData();

            SaveData.player.playerName = trimmedName;

            if (LocalSaveMode.Enabled)
            {
                // 本地模式：优先保留已有存档中的玩家编号，否则使用默认本地编号。
                SetAuthToken(null);
                if (!int.TryParse(SaveData.player.playerId, out var existingPlayerId) || existingPlayerId <= 0)
                {
                    SaveData.player.playerId = (userId > 0 ? userId : SelfPlayerBuildingId).ToString();
                }
            }
            else if (userId > 0)
            {
                SaveData.player.playerId = userId.ToString();
            }
            else if (string.IsNullOrWhiteSpace(SaveData.player.playerId))
            {
                SaveData.player.playerId = Guid.NewGuid().ToString("N");
            }

            if (SaveData.player.createdAtUtcTicks <= 0)
            {
                SaveData.player.createdAtUtcTicks = DateTime.UtcNow.Ticks;
            }

            PlayerData = SaveData.player;
            SaveData.gameplay.localPlayerNumericId = ResolveLocalPlayerNumericId(SaveData.gameplay.localPlayerNumericId);

            EnsureTownBuildingDefaults();
            EnsureTavernDefaults();
            EnsureGameplayDefaults();
            tableNum = GetUnlockedTableCount();
            SaveGame();
            return true;
        }

        /// <summary>
        /// 列出本机全部本地存档摘要。
        /// </summary>
        public List<LocalSaveFileInfo> ListLocalSaves()
        {
            return LocalSaveStore.ListSaves();
        }

        /// <summary>
        /// 删除指定本地存档文件；若删的是当前活动槽，会重建空存档。
        /// </summary>
        public bool DeleteLocalSave(string path, out string message)
        {
            if (!LocalSaveStore.DeleteSave(path, out var error))
            {
                message = error;
                return false;
            }

            var deletedActive = string.Equals(path, SavePath, StringComparison.OrdinalIgnoreCase);
            var deletedCurrentNamed = HasCreatedPlayer
                && string.Equals(path, GetNamedSavePath(PlayerData.playerName), StringComparison.OrdinalIgnoreCase);

            if (deletedActive || deletedCurrentNamed)
            {
                ResetToEmptySave();
            }

            message = "存档已删除";
            return true;
        }

        /// <summary>
        /// 删除全部本地存档并重置内存中的空存档。
        /// </summary>
        public int DeleteAllLocalSaves()
        {
            var deleted = LocalSaveStore.DeleteAllSaves();
            ResetToEmptySave();
            return deleted;
        }

        /// <summary>
        /// 将内存状态重置为默认空存档并立即写入活动槽。
        /// </summary>
        public void ResetToEmptySave()
        {
            SetAuthToken(null);
            SaveData = CreateDefaultSave();
            PlayerData = SaveData.player;
            EnsureTownBuildingDefaults();
            EnsureTavernDefaults();
            EnsureGameplayDefaults();
            tableNum = GetUnlockedTableCount();
            IsInitialized = true;
            SaveGame();
        }

        /// <summary>
        /// 获取恢复场景名称。
        /// </summary>
        /// <returns>返回方法执行后的结果。</returns>
        public string GetResumeSceneName()
        {
            EnsureInitialized();
            if (string.IsNullOrWhiteSpace(SaveData.lastSceneName))
            {
                return "Town";
            }

            return SaveData.lastSceneName switch
            {
                "SCN_Town_Main" => "Town",
                "SCN_Tavern_Gameplay" => "GamePlay_TavernWJ",
                "Tavern_Gameplay" => "GamePlay_TavernWJ",
                "GamePlay_Tavern" => "GamePlay_TavernWJ",
                "GamePlay_Tavern2WJ" => "GamePlay_TavernWJ",
                _ => SaveData.lastSceneName
            };
        }

        /// <summary>
        /// 记录上一次场景。
        /// </summary>
        /// <param name="sceneName">名称。</param>
        public void RecordLastScene(string sceneName)
        {
            EnsureInitialized();
            if (string.IsNullOrWhiteSpace(sceneName) || sceneName == "Start" || sceneName == "SCN_Common_Start")
            {
                return;
            }

            SaveData.lastSceneName = sceneName switch
            {
                "SCN_Town_Main" => "Town",
                "SCN_Tavern_Gameplay" => "GamePlay_TavernWJ",
                "Tavern_Gameplay" => "GamePlay_TavernWJ",
                "GamePlay_Tavern" => "GamePlay_TavernWJ",
                "GamePlay_Tavern2WJ" => "GamePlay_TavernWJ",
                _ => sceneName
            };
            SaveGame();
        }

        /// <summary>
        /// 获取大地图建筑信息列表。
        /// </summary>
        /// <returns>返回方法执行后的结果。</returns>
        public List<BuildingInfo> GetTownBuildingInfos()
        {
            EnsureTownBuildingDefaults();
            return SaveData.town.buildingInfos;
        }

        /// <summary>
        /// 处理新增或更新建筑信息相关逻辑。
        /// </summary>
        /// <param name="building信息">参数值。</param>
        public void UpsertBuildingInfo(BuildingInfo buildingInfo)
        {
            if (buildingInfo == null)
            {
                return;
            }

            EnsureTownBuildingDefaults();
            var buildingInfos = SaveData.town.buildingInfos;
            var existingIndex = buildingInfos.FindIndex(info => info.tileId == buildingInfo.tileId);
            if (existingIndex >= 0)
            {
                buildingInfos[existingIndex] = CloneBuildingInfo(buildingInfo);
            }
            else
            {
                buildingInfos.Add(CloneBuildingInfo(buildingInfo));
            }

            SaveGame();
        }

        /// <summary>
        /// 处理是否拥有大地图建筑相关逻辑。
        /// </summary>
        /// <param name="playerId">数据编号。</param>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        public bool HasOwnedTownBuilding(int playerId = SelfPlayerBuildingId)
        {
            EnsureTownBuildingDefaults();
            if (playerId <= 0)
            {
                return false;
            }
            return SaveData.town.buildingInfos.Exists(info => info != null && info.playerId == playerId);
        }

        /// <summary>
        /// 判断当前玩家购买过大地图地块是否到上限，包含未建造和建造中的地块。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>到达上限时返回 true。</returns>
        public bool IsTownLandCountAtLimit(int playerId, out int hasLandCount)
        {
            hasLandCount = 0;
            EnsureTownBuildingDefaults();
            if (playerId <= 0)
            {
                return false;
            }

            foreach (var info in SaveData.town.buildingInfos)
            {
                if (info.playerId == playerId)
                {
                    hasLandCount++;
                }
            }
            return hasLandCount >= DefaultLandCount;
        }

        /// <summary>
        /// 判断当前玩家是否已经拥有建成建筑。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>存在已建成建筑时返回 true。</returns>
        public bool HasCompletedTownBuilding(int playerId)
        {
            EnsureTownBuildingDefaults();
            if (playerId <= 0)
            {
                return false;
            }

            return SaveData.town.buildingInfos.Exists(info => info != null
                                                              && info.playerId == playerId
                                                              && info.status == 2
                                                              && info.buildingLevel > 0);
        }

        /// <summary>
        /// 自家酒楼地块是否已完工（用于城镇底部「进入酒楼」显隐）。
        /// </summary>
        public bool HasCompletedOwnedSelfTownBuilding()
        {
            return GetCompletedOwnedSelfTownBuilding() != null;
        }

        /// <summary>
        /// 获取已完工的自家酒楼建筑（优先 selfBuildingFieldId；否则匹配当前玩家 playerId）。
        /// </summary>
        public BuildingInfo GetCompletedOwnedSelfTownBuilding()
        {
            EnsureTownBuildingDefaults();
            var selfPlayerId = ResolveCurrentPlayerId();
            BuildingInfo byField = null;
            BuildingInfo byPlayer = null;
            for (var index = 0; index < SaveData.town.buildingInfos.Count; index++)
            {
                var info = SaveData.town.buildingInfos[index];
                if (info == null || info.status != 2 || info.buildingLevel <= 0 || info.playerId <= 0)
                {
                    continue;
                }

                if (IsSelfTownBuildingField(info.tileId))
                {
                    byField = info;
                    break;
                }

                if (selfPlayerId > 0 && info.playerId == selfPlayerId && byPlayer == null)
                {
                    byPlayer = info;
                }
            }

            return byField ?? byPlayer;
        }

        /// <summary>
        /// 获取已拥有大地图建筑。
        /// </summary>
        /// <param name="playerId">数据编号。</param>
        /// <returns>返回方法执行后的结果。</returns>
        public BuildingInfo GetOwnedTownBuilding(int playerId = SelfPlayerBuildingId)
        {
            EnsureTownBuildingDefaults();
            if (playerId <= 0)
            {
                return null;
            }

            return SaveData.town.buildingInfos.Find(info => info != null && info.playerId == playerId);
        }

        /// <summary>
        /// 获取购买大地图地块需要的铜钱数量。
        /// 首块开店地免费。
        /// </summary>
        public int GetTownLandPurchaseCost(int tileId = 0)
        {
            var selfPlayerId = ResolveCurrentPlayerId();
            if (selfPlayerId > 0 && !HasOwnedTownBuilding(selfPlayerId))
            {
                return 0;
            }

            return TownLandPurchaseCost;
        }

        /// <summary>
        /// 尝试购买大地图地块；每个玩家最多只能拥有一个地块。
        /// </summary>
        /// <param name="tileId">地块编号。</param>
        /// <param name="message">返回失败或成功原因。</param>
        /// <returns>购买成功时返回 true。</returns>
        public bool TryPurchaseTownLand(int tileId, out string message)
        {
            EnsureTownBuildingDefaults();
            var selfPlayerId = ResolveCurrentPlayerId();
            if (selfPlayerId <= 0)
            {
                message = "当前玩家数据异常，无法购买地块";
                return false;
            }

            if (IsTownLandCountAtLimit(selfPlayerId, out int hasLandCount))
            {
                message = $"每位玩家暂时只能拥有{hasLandCount}个地块";
                return false;
            }

            var buildingInfo = SaveData.town.buildingInfos.Find(info => info != null && info.tileId == tileId);
            if (buildingInfo == null)
            {
                message = "未找到目标地块";
                return false;
            }

            if (!IsSelfTownBuildingField(tileId))
            {
                message = $"只能在地块 {GetSelfBuildingFieldId()} 建造自家酒楼";
                return false;
            }

            if (buildingInfo.playerId != 0)
            {
                message = "该地块已被购买";
                return false;
            }

            var purchaseCost = GetTownLandPurchaseCost(tileId);
            if (PlayerData.coinNum < purchaseCost)
            {
                message = $"铜钱不足，购买地块需要 {purchaseCost}";
                return false;
            }

            ChangeCoinNum(-purchaseCost);
            buildingInfo.playerId = selfPlayerId;
            buildingInfo.name = PlayerData.playerName;
            buildingInfo.buildingId = 0;
            buildingInfo.buildingLevel = 0;
            buildingInfo.buildingTime = 0;
            buildingInfo.status = 0;
            PlayerData.buildId = tileId;
            SaveGame();
            message = "地块购买成功";
            return true;
        }

        /// <summary>
        /// 尝试在已购买地块上开始建造建筑。
        /// </summary>
        /// <param name="tileId">地块编号。</param>
        /// <param name="buildingLevel">建筑等级。</param>
        /// <param name="coinChange">铜钱变化值，负数表示花费。</param>
        /// <param name="buildDuration">建造持续时间。</param>
        /// <param name="message">返回失败或成功原因。</param>
        /// <returns>开始建造成功时返回 true。</returns>
        public bool TryStartTownBuilding(int tileId, int buildingLevel, int coinChange, int buildDuration, out string message)
        {
            EnsureTownBuildingDefaults();
            var selfPlayerId = ResolveCurrentPlayerId();
            if (!IsSelfTownBuildingField(tileId))
            {
                message = $"只能在地块 {GetSelfBuildingFieldId()} 建造自家酒楼";
                return false;
            }

            var buildingInfo = SaveData.town.buildingInfos.Find(info => info != null && info.tileId == tileId);
            if (buildingInfo == null || buildingInfo.playerId != selfPlayerId)
            {
                message = "请先购买该地块";
                return false;
            }

            if (buildingInfo.buildingLevel > 0 || buildingInfo.status != 0)
            {
                message = "该地块已经开始建造";
                return false;
            }

            if (PlayerData.coinNum + coinChange < 0)
            {
                message = "铜钱不足，无法建造该建筑";
                return false;
            }

            ChangeCoinNum(coinChange);
            buildingInfo.name = PlayerData.playerName;
            buildingInfo.buildingId = 1;
            buildingInfo.buildingLevel = Mathf.Clamp(buildingLevel, 1, 3);
            buildingInfo.buildingTime = Mathf.Max(0, buildDuration);
            buildingInfo.status = buildingInfo.buildingTime > 0 ? 1 : 2;
            UpsertBuildingInfo(buildingInfo);
            message = "建筑开始建造";
            return true;
        }

        /// <summary>
        /// 设置当前已拥有建筑。
        /// </summary>
        /// <param name="tileId">数据编号。</param>
        /// <param name="buildingLevel">等级。</param>
        public void SetActiveOwnedBuilding(int tileId, int buildingLevel)
        {
            EnsureGameplayDefaults();

            SaveData.gameplay.activeShopId = (byte)Mathf.Clamp(tileId, 0, byte.MaxValue);
            SaveData.gameplay.ownedShops.Clear();
            SaveData.gameplay.ownedShops.Add(new LocalShopSaveData
            {
                mapSlotIndex = (byte)Mathf.Clamp(tileId, 0, byte.MaxValue),
                shopTypeId = 1,
                shopLevel = (byte)Mathf.Clamp(buildingLevel, 1, byte.MaxValue)
            });

            SaveGame();
        }

        /// <summary>
        /// 解析当前登录玩家的数字编号。
        /// </summary>
        /// <returns>解析成功返回玩家编号，否则返回 0。</returns>
        private int ResolveCurrentPlayerId()
        {
            if (PlayerData == null)
            {
                return 0;
            }

            return int.TryParse(PlayerData.playerId, out var playerId) ? playerId : 0;
        }

        /// <summary>
        /// 获取桌位数据。
        /// </summary>
        /// <param name="桌位编号">数据编号。</param>
        /// <returns>返回方法执行后的结果。</returns>
        public TavernTableSaveData GetTableData(int tableId)
        {
            EnsureTavernDefaults();
            return SaveData.tavern.tables.Find(table => table.tableId == tableId);
        }

        /// <summary>
        /// 获取全部桌位数据。
        /// </summary>
        /// <returns>返回方法执行后的结果。</returns>
        public IReadOnlyList<TavernTableSaveData> GetAllTableData()
        {
            EnsureTavernDefaults();
            return SaveData.tavern.tables;
        }

        /// <summary>
        /// 获取已解锁桌位数量。
        /// </summary>
        /// <returns>返回计算后的数值。</returns>
        public int GetUnlockedTableCount()
        {
            EnsureTavernDefaults();
            return SaveData.tavern.tables.FindAll(table => table.isUnlocked).Count;
        }

        /// <summary>酒店星级上限。</summary>
        public const int MaxTavernLevel = 4;

        /// <summary>
        /// 当前酒店星级（0~4，默认 0）。由声望升级提升。
        /// </summary>
        public int GetTavernLevel()
        {
            EnsureTavernDefaults();
            return Mathf.Clamp(SaveData.tavern.tavernLevel, 0, MaxTavernLevel);
        }

        /// <summary>
        /// 当前场景酒楼星级是否已达 2 级及以上。
        /// 拜访他人酒楼时按对方星级判定。
        /// </summary>
        public bool IsTavernExpandedToLevel2()
        {
            return GetSceneTavernLevelForSpawn() >= 2;
        }

        /// <summary>
        /// 左侧 wall01 是否已完成扩建（付费或三星默认）。
        /// </summary>
        public bool IsInteriorWallExpanded()
        {
            EnsureTavernDefaults();
            return SaveData?.tavern != null && SaveData.tavern.interiorWallExpanded;
        }

        /// <summary>
        /// 是否应显示墙体扩建按钮：自家、二星及以上、尚未扩建。
        /// </summary>
        public bool ShouldShowInteriorWallExpandButton()
        {
            if (IsVisitingOtherTavern || SaveData?.tavern == null)
            {
                return false;
            }

            return GetTavernLevel() >= 2 && !IsInteriorWallExpanded();
        }

        /// <summary>
        /// 尝试付费扩建左侧墙体。
        /// </summary>
        public bool TryPurchaseInteriorWallExpand(out string message)
        {
            EnsureTavernDefaults();
            if (IsVisitingOtherTavern)
            {
                message = "访客模式下不可扩建";
                return false;
            }

            if (GetTavernLevel() < 2)
            {
                message = "酒楼达到2星后可扩建";
                return false;
            }

            if (IsInteriorWallExpanded())
            {
                message = "已扩建";
                return false;
            }

            var cost = TbConfigRuntime.GetTavernExpandCost();
            if (PlayerData.coinNum < cost)
            {
                message = $"铜钱不足，扩建需要{cost}";
                return false;
            }

            ChangeCoinNum(-cost);
            SaveData.tavern.interiorWallExpanded = true;
            SaveData.tavern.tavernExpandLevel = Mathf.Max(SaveData.tavern.tavernExpandLevel, 2);
            SaveGame();
            NotifyAchievementStatsChanged();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            Signals.Get<GameplayGuideProgressSignal>().Dispatch();
            message = "扩建成功";
            return true;
        }

        /// <summary>当前累计声望。</summary>
        public int GetTavernPrestige()
        {
            EnsureTavernDefaults();
            return Mathf.Max(0, SaveData.tavern.tavernPrestige);
        }

        /// <summary>升到下一星所需累计声望；已满星返回当前阈值。</summary>
        public int GetNextTavernPrestigeRequirement()
        {
            var level = GetTavernLevel();
            if (level >= MaxTavernLevel)
            {
                return TbConfigRuntime.GetUpgradeLevelPrestige(MaxTavernLevel, 99999);
            }

            return TbConfigRuntime.GetUpgradeLevelPrestige(
                level + 1,
                ResolveDefaultUpgradePrestige(level + 1));
        }

        private static int ResolveDefaultUpgradePrestige(int targetLevel)
        {
            return targetLevel switch
            {
                1 => 700,
                2 => 1700,
                3 => 3100,
                4 => 99999,
                _ => 99999
            };
        }

        /// <summary>升到下一星所需金币（读 upgradeLevelMoney，与声望档位一致）。</summary>
        public int GetNextTavernUpgradeMoneyCost()
        {
            var level = GetTavernLevel();
            if (level >= MaxTavernLevel)
            {
                return 0;
            }

            return TbConfigRuntime.GetUpgradeLevelMoney(level + 1);
        }

        /// <summary>声望是否已达下一星门槛（满星为 false）。升级时另扣 upgradeLevelMoney。</summary>
        public bool CanUpgradeTavernPrestigeLevel()
        {
            EnsureTavernDefaults();
            if (IsVisitingOtherTavern)
            {
                return false;
            }

            var level = GetTavernLevel();
            if (level >= MaxTavernLevel)
            {
                return false;
            }

            return GetTavernPrestige() >= GetNextTavernPrestigeRequirement();
        }

        /// <summary>增加声望并刷新 HUD；飘字「声望+XXX」。</summary>
        public void AddTavernPrestige(int amount)
        {
            if (amount <= 0 || IsVisitingOtherTavern)
            {
                return;
            }

            EnsureTavernDefaults();
            SaveData.tavern.tavernPrestige = Mathf.Max(0, SaveData.tavern.tavernPrestige + amount);
            SaveGame();
            Signals.Get<TavernPrestigeChangedSignal>().Dispatch();
            HudOverlayService.ShowFloatingWarning($"声望+{amount}");
        }

        /// <summary>完成一桌：普客/贵客按 Config 发放声望。</summary>
        public void AddPrestigeForCompletedTable(bool hasVipCustomer)
        {
            var amount = hasVipCustomer
                ? TbConfigRuntime.GetVipCustomerTablePrestige(50)
                : TbConfigRuntime.GetNormalCustomerTablePrestige(10);
            AddTavernPrestige(amount);
        }

        /// <summary>声望满进度后提升酒店星级（tavernLevel），并扣除 upgradeLevelMoney。</summary>
        public bool TryUpgradeTavernPrestigeLevel(out string message)
        {
            message = string.Empty;
            EnsureTavernDefaults();
            if (IsVisitingOtherTavern)
            {
                message = "访客模式下不可升级";
                return false;
            }

            var level = GetTavernLevel();
            if (level >= MaxTavernLevel)
            {
                message = "已达最高星级";
                return false;
            }

            if (GetTavernPrestige() < GetNextTavernPrestigeRequirement())
            {
                message = "声望不足，无法升级";
                return false;
            }

            var moneyCost = GetNextTavernUpgradeMoneyCost();
            if (PlayerData == null || PlayerData.coinNum < moneyCost)
            {
                message = moneyCost > 0 ? $"金币不足，升级需要 {moneyCost}" : "金币不足，无法升级";
                return false;
            }

            if (moneyCost > 0)
            {
                ChangeCoinNum(-moneyCost);
            }

            SaveData.tavern.tavernLevel = level + 1;
            if (SaveData.tavern.tavernLevel >= 3)
            {
                SaveData.tavern.interiorWallExpanded = true;
            }

            SaveGame();
            NotifyAchievementStatsChanged();
            Signals.Get<TavernPrestigeChangedSignal>().Dispatch();
            Signals.Get<GameplayGuideProgressSignal>().Dispatch();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            message = $"酒楼升至 {SaveData.tavern.tavernLevel} 星";
            return true;
        }

        /// <summary>
        /// 处理解锁桌位相关逻辑。
        /// </summary>
        /// <param name="桌位编号">数据编号。</param>
        public void UnlockTable(int tableId)
        {
            var tableData = GetTableData(tableId);
            if (tableData == null)
            {
                return;
            }

            tableData.isUnlocked = true;
            tableData.runtimeState = (int)TavernTableRuntimeState.Idle;
            tableNum = GetUnlockedTableCount();
            GrantFacilityBuildPrestige(FacilityConfigUtility.GetTableFacility(tableId));
            SyncGameplayGuideProgress();
            Signals.Get<TableNumSignal>().Dispatch();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            Signals.Get<GameplayGuideProgressSignal>().Dispatch();
            NotifyAchievementStatsChanged();
            SaveGame();
        }

        /// <summary>
        /// 桌子允许的最高等级，UI 与升级逻辑共用同一上限。
        /// </summary>
        public const int MaxTableLevel = 3;

        /// <summary>
        /// 升级指定桌位等级，达到最高等级时不再叠加。
        /// </summary>
        /// <param name="tableId">桌位编号。</param>
        /// <returns>真实发生升级时返回 true，已经满级或桌位无效时返回 false。</returns>
        public bool UpgradeTable(int tableId)
        {
            var tableData = GetTableData(tableId);
            if (tableData == null || !tableData.isUnlocked)
            {
                return false;
            }

            var currentLevel = Mathf.Max(1, tableData.level);
            if (currentLevel >= MaxTableLevel)
            {
                return false;
            }

            tableData.level = currentLevel + 1;
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            SaveGame();
            return true;
        }

        /// <summary>
        /// 设置桌位运行时状态。
        /// </summary>
        /// <param name="桌位编号">数据编号。</param>
        /// <param name="state">参数值。</param>
        public void SetTableRuntimeState(int tableId, TavernTableRuntimeState state)
        {
            var tableData = GetTableData(tableId);
            if (tableData == null)
            {
                return;
            }

            tableData.runtimeState = (int)state;
            // 拜访他人店只改内存桌态供模拟用，不落盘，避免污染自家桌态与快照。
            if (IsVisitingOtherTavern)
            {
                return;
            }

            SaveGame();
        }

        /// <summary>
        /// 添加桌位收入。
        /// </summary>
        /// <param name="桌位编号">数据编号。</param>
        /// <param name="income">参数值。</param>
        public void AddTableIncome(int tableId, int income)
        {
            var tableData = GetTableData(tableId);
            if (tableData == null)
            {
                return;
            }

            tableData.totalIncome += income;
            tableData.totalServedCustomers += 1;
            SaveData.tavern.totalIncome += income;
            SaveData.tavern.totalServedCustomers += 1;
            SaveData.gameplay.dailyRevenue += income;
            if (income > 0)
            {
                SaveData.gameplay.pendingSettlementIncome += income;
            }

            SaveData.gameplay.totalDepositedIncome += income;
            NotifyAchievementStatsChanged();
            TavernFeatureUnlockPresenter.TryRevealAchievementEntry();
            TavernFeatureUnlockPresenter.TryRevealTechEntry();
            SaveGame();
        }

        /// <summary>
        /// 掌柜柜台随机收益：加币并计入本营业日结算收入。
        /// </summary>
        public void GrantCounterRandomRewardIncome(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            EnsureGameplayDefaults();
            SaveData.gameplay.dailyRevenue += amount;
            SaveData.gameplay.pendingSettlementIncome += amount;
            SaveData.gameplay.totalDepositedIncome += amount;
            EnsureTavernDefaults();
            SaveData.tavern.totalIncome += amount;
            ChangeCoinNum(amount);
            NotifyAchievementStatsChanged();
        }

        /// <summary>
        /// 当前是否已经解锁桌子升级功能。
        /// </summary>
        /// <returns>已解锁时返回 true。</returns>
        public bool IsTableLv2UpgradeUnlocked()
        {
            EnsureTavernDefaults();
            return SaveData.tavern.tableLv2UpgradeUnlocked;
        }

        /// <summary>
        /// 标记桌子升级功能已解锁。
        /// </summary>
        public void UnlockTableLv2Upgrade()
        {
            EnsureTavernDefaults();
            if (SaveData.tavern.tableLv2UpgradeUnlocked)
            {
                return;
            }

            SaveData.tavern.tableLv2UpgradeUnlocked = true;
            SaveGame();
        }

        /// <summary>
        /// 设置酒楼开业状态并通知场景刷新。
        /// </summary>
        /// <param name="isOpen">是否开业。</param>
        /// <param name="countAsNewRound">为 true 时累计开业轮次（仅首次点开业或三分钟循环续轮时需要）。</param>
        public void SetTavernOpen(bool isOpen, bool countAsNewRound = true)
        {
            EnsureTavernDefaults();
            SaveData.tavern.isOpen = isOpen;
            SaveData.gameplay.shopOpened = isOpen;

            if (isOpen)
            {
                SaveData.gameplay.shopOpenDuration = 0f;
                SaveData.gameplay.dailyRevenue = 0;
                SaveData.gameplay.pendingSettlementIncome = 0;
                SaveData.gameplay.pendingSettlementCosts = 0;
                ResetClosingSessionStats();
                if (countAsNewRound)
                {
                    IncrementBusinessOpenRoundInternal(saveImmediately: false);
                }

                GameplayGuideData.onboardingCompleted = true;
                NotifyAchievementStatsChanged();
            }

            Signals.Get<TavernBusinessStateSignal>().Dispatch(isOpen);
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            Signals.Get<GameplayGuideProgressSignal>().Dispatch();
            SaveGame();
        }

        /// <summary>
        /// 三分钟营业循环续轮：累加开业轮次并落盘（不切换 isOpen）。
        /// </summary>
        public void AdvanceBusinessOpenRound()
        {
            EnsureTavernDefaults();
            IncrementBusinessOpenRoundInternal(saveImmediately: true);
            AdvanceSessionBusinessTurn();
            NotifyAchievementStatsChanged();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
        }

        private void IncrementBusinessOpenRoundInternal(bool saveImmediately)
        {
            SaveData.gameplay.businessOpenCount = Mathf.Max(0, SaveData.gameplay.businessOpenCount) + 1;
            if (GetUnlockedTableCount() >= PostTenTablePriceGrowthTableCount)
            {
                SaveData.gameplay.postTenTableBusinessOpenCount = Mathf.Max(
                    0,
                    SaveData.gameplay.postTenTableBusinessOpenCount) + 1;
            }

            if (saveImmediately)
            {
                SaveGame();
            }
        }

        /// <summary>
        /// 是否允许购买设施：已开业过则营业中可买；未开业引导期也可买。无单独停业购买流程。
        /// </summary>
        public bool AllowsFacilityPurchaseNow()
        {
            EnsureTavernDefaults();
            if (IsVisitingOtherTavern || SaveData?.tavern == null)
            {
                return false;
            }

            return !SaveData.tavern.isOpen || GetBusinessOpenCount() > 0;
        }

        /// <summary>
        /// 确保初始化状态。
        /// </summary>
        private void EnsureInitialized()
        {
            if (IsInitialized && SaveData != null)
            {
                return;
            }

            LoadOrCreateSave();
            IsInitialized = true;
        }

        /// <summary>
        /// 确保核心初始化状态。
        /// </summary>
        private void EnsureInitializedCore()
        {
            if (SaveData != null)
            {
                return;
            }

            LoadOrCreateSave();
        }

        /// <summary>
        /// 解析本地玩家数字编号。
        /// </summary>
        /// <param name="currentValue">参数值。</param>
        /// <returns>返回方法执行后的结果。</returns>
        private ushort ResolveLocalPlayerNumericId(ushort currentValue)
        {
            if (currentValue > 0)
            {
                return currentValue;
            }

            var seed = !string.IsNullOrWhiteSpace(PlayerData?.playerId)
                ? PlayerData.playerId
                : NormalizePlayerName(PlayerData?.playerName);

            if (string.IsNullOrWhiteSpace(seed))
            {
                return 1;
            }

            unchecked
            {
                var hash = 17;
                for (var index = 0; index < seed.Length; index++)
                {
                    hash = (hash * 31) + seed[index];
                }

                return (ushort)Mathf.Clamp(Math.Abs(hash % 60000) + 1, 1, ushort.MaxValue);
            }
        }

        /// <summary>
        /// 规范化玩家名称。
        /// </summary>
        /// <param name="playerName">名称。</param>
        /// <returns>返回方法执行后的结果。</returns>
        private static string NormalizePlayerName(string playerName)
        {
            return string.IsNullOrWhiteSpace(playerName) ? null : playerName.Trim();
        }

        /// <summary>
        /// 获取命名存档路径。
        /// </summary>
        /// <param name="playerName">名称。</param>
        /// <returns>返回方法执行后的结果。</returns>
        private static string GetNamedSavePath(string playerName)
        {
            return LocalSaveStore.GetNamedSavePath(playerName);
        }

        /// <summary>
        /// 响应应用聚焦事件并同步状态。
        /// </summary>
        /// <param name="focus">参数值。</param>
        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                SaveGame();
            }
        }

        /// <summary>
        /// 响应应用暂停事件并同步状态。
        /// </summary>
        /// <param name="pause状态">参数值。</param>
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGame();
            }
        }

        /// <summary>
        /// 响应应用退出事件并同步状态。
        /// </summary>
        protected override void OnApplicationQuit()
        {
            SaveGame();
        }
    }
}
