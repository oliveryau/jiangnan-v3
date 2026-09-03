using System.Collections.Generic;
using cfg;
using JN.Client.Config;
using UnityEngine;

namespace JN.Client.Scene
{
    public enum VipGuestGuessOutcome
    {
        None = 0,
        /// <summary>需求匹配。</summary>
        Satisfied = 1,
        /// <summary>需求不符但选了最贵。</summary>
        PremiumMismatch = 2,
        /// <summary>未满足需求。</summary>
        Disappointed = 3
    }

    /// <summary>
    /// 单桌贵客猜菜会话状态（预留场景接入）。
    /// </summary>
    public sealed class VipGuestDishGuessSession
    {
        public int TableId { get; internal set; }
        public VipGuestDemand Demand { get; internal set; }
        public List<Dish> Options { get; } = new(3);
        public int SelectedIndex { get; internal set; } = -1;
        public bool HasAnswered { get; internal set; }
        public bool IsCorrect { get; internal set; }
        public VipGuestGuessOutcome Outcome { get; internal set; } = VipGuestGuessOutcome.None;

        public Dish SelectedDish =>
            SelectedIndex >= 0 && SelectedIndex < Options.Count ? Options[SelectedIndex] : null;
    }

    /// <summary>
    /// 贵客猜菜：随机需求、三选一候选与判定。
    /// </summary>
    public static class VipGuestDishGuessService
    {
        public const float PlayerOrderBubbleDurationSeconds = 8f;

        private const float MaterialDemandChance = 0.20f;
        private const float FlavorDemandChance = 0.60f;
        // 剩余 20% 为同口味不同价三选一（三道均满足口味，任选均对）。

        private enum VipTableOrderInteractionPhase
        {
            None = 0,
            AwaitingPlayerClick = 1,
            PanelOpen = 2,
            ReadyToDispatch = 3
        }

        private static readonly Dictionary<int, VipGuestDishGuessSession> TableSessions = new();
        private static readonly Dictionary<int, VipTableOrderInteractionPhase> OrderInteractionPhases = new();

        public static bool CanOpen(int tableId)
        {
            if (tableId <= 0)
            {
                return true;
            }

            return !TableSessions.TryGetValue(tableId, out var session) || !session.HasAnswered;
        }

        public static VipGuestDishGuessSession GetOrCreateSession(int tableId, bool forceRegenerate = false)
        {
            if (tableId > 0
                && !forceRegenerate
                && TableSessions.TryGetValue(tableId, out var existing)
                && existing != null
                && (existing.Options.Count > 0 || existing.HasAnswered))
            {
                return existing;
            }

            var session = new VipGuestDishGuessSession
            {
                TableId = tableId,
                Demand = RollDemand(),
                SelectedIndex = 0,
                HasAnswered = false,
                IsCorrect = false,
                Outcome = VipGuestGuessOutcome.None
            };
            session.Options.Clear();
            session.Options.AddRange(RollOptions(session.Demand));

            if (tableId > 0)
            {
                TableSessions[tableId] = session;
            }

            return session;
        }

        public static bool TryConfirm(VipGuestDishGuessSession session)
        {
            if (session == null || session.HasAnswered || session.Options.Count <= 0)
            {
                return false;
            }

            session.SelectedIndex = Mathf.Clamp(session.SelectedIndex, 0, session.Options.Count - 1);
            var selected = session.SelectedDish;
            session.IsCorrect = selected != null && DishConfigUtility.MatchesDemand(selected, session.Demand);
            session.Outcome = ResolveGuessOutcome(session, selected);
            session.HasAnswered = true;

            if (session.TableId > 0)
            {
                TableSessions[session.TableId] = session;
            }

            return true;
        }

        private static VipGuestGuessOutcome ResolveGuessOutcome(VipGuestDishGuessSession session, Dish selected)
        {
            if (selected == null)
            {
                return VipGuestGuessOutcome.Disappointed;
            }

            if (DishConfigUtility.MatchesDemand(selected, session.Demand))
            {
                return VipGuestGuessOutcome.Satisfied;
            }

            if (IsHighestPricedOption(session.Options, selected))
            {
                return VipGuestGuessOutcome.PremiumMismatch;
            }

            return VipGuestGuessOutcome.Disappointed;
        }

        private static bool IsHighestPricedOption(IReadOnlyList<Dish> options, Dish selected)
        {
            if (options == null || options.Count == 0 || selected == null)
            {
                return false;
            }

            var maxPrice = int.MinValue;
            for (var index = 0; index < options.Count; index++)
            {
                var option = options[index];
                if (option == null)
                {
                    continue;
                }

                maxPrice = Mathf.Max(maxPrice, option.Price);
            }

            return maxPrice > int.MinValue && selected.Price >= maxPrice;
        }

