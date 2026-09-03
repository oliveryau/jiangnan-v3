using System;
using UnityEngine;

namespace JN.Client.UI
{
    /// <summary>
    /// 飞金币到达顶部前暂缓刷新金币数字，到达后由面板播放缩放并写入最新值。
    /// </summary>
    public static class CoinDisplayRefreshCoordinator
    {
        public static event Action GoldRefreshArrived;

        private static int deferCount;

        public static bool ShouldDeferGoldRefresh => deferCount > 0;

        private static int pendingPositiveDisplayDelta;

        public static void DeferGoldRefreshUntilFlyComplete()
        {
            deferCount++;
        }

        /// <summary>
        /// 飞金币期间暂存的正数变化，待全部飞抵后再展示 +xxx。
        /// </summary>
        public static void RegisterPendingPositiveDisplay(int delta)
        {
            if (delta > 0)
            {
                pendingPositiveDisplayDelta += delta;
            }
        }

        /// <summary>
        /// 取出并清零暂存的正数展示量。
        /// </summary>
        public static int ConsumePendingPositiveDisplay()
        {
            var delta = pendingPositiveDisplayDelta;
            pendingPositiveDisplayDelta = 0;
            return delta;
        }

        public static void NotifyFlyComplete()
        {
            deferCount = Mathf.Max(0, deferCount - 1);
            if (deferCount > 0)
            {
                return;
            }

            GoldRefreshArrived?.Invoke();
        }

        public static void Reset()
        {
            deferCount = 0;
            pendingPositiveDisplayDelta = 0;
        }
    }
}
