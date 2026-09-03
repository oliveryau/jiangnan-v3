using System;
using System.Collections.Generic;
using JN.Client;
using JN.Client.Config;
using JN.Client.Model;
using QFramework;
using UnityEngine;

namespace JN.Client.Manager
{
    public partial class DataManager
    {
        /// <summary>待卸客类型：普通。</summary>
        public const int PulledCustomerKindNormal = 0;
        /// <summary>待卸客类型：稀客。</summary>
        public const int PulledCustomerKindRare = 1;
        /// <summary>待卸客类型：贵客。</summary>
        public const int PulledCustomerKindVip = 2;

        /// <summary>轿子最大拉客容量（Config jiaoziCapacity）。</summary>
        public int GetJiaoziCapacity()
        {
            return TbConfigRuntime.GetJiaoziCapacity(10);
        }

        /// <summary>拉回自家后进店间隔（秒）。已改为一次卸完，保留供配置/旧调用兼容。</summary>
        public float GetOtherTavernPullEnterInterval()
        {
            return TbConfigRuntime.GetOtherTavernPullEnterInterval(2f);
        }

        /// <summary>进入他人酒楼时初始排队人数。</summary>
        public int GetOtherTavernInitialQueueCount()
        {
            return TbConfigRuntime.GetOtherTavernInitialQueueCount(5);
        }

        /// <summary>当前已占用轿子容量。</summary>
        public int GetJiaoziUsedCapacity()
        {
            EnsureJiaoziPullDefaults();
            return Mathf.Clamp(SaveData.gameplay.jiaoziUsedCapacity, 0, GetJiaoziCapacity());
        }

        /// <summary>轿子剩余容量。</summary>
        public int GetJiaoziRemainingCapacity()
        {
            return Mathf.Max(0, GetJiaoziCapacity() - GetJiaoziUsedCapacity());
        }

        /// <summary>容量是否已满（剩余不足以再拉任何客人）。</summary>
        public bool IsJiaoziCapacityFull()
        {
            return GetJiaoziRemainingCapacity() <= 0;
        }

        /// <summary>
        /// 将贵客/稀客标记转为待卸客类型。
        /// </summary>
        public static int ResolvePulledCustomerKind(bool isVip, bool isRare)
        {
            if (isVip)
            {
                return PulledCustomerKindVip;
            }

            if (isRare)
            {
                return PulledCustomerKindRare;
            }

            return PulledCustomerKindNormal;
        }

        /// <summary>
        /// 计算拉客占用容量：普通 1，稀客 2，贵客 3。
        /// </summary>
        /// <summary>
        /// 计算拉客占用容量：任意客人均占 1。
        /// </summary>
        public static int ResolvePullCapacityCost(bool isVip, bool isRare)
        {
            return ResolvePullCapacityCostByKind(ResolvePulledCustomerKind(isVip, isRare));
        }

        /// <summary>按待卸客类型取轿子占用：任意类型均占 1。</summary>
        public static int ResolvePullCapacityCostByKind(int kind)
        {
            return 1;
        }

        /// <summary>指定占用是否还能装进轿子。</summary>
        public bool CanPullWithCapacityCost(int capacityCost)
        {
            return capacityCost > 0 && GetJiaoziRemainingCapacity() >= capacityCost;
        }

        /// <summary>
        /// 拉客成功：按客人类型立刻写入待卸队列（与离桌走路表现无关）。
        /// </summary>
        public bool TryPullCustomerOntoJiaozi(int kind)
        {
            if (!IsVisitingOtherTavern)
            {
                return false;
            }

            kind = NormalizePulledCustomerKind(kind);
            var capacityCost = ResolvePullCapacityCostByKind(kind);
            if (!CanPullWithCapacityCost(capacityCost))
            {
                return false;
            }

            EnsureJiaoziPullDefaults();
            SaveData.gameplay.pendingPulledCustomerKinds.Add(kind);
            RecalcJiaoziUsedFromPending();
            SaveGame();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            return true;
        }

        /// <summary>兼容：由贵客/稀客布尔写入待卸队列。</summary>
        public bool TryPullCustomerOntoJiaozi(bool isVip, bool isRare)
        {
            return TryPullCustomerOntoJiaozi(ResolvePulledCustomerKind(isVip, isRare));
        }

