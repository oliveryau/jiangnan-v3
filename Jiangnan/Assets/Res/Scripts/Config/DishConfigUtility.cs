using System;
using System.Collections.Generic;
using cfg;

namespace JN.Client.Config
{
    public enum VipGuestDemandType
    {
        Material = 0,
        Flavor = 1,
        /// <summary>
        /// 三道同口味不同价，任选均算猜对。
        /// </summary>
        FlavorPriceChoice = 2
    }

    /// <summary>
    /// 贵客随机需求。
    /// </summary>
    public readonly struct VipGuestDemand
    {
        public VipGuestDemandType DemandType { get; }
        public string Keyword { get; }
        public string DisplayText { get; }
        public int HintId { get; }

        public VipGuestDemand(VipGuestDemandType demandType, string keyword, string displayText, int hintId)
        {
            DemandType = demandType;
            Keyword = keyword ?? string.Empty;
            DisplayText = displayText ?? string.Empty;
            HintId = hintId;
        }
    }

    /// <summary>
    /// Dish 表读取与贵客需求匹配辅助。
    /// </summary>
    public static class DishConfigUtility
    {
        public static Dish Get(int id)
        {
            return LubanTablesRuntime.GetDish(id);
        }

        public static IReadOnlyList<Dish> GetAll()
        {
            return LubanTablesRuntime.GetDishList();
        }

        public static Dish GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var list = GetAll();
            for (var index = 0; index < list.Count; index++)
            {
                var dish = list[index];
                if (dish != null && dish.Name == name)
                {
                    return dish;
                }
            }

            return null;
        }

        public static bool MatchesDemand(Dish dish, VipGuestDemand demand)
        {
            if (dish == null || string.IsNullOrWhiteSpace(demand.Keyword))
            {
                return false;
            }

            if (demand.DemandType == VipGuestDemandType.Material)
            {
                return ContainsTag(dish.Materials, demand.Keyword);
            }

            return ContainsTag(dish.Flavor, demand.Keyword);
        }

        /// <summary>
        /// 收集至少存在 minDishCount 道菜且至少 minDistinctPrices 种不同价格的口味标签。
        /// </summary>
        public static List<string> CollectFlavorTagsForPriceChoice(int minDishCount = 3, int minDistinctPrices = 3)
        {
            var flavors = CollectFlavorTags();
            var eligible = new List<string>();
            for (var index = 0; index < flavors.Count; index++)
            {
                var flavor = flavors[index];
                var matching = GetMatchingDishes(new VipGuestDemand(VipGuestDemandType.Flavor, flavor, string.Empty, 0));
                if (matching.Count < minDishCount || CountDistinctPrices(matching) < minDistinctPrices)
                {
                    continue;
                }

                eligible.Add(flavor);
            }

            return eligible;
        }

        public static string BuildPanelPriceText(Dish dish)
        {
            if (dish == null)
            {
                return string.Empty;
            }

            return dish.Price > 0 ? $"{dish.Price} 铜钱" : string.Empty;
        }

        public static string BuildPanelDescription(Dish dish)
        {
            if (dish == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(dish.Summary))
            {
                return dish.Summary;
            }

            return $"材料：{JoinTags(dish.Materials)} · 口味：{JoinTags(dish.Flavor)}";
        }

        public static List<string> CollectMaterialTags()
        {
            return CollectTags(dish => dish?.Materials);
        }

        public static List<string> CollectFlavorTags()
        {
            return CollectTags(dish => dish?.Flavor);
        }

        public static List<Dish> GetMatchingDishes(VipGuestDemand demand)
        {
            var result = new List<Dish>();
            var list = GetAll();
            for (var index = 0; index < list.Count; index++)
            {
                var dish = list[index];
                if (MatchesDemand(dish, demand))
                {
                    result.Add(dish);
                }
            }

            return result;
        }

        public static List<Dish> GetNonMatchingDishes(VipGuestDemand demand)
        {
            var result = new List<Dish>();
            var list = GetAll();
            for (var index = 0; index < list.Count; index++)
            {
                var dish = list[index];
                if (dish != null && !MatchesDemand(dish, demand))
                {
                    result.Add(dish);
                }
            }

            return result;
        }

        private static List<string> CollectTags(Func<Dish, IReadOnlyList<string>> selector)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var list = GetAll();
            for (var index = 0; index < list.Count; index++)
            {
                var tags = selector(list[index]);
                if (tags == null)
                {
                    continue;
                }

                for (var tagIndex = 0; tagIndex < tags.Count; tagIndex++)
                {
                    var tag = tags[tagIndex];
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        set.Add(tag.Trim());
                    }
                }
            }

            return new List<string>(set);
        }

        private static int CountDistinctPrices(IReadOnlyList<Dish> dishes)
        {
            if (dishes == null || dishes.Count == 0)
            {
                return 0;
            }

            var prices = new HashSet<int>();
            for (var index = 0; index < dishes.Count; index++)
            {
                var dish = dishes[index];
                if (dish != null)
                {
                    prices.Add(dish.Price);
                }
            }

            return prices.Count;
        }

        private static bool ContainsTag(IReadOnlyList<string> tags, string keyword)
        {
            if (tags == null || string.IsNullOrWhiteSpace(keyword))
            {
                return false;
            }

            for (var index = 0; index < tags.Count; index++)
            {
                var tag = tags[index];
                if (!string.IsNullOrWhiteSpace(tag)
                    && string.Equals(tag.Trim(), keyword.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string JoinTags(IReadOnlyList<string> tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return "-";
            }

            return string.Join("、", tags);
        }
    }
}

