using System.Collections.Generic;
using cfg;
using JN.Client.Config;
using JN.Client.Manager;
using JN.Client.Model;
using JN.Client.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    public class StaffInfoPanelControllerData : QFramework.UIPanelData
    {
        public int FocusStaffId;
    }

    /// <summary>
    /// 员工信息：列表 + 当前能力与下一项科技提示。
    /// </summary>
    public class StaffInfoPanelController : OverlayPanelController<StaffInfoPanelControllerData>
    {
        private const int MaxListItemCount = 6;
        private const string FireButtonDefaultLabel = "解雇";

        private int selectedIndex;
        private bool fireConfirmPending;
        private int fireConfirmStaffId;
        private Button fireButton;
        private TMP_Text fireButtonLabel;

        protected override void OnPanelInit()
        {
            BindButton(ResolveButton("Panel/btn_Close", "btn_Close"), CloseSelf);
            BindButton(ResolveButton("Panel/btn_Hire", "btn_Hire"), () =>
            {
                var dataManager = DataManager.Instance;
                if (dataManager == null || dataManager.IsStaffHireSlotCapReached())
                {
                    HudOverlayService.ShowFloatingWarning("请先升级酒楼");
                    return;
                }

                HudOverlayService.ShowStaffHireSelectPanel(StaffHireSelectRole.Waiter);
            });
            BindButton(ResolveButton("Panel/btn_Tech", "btn_Tech"), HudOverlayService.ShowTavernTechTreePanel);
            fireButton = ResolveButton("Panel/btn_Fire", "btn_Fire")
                         ?? ResolveButton("Panel/btn_Upgrade", "btn_Upgrade");
            fireButtonLabel = fireButton != null ? fireButton.GetComponentInChildren<TMP_Text>(true) : null;
            BindButton(fireButton, HandleFireButtonClicked);
        }

        protected override void OnPanelOpen(StaffInfoPanelControllerData data)
        {
            selectedIndex = 0;
            ResetFireConfirmState();
            if (data != null && data.FocusStaffId > 0)
            {
                var list = DataManager.Instance?.GetOwnedStaffList();
                if (list != null)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        if (list[i] != null && list[i].staffId == data.FocusStaffId)
                        {
                            selectedIndex = i;
                            break;
                        }
                    }
                }
            }

            RefreshPanel();
        }

        protected override void OnPanelShow()
        {
            RefreshPanel();
        }

        protected override void OnPanelClose()
        {
            ResetFireConfirmState();
        }

        private void RefreshPanel()
        {
            SetText("Panel/txt_Title", "员工信息", "txt_Title");
            RefreshHireButton();
            var owned = DataManager.Instance != null
                ? DataManager.Instance.GetOwnedStaffList()
                : (IReadOnlyList<LocalStaffSaveData>)System.Array.Empty<LocalStaffSaveData>();

            for (var index = 0; index < MaxListItemCount; index++)
            {
                RefreshListItem(index, index < owned.Count ? owned[index] : null);
            }

            if (owned.Count == 0)
            {
                ResetFireConfirmState();
                RefreshFireButton(null);
                SetText("Panel/group_Detail/txt_Name", "暂无员工");
                SetText("Panel/group_Detail/txt_Basic", "点击「招聘」雇佣低阶员工");
                SetText("Panel/group_Detail/txt_Status", "-");
                SetText("Panel/group_Detail/txt_Upgrade", "-");
                ApplyPortrait("Panel/group_Detail/img_Portrait", null);
                return;
            }

            selectedIndex = Mathf.Clamp(selectedIndex, 0, owned.Count - 1);
            var selectedSave = owned[selectedIndex];
            if (fireConfirmPending
                && (selectedSave == null || selectedSave.staffId != fireConfirmStaffId))
            {
                ResetFireConfirmState();
            }

            RefreshFireButton(selectedSave);
            RefreshDetail(selectedSave);
        }

        private void RefreshHireButton()
        {
            // 招聘按钮常显；满员时点击 tips「请先升级酒楼」，不再隐藏。
            SetNodeVisible("Panel/btn_Hire", true);
        }

        private void RefreshFireButton(LocalStaffSaveData save)
        {
            var staff = save != null ? StaffConfigUtility.GetOrNull(save.staffId) : null;
            var canFire = staff != null && staff.Position == StaffPosition.Waiter;
            if (fireButton != null)
            {
                fireButton.gameObject.SetActive(canFire);
            }

            if (!canFire)
            {
                return;
            }

            if (fireButtonLabel != null)
            {
                fireButtonLabel.text = fireConfirmPending && save.staffId == fireConfirmStaffId
                    ? "确认解雇？"
                    : FireButtonDefaultLabel;
            }
        }

        private void HandleFireButtonClicked()
        {
            var owned = DataManager.Instance?.GetOwnedStaffList();
            if (owned == null || owned.Count == 0)
            {
                return;
            }

            selectedIndex = Mathf.Clamp(selectedIndex, 0, owned.Count - 1);
            var save = owned[selectedIndex];
            if (save == null)
            {
                return;
            }

            var dataManager = DataManager.Instance;
            if (dataManager == null)
            {
                HudOverlayService.ShowFloatingWarning("操作失败");
                ResetFireConfirmState();
                RefreshPanel();
                return;
            }

            if (!dataManager.CanFireWaiter(save.staffId, out var message))
            {
                HudOverlayService.ShowFloatingWarning(message);
                ResetFireConfirmState();
                RefreshPanel();
                return;
            }

            if (!fireConfirmPending || fireConfirmStaffId != save.staffId)
            {
                fireConfirmPending = true;
                fireConfirmStaffId = save.staffId;
                RefreshFireButton(save);
                return;
            }

            ResetFireConfirmState();
            TavernSceneManager.Instance?.DismissWaiterByStaffId(save.staffId);
            if (dataManager.TryFireWaiter(save.staffId, out message))
            {
                HudOverlayService.ShowFloatingWarning(message);
                var remainingCount = dataManager.GetOwnedStaffList()?.Count ?? 0;
                selectedIndex = remainingCount > 0
                    ? Mathf.Min(selectedIndex, remainingCount - 1)
                    : 0;
                RefreshPanel();
                return;
            }

            HudOverlayService.ShowFloatingWarning(message);
            RefreshPanel();
        }

        private void ResetFireConfirmState()
        {
            fireConfirmPending = false;
            fireConfirmStaffId = 0;
        }

        private void RefreshListItem(int index, LocalStaffSaveData save)
        {
            var path = $"Panel/group_List/item_{index + 1}";
            if (save == null)
            {
                SetNodeVisible(path, false);
                return;
            }

            SetNodeVisible(path, true);
            var staff = StaffConfigUtility.GetOrNull(save.staffId);
            var profile = StaffConfigUtility.GetProfile(save.staffId, save);
            var selected = index == selectedIndex;

            SetText($"{path}/txt_Name", profile.Name);
            SetText($"{path}/txt_Pos", ResolveStaffPositionLabel(staff));
            SetNodeVisible($"{path}/txt_Level", false);
            SetNodeVisible($"{path}/img_sel", selected);

            ApplyPortrait($"{path}/portrait", staff);

            var bg = ResolveImage($"{path}/img_Bg");
            if (bg != null)
            {
                bg.color = Color.white;
            }

            var captured = index;
            BindButton(ResolveButton($"{path}/img_Bg"), () =>
            {
                selectedIndex = captured;
                if (fireConfirmPending)
                {
                    var owned = DataManager.Instance?.GetOwnedStaffList();
                    var selectedSave = owned != null && captured < owned.Count ? owned[captured] : null;
                    if (selectedSave == null || selectedSave.staffId != fireConfirmStaffId)
                    {
                        ResetFireConfirmState();
                    }
                }

                RefreshPanel();
            });
        }

        private void RefreshDetail(LocalStaffSaveData save)
        {
            var staff = StaffConfigUtility.GetOrNull(save.staffId);
            var profile = StaffConfigUtility.GetProfile(save.staffId, save);

            SetText("Panel/group_Detail/txt_Name", profile.Name);
            SetText(
                "Panel/group_Detail/txt_Basic",
                BuildStaffBasicInfo(staff));

            var basicText = ResolveText("Panel/group_Detail/txt_Basic", "txt_Basic");
            if (basicText != null)
            {
                basicText.color = Color.white;
            }

            ApplyPortrait("Panel/group_Detail/img_Portrait", staff);

            var skills =
                $"点单{(profile.CanOrder ? " 会" : " 不会")}  " +
                $"上菜{(profile.CanServe ? " 会" : " 不会")}  " +
                $"收账{(profile.CanCheckout ? " 会" : " 不会")}";
            var extraMul = profile.Position == StaffPosition.Chef
                ? $"做菜×{profile.CookSpeedMul:0.##}"
                : $"点单×{profile.OrderTimeMul:0.##}  上菜×{profile.ServeTimeMul:0.##}  收账×{profile.CheckoutTimeMul:0.##}";
            // 员工信息隐藏天赋展示。
            var statusText = $"技能：{skills}\n情绪 {profile.Emotion:0}\n{extraMul}";
            SetText("Panel/group_Detail/txt_Status", statusText);

            SetText(
                "Panel/group_Detail/txt_Upgrade",
                StaffProgressionService.BuildStaffTechHint(save, profile.Position));
        }

        private static string ResolveStaffPositionLabel(Staff staff)
        {
            if (staff == null)
            {
                return string.Empty;
            }

            return staff.Position switch
            {
                StaffPosition.Shopkeeper => "掌柜",
                StaffPosition.Chef => "厨师",
                StaffPosition.Waiter => "小二",
                _ => string.Empty
            };
        }

        private static string BuildStaffBasicInfo(Staff staff)
        {
            if (staff == null)
            {
                return string.Empty;
            }

            // 不展示天赋名/描述。
            if (!string.IsNullOrWhiteSpace(staff.Remark))
            {
                return staff.Remark.Trim();
            }

            return ResolveStaffPositionLabel(staff);
        }

        private void ApplyPortrait(string path, Staff staff)
        {
            var portrait = HudOverlayAssetCatalog.ResolveStaffPortrait(staff);
            var image = ResolveImage(path, "portrait");
            if (image == null)
            {
                return;
            }

            if (portrait != null)
            {
                image.sprite = portrait;
                image.enabled = true;
            }
        }
    }
}
