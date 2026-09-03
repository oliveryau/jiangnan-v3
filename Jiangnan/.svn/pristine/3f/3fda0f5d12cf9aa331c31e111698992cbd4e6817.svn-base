using JN.Client.Manager;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    /// <summary>
    /// 酒楼声望升级弹窗数据。
    /// </summary>
    public class UpgradeTavernPanelControllerData : UIPanelData
    {
    }

    /// <summary>
    /// 酒楼声望升级弹窗：展示当前/下一星级、声望进度，并在条件满足时提供升级。
    /// </summary>
    public class UpgradeTavernPanelController : OverlayPanelController<UpgradeTavernPanelControllerData>
    {
        private const string TavernLevelSpritePathFormat =
            "Assets/Res/Resources/Textures/UI/Panel/UpgradeTavern/lv{0}.png";
        private const string NewFuncIconPathFormat =
            "Assets/Res/Resources/Textures/UI/FuncUI/lv{0}-{1}.png";
        private const int NewFuncItemCapacity = 3;
        /// <summary>不能升级时按钮底图与 newText1 着色（#646464）。</summary>
        private static readonly Color UpgradeButtonDisabledTint = new(0x64 / 255f, 0x64 / 255f, 0x64 / 255f, 1f);
        /// <summary>可升级时 newText1 字色（#71400C）。</summary>
        private static readonly Color UpgradeButtonLabelEnabledColor = new(0x71 / 255f, 0x40 / 255f, 0x0C / 255f, 1f);

        /// <summary>升到目标星级时解锁的功能文案（下标 0 起对应 item1）。</summary>
        private static readonly string[][] NewFuncsByTargetLevel =
        {
            null,
            // → LV1
            new[] { "开店营业", "新桌椅" },
            // → LV2
            new[] { "拉客", "新员工", "店铺扩大" },
            // → LV3
            new[] { "二楼", "贵客" },
        };

        private Button closeButton;
        private Button upgradeButton;
        private TMP_Text upgradeButtonLabel;
        private Image levelNowImage;
        private Image levelNextImage;
        private Image progressBar;
        private TMP_Text progressText;
        private TMP_Text expectSoonText;
        private Transform newFuncGroup;
        private readonly Transform[] newFuncItems = new Transform[NewFuncItemCapacity];
        private readonly Image[] newFuncImages = new Image[NewFuncItemCapacity];
        private readonly TMP_Text[] newFuncTexts = new TMP_Text[NewFuncItemCapacity];
        private bool buttonsBound;
        private bool newFuncNodesBound;

        /// <summary>
        /// 绑定关闭与升级按钮。
        /// </summary>
        protected override void OnPanelInit()
        {
            EnsureNodes();
            BindPanelButtons();
        }

        /// <summary>
        /// 打开时监听声望变化并刷新。
        /// </summary>
        protected override void OnPanelOpen(UpgradeTavernPanelControllerData data)
        {
            Signals.Get<TavernPrestigeChangedSignal>().RemoveListener(HandlePrestigeChanged);
            Signals.Get<TavernPrestigeChangedSignal>().AddListener(HandlePrestigeChanged);
            Signals.Get<TavernRuntimeChangedSignal>().RemoveListener(HandlePrestigeChanged);
            Signals.Get<TavernRuntimeChangedSignal>().AddListener(HandlePrestigeChanged);
            EnsureNodes();
            BindPanelButtons();
            RefreshPanel();
        }

        /// <summary>
        /// 显示时刷新内容。
        /// </summary>
        protected override void OnPanelShow()
        {
            EnsureNodes();
            BindPanelButtons();
            RefreshPanel();
        }

        /// <summary>
        /// 关闭时移除监听。
        /// </summary>
        protected override void OnPanelClose()
        {
            Signals.Get<TavernPrestigeChangedSignal>().RemoveListener(HandlePrestigeChanged);
            Signals.Get<TavernRuntimeChangedSignal>().RemoveListener(HandlePrestigeChanged);
        }

        private void HandlePrestigeChanged()
        {
            RefreshPanel();
        }

        /// <summary>
        /// 解析弹窗节点；按钮节点若只有 Image，运行时补齐 Button。
        /// </summary>
        private void EnsureNodes()
        {
            closeButton = EnsureUiButton(closeButton, "btn_Close");
            upgradeButton = EnsureUiButton(upgradeButton, "btn_Upgrade");
            upgradeButtonLabel ??= ResolveText("newText1", "newText1");
            levelNowImage ??= ResolveImage("img_LvNow", "img_LvNow");
            levelNextImage ??= ResolveImage("img_LvNext", "img_LvNext");
            progressBar ??= ResolveImage("img_bar", "img_bar");
            progressText ??= ResolveText("text_progress", "text_progress");
            expectSoonText ??= ResolveText("text_qidai", "text_qidai");
            EnsureNewFuncNodes();
        }

        private void EnsureNewFuncNodes()
        {
            if (newFuncNodesBound)
            {
                return;
            }

            newFuncGroup ??= ResolveTransform("group_NewFunc", "group_NewFunc");
            if (newFuncGroup == null)
            {
                return;
            }

            for (var index = 0; index < NewFuncItemCapacity; index++)
            {
                var item = newFuncGroup.Find($"NewFuncItem{index + 1}");
                newFuncItems[index] = item;
                if (item == null)
                {
                    continue;
                }

                newFuncImages[index] = item.Find("img_func")?.GetComponent<Image>();
                newFuncTexts[index] = item.Find("txt_func")?.GetComponent<TMP_Text>();
            }

            newFuncNodesBound = true;
        }

        private void BindPanelButtons()
        {
            if (buttonsBound)
            {
                return;
            }

            BindButton(closeButton, CloseSelf);
            BindButton(upgradeButton, OnClickUpgrade);
            buttonsBound = closeButton != null || upgradeButton != null;
        }

        /// <summary>
        /// 按节点名取 Button；缺失时自动添加，避免只有 ButtonPressScale 时无法点击。
        /// </summary>
        private Button EnsureUiButton(Button existing, string nodeName)
        {
            if (existing != null)
            {
                return existing;
            }

            var node = ResolveTransform(nodeName, nodeName);
            if (node == null)
            {
                return null;
            }

            var button = node.GetComponent<Button>();
            if (button == null)
            {
                button = node.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
            }

            var image = node.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = true;
                button.targetGraphic = image;
            }

            return button;
        }

        /// <summary>
        /// 刷新星级图、进度条、进度文案、下一级新功能；升级按钮常驻，不能升时置灰。
        /// </summary>
        private void RefreshPanel()
        {
            EnsureNodes();
            var dataManager = DataManager.Instance;
            if (dataManager == null)
            {
                return;
            }

            var level = dataManager.GetTavernLevel();
            var prestige = dataManager.GetTavernPrestige();
            var required = Mathf.Max(1, dataManager.GetNextTavernPrestigeRequirement());
            var isMaxLevel = level >= DataManager.MaxTavernLevel;
            var canUpgrade = !dataManager.IsVisitingOtherTavern
                             && dataManager.CanUpgradeTavernPrestigeLevel();

            ApplyLevelSprite(levelNowImage, ResolveSpriteLevel(level));
            ApplyLevelSprite(levelNextImage, isMaxLevel ? ResolveSpriteLevel(level) : ResolveSpriteLevel(level + 1));

            if (progressBar != null)
            {
                progressBar.type = Image.Type.Filled;
                progressBar.fillMethod = Image.FillMethod.Horizontal;
                progressBar.fillOrigin = (int)Image.OriginHorizontal.Left;
                progressBar.fillAmount = isMaxLevel ? 1f : Mathf.Clamp01((float)prestige / required);
            }

            if (progressText != null)
            {
                if (isMaxLevel)
                {
                    progressText.SetText("声望{0}/{1}", prestige, prestige);
                }
                else
                {
                    progressText.SetText("声望{0}/{1}", prestige, required);
                }
            }

            if (upgradeButton != null)
            {
                upgradeButton.gameObject.SetActive(true);
                ApplyUpgradeButtonVisual(canUpgrade);
            }

            // 三星→四星：无新功能列表，改显示「敬请期待」。
            var showExpectSoon = !isMaxLevel && level == 3;
            if (expectSoonText != null
                && expectSoonText.gameObject.activeSelf != showExpectSoon)
            {
                expectSoonText.gameObject.SetActive(showExpectSoon);
            }

            RefreshNewFuncs(showExpectSoon || isMaxLevel ? 0 : level + 1);
        }

        /// <summary>
        /// 展示升到 targetLevel 将解锁的功能；满级或无配置时隐藏整组。
        /// </summary>
        private void RefreshNewFuncs(int targetLevel)
        {
            EnsureNewFuncNodes();
            if (newFuncGroup == null)
            {
                return;
            }

            var entries = targetLevel > 0
                          && targetLevel < NewFuncsByTargetLevel.Length
                ? NewFuncsByTargetLevel[targetLevel]
                : null;
            var hasEntries = entries != null && entries.Length > 0;
            if (newFuncGroup.gameObject.activeSelf != hasEntries)
            {
                newFuncGroup.gameObject.SetActive(hasEntries);
            }

            if (!hasEntries)
            {
                return;
            }

            for (var index = 0; index < NewFuncItemCapacity; index++)
            {
                var item = newFuncItems[index];
                if (item == null)
                {
                    continue;
                }

                var show = index < entries.Length;
                if (item.gameObject.activeSelf != show)
                {
                    item.gameObject.SetActive(show);
                }

                if (!show)
                {
                    continue;
                }

                if (newFuncTexts[index] != null)
                {
                    newFuncTexts[index].text = entries[index];
                }

                if (newFuncImages[index] == null)
                {
                    continue;
                }

                var sprite = GameplayResourceStore.LoadAsset<Sprite>(
                    string.Format(NewFuncIconPathFormat, targetLevel, index + 1));
                if (sprite != null)
                {
                    newFuncImages[index].sprite = sprite;
                    newFuncImages[index].enabled = true;
                }
            }
        }

        /// <summary>
        /// 可升级保持原色；不可升级置灰但仍可点，用于弹出「声望不足」。
        /// </summary>
        private void ApplyUpgradeButtonVisual(bool canUpgrade)
        {
            if (upgradeButton == null)
            {
                return;
            }

            upgradeButton.interactable = true;
            var tint = canUpgrade ? Color.white : UpgradeButtonDisabledTint;
            var graphics = upgradeButton.GetComponentsInChildren<Graphic>(true);
            for (var index = 0; index < graphics.Length; index++)
            {
                var graphic = graphics[index];
                if (graphic == null || graphic is TMP_Text)
                {
                    continue;
                }

                graphic.color = tint;
            }

            if (upgradeButtonLabel != null)
            {
                upgradeButtonLabel.color = canUpgrade
                    ? UpgradeButtonLabelEnabledColor
                    : UpgradeButtonDisabledTint;
            }
        }

        /// <summary>
        /// 可升级则走升星；不可升级飘字「声望不足」。
        /// </summary>
        private void OnClickUpgrade()
        {
            if (DataManager.Instance == null)
            {
                return;
            }

            if (DataManager.Instance.IsVisitingOtherTavern
                || !DataManager.Instance.CanUpgradeTavernPrestigeLevel())
            {
                HudOverlayService.ShowFloatingWarning("声望不足");
                return;
            }

            if (!DataManager.Instance.TryUpgradeTavernPrestigeLevel(out var message))
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    HudOverlayService.ShowFloatingWarning(message);
                }

                RefreshPanel();
                return;
            }

            // 升级成功：关详情 → 升星视频（暂停营业）→ 恭喜弹窗 → 原有高峰提示。
            var newLevel = DataManager.Instance.GetTavernLevel();
            CloseSelf();
            HudOverlayService.PlayLevelUpgradeCinematicThenCongrats(newLevel);
        }

        /// <summary>
        /// 资源为 lv0~lv4；按存档星级直接取图。
        /// </summary>
        private static int ResolveSpriteLevel(int tavernLevel)
        {
            return Mathf.Clamp(tavernLevel, 0, 4);
        }

        private static void ApplyLevelSprite(Image image, int spriteLevel)
        {
            if (image == null)
            {
                return;
            }

            var sprite = GameplayResourceStore.LoadAsset<Sprite>(
                string.Format(TavernLevelSpritePathFormat, spriteLevel));
            if (sprite != null)
            {
                image.sprite = sprite;
                image.enabled = true;
            }
        }
    }
}
