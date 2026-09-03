using System;
using System.Collections.Generic;

namespace JN.Client.Model
{
    /// <summary>
    /// 自家酒楼营业中离店时的运行时快照（冻结语义：存 elapsed + duration）。
    /// </summary>
    [Serializable]
    public class TavernRuntimeSnapshotSaveData
    {
        public bool valid;
        public float nextCustomerSpawnRemaining = -1f;
        /// <summary>本轮营业已过秒数。</summary>
        public float businessOpenElapsedSeconds;
        /// <summary>本轮经营剩余秒数（与顶栏倒计时一致；冻结后续跑）。</summary>
        public float businessRemainingSeconds = -1f;
        public List<TavernTablePhaseSnapshot> tables = new();
        public List<TavernFrontOrderSnapshot> frontOrders = new();
        /// <summary>进店但尚未入座、且未绑前台点单桌的客人（含高峰排队）。</summary>
        public List<TavernUnseatedCustomerSnapshot> unseatedQueue = new();
        /// <summary>高峰分批还剩未刷出的人数。</summary>
        public int peakSpawnRemainingGuests;
        public bool peakSpawnBatchActive;
        public float peakSpawnBatchCooldown;
        public int peakSpawnActiveCapacityOverride;
    }

    /// <summary>
    /// 未入座客人快照：类型 + 是否还在等贵客楼层选择。
    /// </summary>
    [Serializable]
    public class TavernUnseatedCustomerSnapshot
    {
        /// <summary>0 普通 / 1 稀客 / 2 贵客，与 DataManager 拉客 kind 一致。</summary>
        public int kind;
        public bool awaitingVipFloorChoice;
    }

    /// <summary>
    /// 单桌阶段快照：桌态、阶段计时、入座/绑队人数、厨工单。
    /// </summary>
    [Serializable]
    public class TavernTablePhaseSnapshot
    {
        public int tableId;
        public int runtimeState = (int)TavernTableRuntimeState.Idle;
        /// <summary>当前阶段已过秒（冻结后用 duration - elapsed 续跑）。</summary>
        public float stateElapsed;
        /// <summary>当前阶段总时长（秒）；无计时阶段可为 0。</summary>
        public float stateDuration;
        /// <summary>该桌要重建的入座人数（上限座位数）。</summary>
        public int seatedCount;
        /// <summary>前台已绑未入座人数（WaitingOrder / Reserved）。</summary>
        public int queuedBoundCount;

        public bool hasCookTicket;
        public float cookElapsed;
        public float cookDuration;
        public bool isChefNotified;
        public bool isCooking;
    }

    /// <summary>
    /// 前台点单进度快照。
    /// </summary>
    [Serializable]
    public class TavernFrontOrderSnapshot
    {
        public int tableId;
        public float orderElapsed;
        public float orderDuration;
        public int boundCount;
    }
}
