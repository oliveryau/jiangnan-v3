using System.Collections.Generic;
using JN.Client.Manager;
using JN.Client.Model;
using JN.Client.UI;
using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 营业中检测排队/上菜等待超时并触发顾客离场（逻辑层；表现层后续接入）。
    /// </summary>
    internal static class TavernCustomerWalkoutService
    {
        public static void TryProcessWalkouts(TavernSceneManager scene)
        {
            if (scene == null
                || !scene.IsBusinessOpenForWalkout()
                || scene.IsClosingBusiness)
            {
                return;
            }

            TryWalkoutQueuedCustomers(scene);
            TryWalkoutWaitingServeTables(scene);
        }

        private static void TryWalkoutQueuedCustomers(TavernSceneManager scene)
        {
            var eligible = scene.GetQueuePatienceEligibleCustomers(2);
            if (eligible == null || eligible.Count == 0)
            {
                return;
            }

            var threshold = DataManager.GetWalkoutQueueWaitSeconds();
            for (var index = 0; index < eligible.Count; index++)
            {
                var customer = eligible[index];
                if (customer == null)
                {
                    continue;
                }

                var wait = scene.GetCustomerQueueWaitSeconds(customer);
                if (wait < threshold)
                {
                    continue;
                }

                scene.TriggerCustomerWalkout(customer, CustomerWalkoutReason.QueueTooLong);
                return;
            }
        }

        private static void TryWalkoutWaitingServeTables(TavernSceneManager scene)
        {
            var threshold = DataManager.GetWalkoutServeWaitSeconds();
            var tableIds = scene.GetWaitingServeTableIdsSnapshot();
            for (var index = 0; index < tableIds.Count; index++)
            {
                var tableId = tableIds[index];
                var serveWait = scene.GetTableServeWaitSeconds(tableId);
                if (serveWait < threshold)
                {
                    continue;
                }

                if (!scene.TryGetWalkoutCustomerForTable(tableId, out var customer) || customer == null)
                {
                    continue;
                }

                scene.TriggerCustomerWalkout(customer, CustomerWalkoutReason.ServeTooSlow);
                return;
            }
        }
    }

    /// <summary>
    /// 维护顾客等待 HUD：每组仅 1~2 人显示，进度满后整组离场。
    /// </summary>
    internal sealed class TavernCustomerWaitHudService
    {
        private const int MaxDisplaysPerGroup = 2;
        private static readonly Vector3 HeadOffset = new(0f, TavernWorldWaitHudItemView.DefaultHeadOffsetY, 0f);

        private sealed class GroupBinding
        {
            public string Key;
            public CustomerWaitHudState State;
            public int TableId;
            public int QueueGroupId;
            public readonly List<TavernCustomerRuntimeController> Members = new();
            public readonly Dictionary<TavernCustomerRuntimeController, TavernWorldWaitHudItemView> HudItems = new();
            public bool WalkoutTriggered;
        }

        private readonly Dictionary<string, GroupBinding> activeGroups = new();
        private TavernWorldRuntimeHudPanelController hudPanel;

        public void Clear()
        {
            var groupKeys = SnapshotGroupKeys();
            for (var index = 0; index < groupKeys.Count; index++)
            {
                if (!activeGroups.TryGetValue(groupKeys[index], out var binding))
                {
                    continue;
                }

                ReleaseGroupHud(binding, TavernSceneManager.Instance);
            }

            activeGroups.Clear();
            hudPanel = null;
        }

        public void ReleaseCustomer(TavernCustomerRuntimeController customer)
        {
            if (customer == null)
            {
                return;
            }

            hudPanel ??= HudOverlayService.EnsureWorldRuntimeHudPanelForWaitHud();

            var emptyGroups = new List<string>();
            var groupKeys = SnapshotGroupKeys();
            for (var groupIndex = 0; groupIndex < groupKeys.Count; groupIndex++)
            {
                if (!activeGroups.TryGetValue(groupKeys[groupIndex], out var binding))
                {
                    continue;
                }
                if (binding?.HudItems == null || binding.HudItems.Count == 0)
                {
                    continue;
                }

                if (!binding.HudItems.TryGetValue(customer, out var item))
                {
                    continue;
                }

                if (item != null)
                {
                    hudPanel?.ReleaseWaitHudItem(item.gameObject);
                }

                binding.HudItems.Remove(customer);
                binding.Members.Remove(customer);

                if (binding.HudItems.Count == 0 && binding.Members.Count == 0)
                {
                    emptyGroups.Add(groupKeys[groupIndex]);
                }
            }

            for (var index = 0; index < emptyGroups.Count; index++)
            {
                activeGroups.Remove(emptyGroups[index]);
            }
        }

        private static bool ShouldTrackCustomer(TavernCustomerRuntimeController customer)
        {
            return customer != null && !customer.IsLeavingTavern;
        }

        public void Tick(TavernSceneManager scene, float deltaTime)
        {
            if (scene == null || !scene.IsBusinessOpenForWalkout() || scene.IsClosingBusiness)
            {
                Clear();
                return;
            }

            hudPanel ??= HudOverlayService.EnsureWorldRuntimeHudPanelForWaitHud();
            if (hudPanel == null)
            {
                return;
            }

            var desired = BuildDesiredGroups(scene);
            RemoveStaleGroups(scene, desired);
            UpsertGroups(scene, desired);

            var groupKeys = SnapshotGroupKeys();
            var pendingWalkouts = new List<GroupBinding>();
            for (var index = 0; index < groupKeys.Count; index++)
            {
                if (!activeGroups.TryGetValue(groupKeys[index], out var binding) || binding == null)
                {
                    continue;
                }

                if (binding.Members.Count == 0)
                {
                    continue;
                }

                var waitSeconds = ResolveGroupWaitSeconds(scene, binding);
                // 排队等座：立刻显示；其余状态读配置宽限。
                var graceSeconds = binding.State == CustomerWaitHudState.Queue
                    ? 0f
                    : ResolveGraceSeconds(binding.State);
                var bubbleSeconds = ResolveBubbleSeconds(binding.State);
                var inGrace = waitSeconds < graceSeconds;

                // 等菜 / 等结账：宽限期内不计走客，也不显示耐心条。
                if (inGrace
                    && binding.State is CustomerWaitHudState.WaitingServe
                        or CustomerWaitHudState.WaitingCheckout)
                {
                    HideGroupPatienceHud(binding, scene);
                    continue;
                }

                var progress = inGrace
                    ? 0f
                    : bubbleSeconds > 0f
                        ? Mathf.Clamp01((waitSeconds - graceSeconds) / bubbleSeconds)
                        : 1f;
                var icon = CustomerWaitHudIconCatalog.Resolve(binding.State, scene, binding.TableId);

                EnsureDisplays(scene, binding, icon);
                UpdateDisplays(binding, progress);
                SyncTableWaitStatusText(scene, binding, true);

                if (!binding.WalkoutTriggered && progress >= 1f)
                {
                    binding.WalkoutTriggered = true;
                    pendingWalkouts.Add(binding);
                }
            }

            for (var index = 0; index < pendingWalkouts.Count; index++)
            {
                TriggerGroupWalkout(scene, pendingWalkouts[index]);
            }
        }

        private List<string> SnapshotGroupKeys()
        {
            return new List<string>(activeGroups.Keys);
        }

        private static Dictionary<string, GroupBinding> BuildDesiredGroups(TavernSceneManager scene)
        {
            var desired = new Dictionary<string, GroupBinding>();
            BuildQueueGroups(scene, desired);
            BuildTableGroups(scene, desired);
            return desired;
        }

        private static void BuildQueueGroups(TavernSceneManager scene, Dictionary<string, GroupBinding> desired)
        {
            // 仅前两名：已站定排队 + 无可坐空桌 + 未点单；每人独立一条。
            var eligible = scene.GetQueuePatienceEligibleCustomers(2);
            if (eligible == null || eligible.Count == 0)
            {
                return;
            }

            for (var index = 0; index < eligible.Count; index++)
            {
                var customer = eligible[index];
                if (customer == null)
                {
                    continue;
                }

                var instanceId = customer.GetInstanceID();
                var key = $"queue:cust:{instanceId}";
                var binding = new GroupBinding
                {
                    Key = key,
                    State = CustomerWaitHudState.Queue,
                    QueueGroupId = instanceId,
                };
                binding.Members.Add(customer);
                desired[key] = binding;
            }
        }

        private static void BuildTableGroups(TavernSceneManager scene, Dictionary<string, GroupBinding> desired)
        {
            var tableIds = new List<int>(scene.AllTables.Keys);
            for (var tableIndex = 0; tableIndex < tableIds.Count; tableIndex++)
            {
                var tableId = tableIds[tableIndex];
                var tableData = DataManager.Instance?.GetTableData(tableId);
                if (tableData == null || !tableData.isUnlocked)
                {
                    continue;
                }

                var runtimeState = (TavernTableRuntimeState)tableData.runtimeState;
                // 点单阶段不显示客人耐心条（进度改挂前台掌柜）。
                var waitState = runtimeState switch
                {
                    TavernTableRuntimeState.WaitingOrder => CustomerWaitHudState.None,
                    TavernTableRuntimeState.WaitingServe => CustomerWaitHudState.WaitingServe,
                    TavernTableRuntimeState.Checkout => CustomerWaitHudState.WaitingCheckout,
                    _ => CustomerWaitHudState.None,
                };
                if (waitState == CustomerWaitHudState.None)
                {
                    continue;
                }

                if (scene.ShouldSuppressCustomerWaitHud(tableId, waitState))
                {
                    continue;
                }

                if (!scene.TryGetTableCustomerGroupForWaitHud(tableId, out var customers) || customers == null || customers.Count == 0)
                {
                    continue;
                }

                var key = $"table:{tableId}:{(int)waitState}";
                var binding = new GroupBinding
                {
                    Key = key,
                    State = waitState,
                    TableId = tableId,
                };
                // 上菜耐心：须已入座。点单后走向座位途中（MovingToTable）不挂条。
                binding.Members.AddRange(customers.FindAll(customer =>
                    ShouldTrackCustomer(customer)
                    && (waitState != CustomerWaitHudState.WaitingServe || customer.IsSeated)));
                if (binding.Members.Count == 0)
                {
                    continue;
                }

                desired[key] = binding;
            }
        }

        private static void SyncTableWaitStatusText(TavernSceneManager scene, GroupBinding binding, bool visible)
        {
            if (binding == null || binding.TableId <= 0 || scene == null)
            {
                return;
            }

            if (!scene.AllTables.TryGetValue(binding.TableId, out var table) || table?.linkedUI == null)
            {
                return;
            }

            table.linkedUI.SetCustomerWaitHudActive(visible);
        }

        private void RemoveStaleGroups(TavernSceneManager scene, Dictionary<string, GroupBinding> desired)
        {
            var staleKeys = new List<string>();
            var groupKeys = SnapshotGroupKeys();
            for (var index = 0; index < groupKeys.Count; index++)
            {
                var key = groupKeys[index];
                if (!desired.ContainsKey(key))
                {
                    if (activeGroups.TryGetValue(key, out var binding))
                    {
                        ReleaseGroupHud(binding, scene);
                    }

                    staleKeys.Add(key);
                }
            }

            for (var index = 0; index < staleKeys.Count; index++)
            {
                activeGroups.Remove(staleKeys[index]);
            }
        }

        private void UpsertGroups(TavernSceneManager scene, Dictionary<string, GroupBinding> desired)
        {
            foreach (var pair in desired)
            {
                if (!activeGroups.TryGetValue(pair.Key, out var existing))
                {
                    activeGroups[pair.Key] = pair.Value;
                    continue;
                }

                if (existing.State != pair.Value.State)
                {
                    ReleaseGroupHud(existing, scene);
                    activeGroups[pair.Key] = pair.Value;
                    continue;
                }

                existing.Members.Clear();
                existing.Members.AddRange(pair.Value.Members);
            }
        }

        private static float ResolveGroupWaitSeconds(TavernSceneManager scene, GroupBinding binding)
        {
            var maxWait = 0f;
            for (var index = 0; index < binding.Members.Count; index++)
            {
                var customer = binding.Members[index];
                if (customer == null)
                {
                    continue;
                }

                var wait = binding.State switch
                {
                    CustomerWaitHudState.Queue => scene.GetCustomerQueueWaitSeconds(customer),
                    CustomerWaitHudState.WaitingOrder => scene.GetTableOrderWaitSeconds(binding.TableId),
                    CustomerWaitHudState.WaitingServe => scene.GetTableServeWaitSeconds(binding.TableId),
                    CustomerWaitHudState.WaitingCheckout => scene.GetTableCheckoutWaitSeconds(binding.TableId),
                    _ => 0f,
                };
                maxWait = Mathf.Max(maxWait, wait);
            }

            return maxWait;
        }

        private static float ResolveGraceSeconds(CustomerWaitHudState state)
        {
            return DataManager.GetCustomerWaitGraceSeconds(state);
        }

        private static float ResolveBubbleSeconds(CustomerWaitHudState state)
        {
            return DataManager.GetCustomerWaitBubbleSeconds(state);
        }

        private void EnsureDisplays(TavernSceneManager scene, GroupBinding binding, Sprite icon)
        {
            var displayCount = Mathf.Min(MaxDisplaysPerGroup, binding.Members.Count);
            for (var index = 0; index < displayCount; index++)
            {
                var customer = binding.Members[index];
                if (!ShouldTrackCustomer(customer))
                {
                    continue;
                }

                if (binding.HudItems.TryGetValue(customer, out var existing) && existing != null)
                {
                    existing.RefreshVisual(binding.State, icon);
                    continue;
                }

                // itemName 按客人区分，避免同组多人复用同一名字导致后一个覆盖前一个。
                var itemName = $"{binding.Key}:{customer.GetInstanceID()}";
                var item = hudPanel.CreateWaitHudItem(customer.transform, HeadOffset, itemName, icon, binding.State);
                if (item == null)
                {
                    continue;
                }

                item.Configure(binding.State, icon);
                binding.HudItems[customer] = item;
            }

            var staleCustomers = new List<TavernCustomerRuntimeController>();
            foreach (var pair in binding.HudItems)
            {
                if (!ShouldTrackCustomer(pair.Key) || !binding.Members.Contains(pair.Key))
                {
                    staleCustomers.Add(pair.Key);
                    continue;
                }

                var memberIndex = binding.Members.IndexOf(pair.Key);
                if (memberIndex >= displayCount)
                {
                    staleCustomers.Add(pair.Key);
                }
            }

            for (var index = 0; index < staleCustomers.Count; index++)
            {
                var customer = staleCustomers[index];
                if (customer != null && binding.HudItems.TryGetValue(customer, out var item) && item != null)
                {
                    hudPanel.ReleaseWaitHudItem(item.gameObject);
                }

                binding.HudItems.Remove(customer);
            }
        }

        private static void UpdateDisplays(GroupBinding binding, float progress)
        {
            foreach (var pair in binding.HudItems)
            {
                pair.Value?.SetWaitProgress(progress);
            }
        }

        private static void TriggerGroupWalkout(TavernSceneManager scene, GroupBinding binding)
        {
            var reason = binding.State switch
            {
                CustomerWaitHudState.Queue => CustomerWalkoutReason.QueueTooLong,
                CustomerWaitHudState.WaitingOrder => CustomerWalkoutReason.OrderTooLong,
                CustomerWaitHudState.WaitingServe => CustomerWalkoutReason.ServeTooSlow,
                CustomerWaitHudState.WaitingCheckout => CustomerWalkoutReason.CheckoutTooLong,
                _ => CustomerWalkoutReason.None,
            };
            if (reason == CustomerWalkoutReason.None)
            {
                return;
            }

            if (binding.TableId > 0)
            {
                if (scene.TryGetWalkoutCustomerForTable(binding.TableId, out var representative) && representative != null)
                {
                    scene.TriggerCustomerWalkout(representative, reason);
                }

                return;
            }

            var snapshot = new List<TavernCustomerRuntimeController>(binding.Members);
            for (var index = 0; index < snapshot.Count; index++)
            {
                var customer = snapshot[index];
                if (customer == null)
                {
                    continue;
                }

                scene.TriggerCustomerWalkout(customer, reason);
            }
        }

        private void ReleaseGroupHud(GroupBinding binding, TavernSceneManager scene = null)
        {
            if (binding == null)
            {
                return;
            }

            HideGroupPatienceHud(binding, scene);
            binding.Members.Clear();
        }

        /// <summary>
        /// 仅回收耐心 HUD，保留组成员（宽限期内隐藏条时用）。
        /// </summary>
        private void HideGroupPatienceHud(GroupBinding binding, TavernSceneManager scene)
        {
            if (binding == null)
            {
                return;
            }

            SyncTableWaitStatusText(scene, binding, false);

            if (hudPanel == null)
            {
                binding.HudItems.Clear();
                return;
            }

            foreach (var pair in binding.HudItems)
            {
                if (pair.Value != null)
                {
                    hudPanel.ReleaseWaitHudItem(pair.Value.gameObject);
                }
            }

            binding.HudItems.Clear();
        }

        /// <summary>
        /// 立即移除指定桌位在某个等待阶段的 HUD（派单/上菜完成时主动清理，不依赖下一帧 Tick）。
        /// </summary>
        public void ReleaseTableWaitState(int tableId, CustomerWaitHudState state)
        {
            if (tableId <= 0 || state == CustomerWaitHudState.None)
            {
                return;
            }

            var key = $"table:{tableId}:{(int)state}";
            if (!activeGroups.TryGetValue(key, out var binding))
            {
                return;
            }

            ReleaseGroupHud(binding, TavernSceneManager.Instance);
            activeGroups.Remove(key);
        }
    }
}
