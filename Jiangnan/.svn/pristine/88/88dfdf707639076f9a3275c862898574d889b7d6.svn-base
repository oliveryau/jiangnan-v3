using System.Collections.Generic;
using JN.Client;
using JN.Client.Config;
using JN.Client.Messages;
using JN.Client.Model;
using UnityEngine;

namespace JN.Client.Manager
{
    public partial class DataManager
    {
        /// <summary>
        /// 拜访会话用静态字段：MonoSingleton 可能随场景重建，实例字段会丢标记，
        /// 导致回城 Capture 误把访客模拟写成自家快照。
        /// </summary>
        private static bool s_isVisitingOtherTavern;
        private static int s_visitingTileId;
        private static string s_visitingShopName;
        private static int s_visitingTavernLevel;
        private static bool s_visitingHotMark;
        /// <summary>拜访前各桌 isUnlocked 备份（按下标=tableId）。</summary>
        private static bool[] s_visitTableUnlockBackup;
        /// <summary>拜访中 SaveGame 解锁守卫重入深度，防止嵌套写盘冲掉原始备份。</summary>
        private static int s_visitUnlockSaveGuardDepth;

        /// <summary>是否正在访问他人酒楼（运行时会话，不落盘）。</summary>
        public bool IsVisitingOtherTavern => s_isVisitingOtherTavern;

        /// <summary>不依赖 Instance 的拜访会话查询（离店 Capture 守卫用）。</summary>
        public static bool IsInOtherTavernVisitSession => s_isVisitingOtherTavern;

        /// <summary>当前访问的他人地块 Id。</summary>
        public int VisitingTileId => s_visitingTileId;

        /// <summary>当前访问的店铺显示名。</summary>
        public string VisitingShopName => s_visitingShopName;

        /// <summary>当前访问酒楼的等级（1~3，用于贵客概率等按等级配置）。</summary>
        public int VisitingTavernLevel => s_visitingTavernLevel;

        /// <summary>当前拜访建筑是否火爆（TownBuilding.hotMark）。</summary>
        public bool IsVisitingHotTavern => s_isVisitingOtherTavern && s_visitingHotMark;

        /// <summary>
        /// 是否已解锁轿子（HireStaff_enter 结束后或 3 星自动解锁，无需购买）。
        /// 禁止在此调用 EnsureGameplayDefaults，避免与 SyncGameplayGuideProgress 递归。
        /// </summary>
        public bool IsJiaoziUnlocked()
        {
            return SaveData?.gameplay?.gameplayGuide != null
                   && SaveData.gameplay.gameplayGuide.purchasedJiaozi;
        }

        /// <summary>
        /// 是否已解锁楼梯（需 3 星酒楼购买后生效）。
        /// 禁止在此调用 EnsureGameplayDefaults，避免与 SyncGameplayGuideProgress 递归。
        /// </summary>
        public bool IsStairsUnlocked()
        {
            return SaveData?.gameplay?.gameplayGuide != null
                   && SaveData.gameplay.gameplayGuide.purchasedStairs;
        }

        /// <summary>
        /// 自家城镇店铺外观层数：未扩建二楼为 1，楼梯解锁后为 2（Prefab_BuildingLv2）。
        /// </summary>
        public int ResolveOwnTownExteriorBuildingLevel()
        {
            return IsStairsUnlocked() ? 2 : 1;
        }

        /// <summary>
        /// 按二楼解锁状态同步自家城镇 BuildingInfo.buildingLevel，并返回是否有变更。
        /// </summary>
        public bool TrySyncOwnTownExteriorBuildingLevel()
        {
            var owned = GetOwnedTownBuilding(ResolveCurrentPlayerId());
            if (owned == null || owned.status != 2 || owned.buildingLevel <= 0)
            {
                return false;
            }

            var desired = ResolveOwnTownExteriorBuildingLevel();
            if (owned.buildingLevel == desired)
            {
                return false;
            }

            owned.buildingLevel = desired;
            SaveGame();
            return true;
        }

        /// <summary>
        /// 是否允许拜访他人酒楼（需已购买轿子）。
        /// </summary>
        public bool CanVisitOtherTavern()
        {
            return IsJiaoziUnlocked();
        }

