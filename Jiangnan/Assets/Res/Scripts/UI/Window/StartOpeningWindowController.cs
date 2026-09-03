using System.Collections;
using System.Collections.Generic;
using cfg;
using JN.Client;
using JN.Client.Config;
using JN.Client.Manager;
using JN.Client.Model;
using JN.Client.Scene;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    public class StartOpeningWindowControllerData : UIPanelData
    {
    }

    /// <summary>
    /// 负责开业引导窗口逻辑。
    /// </summary>
    public class StartOpeningWindowController : QFrameworkPanel<StartOpeningWindowControllerData>
    {
        [SerializeField] private Button btn_Opening;
        [SerializeField] private TextMeshProUGUI txt_OpeningInfo;
        [SerializeField] private GameObject group_OpeningEffect;

        [SerializeField] private RectTransform guideTaskPanel;
        [SerializeField] private TextMeshProUGUI guideTaskTitle;
        [SerializeField] private List<TextMeshProUGUI> guideTaskTexts = new();
        [SerializeField] private Button guidePrimaryActionButton;
        [SerializeField] private Button guideSecondaryActionButton;
        [SerializeField] private TextMeshProUGUI guidePrimaryActionText;
        [SerializeField] private TextMeshProUGUI guideSecondaryActionText;
        [SerializeField] private CanvasGroup guideToastCanvasGroup;
        [SerializeField] private TextMeshProUGUI guideToastText;

        private Coroutine guideToastRoutine;
        private TavernGuideService GuideService => TavernGuideService.Instance;

        /// <summary>
        /// 面板初始化时绑定控件和事件。
        /// </summary>
        protected override void OnPanelInit()
        {
            EnsureGuideUi();

            // @txt_OpeningInfo 已不再展示开业进度文案，统一交由 GuideTaskPanel 来呈现任务进度。
            if (txt_OpeningInfo != null)
            {
                txt_OpeningInfo.gameObject.SetActive(false);
            }

            Signals.Get<TableNumSignal>().AddListener(RefreshOpeningInfo);
            Signals.Get<TableNumSignal>().AddListener(RefreshGuideUi);
            Signals.Get<TavernRuntimeChangedSignal>().AddListener(RefreshOpeningInfo);
            Signals.Get<TavernRuntimeChangedSignal>().AddListener(RefreshGuideUi);
            Signals.Get<GameplayGuideProgressSignal>().AddListener(RefreshGuideUi);
        }

        /// <summary>
        /// 响应面板显示事件并同步状态。
        /// </summary>
        protected override void OnPanelShow()
        {
            RefreshOpeningInfo();
            RefreshGuideUi();
        }

        /// <summary>
        /// 面板关闭时清理临时状态和监听。
        /// </summary>
        protected override void OnPanelClose()
        {
            if (btn_Opening != null)
            {
                btn_Opening.onClick.RemoveListener(OnClickBtnOpening);
            }

            Signals.Get<TableNumSignal>().RemoveListener(RefreshOpeningInfo);
            Signals.Get<TableNumSignal>().RemoveListener(RefreshGuideUi);
            Signals.Get<TavernRuntimeChangedSignal>().RemoveListener(RefreshOpeningInfo);
            Signals.Get<TavernRuntimeChangedSignal>().RemoveListener(RefreshGuideUi);
            Signals.Get<GameplayGuideProgressSignal>().RemoveListener(RefreshGuideUi);

            if (guideToastRoutine != null)
            {
                StopCoroutine(guideToastRoutine);
                guideToastRoutine = null;
            }
        }

        /// <summary>
        /// 处理开业按钮点击并切换酒楼营业状态。
        /// </summary>
        private void OnClickBtnOpening()
        {
            // 开业入口已迁移到 TavernStatusBarPanelController.GuidePanel.OpenBtn，此窗口暂不响应开业。
        }

        /// <summary>
        /// 刷新开局信息。
        /// 当前需求下顶部不再展示开业引导文本（统一在 GuideTaskPanel 内呈现任务），
        /// 这里仅保留为占位，确保旧字段不会再被赋值，但仍允许将组件隐藏。
        /// </summary>
        private void RefreshOpeningInfo()
        {
            if (txt_OpeningInfo != null && txt_OpeningInfo.gameObject.activeSelf)
            {
                txt_OpeningInfo.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 刷新引导界面。
        /// </summary>
        private void RefreshGuideUi()
        {
            EnsureGuideUi();
            var presentation = GuidePresentationAdapter.BuildStartOpeningPresentation(GuideService);

            if (guideTaskPanel != null)
            {
                guideTaskPanel.gameObject.SetActive(presentation.Tasks.Count > 0);
            }

            if (guideTaskTitle != null)
            {
                guideTaskTitle.text = presentation.Title;
            }

            for (var index = 0; index < guideTaskTexts.Count; index++)
            {
                if (guideTaskTexts[index] == null)
                {
                    continue;
                }

                if (index >= presentation.Tasks.Count)
                {
                    guideTaskTexts[index].gameObject.SetActive(false);
                    continue;
                }

                var task = presentation.Tasks[index];
                guideTaskTexts[index].gameObject.SetActive(true);
                guideTaskTexts[index].text = task.Text;
                guideTaskTexts[index].color = task.Color;
            }

            if (btn_Opening != null)
            {
                // 旧开业按钮保留在预制体中，但当前流程不再从此窗口触发开业。
                btn_Opening.gameObject.SetActive(false);
                btn_Opening.interactable = false;
            }

            UpdateGuideActionButtons(presentation);

            if (guideTaskPanel != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(guideTaskPanel);
            }
        }

        /// <summary>
        /// 更新引导操作按钮。
        /// </summary>
        /// <param name="snapshot">参数值。</param>
        private void UpdateGuideActionButtons(StartOpeningGuidePresentation presentation)
        {
            if (GuideService.IsBusinessOpen())
            {
                BindGuideActionButton(guidePrimaryActionButton, guidePrimaryActionText, false, string.Empty, null);
                BindGuideActionButton(guideSecondaryActionButton, guideSecondaryActionText, false, string.Empty, null);
                return;
            }

            var primaryAction = presentation.Actions.Count > 0 ? presentation.Actions[0] : null;
            var secondaryAction = presentation.Actions.Count > 1 ? presentation.Actions[1] : null;
            BindGuideActionButton(
                guidePrimaryActionButton,
                guidePrimaryActionText,
                primaryAction != null,
                primaryAction != null ? primaryAction.Label : string.Empty,
                ResolveGuideActionHandler(primaryAction));
            BindGuideActionButton(
                guideSecondaryActionButton,
                guideSecondaryActionText,
                secondaryAction != null,
                secondaryAction != null ? secondaryAction.Label : string.Empty,
                ResolveGuideActionHandler(secondaryAction));
        }

        /// <summary>
        /// 处理购买柜台并播放搬运表现。
        /// </summary>
        private void HandleBuyCounter()
        {
            GuideService.TryPurchaseCounter(out var message);
            ShowGuideToast(message, 1.6f);
            RefreshOpeningInfo();
            RefreshGuideUi();
        }

        /// <summary>
        /// 处理购买灶台并播放搬运表现。
        /// </summary>
        private void HandleBuyStove()
        {
            GuideService.TryPurchaseStove(out var message);
            ShowGuideToast(message, 1.6f);
            RefreshOpeningInfo();
            RefreshGuideUi();
        }

        /// <summary>
        /// 处理招聘掌柜。
        /// </summary>
        private void HandleHireShopkeeper()
        {
            if (TavernSceneManager.Instance != null)
            {
                TavernSceneManager.Instance.RequestGuideHireShopkeeper();
                return;
            }

            GuideService.TryHireShopkeeper(out var message);
            ShowGuideToast(message, 1.6f);
            RefreshOpeningInfo();
            RefreshGuideUi();
        }

        /// <summary>
        /// 处理招聘厨师。
        /// </summary>
        private void HandleHireChef()
        {
            if (TavernSceneManager.Instance != null)
            {
                TavernSceneManager.Instance.RequestGuideHireChef();
                return;
            }

            GuideService.TryHireChef(out var message);
            ShowGuideToast(message, 1.6f);
            RefreshOpeningInfo();
            RefreshGuideUi();
        }

        /// <summary>
        /// 处理招聘小二。
        /// </summary>
        private void HandleHireWaiter()
        {
            if (TavernSceneManager.Instance != null)
            {
                TavernSceneManager.Instance.RequestGuideHireWaiter();
                return;
            }

            GuideService.TryHireWaiter(out var message);
            ShowGuideToast(message, 1.6f);
            RefreshOpeningInfo();
            RefreshGuideUi();
        }

        /// <summary>
        /// 确保引导界面。
        /// </summary>
        private void EnsureGuideUi()
        {
            guideTaskPanel ??= transform.Find("GuideTaskPanel") as RectTransform;
            guideTaskTitle ??= guideTaskPanel != null ? guideTaskPanel.Find("GuideTaskTitle")?.GetComponent<TextMeshProUGUI>() : null;

            if (guideTaskTexts.Count == 0 && guideTaskPanel != null)
            {
                for (var index = 0; index < 3; index++)
                {
                    var taskText = guideTaskPanel.Find($"GuideTask_{index}")?.GetComponent<TextMeshProUGUI>();
                    if (taskText != null)
                    {
                        guideTaskTexts.Add(taskText);
                    }
                }
            }

            guidePrimaryActionButton ??= transform.Find("GuidePrimaryActionButton")?.GetComponent<Button>();
            guideSecondaryActionButton ??= transform.Find("GuideSecondaryActionButton")?.GetComponent<Button>();
            guidePrimaryActionText ??= guidePrimaryActionButton != null ? guidePrimaryActionButton.GetComponentInChildren<TextMeshProUGUI>(true) : null;
            guideSecondaryActionText ??= guideSecondaryActionButton != null ? guideSecondaryActionButton.GetComponentInChildren<TextMeshProUGUI>(true) : null;
            guideToastCanvasGroup ??= transform.Find("GuideToast")?.GetComponent<CanvasGroup>();
            guideToastText ??= guideToastCanvasGroup != null ? guideToastCanvasGroup.GetComponentInChildren<TextMeshProUGUI>(true) : null;

            if (guideTaskPanel == null
                || guideTaskTitle == null
                || guideTaskTexts.Count < 3
                || guidePrimaryActionButton == null
                || guideSecondaryActionButton == null
                || guidePrimaryActionText == null
                || guideSecondaryActionText == null
                || guideToastCanvasGroup == null
                || guideToastText == null)
            {
                Debug.LogWarning("[StartOpeningWindowController] 缺少静态引导 UI 节点，请检查 prefab 配置。");
            }
        }

        /// <summary>
        /// 处理绑定引导操作按钮相关逻辑。
        /// </summary>
        /// <param name="button">按钮对象。</param>
        /// <param name="buttonText">按钮对象。</param>
        /// <param name="visible">参数值。</param>
        /// <param name="label">参数值。</param>
        /// <param name="onClick">参数值。</param>
        private void BindGuideActionButton(Button button, TextMeshProUGUI buttonText, bool visible, string label, UnityEngine.Events.UnityAction onClick)
        {
            if (button == null)
            {
                return;
            }

            button.gameObject.SetActive(visible);
            button.onClick.RemoveAllListeners();
            if (!visible)
            {
                return;
            }

            if (buttonText != null)
            {
                buttonText.text = label;
            }

            if (onClick != null)
            {
                button.onClick.AddListener(() =>
                {
                    GameAudioManager.PlayButtonClick();
                    onClick.Invoke();
                });
            }
        }

        /// <summary>
        /// 按顺序把招聘按钮绑定到主按钮或副按钮。
        /// </summary>
        /// <param name="boundButtonCount">当前已经绑定的按钮数量。</param>
        /// <param name="visible">当前招聘项是否需要显示。</param>
        /// <param name="label">按钮文案。</param>
        /// <param name="onClick">点击回调。</param>
        private UnityEngine.Events.UnityAction ResolveGuideActionHandler(GuideActionPresentation action)
        {
            if (action == null)
            {
                return null;
            }

            return action.Kind switch
            {
                GuideActionKind.BuyCounter => HandleBuyCounter,
                GuideActionKind.BuyStove => HandleBuyStove,
                GuideActionKind.HireShopkeeper => HandleHireShopkeeper,
                GuideActionKind.HireChef => HandleHireChef,
                GuideActionKind.HireWaiter => HandleHireWaiter,
                _ => null
            };
        }

        /// <summary>
        /// 显示引导提示。
        /// </summary>
        /// <param name="message">参数值。</param>
        /// <param name="duration">持续时间。</param>
        private void ShowGuideToast(string message, float duration)
        {
            EnsureGuideUi();
            if (guideToastText == null || guideToastCanvasGroup == null)
            {
                return;
            }

            if (guideToastRoutine != null)
            {
                StopCoroutine(guideToastRoutine);
            }

            guideToastText.text = message;
            guideToastRoutine = StartCoroutine(GuideToastRoutine(duration));
        }

        /// <summary>
        /// 按持续时间显示引导提示。
        /// </summary>
        /// <param name="duration">持续时间。</param>
        /// <returns>协程迭代器。</returns>
        private IEnumerator GuideToastRoutine(float duration)
        {
            guideToastCanvasGroup.alpha = 1f;
            yield return new WaitForSeconds(duration);

            const float fadeDuration = 0.25f;
            var elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                guideToastCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }

            guideToastCanvasGroup.alpha = 0f;
            guideToastRoutine = null;
        }

    }

    public enum GuideActionKind
    {
        None = 0,
        BuyCounter = 1,
        BuyStove = 2,
        HireShopkeeper = 3,
        HireChef = 4,
        HireWaiter = 5
    }

    public sealed class GuideTaskLinePresentation
    {
        public string Text { get; set; }
        public Color Color { get; set; }
        public bool IsCompleted { get; set; }
    }

    public sealed class GuideActionPresentation
    {
        public GuideActionKind Kind { get; set; }
        public string Label { get; set; }
    }

    public sealed class TavernGuidePanelPresentation
    {
        public bool ShouldShowPanel { get; set; }
        public bool ShouldShowDetails { get; set; }
        public bool CanOpenBusiness { get; set; }
        public int BuildCurrent { get; set; }
        public int BuildTarget { get; set; }
        public int EmployCurrent { get; set; }
        public int EmployTarget { get; set; }
    }

    public sealed class StartOpeningGuidePresentation
    {
        public string Title { get; set; }
        public List<GuideTaskLinePresentation> Tasks { get; } = new();
        public List<GuideActionPresentation> Actions { get; } = new();
    }

    public sealed class GuideWorldPresentation
    {
        public bool ShowCounterPurchase { get; set; }
        public bool ShowStovePurchase { get; set; }
        public bool ShowShopkeeperRecruit { get; set; }
        public bool ShowChefRecruit { get; set; }
        public bool ShowWaiterRecruit { get; set; }
        public bool ShowNextCustomerTimer { get; set; }
    }

    /// <summary>
    /// 把引导快照转换成 UI / 世界表现可直接消费的数据，避免各处重复拼装。
    /// </summary>
    public static class GuidePresentationAdapter
    {
        private const int CounterEquipmentId = 0;
        private const int StoveEquipmentId = 3;
        private const int ShopkeeperStaffId = 1;
        private const int ChefStaffId = 4;
        private const int WaiterStaffId = 5;

        private static readonly Color PendingTaskColor = new(1f, 0.94f, 0.76f, 1f);
        private static readonly Color CompletedTaskColor = new(0.56f, 1f, 0.57f, 1f);

        public static TavernGuidePanelPresentation BuildTavernGuidePanelPresentation(TavernGuideService guideService)
        {
            var shouldShowPanel = guideService != null && guideService.ShouldShowGuidePanel();
            return new TavernGuidePanelPresentation
            {
                ShouldShowPanel = shouldShowPanel,
                ShouldShowDetails = shouldShowPanel,
                CanOpenBusiness = guideService != null && guideService.CanOpenBusiness(),
                BuildCurrent = guideService != null ? guideService.GetBuildProgressCurrent() : 0,
                BuildTarget = guideService != null ? guideService.GetBuildProgressTarget() : 0,
                EmployCurrent = guideService != null ? guideService.GetEmployProgressCurrent() : 0,
                EmployTarget = guideService != null ? guideService.GetEmployProgressTarget() : 0
            };
        }

        public static StartOpeningGuidePresentation BuildStartOpeningPresentation(TavernGuideService guideService)
        {
            var presentation = new StartOpeningGuidePresentation();
            var snapshot = guideService?.GetSnapshot();
            if (snapshot == null)
            {
                presentation.Title = "主线任务";
                return presentation;
            }

            presentation.Title = snapshot.Stage == GameplayGuideStage.Recruit ? "主线任务: 招聘" : "主线任务: 开店";
            for (var index = 0; index < snapshot.ActiveTasks.Count; index++)
            {
                var task = snapshot.ActiveTasks[index];
                if (task == null)
                {
                    continue;
                }

                presentation.Tasks.Add(new GuideTaskLinePresentation
                {
                    Text = $"• {task.Title} ({task.Current}/{task.Target})",
                    Color = task.IsCompleted ? CompletedTaskColor : PendingTaskColor,
                    IsCompleted = task.IsCompleted
                });
            }

            if (guideService != null && !guideService.IsBusinessOpen())
            {
                if (snapshot.Stage == GameplayGuideStage.Build)
                {
                    if (guideService.ShouldShowCounterPurchase())
                    {
                        presentation.Actions.Add(new GuideActionPresentation
                        {
                            Kind = GuideActionKind.BuyCounter,
                            Label = $"购买掌柜桌\n{guideService.GetEquipmentCost(CounterEquipmentId)} 铜钱"
                        });
                    }

                    if (guideService.ShouldShowStovePurchase())
                    {
                        presentation.Actions.Add(new GuideActionPresentation
                        {
                            Kind = GuideActionKind.BuyStove,
                            Label = $"购买灶台\n{guideService.GetEquipmentCost(StoveEquipmentId)} 铜钱"
                        });
                    }
                }
                else if (snapshot.Stage == GameplayGuideStage.Recruit)
                {
                    var shopkeeperId = ShopkeeperStaffId;
                    var chefId = ChefStaffId;
                    var waiterId = WaiterStaffId;

                    if (guideService.ShouldShowShopkeeperRecruit())
                    {
                        presentation.Actions.Add(new GuideActionPresentation
                        {
                            Kind = GuideActionKind.HireShopkeeper,
                            Label = $"招聘{StaffConfigUtility.GetName(shopkeeperId, "掌柜")}\n{guideService.GetStaffCost(shopkeeperId, StaffRole.Waiter)} 铜钱"
                        });
                    }

                    if (guideService.ShouldShowChefRecruit())
                    {
                        presentation.Actions.Add(new GuideActionPresentation
                        {
                            Kind = GuideActionKind.HireChef,
                            Label = $"招聘{StaffConfigUtility.GetName(chefId, "厨师")}\n{guideService.GetStaffCost(chefId, StaffRole.Chef)} 铜钱"
                        });
                    }

                    if (guideService.ShouldShowWaiterRecruit())
                    {
                        presentation.Actions.Add(new GuideActionPresentation
                        {
                            Kind = GuideActionKind.HireWaiter,
                            Label = $"招聘{StaffConfigUtility.GetName(waiterId, "小二")}\n{guideService.GetStaffCost(waiterId, StaffRole.Waiter)} 铜钱"
                        });
                    }
                }
            }

            return presentation;
        }

        public static GuideWorldPresentation BuildWorldPresentation(TavernGuideService guideService)
        {
            return new GuideWorldPresentation
            {
                ShowCounterPurchase = guideService != null && guideService.ShouldShowCounterPurchase(),
                ShowStovePurchase = guideService != null && guideService.ShouldShowStovePurchase(),
                ShowShopkeeperRecruit = guideService != null && guideService.ShouldShowShopkeeperRecruit(),
                ShowChefRecruit = guideService != null && guideService.ShouldShowChefRecruit(),
                ShowWaiterRecruit = guideService != null && guideService.ShouldShowWaiterRecruit(),
                ShowNextCustomerTimer = guideService != null && guideService.IsBusinessOpen()
            };
        }
    }
}
