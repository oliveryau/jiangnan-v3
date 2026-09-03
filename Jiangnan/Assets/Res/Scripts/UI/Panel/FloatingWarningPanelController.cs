using System.Collections;
using TMPro;
using UnityEngine;

namespace JN.Client.UI
{
    /// <summary>
    /// 浮动提示的数据载体。
    /// </summary>
    public class FloatingWarningPanelControllerData : QFramework.UIPanelData
    {
        public string Content;
    }

    /// <summary>
    /// 显示短时上浮并自动消失的警告提示。
    /// UIKit 打开时会 SetDefaultSizeOfPanel 拉成全屏，因此每次显示都强制恢复为屏幕中上布局。
    /// </summary>
    public class FloatingWarningPanelController : OverlayPanelController<FloatingWarningPanelControllerData>
    {
        private const float Duration = 2f;
        private const float RiseDistance = 36f;

        // 1080x1920 参考分辨率下的屏幕中上位置（顶锚点 + 向下偏移）。
        private static readonly Vector2 PanelAnchor = new(0.5f, 1f);
        private static readonly Vector2 PanelPivot = new(0.5f, 1f);
        private static readonly Vector2 PanelSize = new(560f, 100f);
        private static readonly Vector2 PanelAnchoredPosition = new(0f, -220f);

        private RectTransform panelRect;
        private CanvasGroup canvasGroup;
        private TMP_Text tipText;
        private Coroutine floatingRoutine;

        protected override void OnPanelInit()
        {
            EnsureNodes();
        }

        protected override void OnPanelOpen(FloatingWarningPanelControllerData data)
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
            RestartFloatingRoutine();
        }

        protected override void OnPanelClose()
        {
            if (floatingRoutine != null)
            {
                StopCoroutine(floatingRoutine);
                floatingRoutine = null;
            }
        }

        private void EnsureNodes()
        {
            panelRect ??= GetComponent<RectTransform>();
            canvasGroup ??= GetComponent<CanvasGroup>();
            tipText ??= ResolveText("txt_Tip");
        }

        /// <summary>
        /// 强制恢复屏幕中上布局，覆盖 UIKit SetDefaultSizeOfPanel 的全屏拉伸。
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

            tipText.text = Data?.Content ?? string.Empty;
            tipText.color = new Color32(255, 245, 204, 255);
            tipText.alignment = TextAlignmentOptions.Center;
            tipText.enableWordWrapping = false;
            tipText.overflowMode = TextOverflowModes.Ellipsis;
        }

        private void RestartFloatingRoutine()
        {
            if (floatingRoutine != null)
            {
                StopCoroutine(floatingRoutine);
            }

            floatingRoutine = StartCoroutine(FloatingRoutine());
        }

        private IEnumerator FloatingRoutine()
        {
            var elapsed = 0f;
            var startPos = panelRect != null ? panelRect.anchoredPosition : PanelAnchoredPosition;

            while (elapsed < Duration)
            {
                elapsed += Time.deltaTime;
                var progress = Mathf.Clamp01(elapsed / Duration);

                if (panelRect != null)
                {
                    panelRect.anchoredPosition = startPos + new Vector2(0f, RiseDistance * progress);
                }

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = progress < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (progress - 0.6f) / 0.4f);
                }

                yield return null;
            }

            floatingRoutine = null;
            CloseSelf();
        }
    }
}
