using DG.Tweening;
using JN.Client.Manager;
using TMPro;
using UnityEngine;

namespace JN.Client.UI
{
    /// <summary>
    /// 高峰期提示数据（文案固定，预留扩展）。
    /// </summary>
    public class PeakTimeWarningPanelControllerData : QFramework.UIPanelData
    {
        public string Content = "限时客流+200%";
    }

    /// <summary>
    /// 高峰期到来提示：先在屏幕中间大图展示 1 秒，再缩放到 1 并移到 y=-300，停留约 10 秒后关闭。
    /// </summary>
    public class PeakTimeWarningPanelController : OverlayPanelController<PeakTimeWarningPanelControllerData>
    {
        private const string DefaultTipText = "限时客流+200%";

        // 顶锚：初始 Y=-800 约屏幕中间；收拢后 Y=-300。
        private static readonly Vector2 PanelAnchor = new(0.5f, 1f);
        private static readonly Vector2 PanelPivot = new(0.5f, 1f);
        private static readonly Vector2 PanelSize = new(560f, 100f);

        [SerializeField] private float appearHoldSeconds = 1f;
        [SerializeField] private float settleAnimDuration = 0.45f;
        [SerializeField] private float stayAtTargetSeconds = 10f;
        [SerializeField] private float startScale = 1.8f;
        [SerializeField] private float startAnchoredY = -800f;
        [SerializeField] private float settledAnchoredY = -300f;

        private RectTransform panelRect;
        private CanvasGroup canvasGroup;
        private TMP_Text tipText;
        private Tween panelTween;

        protected override void OnPanelInit()
        {
            EnsureNodes();
        }

        protected override void OnPanelOpen(PeakTimeWarningPanelControllerData data)
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
            // 高峰提示弹出瞬间播小二吆喝（开业/升星共用此面板）。
            GameAudioManager.PlayPeakTimeWaiterShout();
            PlayAppearSettleSequence();
        }

        protected override void OnPanelClose()
        {
            KillPanelTween();
        }

        private void EnsureNodes()
        {
            panelRect ??= GetComponent<RectTransform>();
            canvasGroup ??= GetComponent<CanvasGroup>();
            tipText ??= ResolveText("txt_Tip", "txt_Tip");
        }

        /// <summary>
        /// 强制恢复顶锚布局（初始居中 Y=-800），覆盖 UIKit SetDefaultSizeOfPanel 的全屏拉伸。
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
            panelRect.anchoredPosition = new Vector2(0f, startAnchoredY);
            panelRect.localScale = Vector3.one * startScale;
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

            var content = Data != null && !string.IsNullOrWhiteSpace(Data.Content)
                ? Data.Content
                : DefaultTipText;
            tipText.text = content;
            tipText.alignment = TextAlignmentOptions.Center;
            tipText.enableWordWrapping = false;
            tipText.overflowMode = TextOverflowModes.Ellipsis;
        }

        /// <summary>
        /// 出现 hold → DOScale(1) + Y 移到 settled → 停留 → 关闭。
        /// Scale 用 Transform 动画（醒目），位置用 AnchoredPosition（UI 布局坐标）。
        /// </summary>
        private void PlayAppearSettleSequence()
        {
            KillPanelTween();
            if (panelRect == null)
            {
                CloseSelf();
                return;
            }

            panelRect.localScale = Vector3.one * startScale;
            panelRect.anchoredPosition = new Vector2(0f, startAnchoredY);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            var settledPos = new Vector2(0f, settledAnchoredY);
            var animDuration = Mathf.Max(0.01f, settleAnimDuration);
            panelTween = DOTween.Sequence()
                .SetUpdate(true)
                .AppendInterval(Mathf.Max(0f, appearHoldSeconds))
                .Append(panelRect.DOScale(1f, animDuration).SetEase(Ease.OutQuad))
                .Join(panelRect.DOAnchorPos(settledPos, animDuration).SetEase(Ease.OutQuad))
                .AppendInterval(Mathf.Max(0f, stayAtTargetSeconds))
                .OnComplete(() =>
                {
                    panelTween = null;
                    CloseSelf();
                });
        }

        private void KillPanelTween()
        {
            if (panelTween == null)
            {
                return;
            }

            panelTween.Kill();
            panelTween = null;
        }
    }
}
