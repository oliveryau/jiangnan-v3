using System;
using JN.Client.Manager;
using JN.Client.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    /// <summary>
    /// 桌子升级面板的数据载体。
    /// </summary>
    public class TableUpgradePanelControllerData : QFramework.UIPanelData
    {
        public TableArea Table;
        public Action OnConfirm;
    }

    /// <summary>
    /// 管理桌子升级确认弹层及费用展示。
    /// </summary>
    public class TableUpgradePanelController : OverlayPanelController<TableUpgradePanelControllerData>
    {
        private Button closeButton;
        private Button confirmButton;
        private TMP_Text titleText;
        private TMP_Text confirmLabelText;
        private TMP_Text confirmCostText;
        private GameObject confirmCoinNode;
        private int currentUpgradeCost;
        private bool isMaxLevel;

        /// <summary>
        /// 初始化时绑定关闭和升级按钮。
        /// </summary>
        protected override void OnPanelInit()
        {
            EnsureNodes();
            BindButton(closeButton, CloseSelf);
            BindButton(confirmButton, ConfirmUpgrade);
        }

        /// <summary>
        /// 面板打开时同步当前桌子的升级信息。
        /// </summary>
        protected override void OnPanelOpen(TableUpgradePanelControllerData data)
        {
            EnsureNodes();
            RefreshPanel();
        }

        /// <summary>
        /// 面板显示时刷新升级内容。
        /// </summary>
        protected override void OnPanelShow()
        {
            RefreshPanel();
        }

        /// <summary>
        /// 解析桌子升级弹层所需节点。
        /// </summary>
        private void EnsureNodes()
        {
            closeButton ??= ResolveButton("Panel/btn_Close", "btn_Close");
            confirmButton ??= ResolveButton("Panel/btn_Confirm", "btn_Confirm");
            titleText ??= ResolveText("Panel/img_Title/txt_Title", "txt_Title");
            confirmLabelText ??= ResolveText("Panel/btn_Confirm/txt_Label", "txt_Label");
            confirmCostText ??= ResolveText("Panel/btn_Confirm/txt_CostCoinNum", "txt_CostCoinNum");
            confirmCoinNode ??= ResolveNode("Panel/btn_Confirm/img_Coin", "img_Coin");
        }

        /// <summary>
        /// 刷新当前等级、目标等级和升级花费。
        /// </summary>
        private void RefreshPanel()
        {
            if (Data.Table == null || DataManager.Instance == null)
            {
                return;
            }

            if (titleText != null)
            {
                titleText.text = "桌子升级";
            }

            var tableData = DataManager.Instance.GetTableData(Data.Table.tableId);
            var currentLevel = tableData != null ? Mathf.Clamp(tableData.level, 1, HudOverlayAssetCatalog.MaxTableLevel) : 1;
            isMaxLevel = currentLevel >= HudOverlayAssetCatalog.MaxTableLevel;
            var nextLevel = isMaxLevel ? currentLevel : currentLevel + 1;
            currentUpgradeCost = isMaxLevel ? 0 : HudOverlayAssetCatalog.GetTableUpgradeCost(nextLevel);

            BindUpgradeTableInfo("Panel/group_CurTableInfo", currentLevel, true);
            BindUpgradeTableInfo("Panel/group_NextTableInfo", nextLevel, !isMaxLevel, isMaxLevel ? "已满级" : null);

            if (confirmLabelText != null)
            {
                confirmLabelText.text = isMaxLevel ? "已满级" : "升级";
            }

            if (confirmCostText != null)
            {
                confirmCostText.text = isMaxLevel ? string.Empty : currentUpgradeCost.ToString();
                confirmCostText.gameObject.SetActive(!isMaxLevel);
            }

            if (confirmCoinNode != null)
            {
                confirmCoinNode.SetActive(!isMaxLevel);
            }

            if (confirmButton != null)
            {
                confirmButton.interactable = !isMaxLevel;
            }
        }

        /// <summary>
        /// 绑定一侧桌子信息区域的图标和名称。
        /// </summary>
        private void BindUpgradeTableInfo(string groupPath, int level, bool includeLevelTag, string overrideName = null)
        {
            var iconImage = ResolveImage($"{groupPath}/img_TableIcon");
            if (iconImage != null)
            {
                var iconSprite = HudOverlayAssetCatalog.LoadSprite(HudOverlayAssetCatalog.GetTableIconPath(level));
                if (iconSprite != null)
                {
                    iconImage.sprite = iconSprite;
                    iconImage.preserveAspect = true;
                }
            }

            var displayName = !string.IsNullOrEmpty(overrideName)
                ? overrideName
                : includeLevelTag
                    ? $"{HudOverlayAssetCatalog.GetTableLevelDisplayName(level)} Lv.{level}"
                    : HudOverlayAssetCatalog.GetTableLevelDisplayName(level);

            SetText($"{groupPath}/txt_TableName", displayName);
        }

        /// <summary>
        /// 确认升级，余额不足时给出提示。
        /// </summary>
        private void ConfirmUpgrade()
        {
            if (isMaxLevel || DataManager.Instance == null)
            {
                return;
            }

            if (DataManager.Instance.PlayerData.coinNum < currentUpgradeCost)
            {
                HudOverlayService.ShowFloatingWarning("金币不足，无法升级桌子");
                return;
            }

            var onConfirm = Data.OnConfirm;
            CloseSelf();
            onConfirm?.Invoke();
        }
    }
}
