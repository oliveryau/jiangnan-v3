using System.Collections;
using JN.Client.Config;
using JN.Client.Manager;
using JN.Client.Scene;
using QFramework;
using TMPro;
using JN.Client.Manager;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace JN.Client.UI
{
    /// <summary>
    /// 管理酒楼营业中的涨价、鼓舞和加速入口及倒计时表现。
    /// 当前 HUD 已停用本面板（不再 Open/Show），保留脚本与预制体便于日后恢复。
    /// </summary>
    public class TavernBusinessBoostPanelController : HudPanelController<TavernHudPanelData>
    {
        private const int DefaultPriceIncreaseCost = 500;
        private const int DefaultPriceIncreaseProfitPercent = 100;
        private const float DefaultPriceIncreaseDuration = 30f;
        private const int DefaultInspireCost = 500;
        private const int DefaultInspireCustomerPercent = 100;
        private const float DefaultInspireDuration = 30f;
        private const int DefaultSpeedUpCost = 500;
        private const float DefaultSpeedUpDuration = 30f;
        private const float DefaultSpeedUpMultiplier = 3f;
        private const string DefaultTavernVideoPath = "Assets/Res/Textures/UI/TavernStatusBarPanel/inspireVideo.mp4";

        [SerializeField] private Button priceIncreaseBtn;
        [SerializeField] private RectTransform priceIncreaseRoot;
        [SerializeField] private TMP_Text priceIncreaseCostText;
        [SerializeField] private RectTransform priceIncreaseTips;
        [SerializeField] private TMP_Text priceIncreaseTipsText;
        [SerializeField] private TMP_Text priceIncreaseCountdownText;

        [SerializeField] private RectTransform inspireRoot;
        [SerializeField] private Button inspireBtn;
        [SerializeField] private TMP_Text inspireCostText;
        [SerializeField] private RectTransform inspireTips;
        [SerializeField] private TMP_Text inspireTipsText;
        [SerializeField] private TMP_Text inspireCountdownText;
        [SerializeField] private VideoClip inspireVideo;

        [SerializeField] private RectTransform speedUpRoot;
        [SerializeField] private Button speedUpBtn;
        [SerializeField] private TMP_Text speedUpCostText;

        private Coroutine priceIncreaseRoutine;
        private Coroutine inspireRoutine;
        private Coroutine speedUpRoutine;
        private bool priceIncreaseActive;
        private bool inspireActive;
        private bool speedUpActive;
        private float currentCustomerCoefficient = 1f;
        private float currentPriceCoefficient = 1f;
        private bool businessBoostClosedForBusinessEnd;

        /// <summary>
        /// 面板首次打开时缓存按钮和文本节点。
        /// </summary>
        protected override void OnPanelOpen(TavernHudPanelData data)
        {
            EnsureNodes();
        }

        /// <summary>
        /// 面板显示时恢复节点并同步当前营业加成状态。
        /// </summary>
        protected override void OnPanelShow()
        {
            SetManagedNodesVisible(true);
            RefreshPanel();
        }

        /// <summary>
        /// 面板关闭时停止加成表现并隐藏自身节点。
        /// </summary>
        protected override void OnPanelClose()
        {
            StopPriceAdjustment(true);
            SetManagedNodesVisible(false);
        }

        /// <summary>
        /// 刷新加成按钮、花费和倒计时显示。
        /// </summary>
        public void RefreshPanel()
        {
            EnsureNodes();
            RefreshPriceAdjustmentBarState();
        }

        /// <summary>
        /// 根据营业状态决定是否保留当前加成表现。
        /// </summary>
        public void HandleBusinessStateChanged(bool isOpen)
        {
            if (isOpen)
            {
                businessBoostClosedForBusinessEnd = false;
            }
            else
            {
                StopPriceAdjustment(true);
            }

            RefreshPanel();
        }

        /// <summary>
        /// 营业结束后立即回收加成 UI。
        /// </summary>
        public void HideForBusinessEnd()
        {
            businessBoostClosedForBusinessEnd = true;
            StopPriceAdjustment(true);
            RefreshPanel();
        }

        /// <summary>
        /// 延迟绑定加成按钮、提示条和倒计时文本。
        /// </summary>
        private void EnsureNodes()
        {
            var hudRoot = transform;

            priceIncreaseBtn ??= HudBindingUtility.FindChildRecursive(hudRoot, "btn_PriceIncrease")?.GetComponent<Button>();
            inspireBtn ??= HudBindingUtility.FindChildRecursive(hudRoot, "btn_Inspire")?.GetComponent<Button>();
            speedUpBtn ??= HudBindingUtility.FindChildRecursive(hudRoot, "btn_SpeedUp")?.GetComponent<Button>();

            if (priceIncreaseBtn != null)
            {
                priceIncreaseRoot ??= FindAncestorRect(priceIncreaseBtn.transform, "PriceIncrease") ?? priceIncreaseBtn.transform as RectTransform;
                priceIncreaseCostText ??= ResolveNestedText(priceIncreaseRoot, "PriceIncrease");
                priceIncreaseBtn.onClick.RemoveListener(OnClickPriceIncreaseButton);
                priceIncreaseBtn.onClick.AddListener(OnClickPriceIncreaseButton);
            }

            if (inspireBtn != null)
            {
                inspireRoot ??= FindAncestorRect(inspireBtn.transform, "Inspire") ?? inspireBtn.transform as RectTransform;
                inspireCostText ??= ResolveNestedText(inspireRoot, "PriceInspire");
                inspireBtn.onClick.RemoveListener(OnClickInspireButton);
                inspireBtn.onClick.AddListener(OnClickInspireButton);
            }

            if (speedUpBtn != null)
            {
                speedUpRoot ??= FindAncestorRect(speedUpBtn.transform, "SpeedUp") ?? speedUpBtn.transform as RectTransform;
                speedUpCostText ??= ResolveNestedText(speedUpRoot, "PriceSpeedUp")
                                   ?? ResolveNestedText(speedUpRoot, "PriceSpeed")
                                   ?? ResolveNestedText(speedUpRoot, "Price")
                                   ?? HudBindingUtility.ResolveChildText(speedUpRoot, "Text (TMP)");
                speedUpBtn.onClick.RemoveListener(OnClickSpeedUpButton);
                speedUpBtn.onClick.AddListener(OnClickSpeedUpButton);
            }

            priceIncreaseTips ??= HudBindingUtility.FindChildRecursive(hudRoot, "tips_PriceIncrease") as RectTransform;
            inspireTips ??= HudBindingUtility.FindChildRecursive(hudRoot, "tips_Inspire") as RectTransform;
            priceIncreaseTipsText ??= HudBindingUtility.ResolveChildText(priceIncreaseTips, "Text (TMP)");
            priceIncreaseCountdownText ??= HudBindingUtility.ResolveChildText(priceIncreaseTips, "ClockDown");
            inspireTipsText ??= HudBindingUtility.ResolveChildText(inspireTips, "Text (TMP)");
            inspireCountdownText ??= HudBindingUtility.ResolveChildText(inspireTips, "ClockDown");
        }

        /// <summary>
        /// 按当前营业状态刷新三个加成功能的可见性和可交互性。
        /// </summary>
        private void RefreshPriceAdjustmentBarState()
        {
            var active = CanShowBusinessBoostButtons();

            if (priceIncreaseRoot != null)
            {
                priceIncreaseRoot.gameObject.SetActive(active);
            }

            if (inspireRoot != null)
            {
                inspireRoot.gameObject.SetActive(active);
            }

            if (speedUpRoot != null)
            {
                speedUpRoot.gameObject.SetActive(active);
            }

            if (priceIncreaseBtn != null)
            {
                priceIncreaseBtn.interactable = active && !priceIncreaseActive;
            }

            if (inspireBtn != null)
            {
                inspireBtn.interactable = active && !inspireActive;
            }

            if (speedUpBtn != null)
            {
                speedUpBtn.interactable = active && !speedUpActive;
            }

            if (priceIncreaseCostText != null)
            {
                priceIncreaseCostText.text = GetPriceIncreaseCost().ToString();
            }

            if (inspireCostText != null)
            {
                inspireCostText.text = GetInspireCost().ToString();
            }

            if (speedUpCostText != null)
            {
                speedUpCostText.text = GetSpeedUpCost().ToString();
            }

            if (priceIncreaseTips != null)
            {
                priceIncreaseTips.gameObject.SetActive(active && priceIncreaseActive);
            }

            if (inspireTips != null)
            {
                inspireTips.gameObject.SetActive(active && inspireActive);
            }
        }

        /// <summary>
        /// 触发涨价加成并开始倒计时。
        /// </summary>
        private void OnClickPriceIncreaseButton()
        {
            if (!CanShowBusinessBoostButtons())
            {
                RefreshPanel();
                return;
            }

            var cost = GetPriceIncreaseCost();
            if (!TrySpendBusinessBoostCost(cost))
            {
                RefreshPanel();
                return;
            }

            if (priceIncreaseRoutine != null)
            {
                StopCoroutine(priceIncreaseRoutine);
            }

            var percent = GetPriceIncreaseProfitPercent();
            priceIncreaseActive = true;
            currentPriceCoefficient = 1f + Mathf.Max(0, percent) / 100f;
            ApplyBusinessBoostCoefficients();
            RefreshPanel();
            priceIncreaseRoutine = StartCoroutine(PriceIncreaseCountdownRoutine(GetPriceIncreaseDuration(), percent));
        }

        /// <summary>
        /// 触发鼓舞加成并播放对应表现。
        /// </summary>
        private void OnClickInspireButton()
        {
            if (!CanShowBusinessBoostButtons())
            {
                RefreshPanel();
                return;
            }

            var cost = GetInspireCost();
            if (!TrySpendBusinessBoostCost(cost))
            {
                RefreshPanel();
                return;
            }

            if (inspireRoutine != null)
            {
                StopCoroutine(inspireRoutine);
            }

            var percent = GetInspireCustomerPercent();
            inspireActive = true;
            currentCustomerCoefficient = 1f + Mathf.Max(0, percent) / 100f;
            ApplyBusinessBoostCoefficients();
            RefreshPanel();
            inspireRoutine = StartCoroutine(InspireCountdownRoutine(GetInspireDuration(), percent));
            TryPlayRuntimeVideo(inspireVideo, DefaultTavernVideoPath);
        }

        /// <summary>
        /// 触发营业加速修正器。
        /// </summary>
        private void OnClickSpeedUpButton()
        {
            if (!CanShowBusinessBoostButtons())
            {
                RefreshPanel();
                return;
            }

            var cost = GetSpeedUpCost();
            if (!TrySpendBusinessBoostCost(cost))
            {
                RefreshPanel();
                return;
            }

            if (speedUpRoutine != null)
            {
                StopCoroutine(speedUpRoutine);
            }

            speedUpActive = true;
            TavernSceneManager.Instance?.RestoreAllWaiterStaminaToFull();
            ApplyServiceSpeedBoost();
            RefreshPanel();
            speedUpRoutine = StartCoroutine(SpeedUpCountdownRoutine(GetSpeedUpDuration()));
        }

        private bool TrySpendBusinessBoostCost(int cost)
        {
            if (DataManager.Instance == null)
            {
                return false;
            }

            if (cost > 0 && DataManager.Instance.PlayerData.coinNum < cost)
            {
                HudOverlayService.ShowFloatingWarning($"铜钱不足，需要 {cost}");
                return false;
            }

            if (cost > 0)
            {
                DataManager.Instance.ChangeCoinNum(-cost);
            }

            return true;
        }

        private IEnumerator PriceIncreaseCountdownRoutine(float duration, int percent)
        {
            var remainingTime = Mathf.Max(0.1f, duration);
            while (remainingTime > 0f && IsBusinessBoostSessionActive())
            {
                if (priceIncreaseTipsText != null)
                {
                    priceIncreaseTipsText.text = $"盈利+{Mathf.Max(0, percent)}%";
                }

                if (priceIncreaseCountdownText != null)
                {
                    priceIncreaseCountdownText.text = $"{Mathf.CeilToInt(remainingTime)}s";
                }

                remainingTime -= Time.deltaTime;
                yield return null;
            }

            priceIncreaseRoutine = null;
            StopPriceIncreaseEffect(true);
        }

        private IEnumerator InspireCountdownRoutine(float duration, int percent)
        {
            var remainingTime = Mathf.Max(0.1f, duration);
            while (remainingTime > 0f && IsBusinessBoostSessionActive())
            {
                if (inspireTipsText != null)
                {
                    inspireTipsText.text = $"客流+{Mathf.Max(0, percent)}%";
                }

                if (inspireCountdownText != null)
                {
                    inspireCountdownText.text = $"{Mathf.CeilToInt(remainingTime)}s";
                }

                remainingTime -= Time.deltaTime;
                yield return null;
            }

            inspireRoutine = null;
            StopInspireEffect(true);
        }

        private IEnumerator SpeedUpCountdownRoutine(float duration)
        {
            var remainingTime = Mathf.Max(0.1f, duration);
            while (remainingTime > 0f && IsBusinessBoostSessionActive())
            {
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            speedUpRoutine = null;
            StopSpeedUpEffect(true);
        }

        private void StopPriceAdjustment(bool refreshUi)
        {
            StopPriceIncreaseEffect(false);
            StopInspireEffect(false);
            StopSpeedUpEffect(false);
            currentCustomerCoefficient = 1f;
            currentPriceCoefficient = 1f;
            ApplyBusinessBoostCoefficients();
            ApplyServiceSpeedBoost();
            if (refreshUi)
            {
                RefreshPanel();
            }
        }

        private void StopPriceIncreaseEffect(bool refreshUi)
        {
            if (priceIncreaseRoutine != null)
            {
                StopCoroutine(priceIncreaseRoutine);
                priceIncreaseRoutine = null;
            }

            priceIncreaseActive = false;
            currentPriceCoefficient = 1f;
            if (refreshUi)
            {
                ApplyBusinessBoostCoefficients();
                RefreshPanel();
            }
        }

        private void StopSpeedUpEffect(bool refreshUi)
        {
            if (speedUpRoutine != null)
            {
                StopCoroutine(speedUpRoutine);
                speedUpRoutine = null;
            }

            speedUpActive = false;
            if (refreshUi)
            {
                ApplyServiceSpeedBoost();
                RefreshPanel();
            }
        }

        private void StopInspireEffect(bool refreshUi)
        {
            if (inspireRoutine != null)
            {
                StopCoroutine(inspireRoutine);
                inspireRoutine = null;
            }

            inspireActive = false;
            currentCustomerCoefficient = 1f;
            if (refreshUi)
            {
                ApplyBusinessBoostCoefficients();
                RefreshPanel();
            }
        }

        private void ApplyBusinessBoostCoefficients()
        {
            var modifierService = TavernBusinessModifierService.Instance;
            if (currentCustomerCoefficient > 1f)
            {
                modifierService.SetCustomerFlowModifier(TavernBusinessModifierService.InspireSource, currentCustomerCoefficient);
            }
            else
            {
                modifierService.ClearCustomerFlowModifier(TavernBusinessModifierService.InspireSource);
            }

            if (currentPriceCoefficient > 1f)
            {
                modifierService.SetPriceModifier(TavernBusinessModifierService.PriceIncreaseSource, currentPriceCoefficient);
            }
            else
            {
                modifierService.ClearPriceModifier(TavernBusinessModifierService.PriceIncreaseSource);
            }
        }

        private void ApplyServiceSpeedBoost()
        {
            if (speedUpActive)
            {
                TavernBusinessModifierService.Instance.SetServiceSpeedModifier(
                    TavernBusinessModifierService.SpeedUpButtonSource,
                    GetSpeedUpMultiplier());
                return;
            }

            TavernBusinessModifierService.Instance.ClearServiceSpeedModifier(TavernBusinessModifierService.SpeedUpButtonSource);
        }

        private bool CanShowBusinessBoostButtons()
        {
            var sceneManager = TavernSceneManager.Instance;
            var closingFlowPanel = UIKit.GetPanel<TavernBusinessFlowPanelController>();
            return DataManager.Instance != null
                   && DataManager.Instance.TavernData != null
                   && DataManager.Instance.TavernData.isOpen
                   && !businessBoostClosedForBusinessEnd
                   && (closingFlowPanel == null || !closingFlowPanel.IsWaitingSettlementConfirm)
                   && (sceneManager == null || !sceneManager.IsClosingBusiness);
        }

        private bool IsBusinessBoostSessionActive()
        {
            return DataManager.Instance != null
                   && DataManager.Instance.TavernData != null
                   && DataManager.Instance.TavernData.isOpen
                   && !businessBoostClosedForBusinessEnd
                   && (TavernSceneManager.Instance == null || !TavernSceneManager.Instance.IsClosingBusiness);
        }

        private void SetManagedNodesVisible(bool visible)
        {
            if (!visible)
            {
                if (priceIncreaseTips != null)
                {
                    priceIncreaseTips.gameObject.SetActive(false);
                }

                if (inspireTips != null)
                {
                    inspireTips.gameObject.SetActive(false);
                }
            }
        }

        private static RectTransform FindAncestorRect(Transform child, string nodeName)
        {
            var current = child;
            while (current != null)
            {
                if (current.name == nodeName)
                {
                    return current as RectTransform;
                }

                current = current.parent;
            }

            return null;
        }

        private static TMP_Text ResolveNestedText(Transform root, string childName)
        {
            return HudBindingUtility.ResolveChildText(HudBindingUtility.FindChildRecursive(root, childName), "Text (TMP)")
                   ?? HudBindingUtility.FindChildRecursive(root, childName)?.GetComponentInChildren<TMP_Text>(true);
        }

        private int GetPriceIncreaseCost()
        {
            return TbConfigRuntime.GetPriceIncreaseCost(DefaultPriceIncreaseCost);
        }

        private int GetPriceIncreaseProfitPercent()
        {
            return TbConfigRuntime.GetPriceIncreaseProfitPercent(DefaultPriceIncreaseProfitPercent);
        }

        private int GetInspireCustomerPercent()
        {
            return TbConfigRuntime.GetInspireCustomerPercent(DefaultInspireCustomerPercent);
        }

        private float GetPriceIncreaseDuration()
        {
            return TbConfigRuntime.GetPriceIncreaseDuration(DefaultPriceIncreaseDuration);
        }

        private int GetInspireCost()
        {
            return TbConfigRuntime.GetInspireCost(DefaultInspireCost);
        }

        private float GetInspireDuration()
        {
            return TbConfigRuntime.GetInspireDuration(DefaultInspireDuration);
        }

        private float GetSpeedUpDuration()
        {
            return Mathf.Max(0.1f, DefaultSpeedUpDuration);
        }

        private int GetSpeedUpCost()
        {
            return TbConfigRuntime.GetSpeedUpCost(DefaultSpeedUpCost);
        }

        private float GetSpeedUpMultiplier()
        {
            return Mathf.Max(1f, DefaultSpeedUpMultiplier);
        }

        private static void TryPlayRuntimeVideo(VideoClip clip, string fallbackAssetPath = null)
        {
            clip ??= ResolveVideoClipFallback(fallbackAssetPath);
            if (clip == null)
            {
                return;
            }

            VideoWindowController.Show(clip, null, false);
        }

        private static VideoClip ResolveVideoClipFallback(string fallbackAssetPath)
        {
            if (string.IsNullOrWhiteSpace(fallbackAssetPath))
            {
                return null;
            }

            return GameplayResourceStore.LoadAsset<VideoClip>(fallbackAssetPath);
        }
    }
}
