using System.Collections.Generic;
using JN.Client.Config;
using JN.Client.Manager;
using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 贵客刷客：按 vipSpawnChancePermille 从独立预制体池刷出 CustomerM5，不依赖科技。
    /// 结账收入按 Config 倍率加在贵客座位上。
    /// </summary>
    internal static class VipCustomerService
    {
        public const float DefaultIncomeMultiplier = 1.5f;

        /// <summary>科技「贵客到来」是否已研究（仅用于研究完成补刷，刷客判定不读此项）。</summary>
        public static bool IsEnabled => DataManager.Instance != null && DataManager.Instance.IsVipCustomerEnabled();

        /// <summary>
        /// 是否在本轮刷客中走贵客预制体池（不进普通随机池）。不检查科技。
        /// </summary>
        public static bool TrySpawnVip(bool hasVipTemplates, float spawnChance)
        {
            if (!hasVipTemplates)
            {
                return false;
            }

            var chance = Mathf.Clamp01(spawnChance);
            return chance > 0f && Random.value < chance;
        }

        /// <summary>
        /// 贵客结账倍率：只读 Config（千分比）。
        /// </summary>
        public static float ResolveVipCheckoutMultiplier()
        {
            return Mathf.Max(0f, TbConfigRuntime.GetVipCheckoutIncomeMultiplier(DefaultIncomeMultiplier));
        }

        /// <summary>
        /// 按座计价：普客 = 基础价，贵客 = 倍率 × 基础价；无客人时按人数 × 基础价。
        /// </summary>
        public static int ResolveCheckoutIncomeBySeats(
            int unitPrice,
            IList<TavernCustomerRuntimeController> customers,
            int fallbackGroupSize,
            float vipMultiplier)
        {
            var unit = Mathf.Max(1, unitPrice);
            if (customers == null || customers.Count == 0)
            {
                return unit * Mathf.Max(1, fallbackGroupSize);
            }

            var mult = Mathf.Max(0f, vipMultiplier);
            var total = 0;
            for (var i = 0; i < customers.Count; i++)
            {
                var customer = customers[i];
                if (customer != null && customer.IsVip)
                {
                    total += Mathf.Max(1, Mathf.RoundToInt(unit * mult));
                }
                else
                {
                    total += unit;
                }
            }

            return Mathf.Max(1, total);
        }
    }

    /// <summary>
    /// 稀客刷客：按 rareSpawnChancePermille（酒楼等级三档）从 CustomerM6 独立池刷出；无科技门闩。
    /// </summary>
    internal static class RareCustomerService
    {
        /// <summary>
        /// 是否在本轮刷客中走稀客预制体池（不进普通随机池）。
        /// </summary>
        public static bool TrySpawnRare(bool hasRareTemplates, float spawnChance)
        {
            if (!hasRareTemplates)
            {
                return false;
            }

            var chance = Mathf.Clamp01(spawnChance);
            return chance > 0f && Random.value < chance;
        }
    }
}
