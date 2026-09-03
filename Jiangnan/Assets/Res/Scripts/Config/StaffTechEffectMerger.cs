using System;
using System.Collections.Generic;
using cfg;
using UnityEngine;

namespace JN.Client.Config
{
    /// <summary>
    /// 合并已研究员工科技为运行时能力快照。
    /// </summary>
    public static class StaffTechEffectMerger
    {
        private const int DefaultPermille = 1000;

        public sealed class StaffCapabilitySnapshot
        {
            public bool CanOrder { get; internal set; }
            public bool CanServe { get; internal set; }
            public bool CanCheckout { get; internal set; }
            public int MoveSpeedPermille { get; internal set; } = DefaultPermille;
            public int CookSpeedPermille { get; internal set; } = DefaultPermille;
            public int OrderTimePermille { get; internal set; } = DefaultPermille;
            public int ServeTimePermille { get; internal set; } = DefaultPermille;
            public int CheckoutTimePermille { get; internal set; } = DefaultPermille;
            public int CleanTimePermille { get; internal set; } = DefaultPermille;

            public float MoveSpeedMul => MoveSpeedPermille / 1000f;
            public float CookSpeedMul => CookSpeedPermille / 1000f;
            public float OrderTimeMul => OrderTimePermille / 1000f;
            public float ServeTimeMul => ServeTimePermille / 1000f;
            public float CheckoutTimeMul => CheckoutTimePermille / 1000f;
            public float CleanTimeMul => CleanTimePermille / 1000f;
        }

        /// <summary>
        /// 合并 Staff 配表基线技能与已研究同职位科技。
        /// </summary>
        public static StaffCapabilitySnapshot Merge(Staff staff, IEnumerable<int> researchedTechIds)
        {
            var snapshot = CreateBaseline(staff);
            if (staff == null || researchedTechIds == null)
            {
                return snapshot;
            }

            var position = staff.Position;
            foreach (var techId in researchedTechIds)
            {
                var tech = TavernTechConfigUtility.Get(techId);
                if (tech == null
                    || !tech.StaffPosition.HasValue
                    || tech.StaffPosition.Value != position
                    || tech.StaffEffect == StaffTechEffect.None)
                {
                    continue;
                }

                ApplyEffect(snapshot, tech);
            }

            return snapshot;
        }

        public static bool IsCounterRandomRewardEnabled(IEnumerable<int> researchedTechIds)
        {
            if (researchedTechIds == null)
            {
                return false;
            }

            foreach (var techId in researchedTechIds)
            {
                var tech = TavernTechConfigUtility.Get(techId);
                if (tech != null && tech.StaffEffect == StaffTechEffect.EnableCounterRandomReward)
                {
                    return true;
                }
            }

            return false;
        }

        public static TavernTech GetNextStaffTech(StaffPosition position, Func<int, bool> isResearched)
        {
            TavernTech next = null;
            var all = TavernTechConfigUtility.GetAll();
            for (var index = 0; index < all.Count; index++)
            {
                var tech = all[index];
                if (tech == null
                    || !tech.StaffPosition.HasValue
                    || tech.StaffPosition.Value != position
                    || tech.StaffEffect == StaffTechEffect.None)
                {
                    continue;
                }

                if (isResearched != null && isResearched(tech.Id))
                {
                    continue;
                }

                if (!TavernTechConfigUtility.MeetsPrerequisites(tech, isResearched))
                {
                    continue;
                }

                if (next == null || tech.SortOrder < next.SortOrder)
                {
                    next = tech;
                }
            }

            return next;
        }

        public static string DescribeStaffEffect(TavernTech tech)
        {
            if (tech == null || tech.StaffEffect == StaffTechEffect.None)
            {
                return string.Empty;
            }

            return tech.StaffEffect switch
            {
                StaffTechEffect.GrantCanOrder => "解锁点单",
                StaffTechEffect.GrantCanServe => "解锁上菜",
                StaffTechEffect.GrantCanCheckout => "解锁收账",
                StaffTechEffect.ImproveMoveSpeed => $"移速×{tech.StaffEffectValue / 1000f:0.##}",
                StaffTechEffect.ImproveOrderTime => $"点单耗时×{tech.StaffEffectValue / 1000f:0.##}",
                StaffTechEffect.ImproveServeTime => $"上菜耗时×{tech.StaffEffectValue / 1000f:0.##}",
                StaffTechEffect.ImproveCheckoutTime => $"收账耗时×{tech.StaffEffectValue / 1000f:0.##}",
                StaffTechEffect.ImproveCleanTime => $"清扫耗时×{tech.StaffEffectValue / 1000f:0.##}",
                StaffTechEffect.ImproveCookSpeed => $"做菜速度×{tech.StaffEffectValue / 1000f:0.##}",
                StaffTechEffect.EnableCounterRandomReward => "开启柜台随机进账",
                _ => tech.Name
            };
        }

        public static IReadOnlyList<int> CollectStaffTechIds(StaffPosition position)
        {
            var result = new List<int>();
            var all = TavernTechConfigUtility.GetAll();
            for (var index = 0; index < all.Count; index++)
            {
                var tech = all[index];
                if (tech != null
                    && tech.StaffPosition.HasValue
                    && tech.StaffPosition.Value == position
                    && tech.StaffEffect != StaffTechEffect.None)
                {
                    result.Add(tech.Id);
                }
            }

            result.Sort();
            return result;
        }

        private static StaffCapabilitySnapshot CreateBaseline(Staff staff)
        {
            var snapshot = new StaffCapabilitySnapshot();
            if (staff != null && staff.Position == StaffPosition.Waiter)
            {
                // 点单/上菜默认开启；收银需玩家点气泡派工，不自动收账。
                snapshot.CanOrder = true;
                snapshot.CanServe = true;
                snapshot.CanCheckout = false;
            }

            return snapshot;
        }

        private static void ApplyEffect(StaffCapabilitySnapshot snapshot, TavernTech tech)
        {
            var value = Mathf.Max(1, tech.StaffEffectValue);
            switch (tech.StaffEffect)
            {
                case StaffTechEffect.GrantCanOrder:
                    snapshot.CanOrder = true;
                    break;
                case StaffTechEffect.GrantCanServe:
                    snapshot.CanServe = true;
                    break;
                case StaffTechEffect.GrantCanCheckout:
                    snapshot.CanCheckout = true;
                    break;
                case StaffTechEffect.ImproveMoveSpeed:
                    snapshot.MoveSpeedPermille = Mathf.Max(snapshot.MoveSpeedPermille, value);
                    break;
                case StaffTechEffect.ImproveOrderTime:
                    snapshot.OrderTimePermille = Mathf.Min(snapshot.OrderTimePermille, value);
                    break;
                case StaffTechEffect.ImproveServeTime:
                    snapshot.ServeTimePermille = Mathf.Min(snapshot.ServeTimePermille, value);
                    break;
                case StaffTechEffect.ImproveCheckoutTime:
                    snapshot.CheckoutTimePermille = Mathf.Min(snapshot.CheckoutTimePermille, value);
                    break;
                case StaffTechEffect.ImproveCleanTime:
                    snapshot.CleanTimePermille = Mathf.Min(snapshot.CleanTimePermille, value);
                    break;
                case StaffTechEffect.ImproveCookSpeed:
                    snapshot.CookSpeedPermille = Mathf.Max(snapshot.CookSpeedPermille, value);
                    break;
            }
        }
    }
}
