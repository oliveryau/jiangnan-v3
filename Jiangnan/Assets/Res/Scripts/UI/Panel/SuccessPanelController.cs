using System;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    public class SuccessPanelControllerData : QFramework.UIPanelData
    {
        public string Headline = "祝贺！祝贺！";
        public string Message = string.Empty;
        public string ButtonText = "知道了";
        public Action OnClosed;
        /// <summary>GameplaySuccessToastService 入队时附带的外部回调。</summary>
        public Action ExtraOnClosed;
    }

    /// <summary>
    /// 通用成功/解锁弹窗（基于 UI/Panel/SuccessPanelController 预制体）。
    /// </summary>
    public class SuccessPanelController : OverlayPanelController<SuccessPanelControllerData>
    {
        private Button maskButton;
        private Button confirmButton;
        private TMP_Text headlineText;
        private TMP_Text messageText;
        private TMP_Text confirmButtonText;

        protected override void OnPanelInit()
        {
            EnsureNodes();
            BindButton(maskButton, CloseWithCallback);
            BindButton(confirmButton, CloseWithCallback);
        }

        protected override void OnPanelOpen(SuccessPanelControllerData data)
        {
            EnsureNodes();
            ApplyContent();
        }

        protected override void OnPanelShow()
        {
            EnsureNodes();
            ApplyContent();
        }

        protected override void OnPanelClose()
        {
            var callback = Data?.OnClosed;
            if (Data != null)
            {
                Data.OnClosed = null;
            }

            if (callback == null)
            {
                return;
            }

            // 队列续播需在 Close 结束后再开下一个面板，避免 Loader 为空 NRE。
            ActionKit.NextFrame(callback).StartGlobal();
        }

        private void EnsureNodes()
        {
            var panel = ResolveTransform("Panel");
            maskButton ??= panel != null ? panel.GetComponent<Button>() : null;
            confirmButton ??= ResolveButton("Panel/Button", "Button");
            confirmButtonText ??= confirmButton != null
                ? confirmButton.GetComponentInChildren<TMP_Text>(true)
                : null;

            ResolveContentTexts(out headlineText, out messageText);
        }

        private void ResolveContentTexts(out TMP_Text headline, out TMP_Text message)
        {
            headline = headlineText;
            message = messageText;
            if (headline != null && message != null)
            {
                return;
            }

            var panel = ResolveTransform("Panel");
            if (panel == null)
            {
                return;
            }

            for (var index = 0; index < panel.childCount; index++)
            {
                var child = panel.GetChild(index);
                if (child == null || child.name != "Image")
                {
                    continue;
                }

                var texts = child.GetComponentsInChildren<TMP_Text>(true);
                if (texts == null || texts.Length == 0)
                {
                    continue;
                }

                headline ??= texts[0];
                if (texts.Length > 1)
                {
                    message ??= texts[1];
                }

                headlineText = headline;
                messageText = message;
                return;
            }
        }

        private void ApplyContent()
        {
            if (Data == null)
            {
                return;
            }

            if (headlineText != null)
            {
                headlineText.text = string.IsNullOrWhiteSpace(Data.Headline) ? "祝贺！祝贺！" : Data.Headline;
            }

            if (messageText != null)
            {
                messageText.text = Data.Message ?? string.Empty;
                messageText.gameObject.SetActive(!string.IsNullOrWhiteSpace(Data.Message));
            }

            if (confirmButtonText != null)
            {
                confirmButtonText.text = string.IsNullOrWhiteSpace(Data.ButtonText) ? "知道了" : Data.ButtonText;
            }
        }

        private void CloseWithCallback()
        {
            CloseSelf();
        }
    }
}
