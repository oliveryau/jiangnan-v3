using System;
using System.Collections.Generic;
using cfg;
using UnityEngine;

namespace JN.Client.Config
{
    /// <summary>
    /// Facility 表读取：花费、绑定节点、解锁条件、升级设备组。
    /// </summary>
    public static class FacilityConfigUtility
    {
        public static TbFacility GetTable()
        {
            return LubanTablesRuntime.GetTables()?.TbFacility;
        }

        public static Facility Get(int facilityId)
        {
            return facilityId <= 0 ? null : GetTable()?.GetOrDefault(facilityId);
        }

        /// <summary>
        /// 桌子设施：优先 id 直配（1-9），否则按 guideKey / bindNode 查找（如 10-12 用独立 facility id）。
        /// </summary>
        public static Facility GetTableFacility(int tableId)
        {
            if (tableId <= 0)
            {
                return null;
            }

            var direct = Get(tableId);
            if (direct != null && direct.FacilityType == FacilityType.Table)
            {
                return direct;
            }

            return GetByGuideKey($"table_{tableId}");
        }

        /// <summary>
        /// 从桌子设施行解析 TableArea.tableId。
        /// </summary>
        public static bool TryResolveTableId(Facility facility, out int tableId)
        {
            tableId = 0;
            if (facility == null || facility.FacilityType != FacilityType.Table)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(facility.GuideKey)
                && facility.GuideKey.StartsWith("table_", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(facility.GuideKey.Substring("table_".Length), out tableId)
                && tableId > 0)
            {
                return true;
            }

            const string bindPrefix = "TableArea_";
            if (!string.IsNullOrWhiteSpace(facility.BindNode)
                && facility.BindNode.StartsWith(bindPrefix, StringComparison.OrdinalIgnoreCase)
                && int.TryParse(facility.BindNode.Substring(bindPrefix.Length), out tableId)
                && tableId > 0)
            {
                return true;
            }

            var byId = Get(facility.Id);
            if (byId != null && byId.FacilityType == FacilityType.Table)
            {
                tableId = facility.Id;
                return true;
            }

            tableId = 0;
            return false;
        }

        public static Facility GetByGuideKey(string guideKey)
        {
            if (string.IsNullOrWhiteSpace(guideKey))
            {
                return null;
            }

            var list = GetTable()?.DataList;
            if (list == null)
            {
                return null;
            }

            for (var index = 0; index < list.Count; index++)
            {
                var facility = list[index];
                if (facility != null
                    && string.Equals(facility.GuideKey, guideKey, StringComparison.OrdinalIgnoreCase))
                {
                    return facility;
                }
            }

            return null;
        }

        public static Facility GetByEquipmentId(int equipmentId)
        {
            if (equipmentId < 0)
            {
                return null;
            }

            var list = GetTable()?.DataList;
            if (list == null)
            {
                return null;
            }

            for (var index = 0; index < list.Count; index++)
            {
                var facility = list[index];
                if (facility != null && facility.EquipmentId == equipmentId)
                {
                    return facility;
                }
            }

            return null;
        }

        public static IReadOnlyList<Facility> GetByType(FacilityType facilityType)
        {
            var result = new List<Facility>();
            var list = GetTable()?.DataList;
            if (list == null)
            {
                return result;
            }

            for (var index = 0; index < list.Count; index++)
            {
                var facility = list[index];
                if (facility != null && facility.FacilityType == facilityType)
                {
                    result.Add(facility);
                }
            }

            result.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
            return result;
        }

        /// <summary>
        /// 解锁花费：表 cost&gt;0 用表；否则回退 SO_Equipment.lv1；再回退 fallback。
        /// </summary>
        public static int GetUnlockCost(Facility facility, int fallback = 0)
        {
            if (facility == null)
            {
                return fallback;
            }

            if (facility.Cost > 0)
            {
                return facility.Cost;
            }

            if (facility.EquipmentId > 0)
            {
                var equipment = SO_Equipment.GetById(facility.EquipmentId);
                var levelConfig = equipment != null ? equipment.GetLevelConfig(1) : null;
                if (levelConfig != null)
                {
                    return Mathf.Max(0, levelConfig.upgradeCost);
                }
            }

            return fallback;
        }

        public static int GetTableUnlockCost(int tableId, int fallback)
        {
            return GetUnlockCost(GetTableFacility(tableId), fallback);
        }

        /// <summary>
        /// 升级用 SO_Equipment Id：优先 upgradeGroup[0]，否则 equipmentId。
        /// </summary>
        public static int GetUpgradeEquipmentId(Facility facility, int fallback)
        {
            if (facility == null)
            {
                return fallback;
            }

            if (facility.UpgradeGroup != null && facility.UpgradeGroup.Count > 0 && facility.UpgradeGroup[0] > 0)
            {
                return facility.UpgradeGroup[0];
            }

            return facility.EquipmentId > 0 ? facility.EquipmentId : fallback;
        }

        public static int GetTableUpgradeEquipmentId(int tableId, int fallback)
        {
            return GetUpgradeEquipmentId(GetTableFacility(tableId), fallback);
        }

        public static bool MeetsIncome(Facility facility, int totalIncome)
        {
            return facility == null || facility.UnlockIncome <= 0 || totalIncome >= facility.UnlockIncome;
        }

        /// <summary>
        /// 厨房桌等随灶台链路一起建造的设施：不按酒楼星级门闩。
        /// </summary>
        public static bool SkipsTavernLevelGate(Facility facility)
        {
            return facility != null && facility.FacilityType == FacilityType.KitchenTable;
        }

        /// <summary>
        /// unlockLevel≤0 无要求；厨房桌跳过；否则当前酒楼等级 ≥ 配置等级才可建。
        /// </summary>
        public static bool MeetsUnlockLevel(Facility facility, int tavernLevel)
        {
            if (facility == null || facility.UnlockLevel <= 0 || SkipsTavernLevelGate(facility))
            {
                return true;
            }

            return tavernLevel >= facility.UnlockLevel;
        }

        /// <summary>
        /// 建造奖励声望（表字段 getPresitige，生成属性为 GetPresitige）。
        /// </summary>
        public static int GetBuildPrestige(Facility facility)
        {
            return facility == null ? 0 : Mathf.Max(0, facility.GetPresitige);
        }

        public static bool MeetsPrerequisites(Facility facility, Func<int, bool> isFacilityUnlocked)
        {
            if (facility?.Unlock == null || facility.Unlock.Count == 0)
            {
                return true;
            }

            if (isFacilityUnlocked == null)
            {
                return false;
            }

            for (var index = 0; index < facility.Unlock.Count; index++)
            {
                if (!isFacilityUnlocked(facility.Unlock[index]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
