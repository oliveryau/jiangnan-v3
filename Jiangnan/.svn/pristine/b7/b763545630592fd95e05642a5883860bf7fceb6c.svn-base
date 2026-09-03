using System;
using System.Collections;
using cfg;
using DG.Tweening;
using JN.Client.Config;
using JN.Client.Manager;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    public class GetAchievementPanelControllerData : QFramework.UIPanelData
    {
        public int AchievementId;
        public Action OnClosed;
    }

    /// <summary>
    /// 成就获得横幅：Panel/Image 宽度 75→900 展开，展示后 900→75 收回后立即关闭。
    /// </summary>
    public class GetAchievementPanelController : OverlayPanelController<GetAchievementPanelControllerData>
    {
        private const string BannerPath = "Panel/Image";
        private const float CollapsedWidth = 75f;
        private const float ExpandedWidth = 900f;
        private const float ExpandDuration = 0.8f;
        private const float CollapseDuration = 0.8f;
        private const float DisplayDuration = 2f;

        private RectTransform bannerRect;
        private Image iconImage;
        private Image frameImage;
        private TMP_Text nameText;
        private TMP_Text descText;
        private Coroutine displayRoutine;
        private Tween widthTween;

        protected override void OnPanelInit()
        {
            EnsureNodes();
        }

        protected override void OnPanelOpen(GetAchievementPanelControllerData data)
        {
            EnsureNodes();
            ApplyContent();
        }

        protected override void OnPanelShow()
        {
            EnsureNodes();
            ApplyContent();
            RestartDisplayRoutine();
        }

        protected override void OnPanelClose()
        {
            KillWidthTween();
            if (displayRoutine != null)
            {
                StopCoroutine(displayRoutine);
                displayRoutine = null;
            }

            ResetBannerWidth();

            var callback = Data?.OnClosed;
            if (Data != null)
            {
                Data.OnClosed = null;
            }

            if (callback == null)
            {
                return;
            }

            ActionKit.NextFrame(callback).StartGlobal();
        }

        private void EnsureNodes()
        {
            bannerRect ??= ResolveTransform(BannerPath) as RectTransform;
            iconImage ??= ResolveImage($"{BannerPath}/Icon", "Icon");
            frameImage ??= ResolveImage($"{BannerPath}/Frame", "Frame");
            nameText ??= ResolveText($"{BannerPath}/Name", "Name");
            descText ??= ResolveText($"{BannerPath}/Desc", "Desc");
        }

        private void ApplyContent()
        {
            var achievement = ResolveAchievement();
            if (achievement == null)
            {
                SetText($"{BannerPath}/Name", "新成就");
                SetText($"{BannerPath}/Desc", string.Empty);
                return;
            }

            SetText($"{BannerPath}/Name", achievement.Name);
            SetText($"{BannerPath}/Desc", achievement.Desc ?? string.Empty);

            if (iconImage != null)
            {
                var icon = AchievementDisplayAssetCatalog.ResolveAchievementIcon(achievement);
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
                iconImage.color = Color.white;
            }

            if (frameImage != null)
            {
                var frame = AchievementDisplayAssetCatalog.ResolveAchievementFrame(achievement);
                frameImage.sprite = frame;
                frameImage.enabled = frame != null;
                frameImage.color = Color.white;
            }
        }

        private Achievement ResolveAchievement()
        {
            var achievementId = Data?.AchievementId ?? 0;
            return achievementId > 0 ? AchievementConfigUtility.Get(achievementId) : null;
        }

        private void RestartDisplayRoutine()
        {
            KillWidthTween();
            if (displayRoutine != null)
            {
                StopCoroutine(displayRoutine);
            }

            displayRoutine = StartCoroutine(DisplayRoutine());
        }

        private IEnumerator DisplayRoutine()
        {
            ResetBannerWidth();
            GameAudioManager.PlayGetAchievement();
            if (bannerRect != null)
            {
                var targetSize = bannerRect.sizeDelta;
                targetSize.x = ExpandedWidth;
                widthTween = bannerRect
                    .DOSizeDelta(targetSize, ExpandDuration)
                    .SetEase(Ease.OutCubic);
                yield return widthTween.WaitForCompletion();
            }
            else
            {
                yield return new WaitForSeconds(ExpandDuration);
            }

            yield return new WaitForSeconds(DisplayDuration);

            GameAudioManager.PlayGetAchievement();
            if (bannerRect != null)
            {
                var collapsedSize = bannerRect.sizeDelta;
                collapsedSize.x = CollapsedWidth;
                widthTween = bannerRect
                    .DOSizeDelta(collapsedSize, CollapseDuration)
                    .SetEase(Ease.InCubic);
                yield return widthTween.WaitForCompletion();
            }
            else
            {
                yield return new WaitForSeconds(CollapseDuration);
            }

            displayRoutine = null;
            CloseSelf();
        }

        private void ResetBannerWidth()
        {
            if (bannerRect == null)
            {
                return;
            }

            var size = bannerRect.sizeDelta;
            size.x = CollapsedWidth;
            bannerRect.sizeDelta = size;
        }

        private void KillWidthTween()
        {
            if (widthTween != null && widthTween.IsActive())
            {
                widthTween.Kill();
            }

            widthTween = null;
        }
    }
}
