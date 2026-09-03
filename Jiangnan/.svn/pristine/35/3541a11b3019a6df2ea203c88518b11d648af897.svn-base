using JN.Client;
using JN.Client.Manager;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    /// <summary>
    /// Town 底部导航面板：进入酒楼入口，以及轿子容量展示。
    /// </summary>
    public class TownBottomNavPanelController : HudPanelController<TownHudPanelData>
    {
        [SerializeField] private RectTransform groupBottomBar;
        [SerializeField] private Button btnEnter;
        [SerializeField] private Button btnExitFocus;

        private GameObject jiaoziGroupRoot;
        private TMP_Text jiaoziCapacityText;

        /// <summary>
        /// 打开时缓存节点引用。
        /// </summary>
        protected override void OnPanelOpen(TownHudPanelData data)
        {
            EnsureNodes();
            Signals.Get<TavernRuntimeChangedSignal>().RemoveListener(HandleRuntimeChanged);
            Signals.Get<TavernRuntimeChangedSignal>().AddListener(HandleRuntimeChanged);
            Signals.Get<GameplayGuideProgressSignal>().RemoveListener(HandleGuideProgressChanged);
            Signals.Get<GameplayGuideProgressSignal>().AddListener(HandleGuideProgressChanged);
            Signals.Get<StartBuildingSignal>().RemoveListener(HandleStartBuilding);
            Signals.Get<StartBuildingSignal>().AddListener(HandleStartBuilding);
        }

        /// <summary>
        /// 显示时绑定按钮并刷新状态。
        /// </summary>
        protected override void OnPanelShow()
        {
            EnsureNodes();
            BindButtons();
            RefreshPanel();
        }

        protected override void OnPanelClose()
        {
            Signals.Get<TavernRuntimeChangedSignal>().RemoveListener(HandleRuntimeChanged);
            Signals.Get<GameplayGuideProgressSignal>().RemoveListener(HandleGuideProgressChanged);
            Signals.Get<StartBuildingSignal>().RemoveListener(HandleStartBuilding);
            base.OnPanelClose();
        }

        /// <summary>
        /// 刷新底部按钮与轿子容量。
        /// </summary>
        public void RefreshPanel()
        {
            EnsureNodes();
            RefreshEnterButtonState();
            RefreshExitFocusButtonState();
            RefreshJiaoziCapacityEntry();
        }

        private void HandleRuntimeChanged()
        {
            RefreshPanel();
        }

        private void HandleGuideProgressChanged()
        {
            RefreshPanel();
        }

        private void HandleStartBuilding(int tileId)
        {
            _ = tileId;
            RefreshPanel();
        }

        /// <summary>
        /// group_jiaozi：购买轿子后显示，文案「容量：当前/最大」。
        /// </summary>
        private void RefreshJiaoziCapacityEntry()
        {
            EnsureJiaoziNodes();
            if (jiaoziGroupRoot == null)
            {
                return;
            }

            var dataManager = DataManager.Instance;
            var show = dataManager != null && dataManager.IsJiaoziUnlocked();
            if (jiaoziGroupRoot.activeSelf != show)
            {
                jiaoziGroupRoot.SetActive(show);
            }

            if (!show || jiaoziCapacityText == null)
            {
                return;
            }

            var used = dataManager.GetJiaoziUsedCapacity();
            var max = dataManager.GetJiaoziCapacity();
            jiaoziCapacityText.text = $"容量：{used}/{max}";
        }

        private void EnsureJiaoziNodes()
        {
            if (jiaoziGroupRoot != null && jiaoziCapacityText != null)
            {
                return;
            }

            jiaoziGroupRoot ??= HudBindingUtility.FindChildRecursive(transform, "group_jiaozi")?.gameObject;
            if (jiaoziGroupRoot == null)
            {
                return;
            }

            var content = HudBindingUtility.FindChildRecursive(jiaoziGroupRoot.transform, "txt_content");
            jiaoziCapacityText ??= content != null
                ? content.GetComponent<TMP_Text>() ?? content.GetComponentInChildren<TMP_Text>(true)
                : jiaoziGroupRoot.GetComponentInChildren<TMP_Text>(true);
        }

        /// <summary>
        /// 查找并缓存底部栏节点。
        /// </summary>
        private void EnsureNodes()
        {
            groupBottomBar ??= HudBindingUtility.FindChildRecursive(transform, "group_BottomBar") as RectTransform;
            if (btnEnter == null)
            {
                var enterNode = groupBottomBar != null
                    ? groupBottomBar.Find("btn_Enter")
                    : null;
                enterNode ??= HudBindingUtility.FindChildRecursive(transform, "btn_Enter");
                btnEnter = enterNode != null ? enterNode.GetComponent<Button>() : null;
            }

            if (btnExitFocus == null && groupBottomBar != null)
            {
                btnExitFocus = groupBottomBar.Find("btn_ExitFocus")?.GetComponent<Button>();
            }

            EnsureJiaoziNodes();
        }

        /// <summary>
        /// 绑定底部按钮点击事件。
        /// </summary>
        private void BindButtons()
        {
            if (btnEnter != null)
            {
                btnEnter.onClick.RemoveAllListeners();
                btnEnter.onClick.AddListener(() =>
                {
                    GameAudioManager.PlayButtonClick();
                    if (Data?.RootController != null)
                    {
                        Data.RootController.HandleEnterTavernRequest();
                        return;
                    }

                    var owned = DataManager.Instance != null
                        ? DataManager.Instance.GetCompletedOwnedSelfTownBuilding()
                        : null;
                    if (owned == null)
                    {
                        return;
                    }

                    var tileId = owned.tileId;
                    var buildingLevel = owned.buildingLevel;
                    var tavernLevel = DataManager.Instance.GetTavernLevel();
                    if (tavernLevel < 2)
                    {
                        StartCoroutine(SceneFlowCoordinator.EnterTavern(tileId, buildingLevel));
                        return;
                    }

                    var used = DataManager.Instance.GetJiaoziUsedCapacity();
                    var max = DataManager.Instance.GetJiaoziCapacity();
                    HudOverlayService.ShowConfirmBox(
                        "确定返回酒楼吗？",
                        $"拉客 {used}/{max}",
                        () => StartCoroutine(SceneFlowCoordinator.EnterTavern(tileId, buildingLevel)));
                });
            }
        }

        /// <summary>
        /// 根据建筑完成情况控制“进入酒楼”按钮显隐。
        /// </summary>
        private void RefreshEnterButtonState()
        {
            EnsureNodes();
            if (btnEnter == null)
            {
                return;
            }

            var show = DataManager.Instance != null && DataManager.Instance.HasCompletedOwnedSelfTownBuilding();
            if (btnEnter.gameObject.activeSelf != show)
            {
                btnEnter.gameObject.SetActive(show);
            }

            // 兜底：父节点被藏时一并把 BottomBar 打开（仅在有已完工酒楼时）。
            if (show && groupBottomBar != null && !groupBottomBar.gameObject.activeSelf)
            {
                groupBottomBar.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 取消聚焦按钮永久隐藏。
        /// </summary>
        private void RefreshExitFocusButtonState()
        {
            if (btnExitFocus == null)
            {
                return;
            }

            if (btnExitFocus.gameObject.activeSelf)
            {
                btnExitFocus.gameObject.SetActive(false);
            }
        }
    }
}