        public static void ClearTableSession(int tableId)
        {
            if (tableId <= 0)
            {
                return;
            }

            TableSessions.Remove(tableId);
            ClearOrderInteraction(tableId);
        }

        /// <summary>
        /// 贵客桌进入待点单：开启可点击气泡窗口（默认 8s，超时后走自动派单）。
        /// </summary>
        public static void BeginWaitingOrderInteraction(int tableId)
        {
            if (tableId <= 0)
            {
                return;
            }

            OrderInteractionPhases[tableId] = VipTableOrderInteractionPhase.AwaitingPlayerClick;
        }

        /// <summary>
        /// 是否仍需要玩家点击点单气泡以打开猜菜面板。
        /// </summary>
        public static bool RequiresPlayerOrderClick(int tableId)
        {
            return tableId > 0
                   && OrderInteractionPhases.TryGetValue(tableId, out var phase)
                   && phase == VipTableOrderInteractionPhase.AwaitingPlayerClick;
        }

        /// <summary>
        /// 猜菜面板已关闭或气泡超时后，允许常规小二自动点单。
        /// </summary>
        public static bool ShouldAutoDispatchOrder(int tableId)
        {
            return tableId > 0
                   && OrderInteractionPhases.TryGetValue(tableId, out var phase)
                   && phase == VipTableOrderInteractionPhase.ReadyToDispatch;
        }

        public static void NotifyOrderPanelOpened(int tableId)
        {
            if (tableId <= 0)
            {
                return;
            }

            OrderInteractionPhases[tableId] = VipTableOrderInteractionPhase.PanelOpen;
        }

        public static void NotifyOrderPanelClosed(int tableId)
        {
            if (tableId <= 0)
            {
                return;
            }

            OrderInteractionPhases[tableId] = VipTableOrderInteractionPhase.ReadyToDispatch;
        }

        public static void NotifyOrderInteractionTimedOut(int tableId)
        {
            NotifyOrderPanelClosed(tableId);
        }

        public static void ClearOrderInteraction(int tableId)
        {
            if (tableId <= 0)
            {
                return;
            }

            OrderInteractionPhases.Remove(tableId);
        }

        public static VipGuestDemand RollDemand()
        {
            for (var attempt = 0; attempt < 24; attempt++)
            {
                var demandType = RollDemandType();
                var demand = demandType switch
                {
                    VipGuestDemandType.Material => TryRollTaggedDemand(VipGuestDemandType.Material),
                    VipGuestDemandType.Flavor => TryRollTaggedDemand(VipGuestDemandType.Flavor),
                    _ => TryRollFlavorPriceDemand()
                };
                if (demand.HasValue)
                {
                    return demand.Value;
                }
            }

            return TryRollTaggedDemand(VipGuestDemandType.Flavor)
                   ?? new VipGuestDemand(VipGuestDemandType.Flavor, "辣", "最近馋辣口的了", 0);
        }

        public static List<Dish> RollOptions(VipGuestDemand demand, int count = 3)
        {
            if (demand.DemandType == VipGuestDemandType.FlavorPriceChoice)
            {
                return RollFlavorPriceOptions(demand, count);
            }

            return RollSingleCorrectOptions(demand, count);
        }

        public static bool IsCorrect(int selectedDishId, VipGuestDemand demand)
        {
            var dish = DishConfigUtility.Get(selectedDishId);
            return DishConfigUtility.MatchesDemand(dish, demand);
        }

        private static VipGuestDemandType RollDemandType()
        {
            var roll = Random.value;
            if (roll < MaterialDemandChance)
            {
                return VipGuestDemandType.Material;
            }

            if (roll < MaterialDemandChance + FlavorDemandChance)
            {
                return VipGuestDemandType.Flavor;
            }

            return VipGuestDemandType.FlavorPriceChoice;
        }

        private static VipGuestDemand? TryRollTaggedDemand(VipGuestDemandType demandType)
        {
            var pool = demandType == VipGuestDemandType.Material
                ? DishConfigUtility.CollectMaterialTags()
                : DishConfigUtility.CollectFlavorTags();
            if (pool.Count == 0)
            {
                return null;
            }

            var keyword = pool[Random.Range(0, pool.Count)];
            if (DishConfigUtility.GetMatchingDishes(new VipGuestDemand(demandType, keyword, string.Empty, 0)).Count == 0)
            {
                return null;
            }

            var hint = VipGuestDemandHintConfigUtility.GetRandom();
            var template = hint != null && !string.IsNullOrWhiteSpace(hint.Text)
                ? hint.Text
                : demandType == VipGuestDemandType.Material
                    ? "最近馋{0}菜了"
                    : "最近馋{0}口的了";
            var displayText = string.Format(template, keyword);
            return new VipGuestDemand(demandType, keyword, displayText, hint?.Id ?? 0);
        }