        /// <summary>
        /// 进入他人酒楼访客会话：使用本机酒楼存档作为外观副本，不改写 activeShop。
        /// </summary>
        /// <param name="tileId">他人地块 Id。</param>
        /// <param name="shopName">店铺显示名。</param>
        /// <param name="tavernLevel">对方酒楼等级（通常为建筑等级 1~3）。</param>
        public void BeginVisitOtherTavern(int tileId, string shopName, int tavernLevel = 1)
        {
            BackupTableRuntimeStatesForVisit();
            s_isVisitingOtherTavern = true;
            s_visitingTileId = Mathf.Max(0, tileId);
            s_visitingShopName = string.IsNullOrWhiteSpace(shopName) ? "他人酒楼" : shopName.Trim();
            s_visitingTavernLevel = Mathf.Clamp(Mathf.Max(tavernLevel, 1), 1, MaxTavernLevel);
            s_visitingHotMark = TownBuildingConfigUtility.IsHotByFieldId(s_visitingTileId);
            // 拜访桌数按对方等级固定：LV1=2、LV2=4、LV3=6（临时改内存，离店还原）。
            ApplyVisitTableUnlockLayout();
        }

        /// <summary>
        /// 结束他人酒楼访客会话。
        /// </summary>
        public void EndVisitOtherTavern()
        {
            if (!s_isVisitingOtherTavern)
            {
                return;
            }

            // 先还原桌态/解锁再清拜访标记，确保不把拜访模拟写入自家存档。
            RestoreVisitTableUnlockLayout();
            RestoreTableRuntimeStatesAfterVisit();
            s_isVisitingOtherTavern = false;
            s_visitingTileId = 0;
            s_visitingShopName = null;
            s_visitingTavernLevel = 0;
            s_visitingHotMark = false;
            // 立刻落盘自家解锁，避免拜访脏布局曾写盘或进程中断后回店桌变「未买」。
            SaveGame();
        }

        /// <summary>
        /// 拜访他人酒楼时按建筑等级固定开放的桌数。
        /// </summary>
        public static int GetVisitUnlockedTableCountForLevel(int tavernLevel)
        {
            var level = Mathf.Clamp(Mathf.Max(tavernLevel, 1), 1, MaxTavernLevel);
            return level switch
            {
                1 => 2,
                2 => 4,
                _ => 6
            };
        }

        /// <summary>
        /// 拜访会话：按对方等级临时解锁前 N 张桌（按 tableId 升序），并备份原 isUnlocked。
        /// 同一拜访会话内只备份一次，避免 SaveGame 守卫重入时用拜访布局覆盖自家备份。
        /// </summary>
        private void ApplyVisitTableUnlockLayout()
        {
            EnsureTavernDefaults();
            var tables = SaveData.tavern.tables;
            if (tables == null || tables.Count <= 0)
            {
                return;
            }

            EnsureVisitTableUnlockBackup(tables);

            var ordered = new List<TavernTableSaveData>(tables.Count);
            for (var index = 0; index < tables.Count; index++)
            {
                var table = tables[index];
                if (table == null || table.tableId <= 0)
                {
                    continue;
                }

                ordered.Add(table);
            }

            ordered.Sort((a, b) => a.tableId.CompareTo(b.tableId));
            var unlockCount = GetVisitUnlockedTableCountForLevel(s_visitingTavernLevel);
            for (var index = 0; index < ordered.Count; index++)
            {
                ordered[index].isUnlocked = index < unlockCount;
            }
        }

