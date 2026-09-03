using JN.Client.Manager;
using JN.Client.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    /// <summary>
    /// 招募列表面板的数据载体。
    /// </summary>
    public class RecruitListPanelControllerData : QFramework.UIPanelData
    {
        public RecruitPanelRole DefaultRole = RecruitPanelRole.Chef;
    }

    /// <summary>
    /// 管理厨师和小二的列表式招募弹层。
    /// </summary>
    public class RecruitListPanelController : OverlayPanelController<RecruitListPanelControllerData>
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button chefButton;
        [SerializeField] private Button waiterButton;
        [SerializeField] private TMP_Text chefButtonLabel;
        [SerializeField] private TMP_Text waiterButtonLabel;
        [SerializeField] private Image chefButtonImage;
        [SerializeField] private Image waiterButtonImage;
        [SerializeField] private RecruitPanelRole currentRole;

        /// <summary>
        /// 初始化时绑定关闭和页签切换按钮。
        /// </summary>
        protected override void OnPanelInit()
        {
            EnsureNodes();
            BindButton(closeButton, CloseSelf);
            BindButton(chefButton, () => SwitchRole(RecruitPanelRole.Chef));
            BindButton(waiterButton, () => SwitchRole(RecruitPanelRole.Waiter));
        }

        /// <summary>
        /// 面板打开时记录默认页签并刷新列表。
        /// </summary>
        protected override void OnPanelOpen(RecruitListPanelControllerData data)
        {
            EnsureNodes();
            currentRole = data.DefaultRole;
            RefreshPanel();
        }

        /// <summary>
        /// 面板显示时刷新当前招募列表。
        /// </summary>
        protected override void OnPanelShow()
        {
            RefreshPanel();
        }

        /// <summary>
        /// 解析招募列表面板的页签和文本节点。
        /// </summary>
        private void EnsureNodes()
        {
            titleText ??= ResolveText("Panel/txt_Title", "txt_Title");
            closeButton ??= ResolveButton("Panel/btn_Close", "btn_Close");
            chefButton ??= ResolveButton("Panel/group_Tabs/btn_Chef", "btn_Chef");
            waiterButton ??= ResolveButton("Panel/group_Tabs/btn_Waiter", "btn_Waiter");
            chefButtonLabel ??= chefButton != null ? chefButton.GetComponentInChildren<TMP_Text>(true) : null;
            waiterButtonLabel ??= waiterButton != null ? waiterButton.GetComponentInChildren<TMP_Text>(true) : null;
            chefButtonImage ??= chefButton != null ? chefButton.GetComponent<Image>() : null;
            waiterButtonImage ??= waiterButton != null ? waiterButton.GetComponent<Image>() : null;
        }

        /// <summary>
        /// 切换当前招募角色页签。
        /// </summary>
        private void SwitchRole(RecruitPanelRole role)
        {
            currentRole = role;
            RefreshPanel();
        }

        /// <summary>
        /// 刷新标题、页签和招募列表。
        /// </summary>
        private void RefreshPanel()
        {
            if (titleText != null)
            {
                titleText.text = "招聘员工";
            }

            SetNodeVisible("Panel/group_Tabs", true);
            SetNodeVisible("Panel/group_List", true);
            RefreshRecruitTabs();
            RefreshRecruitRows();
        }

        /// <summary>
        /// 刷新厨师和小二页签的计数与选中态。
        /// </summary>
        private void RefreshRecruitTabs()
        {
            var dataManager = DataManager.Instance;
            var chefLabel = dataManager == null
                ? "厨师"
                : $"厨师 {dataManager.GetHiredGuideChefCount()}/{DataManager.MaxGuideChefCount}";
            var waiterLabel = dataManager == null
                ? "小二"
                : $"小二 {dataManager.GetHiredGuideWaiterCount()}/{DataManager.MaxGuideWaiterCount}";

            ApplyTabState(chefButtonImage, chefButtonLabel, chefLabel, currentRole == RecruitPanelRole.Chef);
            ApplyTabState(waiterButtonImage, waiterButtonLabel, waiterLabel, currentRole == RecruitPanelRole.Waiter);
        }

        /// <summary>
        /// 应用页签底图和标题文本状态。
        /// </summary>
        private void ApplyTabState(Image background, TMP_Text label, string content, bool selected)
        {
            if (background != null)
            {
                background.sprite = HudOverlayAssetCatalog.LoadSprite(
                    selected ? HudOverlayAssetCatalog.RecruitTabSelectedSpritePath : HudOverlayAssetCatalog.RecruitTabNormalSpritePath);
            }

            if (label != null)
            {
                label.text = content;
            }
        }

        /// <summary>
        /// 按当前页签刷新所有招募行。
        /// </summary>
        private void RefreshRecruitRows()
        {
            var dataManager = DataManager.Instance;
            if (dataManager == null)
            {
                return;
            }

            var isChef = currentRole == RecruitPanelRole.Chef;
            var staffId = HudOverlayAssetCatalog.GetRecruitStaffId(currentRole);
            var staffRole = HudOverlayAssetCatalog.GetRecruitStaffRole(currentRole);
            var roleName = HudOverlayAssetCatalog.GetRecruitRoleName(currentRole);
            var maxCount = isChef ? DataManager.MaxGuideChefCount : DataManager.MaxGuideWaiterCount;
            var hiredCount = isChef ? dataManager.GetHiredGuideChefCount() : dataManager.GetHiredGuideWaiterCount();
            var staff = dataManager.GetGuideStaffConfig(staffId, staffRole);
            var fallbackPortrait = staff != null ? staff.icon : null;
            var cost = dataManager.GetGuideStaffHireCost(staffId, staffRole);

            for (var index = 0; index < maxCount; index++)
            {
                RefreshRecruitRow(index, hiredCount, roleName, fallbackPortrait, cost);
            }
        }

        /// <summary>
        /// 刷新单条招募行的头像、价格和按钮状态。
        /// </summary>
        private void RefreshRecruitRow(int index, int hiredCount, string roleName, Sprite fallbackPortrait, int cost)
        {
            var rowPath = $"Panel/group_List/item_{index + 1}";
            var isHired = index < hiredCount;
            SetText($"{rowPath}/txt_Name", $"{roleName}{index + 1}");
            SetText($"{rowPath}/txt_Status", isHired ? "已招募" : "未招募");
            SetText($"{rowPath}/txt_Cost", isHired ? "已入职" : $"招聘价格：{cost}");
            SetText($"{rowPath}/btn_Recruit/txt_Label", isHired ? "已招募" : $"{cost}");
            SetNodeVisible($"{rowPath}/btn_Recruit", !isHired);

            var statusText = ResolveText($"{rowPath}/txt_Status");
            if (statusText != null)
            {
                statusText.color = isHired ? new Color(0.21f, 0.67f, 0.25f, 1f) : new Color(0.83f, 0.24f, 0.20f, 1f);
            }

            var bgImage = ResolveImage($"{rowPath}/img_Bg");
            if (bgImage != null)
            {
                bgImage.color = isHired ? new Color(0.92f, 0.84f, 0.68f, 0.95f) : new Color(1f, 0.94f, 0.78f, 0.95f);
            }

            var portraitImage = ResolveImage($"{rowPath}/img_Portrait");
            if (portraitImage != null)
            {
                var portrait = HudOverlayAssetCatalog.ResolveRecruitListPortrait(currentRole, index, fallbackPortrait);
                if (portrait != null)
                {
                    portraitImage.sprite = portrait;
                }
            }

            var recruitButton = ResolveButton($"{rowPath}/btn_Recruit");
            BindButton(recruitButton, isHired ? null : TryRecruitCurrentRole);
        }

        /// <summary>
        /// 打开当前页签对应的招募确认弹层。
        /// </summary>
        private void TryRecruitCurrentRole()
        {
            var dataManager = DataManager.Instance;
            if (dataManager == null)
            {
                return;
            }

            var isChef = currentRole == RecruitPanelRole.Chef;
            var currentCount = isChef ? dataManager.GetHiredGuideChefCount() : dataManager.GetHiredGuideWaiterCount();
            var displayName = $"{HudOverlayAssetCatalog.GetRecruitRoleName(currentRole)}{currentCount + 1}";
            var portrait = HudOverlayAssetCatalog.ResolveRecruitListPortrait(currentRole, currentCount, null);
            var staffRole = HudOverlayAssetCatalog.GetRecruitStaffRole(currentRole);
            var staffId = HudOverlayAssetCatalog.GetRecruitStaffId(currentRole);
            var cost = dataManager.GetGuideStaffHireCost(staffId, staffRole);

            HudOverlayService.ShowRecruitPanel(displayName, HudOverlayAssetCatalog.GetRecruitRoleName(currentRole), portrait, cost, () =>
            {
                string message;
                var success = isChef
                    ? dataManager.TryHireGuideChef(out message)
                    : dataManager.TryHireGuideWaiter(out message);

                if (!success)
                {
                    if (HudOverlayAssetCatalog.IsCoinShortageMessage(message))
                    {
                        HudOverlayService.ShowFloatingWarning(message);
                    }
                    else
                    {
                        HudOverlayService.ShowInfoPanel("招聘失败", message);
                    }

                    return;
                }

                if (isChef)
                {
                    GameAudioManager.PlayRecruitChef();
                }
                else
                {
                    GameAudioManager.PlayRecruitWaiter();
                }

                if (TavernSceneManager.Instance != null)
                {
                    if (isChef)
                    {
                        TavernSceneManager.Instance.PlayGuideChefEnterFromBottomRecruit(staffId);
                    }
                    else
                    {
                        TavernSceneManager.Instance.PlayGuideWaiterEnterFromBottomRecruit(staffId);
                    }
                }

                RefreshPanel();
            });
        }
    }
}
