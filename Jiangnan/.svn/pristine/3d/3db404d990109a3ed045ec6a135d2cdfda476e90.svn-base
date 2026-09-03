using System.Collections.Generic;
using JN.Client.Manager;
using JN.Client.Model;
using QFramework;
using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 统一收口酒楼桌位状态切换，避免场景编排器在多处重复改数据和刷新表现。
    /// </summary>
    internal sealed class TavernTableStateService
    {
        private TavernWaiterTaskWaitTracker taskWaitTracker;

        internal void BindTaskWaitTracker(TavernWaiterTaskWaitTracker tracker)
        {
            taskWaitTracker = tracker;
        }

        /// <summary>
        /// 切到已占位，表示顾客已锁定桌位但尚未进入点单流程。
        /// </summary>
        public void SetReserved(int tableId, TableArea table, string customText = null, bool dispatchRuntimeChanged = true)
        {
            if (!TryValidate(tableId, table))
            {
                return;
            }

            DataManager.Instance.SetTableRuntimeState(tableId, TavernTableRuntimeState.Reserved);
            NotifyWaiterTaskWaitState(tableId, TavernTableRuntimeState.Reserved);
            table.RefreshRuntimeState(TavernTableRuntimeState.Reserved, customText);
            table.linkedUI?.StopStateCountdown();
            if (dispatchRuntimeChanged)
            {
                DispatchRuntimeChanged();
            }
        }

        /// <summary>
        /// 切到待点单，并停止上一阶段遗留倒计时。
        /// </summary>
        public void SetWaitingOrder(int tableId, TableArea table, string customText = null, bool dispatchRuntimeChanged = true)
        {
            if (!TryValidate(tableId, table))
            {
                return;
            }

            DataManager.Instance.SetTableRuntimeState(tableId, TavernTableRuntimeState.WaitingOrder);
            NotifyWaiterTaskWaitState(tableId, TavernTableRuntimeState.WaitingOrder);
            table.RefreshRuntimeState(TavernTableRuntimeState.WaitingOrder, customText);
            table.linkedUI?.StopStateCountdown();
            if (dispatchRuntimeChanged)
            {
                DispatchRuntimeChanged();
            }
        }

        /// <summary>
        /// 切到待上菜，并停止点单阶段倒计时。
        /// </summary>
        public void SetWaitingServe(int tableId, TableArea table, string customText = "待上菜", bool dispatchRuntimeChanged = true)
        {
            if (!TryValidate(tableId, table))
            {
                return;
            }

            DataManager.Instance.SetTableRuntimeState(tableId, TavernTableRuntimeState.WaitingServe);
            NotifyWaiterTaskWaitState(tableId, TavernTableRuntimeState.WaitingServe);
            table.RefreshRuntimeState(TavernTableRuntimeState.WaitingServe, customText);
            table.linkedUI?.StopStateCountdown();
            if (dispatchRuntimeChanged)
            {
                DispatchRuntimeChanged();
            }
        }

        /// <summary>
        /// 切到用餐中，并同步桌面菜品与顾客用餐表现。
        /// </summary>
        public void SetDining(
            int tableId,
            TableArea table,
            float diningDuration,
            GameObject dishPrefab,
            IReadOnlyList<TavernCustomerRuntimeController> diningCustomers,
            bool dispatchRuntimeChanged = true)
        {
            if (!TryValidate(tableId, table))
            {
                return;
            }

            DataManager.Instance.SetTableRuntimeState(tableId, TavernTableRuntimeState.Dining);
            NotifyWaiterTaskWaitState(tableId, TavernTableRuntimeState.Dining);
            table.RefreshRuntimeState(TavernTableRuntimeState.Dining);
            table.linkedUI?.StartStateCountdown(TavernTableRuntimeState.Dining, diningDuration, "用餐中");
            table.ShowDishVisual(dishPrefab);

            if (diningCustomers != null)
            {
                for (var index = 0; index < diningCustomers.Count; index++)
                {
                    diningCustomers[index]?.BeginDining(diningDuration);
                }
            }

            if (dispatchRuntimeChanged)
            {
                DispatchRuntimeChanged();
            }
        }

        /// <summary>
        /// 切到待结账，并在需要时显示空盘。
        /// </summary>
        public void SetCheckout(
            int tableId,
            TableArea table,
            string customText = null,
            bool showEmptyPlateVisual = false,
            bool dispatchRuntimeChanged = true)
        {
            if (!TryValidate(tableId, table))
            {
                return;
            }

            DataManager.Instance.SetTableRuntimeState(tableId, TavernTableRuntimeState.Checkout);
            NotifyWaiterTaskWaitState(tableId, TavernTableRuntimeState.Checkout);
            table.RefreshRuntimeState(TavernTableRuntimeState.Checkout, customText);
            if (showEmptyPlateVisual)
            {
                table.ShowEmptyPlateVisual();
            }

            table.linkedUI?.StopStateCountdown();
            if (dispatchRuntimeChanged)
            {
                DispatchRuntimeChanged();
            }
        }

        /// <summary>
        /// 切到清理中，并停止上一阶段倒计时。
        /// </summary>
        public void SetCleaning(int tableId, TableArea table, string customText = "等待清理", bool dispatchRuntimeChanged = true)
        {
            if (!TryValidate(tableId, table))
            {
                return;
            }

            DataManager.Instance.SetTableRuntimeState(tableId, TavernTableRuntimeState.Cleaning);
            NotifyWaiterTaskWaitState(tableId, TavernTableRuntimeState.Cleaning);
            table.RefreshRuntimeState(TavernTableRuntimeState.Cleaning, customText);
            table.linkedUI?.StopStateCountdown();
            if (dispatchRuntimeChanged)
            {
                DispatchRuntimeChanged();
            }
        }

        /// <summary>
        /// 切回空闲态，并清理桌面残留表现。
        /// </summary>
        public void SetIdle(int tableId, TableArea table, bool clearDishVisual = true, bool dispatchRuntimeChanged = true)
        {
            if (!TryValidate(tableId, table))
            {
                return;
            }

            DataManager.Instance.SetTableRuntimeState(tableId, TavernTableRuntimeState.Idle);
            NotifyWaiterTaskWaitState(tableId, TavernTableRuntimeState.Idle);
            table.RefreshRuntimeState(TavernTableRuntimeState.Idle);
            table.linkedUI?.StopStateCountdown();
            if (clearDishVisual)
            {
                table.ClearDishVisual();
            }

            if (dispatchRuntimeChanged)
            {
                DispatchRuntimeChanged();
            }
        }

        private static bool TryValidate(int tableId, TableArea table)
        {
            return tableId > 0 && table != null && DataManager.Instance != null;
        }

        private static void DispatchRuntimeChanged()
        {
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
        }

        private void NotifyWaiterTaskWaitState(int tableId, TavernTableRuntimeState state)
        {
            taskWaitTracker?.OnTableWaitStateChanged(tableId, state);
        }
    }
}
