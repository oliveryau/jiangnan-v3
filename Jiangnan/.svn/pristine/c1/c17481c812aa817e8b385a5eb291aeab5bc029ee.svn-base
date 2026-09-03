using System;
using DG.Tweening;
using JN.Client.Manager;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    /// <summary>
    /// 酒楼恭喜升级弹窗数据。
    /// </summary>
    public class UpgradeTavernPopPanelControllerData : UIPanelData
    {
        /// <summary>升级后的酒楼星级（用于 icon 图）。</summary>
        public int TavernLevel = 1;

        /// <summary>弹窗关闭后回调（高峰期提示等接续 UI）。</summary>
        public Action OnClosed;
    }

    /// <summary>
    /// 酒楼恭喜升级弹窗：Root 拍脸放大后回弹到 1.5；打开 1.5 秒后自动关闭。
    /// </summary>
    public class UpgradeTavernPopPanelController : OverlayPanelController<UpgradeTavernPopPanelControllerData>
    {
        private const string TavernLevelSpritePathFormat =
            "Assets/Res/Resources/Textures/UI/Panel/UpgradeTavern/lv{0}.png";
        private const float AutoCloseDelaySeconds = 1.5f;
        /// <summary>拍脸回弹后的最终缩放。</summary>
        private const float PopScaleFinal = 1.5f;
        /// <summary>拍脸峰值：相对最终尺寸再放大一截。</summary>
        private const float PopScalePeak = 1.18f * PopScaleFinal;
        private const float PopInDuration = 0.22f;
        private const float PopSettleDuration = 0.16f;

        private RectTransform rootRect;
        private Image levelIcon;
        private Tween rootPopTween;
        private Tween autoCloseTween;

        protected override void OnPanelInit()
        {
            EnsureNodes();
        }

        protected override void OnPanelOpen(UpgradeTavernPopPanelControllerData data)
        {
            EnsureNodes();
            ApplyLevelIcon();
            GameAudioManager.PlayFacilityPurchaseSuccess();
            PlayRootPopAnimation();
            BeginAutoCloseCountdown();
        }

        protected override void OnPanelShow()
        {
            EnsureNodes();
        }

        protected override void OnPanelClose()
        {
            KillTweens();
            if (rootRect != null)
            {
                rootRect.localScale = Vector3.one;
            }

            var callback = Data?.OnClosed;
            if (Data != null)
            {
                Data.OnClosed = null;
            }

            callback?.Invoke();
        }

        private void OnDestroy()
        {
            KillTweens();
        }

        private void EnsureNodes()
        {
            var root = ResolveTransform("Root", "Root");
            rootRect = root as RectTransform ?? root?.GetComponent<RectTransform>();
            levelIcon ??= ResolveImage("Root/icon", "icon");
        }

        private void ApplyLevelIcon()
        {
            if (levelIcon == null)
            {
                return;
            }

            var level = Data != null ? Data.TavernLevel : 1;
            if (level <= 0 && DataManager.Instance != null)
            {
                level = DataManager.Instance.GetTavernLevel();
            }

            var spriteLevel = Mathf.Clamp(level, 0, 4);
            var sprite = GameplayResourceStore.LoadAsset<Sprite>(
                string.Format(TavernLevelSpritePathFormat, spriteLevel));
            if (sprite == null)
            {
                return;
            }

            levelIcon.sprite = sprite;
            levelIcon.enabled = true;
            levelIcon.SetNativeSize();
        }

        /// <summary>
        /// Root 拍脸：从小放大超过最终尺寸，再回弹到 1.5。
        /// </summary>
        private void PlayRootPopAnimation()
        {
            if (rootRect == null)
            {
                return;
            }

            KillRootPopTween();
            rootRect.localScale = Vector3.one * 0.35f;
            rootPopTween = DOTween.Sequence()
                .SetUpdate(true)
                .Append(rootRect.DOScale(PopScalePeak, PopInDuration).SetEase(Ease.OutQuad))
                .Append(rootRect.DOScale(PopScaleFinal, PopSettleDuration).SetEase(Ease.OutBack))
                .OnKill(() => rootPopTween = null);
        }

        private void BeginAutoCloseCountdown()
        {
            KillAutoCloseTween();
            autoCloseTween = DOVirtual.DelayedCall(AutoCloseDelaySeconds, () =>
                {
                    autoCloseTween = null;
                    CloseSelf();
                })
                .SetUpdate(true);
        }

        private void KillTweens()
        {
            KillRootPopTween();
            KillAutoCloseTween();
        }

        private void KillRootPopTween()
        {
            if (rootPopTween == null)
            {
                return;
            }

            rootPopTween.Kill();
            rootPopTween = null;
        }

        private void KillAutoCloseTween()
        {
            if (autoCloseTween == null)
            {
                return;
            }

            autoCloseTween.Kill();
            autoCloseTween = null;
        }
    }
}
