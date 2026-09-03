using System;
using System.Collections.Generic;
using cfg;
using UnityEngine;

namespace JN.Client.Config
{
    /// <summary>
    /// TavernTech 科技树表读取与效果参数解析。
    /// </summary>
    public static class TavernTechConfigUtility
    {
        public const int LockedSecondFloorTechId = 107;
        public const int VipCustomerTechId = 105;

        public const int MainPanelTechIdMin = 101;
        public const int MainPanelTechIdMax = 107;

        private static readonly int[] MainPanelTechIds =
        {
            101, 102, 103, 104, 105, 106, 107
        };

        /// <summary>
        /// 科技树主界面展示的 techId 列表（101–107，与预制体 node_{id} 对应）。
        /// </summary>
        public static IReadOnlyList<int> CollectMainPanelTechIds()
        {
            return MainPanelTechIds;
        }

        public static TbTavernTech GetTable()
        {
            return LubanTablesRuntime.GetTables()?.TbTavernTech;
        }

        public static TavernTech Get(int techId)
        {
            return techId <= 0 ? null : GetTable()?.GetOrDefault(techId);
        }

        public static IReadOnlyList<TavernTech> GetAll()
        {
            var table = GetTable();
            return table != null ? table.DataList : Array.Empty<TavernTech>();
        }

        /// <summary>
        /// 效果数值：取 param[0]，缺省 defaultValue。
        /// </summary>
        public static int GetEffectValue(TavernTech tech, int defaultValue = 1)
        {
            if (tech?.Param == null || tech.Param.Count == 0)
            {
                return defaultValue;
            }

            return tech.Param[0];
        }

        public static bool MeetsPrerequisites(TavernTech tech, Func<int, bool> isResearched)
        {
            if (tech == null)
            {
                return false;
            }

            if (tech.Unlock == null || tech.Unlock.Count == 0)
            {
                return true;
            }

            if (isResearched == null)
            {
                return false;
            }

            for (var index = 0; index < tech.Unlock.Count; index++)
            {
                if (!isResearched(tech.Unlock[index]))
                {
                    return false;
                }
            }

            return true;
        }

        public static int SumAdditive(TavernTechType techType, IEnumerable<int> researchedTechIds)
        {
            if (researchedTechIds == null)
            {
                return 0;
            }

            var sum = 0;
            foreach (var techId in researchedTechIds)
            {
                var tech = Get(techId);
                if (tech == null || tech.TechType != techType)
                {
                    continue;
                }

                sum += GetEffectValue(tech, 0);
            }

            return sum;
        }

        /// <summary>
        /// 千分比倍率相乘：param 900 → ×0.9；无相关科技时返回 1。
        /// </summary>
        public static float ProductPermilleMul(TavernTechType techType, IEnumerable<int> researchedTechIds)
        {
            if (researchedTechIds == null)
            {
                return 1f;
            }

            var product = 1f;
            var any = false;
            foreach (var techId in researchedTechIds)
            {
                var tech = Get(techId);
                if (tech == null || tech.TechType != techType)
                {
                    continue;
                }

                any = true;
                var permille = GetEffectValue(tech, 1000);
                product *= Mathf.Max(0.01f, permille / 1000f);
            }

            return any ? Mathf.Clamp(product, 0.1f, 2f) : 1f;
        }

        public static int SumExtraCap(TavernTechType techType, IEnumerable<int> researchedTechIds)
        {
            return Mathf.Max(0, SumAdditive(techType, researchedTechIds));
        }

        /// <summary>
        /// 是否已研究指定效果类型的科技。
        /// </summary>
        public static bool HasResearchedTechType(TavernTechType techType, IEnumerable<int> researchedTechIds)
        {
            if (researchedTechIds == null)
            {
                return false;
            }

            foreach (var techId in researchedTechIds)
            {
                var tech = Get(techId);
                if (tech != null && tech.TechType == techType)
                {
                    return true;
                }
            }

            return false;
        }

        public static TavernTechType CapTypeForPosition(StaffPosition position)
        {
            return position switch
            {
                StaffPosition.Waiter => TavernTechType.ExtraWaiterCap,
                StaffPosition.Chef => TavernTechType.ExtraChefCap,
                StaffPosition.Shopkeeper => TavernTechType.ExtraShopkeeperCap,
                _ => TavernTechType.Custom
            };
        }

        public static IReadOnlyList<int> CollectFlowTechIds()
        {
            return CollectByIdRange(200, 299);
        }

        public static IReadOnlyList<int> CollectBizTechIds()
        {
            return CollectByIdRange(300, 399);
        }

        private static List<int> CollectByIdRange(int minId, int maxId)
        {
            var result = new List<int>();
            var all = GetAll();
            for (var index = 0; index < all.Count; index++)
            {
                var tech = all[index];
                if (tech != null && tech.Id >= minId && tech.Id <= maxId)
                {
                    result.Add(tech.Id);
                }
            }

            result.Sort();
            return result;
        }
    }
}
