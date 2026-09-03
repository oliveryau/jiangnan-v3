using System;
using System.Collections;
using UnityEngine;

namespace JN.Client.UI
{
    /// <summary>
    /// 功能解锁提示的数据载体。
    /// </summary>
    public class NewFeatureOpenToastPanelControllerData : QFramework.UIPanelData
    {
        public float Duration = 2f;
        public Action OnComplete;
    }

    /// <summary>
    /// 显示一段时间后自动关闭的功能解锁提示。
    /// </summary>
    public class NewFeatureOpenToastPanelController : OverlayPanelController<NewFeatureOpenToastPanelControllerData>
    {
        private Coroutine closeRoutine;

        /// <summary>
        /// 面板显示时启动自动关闭协程。
        /// </summary>
        protected override void OnPanelShow()
        {
            RestartCloseRoutine();
        }

        /// <summary>
        /// 面板关闭时停止自动关闭协程。
        /// </summary>
        protected override void OnPanelClose()
        {
            if (closeRoutine != null)
            {
                StopCoroutine(closeRoutine);
                closeRoutine = null;
            }
        }

        /// <summary>
        /// 重启自动关闭计时。
        /// </summary>
        private void RestartCloseRoutine()
        {
            if (closeRoutine != null)
            {
                StopCoroutine(closeRoutine);
            }

            closeRoutine = StartCoroutine(CloseAfterDelay());
        }

        /// <summary>
        /// 等待指定时间后关闭提示并回调完成事件。
        /// </summary>
        private IEnumerator CloseAfterDelay()
        {
            yield return new UnityEngine.WaitForSeconds(Data.Duration <= 0f ? 2f : Data.Duration);
            closeRoutine = null;
            var onComplete = Data.OnComplete;
            CloseSelf();
            onComplete?.Invoke();
        }
    }
}
