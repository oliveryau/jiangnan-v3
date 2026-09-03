using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 小二任务派发服务。
    /// </summary>
    internal sealed class TavernTaskDispatchService
    {
        public bool TryDispatchWaiterTask(IWaiterRuntimeHost host, WaiterTask task, bool ignoreSkillGate = false)
        {
            if (host == null || task == null)
            {
                return false;
            }

            var waiter = host.GetAvailableWaiterForTask(task, ignoreSkillGate);
            if (waiter == null)
            {
                return false;
            }

            return host.TryStartWaiterTask(waiter, task, CreateInitialState(task));
        }

        public void StartReturnHome(IWaiterRuntimeHost host, GameObject waiter)
        {
            if (host == null || waiter == null)
            {
                return;
            }

            var context = host.GetOrCreateWaiterContext(waiter);
            if (context.CurrentStateKey == WaiterStateKeys.ReturningHome)
            {
                return;
            }

            context.HomeIndex = host.ResolveWaiterHomeIndex(waiter);
            context.CurrentTask = null;
            context.TransitionTo(new WaiterReturningHomeState());
        }

        public void StartAttractCustomers(IWaiterRuntimeHost host, GameObject waiter)
        {
            if (host == null || waiter == null)
            {
                return;
            }

            host.EnsureWaiterAnimationReceiver(waiter);
            host.StopTrackedWaiterRoutine(waiter);
            var context = host.GetOrCreateWaiterContext(waiter);
            context.CurrentTask = null;
            context.HomeIndex = host.ResolveWaiterHomeIndex(waiter);
            context.TransitionTo(new WaiterMoveToAttractPointState());
        }

        public void EnterNap(IWaiterRuntimeHost host, GameObject waiter)
        {
            if (host == null || waiter == null)
            {
                return;
            }

            host.StopTrackedWaiterRoutine(waiter);
            var context = host.GetOrCreateWaiterContext(waiter);
            context.CurrentTask = null;
            host.ClearWaiterCarryPlate(waiter);
            context.PendingDishPrefab = null;
            context.SetPassiveState(new WaiterNappingState());
        }

        public void WakeFromNap(IWaiterRuntimeHost host, GameObject waiter)
        {
            if (host == null || waiter == null)
            {
                return;
            }

            var context = host.GetOrCreateWaiterContext(waiter);
            context.CurrentTask = null;
            // 必须切到 Idle：清除 CurrentStateKey=Napping，并经 ApplyWaiterPresentation 同步服务态。
            context.SetPassiveState(new WaiterIdleState());
        }

        private static ICharacterState<WaiterCharacter> CreateInitialState(WaiterTask task)
        {
            return task switch
            {
                WaiterOrderTask => new WaiterMoveToTableForOrderState(),
                WaiterServeTask => new WaiterMoveToServeTableState(),
                WaiterCheckoutTask => new WaiterMoveToTableForCheckoutState(),
                WaiterCleanTask => new WaiterMoveToCleanTableState(),
                _ => new WaiterIdleState()
            };
        }
    }
}