        public int GetPendingPulledCustomerCount()
        {
            EnsureJiaoziPullDefaults();
            return SaveData.gameplay.pendingPulledCustomerKinds.Count;
        }

        /// <summary>复制待卸客类型列表（卸客进店用，不消费容量）。</summary>
        public List<int> GetPendingPulledCustomerKindsCopy()
        {
            EnsureJiaoziPullDefaults();
            return new List<int>(SaveData.gameplay.pendingPulledCustomerKinds);
        }

        /// <summary>查看队首待卸客类型，不消费。</summary>
        public bool TryPeekPendingPulledCustomerKind(out int kind)
        {
            EnsureJiaoziPullDefaults();
            var list = SaveData.gameplay.pendingPulledCustomerKinds;
            if (list.Count <= 0)
            {
                kind = PulledCustomerKindNormal;
                return false;
            }

            kind = NormalizePulledCustomerKind(list[0]);
            return true;
        }

        /// <summary>自家进店消耗 1 个待拉客人并释放对应容量；成功返回 true。</summary>
        public bool TryConsumePendingPulledCustomer(out int kind)
        {
            EnsureJiaoziPullDefaults();
            var list = SaveData.gameplay.pendingPulledCustomerKinds;
            if (list.Count <= 0)
            {
                kind = PulledCustomerKindNormal;
                return false;
            }

            kind = NormalizePulledCustomerKind(list[0]);
            list.RemoveAt(0);
            RecalcJiaoziUsedFromPending();
            SaveGame();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            return true;
        }

        /// <summary>卸客刷人失败时，把类型插回队首，避免丢客。</summary>
        public void RequeuePulledCustomerAtFront(int kind)
        {
            EnsureJiaoziPullDefaults();
            SaveData.gameplay.pendingPulledCustomerKinds.Insert(0, NormalizePulledCustomerKind(kind));
            RecalcJiaoziUsedFromPending();
            SaveGame();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
        }

        /// <summary>兼容旧调用：只消费不关心类型。</summary>
        public bool TryConsumePendingPulledCustomer()
        {
            return TryConsumePendingPulledCustomer(out _);
        }

        /// <summary>
        /// 卸客收尾：清空待卸队列并使轿子容量归零。
        /// </summary>
        public void ClearAllPendingPulledCustomers()
        {
            EnsureJiaoziPullDefaults();
            SaveData.gameplay.pendingPulledCustomerKinds.Clear();
            SaveData.gameplay.pendingPulledCustomerCapacities?.Clear();
            RecalcJiaoziUsedFromPending();
            SaveGame();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
        }

        /// <summary>
        /// 回店卸客完成后开启拉客冷却（真实 UTC 时间）。
        /// </summary>
        public void StartPullCustomerCooldown()
        {
            EnsureGameplayDefaults();
            var duration = TbConfigRuntime.GetPullCustomerCooldownSeconds(300f);
            if (duration <= 0f)
            {
                SaveData.gameplay.pullCustomerCooldownEndUnixTime = 0d;
                SaveGame();
                Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
                return;
            }

            SaveData.gameplay.pullCustomerCooldownEndUnixTime = GetPullUtcNowSeconds() + duration;
            SaveGame();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
        }

        /// <summary>拉客冷却剩余秒数（真实时间）；0 表示可拉客。</summary>
        public float GetPullCustomerCooldownRemainingSeconds()
        {
            EnsureGameplayDefaults();
            var end = SaveData.gameplay.pullCustomerCooldownEndUnixTime;
            if (end <= 0d)
            {
                return 0f;
            }

            var remaining = end - GetPullUtcNowSeconds();
            if (remaining <= 0d)
            {
                if (SaveData.gameplay.pullCustomerCooldownEndUnixTime != 0d)
                {
                    SaveData.gameplay.pullCustomerCooldownEndUnixTime = 0d;
                    SaveGame();
                }

                return 0f;
            }

            return (float)remaining;
        }

        /// <summary>拉客冷却是否已结束。</summary>
        public bool IsPullCustomerCooldownReady()
        {
            return GetPullCustomerCooldownRemainingSeconds() <= 0f;
        }

