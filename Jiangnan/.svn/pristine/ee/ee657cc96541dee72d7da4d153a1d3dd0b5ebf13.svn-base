using System.Collections;
using System;
using JN.Client.Model;
using UnityEngine;

namespace JN.Client.Scene
{
    internal enum WaiterStaminaAction
    {
        Ordering,
        NotifyChef,
        Serving,
        Checkout,
        Cleaning
    }

    /// <summary>
    /// 小二状态机对 Tavern 场景门面的最小依赖接口。
    /// </summary>
    internal interface IWaiterRuntimeHost
    {
        bool IsBusinessOpen { get; }

        GameObject GetAvailableWaiterForTask(WaiterTask task, bool ignoreSkillGate = false);

        bool TryStartWaiterTask(GameObject waiter, WaiterTask task, ICharacterState<WaiterCharacter> initialState);

        WaiterCharacter GetOrCreateWaiterContext(GameObject waiter);

        void StartWaiterStateRoutine(WaiterCharacter context, IEnumerator routine);

        void StopTrackedWaiterRoutine(GameObject waiter);

        void ReleaseTrackedWaiterRoutineReference(GameObject waiter);

        void CompleteWaiterTask(WaiterCharacter context);

        /// <summary>玩家点结账导致结账任务作废：优先改接清扫，否则原地待命。</summary>
        void CompleteWaiterCheckoutCancelledStayInPlace(WaiterCharacter context);

        /// <summary>结账桌已变清理时，把该小二无缝切到清扫任务。</summary>
        bool TryContinueCheckoutWaiterAsClean(WaiterCharacter context);

        void ApplyWaiterPresentation(GameObject waiter, string stateKey);

        void EnsureWaiterAnimationReceiver(GameObject waiter);

        bool TryGetTable(int tableId, out TableArea table);

        bool IsTableInState(int tableId, TavernTableRuntimeState state);

        IEnumerator MoveWaiterToTable(GameObject waiter, TableArea table);

        /// <summary>小二前往前台柜台点单（缺省回退到队首排队点）。</summary>
        IEnumerator MoveWaiterToCounter(GameObject waiter);

        IEnumerator MoveWaiterToDishPickup(GameObject waiter);

        IEnumerator ReturnWaiterHome(GameObject waiter, int waiterIndex);

        int ResolveWaiterHomeIndex(GameObject waiter);

        Animator GetWaiterAnimator(GameObject waiter);

        void SetWaiterAnimatorSpeed(Animator animator, float speed);

        void ResetWaiterAnimation(Animator animator);

        void TriggerWaiterCleanAnimation(Animator animator);

        void ConsumeWaiterStamina(GameObject waiter, WaiterStaminaAction action);

        float GetEffectiveWaiterOrderDuration(GameObject waiter);

        float GetEffectiveWaiterCheckoutDuration(GameObject waiter);

        float GetEffectiveWaiterServeDuration(GameObject waiter);

        float GetEffectiveWaiterStealDuration();

        float GetEffectiveAutoCleanDuration(GameObject waiter);

        void HideWaitingOrderDisplay(TableArea table);

        void HideCheckoutDisplay(TableArea table);

        void SealCustomerWaitOnWaiterArrival(int tableId, CustomerWaitHudState waitState);

        void ShowWaiterTaskProgress(GameObject waiter, float duration, Sprite icon);

        GameObject ShowWaiterClickableTaskProgress(GameObject waiter, float duration, Sprite icon, Action onClick);

        Sprite ResolveWaiterOrderingIcon();

        Sprite ResolveWaiterServingIcon();

        Sprite ResolveWaiterCheckoutIcon(int tableId);

        Sprite ResolveWaiterStealingIcon();

        Sprite ResolveWaiterNotifyChefIcon();

        Sprite ResolveWaiterCleaningIcon();

        bool ShouldWaiterStealBeforeCheckout(GameObject waiter, int tableId);

        bool HasWaiterStealBeenStopped(GameObject waiter);

        void NotifyWaiterStealStopped(GameObject waiter);

        void ClearWaiterStealProgress(GameObject waiter);

        void ResetWaiterStealCooldown(GameObject waiter);

        void MarkTableCheckoutInProgress(TableArea table, string customText);

        void CompleteCheckoutWithIncome(int tableId, GameObject waiter);

        void CompleteCheckoutWithoutIncome(int tableId);

        bool IsWaiterTransitioningToNap(GameObject waiter);

        bool IsWaiterNapping(GameObject waiter);

        bool TryStartWaiterNapAfterCleaning(int tableId, GameObject preferredWaiter);

        void CompleteTableOrderByWaiter(int tableId, TableArea table, Sprite orderIcon);

        void NotifyChefCookOrderTicket(int tableId);

        bool ShouldWaiterStealWhileCooking(GameObject waiter, int tableId);

        void ShowWaiterOrderCookProgress(GameObject waiter, int tableId, Sprite icon);

        void ShowWaiterCookStealingProgress(GameObject waiter, int tableId);

        void ClearWaiterOrderCookProgress(GameObject waiter);

        bool HasWaiterCookStealBeenStopped(GameObject waiter);

        void NotifyWaiterCookStealStopped(GameObject waiter);

        bool HasAvailablePreparedDishForServe(int tableId);

        void ReleaseReservedServeDish();

        GameObject TakePreparedDishPrefab();

        void ReturnPreparedDishPrefab(GameObject dishPrefab);

        GameObject TakePreparedDishForWaiter(GameObject waiter);

        void ReturnWaiterCarryDish(GameObject waiter, GameObject dishPrefab);

        void ClearWaiterCarryPlate(GameObject waiter);

        void TransitionWaiterOrderAssignmentToServe(GameObject waiter, int tableId);

        void ServeTableByWaiter(int tableId, TableArea table, GameObject dishPrefab);

        void MarkTableCleaningInProgress(TableArea table);

        GameObject PlayCleanSmokeEffect(int tableId, TableArea table);

        void StopCleanSmokeEffect(int tableId, GameObject smokeEffect);

        void FinishCleaning(int tableId);

        void PlayCleanAudio(int tableId);

        void StopCleanAudio(int tableId);

        bool IsWaiterAttracting(GameObject waiter);

        bool ShouldWaiterForceStopAttracting(GameObject waiter);

        bool ShouldWaiterVoluntarilyStopAttracting(GameObject waiter);

        IEnumerator MoveWaiterToAttractPoint(GameObject waiter);

        bool TrySpawnAttractCustomers(out int spawnedCustomerCount);

        void RecordWaiterAttractSpawn(GameObject waiter, int spawnedCustomerCount);

        float GetWaiterAttractIntervalSeconds();

        void PerformWaiterAttractWave(GameObject waiter);

        void EnsureWaiterAttractBubble(GameObject waiter);

        void StopWaiterAttract(GameObject waiter);
    }
}
