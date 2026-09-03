using System.Collections;
using TMPro;
using UnityEngine;

namespace JN.Client.UI
{
    /// <summary>
    /// 菜单切换成功后的短时提示数据。
    /// </summary>
    public class SwitchMenuTipsPanelControllerData : QFramework.UIPanelData
    {
        public bool VipMenu;
    }

    /// <summary>
    /// 翻牌切换菜单并关闭弹窗后，屏幕上方展示 3 秒后自动消失。
    /// </summary>
    public class SwitchMenuTipsPanelController : OverlayPanelController<SwitchMenuTipsPanelControllerData>
    {
        private const float DisplayDurationSeconds = 3f;

        private static readonly Vector2 PanelAnchor = new(0.5f, 1f);
        private static readonly Vector2 PanelPivot = new(0.5f, 1f);
        private static readonly Vector2 PanelSize = new(560f, 100f);
        private static readonly Vector2 PanelAnchoredPosition = new(0f, -500f);

        private RectTransform panelRect;
        private RectTransform rootRect;
        private CanvasGroup canvasGroup;
        private TMP_Text tipText;
        private Coroutine closeRoutine;

        protected override void OnPanelInit()
        {
            EnsureNodes();
        }

        protected override void OnPanelOpen(SwitchMenuTipsPanelControllerData data)
        {
            EnsureNodes();
            ApplyPanelLayout();
            ApplyContent();
        }

        protected override void OnPanelShow()
        {
            EnsureNodes();
            ApplyPanelLayout();
            ApplyContent();
            RestartCloseRoutine();
        }

        protected override void OnPanelClose()
        {
            if (closeRoutine != null)
            {
                StopCoroutine(closeRoutine);
                closeRoutine = null;
            }
        }

        private void EnsureNodes()
        {
            panelRect ??= GetComponent<RectTransform>();
            rootRect ??= ResolveTransform("Root", "Root") as RectTransform;
            canvasGroup ??= GetComponent<CanvasGroup>();
            tipText ??= ResolveText("Root/txt_Tip", "txt_Tip");
        }

        /// <summary>
        /// 恢复预制体顶锚布局，覆盖 UIKit SetDefaultSizeOfPanel 的全屏拉伸。
        /// </summary>
        private void ApplyPanelLayout()
        {
            if (panelRect == null)
            {
                return;
            }

            panelRect.anchorMin = PanelAnchor;
            panelRect.anchorMax = PanelAnchor;
            panelRect.pivot = PanelPivot;
            panelRect.sizeDelta = PanelSize;
            panelRect.anchoredPosition = PanelAnchoredPosition;
            panelRect.localScale = Vector3.one;

            // Root 承载背景与文案，保持预制体居中缩放，避免被 UIKit 全屏拉伸带偏。
            if (rootRect != null)
            {
                rootRect.anchorMin = new Vector2(0.5f, 0.5f);
                rootRect.anchorMax = new Vector2(0.5f, 0.5f);
                rootRect.pivot = new Vector2(0.5f, 0.5f);
                rootRect.anchoredPosition = Vector2.zero;
            }
        }

        private void ApplyContent()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            if (tipText == null)
            {
                return;
            }

            var vipMenu = Data != null && Data.VipMenu;
            tipText.text = vipMenu ? "切换为贵客菜单" : "切换为大众菜单";
            tipText.alignment = TextAlignmentOptions.Center;
            tipText.enableWordWrapping = false;
            tipText.overflowMode = TextOverflowModes.Ellipsis;
        }

        private void RestartCloseRoutine()
        {
            if (closeRoutine != null)
            {
                StopCoroutine(closeRoutine);
            }

            closeRoutine = StartCoroutine(CloseAfterDelay());
        }

        private IEnumerator CloseAfterDelay()
        {
            yield return new WaitForSecondsRealtime(DisplayDurationSeconds);
            closeRoutine = null;
            CloseSelf();
        }
    }
}
