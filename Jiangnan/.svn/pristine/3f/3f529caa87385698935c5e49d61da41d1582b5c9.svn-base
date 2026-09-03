using JN.Client;
using JN.Client.Config;
using JN.Client.Messages;
using JN.Client.Model;
using JN.Client.UI;
using QFramework;
using System.Collections.Generic;
using UnityEngine;
using cfg;

namespace JN.Client.Manager
{
    /// <summary>
    /// 负责数据相关的运行时逻辑。
    /// </summary>
    public partial class DataManager
    {
        public const int MaxGuideChefCount = 3;
        public const int MaxGuideWaiterCount = 3;
        private const int DefaultGuideOpeningTableCount = 2;
        /// <summary>酒楼 LV1 引导期可同时半透展示/购买的最大桌号（不含需 2 星才开放的 5/6）。</summary>
        private const int GuideLv1MaxPurchasableTableId = 4;
        private const int DefaultGuideRequiredBasicEquipmentCount = 1;
        private const int DefaultGuideRequiredKitchenEquipmentCount = 2;
        private const int DefaultGuideRequiredShopkeeperCount = 0;
        private const int DefaultGuideRequiredChefCount = 1;
        private const int DefaultGuideRequiredWaiterCount = 1;
        private const string GuideBasicCounter = "counter";
        private const string GuideKitchenStove = "stove";
        private const string GuideKitchenFurnace = "furnace";
        /// <summary>Facility.guideKey：柜子。</summary>
        private const string GuideKitchenCabinet = "cabinet_1";
        /// <summary>Facility.guideKey：柜子2。</summary>
        private const string GuideKitchenCabinet2 = "cabinet_2";
        /// <summary>Facility.guideKey：酒柜。</summary>
        private const string GuideKitchenWineCabinet = "cabinet_3";
        /// <summary>Facility.guideKey：水缸堆。</summary>
        private const string GuideKitchenWaterJarPile = "cabinet_4";
        /// <summary>Facility.guideKey：轿子。</summary>
        private const string GuideJiaozi = "jiaozi";
        /// <summary>Facility.guideKey：楼梯。</summary>
        private const string GuideStairs = "stairs";
        /// <summary>Facility.guideKey：二楼桌子。</summary>
        public const string GuideSecondFloorTable = "second_floor_table";
        /// <summary>Facility.guideKey：二楼戏台。</summary>
        public const string GuideXitai = "xitai";
        public const string GuideDecoration1 = "Decoration_1";
        public const string GuideDecoration2 = "Decoration_2";
        public const string GuideDecoration3 = "Decoration_3";
        public const string GuideDecoration4 = "Decoration_4";
        public const string GuideDecoration5 = "Decoration_5";
        public const string GuideDecoration6 = "Decoration_6";
        private const string GuideKitchenTable1 = "kitchen_table_1";
        private const string GuideKitchenTable2 = "kitchen_table_2";
        private static readonly string[] GuideBasicEquipmentOrder =
        {
            GuideBasicCounter,
            GuideKitchenCabinet,
            GuideKitchenCabinet2,
            GuideKitchenWineCabinet,
            GuideKitchenWaterJarPile
            // 轿子/楼梯：按星级开放购买入口，不计入引导顺序。
        };
        private static readonly string[] GuideKitchenEquipmentOrder = { GuideKitchenStove, GuideKitchenFurnace, GuideKitchenTable1, GuideKitchenTable2 };
        private readonly HashSet<string> guideBuildPlacementPendingKeys = new();

        private enum GameplayGuideBuildStep
        {
            BasicEquipment,
            Tables,
            KitchenEquipment
        }

        // 调整建造引导的三个子阶段顺序，只需要改这个数组。
        private static readonly GameplayGuideBuildStep[] GuideBuildStepOrder =
        {
            GameplayGuideBuildStep.BasicEquipment,
            GameplayGuideBuildStep.Tables,
            GameplayGuideBuildStep.KitchenEquipment
        };

        /// <summary>
        /// 获取玩法引导快照。
        /// </summary>
        /// <returns>返回方法执行后的结果。</returns>
        public GameplayGuideSnapshot GetGameplayGuideSnapshot()
        {
            EnsureGameplayDefaults();
            SyncGameplayGuideProgress();

            var guide = SaveData.gameplay.gameplayGuide;
            var snapshot = new GameplayGuideSnapshot
            {
                Stage = guide.currentStage,
                RecruitmentUnlocked = guide.recruitmentUnlocked,
                CanOpenBusiness = guide.openingUnlocked,
                OnboardingCompleted = guide.onboardingCompleted
            };

            if (snapshot.Stage == GameplayGuideStage.Build)
            {
                AddActiveGuideBuildStepTask(snapshot, guide);
            }
            else if (snapshot.Stage == GameplayGuideStage.Recruit)
            {
                AddQuantityTask(
                    snapshot,
                    GameplayGuideTaskId.HireShopkeeper,
                    "招聘掌柜",
                    GetHiredGuideShopkeeperCount(),
                    GetGuideRequiredShopkeeperCount());
                AddQuantityTask(
                    snapshot,
                    GameplayGuideTaskId.HireChef,
                    "招聘厨师",
                    GetHiredGuideChefCount(),
                    GetGuideRequiredChefCount());
                AddQuantityTask(
                    snapshot,
                    GameplayGuideTaskId.HireWaiter,
                    "招聘小二",
                    GetHiredGuideWaiterCount(),
                    GetGuideRequiredWaiterCount());
            }

            return snapshot;
        }

        /// <summary>
        /// 获取当前玩法引导任务。
        /// </summary>
        /// <returns>返回方法执行后的结果。</returns>
        public GameplayGuideTaskProgress GetCurrentGameplayGuideTask()
        {
            var snapshot = GetGameplayGuideSnapshot();
            for (var index = 0; index < snapshot.ActiveTasks.Count; index++)
            {
                var task = snapshot.ActiveTasks[index];
                if (task != null && !task.IsCompleted)
                {
                    return task;
                }
            }

            return snapshot.ActiveTasks.Count > 0 ? snapshot.ActiveTasks[^1] : null;
        }

        /// <summary>
        /// 添加数量型引导任务；目标为 0 时表示无需该条件。
        /// </summary>
        private static void AddQuantityTask(GameplayGuideSnapshot snapshot, GameplayGuideTaskId taskId, string title, int current, int target)
        {
            if (snapshot == null || target <= 0)
            {
                return;
            }

            snapshot.ActiveTasks.Add(new GameplayGuideTaskProgress(taskId, title, Mathf.Clamp(current, 0, target), target));
        }

        /// <summary>
        /// 根据子阶段顺序数组添加当前建造任务。
        /// </summary>
        private static void AddActiveGuideBuildStepTask(GameplayGuideSnapshot snapshot, GameplayGuideSaveData guide)
        {
            if (!TryGetActiveGuideBuildStep(guide, out var buildStep))
            {
                return;
            }

            switch (buildStep)
            {
                case GameplayGuideBuildStep.BasicEquipment:
                    AddQuantityTask(
                        snapshot,
                        GameplayGuideTaskId.BuyCounter,
                        "购买基础设施",
                        GetPurchasedBasicEquipmentCount(guide),
                        GetResolvedGuideBuildBasicEquipmentTargetCount());
                    break;
                case GameplayGuideBuildStep.Tables:
                    var tableTargetCount = GetResolvedGuideBuildTableTargetCount();
                    AddQuantityTask(
                        snapshot,
                        GameplayGuideTaskId.BuyTables,
                        $"购买{tableTargetCount}张桌子",
                        guide.purchasedTableCount,
                        tableTargetCount);
                    break;
                case GameplayGuideBuildStep.KitchenEquipment:
                    AddQuantityTask(
                        snapshot,
                        GameplayGuideTaskId.BuyStove,
                        "购买厨房设施",
                        GetPurchasedKitchenEquipmentCount(guide),
                        GetResolvedGuideBuildKitchenEquipmentTargetCount());
                    break;
            }
        }

        /// <summary>
        /// 判断是否满足酒楼开业条件。
        /// 开业按钮仅在当前主线任务 Id=4（新店开张）时出现。
        /// </summary>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        public bool CanOpenTavernBusiness()
        {
            if (IsVisitingOtherTavern)
            {
                return false;
            }

            // 已首次开业后永久隐藏开业按钮，进入三分钟循环。
            if (GetBusinessOpenCount() > 0)
            {
                return false;
            }

            // 仅当前成就任务为「新店开张」时显示开业入口。
            var currentTask = GetCurrentAchievementTask();
            if (currentTask == null || currentTask.Id != AchievementConfigUtility.MainlineOpenNewShopId)
            {
                return false;
            }

            var snapshot = GetGameplayGuideSnapshot();
            return snapshot.CanOpenBusiness && !SaveData.tavern.isOpen;
        }

        /// <summary>
        /// 判断桌位子阶段是否已轮到执行。
        /// </summary>
        /// <returns>轮到桌位子阶段时返回 true。</returns>
        public bool CanPurchaseGuideTables()
        {
            EnsureGameplayDefaults();
            return CanPurchaseGuideTables(SaveData.gameplay.gameplayGuide);
        }

        /// <summary>
        /// 判断厨房设施子阶段是否已轮到执行。
        /// </summary>
        /// <returns>轮到厨房设施子阶段时返回 true。</returns>
        public bool CanPurchaseGuideKitchenEquipment()
        {
            EnsureGameplayDefaults();
            return CanPurchaseGuideKitchenEquipment(SaveData.gameplay.gameplayGuide);
        }

        /// <summary>
        /// 判断引导桌位购买入口是否还应该显示或响应点击。
        /// LV1：打烊时可半透展示/购买 1~4 号桌；开业只需达到 tbconfig 桌数（默认 2）。
        /// </summary>
        public bool CanPurchaseGuideTable(int tableId)
        {
            EnsureGameplayDefaults();
            if (IsVisitingOtherTavern)
            {
                return false;
            }

            var guide = SaveData.gameplay.gameplayGuide;
            if (tableId <= 0
                || SaveData?.tavern == null
                || !AllowsFacilityPurchaseNow()
                || !CanPurchaseGuideTables(guide))
            {
                return false;
            }

            // 可买范围与开业所需桌数解耦：始终允许 LV1 的 1~4 号桌。
            if (tableId > GuideLv1MaxPurchasableTableId)
            {
                return false;
            }

            var facility = FacilityConfigUtility.GetTableFacility(tableId);
            if (!FacilityConfigUtility.MeetsUnlockLevel(facility, GetTavernLevel()))
            {
                return false;
            }

            var tableData = GetTableData(tableId);
            return tableData != null && !tableData.isUnlocked;
        }

        /// <summary>
        /// 按 Facility 表判断桌位是否可解锁（前置设施 + 累计营收门槛）。
        /// 用于引导目标之外的扩建，以及开业前后的营收节奏解锁。
        /// </summary>
        public bool CanPurchaseConfiguredTable(int tableId)
        {
            EnsureGameplayDefaults();
            if (IsVisitingOtherTavern)
            {
                return false;
            }

            if (tableId <= 0 || SaveData?.tavern == null)
            {
                return false;
            }

            // 首次开业后营业中也可购买未解锁桌子。
            if (!AllowsFacilityPurchaseNow())
            {
                return false;
            }

            // TableArea_5 / TableArea_6：墙体扩建完成后才可建造。
            if ((tableId == 5 || tableId == 6) && !IsInteriorWallExpanded())
            {
                return false;
            }

            var tableData = GetTableData(tableId);
            if (tableData == null || tableData.isUnlocked)
            {
                return false;
            }

            var facility = FacilityConfigUtility.GetTableFacility(tableId);
            if (facility == null)
            {
                return false;
            }

            // 引导桌子阶段仍由 CanPurchaseGuideTable 负责前 N 张。
            var guideTarget = GetResolvedGuideBuildTableTargetCount();
            var inGuideTableStep = !SaveData.tavern.isOpen
                                   && CanPurchaseGuideTables(SaveData.gameplay.gameplayGuide)
                                   && tableId <= guideTarget;
            if (inGuideTableStep)
            {
                return false;
            }

            // 开业前：仅「可开业补建窗口」或已超过引导桌数时走配置解锁。
            if (!SaveData.tavern.isOpen && !CanOpenTavernBusiness() && tableId <= guideTarget)
            {
                return false;
            }

            if (!FacilityConfigUtility.MeetsPrerequisites(facility, IsConfiguredFacilityUnlocked))
            {
                return false;
            }

            if (!FacilityConfigUtility.MeetsUnlockLevel(facility, GetTavernLevel()))
            {
                return false;
            }

            var totalIncome = SaveData.tavern != null ? SaveData.tavern.totalIncome : 0;
            return FacilityConfigUtility.MeetsIncome(facility, totalIncome);
        }

