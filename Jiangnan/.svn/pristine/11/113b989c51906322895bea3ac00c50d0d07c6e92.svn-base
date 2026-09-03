using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using JN.Client;
using JN.Client.Config;
using JN.Client.Manager;
using JN.Client.Model;
using JN.Client.Scene;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace JN.Client.UI
{
    /// <summary>
    /// 酒楼营业流程 HUD：开业入口与打烊结算。
    /// 开业后剩余时间由顶部状态栏展示；本面板不提供手动打烊（btn_Close 已停用）。
    /// </summary>
    public class TavernBusinessFlowPanelController : HudPanelController<TavernHudPanelData>
    {
        private const string SettlementDetailNodeName = "txt_ClosingDetail";
        private const string GrandOpeningVideoAssetPath = "Assets/Res/Resources/Videos/grandOpeningVideo.mp4";
        private const float SettlementBarBaseline = 5000f;
        private const float SettlementBillTextRefreshInterval = 0.05f;
        private static readonly Color SettlementProfitPositiveColor = new(62f / 255f, 166f / 255f, 45f / 255f);
        private static readonly Color SettlementProfitNegativeColor = new(200f / 255f, 77f / 255f, 77f / 255f);
        private static readonly string[] SettlementSumItemNodeNames = { "SumItem", "SumItem_1", "SumItem_2" };

        private struct SettlementSumItemBinding
        {
            public Transform Root;
            public TMP_Text Title;
            public TMP_Text Value;
            public Image IncomeFill;
            public Image CostFill;
        }

        [SerializeReference] private Button openButton;
        [SerializeReference] private Button closeButton;
        [SerializeReference] private RectTransform closeConfirmPanel;
        [SerializeReference] private Button hideCloseConfirmButton;
        [SerializeReference] private Button confirmCloseButton;
        [SerializeReference] private RectTransform settlementPanel;
        [SerializeReference] private Image settlementResultIcon;
        [SerializeReference] private Image settlementResultTextBg;
        [SerializeReference] private Image settlementResultTextIcon;
        [SerializeReference] private TMP_Text settlementRevenueText;
        [SerializeReference] private TMP_Text settlementSalaryText;
        [SerializeReference] private TMP_Text settlementProfitLabelText;
        [SerializeReference] private TMP_Text settlementProfitText;
        [SerializeReference] private Transform settlementProfitCoinTarget;
        [SerializeReference] private TMP_Text settlementDetailText;
        [SerializeReference] private Button confirmSettlementButton;
        [SerializeReference] private Button settlementMaskButton;

        [Header("Profit Stage")]
        [SerializeField] private int highStage = 3000;
        [SerializeField] private int middleStage = 1600;
        [SerializeField] private int lowStage = 500;

        [Header("Profit Stage Icon")]
        [SerializeField] private Sprite highStageIcon;
        [SerializeField] private Sprite highStageTextBgIcon;
        [SerializeField] private Sprite highStageTextIcon;
        [SerializeField] private Sprite middleStageIcon;
        [SerializeField] private Sprite middleStageTextBgIcon;
        [SerializeField] private Sprite middleStageTextIcon;
        [SerializeField] private Sprite lowStageIcon;
        [SerializeField] private Sprite lowStageTextBgIcon;
        [SerializeField] private Sprite lowStageTextIcon;

        [Header("Settlement Animation")]
        [SerializeField, Min(0f)] private float settlementPanelPopDuration = 0.25f;
        [SerializeField, Min(0f)] private float settlementPanelStartScale = 0.7f;
        [SerializeField, Min(0.01f)] private float settlementSumItemFullFillDuration = 2f;
        private const float MinSettlementIncomeRiseDuration = 1f;

        private bool businessSessionStarted;

        /// <summary>首次开业视频播放中，尚未真正开业接客。</summary>
        private bool waitingGrandOpeningVideo;
        private bool settlementPresentationActive;
        private Tween settlementPanelTween;
        private Vector3 settlementPanelDefaultScale = Vector3.one;
        private bool settlementPanelScaleCached;
        private bool settlementCommitted;
        private bool settlementBgmDucked;
        private SettlementSumItemBinding[] settlementSumItems;
        private readonly List<Tween> settlementFillTweens = new List<Tween>();
        private Coroutine settlementBillAnimCoroutine;
        private Tween settlementProfitRevealTween;
        private TavernGuideService GuideService => TavernGuideService.Instance;

        /// <summary>
        /// 当前是否正在显示结算面板。
        /// </summary>
        public bool IsSettlementVisible => settlementPanel != null && settlementPanel.gameObject.activeSelf;
        public bool IsWaitingSettlementConfirm { get; private set; }
        public bool IsSettlementPresentationPlaying =>
            settlementPresentationActive
            || settlementBillAnimCoroutine != null
            || HasActiveSettlementFillTween()
            || (settlementProfitRevealTween != null && settlementProfitRevealTween.IsActive());

        /// <summary>
        /// 面板首次打开时缓存打烊与结算节点。
        /// </summary>
        protected override void OnPanelOpen(TavernHudPanelData data)
        {
            EnsureNodes();
        }

        /// <summary>
        /// 面板显示时重置结算状态并同步当前营业会话。
        /// </summary>
        protected override void OnPanelShow()
        {
            SetManagedNodesVisible(true);
            EnsureNodes();
            if (!IsWaitingSettlementConfirm)
            {
                HideSettlementPanelImmediately();
                SyncBusinessStateOnShow();
                RefreshPanel();
                return;
            }

            SyncBusinessStateOnShow();
            RefreshOpenButtonState();
            HideManualCloseUi();
        }

        /// <summary>
        /// 面板关闭时解绑开业与结算按钮。
        /// </summary>
        protected override void OnPanelClose()
        {
            if (openButton != null)
            {
                openButton.onClick.RemoveListener(OnClickOpenTavernBusiness);
            }

            if (confirmSettlementButton != null)
            {
                confirmSettlementButton.onClick.RemoveListener(CloseSettlementPanel);
            }

            if (settlementMaskButton != null)
            {
                settlementMaskButton.onClick.RemoveListener(CloseSettlementPanel);
            }

            SetSettlementBgmDucked(false);
            SetManagedNodesVisible(false);
        }

        /// <summary>
        /// 持续校正开业按钮的可见性。
        /// </summary>
        private void Update()
        {
            if (isActiveAndEnabled)
            {
                RefreshOpenButtonState();
                HideManualCloseUi();
            }
        }

        /// <summary>
        /// 刷新开业按钮与结算金额显示。
        /// </summary>
        public void RefreshPanel()
        {
            EnsureNodes();
            RefreshOpenButtonState();
            HideManualCloseUi();
            if (!IsSettlementPresentationPlaying)
            {
                RefreshSettlementRevenue();
            }
        }

        /// <summary>
        /// 按营业开关切换打烊确认和结算流程。
        /// </summary>
        public void HandleBusinessStateChanged(bool isOpen)
        {
            if (isOpen)
            {
                HideSettlementPanelImmediately();
                IsWaitingSettlementConfirm = false;
                settlementCommitted = false;
                businessSessionStarted = true;
            }
            else if (businessSessionStarted)
            {
                ShowSettlementPanel();
                businessSessionStarted = false;
                RefreshOpenButtonState();
                HideManualCloseUi();
                return;
            }
            else
            {
                IsWaitingSettlementConfirm = false;
                HideSettlementPanelImmediately();
            }

            RefreshPanel();
        }

        /// <summary>
        /// 进入打烊申请阶段，等待场景侧真正关店。
        /// </summary>
        public void PrepareForCloseRequest()
        {
            if (closeConfirmPanel != null)
            {
                closeConfirmPanel.gameObject.SetActive(false);
            }

            RefreshPanel();
        }

        /// <summary>
        /// 延迟绑定打烊确认和结算面板节点。
        /// </summary>
        private void EnsureNodes()
        {
            var hudRoot = transform;

            openButton ??= HudBindingUtility.FindChildRecursive(hudRoot, "OpenBtn")?.GetComponent<Button>();
            closeButton ??= HudBindingUtility.FindChildRecursive(hudRoot, "btn_Close")?.GetComponent<Button>();
            closeConfirmPanel ??= HudBindingUtility.FindChildRecursive(hudRoot, "closeConfirmPanel") as RectTransform
                                 ?? HudBindingUtility.FindChildRecursive(hudRoot, "CloseConfirmPanel") as RectTransform;
            hideCloseConfirmButton ??= HudBindingUtility.FindChildRecursive(closeConfirmPanel, "btn_hideCloseConfirmPanel")?.GetComponent<Button>();
            confirmCloseButton ??= HudBindingUtility.FindChildRecursive(closeConfirmPanel, "btn_confirmClose")?.GetComponent<Button>();
            settlementPanel ??= HudBindingUtility.FindChildRecursive(hudRoot, "SettlementPanel") as RectTransform;
            var profitRoot = HudBindingUtility.FindChildRecursive(settlementPanel, "Profit");
            settlementProfitLabelText ??= profitRoot?.GetComponent<TMP_Text>();
            settlementProfitText ??= HudBindingUtility.FindChildRecursive(settlementPanel, "ProfitText")?.GetComponent<TMP_Text>();
            settlementProfitCoinTarget ??= profitRoot != null
                ? HudBindingUtility.FindChildRecursive(profitRoot, "Coin")
                : HudBindingUtility.FindChildRecursive(settlementPanel, "Coin");
            settlementRevenueText ??= HudBindingUtility.FindChildRecursive(settlementPanel, "RevenueText")?.GetComponent<TMP_Text>();
            settlementSalaryText ??= HudBindingUtility.FindChildRecursive(settlementPanel, "SalaryText")?.GetComponent<TMP_Text>();
            confirmSettlementButton ??= HudBindingUtility.FindChildRecursive(settlementPanel, "btn_confirmSettlement")?.GetComponent<Button>()
                                      ?? HudBindingUtility.FindChildRecursive(settlementPanel, "Confirm")?.GetComponent<Button>();
            EnsureSettlementMaskButton();
            EnsureSettlementSummaryItems();
            EnsureSettlementDetailText();
            if (!settlementPanelScaleCached && settlementPanel != null)
            {
                settlementPanelDefaultScale = settlementPanel.localScale;
                settlementPanelScaleCached = true;
            }

            if (openButton != null)
            {
                openButton.onClick.RemoveListener(OnClickOpenTavernBusiness);
                openButton.onClick.AddListener(OnClickOpenTavernBusiness);
            }

            // 手动打烊入口已停用：营业时长结束后由顶部倒计时自动关店。
            HideManualCloseUi();

            if (confirmSettlementButton != null)
            {
                confirmSettlementButton.onClick.RemoveListener(CloseSettlementPanel);
                confirmSettlementButton.onClick.AddListener(CloseSettlementPanel);
            }

            if (settlementMaskButton != null)
            {
                settlementMaskButton.onClick.RemoveListener(CloseSettlementPanel);
                settlementMaskButton.onClick.AddListener(CloseSettlementPanel);
            }
        }

        /// <summary>
        /// 结算遮罩点击与 Confirm 一致：关闭结算并写入打烊记录。
        /// </summary>
        private void EnsureSettlementMaskButton()
        {
            if (settlementPanel == null)
            {
                return;
            }

            var maskTransform = HudBindingUtility.FindChildRecursive(settlementPanel, "Mask");
            if (maskTransform == null)
            {
                return;
            }

            settlementMaskButton ??= maskTransform.GetComponent<Button>();
            if (settlementMaskButton == null)
            {
                settlementMaskButton = maskTransform.gameObject.AddComponent<Button>();
                var maskImage = maskTransform.GetComponent<Image>();
                if (maskImage != null)
                {
                    settlementMaskButton.targetGraphic = maskImage;
                }

                settlementMaskButton.transition = Selectable.Transition.None;
            }
        }

        /// <summary>
        /// 绑定结算面板前天 / 昨天 / 今天摘要条。
        /// </summary>
        private void EnsureSettlementSummaryItems()
        {
            if (settlementPanel == null || settlementSumItems != null)
            {
                return;
            }

            settlementSumItems = new SettlementSumItemBinding[SettlementSumItemNodeNames.Length];
            for (var index = 0; index < SettlementSumItemNodeNames.Length; index++)
            {
                var root = HudBindingUtility.FindChildRecursive(settlementPanel, SettlementSumItemNodeNames[index]);
                if (root == null)
                {
                    continue;
                }

                var group = HudBindingUtility.FindChildRecursive(root, "Group");
                settlementSumItems[index] = new SettlementSumItemBinding
                {
                    Root = root,
                    Title = HudBindingUtility.FindChildRecursive(root, "Title")?.GetComponent<TMP_Text>(),
                    Value = group != null
                        ? group.Find("Value")?.GetComponent<TMP_Text>()
                          ?? HudBindingUtility.FindChildRecursive(group, "Value")?.GetComponent<TMP_Text>()
                        : HudBindingUtility.FindChildRecursive(root, "Value")?.GetComponent<TMP_Text>(),
                    IncomeFill = HudBindingUtility.FindChildRecursive(root, "income")?.GetComponent<Image>(),
                    CostFill = HudBindingUtility.FindChildRecursive(root, "cost")?.GetComponent<Image>()
                };
            }
        }

        /// <summary>
        /// 在结算面板上确保详情文案节点（满意度 / 反馈）。
        /// </summary>
        private void EnsureSettlementDetailText()
        {
            if (settlementPanel == null)
            {
                return;
            }

            if (settlementDetailText == null)
            {
                var existing = HudBindingUtility.FindChildRecursive(settlementPanel, SettlementDetailNodeName);
                settlementDetailText = existing != null ? existing.GetComponent<TMP_Text>() : null;
            }

            if (settlementDetailText != null)
            {
                return;
            }

            var go = new GameObject(SettlementDetailNodeName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(settlementPanel, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(780f, 280f);
            rect.anchoredPosition = new Vector2(0f, -160f);

            settlementDetailText = go.GetComponent<TextMeshProUGUI>();
            settlementDetailText.font = TMP_Settings.defaultFontAsset;
            settlementDetailText.fontSize = 26f;
            settlementDetailText.color = new Color(0.35f, 0.22f, 0.1f, 1f);
            settlementDetailText.alignment = TextAlignmentOptions.TopLeft;
            settlementDetailText.enableWordWrapping = true;
            settlementDetailText.raycastTarget = false;
            settlementDetailText.lineSpacing = 8f;
        }

        /// <summary>
        /// 隐藏手动打烊按钮与确认弹层（营业不可由玩家主动关闭）。
        /// </summary>
        private void HideManualCloseUi()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(ShowCloseConfirmPanel);
                closeButton.gameObject.SetActive(false);
            }

            if (hideCloseConfirmButton != null)
            {
                hideCloseConfirmButton.onClick.RemoveListener(ShowCloseConfirmPanel);
            }

            if (confirmCloseButton != null)
            {
                confirmCloseButton.onClick.RemoveListener(OnClickCloseTavernBusiness);
            }

            if (closeConfirmPanel != null)
            {
                closeConfirmPanel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 打烊确认弹层已停用。
        /// </summary>
        private void ShowCloseConfirmPanel()
        {
            HideManualCloseUi();
        }

        /// <summary>
        /// 根据营业状态刷新开业按钮。
        /// </summary>
        private void RefreshOpenButtonState()
        {
            if (openButton == null)
            {
                return;
            }

            var sceneManager = TavernSceneManager.Instance;
            var canShow = DataManager.Instance != null
                          && !DataManager.Instance.IsVisitingOtherTavern
                          && DataManager.Instance.TavernData != null
                          && !DataManager.Instance.TavernData.isOpen
                          && GuideService.CanOpenBusiness()
                          && !waitingGrandOpeningVideo
                          && !IsWaitingSettlementConfirm
                          && !IsSettlementVisible
                          && (sceneManager == null || !sceneManager.IsClosingBusiness);

            if (canShow)
            {
                openButton.gameObject.SetActive(true);
                openButton.interactable = true;
            }
            else
            {
                openButton.interactable = false;
                openButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 点击开业按钮后正式进入营业状态；首次开业先播视频，结束后再开始经营。
        /// </summary>
        private void OnClickOpenTavernBusiness()
        {
            GameAudioManager.PlayButtonClick();
            if (DataManager.Instance == null || !GuideService.CanOpenBusiness())
            {
                RefreshPanel();
                return;
            }

            var isFirstOpening = DataManager.Instance.GetBusinessOpenCount() <= 0;

            openButton.gameObject.SetActive(false);
            openButton.interactable = false;

            if (isFirstOpening)
            {
                waitingGrandOpeningVideo = true;
                if (TryPlayGrandOpeningVideo(BeginOpenBusinessFlow))
                {
                    RefreshOpenButtonState();
                    return;
                }

                waitingGrandOpeningVideo = false;
            }

            BeginOpenBusinessFlow();
        }

        /// <summary>
        /// 真正开业并启动接客/经营流程。
        /// </summary>
        private void BeginOpenBusinessFlow()
        {
            if (DataManager.Instance == null)
            {
                return;
            }

            waitingGrandOpeningVideo = false;
            businessSessionStarted = true;

            if (TavernSceneManager.Instance != null)
            {
                TavernSceneManager.Instance.OpenTavernBusiness();
            }
            else
            {
                DataManager.Instance.ResetTransientTavernState();
                TavernBusinessModifierService.Instance.ResetAll();
                DataManager.Instance.SetTavernOpen(true);
            }

            DataManager.Instance.AddPendingSettlementCost(CalculateOpeningSalaryCost());
            GameAudioManager.PlayInspire();
            RefreshPanel();
            Data?.RootController?.RefreshAllPanels();
        }

        /// <summary>
        /// 首次开业：播放 grandOpeningVideo；播完回调后关闭视频并开始经营。
        /// </summary>
        /// <returns>已成功开播时返回 true。</returns>
        private static bool TryPlayGrandOpeningVideo(System.Action onFinished)
        {
            var clip = GameplayResourceStore.LoadAsset<VideoClip>(GrandOpeningVideoAssetPath);
            if (clip == null)
            {
                Debug.LogWarning($"[TavernBusinessFlowPanelController] 缺少开业视频：{GrandOpeningVideoAssetPath}");
                return false;
            }

            VideoWindowController.Show(clip, onFinished, pauseOnLastFrame: false);
            return true;
        }

        /// <summary>
        /// 确认打烊后通知场景开始关店流程。
        /// </summary>
        private void OnClickCloseTavernBusiness()
        {
            PrepareForCloseRequest();
            Data?.RootController?.RefreshAllPanels();
            TavernSceneManager.Instance?.CloseTavernBusiness();
        }

        /// <summary>
        /// 营业结束后显示结算面板。
        /// </summary>
        private void ShowSettlementPanel()
        {
            EnsureNodes();
            settlementCommitted = false;
            if (settlementPanel == null)
            {
                RefreshSettlementRevenue();
                FinalizeSettlementWithoutPanel();
                return;
            }

            IsWaitingSettlementConfirm = true;
            ResetSettlementPanelPresentationForShow();
            settlementPanel.gameObject.SetActive(true);
            settlementPanel.localScale = settlementPanelDefaultScale * Mathf.Max(0f, settlementPanelStartScale);
            settlementPanelTween = settlementPanel
                .DOScale(settlementPanelDefaultScale, Mathf.Max(0f, settlementPanelPopDuration))
                .SetEase(Ease.OutBack)
                .OnKill(() => settlementPanelTween = null);
            SetSettlementBgmDucked(true);
            RefreshSettlementRevenue(animateSumItemFill: true);
        }

        /// <summary>
        /// 不带动画地立即隐藏结算面板。
        /// </summary>
        private void HideSettlementPanelImmediately()
        {
            KillSettlementPanelTween();
            if (settlementPanel != null)
            {
                settlementPanel.localScale = settlementPanelDefaultScale;
                settlementPanel.gameObject.SetActive(false);
            }

            SetSettlementBgmDucked(false);
        }

        /// <summary>
        /// 关闭结算面板并写入打烊历史。
        /// </summary>
        private void CloseSettlementPanel()
        {
            CommitClosingRecordIfNeeded();
            GameAudioManager.PlaySettlementSuccess();

            if (settlementPanel != null)
            {
                KillSettlementPanelTween();
                settlementPanel.localScale = settlementPanelDefaultScale;
                settlementPanel.gameObject.SetActive(false);
            }

            SetSettlementBgmDucked(false);
            IsWaitingSettlementConfirm = false;
            RefreshOpenButtonState();
            Data?.RootController?.RefreshAllPanels();
        }

        private void SetSettlementBgmDucked(bool ducked)
        {
            if (settlementBgmDucked == ducked)
            {
                return;
            }

            settlementBgmDucked = ducked;
            if (ducked)
            {
                GameAudioManager.DuckBgmForOverlay();
            }
            else
            {
                GameAudioManager.UnduckBgmForOverlay();
            }
        }

        private void FinalizeSettlementWithoutPanel()
        {
            CommitClosingRecordIfNeeded();
            IsWaitingSettlementConfirm = false;
            RefreshOpenButtonState();
            Data?.RootController?.RefreshAllPanels();
        }

        private void CommitClosingRecordIfNeeded()
        {
            if (settlementCommitted || DataManager.Instance == null)
            {
                return;
            }

            DataManager.Instance.CommitClosingRecord();
            settlementCommitted = true;
        }

        /// <summary>
        /// 刷新结算收益、对比与满意度。
        /// </summary>
        private void RefreshSettlementRevenue(bool animateSumItemFill = false)
        {
            if (!animateSumItemFill && IsSettlementPresentationPlaying)
            {
                return;
            }

            var dataManager = DataManager.Instance;
            var gameplayData = dataManager != null ? dataManager.GameplayData : null;
            var revenue = gameplayData != null ? Mathf.Max(0, gameplayData.pendingSettlementIncome) : 0;
            var salary = gameplayData != null ? Mathf.Max(0, gameplayData.pendingSettlementCosts) : 0;
            var profit = revenue - salary;

            if (animateSumItemFill)
            {
                KillSettlementPresentationAnimation();
                SetSettlementBillTexts(0, 0);
                SetSettlementProfitTextVisible(false);
            }
            else
            {
                SetSettlementBillTexts(revenue, salary);
                SetSettlementProfitTextVisible(true, profit);
            }

            ApplySettlementProfitColors(profit);

            if (settlementDetailText != null && dataManager != null)
            {
                settlementDetailText.text = dataManager.BuildClosingSatisfactionDetailText();
            }

            if (animateSumItemFill)
            {
                var incomeRiseDuration = ComputeSettlementIncomeRiseDuration(dataManager);
                BeginSettlementPresentationAnimation(revenue, salary, profit, incomeRiseDuration);
                RefreshSettlementSummaryGroup(dataManager, animateSumItemFill: true);
            }
            else if (!IsSettlementVisible)
            {
                RefreshSettlementSummaryGroup(dataManager, animateSumItemFill: false);
            }

            ApplyProfitStageIcons(profit);
        }

        private void BeginSettlementPresentationAnimation(int revenue, int salary, int profit, float incomeRiseDuration)
        {
            var presentationDuration = Mathf.Max(incomeRiseDuration, MinSettlementIncomeRiseDuration);
            settlementPresentationActive = true;
            GameAudioManager.PlaySettlementIncomeRise(presentationDuration);

            KillSettlementPresentationAnimation();
            settlementBillAnimCoroutine = StartCoroutine(
                AnimateSettlementBillDuringIncome(revenue, salary, profit, presentationDuration));
        }

        private IEnumerator AnimateSettlementBillDuringIncome(int revenue, int salary, int profit, float duration)
        {
            var elapsed = 0f;
            var nextRefresh = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (elapsed >= nextRefresh)
                {
                    nextRefresh += SettlementBillTextRefreshInterval;
                    var progress = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
                    SetSettlementBillTexts(
                        Mathf.RoundToInt(revenue * progress),
                        Mathf.RoundToInt(salary * progress));
                }

                yield return null;
            }

            settlementBillAnimCoroutine = null;
            CompleteSettlementPresentation(revenue, salary, profit);
        }

        private void CompleteSettlementPresentation(int revenue, int salary, int profit)
        {
            settlementPresentationActive = false;
            SetSettlementBillTexts(revenue, salary);
            RevealSettlementProfitText(profit);
            GameAudioManager.PlayCheckoutCoins();

            if (settlementProfitCoinTarget == null)
            {
                return;
            }

            GameUIEffects.PlayCoinsFlyFromRandomScreen(settlementProfitCoinTarget);
        }

        private void SetSettlementBillTexts(int revenue, int salary)
        {
            if (settlementRevenueText != null)
            {
                settlementRevenueText.text = revenue.ToString();
            }

            if (settlementSalaryText != null)
            {
                settlementSalaryText.text = salary.ToString();
            }
        }

        private void SetSettlementProfitTextVisible(bool visible, int profit = 0)
        {
            if (settlementProfitText == null)
            {
                return;
            }

            if (settlementProfitRevealTween != null && settlementProfitRevealTween.IsActive())
            {
                settlementProfitRevealTween.Kill();
                settlementProfitRevealTween = null;
            }

            if (visible)
            {
                settlementProfitText.text = profit.ToString();
                var color = settlementProfitText.color;
                color.a = 1f;
                settlementProfitText.color = color;
                settlementProfitText.transform.localScale = Vector3.one;
                return;
            }

            settlementProfitText.text = string.Empty;
            var hiddenColor = settlementProfitText.color;
            hiddenColor.a = 0f;
            settlementProfitText.color = hiddenColor;
            settlementProfitText.transform.localScale = Vector3.one;
        }

        private void RevealSettlementProfitText(int profit)
        {
            if (settlementProfitText == null)
            {
                return;
            }

            if (settlementProfitRevealTween != null && settlementProfitRevealTween.IsActive())
            {
                settlementProfitRevealTween.Kill();
            }

            settlementProfitText.text = profit.ToString();
            var color = settlementProfitText.color;
            color.a = 0f;
            settlementProfitText.color = color;
            settlementProfitText.transform.localScale = Vector3.one * 0.86f;

            settlementProfitRevealTween = DOTween.Sequence()
                .Join(settlementProfitText.DOFade(1f, 0.22f))
                .Join(settlementProfitText.transform.DOScale(1f, 0.24f).SetEase(Ease.OutBack))
                .OnKill(() => settlementProfitRevealTween = null);
        }

        private float ComputeSettlementIncomeRiseDuration(DataManager dataManager)
        {
            EnsureSettlementSummaryItems();
            if (dataManager == null || settlementSumItems == null)
            {
                return 0f;
            }

            dataManager.GetClosingSummaryThreeDays(
                out var dayBeforeYesterday,
                out var yesterday,
                out var today);

            var barBaseline = ResolveSettlementBarBaseline(
                dayBeforeYesterday,
                yesterday,
                today);

            return ComputeMaxIncomeFillDuration(
                dayBeforeYesterday,
                yesterday,
                today,
                barBaseline);
        }

        private void RefreshSettlementSummaryGroup(DataManager dataManager, bool animateSumItemFill = false)
        {
            EnsureSettlementSummaryItems();
            if (dataManager == null || settlementSumItems == null)
            {
                return;
            }

            if (animateSumItemFill)
            {
                KillSettlementSumItemFillTweens(stopIncomeRiseSfx: false);
            }

            dataManager.GetClosingSummaryThreeDays(
                out var dayBeforeYesterday,
                out var yesterday,
                out var today);

            var barBaseline = ResolveSettlementBarBaseline(
                dayBeforeYesterday,
                yesterday,
                today);

            ApplySettlementSumItem(settlementSumItems[0], dayBeforeYesterday, barBaseline, animateSumItemFill);
            ApplySettlementSumItem(settlementSumItems[1], yesterday, barBaseline, animateSumItemFill);
            ApplySettlementSumItem(settlementSumItems[2], today, barBaseline, animateSumItemFill);
        }

        /// <summary>
        /// 收入/支出共用满格基准：全部低于 5000 时取 5000，否则取三天内收支最大值。
        /// </summary>
        private static float ResolveSettlementBarBaseline(
            SettlementSummaryDaySnapshot dayBeforeYesterday,
            SettlementSummaryDaySnapshot yesterday,
            SettlementSummaryDaySnapshot today)
        {
            var maxValue = Mathf.Max(
                dayBeforeYesterday.Income,
                dayBeforeYesterday.Spend,
                yesterday.Income,
                yesterday.Spend,
                today.Income,
                today.Spend);
            return Mathf.Max(SettlementBarBaseline, maxValue);
        }

        private float ComputeMaxIncomeFillDuration(
            SettlementSummaryDaySnapshot dayBeforeYesterday,
            SettlementSummaryDaySnapshot yesterday,
            SettlementSummaryDaySnapshot today,
            float barBaseline)
        {
            if (barBaseline <= 0f)
            {
                return 0f;
            }

            var maxTarget = Mathf.Max(
                Mathf.Clamp01(dayBeforeYesterday.Income / barBaseline),
                Mathf.Clamp01(yesterday.Income / barBaseline),
                Mathf.Clamp01(today.Income / barBaseline));
            return maxTarget * settlementSumItemFullFillDuration;
        }

        private void ApplySettlementSumItem(
            SettlementSumItemBinding binding,
            SettlementSummaryDaySnapshot snapshot,
            float barBaseline,
            bool animateFill)
        {
            if (binding.Root == null)
            {
                return;
            }

            if (binding.Title != null)
            {
                binding.Title.text = snapshot.DayLabel;
            }

            if (binding.Value != null)
            {
                binding.Value.text = FormatSignedProfit(snapshot.Profit);
            }

            var incomeTarget = barBaseline > 0f
                ? Mathf.Clamp01(snapshot.Income / barBaseline)
                : 0f;
            var costTarget = barBaseline > 0f
                ? Mathf.Clamp01(snapshot.Spend / barBaseline)
                : 0f;

            if (animateFill)
            {
                PlaySettlementFillTween(binding.IncomeFill, incomeTarget);
                PlaySettlementFillTween(binding.CostFill, costTarget);
                return;
            }

            if (binding.IncomeFill != null)
            {
                binding.IncomeFill.fillAmount = incomeTarget;
            }

            if (binding.CostFill != null)
            {
                binding.CostFill.fillAmount = costTarget;
            }
        }

        private void PlaySettlementFillTween(Image fillImage, float targetFill)
        {
            if (fillImage == null)
            {
                return;
            }

            fillImage.fillAmount = 0f;
            if (targetFill <= 0f)
            {
                return;
            }

            // 匀速：fillAmount 1.0 对应 settlementSumItemFullFillDuration，目标越大耗时越长。
            var duration = targetFill * settlementSumItemFullFillDuration;
            var tween = fillImage
                .DOFillAmount(targetFill, duration)
                .SetEase(Ease.Linear);
            settlementFillTweens.Add(tween);
            tween.OnKill(() => settlementFillTweens.Remove(tween));
        }

        private static string FormatSignedProfit(int profit)
        {
            return profit > 0 ? $"+{profit}" : profit.ToString();
        }

        private void ApplySettlementProfitColors(int profit)
        {
            var color = profit < 0 ? SettlementProfitNegativeColor : SettlementProfitPositiveColor;
            if (settlementProfitLabelText != null)
            {
                settlementProfitLabelText.color = color;
            }

            if (settlementProfitText != null)
            {
                color.a = settlementProfitText.color.a;
                settlementProfitText.color = color;
            }
        }

        private void ApplyProfitStageIcons(int profit)
        {
            if (settlementResultIcon == null && settlementResultTextBg == null && settlementResultTextIcon == null)
            {
                return;
            }

            Sprite bg;
            Sprite textBG;
            Sprite textIcon;
            if (profit >= highStage)
            {
                bg = highStageIcon;
                textBG = highStageTextBgIcon;
                textIcon = highStageTextIcon;
            }
            else if (profit >= middleStage)
            {
                bg = middleStageIcon;
                textBG = middleStageTextBgIcon;
                textIcon = middleStageTextIcon;
            }
            else
            {
                bg = lowStageIcon;
                textBG = lowStageTextBgIcon;
                textIcon = lowStageTextIcon;
            }

            if (settlementResultIcon != null)
            {
                settlementResultIcon.sprite = bg;
            }

            if (settlementResultTextBg != null)
            {
                settlementResultTextBg.sprite = textBG;
            }

            if (settlementResultTextIcon != null)
            {
                settlementResultTextIcon.sprite = textIcon;
            }
        }

        private int CalculateOpeningSalaryCost()
        {
            if (DataManager.Instance == null)
            {
                return 0;
            }

            return StaffConfigUtility.SumDailySalary(DataManager.Instance.GetOwnedStaffList());
        }

        /// <summary>
        /// 面板显示时同步当前营业会话状态。
        /// </summary>
        private void SyncBusinessStateOnShow()
        {
            var isOpen = DataManager.Instance != null
                         && DataManager.Instance.TavernData != null
                         && DataManager.Instance.TavernData.isOpen;
            businessSessionStarted = isOpen;

            if (!isOpen && !IsWaitingSettlementConfirm)
            {
                HideSettlementPanelImmediately();
            }
        }

        /// <summary>
        /// 打烊结算弹出前重置动画状态，但不打断即将重新开始的 up 音效。
        /// </summary>
        private void ResetSettlementPanelPresentationForShow()
        {
            KillSettlementPresentationAnimation();
            KillSettlementSumItemFillTweens(stopIncomeRiseSfx: false);

            if (settlementPanelTween != null && settlementPanelTween.IsActive())
            {
                settlementPanelTween.Kill();
            }

            settlementPanelTween = null;
        }

        /// <summary>
        /// 统一控制营业流程相关节点显隐。
        /// </summary>
        private void SetManagedNodesVisible(bool visible)
        {
            if (openButton != null)
            {
                openButton.gameObject.SetActive(visible && openButton.gameObject.activeSelf);
            }

            HideManualCloseUi();

            if (!visible)
            {
                if (settlementPanel != null)
                {
                    settlementPanel.gameObject.SetActive(false);
                }
            }
        }

        private void KillSettlementPanelTween()
        {
            settlementPresentationActive = false;
            KillSettlementPresentationAnimation();
            KillSettlementSumItemFillTweens();

            if (settlementPanelTween != null && settlementPanelTween.IsActive())
            {
                settlementPanelTween.Kill();
            }

            settlementPanelTween = null;
        }

        private void KillSettlementPresentationAnimation()
        {
            if (settlementBillAnimCoroutine != null)
            {
                StopCoroutine(settlementBillAnimCoroutine);
                settlementBillAnimCoroutine = null;
            }

            if (settlementProfitRevealTween != null && settlementProfitRevealTween.IsActive())
            {
                settlementProfitRevealTween.Kill();
            }

            settlementProfitRevealTween = null;
        }

        private void KillSettlementSumItemFillTweens(bool stopIncomeRiseSfx = true)
        {
            if (stopIncomeRiseSfx)
            {
                GameAudioManager.StopSettlementIncomeRise();
            }

            for (var index = settlementFillTweens.Count - 1; index >= 0; index--)
            {
                var tween = settlementFillTweens[index];
                if (tween != null && tween.IsActive())
                {
                    tween.Kill();
                }
            }

            settlementFillTweens.Clear();
        }

        private bool HasActiveSettlementFillTween()
        {
            for (var index = settlementFillTweens.Count - 1; index >= 0; index--)
            {
                var tween = settlementFillTweens[index];
                if (tween != null && tween.IsActive())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
