using System.Collections.Generic;
using cfg;
using JN.Client.Config;
using JN.Client.Manager;
using JN.Client.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    public enum StaffHireSelectRole
    {
        Chef = 0,
        Waiter = 1,
        Shopkeeper = 2
    }

    public class StaffHireSelectPanelControllerData : QFramework.UIPanelData
    {
        public StaffHireSelectRole DefaultRole = StaffHireSelectRole.Waiter;
        /// <summary>为 true 时展示厨师/小二/掌柜页签。</summary>
        public bool ShowRoleTabs;
    }

    /// <summary>
    /// 招聘面板：小二/厨师固定三槽，点卡内 btn_Recruit 直接招聘；掌柜开场后自动拥有，不在本界面招聘。
    /// </summary>
    public class StaffHireSelectPanelController : OverlayPanelController<StaffHireSelectPanelControllerData>
    {
        private const int LowCoinHintThreshold = 2000;
        private const string DefaultHintText = "点击员工下方招聘按钮进行招聘";
        private const string LowCoinHintReminder = "（资金较少，谨慎选择）";
        private const int FixedHireSlotCount = 3;

        private StaffHireSelectRole currentRole;
        private bool showRoleTabs;
        private List<Staff> cachedCandidates;
        private StaffHireSelectRole? cachedRole;
        private Button chefTabButton;
        private Button waiterTabButton;
        private Button shopkeeperTabButton;
        private Button refreshButton;
        private TMP_Text refreshButtonLabel;
        private TMP_Text chefTabLabel;
        private TMP_Text waiterTabLabel;
        private TMP_Text shopkeeperTabLabel;
        private Image chefTabImage;
        private Image waiterTabImage;
        private Image shopkeeperTabImage;
        private GameObject chefTabRedDot;
        private GameObject waiterTabRedDot;

        protected override void OnPanelInit()
        {
            EnsureNodes();
            BindStaticButtons();
        }

        protected override void OnPanelOpen(StaffHireSelectPanelControllerData data)
        {
            currentRole = data != null ? data.DefaultRole : StaffHireSelectRole.Waiter;
            showRoleTabs = data != null && data.ShowRoleTabs;
            InvalidateCandidateCache();
            EnsureRefreshButtonBinding();
            RefreshPanel();
        }

        protected override void OnPanelShow()
        {
            EnsureRefreshButtonBinding();
            RefreshPanel();
        }

        private void BindStaticButtons()
        {
            BindButton(ResolveButton("Panel/btn_Close", "btn_Close"), CloseSelf);
            BindButton(chefTabButton, () => SwitchRole(StaffHireSelectRole.Chef));
            BindButton(waiterTabButton, () => SwitchRole(StaffHireSelectRole.Waiter));
            BindButton(shopkeeperTabButton, () => SwitchRole(StaffHireSelectRole.Shopkeeper));
            // 底部确认招聘已废弃，改由卡内 btn_Recruit 直招。
            SetNodeVisible("Panel/btn_ConfirmHire", false, "btn_ConfirmHire");
        }

        private void EnsureRefreshButtonBinding()
        {
            refreshButton = ResolveButton("Panel/btn_Refresh", "btn_Refresh");
            refreshButtonLabel = refreshButton != null ? refreshButton.GetComponentInChildren<TMP_Text>(true) : null;
        }

        private void EnsureNodes()
        {
            chefTabButton ??= ResolveButton("Panel/group_Tabs/btn_Chef", "btn_Chef");
            waiterTabButton ??= ResolveButton("Panel/group_Tabs/btn_Waiter", "btn_Waiter");
            shopkeeperTabButton ??= ResolveButton("Panel/group_Tabs/btn_Shopkeeper", "btn_Shopkeeper");
            chefTabLabel ??= chefTabButton != null ? chefTabButton.GetComponentInChildren<TMP_Text>(true) : null;
            waiterTabLabel ??= waiterTabButton != null ? waiterTabButton.GetComponentInChildren<TMP_Text>(true) : null;
            shopkeeperTabLabel ??= shopkeeperTabButton != null ? shopkeeperTabButton.GetComponentInChildren<TMP_Text>(true) : null;
            chefTabImage ??= chefTabButton != null ? chefTabButton.GetComponent<Image>() : null;
            waiterTabImage ??= waiterTabButton != null ? waiterTabButton.GetComponent<Image>() : null;
            shopkeeperTabImage ??= shopkeeperTabButton != null ? shopkeeperTabButton.GetComponent<Image>() : null;
            chefTabRedDot ??= ResolveTabRedDot(chefTabButton);
            waiterTabRedDot ??= ResolveTabRedDot(waiterTabButton);
            EnsureRefreshButtonBinding();

            RefreshTabVisibility();
            SetNodeVisible("Panel/group_List", true);
            SetNodeVisible("Panel/group_Single", false);
            SetNodeVisible("Panel/group_SingleList", false);
            SetNodeVisible("Panel/btn_Refresh", false, "btn_Refresh");
            SetNodeVisible("Panel/btn_Staff", false, "btn_Staff");
            SetNodeVisible("Panel/btn_ConfirmHire", false, "btn_ConfirmHire");
        }

        private bool ShouldShowShopkeeperTab()
        {
            // 掌柜开场视频后自动拥有，招聘界面不再展示掌柜页签。
            return false;
        }

        private bool ShouldShowChefTab()
        {
            // 小二/厨师页签常驻显示。
            return true;
        }

        private bool ShouldShowWaiterTab()
        {
            // 小二/厨师页签常驻显示。
            return true;
        }

        private void RefreshTabVisibility()
        {
            var showShopkeeper = ShouldShowShopkeeperTab();
            var showChef = ShouldShowChefTab();
            var showWaiter = ShouldShowWaiterTab();
            SetNodeVisible("Panel/group_Tabs", showShopkeeper || showChef || showWaiter);
            SetNodeVisible("Panel/group_Tabs/btn_Shopkeeper", showShopkeeper, "btn_Shopkeeper");
            SetNodeVisible("Panel/group_Tabs/btn_Chef", showChef, "btn_Chef");
            SetNodeVisible("Panel/group_Tabs/btn_Waiter", showWaiter, "btn_Waiter");
            EnsureValidCurrentRole(showShopkeeper, showChef, showWaiter);
        }

        private void EnsureValidCurrentRole(bool showShopkeeper, bool showChef, bool showWaiter)
        {
            var roleVisible = currentRole switch
            {
                StaffHireSelectRole.Shopkeeper => showShopkeeper,
                StaffHireSelectRole.Chef => showChef,
                _ => showWaiter
            };

            if (roleVisible)
            {
                return;
            }

            if (showShopkeeper)
            {
                currentRole = StaffHireSelectRole.Shopkeeper;
            }
            else if (showWaiter)
            {
                currentRole = StaffHireSelectRole.Waiter;
            }
            else if (showChef)
            {
                currentRole = StaffHireSelectRole.Chef;
            }

            InvalidateCandidateCache();
        }

        private void SwitchRole(StaffHireSelectRole role)
        {
            if (currentRole == role)
            {
                return;
            }

            if (role == StaffHireSelectRole.Shopkeeper)
            {
                if (!ShouldShowShopkeeperTab())
                {
                    return;
                }
            }
            else if (role == StaffHireSelectRole.Chef)
            {
                if (!ShouldShowChefTab())
                {
                    return;
                }
            }
            else if (!ShouldShowWaiterTab())
            {
                return;
            }

            currentRole = role;
            InvalidateCandidateCache();
            RefreshPanel();
        }

        private void InvalidateCandidateCache()
        {
            cachedCandidates = null;
            cachedRole = null;
        }

        private StaffPosition CurrentPosition => currentRole switch
        {
            StaffHireSelectRole.Chef => StaffPosition.Chef,
            StaffHireSelectRole.Shopkeeper => StaffPosition.Shopkeeper,
            _ => StaffPosition.Waiter
        };

        private string CurrentRoleTitle => currentRole switch
        {
            StaffHireSelectRole.Chef => "招聘厨师",
            StaffHireSelectRole.Shopkeeper => "招聘掌柜",
            _ => "招聘小二"
        };

        private bool IsFixedSlotRole => currentRole is StaffHireSelectRole.Chef or StaffHireSelectRole.Waiter;

        private int GetMaxHireCount(DataManager dataManager)
        {
            if (dataManager == null)
            {
                return 3;
            }

            return CurrentPosition switch
            {
                StaffPosition.Chef => dataManager.GetGuideUnlockedChefHireCount(),
                StaffPosition.Shopkeeper => dataManager.GetMaxShopkeeperHireCount(),
                _ => dataManager.GetGuideUnlockedWaiterHireCount()
            };
        }

        private List<Staff> GetOrRollCandidates()
        {
            if (cachedCandidates != null && cachedRole == currentRole)
            {
                return cachedCandidates;
            }

            var dataManager = DataManager.Instance;
            if (IsFixedSlotRole)
            {
                cachedCandidates = StaffConfigUtility.GetFixedHireSlotStaffs(CurrentPosition, FixedHireSlotCount);
            }
            else
            {
                cachedCandidates = dataManager != null
                    ? dataManager.RollHireCandidatesForRole(CurrentPosition, 1)
                    : new List<Staff>();
            }

            cachedRole = currentRole;
            return cachedCandidates;
        }

        private void RefreshPanel()
        {
            EnsureNodes();
            RefreshTabVisibility();
            SetText("Panel/txt_Title", ShouldShowShopkeeperTab() || ShouldShowChefTab() || ShouldShowWaiterTab()
                ? "招聘员工"
                : CurrentRoleTitle, "txt_Title");

            var dataManager = DataManager.Instance;
            var candidates = GetOrRollCandidates();

            if (ShouldShowShopkeeperTab() || ShouldShowChefTab() || ShouldShowWaiterTab())
            {
                RefreshRecruitTabs(dataManager);
            }

            SetText("Panel/txt_Hint", ResolveHintText(dataManager), "txt_Hint");

            var visibleCount = IsFixedSlotRole ? FixedHireSlotCount : 1;
            for (var index = 0; index < FixedHireSlotCount; index++)
            {
                if (index >= visibleCount)
                {
                    SetNodeVisible($"Panel/group_List/item_{index + 1}", false);
                    continue;
                }

                RefreshCandidateRow(index, index < candidates.Count ? candidates[index] : null);
            }

            SetNodeVisible("Panel/btn_ConfirmHire", false, "btn_ConfirmHire");
            SetNodeVisible("Panel/btn_Refresh", false, "btn_Refresh");
            SetNodeVisible("Panel/btn_Staff", false, "btn_Staff");
        }

        private void RefreshRecruitTabs(DataManager dataManager)
        {
            var chefHired = dataManager != null ? dataManager.CountHiredByPosition(StaffPosition.Chef) : 0;
            var waiterHired = dataManager != null ? dataManager.CountHiredByPosition(StaffPosition.Waiter) : 0;
            var shopkeeperHired = dataManager != null ? dataManager.CountHiredByPosition(StaffPosition.Shopkeeper) : 0;
            var chefMax = dataManager != null ? dataManager.GetGuideUnlockedChefHireCount() : 0;
            var waiterMax = dataManager != null ? dataManager.GetGuideUnlockedWaiterHireCount() : 0;
            var shopkeeperMax = dataManager != null ? dataManager.GetMaxShopkeeperHireCount() : 1;
            ApplyTabState(chefTabImage, chefTabLabel, $"厨师 {chefHired}/{chefMax}", currentRole == StaffHireSelectRole.Chef);
            ApplyTabState(waiterTabImage, waiterTabLabel, $"小二 {waiterHired}/{waiterMax}", currentRole == StaffHireSelectRole.Waiter);
            RefreshTabRedDots(dataManager);
            if (ShouldShowShopkeeperTab())
            {
                ApplyTabState(
                    shopkeeperTabImage,
                    shopkeeperTabLabel,
                    $"掌柜 {shopkeeperHired}/{shopkeeperMax}",
                    currentRole == StaffHireSelectRole.Shopkeeper);
            }
        }

        /// <summary>
        /// 厨师/小二页签红点：有可购买（已解锁且未入职）员工时显示。
        /// </summary>
        private void RefreshTabRedDots(DataManager dataManager)
        {
            SetTabRedDotVisible(
                chefTabRedDot,
                dataManager != null && dataManager.HasPurchasableFixedHireStaff(StaffPosition.Chef, FixedHireSlotCount));
            SetTabRedDotVisible(
                waiterTabRedDot,
                dataManager != null && dataManager.HasPurchasableFixedHireStaff(StaffPosition.Waiter, FixedHireSlotCount));
        }

        private static GameObject ResolveTabRedDot(Button tabButton)
        {
            if (tabButton == null)
            {
                return null;
            }

            return tabButton.transform.Find("img_Red")?.gameObject
                   ?? HudBindingUtility.FindChildRecursive(tabButton.transform, "img_Red")?.gameObject;
        }

        private static void SetTabRedDotVisible(GameObject redDot, bool visible)
        {
            if (redDot == null || redDot.activeSelf == visible)
            {
                return;
            }

            redDot.SetActive(visible);
        }

        private static void ApplyTabState(Image background, TMP_Text label, string content, bool selected)
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

        private static string ResolveHintText(DataManager dataManager)
        {
            var coinNum = dataManager?.PlayerData?.coinNum ?? 0;
            if (coinNum < LowCoinHintThreshold)
            {
                return $"{DefaultHintText}{LowCoinHintReminder}";
            }

            return DefaultHintText;
        }

        private bool IsStaffOwned(int staffId)
        {
            return StaffConfigUtility.FindOwnedStaffSave(staffId, preferNonTemporary: true) is { temporary: false };
        }

        private void RefreshCandidateRow(int index, Staff staff)
        {
            var rowPath = $"Panel/group_List/item_{index + 1}";
            var rowRoot = ResolveTransform(rowPath);
            SetNodeVisible(rowPath, true);

            var dataManager = DataManager.Instance;
            var tavernLevel = dataManager != null ? Mathf.Max(1, dataManager.GetTavernLevel()) : 1;
            var unlockLevel = staff != null ? Mathf.Max(1, staff.UnlockLevel) : index + 1;
            var locked = IsFixedSlotRole && staff != null && staff.UnlockLevel > tavernLevel;
            var owned = staff != null && IsStaffOwned(staff.Id);

            if (staff == null)
            {
                SetText($"{rowPath}/txt_Name", $"名额 {index + 1}");
                SetNodeVisible($"{rowPath}/txt_Status", false, "txt_Status");
                SetNodeVisible($"{rowPath}/txt_Cost", false, "txt_Cost");
                SetNodeVisible($"{rowPath}/btn_Recruit", false);
                RefreshCandidateMask(rowPath, showMask: false, string.Empty);
                var emptyBg = ResolveImage($"{rowPath}/img_Bg");
                if (emptyBg != null)
                {
                    emptyBg.color = new Color(0.85f, 0.82f, 0.75f, 0.7f);
                }

                return;
            }

            SetText($"{rowPath}/txt_Name", staff.Name);
            // 招聘卡隐藏天赋信息；价格在 btn_Recruit/txt_Label。
            SetNodeVisible($"{rowPath}/txt_Status", false, "txt_Status");
            // 已招聘显示 txt_Cost「已招聘」；未招聘隐藏。
            if (owned)
            {
                SetNodeVisible($"{rowPath}/txt_Cost", true, "txt_Cost");
                SetText($"{rowPath}/txt_Cost", "已招聘", "txt_Cost");
            }
            else
            {
                SetNodeVisible($"{rowPath}/txt_Cost", false, "txt_Cost");
            }

            // 仅「需升星解锁」显示遮罩；已招聘不盖遮罩。
            var showMask = locked;
            var maskText = locked ? $"{unlockLevel}星酒楼解锁" : string.Empty;
            RefreshCandidateMask(rowPath, showMask, maskText);

            var canRecruit = !owned && !locked;
            RefreshRecruitButton(rowPath, staff, canRecruit);

            var bg = rowRoot != null ? HudBindingUtility.ResolveChildImage(rowRoot, "img_Bg") : ResolveImage($"{rowPath}/img_Bg");
            if (bg != null)
            {
                bg.raycastTarget = true;
                bg.color = locked
                    ? new Color(0.78f, 0.76f, 0.72f, 0.85f)
                    : new Color(1f, 0.94f, 0.78f, 0.95f);
            }

            var portrait = rowRoot != null
                ? HudBindingUtility.ResolveChildImage(rowRoot, "img_Portrait")
                : ResolveImage($"{rowPath}/img_Portrait");
            if (portrait != null)
            {
                var role = currentRole == StaffHireSelectRole.Chef ? RecruitPanelRole.Chef : RecruitPanelRole.Waiter;
                var sprite = HudOverlayAssetCatalog.ResolveRecruitListPortrait(role, index, null);
                if (sprite != null)
                {
                    portrait.sprite = sprite;
                    portrait.enabled = true;
                }
            }

            var capturedStaff = staff;
            var capturedLocked = locked;
            void OnCardClicked()
            {
                OnCandidateCardClicked(capturedStaff, capturedLocked);
            }

            if (rowRoot != null)
            {
                var legacyRowButton = rowRoot.GetComponent<Button>();
                if (legacyRowButton != null)
                {
                    Destroy(legacyRowButton);
                }
            }

            if (bg != null)
            {
                var cardButton = bg.GetComponent<Button>();
                if (cardButton == null)
                {
                    cardButton = bg.gameObject.AddComponent<Button>();
                    cardButton.targetGraphic = bg;
                    cardButton.transition = Selectable.Transition.None;
                }

                BindButton(cardButton, OnCardClicked);
            }
        }

        /// <summary>
        /// 卡内招聘按钮：可招时显示「招聘 {价格}」并绑定直招。
        /// </summary>
        private void RefreshRecruitButton(string rowPath, Staff staff, bool canRecruit)
        {
            var recruitButton = ResolveButton($"{rowPath}/btn_Recruit", "btn_Recruit");
            SetNodeVisible($"{rowPath}/btn_Recruit", canRecruit);
            if (!canRecruit || recruitButton == null || staff == null)
            {
                return;
            }

            var cost = StaffConfigUtility.GetRecruitmentCost(staff.Id, staff.RecruitmentCosts);
            var label = ResolveRecruitButtonLabel(recruitButton);
            if (label != null)
            {
                label.text = $"招聘 {cost}";
            }

            recruitButton.interactable = true;
            var captured = staff;
            BindButton(recruitButton, () => TryHireSelectedStaff(captured));
        }

        private static TMP_Text ResolveRecruitButtonLabel(Button recruitButton)
        {
            if (recruitButton == null)
            {
                return null;
            }

            var labelTransform = recruitButton.transform.Find("txt_Label")
                                 ?? HudBindingUtility.FindChildRecursive(recruitButton.transform, "txt_Label");
            return labelTransform != null
                ? labelTransform.GetComponent<TMP_Text>()
                : recruitButton.GetComponentInChildren<TMP_Text>(true);
        }

        private void RefreshCandidateMask(string rowPath, bool showMask, string maskText)
        {
            var maskRoot = ResolveTransform($"{rowPath}/mask");
            if (maskRoot == null)
            {
                return;
            }

            if (maskRoot.gameObject.activeSelf != showMask)
            {
                maskRoot.gameObject.SetActive(showMask);
            }

            if (!showMask)
            {
                return;
            }

            SetText($"{rowPath}/mask/txt_mask", maskText, "txt_mask");
            var maskImages = maskRoot.GetComponentsInChildren<Image>(true);
            for (var index = 0; index < maskImages.Length; index++)
            {
                if (maskImages[index] != null)
                {
                    maskImages[index].raycastTarget = true;
                }
            }
        }

        /// <summary>
        /// 点卡：仅未解锁星级出 tips；招聘走 btn_Recruit。
        /// </summary>
        private void OnCandidateCardClicked(Staff staff, bool locked)
        {
            if (staff == null)
            {
                return;
            }

            if (locked)
            {
                var needLevel = Mathf.Max(1, staff.UnlockLevel);
                HudOverlayService.ShowFloatingWarning($"{needLevel}星酒楼解锁");
            }
        }

        /// <summary>
        /// 卡内招聘：扣费入职并刷新面板。
        /// </summary>
        private void TryHireSelectedStaff(Staff staff)
        {
            if (staff == null)
            {
                return;
            }

            var dataManager = DataManager.Instance;
            if (dataManager == null)
            {
                return;
            }

            var tavernLevel = Mathf.Max(1, dataManager.GetTavernLevel());
            if (IsFixedSlotRole && staff.UnlockLevel > tavernLevel)
            {
                HudOverlayService.ShowFloatingWarning($"{Mathf.Max(1, staff.UnlockLevel)}星酒楼解锁");
                return;
            }

            if (IsStaffOwned(staff.Id))
            {
                HudOverlayService.ShowFloatingWarning($"{staff.Name}已招聘");
                return;
            }

            if (!dataManager.TryHireConfiguredStaff(staff.Id, out var message))
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    HudOverlayService.ShowFloatingWarning(message);
                }

                return;
            }

            if (CurrentPosition == StaffPosition.Chef)
            {
                GameAudioManager.PlayRecruitChef();
                TavernSceneManager.Instance?.PlayGuideChefEnterFromBottomRecruit(staff.Id);
            }
            else
            {
                GameAudioManager.PlayRecruitWaiter();
                TavernSceneManager.Instance?.PlayGuideWaiterEnterFromBottomRecruit(staff.Id);
            }

            HudOverlayService.ShowFloatingWarning($"已招聘{staff.Name}");
            RefreshPanel();
        }
    }
}