        /// <summary>
        /// 判断 Facility 表中的设施是否已解锁（桌子看存档，其它看引导购买标记）。
        /// </summary>
        public bool IsConfiguredFacilityUnlocked(int facilityId)
        {
            var facility = FacilityConfigUtility.Get(facilityId);
            if (facility == null)
            {
                return false;
            }

            if (facility.FacilityType == FacilityType.Table)
            {
                if (!FacilityConfigUtility.TryResolveTableId(facility, out var resolvedTableId))
                {
                    return false;
                }

                var tableData = GetTableData(resolvedTableId);
                return tableData != null && tableData.isUnlocked;
            }

            if (string.IsNullOrWhiteSpace(facility.GuideKey))
            {
                return false;
            }

            if (facility.FacilityType == FacilityType.Decoration)
            {
                return IsSecondFloorDecorationPurchased(facility.GuideKey);
            }

            if (facility.FacilityType == FacilityType.Stage)
            {
                return IsGuideKitchenItemPurchased(facility.GuideKey);
            }

            return IsGuideBasicEquipmentPurchased(facility.GuideKey)
                   || IsGuideKitchenItemPurchased(facility.GuideKey)
                   || (facility.EquipmentId > 0 && HasOwnedEquipment(facility.EquipmentId));
        }

        /// <summary>二楼装饰是否已购买。</summary>
        public bool IsSecondFloorDecorationPurchased(string guideKey)
        {
            EnsureGameplayDefaults();
            var list = GameplayGuideData?.purchasedSecondFloorDecorations;
            return !string.IsNullOrWhiteSpace(guideKey)
                   && list != null
                   && list.Contains(guideKey);
        }

        private void SetSecondFloorDecorationPurchased(string guideKey, bool value)
        {
            EnsureGameplayDefaults();
            GameplayGuideData.purchasedSecondFloorDecorations ??= new List<string>();
            if (value)
            {
                if (!GameplayGuideData.purchasedSecondFloorDecorations.Contains(guideKey))
                {
                    GameplayGuideData.purchasedSecondFloorDecorations.Add(guideKey);
                }

                return;
            }

            GameplayGuideData.purchasedSecondFloorDecorations.Remove(guideKey);
        }

        private static bool IsSecondFloorDecorationGuideKey(string guideKey)
        {
            return guideKey == GuideDecoration1
                   || guideKey == GuideDecoration2
                   || guideKey == GuideDecoration3
                   || guideKey == GuideDecoration4
                   || guideKey == GuideDecoration5
                   || guideKey == GuideDecoration6;
        }

        /// <summary>
        /// 读取桌子解锁花费（Facility.cost 优先）。
        /// </summary>
        public int GetTableUnlockCost(int tableId, int fallback = 900)
        {
            return FacilityConfigUtility.GetTableUnlockCost(tableId, fallback);
        }

        /// <summary>
        /// 判断是否完成招聘前置设备，完成后才开放招聘。
        /// </summary>
        /// <returns>完成厨房设备购买时返回 true。</returns>
        public bool CanRecruitGuideStaff()
        {
            EnsureGameplayDefaults();
            return CanRecruitGuideStaff(SaveData.gameplay.gameplayGuide);
        }

        /// <summary>
        /// 员工入口红点：初次招聘引导阶段且尚未完成全部招聘任务时显示。
        /// </summary>
        public bool ShouldShowStaffRecruitGuideRedDot()
        {
            EnsureGameplayDefaults();
            var guide = SaveData.gameplay.gameplayGuide;
            if (guide == null || !guide.recruitmentUnlocked)
            {
                return false;
            }

            if (SaveData.tavern.isOpen)
            {
                return false;
            }

            return guide.currentStage == GameplayGuideStage.Recruit;
        }

        /// <summary>
        /// 底部员工按钮红点：当前还能招聘厨师/小二/掌柜时显示。
        /// </summary>
        public bool ShouldShowStaffHireAvailableRedDot()
        {
            if (IsVisitingOtherTavern || !IsStaffRecruitUiUnlockedByAchievement())
            {
                return false;
            }

            return CanHireMoreGuideChef() || CanHireMoreGuideWaiter() || CanHireMoreGuideShopkeeper();
        }

        /// <summary>
        /// 标记引导建造物是否仍在配送或落位中，落位完成前不进入招聘阶段。
        /// </summary>
        public void MarkGuideBuildPlacementPending(string itemKey, bool pending)
        {
            if (string.IsNullOrWhiteSpace(itemKey))
            {
                return;
            }

            EnsureGameplayDefaults();
            var changed = pending
                ? guideBuildPlacementPendingKeys.Add(itemKey)
                : guideBuildPlacementPendingKeys.Remove(itemKey);
            if (!changed)
            {
                return;
            }

            SyncGameplayGuideProgress();
            Signals.Get<GameplayGuideProgressSignal>().Dispatch();
            SaveGame();
        }

        /// <summary>
        /// 判断是否仍有引导建造物未真正落位。
        /// </summary>
        public bool HasGuideBuildPlacementPending()
        {
            return guideBuildPlacementPendingKeys.Count > 0;
        }

        /// <summary>
        /// 清理所有引导建造落位等待标记。
        /// </summary>
        public void ClearGuideBuildPlacementPending()
        {
            if (guideBuildPlacementPendingKeys.Count <= 0)
            {
                return;
            }

            guideBuildPlacementPendingKeys.Clear();
            EnsureGameplayDefaults();
            SyncGameplayGuideProgress();
            Signals.Get<GameplayGuideProgressSignal>().Dispatch();
            SaveGame();
        }

        /// <summary>
        /// 清理失效的引导建造 pending 标记，避免脏数据阻塞阶段推进。
        /// 仅移除“未真正购买或解锁”的 key，合法配送中的对象会被保留。
        /// </summary>
        private void SanitizeGuideBuildPlacementPendingKeys()
        {
            if (guideBuildPlacementPendingKeys.Count <= 0)
            {
                return;
            }

            var guide = SaveData?.gameplay?.gameplayGuide;
            var staleKeys = new List<string>();
            foreach (var pendingKey in guideBuildPlacementPendingKeys)
            {
                if (string.IsNullOrWhiteSpace(pendingKey))
                {
                    staleKeys.Add(pendingKey);
                    continue;
                }

                if (pendingKey.StartsWith("table_"))
                {
                    if (!TryParseGuidePendingTableId(pendingKey, out var tableId))
                    {
                        staleKeys.Add(pendingKey);
                        continue;
                    }

                    var tableData = GetTableData(tableId);
                    if (tableData == null || !tableData.isUnlocked)
                    {
                        staleKeys.Add(pendingKey);
                    }

                    continue;
                }

                if (pendingKey == GuideBasicCounter)
                {
                    if (!IsGuideBasicEquipmentPurchasedFromGuide(guide, GuideBasicCounter))
                    {
                        staleKeys.Add(pendingKey);
                    }

                    continue;
                }

                if (pendingKey == GuideKitchenStove)
                {
                    if (!HasOwnedEquipment(StoveEquipmentId))
                    {
                        staleKeys.Add(pendingKey);
                    }

                    continue;
                }

                if (IsGuideBasicEquipmentKey(pendingKey) && !IsGuideBasicEquipmentPurchasedFromGuide(guide, pendingKey))
                {
                    staleKeys.Add(pendingKey);
                    continue;
                }

                if (IsGuideKitchenEquipmentKey(pendingKey) && !IsGuideKitchenItemPurchasedFromGuide(guide, pendingKey))
                {
                    staleKeys.Add(pendingKey);
                }
            }

            if (staleKeys.Count <= 0)
            {
                return;
            }

            for (var index = 0; index < staleKeys.Count; index++)
            {
                guideBuildPlacementPendingKeys.Remove(staleKeys[index]);
            }

            Debug.LogWarning($"[GameplayGuide] Cleared stale build pending keys: {string.Join(",", staleKeys)}");
        }

        /// <summary>
        /// 从 pending key 中解析桌位编号。
        /// </summary>
        private static bool TryParseGuidePendingTableId(string pendingKey, out int tableId)
        {
            tableId = 0;
            if (string.IsNullOrWhiteSpace(pendingKey) || !pendingKey.StartsWith("table_"))
            {
                return false;
            }

            return int.TryParse(pendingKey.Substring("table_".Length), out tableId) && tableId > 0;
        }

        /// <summary>
        /// 直接基于已传入引导存档判断基础设施是否已购买，避免触发额外同步。
        /// </summary>
        private bool IsGuideBasicEquipmentPurchasedFromGuide(GameplayGuideSaveData guide, string itemKey)
        {
            if (guide == null)
            {
                return false;
            }

            return itemKey switch
            {
                GuideBasicCounter => guide.purchasedCounter,
                GuideKitchenCabinet => guide.purchasedCabinet,
                GuideKitchenCabinet2 => guide.purchasedCabinet2,
                GuideKitchenWineCabinet => guide.purchasedWineCabinet,
                GuideKitchenWaterJarPile => guide.purchasedWaterJarPile,
                GuideJiaozi => guide.purchasedJiaozi,
                GuideStairs => guide.purchasedStairs,
                _ => false
            };
        }

        /// <summary>
        /// 直接基于已传入引导存档判断厨房设施是否已购买，避免触发额外同步。
        /// </summary>
        private bool IsGuideKitchenItemPurchasedFromGuide(GameplayGuideSaveData guide, string itemKey)
        {
            if (guide == null)
            {
                return false;
            }

            return itemKey switch
            {
                GuideKitchenStove => guide.purchasedStove,
                GuideKitchenFurnace => guide.purchasedFurnace,
                GuideKitchenCabinet => guide.purchasedCabinet,
                GuideKitchenCabinet2 => guide.purchasedCabinet2,
                GuideKitchenWineCabinet => guide.purchasedWineCabinet,
                GuideKitchenWaterJarPile => guide.purchasedWaterJarPile,
                GuideJiaozi => guide.purchasedJiaozi,
                GuideStairs => guide.purchasedStairs,
                GuideSecondFloorTable => IsSecondFloorKitchenTableUnlocked(),
                GuideXitai => guide.purchasedXitai,
                GuideKitchenTable1 => guide.purchasedKitchenTable1,
                GuideKitchenTable2 => guide.purchasedKitchenTable2,
                GuideDecoration1 => guide.purchasedSecondFloorDecorations != null
                                    && guide.purchasedSecondFloorDecorations.Contains(GuideDecoration1),
                GuideDecoration2 => guide.purchasedSecondFloorDecorations != null
                                    && guide.purchasedSecondFloorDecorations.Contains(GuideDecoration2),
                GuideDecoration3 => guide.purchasedSecondFloorDecorations != null
                                    && guide.purchasedSecondFloorDecorations.Contains(GuideDecoration3),
                GuideDecoration4 => guide.purchasedSecondFloorDecorations != null
                                    && guide.purchasedSecondFloorDecorations.Contains(GuideDecoration4),
                GuideDecoration5 => guide.purchasedSecondFloorDecorations != null
                                    && guide.purchasedSecondFloorDecorations.Contains(GuideDecoration5),
                GuideDecoration6 => guide.purchasedSecondFloorDecorations != null
                                    && guide.purchasedSecondFloorDecorations.Contains(GuideDecoration6),
                _ => false
            };
        }

