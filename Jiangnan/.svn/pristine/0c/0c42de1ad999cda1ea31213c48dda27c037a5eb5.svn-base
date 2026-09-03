using System.Collections.Generic;
using cfg;
using JN.Client.Config;
using JN.Client.Model;
using JN.Client.Scene;
using UnityEngine;

namespace JN.Client.Manager
{
    public partial class DataManager
    {
        private const float DefaultCustomerWaitQueueGraceSeconds = 5f;
        private const float DefaultCustomerWaitQueueBubbleSeconds = 30f;
        private const float DefaultCustomerWaitOrderGraceSeconds = 10f;
        private const float DefaultCustomerWaitOrderBubbleSeconds = 20f;
        private const float DefaultCustomerWaitServeGraceSeconds = 10f;
        private const float DefaultCustomerWaitServeBubbleSeconds = 20f;
        private const float DefaultCustomerWaitCheckoutGraceSeconds = 10f;
        private const float DefaultCustomerWaitCheckoutBubbleSeconds = 25f;

        private TavernAchievementSaveData AchievementStats
        {
            get
            {
                EnsureTavernDefaults();
                SaveData.tavern.achievementStats ??= new TavernAchievementSaveData();
                return SaveData.tavern.achievementStats;
            }
        }

        public void RecordStaffTalentUnlocked(int talentId)
        {
            if (talentId <= 0)
            {
                return;
            }

            EnsureTavernDefaults();
            var stats = AchievementStats;
            stats.unlockedStaffTalentIds ??= new List<int>();
            if (stats.unlockedStaffTalentIds.Contains(talentId))
            {
                return;
            }

            stats.unlockedStaffTalentIds.Add(talentId);
            NotifyAchievementStatsChanged();
            SaveGame();
        }

        public int GetDistinctStaffTalentCount()
        {
            EnsureTavernDefaults();
            var list = AchievementStats.unlockedStaffTalentIds;
            return list == null ? 0 : list.Count;
        }

        public void ResetAchievementSessionStats()
        {
            EnsureGameplayDefaults();
            var gameplay = SaveData.gameplay;
            gameplay.sessionHadManualDispatch = false;
            gameplay.sessionPerfectDayViolated = false;
            gameplay.sessionPeakQueueLength = 0;
            gameplay.sessionPeakPendingServeDishes = 0;
            gameplay.sessionPeakPendingCheckoutTables = 0;
            gameplay.sessionPeakDirtyTables = 0;
        }

        public void RecordManualServiceDispatch(bool playerDirected)
        {
            if (!playerDirected)
            {
                return;
            }

            EnsureGameplayDefaults();
            SaveData.gameplay.sessionHadManualDispatch = true;
            AchievementStats.totalManualServiceActions = Mathf.Max(0, AchievementStats.totalManualServiceActions) + 1;
            NotifyAchievementStatsChanged();
        }

        public void UpdateAchievementPeakSamples(
            int queueLength,
            int pendingServeDishes,
            int pendingCheckoutTables,
            int dirtyTables,
            int vipConcurrent)
        {
            EnsureGameplayDefaults();
            EnsureTavernDefaults();
            var gameplay = SaveData.gameplay;
            var stats = AchievementStats;

            gameplay.sessionPeakQueueLength = Mathf.Max(gameplay.sessionPeakQueueLength, queueLength);
            gameplay.sessionPeakPendingServeDishes = Mathf.Max(gameplay.sessionPeakPendingServeDishes, pendingServeDishes);
            gameplay.sessionPeakPendingCheckoutTables = Mathf.Max(gameplay.sessionPeakPendingCheckoutTables, pendingCheckoutTables);
            gameplay.sessionPeakDirtyTables = Mathf.Max(gameplay.sessionPeakDirtyTables, dirtyTables);

            stats.peakQueueLength = Mathf.Max(stats.peakQueueLength, queueLength);
            stats.peakPendingServeDishes = Mathf.Max(stats.peakPendingServeDishes, pendingServeDishes);
            stats.peakPendingCheckoutTables = Mathf.Max(stats.peakPendingCheckoutTables, pendingCheckoutTables);
            stats.peakDirtyTables = Mathf.Max(stats.peakDirtyTables, dirtyTables);
            stats.peakVipConcurrentCount = Mathf.Max(stats.peakVipConcurrentCount, vipConcurrent);
        }

        public void RecordVipCheckout(int tableIncome)
        {
            EnsureTavernDefaults();
            var stats = AchievementStats;
            stats.totalVipCheckout = Mathf.Max(0, stats.totalVipCheckout) + 1;
            // 无猜菜流程：贵客结账即视为成功服务。
            stats.totalVipSuccessfulServe = Mathf.Max(0, stats.totalVipSuccessfulServe) + 1;

            if (tableIncome > stats.peakVipSingleTableIncome)
            {
                stats.peakVipSingleTableIncome = tableIncome;
            }

            NotifyAchievementStatsChanged();
            SaveGame();
        }

