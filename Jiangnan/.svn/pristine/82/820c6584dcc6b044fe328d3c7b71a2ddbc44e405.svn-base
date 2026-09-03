using System;
using System.Collections.Generic;

namespace JN.Client.Model
{
    /// <summary>
    /// 单次打烊结算快照（用于历史对比）。
    /// </summary>
    [Serializable]
    public class ClosingSessionRecord
    {
        public long closedUtcTicks;
        public int income;
        public int spend;
        public int profit;
        public int servedCustomers;
        public int unpaidCheckouts;
        public float satisfactionScore;
        public string topDissatisfactionReason = string.Empty;
        public int topDissatisfactionCount;
    }

    /// <summary>
    /// 本营业日不满意原因计数。
    /// </summary>
    [Serializable]
    public class ClosingDissatisfactionEntry
    {
        public string reason = string.Empty;
        public int count;
    }

    /// <summary>
    /// 打烊结算面板单日收支摘要。
    /// </summary>
    public struct SettlementSummaryDaySnapshot
    {
        public string DayLabel;
        public int Income;
        public int Spend;
        public int Profit;
    }
}
