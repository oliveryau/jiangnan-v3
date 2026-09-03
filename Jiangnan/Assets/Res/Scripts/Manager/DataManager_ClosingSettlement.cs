using System;
using System.Collections.Generic;
using System.Text;
using JN.Client.Config;
using JN.Client.Messages;
using JN.Client.Model;
using JN.Client.Scene;
using QFramework;
using UnityEngine;

namespace JN.Client.Manager
{
    public partial class DataManager
    {
        public const string DissatisfactionQueueWait = "排队等待过长";
        public const string DissatisfactionOrderWait = "点单等待过长";
        public const string DissatisfactionServeWait = "上菜等待过长";
        public const string DissatisfactionCheckoutWait = "结账等待过长";

        private const int MaxRecentClosings = 5;

        // 软阈值：超过开始扣分；硬阈值：记为主要不满原因（整体偏宽松，结账尤甚）
        private const float SoftQueueWait = 24f;
        private const float HardQueueWait = 55f;
        private const float SoftOrderWaitMul = 3.5f;
        private const float HardOrderWaitMul = 7f;
        private const float SoftServeWaitMul = 2.5f;
        private const float HardServeWaitMul = 6f;
        private const float SoftCheckoutWaitMul = 4.5f;
        private const float HardCheckoutWaitMul = 10f;
        /// <summary>满意度尚可且抱怨人数较少时，结算详情用中性反馈替代单点抱怨。</summary>
        private const float LenientFeedbackSatisfactionFloor = 78f;
        private const int LenientFeedbackMaxTopCount = 2;

        /// <summary>本次启动后、进自家店起算的营业轮次显示（不落盘）。</summary>
        private int sessionBusinessTurn;

        /// <summary>本次启动是否已在自家店开启轮次计数。</summary>
        private bool sessionOwnTavernTurnStarted;

        /// <summary>本次启动首次进自家店后，倒计时需从完整一轮重新开始。</summary>
        private bool sessionBusinessCountdownNeedsFreshStart;

        /// <summary>
        /// 累计开业次数。
        /// </summary>
        public int GetBusinessOpenCount()
        {
            EnsureGameplayDefaults();
            return Mathf.Max(0, SaveData.gameplay.businessOpenCount);
        }

        /// <summary>
        /// 当前会话显示用轮次（进自家店后从第 1 轮起；续轮递增）。
        /// </summary>
        public int GetSessionBusinessTurn()
        {
            return sessionOwnTavernTurnStarted ? Mathf.Max(1, sessionBusinessTurn) : 0;
        }

        /// <summary>
        /// 峰谷等 1~5 轮配置用的当前轮次：优先会话「第N轮」，否则回退累计开业次数。
        /// </summary>
        public int GetBusinessCycleRoundForConfig()
        {
            var sessionTurn = GetSessionBusinessTurn();
            if (sessionTurn > 0)
            {
                return sessionTurn;
            }

            return Mathf.Max(1, GetBusinessOpenCount());
        }

        /// <summary>
        /// 本次启动第一次进自家店：从第 1 轮开始。
        /// </summary>
        public void NotifyEnteredOwnTavernSessionTurn()
        {
            if (IsVisitingOtherTavern)
            {
                return;
            }

            if (sessionOwnTavernTurnStarted)
            {
                return;
            }

            sessionOwnTavernTurnStarted = true;
            sessionBusinessTurn = 1;
            sessionBusinessCountdownNeedsFreshStart = true;
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
        }

        /// <summary>
        /// 消费「本会话首次进店需整轮倒计时」标记（进店场景初始化时调用一次）。
        /// </summary>
        public bool ConsumeSessionBusinessCountdownFreshStart()
        {
            if (!sessionBusinessCountdownNeedsFreshStart)
            {
                return false;
            }

            sessionBusinessCountdownNeedsFreshStart = false;
            return true;
        }

        /// <summary>
        /// 三分钟续轮等：会话轮次 +1（须已开启会话轮次）。
        /// </summary>
        public void AdvanceSessionBusinessTurn()
        {
            if (IsVisitingOtherTavern)
            {
                return;
            }

            if (!sessionOwnTavernTurnStarted)
            {
                sessionOwnTavernTurnStarted = true;
                sessionBusinessTurn = 1;
                sessionBusinessCountdownNeedsFreshStart = true;
            }
            else
            {
                sessionBusinessTurn = Mathf.Max(1, sessionBusinessTurn) + 1;
            }

            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
        }

