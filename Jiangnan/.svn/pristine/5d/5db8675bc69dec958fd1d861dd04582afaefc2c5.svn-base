using System.Collections.Generic;
using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 记录本桌各服务阶段等待时长，供打烊满意度按「等待过长」评估。
    /// </summary>
    internal sealed class TavernWaitSatisfactionTracker
    {
        public readonly struct WaitBreakdown
        {
            public readonly float QueueSeconds;
            public readonly float OrderSeconds;
            public readonly float ServeSeconds;
            public readonly float CheckoutSeconds;

            public WaitBreakdown(float queueSeconds, float orderSeconds, float serveSeconds, float checkoutSeconds)
            {
                QueueSeconds = Mathf.Max(0f, queueSeconds);
                OrderSeconds = Mathf.Max(0f, orderSeconds);
                ServeSeconds = Mathf.Max(0f, serveSeconds);
                CheckoutSeconds = Mathf.Max(0f, checkoutSeconds);
            }
        }

        private sealed class TableWaitSample
        {
            public float QueueSeconds;
            public float OrderSeconds;
            public float ServeSeconds;
            public float CheckoutSeconds;
            public float OrderStart = -1f;
            public float ServeStart = -1f;
            public float CheckoutStart = -1f;
        }

        private readonly Dictionary<int, TableWaitSample> tableSamples = new();
        private readonly Dictionary<int, float> customerQueueStartTimes = new();

        public void ClearAll()
        {
            tableSamples.Clear();
            customerQueueStartTimes.Clear();
        }

        public void ClearTable(int tableId)
        {
            if (tableId > 0)
            {
                tableSamples.Remove(tableId);
            }
        }

        /// <summary>
        /// 顾客入队登记。排队耐心不在此处开表，由 <see cref="SyncQueuePatienceForEligible"/> 对符合条件者开表。
        /// </summary>
        public void OnCustomerQueued(int customerInstanceId)
        {
            // 保留调用点兼容；真正开表见 SyncQueuePatienceForEligible。
        }

        /// <summary>
        /// 仅为指定的排队客人累计耐心（通常为已站定排队位、且尚未进入前台点单的前两名）。
        /// 不在名单内的计时会被清除（刚进店走路、点单中、队尾均不计）。
        /// </summary>
        public void SyncQueuePatienceForEligible(IReadOnlyList<int> eligibleCustomerInstanceIds)
        {
            var keep = new HashSet<int>();
            if (eligibleCustomerInstanceIds != null)
            {
                for (var index = 0; index < eligibleCustomerInstanceIds.Count; index++)
                {
                    var id = eligibleCustomerInstanceIds[index];
                    if (id == 0)
                    {
                        continue;
                    }

                    keep.Add(id);
                    if (!customerQueueStartTimes.ContainsKey(id))
                    {
                        customerQueueStartTimes[id] = Time.time;
                    }
                }
            }

            if (customerQueueStartTimes.Count <= 0)
            {
                return;
            }

            var staleIds = new List<int>();
            foreach (var pair in customerQueueStartTimes)
            {
                if (!keep.Contains(pair.Key))
                {
                    staleIds.Add(pair.Key);
                }
            }

            for (var index = 0; index < staleIds.Count; index++)
            {
                customerQueueStartTimes.Remove(staleIds[index]);
            }
        }

        /// <summary>
        /// 兼容旧调用：仅队首开表。
        /// </summary>
        public void SyncQueuePatienceToHead(IReadOnlyList<TavernCustomerRuntimeController> queuedCustomers)
        {
            var ids = new List<int>(1);
            if (queuedCustomers != null)
            {
                for (var index = 0; index < queuedCustomers.Count; index++)
                {
                    var customer = queuedCustomers[index];
                    if (customer == null || customer.IsLeavingTavern)
                    {
                        continue;
                    }

                    ids.Add(customer.GetInstanceID());
                    break;
                }
            }

            SyncQueuePatienceForEligible(ids);
        }

        public void OnCustomerSeated(int customerInstanceId, int tableId)
        {
            if (tableId <= 0)
            {
                return;
            }

            var sample = GetOrCreate(tableId);
            if (customerInstanceId != 0
                && customerQueueStartTimes.TryGetValue(customerInstanceId, out var start))
            {
                sample.QueueSeconds = Mathf.Max(sample.QueueSeconds, Time.time - start);
                customerQueueStartTimes.Remove(customerInstanceId);
            }
        }

        public void OnCustomerLeftQueue(int customerInstanceId)
        {
            if (customerInstanceId != 0)
            {
                customerQueueStartTimes.Remove(customerInstanceId);
            }
        }

        public void OnWaitingOrder(int tableId)
        {
            if (tableId <= 0)
            {
                return;
            }

            var sample = GetOrCreate(tableId);
            SealPhase(ref sample.OrderStart, ref sample.OrderSeconds);
            SealPhase(ref sample.ServeStart, ref sample.ServeSeconds);
            SealPhase(ref sample.CheckoutStart, ref sample.CheckoutSeconds);
            sample.OrderStart = Time.time;
        }

        public void OnWaitingServe(int tableId)
        {
            if (tableId <= 0)
            {
                return;
            }

            var sample = GetOrCreate(tableId);
            SealPhase(ref sample.OrderStart, ref sample.OrderSeconds);
            sample.ServeStart = Time.time;
        }

        public void OnDining(int tableId)
        {
            if (tableId <= 0)
            {
                return;
            }

            var sample = GetOrCreate(tableId);
            SealPhase(ref sample.ServeStart, ref sample.ServeSeconds);
        }

        public void OnCheckout(int tableId)
        {
            if (tableId <= 0)
            {
                return;
            }

            var sample = GetOrCreate(tableId);
            SealPhase(ref sample.OrderStart, ref sample.OrderSeconds);
            SealPhase(ref sample.ServeStart, ref sample.ServeSeconds);
            sample.CheckoutStart = Time.time;
        }

        /// <summary>
        /// 结账完成时取出本桌等待数据并清除计时。
        /// </summary>
        public WaitBreakdown ConsumeTable(int tableId)
        {
            if (tableId <= 0 || !tableSamples.TryGetValue(tableId, out var sample) || sample == null)
            {
                return default;
            }

            SealPhase(ref sample.OrderStart, ref sample.OrderSeconds);
            SealPhase(ref sample.ServeStart, ref sample.ServeSeconds);
            SealPhase(ref sample.CheckoutStart, ref sample.CheckoutSeconds);
            var result = new WaitBreakdown(
                sample.QueueSeconds,
                sample.OrderSeconds,
                sample.ServeSeconds,
                sample.CheckoutSeconds);
            tableSamples.Remove(tableId);
            return result;
        }

        /// <summary>
        /// 打烊清场：按当前进行中的等待阶段估一个未完成等待。
        /// </summary>
        public WaitBreakdown PeekIncomplete(int tableId)
        {
            if (tableId <= 0 || !tableSamples.TryGetValue(tableId, out var sample) || sample == null)
            {
                return default;
            }

            var order = sample.OrderSeconds;
            var serve = sample.ServeSeconds;
            var checkout = sample.CheckoutSeconds;
            if (sample.OrderStart >= 0f)
            {
                order += Time.time - sample.OrderStart;
            }

            if (sample.ServeStart >= 0f)
            {
                serve += Time.time - sample.ServeStart;
            }

            if (sample.CheckoutStart >= 0f)
            {
                checkout += Time.time - sample.CheckoutStart;
            }

            return new WaitBreakdown(sample.QueueSeconds, order, serve, checkout);
        }

        public float PeekQueueWait(int customerInstanceId)
        {
            if (customerInstanceId == 0
                || !customerQueueStartTimes.TryGetValue(customerInstanceId, out var start))
            {
                return 0f;
            }

            return Mathf.Max(0f, Time.time - start);
        }

        /// <summary>
        /// 小二已到桌边：冻结当前阶段等待计时，不再累计至行为读条结束。
        /// </summary>
        public void SealActiveWait(int tableId, CustomerWaitHudState waitState)
        {
            if (tableId <= 0 || !tableSamples.TryGetValue(tableId, out var sample) || sample == null)
            {
                return;
            }

            switch (waitState)
            {
                case CustomerWaitHudState.WaitingOrder:
                    SealPhase(ref sample.OrderStart, ref sample.OrderSeconds);
                    break;
                case CustomerWaitHudState.WaitingServe:
                    SealPhase(ref sample.ServeStart, ref sample.ServeSeconds);
                    break;
                case CustomerWaitHudState.WaitingCheckout:
                    SealPhase(ref sample.CheckoutStart, ref sample.CheckoutSeconds);
                    break;
            }
        }

        private TableWaitSample GetOrCreate(int tableId)
        {
            if (!tableSamples.TryGetValue(tableId, out var sample) || sample == null)
            {
                sample = new TableWaitSample();
                tableSamples[tableId] = sample;
            }

            return sample;
        }

        private static void SealPhase(ref float startTime, ref float accumulated)
        {
            if (startTime < 0f)
            {
                return;
            }

            accumulated += Mathf.Max(0f, Time.time - startTime);
            startTime = -1f;
        }
    }
}
