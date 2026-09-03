using System.Collections.Generic;
using cfg;
using JN.Client.Config;
using JN.Client.Model;
using JN.Client.UI;
using QFramework;
using UnityEngine;

namespace JN.Client.Manager
{
    public partial class DataManager
    {
        private void EnsureTavernAchievementDefaults()
        {
            EnsureTavernDefaults();
            SaveData.tavern.achievementStats ??= new TavernAchievementSaveData();
            SaveData.tavern.achievementStats.unlockedStaffTalentIds ??= new List<int>();
            SaveData.tavern.achievementStats.taskScopedProgress ??= new List<AchievementTaskScopedProgress>();
        }

        /// <summary>
        /// 是否已达成至少一个成就（用于解锁成就入口）。
        /// </summary>
        public bool HasAnyAchievementCompleted()
        {
            var all = AchievementConfigUtility.GetAllSorted();
            for (var index = 0; index < all.Count; index++)
            {
                var achievement = all[index];
                if (achievement != null && IsAchievementCompleted(achievement.Id))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAchievementClaimed(int achievementId)
        {
            if (achievementId <= 0)
            {
                return false;
            }

            EnsureGameplayDefaults();
            var list = SaveData.gameplay.claimedAchievementIds;
            return list != null && list.Contains(achievementId);
        }

        public int GetAchievementCurrentValue(AchievementType type)
        {
            EnsureTavernDefaults();
            EnsureGameplayDefaults();
            EnsureTavernAchievementDefaults();
            var stats = SaveData.tavern.achievementStats;
            return type switch
            {
                AchievementType.ServeCustomers => Mathf.Max(0, SaveData.tavern.totalServedCustomers),
                AchievementType.EarnIncome => Mathf.Max(0, SaveData.tavern.totalIncome),
                AchievementType.CookDishes => Mathf.Max(0, SaveData.tavern.totalCookedDishes),
                AchievementType.OpenBusiness => Mathf.Max(0, SaveData.gameplay.businessOpenCount),
                AchievementType.HireStaff => GetTotalHiredStaffCount(),
                AchievementType.CollectTalent => GetDistinctStaffTalentCount(),
                AchievementType.ExpandTavern => GetUnlockedTableCount(),
                AchievementType.ServeVip => Mathf.Max(0, stats.totalVipCheckout),
                AchievementType.ServeVipSuccess => Mathf.Max(0, stats.totalVipSuccessfulServe),
                AchievementType.VipSingleSpendReached => Mathf.Max(0, stats.peakVipSingleTableIncome),
                AchievementType.VipConcurrentCount => Mathf.Max(0, stats.peakVipConcurrentCount),
                AchievementType.VipWalkout => Mathf.Max(0, stats.totalVipNegativeWalkout),
                AchievementType.PerfectBusinessDay => Mathf.Max(0, stats.perfectBusinessDayCount),
                AchievementType.ServeCustomersOneDay => Mathf.Max(0, stats.peakSessionServedCustomers),
                AchievementType.QueueLengthReached => Mathf.Max(0, stats.peakQueueLength),
                AchievementType.PendingServeDishes => Mathf.Max(0, stats.peakPendingServeDishes),
                AchievementType.PendingCheckoutTables => Mathf.Max(0, stats.peakPendingCheckoutTables),
                AchievementType.DirtyTablePeak => Mathf.Max(0, stats.peakDirtyTables),
                AchievementType.SlowServeWalkout => Mathf.Max(0, stats.totalSlowServeWalkout),
                AchievementType.LongWaitWalkout => Mathf.Max(0, stats.totalLongWaitWalkout),
                AchievementType.ManualServiceActions => Mathf.Max(0, stats.totalManualServiceActions),
                AchievementType.AutoServiceDay => Mathf.Max(0, stats.autoServiceDayCount),
                AchievementType.NegativeProfitDay => Mathf.Max(0, stats.negativeProfitDayCount),
                AchievementType.CompleteAchievements => GetCompletedAchievementCount(excludeMeta: true),
                AchievementType.Expand => IsInteriorWallExpanded() ? 1 : 0,
                AchievementType.Stairs => IsStairsUnlocked() ? 1 : 0,
                _ => 0
            };
        }

        public int GetAchievementCurrentValue(int achievementId)
        {
            var achievement = AchievementConfigUtility.Get(achievementId);
            if (achievement == null)
            {
                return 0;
            }

            // 开业任务链按「条件进度」取值，不走通用 type 统计。
            return achievement.AchievementType switch
            {
                AchievementType.BuyFacility => GetBuyFacilityTaskProgress(),
                AchievementType.EmployFellow => GetEmployFellowTaskProgress(),
                AchievementType.UpgradeLevel => GetTavernLevel(),
                AchievementType.Expand => IsInteriorWallExpanded() ? 1 : 0,
                AchievementType.Stairs => IsStairsUnlocked() ? 1 : 0,
                AchievementType.TakeMoney => GetTaskScopedProgress(achievement.Id),
                AchievementType.KickEmployee => GetTaskScopedProgress(achievement.Id),
                AchievementType.Solicit => GetTaskScopedProgress(achievement.Id),
                _ => GetAchievementCurrentValue(achievement.AchievementType)
            };
        }

        public int GetAchievementTarget(int achievementId)
        {
            return AchievementConfigUtility.GetTarget(AchievementConfigUtility.Get(achievementId));
        }

        /// <summary>
        /// 成就表按 Id 顺序串行：前序未达成时，后续不可完成。
        /// </summary>
        public bool ArePreviousAchievementsCompleted(int achievementId)
        {
            if (achievementId <= 1)
            {
                return true;
            }

            var all = AchievementConfigUtility.GetAllSorted();
            for (var index = 0; index < all.Count; index++)
            {
                var previous = all[index];
                if (previous == null || previous.Id >= achievementId)
                {
                    continue;
                }

                if (!IsAchievementConditionMet(previous))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>当前条件是否达成（不计前序串行门闩）。</summary>
        public bool IsAchievementConditionMet(int achievementId)
        {
            return IsAchievementConditionMet(AchievementConfigUtility.Get(achievementId));
        }

        private bool IsAchievementConditionMet(Achievement achievement)
        {
            if (achievement == null)
            {
                return false;
            }

            return GetAchievementCurrentValue(achievement.Id) >= AchievementConfigUtility.GetTarget(achievement);
        }

        public bool IsAchievementCompleted(int achievementId)
        {
            var achievement = AchievementConfigUtility.Get(achievementId);
            if (achievement == null)
            {
                return false;
            }

            if (!ArePreviousAchievementsCompleted(achievementId))
            {
                return false;
            }

            return IsAchievementConditionMet(achievement);
        }

        /// <summary>
        /// 当前主线任务（成就表按 Id 第一个未完成项）。全部完成时返回 null。
        /// </summary>
        public Achievement GetCurrentAchievementTask()
        {
            var all = AchievementConfigUtility.GetAllSorted();
            for (var index = 0; index < all.Count; index++)
            {
                var achievement = all[index];
                if (achievement != null && !IsAchievementCompleted(achievement.Id))
                {
                    return achievement;
                }
            }

            return null;
        }

        public bool CanClaimAchievement(int achievementId)
        {
            return IsAchievementCompleted(achievementId) && !IsAchievementClaimed(achievementId);
        }

        public int GetDisplayedAchievementId()
        {
            EnsureGameplayDefaults();
            return Mathf.Max(0, SaveData.gameplay.displayedAchievementId);
        }

        public bool IsAchievementDisplayed(int achievementId)
        {
            return achievementId > 0 && GetDisplayedAchievementId() == achievementId;
        }

        /// <summary>
        /// 将已领取的成就设为城镇建筑展示项；同时只能展示一个。
        /// </summary>
        public bool TrySetDisplayedAchievement(int achievementId, out string message)
        {
            EnsureGameplayDefaults();
            message = string.Empty;
            var achievement = AchievementConfigUtility.Get(achievementId);
            if (achievement == null)
            {
                message = "成就不存在";
                return false;
            }

            if (!IsAchievementClaimed(achievementId))
            {
                message = "请先领取奖励";
                return false;
            }

            if (GetDisplayedAchievementId() == achievementId)
            {
                message = "已在城镇建筑展示";
                return true;
            }

            SaveData.gameplay.displayedAchievementId = achievementId;
            SaveGame();
            Signals.Get<AchievementProgressSignal>().Dispatch();
            message = $"已在城镇展示「{achievement.Name}」";
            return true;
        }

        public bool HasClaimableAchievement()
        {
            var entries = AchievementConfigUtility.GetCatalogDisplayAchievements(IsAchievementClaimed);
            for (var index = 0; index < entries.Count; index++)
            {
                var achievement = entries[index];
                if (achievement != null && CanClaimAchievement(achievement.Id))
                {
                    return true;
                }
            }

            return false;
        }

        public void RecordCookedDish(int count = 1)
        {
            if (count <= 0)
            {
                return;
            }

            EnsureTavernDefaults();
            SaveData.tavern.totalCookedDishes = Mathf.Max(0, SaveData.tavern.totalCookedDishes) + count;
            NotifyAchievementStatsChanged();
            SaveGame();
        }

        public void NotifyAchievementStatsChanged()
        {
            NotifyNewlyCompletedAchievements();
            Signals.Get<AchievementProgressSignal>().Dispatch();
        }

        private void EnsureAchievementCompletionToastMigration()
        {
            EnsureGameplayDefaults();
            if (SaveData.gameplay.achievementCompletionToastSeeded)
            {
                return;
            }

            SaveData.gameplay.achievementCompletionToastShownIds ??= new List<int>();
            var shown = SaveData.gameplay.achievementCompletionToastShownIds;
            var all = AchievementConfigUtility.GetAllSorted();
            for (var index = 0; index < all.Count; index++)
            {
                var achievement = all[index];
                if (achievement == null || !IsAchievementCompleted(achievement.Id))
                {
                    continue;
                }

                if (!shown.Contains(achievement.Id))
                {
                    shown.Add(achievement.Id);
                }
            }

            SaveData.gameplay.achievementCompletionToastSeeded = true;
            SaveGame();
        }

        private void NotifyNewlyCompletedAchievements()
        {
            EnsureGameplayDefaults();
            SaveData.gameplay.achievementCompletionToastShownIds ??= new List<int>();
            var shown = SaveData.gameplay.achievementCompletionToastShownIds;

            var all = AchievementConfigUtility.GetAllSorted();
            var changed = false;
            for (var index = 0; index < all.Count; index++)
            {
                var achievement = all[index];
                if (achievement == null || !IsAchievementCompleted(achievement.Id) || shown.Contains(achievement.Id))
                {
                    continue;
                }

                shown.Add(achievement.Id);
                changed = true;
                // 任务链：达成即自动领奖（声望/铜钱），便于后续 UpgradeLevel 等依赖声望的任务。
                if (CanClaimAchievement(achievement.Id))
                {
                    TryClaimAchievement(achievement.Id, out _);
                }
                // 酒馆内成就达成横幅暂时关闭，仅记录已提示 Id，避免之后恢复时重复弹出。
                // GameplaySuccessToastService.EnqueueAchievementCompleted(achievement.Id);
            }

            if (changed)
            {
                SaveGame();
                TavernFeatureUnlockPresenter.TryRevealAchievementEntry();
            }
        }

        /// <summary>
        /// BuyFacility：前台柜台 + 灶台 + 炉子 + 至少一张桌子（各算 1，目标为 4）。
        /// </summary>
        private int GetBuyFacilityTaskProgress()
        {
            EnsureGameplayDefaults();
            var counter = IsGuideBasicEquipmentPurchased(GuideBasicCounter) ? 1 : 0;
            var stove = IsGuideKitchenItemPurchased(GuideKitchenStove) || HasOwnedEquipment(StoveEquipmentId) ? 1 : 0;
            var furnace = IsGuideKitchenItemPurchased(GuideKitchenFurnace) ? 1 : 0;
            var table = GetUnlockedTableCount() > 0 ? 1 : 0;
            return counter + stove + furnace + table;
        }

        /// <summary>
        /// EmployFellow：掌柜、厨师、小二各至少 1 人（各算 1，目标通常为 3）。
        /// </summary>
        private int GetEmployFellowTaskProgress()
        {
            EnsureGameplayDefaults();
            var shopkeeper = CountHiredByPosition(StaffPosition.Shopkeeper) > 0
                             || HasHiredGuideStaff(ShopkeeperStaffId, StaffRole.Waiter)
                ? 1
                : 0;
            var chef = CountHiredByPosition(StaffPosition.Chef) > 0 ? 1 : 0;
            var waiter = CountHiredByPosition(StaffPosition.Waiter) > 0 ? 1 : 0;
            return shopkeeper + chef + waiter;
        }

        /// <summary>
        /// 读取「任务开始后累计」进度（按成就 Id）。
        /// </summary>
        private int GetTaskScopedProgress(int achievementId)
        {
            if (achievementId <= 0)
            {
                return 0;
            }

            EnsureTavernAchievementDefaults();
            var list = AchievementStats.taskScopedProgress;
            for (var index = 0; index < list.Count; index++)
            {
                var entry = list[index];
                if (entry != null && entry.achievementId == achievementId)
                {
                    return Mathf.Max(0, entry.progress);
                }
            }

            return 0;
        }

        /// <summary>
        /// 仅当指定类型是「当前主线任务」时累加进度（任务开始后才计数）。
        /// </summary>
        private void TryIncrementCurrentTaskScopedProgress(AchievementType type, int delta = 1)
        {
            if (delta <= 0)
            {
                return;
            }

            // 结账/踢人/拉客：拜访他人酒楼时不计（Solicit 在回店卸客时累计）。
            if (IsVisitingOtherTavern)
            {
                return;
            }

            var task = GetCurrentAchievementTask();
            if (task == null || task.AchievementType != type)
            {
                return;
            }

            EnsureTavernAchievementDefaults();
            var list = AchievementStats.taskScopedProgress;
            AchievementTaskScopedProgress entry = null;
            for (var index = 0; index < list.Count; index++)
            {
                if (list[index] != null && list[index].achievementId == task.Id)
                {
                    entry = list[index];
                    break;
                }
            }

            if (entry == null)
            {
                entry = new AchievementTaskScopedProgress { achievementId = task.Id, progress = 0 };
                list.Add(entry);
            }

            entry.progress = Mathf.Max(0, entry.progress) + delta;
            NotifyAchievementStatsChanged();
            SaveGame();
        }

        /// <summary>结账收钱 1 次（TakeMoney）。</summary>
        public void RecordTakeMoneyCheckout()
        {
            TryIncrementCurrentTaskScopedProgress(AchievementType.TakeMoney);
        }

        /// <summary>踢醒偷懒小二 1 次（KickEmployee）。</summary>
        public void RecordKickEmployee()
        {
            TryIncrementCurrentTaskScopedProgress(AchievementType.KickEmployee);
        }

        /// <summary>回店卸客进队成功 1 次（Solicit）。</summary>
        public void RecordSolicitSuccess()
        {
            TryIncrementCurrentTaskScopedProgress(AchievementType.Solicit);
        }

        public bool TryClaimAchievement(int achievementId, out string message)
        {
            EnsureGameplayDefaults();
            message = string.Empty;
            var achievement = AchievementConfigUtility.Get(achievementId);
            if (achievement == null)
            {
                message = "成就不存在";
                return false;
            }

            if (IsAchievementClaimed(achievementId))
            {
                message = "奖励已领取";
                return false;
            }

            if (!IsAchievementCompleted(achievementId))
            {
                message = "尚未达成";
                return false;
            }

            SaveData.gameplay.claimedAchievementIds ??= new List<int>();
            SaveData.gameplay.claimedAchievementIds.Add(achievementId);

            var reward = Mathf.Max(0, achievement.RewardCoin);
            if (reward > 0 && PlayerData != null)
            {
                var before = PlayerData.coinNum;
                PlayerData.coinNum = before + reward;
                Debug.Log($"<color={CoinLogColor}>[Coin Change] Achievement#{achievementId} +{reward} Before={before} After={PlayerData.coinNum}</color>");
                Signals.Get<UpdateCoinNumSignal>().Dispatch(reward);
            }

            var prestigeReward = Mathf.Max(0, achievement.ReputationReward);
            if (prestigeReward > 0)
            {
                AddTavernPrestige(prestigeReward);
            }

            SaveGame();
            Signals.Get<AchievementProgressSignal>().Dispatch();
            if (reward > 0 && prestigeReward > 0)
            {
                message = $"已领取 {reward} 铜钱、{prestigeReward} 声望";
            }
            else if (reward > 0)
            {
                message = $"已领取 {reward} 铜钱";
            }
            else if (prestigeReward > 0)
            {
                message = $"已领取 {prestigeReward} 声望";
            }
            else
            {
                message = "已领取";
            }

            return true;
        }

        private int GetTotalHiredStaffCount()
        {
            EnsureGameplayDefaults();
            var owned = SaveData.gameplay.ownedStaff;
            if (owned == null || owned.Count == 0)
            {
                return 0;
            }

            var count = 0;
            for (var index = 0; index < owned.Count; index++)
            {
                var staff = owned[index];
                if (staff != null && !staff.temporary)
                {
                    count++;
                }
            }

            return count;
        }

        private int GetCompletedAchievementCount(bool excludeMeta)
        {
            var all = AchievementConfigUtility.GetAllSorted();
            var count = 0;
            for (var index = 0; index < all.Count; index++)
            {
                var achievement = all[index];
                if (achievement == null)
                {
                    continue;
                }

                if (excludeMeta && achievement.AchievementType == AchievementType.CompleteAchievements)
                {
                    continue;
                }

                if (IsAchievementCompleted(achievement.Id))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