        /// <summary>
        /// 是否仍处于首次开业前的新手引导阶段。
        /// </summary>
        public bool IsOnboardingGuideActive()
        {
            return GetBusinessOpenCount() < 1;
        }

        /// <summary>
        /// 员工底栏入口是否已解锁显示（店内常驻）。
        /// </summary>
        public bool IsStaffTopEntryUnlocked()
        {
            return true;
        }

        /// <summary>
        /// 开业时清空本营业日会话统计（收支 pending 由 SetTavernOpen 清零）。
        /// </summary>
        public void ResetClosingSessionStats()
        {
            EnsureGameplayDefaults();
            var gameplay = SaveData.gameplay;
            gameplay.sessionServedCustomers = 0;
            gameplay.sessionUnpaidCheckouts = 0;
            gameplay.sessionSatisfactionSum = 0f;
            gameplay.sessionSatisfactionSamples = 0;
            gameplay.sessionWaitWalkoutCustomers = 0;
            gameplay.sessionDissatisfactionReasons ??= new List<ClosingDissatisfactionEntry>();
            gameplay.sessionDissatisfactionReasons.Clear();
            gameplay.recentClosings ??= new List<ClosingSessionRecord>();
            ResetAchievementSessionStats();
        }

        /// <summary>
        /// 按各阶段等待时长评估本桌顾客满意度（不含偷钱/偷吃）。
        /// </summary>
        public void RecordVisitSatisfactionFromWaits(
            float queueWaitSeconds,
            float orderWaitSeconds,
            float serveWaitSeconds,
            float checkoutWaitSeconds,
            int customerCount = 1,
            bool completedVisit = true)
        {
            EnsureGameplayDefaults();
            var samples = Mathf.Max(1, customerCount);
            if (completedVisit)
            {
                SaveData.gameplay.sessionServedCustomers += samples;
            }

            ResolveWaitThresholds(
                out var softQueue,
                out var hardQueue,
                out var softOrder,
                out var hardOrder,
                out var softServe,
                out var hardServe,
                out var softCheckout,
                out var hardCheckout);

            var score = 95f;
            string worstReason = null;
            var worstOverage = 0f;

            ApplyWaitPenalty(
                queueWaitSeconds, softQueue, hardQueue, DissatisfactionQueueWait,
                ref score, ref worstReason, ref worstOverage);
            ApplyWaitPenalty(
                orderWaitSeconds, softOrder, hardOrder, DissatisfactionOrderWait,
                ref score, ref worstReason, ref worstOverage);
            ApplyWaitPenalty(
                serveWaitSeconds, softServe, hardServe, DissatisfactionServeWait,
                ref score, ref worstReason, ref worstOverage);
            ApplyWaitPenalty(
                checkoutWaitSeconds, softCheckout, hardCheckout, DissatisfactionCheckoutWait,
                ref score, ref worstReason, ref worstOverage);

            score = Mathf.Clamp(score, 15f, 100f);
            AddSatisfactionSamples(score, samples);
            if (!string.IsNullOrWhiteSpace(worstReason) && worstOverage > 0f)
            {
                AddDissatisfactionReason(worstReason, samples);
            }

            SaveGame();
        }

        /// <summary>
        /// 打烊清场：仅对仍在排队/待服务的顾客按等待记不满。
        /// </summary>
        public void RecordForcedClosingWaitSatisfaction(
            float queueWaitSeconds,
            float orderWaitSeconds,
            float serveWaitSeconds,
            float checkoutWaitSeconds,
            bool wasSeatedOrDining)
        {
            // 已在用餐且无显著服务等待：不算等待不满
            if (wasSeatedOrDining
                && queueWaitSeconds < 1f
                && orderWaitSeconds < 1f
                && serveWaitSeconds < 1f
                && checkoutWaitSeconds < 1f)
            {
                return;
            }

            RecordVisitSatisfactionFromWaits(
                queueWaitSeconds,
                orderWaitSeconds,
                serveWaitSeconds,
                checkoutWaitSeconds,
                customerCount: 1,
                completedVisit: false);
        }

