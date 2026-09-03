using System;
using TMPro;
using UnityEngine.UI;

namespace JN.Client.UI
{
    /// <summary>
    /// 通用二次确认 / 信息弹窗数据。
    /// </summary>
    public class RuntimeInfoPanelControllerData : QFramework.UIPanelData
    {
        public string Title;
        public string Content;
        /// <summary>确认回调；为空时点确认仅关闭。</summary>
        public Action OnConfirm;
    }

    /// <summary>
    /// 通用二次确认弹窗：txt_Title / txt_Content / btn_Close / btn_Confirm。
    /// </summary>
    public class RuntimeInfoPanelController : OverlayPanelController<RuntimeInfoPanelControllerData>
    {
        private TMP_Text titleText;
        private TMP_Text contentText;
        private Button closeButton;
        private Button confirmButton;

        protected override void OnPanelInit()
        {
            EnsureNodes();
            BindButton(closeButton, CloseSelf);
            BindButton(confirmButton, OnClickConfirm);
        }

        protected override void OnPanelOpen(RuntimeInfoPanelControllerData data)
        {
            EnsureNodes();
            BindButton(closeButton, CloseSelf);
            BindButton(confirmButton, OnClickConfirm);
            RefreshPanel();
        }

        protected override void OnPanelShow()
        {
            RefreshPanel();
        }

        private void EnsureNodes()
        {
            titleText ??= ResolveText("Panel/txt_Title", "txt_Title");
            contentText ??= ResolveText("Panel/txt_Content", "txt_Content");
            closeButton ??= ResolveButton("Panel/btn_Close", "btn_Close");
            confirmButton ??= ResolveButton("Panel/btn_Confirm", "btn_Confirm");
        }

        private void RefreshPanel()
        {
            if (titleText != null)
            {
                titleText.text = Data != null ? Data.Title : string.Empty;
            }

            if (contentText != null)
            {
                contentText.text = Data != null ? Data.Content : string.Empty;
            }

            if (confirmButton != null && !confirmButton.gameObject.activeSelf)
            {
                confirmButton.gameObject.SetActive(true);
            }
        }

        private void OnClickConfirm()
        {
            var onConfirm = Data?.OnConfirm;
            CloseSelf();
            onConfirm?.Invoke();
        }
    }
}
