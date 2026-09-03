using System.Collections.Generic;
using JN.Client.Model;
using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 记录桌位进入小二可接任务状态的时间，供自动派单按等待时长优先。
    /// </summary>
    internal sealed class TavernWaiterTaskWaitTracker
    {
        private readonly Dictionary<int, float> waitStartTimes = new();
        private readonly Dictionary<int, TavernTableRuntimeState> waitStates = new();

        public void ClearAll()
        {
            waitStartTimes.Clear();
            waitStates.Clear();
        }

        public void OnTableWaitStateChanged(int tableId, TavernTableRuntimeState state)
        {
            if (tableId <= 0)
            {
                return;
            }

            if (IsWaiterDispatchWaitState(state))
            {
                if (!waitStates.TryGetValue(tableId, out var previousState) || previousState != state)
                {
                    waitStartTimes[tableId] = Time.time;
                    waitStates[tableId] = state;
                }

                return;
            }

            waitStartTimes.Remove(tableId);
            waitStates.Remove(tableId);
        }

        public float GetWaitDuration(int tableId)
        {
            if (tableId <= 0 || !waitStartTimes.TryGetValue(tableId, out var startTime))
            {
                return 0f;
            }

            return Mathf.Max(0f, Time.time - startTime);
        }

        private static bool IsWaiterDispatchWaitState(TavernTableRuntimeState state)
        {
            return state == TavernTableRuntimeState.WaitingOrder
                   || state == TavernTableRuntimeState.WaitingServe
                   || state == TavernTableRuntimeState.Checkout
                   || state == TavernTableRuntimeState.Cleaning;
        }
    }
}
