using System;
using System.Collections.Generic;

namespace JN.Client.Model
{
    /// <summary>
    /// 成就相关生涯统计（存于 TavernSaveData）。
    /// </summary>
    [Serializable]
    public class TavernAchievementSaveData
    {
        public int totalVipCheckout;
        public int totalVipSuccessfulServe;
        public int peakVipSingleTableIncome;
        public int peakVipConcurrentCount;
        public int totalVipNegativeWalkout;
        public int peakQueueLength;
        public int peakPendingServeDishes;
        public int peakPendingCheckoutTables;
        public int peakDirtyTables;
        public int totalSlowServeWalkout;
        public int totalLongWaitWalkout;
        public int totalManualServiceActions;
        public int perfectBusinessDayCount;
        public int peakSessionServedCustomers;
        public int autoServiceDayCount;
        public int negativeProfitDayCount;
        public List<int> unlockedStaffTalentIds = new();

        /// <summary>
        /// 任务开始后累计的进度（按成就 Id；用于 TakeMoney / KickEmployee / Solicit）。
        /// </summary>
        public List<AchievementTaskScopedProgress> taskScopedProgress = new();
    }

    /// <summary>
    /// 单条成就任务自「成为当前任务」后的累计进度。
    /// </summary>
    [Serializable]
    public class AchievementTaskScopedProgress
    {
        public int achievementId;
        public int progress;
    }
}