        /// <summary>
        /// 判断基础设施购买入口是否应显示（营业中或未开业引导期均可购买）。
        /// LV1：不再按顺序，全部未购设施同时显示半透与价格 UI。
        /// 轿子改由 HireStaff_enter 结束后自动解锁，不再显示购买入口。
        /// </summary>
        public bool ShouldShowGuideBasicEquipmentPurchase(string itemKey)
        {
            EnsureGameplayDefaults();
            if (itemKey == GuideJiaozi)
            {
                return false;
            }

            return SaveData?.tavern != null
                   && AllowsFacilityPurchaseNow()
                   && IsGuideBasicEquipmentKey(itemKey)
                   && !IsGuideBasicEquipmentPurchased(itemKey)
                   && MeetsGuideFacilityUnlockLevel(itemKey);
        }

        /// <summary>
        /// 按进度自动解锁轿子：HireStaff_enter 已播过，或酒楼 ≥3 星。
        /// 返回是否新解锁。禁止在此调用 EnsureGameplayDefaults。
        /// </summary>
        public bool TryGrantJiaoziUnlockedByProgress(bool dispatchSignals = true)
        {
            var guide = SaveData?.gameplay?.gameplayGuide;
            if (guide == null || guide.purchasedJiaozi)
            {
                return false;
            }

            var shouldGrant = guide.dialogHireStaffEnterShown || GetTavernLevel() >= 3;
            if (!shouldGrant)
            {
                return false;
            }

            guide.purchasedJiaozi = true;
            SaveGame();
            if (dispatchSignals)
            {
                Signals.Get<GameplayGuideProgressSignal>().Dispatch();
                Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            }

            return true;
        }

        /// <summary>
        /// 轿子是否已揭幕（已购买）。购买入口已取消，仅用于兼容旧调用。
        /// </summary>
        public bool IsJiaoziFacilityRevealed()
        {
            return IsJiaoziUnlocked();
        }

        /// <summary>
        /// 判断厨房设施购买入口是否应显示（营业中或未开业引导期均可购买）。
        /// LV1：不再按顺序，全部未购厨房设施同时显示。
        /// </summary>
        public bool ShouldShowGuideKitchenEquipmentPurchase(string itemKey)
        {
            EnsureGameplayDefaults();
            return SaveData?.tavern != null
                   && AllowsFacilityPurchaseNow()
                   && IsGuideKitchenEquipmentKey(itemKey)
                   && !IsGuideKitchenItemPurchased(itemKey)
                   && MeetsGuideFacilityUnlockLevel(itemKey);
        }

        /// <summary>
        /// 引导设施是否满足 Facility.unlockLevel（厨房桌跳过等级门闩）。
        /// </summary>
        private bool MeetsGuideFacilityUnlockLevel(string itemKey)
        {
            var facility = FacilityConfigUtility.GetByGuideKey(itemKey);
            return FacilityConfigUtility.MeetsUnlockLevel(facility, GetTavernLevel());
        }

        /// <summary>
        /// 建造成功后按 Facility.getPresitige 加声望（tips 由 AddTavernPrestige 统一弹出）。
        /// </summary>
        private void GrantFacilityBuildPrestige(Facility facility)
        {
            GameAudioManager.PlayFacilityPurchaseSuccess();
            var prestige = FacilityConfigUtility.GetBuildPrestige(facility);
            if (prestige > 0)
            {
                AddTavernPrestige(prestige);
            }

            HudOverlayService.SetPendingPrestigeFlySource(null);
        }

        /// <summary>
        /// 判断是否处于“引导建造和招聘完成，但尚未开业”的补建窗口。
        /// </summary>
        private bool IsGuideReadyToOpenBuildWindow(GameplayGuideSaveData guide)
        {
            return SaveData?.tavern != null
                   && !SaveData.tavern.isOpen
                   && guide != null
                   && guide.openingUnlocked;
        }

        /// <summary>
        /// 判断指定基础设施当前是否允许购买。
        /// </summary>
        private bool CanPurchaseGuideBasicEquipmentItem(string itemKey, bool logFailure = false)
        {
            EnsureGameplayDefaults();
            SyncGameplayGuideProgress();
            var guide = SaveData.gameplay.gameplayGuide;
            var hasTavern = SaveData?.tavern != null;
            var allowsPurchase = AllowsFacilityPurchaseNow();
            var isBasicEquipmentKey = IsGuideBasicEquipmentKey(itemKey);
            var isPurchased = IsGuideBasicEquipmentPurchased(itemKey);
            var meetsLevel = MeetsGuideFacilityUnlockLevel(itemKey);
            // 轿子改对话后自动解锁，不可再购买。
            var canPurchase = hasTavern
                              && allowsPurchase
                              && isBasicEquipmentKey
                              && !isPurchased
                              && meetsLevel
                              && itemKey != GuideJiaozi;

            if (!canPurchase && logFailure)
            {
                Debug.LogWarning(
                    $"[GameplayGuide] Block basic equipment purchase. itemKey={itemKey}, " +
                    $"hasTavern={hasTavern}, allowsPurchase={allowsPurchase}, isBasicEquipmentKey={isBasicEquipmentKey}, " +
                    $"isPurchased={isPurchased}, meetsLevel={meetsLevel}, " +
                    $"openingUnlocked={(guide != null && guide.openingUnlocked)}, currentStage={(guide != null ? guide.currentStage.ToString() : "null")}, " +
                    $"{BuildStageDebugSummary(guide)}");
            }

            return canPurchase;
        }

        /// <summary>
        /// 判断指定厨房设施当前是否允许购买。
        /// </summary>
        private bool CanPurchaseGuideKitchenEquipmentItem(string itemKey, bool logFailure = false)
        {
            EnsureGameplayDefaults();
            SyncGameplayGuideProgress();
            var guide = SaveData.gameplay.gameplayGuide;
            var hasTavern = SaveData?.tavern != null;
            var allowsPurchase = AllowsFacilityPurchaseNow();
            var isKitchenEquipmentKey = IsGuideKitchenEquipmentKey(itemKey);
            var isPurchased = IsGuideKitchenItemPurchased(itemKey);
            var meetsLevel = MeetsGuideFacilityUnlockLevel(itemKey);
            var canPurchase = hasTavern
                              && allowsPurchase
                              && isKitchenEquipmentKey
                              && !isPurchased
                              && meetsLevel;

            if (!canPurchase && logFailure)
            {
                Debug.LogWarning(
                    $"[GameplayGuide] Block kitchen equipment purchase. itemKey={itemKey}, " +
                    $"hasTavern={hasTavern}, allowsPurchase={allowsPurchase}, isKitchenEquipmentKey={isKitchenEquipmentKey}, " +
                    $"isPurchased={isPurchased}, meetsLevel={meetsLevel}, " +
                    $"openingUnlocked={(guide != null && guide.openingUnlocked)}, currentStage={(guide != null ? guide.currentStage.ToString() : "null")}, " +
                    $"{BuildStageDebugSummary(guide)}");
            }

            return canPurchase;
        }

        /// <summary>
        /// 读取配置中的基础设施开业数量要求。
        /// </summary>
        public int GetRequiredGuideBasicEquipmentCount()
        {
            return GetGuideRequiredBasicEquipmentCount();
        }

        /// <summary>
        /// 读取当前已购买的基础设施数量。
        /// </summary>
        public int GetPurchasedGuideBasicEquipmentCount()
        {
            EnsureGameplayDefaults();
            return GetPurchasedBasicEquipmentCount(SaveData.gameplay.gameplayGuide);
        }

        /// <summary>
        /// 读取配置中的厨房设施开业数量要求。
        /// </summary>
        public int GetRequiredGuideKitchenEquipmentCount()
        {
            return GetGuideRequiredKitchenEquipmentCount();
        }

        /// <summary>
        /// 读取当前已购买的厨房设施数量。
        /// </summary>
        public int GetPurchasedGuideKitchenEquipmentCount()
        {
            EnsureGameplayDefaults();
            return GetPurchasedKitchenEquipmentCount(SaveData.gameplay.gameplayGuide);
        }

        /// <summary>
        /// 读取配置中的掌柜开业数量要求。
        /// </summary>
        public int GetRequiredGuideShopkeeperCount()
        {
            return GetGuideRequiredShopkeeperCount();
        }

        /// <summary>
        /// 读取配置中的厨师开业数量要求。
        /// </summary>
        public int GetRequiredGuideChefCount()
        {
            return GetGuideRequiredChefCount();
        }

        /// <summary>
        /// 读取配置中的小二开业数量要求。
        /// </summary>
        public int GetRequiredGuideWaiterCount()
        {
            return GetGuideRequiredWaiterCount();
        }

        /// <summary>
        /// 读取建造阶段基础设施目标数量。
        /// </summary>
        public int GetGuideBuildBasicEquipmentTargetCount()
        {
            return GetResolvedGuideBuildBasicEquipmentTargetCount();
        }

        /// <summary>
        /// 读取建造阶段桌位目标数量。
        /// </summary>
        public int GetGuideBuildTableTargetCount()
        {
            return GetResolvedGuideBuildTableTargetCount();
        }

        /// <summary>
        /// 读取建造阶段厨房设施目标数量。
        /// </summary>
        public int GetGuideBuildKitchenEquipmentTargetCount()
        {
            return GetResolvedGuideBuildKitchenEquipmentTargetCount();
        }

        /// <summary>
        /// 使用已传入的引导数据判断是否可买引导桌。
        /// LV1：不再卡在「桌位子阶段」，打烊引导期即可买。
        /// </summary>
        private static bool CanPurchaseGuideTables(GameplayGuideSaveData guide)
        {
            return guide != null;
        }

        /// <summary>
        /// 使用已传入的引导数据判断是否可买厨房设施。
        /// LV1：不再卡在厨房子阶段。
        /// </summary>
        private static bool CanPurchaseGuideKitchenEquipment(GameplayGuideSaveData guide)
        {
            return guide != null;
        }

        /// <summary>
        /// 使用已传入的引导数据判断是否开放招聘。
        /// 不在此处读成就任务：GetCurrentAchievementTask 会 EnsureGameplayDefaults，与 Sync 形成递归。
        /// 任务 Id 门闩仅用于场景/开业雇佣 UI（见 IsStaffRecruitUiUnlockedByAchievement）。
        /// </summary>
        private bool CanRecruitGuideStaff(GameplayGuideSaveData guide)
        {
            return guide != null
                   && SaveData?.tavern != null
                   && !IsVisitingOtherTavern;
        }

        /// <summary>
        /// 场景雇佣 UI / 招聘入口：当前成就任务 Id≥2（或已全部完成）才显示。
        /// 禁止调用 GetCurrentAchievementTask / EnsureGameplayDefaults，避免与 SyncGameplayGuideProgress 栈溢出。
        /// </summary>
        public bool IsStaffRecruitUiUnlockedByAchievement()
        {
            var all = AchievementConfigUtility.GetAllSorted();
            for (var index = 0; index < all.Count; index++)
            {
                var achievement = all[index];
                if (achievement == null)
                {
                    continue;
                }

                if (!IsAchievementConditionMetWithoutEnsure(achievement))
                {
                    return achievement.Id >= 2;
                }
            }

            return true;
        }