        /// <summary>
        /// 记录因等待过久中途离场的顾客（各等待阶段离场均计入）。
        /// </summary>
        public void RecordClosingSessionWaitWalkout(CustomerWalkoutReason reason)
        {
            if (!IsWaitRelatedWalkout(reason))
            {
                return;
            }

            EnsureGameplayDefaults();
            SaveData.gameplay.sessionWaitWalkoutCustomers += 1;

            var dissatisfaction = MapWalkoutReasonToDissatisfaction(reason);
            if (!string.IsNullOrWhiteSpace(dissatisfaction))
            {
                AddDissatisfactionReason(dissatisfaction, 1);
            }

            AddSatisfactionSamples(22f, 1);
            SaveGame();
        }

        /// <summary>
        /// 本营业日因等待过久离场的顾客人数。
        /// </summary>
        public int GetSessionWaitWalkoutCount()
        {
            EnsureGameplayDefaults();
            return Mathf.Max(0, SaveData.gameplay.sessionWaitWalkoutCustomers);
        }

        /// <summary>
        /// 当前营业日平均满意度（0~100）；无样本时返回 -1。
        /// </summary>
        public float GetSessionSatisfactionAverage()
        {
            EnsureGameplayDefaults();
            var samples = SaveData.gameplay.sessionSatisfactionSamples;
            if (samples <= 0)
            {
                return -1f;
            }

            return Mathf.Clamp(SaveData.gameplay.sessionSatisfactionSum / samples, 0f, 100f);
        }

        public string GetTopDissatisfactionReason(out int count)
        {
            EnsureGameplayDefaults();
            count = 0;
            var list = SaveData.gameplay.sessionDissatisfactionReasons;
            if (list == null || list.Count == 0)
            {
                return string.Empty;
            }

            ClosingDissatisfactionEntry best = null;
            for (var index = 0; index < list.Count; index++)
            {
                var entry = list[index];
                if (entry == null || entry.count <= 0 || string.IsNullOrWhiteSpace(entry.reason))
                {
                    continue;
                }

                if (best == null || entry.count > best.count)
                {
                    best = entry;
                }
            }

            if (best == null)
            {
                return string.Empty;
            }

            count = best.count;
            return best.reason;
        }

        /// <summary>
        /// 确认结算时写入一条打烊记录（新在前）。
        /// </summary>
        public ClosingSessionRecord CommitClosingRecord()
        {
            EnsureGameplayDefaults();
            var gameplay = SaveData.gameplay;
            gameplay.recentClosings ??= new List<ClosingSessionRecord>();

            var income = Mathf.Max(0, gameplay.pendingSettlementIncome);
            var spend = Mathf.Max(0, gameplay.pendingSettlementCosts);
            var satisfaction = GetSessionSatisfactionAverage();
            if (satisfaction < 0f)
            {
                satisfaction = 80f;
            }

            var topReason = GetTopDissatisfactionReason(out var topCount);
            var record = new ClosingSessionRecord
            {
                closedUtcTicks = DateTime.UtcNow.Ticks,
                income = income,
                spend = spend,
                profit = income - spend,
                servedCustomers = Mathf.Max(0, gameplay.sessionServedCustomers),
                unpaidCheckouts = Mathf.Max(0, gameplay.sessionUnpaidCheckouts),
                satisfactionScore = satisfaction,
                topDissatisfactionReason = topReason ?? string.Empty,
                topDissatisfactionCount = topCount
            };

            gameplay.recentClosings.Insert(0, record);
            while (gameplay.recentClosings.Count > MaxRecentClosings)
            {
                gameplay.recentClosings.RemoveAt(gameplay.recentClosings.Count - 1);
            }

            gameplay.waitingForSettlement = false;
            EvaluateAchievementDayEnd(record);
            SaveGame();
            return record;
        }

        public IReadOnlyList<ClosingSessionRecord> GetRecentClosingRecords()
        {
            EnsureGameplayDefaults();
            SaveData.gameplay.recentClosings ??= new List<ClosingSessionRecord>();
            return SaveData.gameplay.recentClosings;
        }