        /// <summary>
        /// 确保拜访解锁备份已建立；已有备份时只扩容新桌，不覆盖已有项。
        /// </summary>
        private void EnsureVisitTableUnlockBackup(List<TavernTableSaveData> tables)
        {
            var maxId = 0;
            for (var index = 0; index < tables.Count; index++)
            {
                if (tables[index] != null)
                {
                    maxId = Mathf.Max(maxId, tables[index].tableId);
                }
            }

            if (maxId <= 0)
            {
                return;
            }

            if (s_visitTableUnlockBackup == null)
            {
                s_visitTableUnlockBackup = new bool[maxId + 1];
                for (var index = 0; index < tables.Count; index++)
                {
                    var table = tables[index];
                    if (table == null || table.tableId <= 0 || table.tableId >= s_visitTableUnlockBackup.Length)
                    {
                        continue;
                    }

                    s_visitTableUnlockBackup[table.tableId] = table.isUnlocked;
                }

                return;
            }

            if (maxId + 1 <= s_visitTableUnlockBackup.Length)
            {
                return;
            }

            var expanded = new bool[maxId + 1];
            System.Array.Copy(s_visitTableUnlockBackup, expanded, s_visitTableUnlockBackup.Length);
            var oldLength = s_visitTableUnlockBackup.Length;
            for (var index = 0; index < tables.Count; index++)
            {
                var table = tables[index];
                if (table == null || table.tableId < oldLength || table.tableId >= expanded.Length)
                {
                    continue;
                }

                // 会话中新出现的桌：按当前值记入（通常为未解锁）。
                expanded[table.tableId] = table.isUnlocked;
            }

            s_visitTableUnlockBackup = expanded;
        }

        /// <summary>
        /// 拜访结束：还原 isUnlocked，避免污染自家桌解锁落盘。
        /// </summary>
        private void RestoreVisitTableUnlockLayout()
        {
            if (s_visitTableUnlockBackup == null)
            {
                return;
            }

            EnsureTavernDefaults();
            var tables = SaveData.tavern.tables;
            if (tables != null)
            {
                for (var index = 0; index < tables.Count; index++)
                {
                    var table = tables[index];
                    if (table == null
                        || table.tableId <= 0
                        || table.tableId >= s_visitTableUnlockBackup.Length)
                    {
                        continue;
                    }

                    table.isUnlocked = s_visitTableUnlockBackup[table.tableId];
                }
            }

            s_visitTableUnlockBackup = null;
        }

        /// <summary>
        /// 拜访中若触发 SaveGame：先还原解锁再写盘，写完再套回拜访布局，避免脏解锁落盘。
        /// </summary>
        internal void RunSaveGameWithVisitUnlockGuard(System.Action saveBody)
        {
            if (saveBody == null)
            {
                return;
            }

            if (!s_isVisitingOtherTavern || s_visitTableUnlockBackup == null)
            {
                saveBody();
                return;
            }

            // 嵌套 SaveGame：外层已还原过，直接写盘，禁止再次 Apply 以免冲掉原始备份。
            if (s_visitUnlockSaveGuardDepth > 0)
            {
                saveBody();
                return;
            }

            s_visitUnlockSaveGuardDepth++;
            RestoreVisitTableUnlockLayoutKeepingSession();
            try
            {
                saveBody();
            }
            finally
            {
                s_visitUnlockSaveGuardDepth--;
                if (s_visitUnlockSaveGuardDepth == 0 && s_isVisitingOtherTavern)
                {
                    ApplyVisitTableUnlockLayout();
                }
            }
        }

        /// <summary>
        /// 仅还原解锁备份，但保留拜访会话（供 SaveGame 守卫用）。
        /// </summary>
        private void RestoreVisitTableUnlockLayoutKeepingSession()
        {
            if (s_visitTableUnlockBackup == null)
            {
                return;
            }

            EnsureTavernDefaults();
            var tables = SaveData.tavern.tables;
            if (tables == null)
            {
                return;
            }

            for (var index = 0; index < tables.Count; index++)
            {
                var table = tables[index];
                if (table == null
                    || table.tableId <= 0
                    || table.tableId >= s_visitTableUnlockBackup.Length)
                {
                    continue;
                }

                table.isUnlocked = s_visitTableUnlockBackup[table.tableId];
            }
            // 不清 backup：会话内备份只建一次，ApplyVisit 只会重套拜访桌数。
        }

        /// <summary>
        /// 当前场景用于刷客/结账等按等级读表的酒楼等级：拜访用对方等级，否则用自家星级。
        /// </summary>
        public int GetSceneTavernLevelForSpawn()
        {
            if (IsVisitingOtherTavern)
            {
                return Mathf.Clamp(Mathf.Max(VisitingTavernLevel, 1), 1, MaxTavernLevel);
            }

            return GetTavernLevel();
        }

        /// <summary>
        /// Config.selfBuildingFieldId：自家唯一可买地/建造的 Tile 编号。
        /// </summary>
        public int GetSelfBuildingFieldId()
        {
            return TbConfigRuntime.GetSelfBuildingFieldId(1);
        }

