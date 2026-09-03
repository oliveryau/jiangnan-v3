using System;
using System.Collections.Generic;
using cfg;
using UnityEngine;

namespace JN.Client.Config
{
    /// <summary>
    /// Achievement 表读取辅助。
    /// </summary>
    public static class AchievementConfigUtility
    {
        /// <summary>主线「升级酒楼」（成就 Id=3）。</summary>
        public const int MainlineUpgradeTavernId = 3;

        /// <summary>主线「新店开张」（成就 Id=4）。</summary>
        public const int MainlineOpenNewShopId = 4;

        /// <summary>主线首次「外出揽客」（成就 Id=10）。</summary>
        public const int MainlineFirstSolicitId = 10;

        public static Achievement Get(int id)
        {
            return LubanTablesRuntime.GetAchievement(id);
        }

        public static IReadOnlyList<Achievement> GetAllSorted()
        {
            var table = LubanTablesRuntime.GetAchievementTable();
            if (table == null || table.DataList == null || table.DataList.Count == 0)
            {
                return Array.Empty<Achievement>();
            }

            var list = new List<Achievement>(table.DataList);
            list.Sort(CompareDisplayOrder);
            return list;
        }

        /// <summary>
        /// 图鉴展示：每条成就链只取「当前待处理」的一档（未领取的最低档；若均已领取则展示最高档）。
        /// 链按 <see cref="Achievement.AchievementType"/> 分组。
        /// </summary>
        public static IReadOnlyList<Achievement> GetCatalogDisplayAchievements(Func<int, bool> isClaimed)
        {
            if (isClaimed == null)
            {
                throw new ArgumentNullException(nameof(isClaimed));
            }

            var all = GetAllSorted();
            if (all.Count == 0)
            {
                return Array.Empty<Achievement>();
            }

            var chains = new Dictionary<AchievementType, List<Achievement>>();
            for (var index = 0; index < all.Count; index++)
            {
                var achievement = all[index];
                if (achievement == null)
                {
                    continue;
                }

                if (!chains.TryGetValue(achievement.AchievementType, out var chain))
                {
                    chain = new List<Achievement>();
                    chains[achievement.AchievementType] = chain;
                }

                chain.Add(achievement);
            }

            var result = new List<Achievement>(chains.Count);
            foreach (var pair in chains)
            {
                var chain = pair.Value;
                chain.Sort(CompareChainTier);

                Achievement selected = null;
                for (var index = 0; index < chain.Count; index++)
                {
                    var candidate = chain[index];
                    if (!isClaimed(candidate.Id))
                    {
                        selected = candidate;
                        break;
                    }
                }

                if (selected == null)
                {
                    selected = chain[chain.Count - 1];
                }

                if (selected != null)
                {
                    result.Add(selected);
                }
            }

            result.Sort(CompareDisplayOrder);
            return result;
        }

        public static int GetTarget(Achievement achievement, int fallback = 1)
        {
            if (achievement?.Param == null || achievement.Param.Count <= 0)
            {
                return Mathf.Max(1, fallback);
            }

            return Mathf.Max(1, achievement.Param[0]);
        }

        private static int CompareDisplayOrder(Achievement a, Achievement b)
        {
            if (a == null && b == null)
            {
                return 0;
            }

            if (a == null)
            {
                return 1;
            }

            if (b == null)
            {
                return -1;
            }

            // 表已去掉 SortOrder：按 Id 稳定排序。
            return a.Id.CompareTo(b.Id);
        }

        private static int CompareChainTier(Achievement a, Achievement b)
        {
            if (a == null && b == null)
            {
                return 0;
            }

            if (a == null)
            {
                return 1;
            }

            if (b == null)
            {
                return -1;
            }

            var targetCompare = GetTarget(a).CompareTo(GetTarget(b));
            if (targetCompare != 0)
            {
                return targetCompare;
            }

            return CompareDisplayOrder(a, b);
        }

        /// <summary>
        /// 铜框低档成就（002tong），用于城镇其他玩家店铺展示。
        /// </summary>
        public static IReadOnlyList<Achievement> GetLowTierDisplayAchievements()
        {
            return GetDisplayAchievementsWithFrameToken("002tong");
        }

        /// <summary>
        /// 银框低档成就（003yin），用于城镇 NPC 店与铜框混排展示。
        /// </summary>
        public static IReadOnlyList<Achievement> GetMidLowTierDisplayAchievements()
        {
            return GetDisplayAchievementsWithFrameToken("003yin");
        }