        private static VipGuestDemand? TryRollFlavorPriceDemand()
        {
            var pool = DishConfigUtility.CollectFlavorTagsForPriceChoice();
            if (pool.Count == 0)
            {
                return null;
            }

            var keyword = pool[Random.Range(0, pool.Count)];
            var matching = DishConfigUtility.GetMatchingDishes(
                new VipGuestDemand(VipGuestDemandType.FlavorPriceChoice, keyword, string.Empty, 0));
            if (PickDishesWithDistinctPrices(matching, 3).Count < 3)
            {
                return null;
            }

            var hint = VipGuestDemandHintConfigUtility.GetRandom();
            var template = hint != null && !string.IsNullOrWhiteSpace(hint.Text)
                ? hint.Text
                : "想吃{0}口的，这几道价位不同，任选都行";
            var displayText = string.Format(template, keyword);
            return new VipGuestDemand(VipGuestDemandType.FlavorPriceChoice, keyword, displayText, hint?.Id ?? 0);
        }

        private static List<Dish> RollSingleCorrectOptions(VipGuestDemand demand, int count)
        {
            var result = new List<Dish>(count);
            var allDishes = DishConfigUtility.GetAll();
            if (allDishes == null || allDishes.Count == 0)
            {
                return result;
            }

            var matching = DishConfigUtility.GetMatchingDishes(demand);
            if (matching.Count > 0)
            {
                result.Add(matching[Random.Range(0, matching.Count)]);
            }

            var fillerPool = BuildFillerPool(allDishes, result, demand);
            while (result.Count < count && fillerPool.Count > 0)
            {
                var pickIndex = Random.Range(0, fillerPool.Count);
                result.Add(fillerPool[pickIndex]);
                fillerPool.RemoveAt(pickIndex);
            }

            ShuffleInPlace(result);
            return result;
        }

        private static List<Dish> RollFlavorPriceOptions(VipGuestDemand demand, int count)
        {
            var matching = DishConfigUtility.GetMatchingDishes(demand);
            var result = PickDishesWithDistinctPrices(matching, count);
            if (result.Count < count)
            {
                return RollSingleCorrectOptions(
                    new VipGuestDemand(VipGuestDemandType.Flavor, demand.Keyword, demand.DisplayText, demand.HintId),
                    count);
            }

            ShuffleInPlace(result);
            return result;
        }

        private static List<Dish> PickDishesWithDistinctPrices(IReadOnlyList<Dish> source, int count)
        {
            var result = new List<Dish>(count);
            if (source == null || source.Count == 0 || count <= 0)
            {
                return result;
            }

            var pool = new List<Dish>();
            for (var index = 0; index < source.Count; index++)
            {
                if (source[index] != null)
                {
                    pool.Add(source[index]);
                }
            }

            while (result.Count < count && pool.Count > 0)
            {
                var pickIndex = Random.Range(0, pool.Count);
                var candidate = pool[pickIndex];
                pool.RemoveAt(pickIndex);
                if (ContainsDish(result, candidate.Id) || ContainsPrice(result, candidate.Price))
                {
                    continue;
                }

                result.Add(candidate);
            }

            return result;
        }

        private static bool ContainsPrice(List<Dish> dishes, int price)
        {
            for (var index = 0; index < dishes.Count; index++)
            {
                if (dishes[index] != null && dishes[index].Price == price)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<Dish> BuildFillerPool(
            IReadOnlyList<Dish> allDishes,
            List<Dish> selected,
            VipGuestDemand demand)
        {
            var pool = new List<Dish>();
            for (var index = 0; index < allDishes.Count; index++)
            {
                var dish = allDishes[index];
                if (dish == null || ContainsDish(selected, dish.Id))
                {
                    continue;
                }

                if (!DishConfigUtility.MatchesDemand(dish, demand))
                {
                    pool.Add(dish);
                }
            }

            if (pool.Count == 0)
            {
                for (var index = 0; index < allDishes.Count; index++)
                {
                    var dish = allDishes[index];
                    if (dish != null && !ContainsDish(selected, dish.Id))
                    {
                        pool.Add(dish);
                    }
                }
            }

            return pool;
        }

        private static bool ContainsDish(List<Dish> dishes, int dishId)
        {
            for (var index = 0; index < dishes.Count; index++)
            {
                if (dishes[index] != null && dishes[index].Id == dishId)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ShuffleInPlace(List<Dish> list)
        {
            for (var index = list.Count - 1; index > 0; index--)
            {
                var swap = Random.Range(0, index + 1);
                (list[index], list[swap]) = (list[swap], list[index]);
            }
        }
    }
}