        /// <summary>
        /// 该地块是否允许玩家购买/建造（仅配置的自家 fieldId）。
        /// </summary>
        public bool IsSelfTownBuildingField(int tileId)
        {
            var fieldId = GetSelfBuildingFieldId();
            return fieldId > 0 && tileId == fieldId;
        }

        /// <summary>
        /// 首次进入 Town 时按 TownBuilding 表在指定 fieldId 播种他人店铺，写入存档后不再变化。
        /// </summary>
        private void EnsureOtherPlayerShopsSeeded()
        {
            if (!LocalSaveMode.Enabled)
            {
                return;
            }

            EnsureInitializedCore();
            if (SaveData.town.otherPlayerShopsSeeded)
            {
                return;
            }

            var rows = TownBuildingConfigUtility.GetAll();
            if (rows == null || rows.Count == 0)
            {
                SaveData.town.otherPlayerShopsSeeded = true;
                SaveGame();
                return;
            }

            var selfFieldId = GetSelfBuildingFieldId();
            var displayAchievements = AchievementConfigUtility.RollNpcShopDisplayAchievements(
                rows.Count,
                91357 + 17);

            for (var index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                if (row == null || row.FieldId < 1 || row.FieldId > TownTileCount)
                {
                    Debug.LogWarning($"[TownBuilding] 跳过无效行 id={row?.Id}, fieldId={row?.FieldId}");
                    continue;
                }

                if (selfFieldId > 0 && row.FieldId == selfFieldId)
                {
                    Debug.LogWarning(
                        $"[TownBuilding] id={row.Id} fieldId={row.FieldId} 与自家 selfBuildingFieldId 冲突，已跳过");
                    continue;
                }

                var buildingInfo = SaveData.town.buildingInfos.Find(entry => entry != null && entry.tileId == row.FieldId);
                if (buildingInfo == null)
                {
                    continue;
                }

                // 已被玩家占用的地块不再覆盖。
                if (buildingInfo.playerId != 0)
                {
                    continue;
                }

                buildingInfo.playerId = row.PlayerId;
                buildingInfo.name = string.IsNullOrWhiteSpace(row.Name) ? $"店主{row.Id}" : row.Name.Trim();
                buildingInfo.buildingId = 1;
                buildingInfo.buildingLevel = Mathf.Clamp(row.BuildingLevel, 1, 3);
                buildingInfo.buildingTime = 0;
                buildingInfo.status = 2;
                buildingInfo.value = 0;
                buildingInfo.celebrationTime = 0;
                buildingInfo.displayedAchievementId = index < displayAchievements.Count
                    ? displayAchievements[index]?.Id ?? 0
                    : 0;
            }

            SaveData.town.otherPlayerShopsSeeded = true;
            SaveGame();
            EnsureOtherPlayerShopTitleAssignments();
        }

        /// <summary>
        /// 董政与笛子店铺展示成就互换（位置不变，仅交换头衔）。
        /// </summary>
        private void EnsureOtherPlayerShopTitleAssignments()
        {
            if (!LocalSaveMode.Enabled)
            {
                return;
            }

            EnsureInitializedCore();
            if (SaveData.town.otherPlayerShopTitleAssignmentsApplied || !SaveData.town.otherPlayerShopsSeeded)
            {
                return;
            }

            if (!SwapOtherPlayerShopDisplayedAchievements("董政", "笛子"))
            {
                return;
            }

            SaveData.town.otherPlayerShopTitleAssignmentsApplied = true;
            SaveGame();
        }

        private bool SwapOtherPlayerShopDisplayedAchievements(string nameA, string nameB)
        {
            BuildingInfo buildingA = null;
            BuildingInfo buildingB = null;
            foreach (var info in SaveData.town.buildingInfos)
            {
                if (info == null)
                {
                    continue;
                }

                if (info.name == nameA)
                {
                    buildingA = info;
                }
                else if (info.name == nameB)
                {
                    buildingB = info;
                }
            }

            if (buildingA == null || buildingB == null)
            {
                return false;
            }

            (buildingA.displayedAchievementId, buildingB.displayedAchievementId) =
                (buildingB.displayedAchievementId, buildingA.displayedAchievementId);
            return true;
        }
    }
}