        /// <summary>
        /// 按边框档次筛选可展示成就（如 002tong / 003yin）。
        /// </summary>
        public static IReadOnlyList<Achievement> GetDisplayAchievementsWithFrameToken(string frameToken)
        {
            if (string.IsNullOrWhiteSpace(frameToken))
            {
                return Array.Empty<Achievement>();
            }

            var all = GetAllSorted();
            if (all.Count == 0)
            {
                return Array.Empty<Achievement>();
            }

            var result = new List<Achievement>();
            for (var index = 0; index < all.Count; index++)
            {
                var achievement = all[index];
                if (achievement == null || string.IsNullOrWhiteSpace(achievement.Frame))
                {
                    continue;
                }

                if (achievement.Frame.IndexOf(frameToken, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result.Add(achievement);
                }
            }

            result.Sort(CompareDisplayOrder);
            return result;
        }

        /// <summary>
        /// 为城镇 NPC 店随机挑选成就：铜框 + 银框两档混排，避免全部同品质。
        /// </summary>
        public static IReadOnlyList<Achievement> RollNpcShopDisplayAchievements(int count, int randomSeed)
        {
            if (count <= 0)
            {
                return Array.Empty<Achievement>();
            }

            var tongTier = GetLowTierDisplayAchievements();
            var yinTier = GetMidLowTierDisplayAchievements();
            if (tongTier.Count == 0 && yinTier.Count == 0)
            {
                return Array.Empty<Achievement>();
            }

            var random = new System.Random(randomSeed);
            var tongCount = tongTier.Count > 0 && yinTier.Count > 0
                ? Mathf.Min(tongTier.Count, (count + 1) / 2)
                : Mathf.Min(tongTier.Count, count);
            var yinCount = yinTier.Count > 0
                ? Mathf.Min(yinTier.Count, count - tongCount)
                : 0;
            if (tongCount + yinCount < count)
            {
                tongCount = Mathf.Min(tongTier.Count, count - yinCount);
            }

            var picked = new List<Achievement>(count);
            PickRandomUniqueAchievements(tongTier, tongCount, random, picked);
            PickRandomUniqueAchievements(yinTier, yinCount, random, picked);

            var fallbackPool = new List<Achievement>();
            AppendUniqueAchievements(tongTier, fallbackPool, picked);
            AppendUniqueAchievements(yinTier, fallbackPool, picked);
            ShuffleAchievements(fallbackPool, random);
            for (var index = 0; picked.Count < count && index < fallbackPool.Count; index++)
            {
                picked.Add(fallbackPool[index]);
            }

            ShuffleAchievements(picked, random);
            return picked;
        }

        private static void PickRandomUniqueAchievements(
            IReadOnlyList<Achievement> source,
            int count,
            System.Random random,
            List<Achievement> picked)
        {
            if (source == null || source.Count == 0 || count <= 0 || picked == null)
            {
                return;
            }

            var pool = new List<Achievement>();
            for (var index = 0; index < source.Count; index++)
            {
                var achievement = source[index];
                if (achievement != null)
                {
                    pool.Add(achievement);
                }
            }

            ShuffleAchievements(pool, random);
            for (var index = 0; index < pool.Count && index < count; index++)
            {
                picked.Add(pool[index]);
            }
        }

        private static void AppendUniqueAchievements(
            IReadOnlyList<Achievement> source,
            List<Achievement> target,
            IReadOnlyList<Achievement> exclude)
        {
            if (source == null || target == null)
            {
                return;
            }

            for (var index = 0; index < source.Count; index++)
            {
                var achievement = source[index];
                if (achievement == null || ContainsAchievement(exclude, achievement.Id) || ContainsAchievement(target, achievement.Id))
                {
                    continue;
                }

                target.Add(achievement);
            }
        }

        private static bool ContainsAchievement(IReadOnlyList<Achievement> list, int achievementId)
        {
            if (list == null || achievementId <= 0)
            {
                return false;
            }

            for (var index = 0; index < list.Count; index++)
            {
                if (list[index]?.Id == achievementId)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ShuffleAchievements(IList<Achievement> list, System.Random random)
        {
            if (list == null || random == null)
            {
                return;
            }

            for (var index = list.Count - 1; index > 0; index--)
            {
                var swapIndex = random.Next(index + 1);
                (list[index], list[swapIndex]) = (list[swapIndex], list[index]);
            }
        }
    }
}