        private static double GetPullUtcNowSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
        }

        private void EnsureJiaoziPullDefaults()
        {
            EnsureGameplayDefaults();
            SaveData.gameplay.pendingPulledCustomerKinds ??= new List<int>();
            SaveData.gameplay.pendingPulledCustomerCapacities ??= new List<int>();

            // 旧存档：只有容量队列 → 反推类型写入 kinds。
            if (SaveData.gameplay.pendingPulledCustomerKinds.Count == 0
                && SaveData.gameplay.pendingPulledCustomerCapacities.Count > 0)
            {
                for (var index = 0; index < SaveData.gameplay.pendingPulledCustomerCapacities.Count; index++)
                {
                    SaveData.gameplay.pendingPulledCustomerKinds.Add(
                        KindFromLegacyCapacity(SaveData.gameplay.pendingPulledCustomerCapacities[index]));
                }

                SaveData.gameplay.pendingPulledCustomerCapacities.Clear();
            }

            // 更旧：只有人数 → 全按普通客。
            if (SaveData.gameplay.pendingPulledCustomerKinds.Count == 0
                && SaveData.gameplay.pendingPulledCustomerCount > 0)
            {
                for (var index = 0; index < SaveData.gameplay.pendingPulledCustomerCount; index++)
                {
                    SaveData.gameplay.pendingPulledCustomerKinds.Add(PulledCustomerKindNormal);
                }
            }

            RecalcJiaoziUsedFromPending();
        }

        private static int KindFromLegacyCapacity(int capacity)
        {
            if (capacity >= 3)
            {
                return PulledCustomerKindVip;
            }

            if (capacity >= 2)
            {
                return PulledCustomerKindRare;
            }

            return PulledCustomerKindNormal;
        }

        private static int NormalizePulledCustomerKind(int kind)
        {
            return kind switch
            {
                PulledCustomerKindVip => PulledCustomerKindVip,
                PulledCustomerKindRare => PulledCustomerKindRare,
                _ => PulledCustomerKindNormal
            };
        }

        private void RecalcJiaoziUsedFromPending()
        {
            var list = SaveData.gameplay.pendingPulledCustomerKinds;
            var sum = 0;
            if (list != null)
            {
                for (var index = 0; index < list.Count; index++)
                {
                    sum += ResolvePullCapacityCostByKind(NormalizePulledCustomerKind(list[index]));
                }
            }

            SaveData.gameplay.jiaoziUsedCapacity = sum;
            SaveData.gameplay.pendingPulledCustomerCount = list?.Count ?? 0;
            // 与 kinds 同步清空旧容量队列，避免重复迁移。
            SaveData.gameplay.pendingPulledCustomerCapacities?.Clear();
        }

        /// <summary>
        /// 他人酒楼拜访桌快照：按地块静态缓存，场景重建不丢，进程重启 / 开档 / 卸客完成清空。
        /// 不写入 GameSaveData，避免 SaveGame 带进下次开档。
        /// </summary>
        private static readonly Dictionary<int, OtherTavernVisitSnapshot> s_otherTavernVisitSnapshots = new();

        /// <summary>按地块覆盖写入他人酒楼拜访快照。</summary>
        public void SaveOtherTavernVisitSnapshot(OtherTavernVisitSnapshot snapshot)
        {
            if (snapshot == null || snapshot.tileId <= 0)
            {
                return;
            }

            snapshot.tables ??= new List<OtherTavernVisitTableSnapshot>();
            s_otherTavernVisitSnapshots[snapshot.tileId] = snapshot;
        }

        /// <summary>读取指定地块的他人酒楼拜访快照。</summary>
        public bool TryGetOtherTavernVisitSnapshot(int tileId, out OtherTavernVisitSnapshot snapshot)
        {
            snapshot = null;
            if (tileId <= 0)
            {
                return false;
            }

            return s_otherTavernVisitSnapshots.TryGetValue(tileId, out snapshot) && snapshot != null;
        }

        /// <summary>清空全部他人酒楼拜访快照（重开游戏或单次拉客卸客完成）。</summary>
        public static void ClearAllOtherTavernVisitSnapshots()
        {
            s_otherTavernVisitSnapshots.Clear();
        }
    }
}
