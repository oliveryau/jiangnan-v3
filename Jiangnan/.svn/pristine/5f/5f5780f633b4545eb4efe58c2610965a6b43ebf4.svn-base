using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    /// <summary>
    /// 招募确认面板的数据载体。
    /// </summary>
    public class RecruitConfirmPanelControllerData : QFramework.UIPanelData
    {
        public string DisplayName;
        public string RoleText;
        public Sprite Portrait;
        public int Cost;
        public Action OnConfirm;
    }

    /// <summary>
    /// 显示单个员工招募确认信息。
    /// </summary>
    public class RecruitConfirmPanelController : OverlayPanelController<RecruitConfirmPanelControllerData>
    {
        private TMP_Text titleText;
        private TMP_Text nameText;
        private TMP_Text costText;
        private Image portraitImage;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;

        /// <summary>
        /// 初始化时绑定确认和关闭按钮。
        /// </summary>
        protected override void OnPanelInit()
        {
            EnsureNodes();
            BindButton(closeButton, CloseSelf);
            BindButton(confirmButton, ConfirmRecruit);
        }

        /// <summary>
        /// 面板打开时刷新招募信息。
        /// </summary>
        protected override void OnPanelOpen(RecruitConfirmPanelControllerData data)
        {
            EnsureNodes();
            RefreshPanel();
        }

        /// <summary>
        /// 面板显示时同步角色名称、花费和头像。
        /// </summary>
        protected override void OnPanelShow()
        {
            RefreshPanel();
        }

        /// <summary>
        /// 解析招募确认面板所需节点。
        /// </summary>
        private void EnsureNodes()
        {
            titleText ??= ResolveText("group_Panel/img_TitleBg/txt_Title", "txt_Title");
            nameText ??= ResolveText("group_Panel/img_NameBg/txt_Name", "txt_Name");
            costText ??= ResolveText("group_Panel/btn_Confirm/txt_CostCoinNum", "txt_CostCoinNum");
            portraitImage ??= ResolveImage("img_Portrait", "img_Portrait");
            confirmButton ??= ResolveButton("btn_Confirm", "btn_Confirm");
            closeButton ??= ResolveButton("btn_Close", "btn_Close");
        }

        /// <summary>
        /// 刷新招募确认面板的展示内容。
        /// </summary>
        private void RefreshPanel()
        {
            if (titleText != null)
            {
                titleText.text = $"招聘{Data.RoleText}";
            }

            if (nameText != null)
            {
                nameText.text = HudOverlayAssetCatalog.ToVerticalText(Data.DisplayName);
            }

            if (costText != null)
            {
                costText.text = Data.Cost.ToString();
            }

            if (portraitImage != null)
            {
                var portrait = HudOverlayAssetCatalog.ResolveSingleRecruitPortrait(Data.RoleText, Data.Portrait);
                if (portrait != null)
                {
                    portraitImage.sprite = portrait;
                    portraitImage.preserveAspect = true;
                }
            }
        }

        /// <summary>
        /// 关闭确认面板并回调外部招募逻辑。
        /// </summary>
        private void ConfirmRecruit()
        {
            var onConfirm = Data.OnConfirm;
            CloseSelf();
            onConfirm?.Invoke();
        }
    }
}
