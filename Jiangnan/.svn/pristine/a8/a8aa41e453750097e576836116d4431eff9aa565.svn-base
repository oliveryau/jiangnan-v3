using System.Collections;
using System.Collections.Generic;
using cfg;
using JN.Client;
using JN.Client.Manager;
using JN.Client.Model;
using JN.Client.Scene;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    /// <summary>
    /// Tavern 引导面板。
    /// 负责顶部引导、开业任务区和招募解锁提示。
    /// </summary>
    public class TavernGuidePanelController : HudPanelController<TavernHudPanelData>
    {
        private const string StartOpeningVisualPrefabPath = "Assets/Res/Resources/UI/Window/StartOpeningWindowController.prefab";

        private RectTransform guidePanel;
        private Slider guideProgress;
        private Image guideBuildImage;
        private Image guideEmployImage;

        private RectTransform guideTaskPanel;
        private TextMeshProUGUI guideTaskTitle;
        private readonly List<TextMeshProUGUI> guideTaskTexts = new();
        private readonly List<GameObject> guideTaskCompleteMarks = new();
        private Button guidePrimaryActionButton;
        private Button guideSecondaryActionButton;
        private TextMeshProUGUI guidePrimaryActionText;
        private TextMeshProUGUI guideSecondaryActionText;
        private CanvasGroup guideToastCanvasGroup;
        private TextMeshProUGUI guideToastText;

        private Coroutine guideToastRoutine;
        private GameObject startOpeningVisual;
        private Color guideBuildDefaultColor;
        private Color guideEmployDefaultColor;
        private bool hasGuideBuildDefaultColor;
        private bool hasGuideEmployDefaultColor;

        private TavernGuideService GuideService => TavernGuideService.Instance;

        /// <summary>
        /// 打开时缓存顶部引导和开业任务视觉节点。
        /// </summary>
        protected override void OnPanelOpen(TavernHudPanelData data)
        {
            EnsureGuidePanel();
            EnsureStartOpeningVisual();
        }

        /// <summary>
        /// 显示时恢复引导区域并刷新数据。
        /// </summary>
        protected override void OnPanelShow()
        {
            Signals.Get<AchievementProgressSignal>().RemoveListener(HandleAchievementTaskChanged);
            Signals.Get<AchievementProgressSignal>().AddListener(HandleAchievementTaskChanged);
            Signals.Get<GameplayGuideProgressSignal>().RemoveListener(HandleAchievementTaskChanged);
            Signals.Get<GameplayGuideProgressSignal>().AddListener(HandleAchievementTaskChanged);
            Signals.Get<TavernRuntimeChangedSignal>().RemoveListener(HandleAchievementTaskChanged);
            Signals.Get<TavernRuntimeChangedSignal>().AddListener(HandleAchievementTaskChanged);

            SetManagedNodesVisible(true);
            EnsureGuidePanel();
            EnsureStartOpeningVisual();
            RefreshPanel();
        }

        /// <summary>
        /// 关闭时移除监听、停止提示协程并销毁兼容视觉壳。
        /// </summary>
        protected override void OnPanelClose()
        {
            Signals.Get<AchievementProgressSignal>().RemoveListener(HandleAchievementTaskChanged);
            Signals.Get<GameplayGuideProgressSignal>().RemoveListener(HandleAchievementTaskChanged);
            Signals.Get<TavernRuntimeChangedSignal>().RemoveListener(HandleAchievementTaskChanged);

            if (guideToastRoutine != null)
            {
                StopCoroutine(guideToastRoutine);
                guideToastRoutine = null;
            }

            SetManagedNodesVisible(false);
            if (startOpeningVisual != null)
            {
                Destroy(startOpeningVisual);
                startOpeningVisual = null;
            }
        }

        private void HandleAchievementTaskChanged()
        {
            RefreshGuideTaskPresentation();
        }

        /// <summary>
        /// 刷新引导面板和开业任务展示。
        /// </summary>
        public void RefreshPanel()
        {
            EnsureGuidePanel();
            EnsureStartOpeningVisual();
            if (DataManager.Instance == null || DataManager.Instance.TavernData == null)
            {
                return;
            }

            RefreshCompactGuidePanel();
            RefreshGuideTaskPresentation();
        }

        /// <summary>
        /// 查找并缓存顶部紧凑引导区域（整块 GuidePanel 暂隐藏）。
        /// </summary>
        private void EnsureGuidePanel()
        {
            var hudRoot = transform;

            guidePanel ??= HudBindingUtility.FindChildRecursive(hudRoot, "GuidePanel") as RectTransform;
            guideProgress ??= guidePanel != null ? guidePanel.Find("GuideProgress")?.GetComponent<Slider>() : null;

            if (guidePanel != null)
            {
                guidePanel.gameObject.SetActive(false);
            }

            if (guideBuildImage == null)
            {
                guideBuildImage = ResolveGuideStateImage("Build", "@img_BuildState");
            }

            if (guideEmployImage == null)
            {
                guideEmployImage = ResolveGuideStateImage("Employ", "@img_EmployState");
            }

        }

        /// <summary>
        /// 确保兼容旧 StartOpeningWindow 的视觉壳已挂到 HUD 下。
        /// </summary>
        private void EnsureStartOpeningVisual()
        {
            if (startOpeningVisual != null || HudBindingUtility.FindChildRecursive(transform, "GuideTaskPanel") != null)
            {
                EnsureGuideTaskUi();
                return;
            }

            var prefab = GameplayResourceStore.LoadAsset<GameObject>(StartOpeningVisualPrefabPath);
            if (prefab == null)
            {
                return;
            }

            startOpeningVisual = Instantiate(prefab, transform, false);
            startOpeningVisual.name = "TavernGuideWindowVisual";

            var legacyController = startOpeningVisual.GetComponent<StartOpeningWindowController>();
            if (legacyController != null)
            {
                Destroy(legacyController);
            }

            EnsureGuideTaskUi();
        }

        /// <summary>
        /// 查找并缓存开业任务区节点。
        /// </summary>
        private void EnsureGuideTaskUi()
        {
            var root = startOpeningVisual != null ? startOpeningVisual.transform : transform;
            guideTaskPanel ??= HudBindingUtility.FindChildRecursive(root, "GuideTaskPanel") as RectTransform;
            guideTaskTitle ??= HudBindingUtility.FindChildRecursive(root, "GuideTaskTitle")?.GetComponent<TextMeshProUGUI>();
            HideGuideTaskPrestigeText(root);

            if (guideTaskTexts.Count == 0 && guideTaskPanel != null)
            {
                for (var index = 0; index < 3; index++)
                {
                    var taskRoot = HudBindingUtility.FindChildRecursive(guideTaskPanel, $"GuideTask_{index}");
                    var taskText = taskRoot != null ? taskRoot.GetComponent<TextMeshProUGUI>() : null;
                    if (taskText != null)
                    {
                        guideTaskTexts.Add(taskText);
                    }

                    var completeMark = taskRoot != null ? taskRoot.Find("Complete")?.gameObject : null;
                    guideTaskCompleteMarks.Add(completeMark);
                }
            }

            guidePrimaryActionButton ??= HudBindingUtility.FindChildRecursive(root, "GuidePrimaryActionButton")?.GetComponent<Button>();
            guideSecondaryActionButton ??= HudBindingUtility.FindChildRecursive(root, "GuideSecondaryActionButton")?.GetComponent<Button>();
            guidePrimaryActionText ??= guidePrimaryActionButton != null ? guidePrimaryActionButton.GetComponentInChildren<TextMeshProUGUI>(true) : null;
            guideSecondaryActionText ??= guideSecondaryActionButton != null ? guideSecondaryActionButton.GetComponentInChildren<TextMeshProUGUI>(true) : null;
            guideToastCanvasGroup ??= HudBindingUtility.FindChildRecursive(root, "GuideToast")?.GetComponent<CanvasGroup>();
            guideToastText ??= guideToastCanvasGroup != null ? guideToastCanvasGroup.GetComponentInChildren<TextMeshProUGUI>(true) : null;

            HideLegacyOpeningChrome(root);
        }

        /// <summary>引导任务区不再展示声望奖励文案。</summary>
        private static void HideGuideTaskPrestigeText(Transform root)
        {
            var prestigeText = HudBindingUtility.FindChildRecursive(root, "txt_presitige");
            if (prestigeText != null)
            {
                prestigeText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 隐藏开业窗兼容壳里除任务区外的旧控件（含全屏黑底，避免挡住酒楼操作）。
        /// </summary>
        private void HideLegacyOpeningChrome(Transform root)
        {
            if (root == null)
            {
                return;
            }

            // 只保留 GuideTaskPanel 所在链路，其余子节点（黑底、开业按钮等）全部关掉。
            for (var index = 0; index < root.childCount; index++)
            {
                var child = root.GetChild(index);
                if (child == null)
                {
                    continue;
                }

                if (guideTaskPanel != null && (child == guideTaskPanel || child.name == "GuideTaskPanel"))
                {
                    child.gameObject.SetActive(true);
                    continue;
                }

                if (guideTaskPanel != null && guideTaskPanel.IsChildOf(child))
                {
                    child.gameObject.SetActive(true);
                    for (var nested = 0; nested < child.childCount; nested++)
                    {
                        var nestedChild = child.GetChild(nested);
                        if (nestedChild == null)
                        {
                            continue;
                        }

                        nestedChild.gameObject.SetActive(
                            nestedChild == guideTaskPanel || nestedChild.name == "GuideTaskPanel");
                    }

                    continue;
                }

                child.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 刷新顶部紧凑引导区（GuidePanel 整块暂隐藏，保留刷新入口便于日后恢复）。
        /// </summary>
        private void RefreshCompactGuidePanel()
        {
            if (guidePanel != null)
            {
                guidePanel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 常驻任务区：按成就表 Id 串行展示当前任务 Desc。
        /// </summary>
        private void RefreshGuideTaskPresentation()
        {
            EnsureGuideTaskUi();
            var dataManager = DataManager.Instance;
            var visiting = dataManager != null && dataManager.IsVisitingOtherTavern;
            Achievement task = !visiting && dataManager != null ? dataManager.GetCurrentAchievementTask() : null;
            var show = task != null;

            if (startOpeningVisual != null)
            {
                startOpeningVisual.SetActive(show);
                if (show)
                {
                    HideLegacyOpeningChrome(startOpeningVisual.transform);
                }
            }

            if (guideTaskPanel != null)
            {
                guideTaskPanel.gameObject.SetActive(show);
            }

            if (!show)
            {
                HudOverlayService.TryShowGuideTaskDialogs();
                return;
            }

            if (guideTaskTitle != null)
            {
                guideTaskTitle.text = string.IsNullOrEmpty(task.Name) ? "任务" : task.Name;
            }

            for (var index = 0; index < guideTaskTexts.Count; index++)
            {
                var taskText = guideTaskTexts[index];
                if (taskText == null)
                {
                    continue;
                }

                var isPrimary = index == 0;
                taskText.gameObject.SetActive(isPrimary);
                if (isPrimary)
                {
                    taskText.text = string.IsNullOrEmpty(task.Desc) ? task.Name : task.Desc;
                }

                if (index < guideTaskCompleteMarks.Count && guideTaskCompleteMarks[index] != null)
                {
                    guideTaskCompleteMarks[index].SetActive(false);
                }
            }

            HudOverlayService.TryShowGuideTaskDialogs();
        }

        /// <summary>
        /// 根据展示数据绑定主次行动按钮。
        /// </summary>
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
        /// 统一设置单个引导按钮的文案、显隐和点击事件。
        /// </summary>
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
        /// 将展示层动作映射为真实业务处理函数。
        /// </summary>
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
        /// 处理购买柜台动作。
        /// </summary>
        private void HandleBuyCounter()
        {
            GuideService.TryPurchaseCounter(out var message);
            ShowGuideToast(message, 1.6f);
            Data?.RootController?.RefreshAllPanels();
        }

        /// <summary>
        /// 处理购买灶台动作。
        /// </summary>
        private void HandleBuyStove()
        {
            GuideService.TryPurchaseStove(out var message);
            ShowGuideToast(message, 1.6f);
            Data?.RootController?.RefreshAllPanels();
        }

        /// <summary>
        /// 处理雇佣掌柜动作。
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
            Data?.RootController?.RefreshAllPanels();
        }

        /// <summary>
        /// 处理雇佣厨师动作。
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
            Data?.RootController?.RefreshAllPanels();
        }

        /// <summary>
        /// 处理雇佣小二动作。
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
            Data?.RootController?.RefreshAllPanels();
        }

        /// <summary>
        /// 显示短时引导提示。
        /// </summary>
        private void ShowGuideToast(string message, float duration)
        {
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
        /// 引导提示的淡出协程。
        /// </summary>
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

        /// <summary>
        /// 统一控制本面板托管节点的显隐。
        /// </summary>
        private void SetManagedNodesVisible(bool visible)
        {
            // GuidePanel 暂隐藏；任务区由 RefreshGuideTaskPresentation 按当前成就任务决定。
            if (guidePanel != null)
            {
                guidePanel.gameObject.SetActive(false);
            }

            if (!visible)
            {
                if (startOpeningVisual != null)
                {
                    startOpeningVisual.SetActive(false);
                }

                if (guideTaskPanel != null)
                {
                    guideTaskPanel.gameObject.SetActive(false);
                }

                return;
            }

            RefreshGuideTaskPresentation();
        }

        /// <summary>
        /// 控制引导详情区的显隐。
        /// </summary>
        private void SetGuideDetailVisible(bool visible)
        {
            SetGuideStateGroupVisible(guideBuildImage, visible);
            SetGuideStateGroupVisible(guideEmployImage, visible);
        }

        /// <summary>
        /// 控制单个引导状态组显隐。
        /// </summary>
        private static void SetGuideStateGroupVisible(Image image, bool visible)
        {
            if (image == null)
            {
                return;
            }

            var group = image.transform.parent != null ? image.transform.parent.gameObject : image.gameObject;
            group.SetActive(visible);
        }

        /// <summary>
        /// 解析引导状态图对应的图片组件。
        /// </summary>
        private Image ResolveGuideStateImage(string groupName, string legacyImagePath)
        {
            if (guidePanel == null)
            {
                return null;
            }

            var legacyImage = guidePanel.Find(legacyImagePath)?.GetComponent<Image>();
            if (legacyImage != null)
            {
                return legacyImage;
            }

            var stateGroup = guidePanel.Find(groupName);
            if (stateGroup == null)
            {
                stateGroup = HudBindingUtility.FindChildRecursive(guidePanel, groupName);
            }

            if (stateGroup == null)
            {
                return null;
            }

            return stateGroup.GetComponentInChildren<Image>(true);
        }

        /// <summary>
        /// 根据完成状态设置引导图标颜色。
        /// </summary>
        private static void SetGuideStateImageColor(Image image, bool completed, ref Color defaultColor, ref bool hasDefaultColor)
        {
            if (image == null)
            {
                return;
            }

            if (!hasDefaultColor)
            {
                defaultColor = image.color;
                hasDefaultColor = true;
            }

            image.color = completed ? Color.white : defaultColor;
        }

    }
}
