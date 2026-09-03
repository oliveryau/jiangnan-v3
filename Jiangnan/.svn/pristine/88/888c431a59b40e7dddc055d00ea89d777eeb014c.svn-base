using System.Collections;
using JN.Client.Model;
using UnityEngine;

namespace JN.Client.Scene
{
    internal sealed class WaiterIdleState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.Idle;
    }

    internal sealed class WaiterMoveToTableForOrderState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.MoveToTableForOrder;

        public override IEnumerator Execute(WaiterCharacter actor)
        {
            var host = GetHost(actor);
            var waiter = GetWaiter(actor);
            if (GetTask(actor) is not WaiterOrderTask orderTask
                || host == null
                || waiter == null
                || !host.TryGetTable(orderTask.TableId, out var table)
                || !host.IsTableInState(orderTask.TableId, TavernTableRuntimeState.WaitingOrder))
            {
                host?.CompleteWaiterTask(actor);
                yield break;
            }

            yield return host.MoveWaiterToCounter(waiter);
            if (!host.IsTableInState(orderTask.TableId, TavernTableRuntimeState.WaitingOrder))
            {
                host.CompleteWaiterTask(actor);
                yield break;
            }

            host.SealCustomerWaitOnWaiterArrival(orderTask.TableId, CustomerWaitHudState.WaitingOrder);
            actor.TransitionTo(new WaiterOrderingState());
        }
    }

    internal sealed class WaiterOrderingState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.Ordering;

        public override IEnumerator Execute(WaiterCharacter actor)
        {
            var host = GetHost(actor);
            var waiter = GetWaiter(actor);
            if (GetTask(actor) is not WaiterOrderTask orderTask
                || host == null
                || waiter == null
                || !host.TryGetTable(orderTask.TableId, out var table)
                || !host.IsTableInState(orderTask.TableId, TavernTableRuntimeState.WaitingOrder))
            {
                host?.CompleteWaiterTask(actor);
                yield break;
            }

            host.SetWaiterAnimatorSpeed(host.GetWaiterAnimator(waiter), 0f);
            var duration = host.GetEffectiveWaiterOrderDuration(waiter);
            host.ShowWaiterTaskProgress(waiter, duration, host.ResolveWaiterOrderingIcon());
            yield return new WaitForSeconds(duration);

            if (!host.IsTableInState(orderTask.TableId, TavernTableRuntimeState.WaitingOrder))
            {
                host.CompleteWaiterTask(actor);
                yield break;
            }

            host.ConsumeWaiterStamina(waiter, WaiterStaminaAction.Ordering);
            actor.TransitionTo(new WaiterMoveToNotifyChefState());
        }
    }

    internal sealed class WaiterMoveToNotifyChefState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.MoveToNotifyChef;

        public override IEnumerator Execute(WaiterCharacter actor)
        {
            var host = GetHost(actor);
            var waiter = GetWaiter(actor);
            if (GetTask(actor) is not WaiterOrderTask orderTask
                || host == null
                || waiter == null
                || !host.TryGetTable(orderTask.TableId, out var table)
                || !host.IsTableInState(orderTask.TableId, TavernTableRuntimeState.WaitingOrder))
            {
                host?.CompleteWaiterTask(actor);
                yield break;
            }

            var orderIcon = host.ResolveWaiterNotifyChefIcon();
            // 点单完成后先切到待上菜并通知后厨，小二走到灶台后即空闲，不再挂机等菜。
            host.CompleteTableOrderByWaiter(orderTask.TableId, table, orderIcon);
            yield return host.MoveWaiterToDishPickup(waiter);
            host.NotifyChefCookOrderTicket(orderTask.TableId);
            host.ClearWaiterOrderCookProgress(waiter);
            host.ConsumeWaiterStamina(waiter, WaiterStaminaAction.NotifyChef);
            host.CompleteWaiterTask(actor);
        }
    }

    internal sealed class WaiterCookStealingState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.CookStealing;

        public override IEnumerator Execute(WaiterCharacter actor)
        {
            var host = GetHost(actor);
            var waiter = GetWaiter(actor);
            if (GetTask(actor) is not WaiterOrderTask orderTask
                || host == null
                || waiter == null
                || !host.TryGetTable(orderTask.TableId, out _)
                || !host.IsTableInState(orderTask.TableId, TavernTableRuntimeState.WaitingServe))
            {
                host?.CompleteWaiterTask(actor);
                yield break;
            }

            host.SetWaiterAnimatorSpeed(host.GetWaiterAnimator(waiter), 0f);
            host.ShowWaiterCookStealingProgress(waiter, orderTask.TableId);

            var waitForDish = new WaitForSeconds(0.2f);
            while (host.IsBusinessOpen)
            {
                if (!host.IsTableInState(orderTask.TableId, TavernTableRuntimeState.WaitingServe))
                {
                    host.CompleteWaiterTask(actor);
                    yield break;
                }

                if (host.HasWaiterCookStealBeenStopped(waiter))
                {
                    actor.TransitionTo(new WaiterMoveToPickupDishState());
                    yield break;
                }

                yield return waitForDish;
            }

            host.CompleteWaiterTask(actor);
        }
    }

    internal sealed class WaiterMoveToPickupDishState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.MoveToPickupDish;

        public override IEnumerator Execute(WaiterCharacter actor)
        {
            var host = GetHost(actor);
            var waiter = GetWaiter(actor);
            if (host == null || waiter == null || GetTask(actor) == null)
            {
                host?.CompleteWaiterTask(actor);
                yield break;
            }

            var waitForDish = new WaitForSeconds(0.2f);
            while (host.IsBusinessOpen)
            {
                if (!host.IsTableInState(actor.CurrentTask.TableId, TavernTableRuntimeState.WaitingServe))
                {
                    host.CompleteWaiterTask(actor);
                    yield break;
                }

                if (host.HasAvailablePreparedDishForServe(actor.CurrentTask.TableId))
                {
                    actor.PendingDishPrefab = host.TakePreparedDishForWaiter(waiter);
                    if (actor.PendingDishPrefab != null)
                    {
                        host.ClearWaiterOrderCookProgress(waiter);
                        actor.TransitionTo(new WaiterMoveToServeTableState());
                        yield break;
                    }
                }

                yield return waitForDish;
            }

            host.CompleteWaiterTask(actor);
        }
    }

    internal sealed class WaiterMoveToServeTableState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.MoveToServeTable;

        public override IEnumerator Execute(WaiterCharacter actor)
        {
            var host = GetHost(actor);
            var waiter = GetWaiter(actor);
            if (host == null
                || waiter == null
                || actor.CurrentTask is not WaiterServeTask && actor.CurrentTask is not WaiterOrderTask)
            {
                host?.CompleteWaiterTask(actor);
                yield break;
            }

            if (!host.TryGetTable(actor.CurrentTask.TableId, out var table)
                || !host.IsTableInState(actor.CurrentTask.TableId, TavernTableRuntimeState.WaitingServe))
            {
                if (actor.PendingDishPrefab != null)
                {
                    host.ReturnWaiterCarryDish(waiter, actor.PendingDishPrefab);
                    host.ReleaseReservedServeDish();
                    actor.PendingDishPrefab = null;
                }

                host.CompleteWaiterTask(actor);
                yield break;
            }

            if (actor.PendingDishPrefab == null)
            {
                yield return host.MoveWaiterToDishPickup(waiter);
                var waitForDish = new WaitForSeconds(0.2f);
                while (host.IsBusinessOpen)
                {
                    if (!host.IsTableInState(actor.CurrentTask.TableId, TavernTableRuntimeState.WaitingServe))
                    {
                        host.CompleteWaiterTask(actor);
                        yield break;
                    }

                    if (host.HasAvailablePreparedDishForServe(actor.CurrentTask.TableId))
                    {
                        actor.PendingDishPrefab = host.TakePreparedDishForWaiter(waiter);
                        if (actor.PendingDishPrefab != null)
                        {
                            break;
                        }
                    }

                    yield return waitForDish;
                }

                if (actor.PendingDishPrefab == null)
                {
                    host.CompleteWaiterTask(actor);
                    yield break;
                }
            }

            yield return host.MoveWaiterToTable(waiter, table);
            if (!host.IsTableInState(actor.CurrentTask.TableId, TavernTableRuntimeState.WaitingServe))
            {
                if (actor.PendingDishPrefab != null)
                {
                    host.ReturnWaiterCarryDish(waiter, actor.PendingDishPrefab);
                    host.ReleaseReservedServeDish();
                    actor.PendingDishPrefab = null;
                }

                host.CompleteWaiterTask(actor);
                yield break;
            }

            host.SealCustomerWaitOnWaiterArrival(actor.CurrentTask.TableId, CustomerWaitHudState.WaitingServe);
            actor.TransitionTo(new WaiterServingState());
        }
    }

    internal sealed class WaiterServingState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.Serving;

        public override IEnumerator Execute(WaiterCharacter actor)
        {
            var host = GetHost(actor);
            if (host == null
                || actor.CurrentTask == null
                || !host.TryGetTable(actor.CurrentTask.TableId, out var table))
            {
                if (actor.PendingDishPrefab != null)
                {
                    host?.ReturnWaiterCarryDish(GetWaiter(actor), actor.PendingDishPrefab);
                    host?.ReleaseReservedServeDish();
                    actor.PendingDishPrefab = null;
                }

                host?.CompleteWaiterTask(actor);
                yield break;
            }

            if (host.IsTableInState(actor.CurrentTask.TableId, TavernTableRuntimeState.WaitingServe) && actor.PendingDishPrefab != null)
            {
                var waiter = GetWaiter(actor);
                // 读条仅在到桌取菜后执行，MoveToServeTable 寻路不计入上菜时长。
                var duration = host.GetEffectiveWaiterServeDuration(waiter);
                if (duration > 0.05f && waiter != null)
                {
                    host.SetWaiterAnimatorSpeed(host.GetWaiterAnimator(waiter), 0f);
                    host.ShowWaiterTaskProgress(waiter, duration, host.ResolveWaiterServingIcon());
                    yield return new WaitForSeconds(duration);
                }

                if (!host.IsTableInState(actor.CurrentTask.TableId, TavernTableRuntimeState.WaitingServe)
                    || actor.PendingDishPrefab == null)
                {
                    if (actor.PendingDishPrefab != null)
                    {
                        host.ReturnWaiterCarryDish(waiter, actor.PendingDishPrefab);
                        host.ReleaseReservedServeDish();
                        actor.PendingDishPrefab = null;
                    }

                    host.CompleteWaiterTask(actor);
                    yield break;
                }

                host.ClearWaiterCarryPlate(waiter);
                host.ServeTableByWaiter(actor.CurrentTask.TableId, table, actor.PendingDishPrefab);
                host.ConsumeWaiterStamina(waiter, WaiterStaminaAction.Serving);
            }
            else if (actor.PendingDishPrefab != null)
            {
                host.ReturnWaiterCarryDish(GetWaiter(actor), actor.PendingDishPrefab);
                host.ReleaseReservedServeDish();
            }

            actor.PendingDishPrefab = null;
            host.CompleteWaiterTask(actor);
        }
    }

    internal sealed class WaiterMoveToTableForCheckoutState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.MoveToTableForCheckout;

        public override IEnumerator Execute(WaiterCharacter actor)
        {
            var host = GetHost(actor);
            var waiter = GetWaiter(actor);
            if (GetTask(actor) is not WaiterCheckoutTask checkoutTask
                || host == null
                || waiter == null
                || !host.TryGetTable(checkoutTask.TableId, out var table)
                || !host.IsTableInState(checkoutTask.TableId, TavernTableRuntimeState.Checkout))
            {
                if (host == null || !host.TryContinueCheckoutWaiterAsClean(actor))
                {
                    host?.CompleteWaiterCheckoutCancelledStayInPlace(actor);
                }

                yield break;
            }

            yield return host.MoveWaiterToTable(waiter, table);
            if (!host.IsTableInState(checkoutTask.TableId, TavernTableRuntimeState.Checkout))
            {
                if (!host.TryContinueCheckoutWaiterAsClean(actor))
                {
                    host.CompleteWaiterCheckoutCancelledStayInPlace(actor);
                }

                yield break;
            }

            host.HideCheckoutDisplay(table);
            host.SealCustomerWaitOnWaiterArrival(checkoutTask.TableId, CustomerWaitHudState.WaitingCheckout);

            actor.TransitionTo(host.ShouldWaiterStealBeforeCheckout(waiter, checkoutTask.TableId)
                ? new WaiterStealingState()
                : new WaiterCheckoutingState());
        }
    }

    internal sealed class WaiterCheckoutingState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.Checkouting;

        public override IEnumerator Execute(WaiterCharacter actor)
        {
            var host = GetHost(actor);
            var waiter = GetWaiter(actor);
            if (GetTask(actor) is not WaiterCheckoutTask checkoutTask
                || host == null
                || waiter == null
                || !host.TryGetTable(checkoutTask.TableId, out var table)
                || !host.IsTableInState(checkoutTask.TableId, TavernTableRuntimeState.Checkout))
            {
                if (host == null || !host.TryContinueCheckoutWaiterAsClean(actor))
                {
                    host?.CompleteWaiterCheckoutCancelledStayInPlace(actor);
                }

                yield break;
            }

            host.MarkTableCheckoutInProgress(table, "结账中");
            host.SetWaiterAnimatorSpeed(host.GetWaiterAnimator(waiter), 0f);
            var duration = host.GetEffectiveWaiterCheckoutDuration(waiter);
            host.ShowWaiterTaskProgress(waiter, duration, host.ResolveWaiterCheckoutIcon(checkoutTask.TableId));
            yield return new WaitForSeconds(duration);

            if (host.IsTableInState(checkoutTask.TableId, TavernTableRuntimeState.Checkout))
            {
                host.CompleteCheckoutWithIncome(checkoutTask.TableId, waiter);
            }
            else
            {
                // 读条期间被玩家点结账：优先改清扫，否则原地待命。
                if (!host.TryContinueCheckoutWaiterAsClean(actor))
                {
                    host.CompleteWaiterCheckoutCancelledStayInPlace(actor);
                }

                yield break;
            }

            host.CompleteWaiterTask(actor);
        }
    }

    internal sealed class WaiterStealingState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.Stealing;

        public override IEnumerator Execute(WaiterCharacter actor)
        {
            var host = GetHost(actor);
            var waiter = GetWaiter(actor);
            if (GetTask(actor) is not WaiterCheckoutTask checkoutTask
                || host == null
                || waiter == null
                || !host.TryGetTable(checkoutTask.TableId, out var table)
                || !host.IsTableInState(checkoutTask.TableId, TavernTableRuntimeState.Checkout))
            {
                if (host == null || !host.TryContinueCheckoutWaiterAsClean(actor))
                {
                    host?.CompleteWaiterCheckoutCancelledStayInPlace(actor);
                }

                yield break;
            }

            host.MarkTableCheckoutInProgress(table, "偷钱中。。。");
            host.SetWaiterAnimatorSpeed(host.GetWaiterAnimator(waiter), 0f);
            var duration = host.GetEffectiveWaiterStealDuration();
            host.ShowWaiterClickableTaskProgress(
                waiter,
                duration,
                host.ResolveWaiterStealingIcon(),
                () => host.NotifyWaiterStealStopped(waiter));

            var elapsed = 0f;
            while (elapsed < duration && host.IsTableInState(checkoutTask.TableId, TavernTableRuntimeState.Checkout))
            {
                if (host.HasWaiterStealBeenStopped(waiter))
                {
                    break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            host.ClearWaiterStealProgress(waiter);
            if (!host.IsTableInState(checkoutTask.TableId, TavernTableRuntimeState.Checkout))
            {
                if (!host.TryContinueCheckoutWaiterAsClean(actor))
                {
                    host.CompleteWaiterCheckoutCancelledStayInPlace(actor);
                }

                yield break;
            }

            if (host.HasWaiterStealBeenStopped(waiter))
            {
                host.CompleteCheckoutWithIncome(checkoutTask.TableId, waiter);
            }
            else
            {
                host.CompleteCheckoutWithoutIncome(checkoutTask.TableId);
            }

            if (host.IsWaiterTransitioningToNap(waiter))
            {
                actor.PendingDishPrefab = null;
                actor.CurrentTask = null;
                yield break;
            }

            host.CompleteWaiterTask(actor);
        }
    }

    internal sealed class WaiterMoveToCleanTableState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.MoveToCleanTable;

        public override IEnumerator Execute(WaiterCharacter actor)
        {
            var host = GetHost(actor);
            var waiter = GetWaiter(actor);
            if (GetTask(actor) is not WaiterCleanTask cleanTask
                || host == null
                || waiter == null
                || !host.TryGetTable(cleanTask.TableId, out var table))
            {
                host?.CompleteWaiterTask(actor);
                yield break;
            }

            host.MarkTableCleaningInProgress(table);
            yield return host.MoveWaiterToTable(waiter, table);
            actor.TransitionTo(new WaiterCleaningState());
        }
    }

    internal sealed class WaiterCleaningState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.Cleaning;

        public override IEnumerator Execute(WaiterCharacter actor)
        {
            var host = GetHost(actor);
            var waiter = GetWaiter(actor);
            if (GetTask(actor) is not WaiterCleanTask cleanTask
                || host == null
                || waiter == null
                || !host.TryGetTable(cleanTask.TableId, out var table))
            {
                host?.CompleteWaiterTask(actor);
                yield break;
            }

            var animator = host.GetWaiterAnimator(waiter);
            host.TriggerWaiterCleanAnimation(animator);
            host.SetWaiterAnimatorSpeed(animator, 0f);
            host.PlayCleanAudio(cleanTask.TableId);
            var smoke = host.PlayCleanSmokeEffect(cleanTask.TableId, table);
            var duration = host.GetEffectiveAutoCleanDuration(waiter);
            host.ShowWaiterTaskProgress(waiter, duration, host.ResolveWaiterCleaningIcon());
            yield return new WaitForSeconds(duration);
            host.StopCleanAudio(cleanTask.TableId);
            host.StopCleanSmokeEffect(cleanTask.TableId, smoke);

            if (host.IsTableInState(cleanTask.TableId, TavernTableRuntimeState.Cleaning))
            {
                // 营业中清桌完成扣体力；归零则就地进入偷懒。
                host.ConsumeWaiterStamina(waiter, WaiterStaminaAction.Cleaning);
                // 必须先占打盹桌，再 FinishCleaning：后者会 TryPrepareFrontCounterOrders，
                // 若尚未 BindNap，刚清完的 Idle 桌会被立刻分给排队客人，出现「打盹桌还有人吃饭」。
                host.TryStartWaiterNapAfterCleaning(cleanTask.TableId, waiter);
                host.FinishCleaning(cleanTask.TableId);
            }

            if (host.IsWaiterTransitioningToNap(waiter) || host.IsWaiterNapping(waiter))
            {
                // 进入打盹流程后不要 Reset 回 Movement，否则会在座位上僵站几秒才进 Sleep。
                actor.PendingDishPrefab = null;
                actor.CurrentTask = null;
                yield break;
            }

            host.ResetWaiterAnimation(animator);
            host.CompleteWaiterTask(actor);
        }
    }

    internal sealed class WaiterNappingState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.Napping;
    }

    internal sealed class WaiterReturningHomeState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.ReturningHome;

        public override IEnumerator Execute(WaiterCharacter actor)
        {
            var host = GetHost(actor);
            var waiter = GetWaiter(actor);
            if (host == null || waiter == null)
            {
                yield break;
            }

            yield return host.ReturnWaiterHome(waiter, actor.HomeIndex);
            host.ReleaseTrackedWaiterRoutineReference(waiter);
            actor.SetPassiveState(new WaiterIdleState());
        }
    }

    internal sealed class WaiterMoveToAttractPointState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.MoveToAttractPoint;

        public override IEnumerator Execute(WaiterCharacter actor)
        {
            var host = GetHost(actor);
            var waiter = GetWaiter(actor);
            if (host == null || waiter == null)
            {
                yield break;
            }

            yield return host.MoveWaiterToAttractPoint(waiter);
            if (waiter == null || !host.IsWaiterAttracting(waiter))
            {
                yield break;
            }

            actor.TransitionTo(new WaiterAttractingState());
        }
    }

    internal sealed class WaiterAttractingState : WaiterStateBase
    {
        public override string StateKey => WaiterStateKeys.Attracting;

        public override IEnumerator Execute(WaiterCharacter actor)
        {
            var host = GetHost(actor);
            var waiter = GetWaiter(actor);
            if (host == null || waiter == null)
            {
                yield break;
            }

            host.EnsureWaiterAttractBubble(waiter);

            var arrivalAnimator = host.GetWaiterAnimator(waiter);
            host.TriggerWaiterCleanAnimation(arrivalAnimator);
            host.SetWaiterAnimatorSpeed(arrivalAnimator, 0f);
            PerformAttractWave(host, waiter);

            while (host.IsWaiterAttracting(waiter))
            {
                if (host.ShouldWaiterForceStopAttracting(waiter))
                {
                    // 软清拉客状态即可；StopWaiterAttract 内会 SoftStop，勿依赖自停协程。
                    host.StopWaiterAttract(waiter);
                    yield break;
                }

                var animator = host.GetWaiterAnimator(waiter);
                host.TriggerWaiterCleanAnimation(animator);
                host.SetWaiterAnimatorSpeed(animator, 0f);

                var interval = host.GetWaiterAttractIntervalSeconds();
                yield return new WaitForSeconds(interval);

                if (waiter == null || !host.IsWaiterAttracting(waiter))
                {
                    yield break;
                }

                PerformAttractWave(host, waiter);

                if (host.ShouldWaiterVoluntarilyStopAttracting(waiter))
                {
                    host.StopWaiterAttract(waiter);
                    yield break;
                }
            }

            host.StopWaiterAttract(waiter);
        }

        private static void PerformAttractWave(IWaiterRuntimeHost host, GameObject waiter)
        {
            if (host == null || waiter == null)
            {
                return;
            }

            host.PerformWaiterAttractWave(waiter);
            var animator = host.GetWaiterAnimator(waiter);
            host.ResetWaiterAnimation(animator);
        }
    }
}
