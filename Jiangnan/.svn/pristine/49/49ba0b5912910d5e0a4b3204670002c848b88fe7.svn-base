using System.Collections;
using DG.Tweening;
using JN.Client.Manager;
using QFramework;
using TMPro;
using UnityEngine;

namespace JN.Client.UI
{
    /// <summary>
    /// Town 顶部状态面板。
    /// 负责金币展示和金币浮动文本。
    /// </summary>
    public class TownTopStatusPanelController : HudPanelController<TownHudPanelData>
    {
        private const string ChangeGoldTextPrefabPath = "Assets/Res/Resources/UI/Runtime/ChangeGoldText.prefab";

        private Transform groupGoldNum;
        private TextMeshProUGUI txtGoldNum;
        private TextMeshProUGUI txtChangeGoldNum;
        private Coroutine coinDeltaRoutine;
        private Vector2 coinDeltaBasePosition;
        private bool hasCoinDeltaBasePosition;
        private int displayedCoinNum = -1;
        private Tween goldNumScaleTween;

        /// <summary>
        /// 打开时缓存节点引用。
        /// </summary>
        protected override void OnPanelOpen(TownHudPanelData data)
        {
            CoinDisplayRefreshCoordinator.GoldRefreshArrived -= HandleCoinFlyArrived;
            CoinDisplayRefreshCoordinator.GoldRefreshArrived += HandleCoinFlyArrived;
            EnsureNodes();
        }

        protected override void OnPanelClose()
        {
            CoinDisplayRefreshCoordinator.GoldRefreshArrived -= HandleCoinFlyArrived;
            StopGoldNumScaleTween();
            base.OnPanelClose();
        }

        /// <summary>
        /// 显示时刷新顶部金币区域。
        /// </summary>
        protected override void OnPanelShow()
        {
            EnsureNodes();
            CacheCoinTarget();
            RefreshPanel();
        }

        /// <summary>
        /// 刷新金币显示。
        /// </summary>
        public void RefreshPanel()
        {
            EnsureNodes();
            EnsureChangeGoldText();
            if (txtGoldNum != null && DataManager.Instance?.PlayerData != null)
            {
                SyncDisplayedCoinNum(forceSyncGold: !CoinDisplayRefreshCoordinator.ShouldDeferGoldRefresh);
                txtGoldNum.text = displayedCoinNum.ToString();
            }
        }

        /// <summary>
        /// 响应金币变化，并播放浮动文本动画。
        /// </summary>
        public void HandleCoinChanged(int changeNum)
        {
            if (changeNum < 0)
            {
                SyncDisplayedCoinNum(forceSyncGold: true);
                ApplyGoldDisplay();
                PlayGoldSpendScalePulse();
                PlayCoinDelta(changeNum);
                return;
            }

            RefreshPanel();

            if (changeNum <= 0)
            {
                return;
            }

            if (CoinDisplayRefreshCoordinator.ShouldDeferGoldRefresh)
            {
                CoinDisplayRefreshCoordinator.RegisterPendingPositiveDisplay(changeNum);
                return;
            }

            PlayCoinDelta(changeNum);
        }

