using System;
using DG.Tweening;
using JN.Client.Manager;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    /// <summary>
    /// 新功能开启拍脸弹窗的功能类型（对应 FuncUI 资源后缀）。
    /// </summary>
    public enum NewFunctionUnlockType
    {
        /// <summary>雅间（楼梯建成）。</summary>
        Yajian = 0,
        /// <summary>拉客（轿子购买后）。</summary>
        DrumUp = 1,
        /// <summary>菜单（二楼首次 UnlockMenu 对话结束）。</summary>
        Menu = 2
    }

    /// <summary>
    /// 新功能开启拍脸弹窗数据。
    /// </summary>
    public class NewFunctionUnlockPanelControllerData : UIPanelData
    {
        public NewFunctionUnlockType FunctionType = NewFunctionUnlockType.Yajian;

        /// <summary>弹窗关闭后回调。</summary>
        public Action OnClosed;
    }

    /// <summary>
    /// 新功能开启拍脸：Root 拍脸放大后回弹；打开 1.5 秒后自动关闭。
    /// img_funcName / img_func 按功能类型切换 FuncUI 图。
    /// </summary>
    public class NewFunctionUnlockPanelController : OverlayPanelController<NewFunctionUnlockPanelControllerData>
    {
        private const string FuncNameSpritePathFormat =
            "Assets/Res/Resources/Textures/UI/FuncUI/funcName_{0}.png";
        private const string FuncIconSpritePathFormat =
            "Assets/Res/Resources/Textures/UI/FuncUI/func_{0}.png";
        private const float AutoCloseDelaySeconds = 1.5f;
        private const float PopScaleFinal = 1.5f;
        private const float PopScalePeak = 1.18f * PopScaleFinal;
        private const float PopInDuration = 0.22f;
        private const float PopSettleDuration = 0.16f;

        private RectTransform rootRect;
        private Image funcNameImage;
        private Image funcIconImage;
        private Tween rootPopTween;
        private Tween autoCloseTween;

        protected override void OnPanelInit()
        {
            EnsureNodes();
        }

        protected override void OnPanelOpen(NewFunctionUnlockPanelControllerData data)
        {
            EnsureNodes();
            ApplyFunctionSprites();
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
            funcNameImage ??= ResolveImage("Root/img_funcName", "img_funcName");
            funcIconImage ??= ResolveImage("Root/img_func", "img_func");
        }

        private void ApplyFunctionSprites()
        {
            var key = ResolveFunctionKey(Data != null ? Data.FunctionType : NewFunctionUnlockType.Yajian);
            ApplySprite(funcNameImage, string.Format(FuncNameSpritePathFormat, key));
            ApplySprite(funcIconImage, string.Format(FuncIconSpritePathFormat, key));
        }

        private static string ResolveFunctionKey(NewFunctionUnlockType type)
        {
            return type switch
            {
                NewFunctionUnlockType.DrumUp => "drumUp",
                NewFunctionUnlockType.Menu => "menu",
                _ => "yajian"
            };
        }

        private static void ApplySprite(Image image, string path)
        {
            if (image == null)
            {
                return;
            }

            var sprite = GameplayResourceStore.LoadAsset<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"[NewFunctionUnlock] 缺少图集：{path}");
                return;
            }

            image.sprite = sprite;
            image.enabled = true;
            image.SetNativeSize();
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