        /// <summary>
        /// 读取打烊结算面板用的前天 / 昨天 / 今天收支摘要（今天取当前待结算数据）。
        /// </summary>
        public void GetClosingSummaryThreeDays(
            out SettlementSummaryDaySnapshot dayBeforeYesterday,
            out SettlementSummaryDaySnapshot yesterday,
            out SettlementSummaryDaySnapshot today)
        {
            EnsureGameplayDefaults();
            var gameplay = SaveData.gameplay;
            var history = GetRecentClosingRecords();
            dayBeforeYesterday = BuildSummarySnapshot("前天", history.Count > 1 ? history[1] : null);
            yesterday = BuildSummarySnapshot("昨天", history.Count > 0 ? history[0] : null);

            var income = Mathf.Max(0, gameplay.pendingSettlementIncome);
            var spend = Mathf.Max(0, gameplay.pendingSettlementCosts);
            today = new SettlementSummaryDaySnapshot
            {
                DayLabel = "今天",
                Income = income,
                Spend = spend,
                Profit = income - spend
            };
        }

        /// <summary>
        /// 结算详情：仅展示满意度与核心反馈。
        /// </summary>
        public string BuildClosingSatisfactionDetailText()
        {
            EnsureGameplayDefaults();
            var satisfaction = GetSessionSatisfactionAverage();
            var topReason = GetTopDissatisfactionReason(out var topCount);
            var waitWalkoutCount = GetSessionWaitWalkoutCount();
            var sb = new StringBuilder(128);

            if (satisfaction < 0f)
            {
                sb.Append("顾客满意度  十分满意");
                return sb.ToString();
            }

            sb.AppendLine($"顾客满意度  {satisfaction:0.#} 分 · {DescribeSatisfaction(satisfaction)}");
            sb.Append(BuildClosingFeedbackText(satisfaction, topReason, topCount, waitWalkoutCount));

            return sb.ToString();
        }

        public string BuildClosingSettlementDetailText()
        {
            return BuildClosingSatisfactionDetailText();
        }

        private static SettlementSummaryDaySnapshot BuildSummarySnapshot(string dayLabel, ClosingSessionRecord record)
        {
            if (record == null)
            {
                return new SettlementSummaryDaySnapshot
                {
                    DayLabel = dayLabel,
                    Income = 0,
                    Spend = 0,
                    Profit = 0
                };
            }

            return new SettlementSummaryDaySnapshot
            {
                DayLabel = dayLabel,
                Income = Mathf.Max(0, record.income),
                Spend = Mathf.Max(0, record.spend),
                Profit = record.profit
            };
        }

        private static string BuildClosingFeedbackText(
            float satisfaction,
            string topReason,
            int topCount,
            int waitWalkoutCount)
        {
            if (waitWalkoutCount > 0)
            {
                return BuildWaitWalkoutFeedback(satisfaction, topReason, topCount, waitWalkoutCount);
            }

            if (string.IsNullOrWhiteSpace(topReason) || topCount <= 0)
            {
                return "反馈：等待顺畅，顾客满意";
            }

            if (satisfaction >= LenientFeedbackSatisfactionFloor
                && topCount < LenientFeedbackMaxTopCount)
            {
                return "反馈：整体尚可，偶有少量等候";
            }

            return $"反馈：{BuildDissatisfactionFeedback(topReason)}";
        }

        private static string BuildWaitWalkoutFeedback(
            float satisfaction,
            string topReason,
            int topCount,
            int waitWalkoutCount)
        {
            var walkoutText = waitWalkoutCount == 1
                ? "1 位顾客因等候过久离场"
                : $"{waitWalkoutCount} 位顾客因等候过久离场";

            if (satisfaction >= LenientFeedbackSatisfactionFloor)
            {
                return $"反馈：{walkoutText}，其余顾客满意";
            }

            if (!string.IsNullOrWhiteSpace(topReason) && topCount > waitWalkoutCount)
            {
                return $"反馈：{walkoutText}，{BuildDissatisfactionFeedback(topReason)}";
            }

            return $"反馈：{walkoutText}";
        }