        public void RecordCustomerWalkout(CustomerWalkoutReason reason, bool isVip)
        {
            if (reason == CustomerWalkoutReason.None)
            {
                return;
            }

            EnsureGameplayDefaults();
            SaveData.gameplay.sessionPerfectDayViolated = true;

            RecordClosingSessionWaitWalkout(reason);

            EnsureTavernDefaults();
            var stats = AchievementStats;
            switch (reason)
            {
                case CustomerWalkoutReason.QueueTooLong:
                    stats.totalLongWaitWalkout = Mathf.Max(0, stats.totalLongWaitWalkout) + 1;
                    break;
                case CustomerWalkoutReason.ServeTooSlow:
                    stats.totalSlowServeWalkout = Mathf.Max(0, stats.totalSlowServeWalkout) + 1;
                    break;
            }

            if (isVip && reason != CustomerWalkoutReason.None)
            {
                stats.totalVipNegativeWalkout = Mathf.Max(0, stats.totalVipNegativeWalkout) + 1;
            }

            NotifyAchievementStatsChanged();
            SaveGame();
        }

        public void EvaluateAchievementDayEnd(ClosingSessionRecord record)
        {
            if (record == null)
            {
                return;
            }

            EnsureGameplayDefaults();
            EnsureTavernDefaults();
            var gameplay = SaveData.gameplay;
            var stats = AchievementStats;

            if (record.servedCustomers > 0)
            {
                stats.peakSessionServedCustomers = Mathf.Max(stats.peakSessionServedCustomers, record.servedCustomers);
            }

            if (record.profit < 0)
            {
                stats.negativeProfitDayCount = Mathf.Max(0, stats.negativeProfitDayCount) + 1;
            }

            if (!gameplay.sessionHadManualDispatch && record.servedCustomers > 0)
            {
                stats.autoServiceDayCount = Mathf.Max(0, stats.autoServiceDayCount) + 1;
            }

            if (!gameplay.sessionPerfectDayViolated && record.servedCustomers > 0)
            {
                stats.perfectBusinessDayCount = Mathf.Max(0, stats.perfectBusinessDayCount) + 1;
            }

            NotifyAchievementStatsChanged();
            SaveGame();
        }

        public static float GetCustomerWaitGraceSeconds(CustomerWaitHudState state)
        {
            return state switch
            {
                CustomerWaitHudState.Queue => TbConfigRuntime.GetCustomerWaitQueueGraceTime(DefaultCustomerWaitQueueGraceSeconds),
                CustomerWaitHudState.WaitingOrder => TbConfigRuntime.GetCustomerWaitOrderGraceTime(DefaultCustomerWaitOrderGraceSeconds),
                CustomerWaitHudState.WaitingServe => TbConfigRuntime.GetCustomerWaitServeGraceTime(DefaultCustomerWaitServeGraceSeconds),
                CustomerWaitHudState.WaitingCheckout => TbConfigRuntime.GetCustomerWaitCheckoutGraceTime(DefaultCustomerWaitCheckoutGraceSeconds),
                _ => 0f,
            };
        }

        public static float GetCustomerWaitBubbleSeconds(CustomerWaitHudState state)
        {
            return state switch
            {
                CustomerWaitHudState.Queue => TbConfigRuntime.GetCustomerWaitQueueBubbleTime(DefaultCustomerWaitQueueBubbleSeconds),
                CustomerWaitHudState.WaitingOrder => TbConfigRuntime.GetCustomerWaitOrderBubbleTime(DefaultCustomerWaitOrderBubbleSeconds),
                CustomerWaitHudState.WaitingServe => TbConfigRuntime.GetCustomerWaitServeBubbleTime(DefaultCustomerWaitServeBubbleSeconds),
                CustomerWaitHudState.WaitingCheckout => TbConfigRuntime.GetCustomerWaitCheckoutBubbleTime(DefaultCustomerWaitCheckoutBubbleSeconds),
                _ => 0.1f,
            };
        }

        public static float GetWalkoutQueueWaitSeconds()
            => GetCustomerWaitGraceSeconds(CustomerWaitHudState.Queue) + GetCustomerWaitBubbleSeconds(CustomerWaitHudState.Queue);

        public static float GetWalkoutOrderWaitSeconds()
            => GetCustomerWaitGraceSeconds(CustomerWaitHudState.WaitingOrder) + GetCustomerWaitBubbleSeconds(CustomerWaitHudState.WaitingOrder);

        public static float GetWalkoutServeWaitSeconds()
            => GetCustomerWaitGraceSeconds(CustomerWaitHudState.WaitingServe) + GetCustomerWaitBubbleSeconds(CustomerWaitHudState.WaitingServe);

        public static float GetWalkoutCheckoutWaitSeconds()
            => GetCustomerWaitGraceSeconds(CustomerWaitHudState.WaitingCheckout) + GetCustomerWaitBubbleSeconds(CustomerWaitHudState.WaitingCheckout);
    }
}