        /// <summary>
        /// 查找并缓存本面板依赖的节点，并隐藏金币组。
        /// </summary>
        private void EnsureNodes()
        {
            groupGoldNum ??= HudBindingUtility.FindChildRecursive(transform, "@group_GoldNum");
            txtGoldNum ??= HudBindingUtility.FindChildRecursive(transform, "@txt_GoldNum")?.GetComponent<TextMeshProUGUI>();
            txtChangeGoldNum ??= HudBindingUtility.FindChildRecursive(transform, "txt_ChangeGoldNum")?.GetComponent<TextMeshProUGUI>();
            if (groupGoldNum != null && groupGoldNum.gameObject.activeSelf)
            {
                groupGoldNum.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 缓存金币图标位置，供飞金币等效果复用。
        /// </summary>
        private void CacheCoinTarget()
        {
            if (GOReferenceManager.Instance == null)
            {
                return;
            }

            var target = txtGoldNum != null ? txtGoldNum.rectTransform : groupGoldNum;
            if (target != null)
            {
                GOReferenceManager.Instance.SaveCoinTransform(target);
            }
            else
            {
                Debug.LogWarning("TownTopStatusPanelController coin target is null. Coin fly target was not cached.", this);
            }
        }

        /// <summary>
        /// 确保金币变化文本节点存在。
        /// </summary>
        private void EnsureChangeGoldText()
        {
            if (txtChangeGoldNum != null || txtGoldNum == null)
            {
                if (txtChangeGoldNum != null)
                {
                    if (!hasCoinDeltaBasePosition)
                    {
                        coinDeltaBasePosition = txtChangeGoldNum.rectTransform.anchoredPosition;
                        hasCoinDeltaBasePosition = true;
                    }

                    var existingCanvasGroup = txtChangeGoldNum.GetComponent<CanvasGroup>() ?? txtChangeGoldNum.gameObject.AddComponent<CanvasGroup>();
                    existingCanvasGroup.alpha = 0f;
                }

                return;
            }

            var prefab = GameplayResourceStore.LoadAsset<GameObject>(ChangeGoldTextPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[TownTopStatusPanelController] Missing change gold text prefab: {ChangeGoldTextPrefabPath}");
                return;
            }

            var node = Instantiate(prefab);
            node.transform.SetParent(txtGoldNum.transform.parent, false);
            var rect = node.GetComponent<RectTransform>();
            rect.anchorMin = txtGoldNum.rectTransform.anchorMin;
            rect.anchorMax = txtGoldNum.rectTransform.anchorMax;
            rect.pivot = txtGoldNum.rectTransform.pivot;
            rect.anchoredPosition = txtGoldNum.rectTransform.anchoredPosition + new Vector2(0f, -28f);
            rect.sizeDelta = txtGoldNum.rectTransform.sizeDelta;

            txtChangeGoldNum = node.GetComponent<TextMeshProUGUI>();
            if (txtChangeGoldNum == null)
            {
                Debug.LogWarning($"[TownTopStatusPanelController] Change gold text prefab missing TextMeshProUGUI: {ChangeGoldTextPrefabPath}");
                Destroy(node);
                return;
            }

            txtChangeGoldNum.font = txtGoldNum.font;
            txtChangeGoldNum.fontSize = txtGoldNum.fontSize;
            txtChangeGoldNum.alignment = TextAlignmentOptions.Center;
            txtChangeGoldNum.raycastTarget = false;
            txtChangeGoldNum.text = string.Empty;
            coinDeltaBasePosition = rect.anchoredPosition;
            hasCoinDeltaBasePosition = true;
        }

        /// <summary>
        /// 播放金币变化浮动动画。
        /// </summary>
        private void PlayCoinDelta(int changeNum)
        {
            EnsureChangeGoldText();
            if (txtChangeGoldNum == null || changeNum == 0)
            {
                return;
            }

            txtChangeGoldNum.text = changeNum > 0 ? $"+{changeNum}" : changeNum.ToString();
            txtChangeGoldNum.color = changeNum > 0 ? Color.green : Color.red;
            if (coinDeltaRoutine != null)
            {
                StopCoroutine(coinDeltaRoutine);
            }

            coinDeltaRoutine = StartCoroutine(CoinDeltaAnim(txtChangeGoldNum.rectTransform));
        }

        /// <summary>
        /// 金币变化文本上浮并淡出的动画。
        /// </summary>
        private IEnumerator CoinDeltaAnim(RectTransform target)
        {
            var time = 0f;
            const float duration = 1f;
            var start = coinDeltaBasePosition;
            var end = start + new Vector2(0f, 80f);
            var canvasGroup = target.GetComponent<CanvasGroup>() ?? target.gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 1f;
            target.gameObject.SetActive(true);
            target.SetAsLastSibling();
            target.anchoredPosition = start;
            while (time < duration)
            {
                time += Time.deltaTime;
                var progress = Mathf.Clamp01(time / duration);
                target.anchoredPosition = Vector2.Lerp(start, end, progress);
                canvasGroup.alpha = 1f - progress;
                yield return null;
            }

            canvasGroup.alpha = 0f;
            target.anchoredPosition = start;
            coinDeltaRoutine = null;
        }

        private void HandleCoinFlyArrived()
        {
            var pendingDelta = CoinDisplayRefreshCoordinator.ConsumePendingPositiveDisplay();
            if (pendingDelta > 0)
            {
                PlayCoinDelta(pendingDelta);
            }

            PlayGoldIncomeScaleThenRefresh();
        }

        private void SyncDisplayedCoinNum(bool forceSyncGold)
        {
            if (DataManager.Instance?.PlayerData == null)
            {
                return;
            }

            if (forceSyncGold || displayedCoinNum < 0)
            {
                displayedCoinNum = DataManager.Instance.PlayerData.coinNum;
            }
        }

        private void ApplyGoldDisplay()
        {
            if (txtGoldNum != null)
            {
                txtGoldNum.text = displayedCoinNum.ToString();
            }
        }

        private void PlayGoldIncomeScaleThenRefresh()
        {
            var scaleTarget = ResolveGoldScaleTarget();
            if (scaleTarget == null)
            {
                SyncDisplayedCoinNum(forceSyncGold: true);
                ApplyGoldDisplay();
                return;
            }

            StopGoldNumScaleTween();
            scaleTarget.localScale = Vector3.one;
            goldNumScaleTween = DOTween.Sequence()
                .Append(scaleTarget.DOScale(1.28f, 0.12f).SetEase(Ease.OutQuad))
                .AppendCallback(() =>
                {
                    SyncDisplayedCoinNum(forceSyncGold: true);
                    ApplyGoldDisplay();
                })
                .Append(scaleTarget.DOScale(1f, 0.18f).SetEase(Ease.OutBack))
                .OnKill(() => goldNumScaleTween = null)
                .OnComplete(() => goldNumScaleTween = null);
        }

        private void PlayGoldSpendScalePulse()
        {
            var scaleTarget = ResolveGoldScaleTarget();
            if (scaleTarget == null)
            {
                return;
            }

            StopGoldNumScaleTween();
            scaleTarget.localScale = Vector3.one;
            goldNumScaleTween = scaleTarget
                .DOPunchScale(Vector3.one * 0.14f, 0.28f, 6, 0.55f)
                .OnKill(() => goldNumScaleTween = null)
                .OnComplete(() => goldNumScaleTween = null);
        }

        private RectTransform ResolveGoldScaleTarget()
        {
            if (txtGoldNum != null)
            {
                return txtGoldNum.rectTransform;
            }

            return groupGoldNum as RectTransform;
        }

        private void StopGoldNumScaleTween()
        {
            if (goldNumScaleTween == null)
            {
                return;
            }

            goldNumScaleTween.Kill();
            goldNumScaleTween = null;

            var scaleTarget = ResolveGoldScaleTarget();
            if (scaleTarget != null)
            {
                scaleTarget.localScale = Vector3.one;
            }
        }
    }
}