        private static string BuildDissatisfactionFeedback(string reason)
        {
            return reason switch
            {
                DissatisfactionQueueWait => "排队偶尔偏长，整体还能接受",
                DissatisfactionOrderWait => "点单偶尔偏慢，整体还能接受",
                DissatisfactionServeWait => "上菜偶尔偏慢，整体还能接受",
                DissatisfactionCheckoutWait => "结账偶尔稍慢，整体还能接受",
                _ => "等候略多，但大体尚可"
            };
        }

        private static bool IsWaitRelatedWalkout(CustomerWalkoutReason reason)
        {
            return reason is CustomerWalkoutReason.QueueTooLong
                or CustomerWalkoutReason.OrderTooLong
                or CustomerWalkoutReason.ServeTooSlow
                or CustomerWalkoutReason.CheckoutTooLong;
        }

        private static string MapWalkoutReasonToDissatisfaction(CustomerWalkoutReason reason)
        {
            return reason switch
            {
                CustomerWalkoutReason.QueueTooLong => DissatisfactionQueueWait,
                CustomerWalkoutReason.OrderTooLong => DissatisfactionOrderWait,
                CustomerWalkoutReason.ServeTooSlow => DissatisfactionServeWait,
                CustomerWalkoutReason.CheckoutTooLong => DissatisfactionCheckoutWait,
                _ => string.Empty
            };
        }

        private static void ResolveWaitThresholds(
            out float softQueue,
            out float hardQueue,
            out float softOrder,
            out float hardOrder,
            out float softServe,
            out float hardServe,
            out float softCheckout,
            out float hardCheckout)
        {
            softQueue = SoftQueueWait;
            hardQueue = HardQueueWait;

            var orderBase = Mathf.Max(1f, TbConfigRuntime.GetOrderTime(3f));
            softOrder = orderBase * SoftOrderWaitMul;
            hardOrder = orderBase * HardOrderWaitMul;

            var serveBase = Mathf.Max(1f, TbConfigRuntime.GetChefCookTime(8f));
            softServe = serveBase * SoftServeWaitMul;
            hardServe = serveBase * HardServeWaitMul;

            var checkoutBase = Mathf.Max(1f, TbConfigRuntime.GetWaiterCheckoutTime(3f));
            softCheckout = checkoutBase * SoftCheckoutWaitMul;
            hardCheckout = checkoutBase * HardCheckoutWaitMul;
        }

        private static void ApplyWaitPenalty(
            float waitSeconds,
            float softSeconds,
            float hardSeconds,
            string reason,
            ref float score,
            ref string worstReason,
            ref float worstOverage)
        {
            if (waitSeconds <= softSeconds)
            {
                return;
            }

            var span = Mathf.Max(0.01f, hardSeconds - softSeconds);
            var t = Mathf.Clamp01((waitSeconds - softSeconds) / span);
            score -= Mathf.Lerp(5f, 22f, t);

            var overage = waitSeconds - softSeconds;
            if (waitSeconds >= hardSeconds && overage > worstOverage)
            {
                worstOverage = overage;
                worstReason = reason;
            }
        }

        private void AddSatisfactionSamples(float score, int samples)
        {
            var gameplay = SaveData.gameplay;
            var clamped = Mathf.Clamp(score, 0f, 100f);
            var count = Mathf.Max(1, samples);
            gameplay.sessionSatisfactionSum += clamped * count;
            gameplay.sessionSatisfactionSamples += count;
        }

        private void AddDissatisfactionReason(string reason, int count)
        {
            if (string.IsNullOrWhiteSpace(reason) || count <= 0)
            {
                return;
            }

            var list = SaveData.gameplay.sessionDissatisfactionReasons ??= new List<ClosingDissatisfactionEntry>();
            for (var index = 0; index < list.Count; index++)
            {
                var entry = list[index];
                if (entry != null && entry.reason == reason)
                {
                    entry.count += count;
                    return;
                }
            }

            list.Add(new ClosingDissatisfactionEntry { reason = reason, count = count });
        }

        private static string DescribeSatisfaction(float score)
        {
            if (score >= 82f)
            {
                return "服务快捷";
            }

            if (score >= 68f)
            {
                return "大体顺畅";
            }

            if (score >= 45f)
            {
                return "略嫌等候";
            }

            return "等待过久";
        }
    }
}
