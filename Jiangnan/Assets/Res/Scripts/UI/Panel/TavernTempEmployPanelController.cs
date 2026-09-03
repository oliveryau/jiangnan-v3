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
    /// 管理营业中的临时小二招募入口。
    /// 当前 HUD 已停用本面板（不再 Open），保留脚本与预制体便于日后恢复。
    /// </summary>
    public class TavernTempEmployPanelController : HudPanelController<TavernHudPanelData>
    {
        private const int TemporaryWaiterStaffId = 5;

        private RectTransform tempEmployRoot;
        private Button tempEmployButton;
        private TMP_Text tempEmployCostText;
        private TavernGuideService GuideService => TavernGuideService.Instance;

        /// <summary>
        /// 面板首次打开时缓存临时招募节点。
        /// </summary>
        protected override void OnPanelOpen(TavernHudPanelData data)
        {
            EnsureNodes();
        }

        /// <summary>
        /// 面板显示时恢复节点并刷新招募状态。
        /// </summary>
        protected override void OnPanelShow()
        {
            SetManagedNodesVisible(true);
            RefreshPanel();
        }

        /// <summary>
        /// 面板关闭时解绑事件并隐藏节点。
        /// </summary>
        protected override void OnPanelClose()
        {
            if (tempEmployButton != null)
            {
                tempEmployButton.onClick.RemoveListener(OnClickTempEmployButton);
            }

            SetManagedNodesVisible(false);
        }

        /// <summary>
        /// 按营业和打烊流程状态刷新临时招募入口。
        /// </summary>
        public void RefreshPanel()
        {
            EnsureNodes();
            var isOpen = DataManager.Instance != null
                         && DataManager.Instance.TavernData != null
                         && DataManager.Instance.TavernData.isOpen;
            var closingFlowPanel = UIKit.GetPanel<TavernBusinessFlowPanelController>();
            var active = isOpen
                         && (closingFlowPanel == null || !closingFlowPanel.IsWaitingSettlementConfirm)
                         && (TavernSceneManager.Instance == null || !TavernSceneManager.Instance.IsClosingBusiness);

            if (tempEmployRoot != null)
            {
                tempEmployRoot.gameObject.SetActive(active);
            }

            if (tempEmployButton != null)
            {
                tempEmployButton.interactable = active;
            }

            if (tempEmployCostText == null || DataManager.Instance == null)
            {
                return;
            }

            var cost = GuideService.GetStaffCost(TemporaryWaiterStaffId, StaffRole.Waiter);
            var current = DataManager.Instance.GetHiredGuideWaiterCount();
            //tempEmployCostText.text = $"{cost} ({current}/{DataManager.MaxGuideWaiterCount})";
            tempEmployCostText.text = $"{cost}";
        }

        /// <summary>
        /// 延迟绑定临时招募按钮和价格文本。
        /// </summary>
        private void EnsureNodes()
        {
            var hudRoot = transform;

            tempEmployRoot ??= HudBindingUtility.FindChildRecursive(hudRoot, "TempEmploy") as RectTransform
                              ?? HudBindingUtility.FindChildRecursive(hudRoot, "tempEmploy") as RectTransform;
            tempEmployButton ??= HudBindingUtility.FindChildRecursive(hudRoot, "btn_TempEmploy")?.GetComponent<Button>()
                               ?? tempEmployRoot?.GetComponentInChildren<Button>(true);
            tempEmployCostText ??= tempEmployRoot != null ? tempEmployRoot.GetComponentInChildren<TMP_Text>(true) : null;
            if (tempEmployButton != null)
            {
                tempEmployButton.onClick.RemoveListener(OnClickTempEmployButton);
                tempEmployButton.onClick.AddListener(OnClickTempEmployButton);
            }
        }

        /// <summary>
        /// 尝试招募临时小二，并驱动场景中的入场表现。
        /// </summary>
        private void OnClickTempEmployButton()
        {
            GameAudioManager.PlayButtonClick();
            if (DataManager.Instance == null
                || DataManager.Instance.TavernData == null
                || !DataManager.Instance.TavernData.isOpen)
            {
                RefreshPanel();
                return;
            }

            if (!GuideService.TryHireTemporaryWaiter(out var message))
            {
                HudOverlayService.ShowFloatingWarning(message);
                RefreshPanel();
                return;
            }

            GameAudioManager.PlayRecruitWaiter();
            TavernSceneManager.Instance?.PlayTemporaryWaiterEnterFromRecruit();
            Data?.RootController?.RefreshAllPanels();
        }

        /// <summary>
        /// 统一控制该子面板根节点显隐。
        /// </summary>
        private void SetManagedNodesVisible(bool visible)
        {
            if (tempEmployRoot != null)
            {
                tempEmployRoot.gameObject.SetActive(visible);
            }
        }
    }
}