        /// <summary>
        /// 成就条件是否达成（不触发 Ensure*，供雇佣 UI 门闩使用）。
        /// </summary>
        private bool IsAchievementConditionMetWithoutEnsure(Achievement achievement)
        {
            if (achievement == null)
            {
                return false;
            }

            var target = AchievementConfigUtility.GetTarget(achievement);
            return GetAchievementProgressWithoutEnsure(achievement) >= target;
        }

        /// <summary>
        /// 轻量成就进度：仅覆盖开业任务链（BuyFacility / EmployFellow），其余视为 0。
        /// </summary>
        private int GetAchievementProgressWithoutEnsure(Achievement achievement)
        {
            if (achievement == null)
            {
                return 0;
            }

            return achievement.AchievementType switch
            {
                AchievementType.BuyFacility => GetBuyFacilityTaskProgressWithoutEnsure(),
                AchievementType.EmployFellow => GetEmployFellowTaskProgressWithoutEnsure(),
                AchievementType.UpgradeLevel => SaveData?.tavern != null
                    ? Mathf.Clamp(SaveData.tavern.tavernLevel, 0, MaxTavernLevel)
                    : 0,
                AchievementType.Expand => SaveData?.tavern != null && SaveData.tavern.interiorWallExpanded ? 1 : 0,
                AchievementType.Stairs => SaveData?.gameplay?.gameplayGuide != null
                    && SaveData.gameplay.gameplayGuide.purchasedStairs
                    ? 1
                    : 0,
                _ => 0
            };
        }

        private int GetBuyFacilityTaskProgressWithoutEnsure()
        {
            var guide = SaveData?.gameplay?.gameplayGuide;
            var counter = guide != null && guide.purchasedCounter ? 1 : 0;
            var stove = guide != null && (guide.purchasedStove || HasOwnedEquipment(StoveEquipmentId)) ? 1 : 0;
            var furnace = guide != null && guide.purchasedFurnace ? 1 : 0;
            var table = CountUnlockedTablesWithoutEnsure() > 0 ? 1 : 0;
            return counter + stove + furnace + table;
        }

        private int GetEmployFellowTaskProgressWithoutEnsure()
        {
            var shopkeeper = CountHiredByPosition(StaffPosition.Shopkeeper) > 0
                             || HasHiredGuideStaff(ShopkeeperStaffId, StaffRole.Waiter)
                ? 1
                : 0;
            var chef = CountHiredByPosition(StaffPosition.Chef) > 0 ? 1 : 0;
            var waiter = CountHiredByPosition(StaffPosition.Waiter) > 0 ? 1 : 0;
            return shopkeeper + chef + waiter;
        }

