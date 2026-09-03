using System.Collections.Generic;
using cfg;
using JN.Client;
using JN.Client.Config;
using JN.Client.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    public class AchievementPanelControllerData : QFramework.UIPanelData
    {
    }

    /// <summary>
    /// 经营成就列表：生涯进度展示与手动领奖。
    /// </summary>
    public class AchievementPanelController : OverlayPanelController<AchievementPanelControllerData>
    {
        private int selectedIndex;
        private readonly List<Achievement> cachedList = new();

        protected override void OnPanelInit()
        {
            BindButton(ResolveButton("Panel/btn_Close", "btn_Close"), CloseSelf);
            BindButton(ResolveButton("Panel/btn_Upgrade", "btn_Upgrade"), ClaimSelected);
            SetNodeVisible("Panel/btn_Hire", false);
            SetNodeVisible("Panel/btn_Tech", false);
        }

        protected override void OnPanelOpen(AchievementPanelControllerData data)
        {
            selectedIndex = 0;
            Signals.Get<AchievementProgressSignal>().RemoveListener(RefreshPanel);
            Signals.Get<AchievementProgressSignal>().AddListener(RefreshPanel);
            RefreshPanel();
        }

        protected override void OnPanelShow()
        {
            RefreshPanel();
        }

        protected override void OnPanelClose()
        {
            Signals.Get<AchievementProgressSignal>().RemoveListener(RefreshPanel);
        }

        private void RefreshPanel()
        {
            SetText("Panel/txt_Title", "成就任务", "txt_Title");
            cachedList.Clear();
            var all = AchievementConfigUtility.GetAllSorted();
            for (var index = 0; index < all.Count; index++)
            {
                if (all[index] != null)
                {
                    cachedList.Add(all[index]);
                }
            }

            EnsureListCapacity(cachedList.Count);
            for (var index = 0; index < cachedList.Count; index++)
            {
                RefreshListItem(index, cachedList[index]);
            }

            for (var extra = cachedList.Count; extra < 32; extra++)
            {
                var extraPath = $"Panel/group_List/item_{extra + 1}";
                if (ResolveTransform(extraPath) == null)
                {
                    break;
                }

                SetNodeVisible(extraPath, false);
            }

            if (cachedList.Count == 0)
            {
                SetText("Panel/group_Detail/txt_Name", "暂无成就");
                SetText("Panel/group_Detail/txt_Basic", "请检查 Achievement 配表");
                SetText("Panel/group_Detail/txt_Status", "-");
                SetText("Panel/group_Detail/txt_Upgrade", "-");
                var emptyBtn = ResolveButton("Panel/btn_Upgrade", "btn_Upgrade");
                if (emptyBtn != null)
                {
                    emptyBtn.interactable = false;
                    var emptyLabel = emptyBtn.GetComponentInChildren<TMP_Text>(true);
                    if (emptyLabel != null)
                    {
                        emptyLabel.text = "领取";
                    }
                }

                return;
            }

            selectedIndex = Mathf.Clamp(selectedIndex, 0, cachedList.Count - 1);
            RefreshDetail(cachedList[selectedIndex]);
        }

        private void EnsureListCapacity(int count)
        {
            var listRoot = ResolveTransform("Panel/group_List");
            if (listRoot == null || count <= 0)
            {
                return;
            }

            var template = listRoot.Find("item_1");
            if (template == null)
            {
                return;
            }

            for (var slot = 2; slot <= count; slot++)
            {
                var existing = listRoot.Find($"item_{slot}");
                if (existing != null)
                {
                    continue;
                }

                var clone = Object.Instantiate(template.gameObject, listRoot);
                clone.name = $"item_{slot}";
                clone.SetActive(true);
            }
        }

        private void RefreshListItem(int index, Achievement achievement)
        {
            var path = $"Panel/group_List/item_{index + 1}";
            SetNodeVisible(path, true);
            var dataManager = DataManager.Instance;
            var current = dataManager != null ? dataManager.GetAchievementCurrentValue(achievement.Id) : 0;
            var target = AchievementConfigUtility.GetTarget(achievement);
            var claimed = dataManager != null && dataManager.IsAchievementClaimed(achievement.Id);
            var canClaim = dataManager != null && dataManager.CanClaimAchievement(achievement.Id);
            var selected = index == selectedIndex;

            SetText($"{path}/txt_Name", achievement.Name);
            SetText($"{path}/txt_Level", claimed ? "已领" : canClaim ? "可领" : $"{Mathf.Min(current, target)}/{target}");

            var bg = ResolveImage($"{path}/img_Bg");
            if (bg != null)
            {
                if (claimed)
                {
                    bg.color = new Color(0.72f, 0.86f, 0.62f, 1f);
                }
                else if (canClaim)
                {
                    bg.color = new Color(0.98f, 0.82f, 0.45f, 1f);
                }
                else if (selected)
                {
                    bg.color = new Color(0.98f, 0.88f, 0.55f, 1f);
                }
                else
                {
                    bg.color = new Color(1f, 0.94f, 0.78f, 0.9f);
                }
            }

            var captured = index;
            BindButton(ResolveButton($"{path}/btn_Select", "btn_Select"), () =>
            {
                selectedIndex = captured;
                RefreshPanel();
            });
        }

        private void RefreshDetail(Achievement achievement)
        {
            var dataManager = DataManager.Instance;
            var current = dataManager != null ? dataManager.GetAchievementCurrentValue(achievement.Id) : 0;
            var target = AchievementConfigUtility.GetTarget(achievement);
            var claimed = dataManager != null && dataManager.IsAchievementClaimed(achievement.Id);
            var canClaim = dataManager != null && dataManager.CanClaimAchievement(achievement.Id);

            SetText("Panel/group_Detail/txt_Name", achievement.Name);
            SetText("Panel/group_Detail/txt_Basic", achievement.Desc);
            SetText("Panel/group_Detail/txt_Status", $"进度 {Mathf.Min(current, target)}/{target}");
            SetText(
                "Panel/group_Detail/txt_Upgrade",
                claimed ? "奖励已领取" : canClaim ? $"可领取 {achievement.RewardCoin} 铜钱" : $"达成后领取 {achievement.RewardCoin} 铜钱");

            var claimBtn = ResolveButton("Panel/btn_Upgrade", "btn_Upgrade");
            if (claimBtn != null)
            {
                claimBtn.interactable = canClaim;
                var label = claimBtn.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.text = claimed ? "已领取" : canClaim ? "领取奖励" : "未达成";
                }
            }
        }

        private void ClaimSelected()
        {
            if (selectedIndex < 0 || selectedIndex >= cachedList.Count || DataManager.Instance == null)
            {
                return;
            }

            var achievement = cachedList[selectedIndex];
            if (!DataManager.Instance.TryClaimAchievement(achievement.Id, out var message))
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    HudOverlayService.ShowFloatingWarning(message);
                }

                return;
            }

            GameAudioManager.PlayAchievementRewardCoin();
            HudOverlayService.ShowFloatingWarning(message);
            RefreshPanel();
        }
    }
}