        private int CountUnlockedTablesWithoutEnsure()
        {
            var tables = SaveData?.tavern?.tables;
            if (tables == null)
            {
                return 0;
            }

            var count = 0;
            for (var index = 0; index < tables.Count; index++)
            {
                var table = tables[index];
                if (table != null && table.isUnlocked)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 汇总当前引导建造阶段未能结束的关键信息，便于排查卡在 Build 的原因。
        /// </summary>
        private string BuildStageDebugSummary(GameplayGuideSaveData guide)
        {
            var basicCurrent = GetPurchasedBasicEquipmentCount(guide);
            var basicTarget = GetResolvedGuideBuildBasicEquipmentTargetCount();
            var tableCurrent = guide != null ? guide.purchasedTableCount : 0;
            var tableTarget = GetResolvedGuideBuildTableTargetCount();
            var kitchenCurrent = GetPurchasedKitchenEquipmentCount(guide);
            var kitchenTarget = GetResolvedGuideBuildKitchenEquipmentTargetCount();
            var basicDone = IsGuideBuildStepCompleted(guide, GameplayGuideBuildStep.BasicEquipment);
            var tableDone = IsGuideBuildStepCompleted(guide, GameplayGuideBuildStep.Tables);
            var kitchenDone = IsGuideBuildStepCompleted(guide, GameplayGuideBuildStep.KitchenEquipment);
            var allBuildDone = AreGuideBuildStepsCompleted(guide);
            var hasPending = HasGuideBuildPlacementPending();
            var canRecruit = CanRecruitGuideStaff(guide);
            var pendingKeys = guideBuildPlacementPendingKeys.Count > 0
                ? string.Join(",", guideBuildPlacementPendingKeys)
                : "none";

            return $"buildSummary={{basic:{basicCurrent}/{basicTarget}, table:{tableCurrent}/{tableTarget}, kitchen:{kitchenCurrent}/{kitchenTarget}, " +
                   $"basicDone={basicDone}, tableDone={tableDone}, kitchenDone={kitchenDone}, allBuildDone={allBuildDone}, " +
                   $"hasPending={hasPending}, pendingKeys=[{pendingKeys}], canRecruit={canRecruit}, recruitmentUnlocked={(guide != null && guide.recruitmentUnlocked)}}}";
        }

        /// <summary>
        /// 尝试处理购买引导柜台。
        /// </summary>
        /// <param name="message">参数值。</param>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        public bool TryPurchaseGuideCounter(out string message)
        {
            EnsureGameplayDefaults();
            if (IsVisitingOtherTavern)
            {
                message = "访客模式下不可新增设施";
                return false;
            }

            if (IsGuideBasicEquipmentPurchased(GuideBasicCounter))
            {
                message = "掌柜桌已购买";
                return false;
            }

            if (!CanPurchaseGuideBasicEquipmentItem(GuideBasicCounter, true))
            {
                if (!MeetsGuideFacilityUnlockLevel(GuideBasicCounter))
                {
                    var needLevel = FacilityConfigUtility.GetByGuideKey(GuideBasicCounter)?.UnlockLevel ?? 0;
                    message = $"酒楼等级不足，购买掌柜桌需要达到{needLevel}级";
                    return false;
                }

                message = "当前引导阶段不可购买掌柜桌";
                return false;
            }

            var facility = FacilityConfigUtility.GetByGuideKey(GuideBasicCounter);
            var cost = FacilityConfigUtility.GetUnlockCost(facility, 0);
            if (PlayerData.coinNum < cost)
            {
                message = $"铜钱不足，购买掌柜桌需要{cost}";
                return false;
            }

            ChangeCoinNum(-cost);
            GameplayGuideData.purchasedCounter = true;
            // Facility.equipmentId 为 0 时不写 ownedEquipment，避免 id=0 脏数据。
            if (CounterEquipmentId > 0 && !HasOwnedEquipment(CounterEquipmentId))
            {
                SaveData.gameplay.ownedEquipment ??= new List<LocalEquipmentSaveData>();
                SaveData.gameplay.ownedEquipment.Add(new LocalEquipmentSaveData
                {
                    equipmentId = (byte)CounterEquipmentId,
                    currentLevel = 1,
                    physicalSlotIndex = (byte)SaveData.gameplay.ownedEquipment.Count
                });
            }

            GrantFacilityBuildPrestige(facility);
            SyncGameplayGuideProgress();
            NotifyAchievementStatsChanged();
            Signals.Get<GameplayGuideProgressSignal>().Dispatch();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            SaveGame();
            message = "已购买掌柜桌";
            return true;
        }

        /// <summary>
        /// 尝试处理购买引导灶台。
        /// </summary>
        /// <param name="message">参数值。</param>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        public bool TryPurchaseGuideStove(out string message)
        {
            return TryPurchaseGuideEquipment(StoveEquipmentId, "灶台", out message);
        }

        /// <summary>
        /// 尝试处理购买引导厨房物件。
        /// </summary>
        /// <param name="itemKey">语言表键值。</param>
        /// <param name="message">参数值。</param>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        public bool TryPurchaseGuideKitchenItem(string itemKey, out string message)
        {
            EnsureGameplayDefaults();
            if (string.IsNullOrEmpty(itemKey))
            {
                message = "未找到可购买物件";
                return false;
            }

            if (IsGuideKitchenItemPurchased(itemKey))
            {
                message = $"{GetGuideKitchenDisplayName(itemKey)}已购买";
                return false;
            }

            if (IsGuideBasicEquipmentKey(itemKey))
            {
                if (!CanPurchaseGuideBasicEquipmentItem(itemKey, true))
                {
                    if (!MeetsGuideFacilityUnlockLevel(itemKey))
                    {
                        var needLevel = FacilityConfigUtility.GetByGuideKey(itemKey)?.UnlockLevel ?? 0;
                        message = $"酒楼等级不足，需要达到{needLevel}级";
                        return false;
                    }

                    message = "当前引导阶段不可购买该物件";
                    return false;
                }
            }
            else if (IsGuideKitchenEquipmentKey(itemKey))
            {
                if (!CanPurchaseGuideKitchenEquipmentItem(itemKey, true))
                {
                    if (!MeetsGuideFacilityUnlockLevel(itemKey))
                    {
                        var needLevel = FacilityConfigUtility.GetByGuideKey(itemKey)?.UnlockLevel ?? 0;
                        message = $"酒楼等级不足，需要达到{needLevel}级";
                        return false;
                    }

                    message = "当前引导阶段不可购买该物件";
                    return false;
                }
            }
            else
            {
                message = "未找到可购买物件";
                return false;
            }

            var facility = FacilityConfigUtility.GetByGuideKey(itemKey);
            var cost = facility != null
                ? FacilityConfigUtility.GetUnlockCost(
                    facility,
                    GetGuideEquipmentPurchaseCost(facility.EquipmentId > 0 ? facility.EquipmentId : StoveEquipmentId))
                : GetGuideEquipmentPurchaseCost(StoveEquipmentId);
            if (PlayerData.coinNum < cost)
            {
                message = $"铜钱不足，购买{GetGuideKitchenDisplayName(itemKey)}需要{cost}";
                return false;
            }

            ChangeCoinNum(-cost);
            SetGuideKitchenItemPurchased(itemKey, true);
            GrantFacilityBuildPrestige(facility);
            SyncGameplayGuideProgress();
            NotifyAchievementStatsChanged();
            Signals.Get<GameplayGuideProgressSignal>().Dispatch();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            SaveGame();
            message = $"已购买{GetGuideKitchenDisplayName(itemKey)}";
            return true;
        }

        /// <summary>
        /// 尝试处理招聘引导掌柜。
        /// </summary>
        /// <param name="message">参数值。</param>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        public bool TryHireGuideShopkeeper(out string message)
        {
            var staffId = ShopkeeperStaffId;
            var displayName = StaffConfigUtility.GetName(staffId, "掌柜");
            return TryHireGuideStaff(staffId, StaffRole.Waiter, displayName, out message, GetMaxShopkeeperHireCount(), true);
        }

        /// <summary>
        /// 尝试处理招聘引导厨师。
        /// </summary>
        /// <param name="message">参数值。</param>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        public bool TryHireGuideChef(out string message)
        {
            var staffId = ChefStaffId;
            var displayName = StaffConfigUtility.GetName(staffId, "厨师");
            if (GetGuideUnlockedChefHireCount() <= 0)
            {
                message = "请先建造灶台后再雇佣厨师";
                return false;
            }

            return TryHireGuideStaff(staffId, StaffRole.Chef, displayName, out message, GetGuideUnlockedChefHireCount(), true);
        }

        /// <summary>
        /// 尝试招聘引导阶段的小二。
        /// </summary>
        /// <param name="message">返回招聘失败或成功的提示文案。</param>
        /// <returns>招聘成功时返回 true，否则返回 false。</returns>
        public bool TryHireGuideWaiter(out string message)
        {
            var staffId = WaiterStaffId;
            var displayName = StaffConfigUtility.GetName(staffId, "小二");
            if (GetGuideUnlockedWaiterHireCount() <= 0)
            {
                message = "请先建造桌子后再雇佣小二";
                return false;
            }

            return TryHireGuideStaff(staffId, StaffRole.Waiter, displayName, out message, GetGuideUnlockedWaiterHireCount(), true);
        }

        /// <summary>
        /// 尝试在本次营业中招聘一名临时小二。
        /// </summary>
        /// <param name="message">返回招聘失败或成功的提示文案。</param>
        /// <returns>招聘成功时返回 true，否则返回 false。</returns>
        public bool TryHireTemporaryGuideWaiter(out string message)
        {
            EnsureGameplayDefaults();
            var hiredCount = CountActiveFloorStaffByPosition(StaffPosition.Waiter);
            if (hiredCount >= Mathf.Max(1, GetMaxWaiterHireCount()))
            {
                message = "小二已达到招聘上限";
                return false;
            }

            var staff = FindGuideStaff(WaiterStaffId, StaffRole.Waiter);
            if (staff == null)
            {
                message = "未找到小二配置";
                return false;
            }

            var cost = GetGuideStaffHireCost(WaiterStaffId, StaffRole.Waiter);
            if (PlayerData.coinNum < cost)
            {
                message = $"铜钱不足，招聘临时小二需要 {cost}";
                return false;
            }

            ChangeCoinNum(-cost);
            SaveData.gameplay.ownedStaff.Add(StaffConfigUtility.CreateOwnedStaffSave(WaiterStaffId, temporary: true));

            SyncGameplayGuideProgress();
            Signals.Get<GameplayGuideProgressSignal>().Dispatch();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            SaveGame();
            message = "已招聘临时小二";
            return true;
        }

        /// <summary>
        /// 移除本次营业招聘的临时小二。
        /// </summary>
        /// <returns>移除了任意临时小二时返回 true，否则返回 false。</returns>
        public bool RemoveTemporaryGuideWaiters()
        {
            EnsureGameplayDefaults();
            var ownedStaff = SaveData.gameplay.ownedStaff;
            if (ownedStaff == null)
            {
                return false;
            }

            var removedCount = ownedStaff.RemoveAll(staff => staff != null
                                                            && staff.temporary
                                                            && staff.staffId == WaiterStaffId);
            if (removedCount <= 0)
            {
                return false;
            }

            SyncGameplayGuideProgress();
            Signals.Get<GameplayGuideProgressSignal>().Dispatch();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            SaveGame();
            return true;
        }

        /// <summary>
        /// 获取当前已招聘的引导掌柜数量。
        /// </summary>
        /// <returns>已招聘掌柜数量。</returns>
        public int GetHiredGuideShopkeeperCount()
        {
            return CountHiredByPosition(StaffPosition.Shopkeeper);
        }

        /// <summary>
        /// 判断是否还能招聘掌柜（开业引导仅允许 1 名）。
        /// </summary>
        public bool CanHireMoreGuideShopkeeper()
        {
            return GetHiredGuideShopkeeperCount() < 1;
        }

        /// <summary>
        /// 获取当前已招聘厨师数量（按职位，含新招聘池；不含临时工）。
        /// </summary>
        public int GetHiredGuideChefCount()
        {
            return CountHiredByPosition(StaffPosition.Chef);
        }

        /// <summary>
        /// 获取当前场上小二数量（按职位，含新招聘池与临时工）。
        /// </summary>
        public int GetHiredGuideWaiterCount()
        {
            return CountActiveFloorStaffByPosition(StaffPosition.Waiter);
        }

        /// <summary>
        /// 酒楼星级决定的伙计岗位名额。
        /// 存档 0/1 星（展示 lv1）仅开业首名；2 星可再招 1 名；3 星再多 1 名。
        /// </summary>
        public int GetTavernLevelStaffHireSlotCap()
        {
            var level = GetTavernLevel();
            // 0/1 → 1；2 → 2；3 → 3。
            return level <= 1 ? 1 : Mathf.Min(level, Mathf.Max(MaxGuideChefCount, MaxGuideWaiterCount));
        }

        /// <summary>
        /// 可雇佣厨师上限：灶台造好后才解锁；名额受酒楼星级限制（lv1 不可再招新厨）。
        /// </summary>
        public int GetGuideUnlockedChefHireCount()
        {
            EnsureGameplayDefaults();
            var hardCap = Mathf.Min(
                MaxGuideChefCount,
                GetMaxChefHireCount(),
                GetTavernLevelStaffHireSlotCap());
            if (SaveData?.tavern != null && SaveData.tavern.isOpen)
            {
                return hardCap;
            }

            if (!IsGuideKitchenItemPurchased(GuideKitchenStove))
            {
                return 0;
            }

            return hardCap;
        }

        /// <summary>
        /// 可雇佣小二上限：至少 1 张桌才出槽；名额受酒楼星级限制（lv1 不可再招新小二）。
        /// </summary>
        public int GetGuideUnlockedWaiterHireCount()
        {
            EnsureGameplayDefaults();
            var tableCount = GetUnlockedTableCount();
            if (tableCount <= 0)
            {
                return 0;
            }

            // 星级名额为主：lv1=1、lv2=2…；不再用桌数额外抬高上限。
            return Mathf.Min(
                MaxGuideWaiterCount,
                GetMaxWaiterHireCount(),
                GetTavernLevelStaffHireSlotCap());
        }

        /// <summary>
        /// 判断是否还能继续招聘引导厨师。
        /// </summary>
        /// <returns>未达到厨师上限时返回 true。</returns>
        public bool CanHireMoreGuideChef()
        {
            return GetHiredGuideChefCount() < GetGuideUnlockedChefHireCount()
                   && GetHireCandidatesForRole(StaffPosition.Chef, 1).Count > 0;
        }

        /// <summary>
        /// 当前星级下厨师与小二名额是否都已招满（点员工入口时 tips「请先升级酒楼」）。
        /// </summary>
        public bool IsStaffHireSlotCapReached()
        {
            var slotCap = GetTavernLevelStaffHireSlotCap();
            return GetHiredGuideChefCount() >= slotCap
                   && GetHiredGuideWaiterCount() >= slotCap;
        }

        /// <summary>
        /// 判断是否还能继续招聘引导小二。
        /// </summary>
        /// <returns>未达到小二上限时返回 true。</returns>
        public bool CanHireMoreGuideWaiter()
        {
            return GetHiredGuideWaiterCount() < GetGuideUnlockedWaiterHireCount()
                   && GetHireCandidatesForRole(StaffPosition.Waiter, 1).Count > 0;
        }

        /// <summary>
        /// 是否处于开业前「招聘」引导阶段（尚未凑齐开业所需员工）。
        /// </summary>
        public bool IsInGameplayGuideRecruitStage()
        {
            EnsureGameplayDefaults();
            var guide = SaveData.gameplay.gameplayGuide;
            return guide != null && !SaveData.tavern.isOpen && guide.currentStage == GameplayGuideStage.Recruit;
        }

        /// <summary>
        /// 招聘面板是否展示掌柜页签：掌柜开场后自动拥有，始终不展示。
        /// </summary>
        public bool ShouldShowStaffHireShopkeeperTab()
        {
            return false;
        }

        /// <summary>
        /// 招聘面板是否展示厨师页签：常驻显示。
        /// </summary>
        public bool ShouldShowStaffHireChefTab()
        {
            return true;
        }

        /// <summary>
        /// 招聘面板是否展示小二页签：常驻显示。
        /// </summary>
        public bool ShouldShowStaffHireWaiterTab()
        {
            return true;
        }

        /// <summary>
        /// 解析招聘面板默认页签（优先尚未招满的引导职位）。
        /// </summary>
        public StaffHireSelectRole ResolveStaffHireSelectDefaultRole()
        {
            if (ShouldShowStaffHireWaiterTab())
            {
                return StaffHireSelectRole.Waiter;
            }

            if (ShouldShowStaffHireChefTab())
            {
                return StaffHireSelectRole.Chef;
            }

            return StaffHireSelectRole.Waiter;
        }

        /// <summary>
        /// 获取引导招聘员工的铜钱花费。
        /// </summary>
        /// <param name="preferredStaffId">优先员工编号。</param>
        /// <param name="role">员工角色。</param>
        /// <returns>招聘花费，找不到配置时返回 0。</returns>
        public int GetGuideStaffHireCost(int preferredStaffId, StaffRole role)
        {
            EnsureGameplayDefaults();
            var staff = FindGuideStaff(preferredStaffId, role);
            var levelConfig = staff != null ? staff.GetLevelConfig(1) : null;
            var soCost = levelConfig != null ? Mathf.Max(0, levelConfig.hireUpgradeCost) : 0;
            return StaffConfigUtility.GetRecruitmentCost(preferredStaffId, soCost);
        }

        /// <summary>
        /// 获取引导招聘员工配置。
        /// </summary>
        /// <param name="preferredStaffId">优先员工编号。</param>
        /// <param name="role">员工角色。</param>
        /// <returns>员工配置。</returns>
        public SO_Staff GetGuideStaffConfig(int preferredStaffId, StaffRole role)
        {
            return FindGuideStaff(preferredStaffId, role);
        }

        /// <summary>
        /// 尝试处理购买引导设备。
        /// </summary>
        /// <param name="equipmentId">数据编号。</param>
        /// <param name="displayNameOverride">数据编号。</param>
        /// <param name="message">参数值。</param>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        private bool TryPurchaseGuideEquipment(int equipmentId, string displayNameOverride, out string message)
        {
            EnsureGameplayDefaults();
            if (IsVisitingOtherTavern)
            {
                message = "访客模式下不可新增设施";
                return false;
            }

            if (HasOwnedEquipment(equipmentId))
            {
                message = $"{displayNameOverride}已购买";
                return false;
            }

            if (equipmentId == CounterEquipmentId && !CanPurchaseGuideBasicEquipmentItem(GuideBasicCounter, true))
            {
                message = "当前引导阶段不可购买掌柜桌";
                return false;
            }

            if (equipmentId == StoveEquipmentId && !CanPurchaseGuideKitchenEquipmentItem(GuideKitchenStove, true))
            {
                message = "当前引导阶段不可购买灶台";
                return false;
            }

            var facility = FacilityConfigUtility.GetByEquipmentId(equipmentId)
                           ?? (equipmentId == CounterEquipmentId
                               ? FacilityConfigUtility.GetByGuideKey(GuideBasicCounter)
                               : equipmentId == StoveEquipmentId
                                   ? FacilityConfigUtility.GetByGuideKey(GuideKitchenStove)
                                   : null);
            if (!FacilityConfigUtility.MeetsUnlockLevel(facility, GetTavernLevel()))
            {
                message = $"酒楼等级不足，需要达到{facility.UnlockLevel}级";
                return false;
            }

            var cost = GetGuideEquipmentPurchaseCost(equipmentId);
            if (PlayerData.coinNum < cost)
            {
                message = $"铜钱不足，购买{displayNameOverride}需要 {cost}";
                return false;
            }

            ChangeCoinNum(-cost);
            if (equipmentId == CounterEquipmentId)
            {
                GameplayGuideData.purchasedCounter = true;
            }

            if (equipmentId > 0)
            {
                SaveData.gameplay.ownedEquipment.Add(new LocalEquipmentSaveData
                {
                    equipmentId = (byte)equipmentId,
                    currentLevel = 1,
                    physicalSlotIndex = (byte)SaveData.gameplay.ownedEquipment.Count
                });
            }

            GrantFacilityBuildPrestige(facility);
            SyncGameplayGuideProgress();
            NotifyAchievementStatsChanged();
            Signals.Get<GameplayGuideProgressSignal>().Dispatch();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            SaveGame();
            message = $"已购买{displayNameOverride}";
            return true;
        }

        /// <summary>
        /// 尝试处理招聘引导员工。
        /// </summary>
        /// <param name="preferredStaffId">数据编号。</param>
        /// <param name="role">参数值。</param>
        /// <param name="displayNameOverride">数据编号。</param>
        /// <param name="message">参数值。</param>
        /// <param name="maxCount">允许招聘的最大数量。</param>
        /// <param name="allowDuplicate">是否允许同一种员工重复招聘。</param>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        private bool TryHireGuideStaff(int preferredStaffId, StaffRole role, string displayNameOverride, out string message, int maxCount = 1, bool allowDuplicate = false)
        {
            EnsureGameplayDefaults();
            if (IsVisitingOtherTavern)
            {
                message = "访客模式下不可雇佣员工";
                return false;
            }

            var hiredCount = CountGuideHireSlots(preferredStaffId, role);
            if (allowDuplicate)
            {
                if (hiredCount >= Mathf.Max(1, maxCount))
                {
                    message = $"{displayNameOverride}已达到招聘上限";
                    return false;
                }
            }
            else if (hiredCount > 0)
            {
                message = $"{displayNameOverride}已招聘";
                return false;
            }

            var staff = FindGuideStaff(preferredStaffId, role);
            if (staff == null)
            {
                message = $"未找到{displayNameOverride}配置";
                return false;
            }

            var cost = GetGuideStaffHireCost(preferredStaffId, role);

            if (PlayerData.coinNum < cost)
            {
                message = $"铜钱不足，招聘{displayNameOverride}需要 {cost}";
                return false;
            }

            ChangeCoinNum(-cost);

            if (int.TryParse(staff.staffId, out var numericStaffId) && !SaveData.gameplay.hiredStaffIds.Contains(numericStaffId))
            {
                SaveData.gameplay.hiredStaffIds.Add(numericStaffId);
            }

            SaveData.gameplay.ownedStaff.Add(StaffConfigUtility.CreateOwnedStaffSave(preferredStaffId));

            SyncGameplayGuideProgress();
            NotifyAchievementStatsChanged();
            Signals.Get<GameplayGuideProgressSignal>().Dispatch();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            SaveGame();
            message = $"已招聘{displayNameOverride}";
            return true;
        }

        /// <summary>
        /// 处理同步玩法引导进度相关逻辑。
        /// </summary>
        private void SyncGameplayGuideProgress()
        {
            if (SaveData?.gameplay == null)
            {
                return;
            }

            SaveData.gameplay.gameplayGuide ??= new GameplayGuideSaveData();
            SanitizeGuideBuildPlacementPendingKeys();
            var guide = SaveData.gameplay.gameplayGuide;
            var stageTableCount = GetResolvedGuideBuildTableTargetCount();
            guide.purchasedTableCount = Mathf.Max(guide.purchasedTableCount, Mathf.Min(GetUnlockedTableCount(), stageTableCount));
            // 柜台需引导购买，不再默认解锁。
            guide.purchasedStove = guide.purchasedStove || HasOwnedEquipment(StoveEquipmentId);

            // 二楼解锁后厨房桌子默认建成，不再单独购买。
            if (guide.purchasedStairs)
            {
                guide.purchasedSecondFloorTable = true;
            }

            guide.hiredShopkeeper = guide.hiredShopkeeper
                                    || CountHiredByPosition(StaffPosition.Shopkeeper) > 0
                                    || HasHiredGuideStaff(ShopkeeperStaffId, StaffRole.Waiter);
            guide.hiredChef = guide.hiredChef || CountHiredByPosition(StaffPosition.Chef) > 0;
            guide.hiredWaiter = guide.hiredWaiter || CountHiredByPosition(StaffPosition.Waiter) > 0;
            // 打烊即可雇人；开业：桌≥配置数 + 灶台炉子 + 厨师/小二各达标（掌柜可选）。
            guide.recruitmentUnlocked = CanRecruitGuideStaff(guide);
            guide.openingUnlocked = AreGuideBuildStepsCompleted(guide)
                                     && (GetGuideRequiredShopkeeperCount() <= 0
                                         || CountHiredByPosition(StaffPosition.Shopkeeper) >= GetGuideRequiredShopkeeperCount()
                                         || CountHiredGuideStaff(ShopkeeperStaffId, StaffRole.Waiter) >= GetGuideRequiredShopkeeperCount())
                                     && CountHiredByPosition(StaffPosition.Chef) >= GetGuideRequiredChefCount()
                                     && CountActiveFloorStaffByPosition(StaffPosition.Waiter) >= GetGuideRequiredWaiterCount();
            guide.onboardingCompleted = guide.onboardingCompleted || SaveData.tavern.isOpen;
            guide.currentStage = SaveData.tavern.isOpen
                ? GameplayGuideStage.Running
                : guide.openingUnlocked
                    ? GameplayGuideStage.ReadyToOpen
                    : !AreGuideBuildStepsCompleted(guide)
                        ? GameplayGuideStage.Build
                        : GameplayGuideStage.Recruit;
        }

        /// <summary>
        /// 获取当前轮到执行的建造子阶段。
        /// </summary>
        private static bool TryGetActiveGuideBuildStep(GameplayGuideSaveData guide, out GameplayGuideBuildStep buildStep)
        {
            buildStep = default;
            if (guide == null)
            {
                return false;
            }

            for (var index = 0; index < GuideBuildStepOrder.Length; index++)
            {
                var currentStep = GuideBuildStepOrder[index];
                if (!IsGuideBuildStepCompleted(guide, currentStep))
                {
                    buildStep = currentStep;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断指定建造子阶段是否就是当前阶段。
        /// </summary>
        private static bool IsGuideBuildStepActive(GameplayGuideSaveData guide, GameplayGuideBuildStep buildStep)
        {
            return TryGetActiveGuideBuildStep(guide, out var activeStep) && activeStep == buildStep;
        }

        /// <summary>
        /// 判断所有配置到顺序数组里的建造子阶段是否已完成。
        /// </summary>
        private static bool AreGuideBuildStepsCompleted(GameplayGuideSaveData guide)
        {
            if (guide == null)
            {
                return false;
            }

            for (var index = 0; index < GuideBuildStepOrder.Length; index++)
            {
                if (!IsGuideBuildStepCompleted(guide, GuideBuildStepOrder[index]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 按子阶段类型判断对应目标数量是否已完成。
        /// </summary>
        private static bool IsGuideBuildStepCompleted(GameplayGuideSaveData guide, GameplayGuideBuildStep buildStep)
        {
            if (guide == null)
            {
                return false;
            }

            return buildStep switch
            {
                GameplayGuideBuildStep.BasicEquipment => GetPurchasedBasicEquipmentCount(guide) >= GetResolvedGuideBuildBasicEquipmentTargetCount(),
                GameplayGuideBuildStep.Tables => guide.purchasedTableCount >= GetResolvedGuideBuildTableTargetCount(),
                // 开业厨房硬条件：灶台 + 炉子（不把厨房桌算进开业门槛）。
                GameplayGuideBuildStep.KitchenEquipment => IsGuideKitchenOpeningRequirementMet(guide),
                _ => true
            };
        }

        /// <summary>
        /// 开业厨房条件：配置目标 &gt; 0 时必须已购灶台与炉子。
        /// </summary>
        private static bool IsGuideKitchenOpeningRequirementMet(GameplayGuideSaveData guide)
        {
            if (guide == null)
            {
                return false;
            }

            var target = GetResolvedGuideBuildKitchenEquipmentTargetCount();
            if (target <= 0)
            {
                return true;
            }

            // 灶台、炉子均为开业必须项；目标≥2 时两者都要。
            if (target >= 2)
            {
                return guide.purchasedStove && guide.purchasedFurnace;
            }

            return guide.purchasedStove;
        }

        /// <summary>
        /// 从 TbConfig 读取引导开业所需桌子数（不再读引导任务表）。
        /// </summary>
        private static int GetGuideOpeningTableCount()
        {
            return TbConfigRuntime.GetGuideOpeningTableCount(DefaultGuideOpeningTableCount);
        }

        /// <summary>
        /// 从 TbConfig 读取引导开业所需基础设施数。
        /// </summary>
        private static int GetGuideRequiredBasicEquipmentCount()
        {
            return TbConfigRuntime.GetGuideRequiredBasicEquipmentCount(DefaultGuideRequiredBasicEquipmentCount);
        }

        /// <summary>
        /// 从 TbConfig 读取引导开业所需厨房设施数。
        /// </summary>
        private static int GetGuideRequiredKitchenEquipmentCount()
        {
            return TbConfigRuntime.GetGuideRequiredKitchenEquipmentCount(DefaultGuideRequiredKitchenEquipmentCount);
        }

        /// <summary>
        /// 从 TbConfig 读取引导开业所需掌柜数。
        /// </summary>
        private static int GetGuideRequiredShopkeeperCount()
        {
            return TbConfigRuntime.GetGuideRequiredShopkeeperCount(DefaultGuideRequiredShopkeeperCount);
        }

        /// <summary>
        /// 从 TbConfig 读取引导开业所需厨师数。
        /// </summary>
        private static int GetGuideRequiredChefCount()
        {
            return TbConfigRuntime.GetGuideRequiredChefCount(DefaultGuideRequiredChefCount);
        }

        /// <summary>
        /// 从 TbConfig 读取引导开业所需小二数。
        /// </summary>
        private static int GetGuideRequiredWaiterCount()
        {
            return TbConfigRuntime.GetGuideRequiredWaiterCount(DefaultGuideRequiredWaiterCount);
        }

        /// <summary>
        /// 读取建造阶段基础设施目标数量，使用基础设施开业要求配置。
        /// </summary>
        internal static int GetResolvedGuideBuildBasicEquipmentTargetCount()
        {
            return GetGuideRequiredBasicEquipmentCount();
        }

        /// <summary>
        /// 读取建造阶段桌位目标数量，使用开业桌位配置。
        /// </summary>
        internal static int GetResolvedGuideBuildTableTargetCount()
        {
            return GetGuideOpeningTableCount();
        }

        /// <summary>
        /// 读取建造阶段厨房设施目标数量，使用厨房设施开业要求配置。
        /// </summary>
        internal static int GetResolvedGuideBuildKitchenEquipmentTargetCount()
        {
            return GetGuideRequiredKitchenEquipmentCount();
        }

        /// <summary>
        /// 统计当前引导已购买的基础设施数量。
        /// </summary>
        private static int GetPurchasedBasicEquipmentCount(GameplayGuideSaveData guide)
        {
            if (guide == null)
            {
                return 0;
            }

            var count = 0;
            if (guide.purchasedCounter)
            {
                count++;
            }

            if (guide.purchasedCabinet)
            {
                count++;
            }

            if (guide.purchasedCabinet2)
            {
                count++;
            }

            if (guide.purchasedWineCabinet)
            {
                count++;
            }

            if (guide.purchasedWaterJarPile)
            {
                count++;
            }

            // 轿子/楼梯按星级开放购买，不计入开业引导基础设施进度。
            return count;
        }

        /// <summary>
        /// 统计开业相关厨房进度：仅灶台 + 炉子（厨房桌不计入开业条件）。
        /// </summary>
        private static int GetPurchasedKitchenEquipmentCount(GameplayGuideSaveData guide)
        {
            if (guide == null)
            {
                return 0;
            }

            var count = 0;
            if (guide.purchasedStove)
            {
                count++;
            }

            if (guide.purchasedFurnace)
            {
                count++;
            }

            return count;
        }

        /// <summary>
        /// 已废弃：柜台改为引导购买，不再默认解锁。
        /// </summary>
        private void EnsureGuideCounterDefaultUnlocked(GameplayGuideSaveData guide)
        {
        }

        /// <summary>
        /// 处理是否拥有设备相关逻辑。
        /// </summary>
        /// <param name="equipmentId">数据编号。</param>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        public bool HasOwnedEquipment(int equipmentId)
        {
            var ownedEquipment = SaveData?.gameplay?.ownedEquipment;
            if (ownedEquipment == null)
            {
                return false;
            }

            for (var index = 0; index < ownedEquipment.Count; index++)
            {
                var equipment = ownedEquipment[index];
                if (equipment != null && equipment.equipmentId == equipmentId && equipment.currentLevel > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 处理Is引导厨房物件购买d相关逻辑。
        /// </summary>
        /// <param name="itemKey">语言表键值。</param>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        public bool IsGuideKitchenItemPurchased(string itemKey)
        {
            var guide = GameplayGuideData;
            return itemKey switch
            {
                GuideKitchenStove => guide.purchasedStove,
                GuideKitchenFurnace => guide.purchasedFurnace,
                GuideKitchenCabinet => guide.purchasedCabinet,
                GuideKitchenCabinet2 => guide.purchasedCabinet2,
                GuideKitchenWineCabinet => guide.purchasedWineCabinet,
                GuideKitchenWaterJarPile => guide.purchasedWaterJarPile,
                GuideJiaozi => guide.purchasedJiaozi,
                GuideStairs => guide.purchasedStairs,
                GuideSecondFloorTable => IsSecondFloorKitchenTableUnlocked(),
                GuideXitai => guide.purchasedXitai,
                GuideKitchenTable1 => guide.purchasedKitchenTable1,
                GuideKitchenTable2 => guide.purchasedKitchenTable2,
                GuideDecoration1 => IsSecondFloorDecorationPurchased(GuideDecoration1),
                GuideDecoration2 => IsSecondFloorDecorationPurchased(GuideDecoration2),
                GuideDecoration3 => IsSecondFloorDecorationPurchased(GuideDecoration3),
                GuideDecoration4 => IsSecondFloorDecorationPurchased(GuideDecoration4),
                GuideDecoration5 => IsSecondFloorDecorationPurchased(GuideDecoration5),
                GuideDecoration6 => IsSecondFloorDecorationPurchased(GuideDecoration6),
                _ => false
            };
        }

        /// <summary>
        /// 判断是否为基础设施键值。
        /// </summary>
        private static bool IsGuideBasicEquipmentKey(string itemKey)
        {
            return itemKey == GuideBasicCounter
                   || itemKey == GuideKitchenCabinet
                   || itemKey == GuideKitchenCabinet2
                   || itemKey == GuideKitchenWineCabinet
                   || itemKey == GuideKitchenWaterJarPile
                   || itemKey == GuideJiaozi
                   || itemKey == GuideStairs;
        }

        /// <summary>
        /// 判断是否为厨房设施键值。
        /// </summary>
        private static bool IsGuideKitchenEquipmentKey(string itemKey)
        {
            return itemKey == GuideKitchenStove
                   || itemKey == GuideKitchenFurnace
                   || itemKey == GuideKitchenTable1
                   || itemKey == GuideKitchenTable2;
        }

        /// <summary>
        /// 判断基础设施是否已购买。
        /// </summary>
        private bool IsGuideBasicEquipmentPurchased(string itemKey)
        {
            var guide = GameplayGuideData;
            return itemKey switch
            {
                GuideBasicCounter => guide.purchasedCounter,
                GuideKitchenCabinet => guide.purchasedCabinet,
                GuideKitchenCabinet2 => guide.purchasedCabinet2,
                GuideKitchenWineCabinet => guide.purchasedWineCabinet,
                GuideKitchenWaterJarPile => guide.purchasedWaterJarPile,
                GuideJiaozi => guide.purchasedJiaozi,
                GuideStairs => guide.purchasedStairs,
                _ => false
            };
        }

        /// <summary>
        /// 按阶段配置只展示固定范围内第一个尚未购买的建造入口。
        /// </summary>
        private static bool IsCurrentGuideItemInStageRange(string itemKey, string[] orderedKeys, System.Func<string, bool> isPurchased, int stageCount)
        {
            if (string.IsNullOrEmpty(itemKey) || orderedKeys == null || isPurchased == null || stageCount <= 0)
            {
                return false;
            }

            var maxIndex = Mathf.Min(stageCount, orderedKeys.Length);
            for (var index = 0; index < maxIndex; index++)
            {
                var currentKey = orderedKeys[index];
                if (isPurchased(currentKey))
                {
                    continue;
                }

                return currentKey == itemKey;
            }

            return false;
        }

        /// <summary>
        /// 按阶段配置只展示固定编号范围内第一个尚未解锁的桌位入口。
        /// </summary>
        private bool IsGuideTableInStageRange(int tableId)
        {
            var stageCount = GetResolvedGuideBuildTableTargetCount();
            if (tableId <= 0 || tableId > stageCount)
            {
                return false;
            }

            for (var currentTableId = 1; currentTableId <= stageCount; currentTableId++)
            {
                var tableData = GetTableData(currentTableId);
                if (tableData == null || tableData.isUnlocked)
                {
                    continue;
                }

                return currentTableId == tableId;
            }

            return false;
        }

        /// <summary>
        /// 获取引导厨房显示名称。
        /// </summary>
        /// <param name="itemKey">语言表键值。</param>
        /// <returns>返回方法执行后的结果。</returns>
        public static string GetGuideKitchenDisplayName(string itemKey)
        {
            var facility = FacilityConfigUtility.GetByGuideKey(itemKey);
            if (facility != null && !string.IsNullOrWhiteSpace(facility.Name))
            {
                return facility.Name;
            }

            return itemKey switch
            {
                GuideKitchenStove => "灶台",
                GuideKitchenFurnace => "炉子",
                GuideKitchenCabinet => "柜子",
                GuideKitchenCabinet2 => "柜子2",
                GuideKitchenWineCabinet => "酒柜",
                GuideKitchenWaterJarPile => "水缸堆",
                GuideJiaozi => "轿子",
                GuideStairs => "楼梯",
                GuideSecondFloorTable => "二楼桌子",
                GuideXitai => "戏台",
                GuideDecoration1 => "装饰墙",
                GuideDecoration2 => "贵妃椅子",
                GuideDecoration3 => "植物",
                GuideDecoration4 => "花瓶",
                GuideDecoration5 => "盆景",
                GuideDecoration6 => "盆景2",
                GuideKitchenTable1 => "厨房桌子1",
                GuideKitchenTable2 => "厨房桌子2",
                _ => "厨房物件"
            };
        }

        /// <summary>
        /// 二楼厨房桌子（Shop_Interior/桌子）是否已解锁：随楼梯/二楼开放，无需购买。
        /// </summary>
        public bool IsSecondFloorKitchenTableUnlocked()
        {
            EnsureGameplayDefaults();
            return IsStairsUnlocked() || GameplayGuideData.purchasedSecondFloorTable;
        }

        /// <summary>
        /// 二楼设施（桌子/戏台）是否已购买。
        /// </summary>
        public bool IsSecondFloorFacilityPurchased(string guideKey)
        {
            if (guideKey == GuideSecondFloorTable)
            {
                return IsSecondFloorKitchenTableUnlocked();
            }

            return IsGuideKitchenItemPurchased(guideKey);
        }

        /// <summary>
        /// 购买二楼戏台：读 Facility 表花费，不走一楼引导阶段门闩。桌子已改为二楼解锁默认建成。
        /// </summary>
        public bool TryPurchaseSecondFloorFacility(string guideKey, out string message)
        {
            EnsureGameplayDefaults();
            if (IsVisitingOtherTavern)
            {
                message = "访客模式下不可购买";
                return false;
            }

            if (guideKey == GuideSecondFloorTable)
            {
                message = "二楼桌子已随二楼解锁，无需购买";
                return false;
            }

            var facility = FacilityConfigUtility.GetByGuideKey(guideKey);
            if (facility == null
                || (facility.FacilityType != FacilityType.Stage && facility.FacilityType != FacilityType.Decoration))
            {
                message = "未找到可购买物件";
                return false;
            }

            if (IsGuideKitchenItemPurchased(guideKey))
            {
                message = $"{GetGuideKitchenDisplayName(guideKey)}已购买";
                return false;
            }

            if (!FacilityConfigUtility.MeetsPrerequisites(facility, IsConfiguredFacilityUnlocked))
            {
                message = "需要先购买前置设施";
                return false;
            }

            if (!FacilityConfigUtility.MeetsUnlockLevel(facility, GetTavernLevel()))
            {
                var needLevel = facility?.UnlockLevel ?? 3;
                message = $"酒楼等级不足，需要达到{needLevel}级";
                return false;
            }

            var cost = FacilityConfigUtility.GetUnlockCost(facility, 3000);
            if (PlayerData == null || PlayerData.coinNum < cost)
            {
                message = $"铜钱不足，购买{GetGuideKitchenDisplayName(guideKey)}需要{cost}";
                return false;
            }

            ChangeCoinNum(-cost);
            if (IsSecondFloorDecorationGuideKey(guideKey))
            {
                SetSecondFloorDecorationPurchased(guideKey, true);
            }
            else
            {
                SetGuideKitchenItemPurchased(guideKey, true);
            }

            GrantFacilityBuildPrestige(facility);
            // 二楼购买不标记 guideBuildPlacementPending，也不触发一楼搬运落位。
            SaveGame();
            Signals.Get<GameplayGuideProgressSignal>().Dispatch();
            message = $"{GetGuideKitchenDisplayName(guideKey)}购买成功";
            return true;
        }

        /// <summary>
        /// 读取二楼设施购买价格（Facility.cost）。
        /// </summary>
        public int GetSecondFloorFacilityCost(string guideKey)
        {
            var fallback = guideKey == GuideXitai ? 3000 : 1000;
            return FacilityConfigUtility.GetUnlockCost(
                FacilityConfigUtility.GetByGuideKey(guideKey),
                fallback);
        }

        /// <summary>
        /// 设置引导厨房物件购买d。
        /// </summary>
        /// <param name="itemKey">语言表键值。</param>
        /// <param name="value">参数值。</param>
        private void SetGuideKitchenItemPurchased(string itemKey, bool value)
        {
            var guide = GameplayGuideData;
            switch (itemKey)
            {
                case GuideKitchenStove:
                    guide.purchasedStove = value;
                    break;
                case GuideKitchenFurnace:
                    guide.purchasedFurnace = value;
                    break;
                case GuideKitchenCabinet:
                    guide.purchasedCabinet = value;
                    break;
                case GuideKitchenCabinet2:
                    guide.purchasedCabinet2 = value;
                    break;
                case GuideKitchenWineCabinet:
                    guide.purchasedWineCabinet = value;
                    break;
                case GuideKitchenWaterJarPile:
                    guide.purchasedWaterJarPile = value;
                    break;
                case GuideJiaozi:
                    guide.purchasedJiaozi = value;
                    break;
                case GuideStairs:
                    guide.purchasedStairs = value;
                    if (value)
                    {
                        // 扩建二楼：同步小镇自家店铺为 Prefab_BuildingLv2。
                        TrySyncOwnTownExteriorBuildingLevel();
                    }

                    break;
                case GuideSecondFloorTable:
                    guide.purchasedSecondFloorTable = value;
                    break;
                case GuideXitai:
                    guide.purchasedXitai = value;
                    break;
                case GuideKitchenTable1:
                    guide.purchasedKitchenTable1 = value;
                    break;
                case GuideKitchenTable2:
                    guide.purchasedKitchenTable2 = value;
                    break;
            }
        }

        /// <summary>
        /// 判断指定员工角色是否已经招聘。
        /// </summary>
        /// <param name="role">参数值。</param>
        /// <returns>满足条件时返回 真，否则返回 假。</returns>
        private bool HasHiredStaffRole(StaffRole role)
        {
            var ownedStaff = SaveData?.gameplay?.ownedStaff;
            if (ownedStaff != null)
            {
                for (var index = 0; index < ownedStaff.Count; index++)
                {
                    var staffData = ownedStaff[index];
                    if (staffData == null || staffData.staffId <= 0)
                    {
                        continue;
                    }

                    var staff = FindGuideStaff(staffData.staffId, role, false);
                    if (staff != null && staff.role == role)
                    {
                        return true;
                    }
                }
            }

            var hiredStaffIds = SaveData?.gameplay?.hiredStaffIds;
            if (hiredStaffIds == null)
            {
                return false;
            }

            for (var index = 0; index < hiredStaffIds.Count; index++)
            {
                var staff = FindGuideStaff(hiredStaffIds[index], role, false);
                if (staff != null && staff.role == role)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 引导招聘占用槽位：按职位统计（与新招聘池共享上限）。
        /// </summary>
        private int CountGuideHireSlots(int preferredStaffId, StaffRole role)
        {
            if (preferredStaffId == ShopkeeperStaffId)
            {
                return CountHiredByPosition(StaffPosition.Shopkeeper);
            }

            if (role == StaffRole.Chef)
            {
                return CountHiredByPosition(StaffPosition.Chef);
            }

            return CountActiveFloorStaffByPosition(StaffPosition.Waiter);
        }

        /// <summary>
        /// 判断指定员工编号是否已经被当前存档招聘。
        /// </summary>
        /// <param name="preferredStaffId">员工配置编号。</param>
        /// <param name="role">员工角色，用于校验配置类型。</param>
        /// <returns>已经招聘该员工时返回 true，否则返回 false。</returns>
        private bool HasHiredGuideStaff(int preferredStaffId, StaffRole role)
        {
            return CountHiredGuideStaff(preferredStaffId, role) > 0;
        }

        /// <summary>
        /// 统计指定引导员工在当前存档中的招聘数量。
        /// </summary>
        /// <param name="preferredStaffId">员工配置编号。</param>
        /// <param name="role">员工角色，用于过滤同编号以外的兼容数据。</param>
        /// <returns>已招聘数量。</returns>
        private int CountHiredGuideStaff(int preferredStaffId, StaffRole role)
        {
            var count = 0;
            var ownedStaff = SaveData?.gameplay?.ownedStaff;
            if (ownedStaff != null)
            {
                for (var index = 0; index < ownedStaff.Count; index++)
                {
                    var staffData = ownedStaff[index];
                    if (staffData == null || staffData.staffId <= 0 || staffData.staffId != preferredStaffId)
                    {
                        continue;
                    }

                    var staff = FindGuideStaff(staffData.staffId, role, false);
                    if (staff != null && staff.role == role)
                    {
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                return count;
            }

            // 旧存档可能只写入 hiredStaffIds，至少按 1 个已招聘来兼容。
            var hiredStaffIds = SaveData?.gameplay?.hiredStaffIds;
            if (hiredStaffIds == null)
            {
                return 0;
            }

            for (var index = 0; index < hiredStaffIds.Count; index++)
            {
                if (hiredStaffIds[index] != preferredStaffId)
                {
                    continue;
                }

                var staff = FindGuideStaff(hiredStaffIds[index], role, false);
                if (staff != null && staff.role == role)
                {
                    return 1;
                }
            }

            return 0;
        }

        /// <summary>
        /// 查找引导员工。
        /// </summary>
        /// <param name="preferredStaffId">数据编号。</param>
        /// <param name="role">参数值。</param>
        /// <param name="allowRoleFallback">参数值。</param>
        /// <returns>返回方法执行后的结果。</returns>
        private static SO_Staff FindGuideStaff(int preferredStaffId, StaffRole role, bool allowRoleFallback = true)
        {
            var allStaff = SO_Staff.GetAll();
            SO_Staff fallback = null;
            for (var index = 0; index < allStaff.Count; index++)
            {
                var staff = allStaff[index];
                if (staff == null || staff.role != role)
                {
                    continue;
                }

                fallback ??= staff;

                if (int.TryParse(staff.staffId, out var numericId) && numericId == preferredStaffId)
                {
                    return staff;
                }
            }

            return allowRoleFallback ? fallback : null;
        }

        /// <summary>
        /// 获取引导设备购买花费。
        /// </summary>
        /// <param name="equipmentId">数据编号。</param>
        /// <returns>返回计算后的数值。</returns>
        private static int GetGuideEquipmentPurchaseCost(int equipmentId)
        {
            var facility = FacilityConfigUtility.GetByEquipmentId(equipmentId);
            if (facility != null)
            {
                var fromTable = FacilityConfigUtility.GetUnlockCost(facility, 0);
                if (fromTable > 0)
                {
                    return fromTable;
                }
            }

            var equipment = SO_Equipment.GetById(equipmentId);
            var levelConfig = equipment != null ? equipment.GetLevelConfig(1) : null;
            return levelConfig != null ? Mathf.Max(0, levelConfig.upgradeCost) : 0;
        }
    }

    /// <summary>
    /// 对酒楼引导链路提供稳定的查询与操作入口，隔离 UI / 场景层对 DataManager 细节的直接依赖。
    /// </summary>
    public sealed class TavernGuideService
    {
        public static TavernGuideService Instance { get; } = new();

        public GameplayGuideSnapshot GetSnapshot()
        {
            return DataManager.Instance?.GetGameplayGuideSnapshot();
        }

        public GameplayGuideTaskProgress GetCurrentTask()
        {
            return DataManager.Instance?.GetCurrentGameplayGuideTask();
        }

        public bool IsBusinessOpen()
        {
            return DataManager.Instance != null
                   && DataManager.Instance.TavernData != null
                   && DataManager.Instance.TavernData.isOpen;
        }

        public bool CanOpenBusiness()
        {
            return DataManager.Instance != null && DataManager.Instance.CanOpenTavernBusiness();
        }

        public bool ShouldShowGuidePanel()
        {
            var snapshot = GetSnapshot();
            return snapshot != null && !snapshot.OnboardingCompleted;
        }

        public bool ShouldPlayFirstOpeningVideo()
        {
            // 开场引导视频已关闭，直接开业不播片。
            return false;
        }

        public int GetBuildProgressCurrent()
        {
            if (DataManager.Instance == null)
            {
                return 0;
            }

            var basicTarget = DataManager.GetResolvedGuideBuildBasicEquipmentTargetCount();
            var tableTarget = DataManager.GetResolvedGuideBuildTableTargetCount();
            var kitchenTarget = DataManager.GetResolvedGuideBuildKitchenEquipmentTargetCount();

            var basicCurrent = Mathf.Clamp(DataManager.Instance.GetPurchasedGuideBasicEquipmentCount(), 0, basicTarget);
            var tableCurrent = Mathf.Clamp(DataManager.Instance.GetUnlockedTableCount(), 0, tableTarget);
            var kitchenCurrent = Mathf.Clamp(DataManager.Instance.GetPurchasedGuideKitchenEquipmentCount(), 0, kitchenTarget);
            return basicCurrent + tableCurrent + kitchenCurrent;
        }

        public int GetBuildProgressTarget()
        {
            if (DataManager.Instance == null)
            {
                return 0;
            }

            return Mathf.Max(0, DataManager.GetResolvedGuideBuildBasicEquipmentTargetCount())
                   + Mathf.Max(0, DataManager.GetResolvedGuideBuildTableTargetCount())
                   + Mathf.Max(0, DataManager.GetResolvedGuideBuildKitchenEquipmentTargetCount());
        }

        public int GetEmployProgressCurrent()
        {
            if (DataManager.Instance == null)
            {
                return 0;
            }

            var shopkeeperTarget = DataManager.Instance.GetRequiredGuideShopkeeperCount();
            var chefTarget = DataManager.Instance.GetRequiredGuideChefCount();
            var waiterTarget = DataManager.Instance.GetRequiredGuideWaiterCount();

            var shopkeeperCurrent = Mathf.Clamp(DataManager.Instance.GetHiredGuideShopkeeperCount(), 0, shopkeeperTarget);
            var chefCurrent = Mathf.Clamp(DataManager.Instance.GetHiredGuideChefCount(), 0, chefTarget);
            var waiterCurrent = Mathf.Clamp(DataManager.Instance.GetHiredGuideWaiterCount(), 0, waiterTarget);
            return shopkeeperCurrent + chefCurrent + waiterCurrent;
        }

        public int GetEmployProgressTarget()
        {
            if (DataManager.Instance == null)
            {
                return 0;
            }

            return Mathf.Max(0, DataManager.Instance.GetRequiredGuideShopkeeperCount())
                   + Mathf.Max(0, DataManager.Instance.GetRequiredGuideChefCount())
                   + Mathf.Max(0, DataManager.Instance.GetRequiredGuideWaiterCount());
        }

        public bool ShouldShowCounterPurchase()
        {
            return DataManager.Instance != null && DataManager.Instance.ShouldShowGuideBasicEquipmentPurchase("counter");
        }

        public bool ShouldShowStovePurchase()
        {
            return DataManager.Instance != null && DataManager.Instance.ShouldShowGuideKitchenEquipmentPurchase("stove");
        }

        public bool ShouldShowShopkeeperRecruit()
        {
            return DataManager.Instance != null
                   && DataManager.Instance.IsStaffRecruitUiUnlockedByAchievement()
                   && DataManager.Instance.CanHireMoreGuideShopkeeper();
        }

        public bool ShouldShowChefRecruit()
        {
            return DataManager.Instance != null
                   && DataManager.Instance.IsStaffRecruitUiUnlockedByAchievement()
                   && DataManager.Instance.CanHireMoreGuideChef()
                   && DataManager.Instance.GetHiredGuideChefCount() < DataManager.Instance.GetRequiredGuideChefCount();
        }

        public bool ShouldShowWaiterRecruit()
        {
            return DataManager.Instance != null
                   && DataManager.Instance.IsStaffRecruitUiUnlockedByAchievement()
                   && DataManager.Instance.CanHireMoreGuideWaiter()
                   && DataManager.Instance.GetHiredGuideWaiterCount() < DataManager.Instance.GetRequiredGuideWaiterCount();
        }

        public bool TryPurchaseCounter(out string message)
        {
            message = "数据管理器未初始化";
            return DataManager.Instance != null && DataManager.Instance.TryPurchaseGuideCounter(out message);
        }

        public bool TryPurchaseStove(out string message)
        {
            message = "数据管理器未初始化";
            return DataManager.Instance != null && DataManager.Instance.TryPurchaseGuideStove(out message);
        }

        public bool TryHireShopkeeper(out string message)
        {
            message = "数据管理器未初始化";
            return DataManager.Instance != null && DataManager.Instance.TryHireGuideShopkeeper(out message);
        }

        public bool TryHireChef(out string message)
        {
            message = "数据管理器未初始化";
            return DataManager.Instance != null && DataManager.Instance.TryHireGuideChef(out message);
        }

        public bool TryHireWaiter(out string message)
        {
            message = "数据管理器未初始化";
            return DataManager.Instance != null && DataManager.Instance.TryHireGuideWaiter(out message);
        }

        public bool TryHireTemporaryWaiter(out string message)
        {
            message = "数据管理器未初始化";
            return DataManager.Instance != null && DataManager.Instance.TryHireTemporaryGuideWaiter(out message);
        }

        public int GetEquipmentCost(int equipmentId)
        {
            var equipment = SO_Equipment.GetById(equipmentId);
            var levelConfig = equipment != null ? equipment.GetLevelConfig(1) : null;
            return levelConfig != null ? Mathf.Max(0, levelConfig.upgradeCost) : 0;
        }

        public int GetStaffCost(int preferredStaffId, StaffRole role)
        {
            return DataManager.Instance != null ? DataManager.Instance.GetGuideStaffHireCost(preferredStaffId, role) : 0;
        }

        public SO_Staff GetStaffConfig(int preferredStaffId, StaffRole role)
        {
            return DataManager.Instance != null ? DataManager.Instance.GetGuideStaffConfig(preferredStaffId, role) : null;
        }
    }
}
