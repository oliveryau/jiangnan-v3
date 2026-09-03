using System;
using System.Collections.Generic;
using cfg;
using JN.Client;
using JN.Client.Config;
using JN.Client.Manager;
using JN.Client.Tools;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    public class AchievementCatalogPanelControllerData : QFramework.UIPanelData
    {
    }

    /// <summary>
    /// 成就图鉴：scroll_Content 纵向列表（AchItem 模板）；同类成就链仅展示当前待处理的一档。
    /// </summary>
    public class AchievementCatalogPanelController : OverlayPanelController<AchievementCatalogPanelControllerData>
    {
        private const string HiddenText = "???";
        private const string ItemTemplateName = "AchItem";
        private const string ScrollViewPath = "Panel/scroll_View";
        private const string ScrollContentPath = "Panel/scroll_View/scroll_Viewport/scroll_Content";

        private readonly List<Achievement> cachedEntries = new();
        private ScrollRect catalogScrollRect;
        private bool scrollLayoutPrepared;

        private static readonly Color CompletedSecondaryTextColor = new(0.35f, 0.28f, 0.18f, 1f);
        private static readonly Color HiddenTextColor = new(0.55f, 0.55f, 0.55f, 1f);

        protected override void OnPanelInit()
        {
            BindButton(ResolveButton("Panel/btn_Close", "btn_Close"), CloseSelf);
            PrepareScrollLayout();
        }

        protected override void OnPanelOpen(AchievementCatalogPanelControllerData data)
        {
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
            SetText("Panel/txt_Title", "成就图鉴", "txt_Title");

            cachedEntries.Clear();
            var dataManager = DataManager.Instance;
            var entries = AchievementConfigUtility.GetCatalogDisplayAchievements(
                id => dataManager != null && dataManager.IsAchievementClaimed(id));
            for (var index = 0; index < entries.Count; index++)
            {
                if (entries[index] != null)
                {
                    cachedEntries.Add(entries[index]);
                }
            }

            SortCatalogEntries(dataManager);

            EnsureListCapacity(cachedEntries.Count);
            for (var index = 0; index < cachedEntries.Count; index++)
            {
                RefreshCatalogItem(index, cachedEntries[index]);
            }

            var listRoot = ResolveListRoot();
            if (listRoot != null)
            {
                for (var index = 0; index < listRoot.childCount; index++)
                {
                    if (index >= cachedEntries.Count)
                    {
                        listRoot.GetChild(index).gameObject.SetActive(false);
                    }
                }
            }

            RefreshScrollLayout();
        }

        private void SortCatalogEntries(DataManager dataManager)
        {
            cachedEntries.Sort((left, right) =>
            {
                if (left == null && right == null)
                {
                    return 0;
                }

                if (left == null)
                {
                    return 1;
                }

                if (right == null)
                {
                    return -1;
                }

                var rankCompare = GetCatalogSortRank(left, dataManager).CompareTo(GetCatalogSortRank(right, dataManager));
                if (rankCompare != 0)
                {
                    return rankCompare;
                }

                return left.Id.CompareTo(right.Id);
            });
        }

        // 图鉴排序：可领取 > 可展示 > 未解锁。
        private static int GetCatalogSortRank(Achievement achievement, DataManager dataManager)
        {
            if (achievement == null || dataManager == null)
            {
                return 2;
            }

            if (dataManager.CanClaimAchievement(achievement.Id))
            {
                return 0;
            }

            if (dataManager.IsAchievementClaimed(achievement.Id))
            {
                return 1;
            }

            return 2;
        }

        private void PrepareScrollLayout()
        {
            if (scrollLayoutPrepared)
            {
                return;
            }

            catalogScrollRect = ResolveComponent<ScrollRect>(ScrollViewPath, "scroll_View");
            scrollLayoutPrepared = true;
        }

        private void RefreshScrollLayout()
        {
            PrepareScrollLayout();

            var content = ResolveTransform(ScrollContentPath, "scroll_Content") as RectTransform;
            if (content != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            }

            if (catalogScrollRect != null)
            {
                catalogScrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private static string GetItemNodeName(int index)
        {
            return index == 0 ? ItemTemplateName : $"{ItemTemplateName}_{index + 1}";
        }

        private string GetItemPath(int index)
        {
            return $"{ScrollContentPath}/{GetItemNodeName(index)}";
        }

        private string GetItemImagePath(int index)
        {
            return $"{GetItemPath(index)}/Panel/Image";
        }

        private Transform ResolveListRoot()
        {
            return ResolveTransform(ScrollContentPath, "scroll_Content");
        }

        private void EnsureListCapacity(int count)
        {
            var listRoot = ResolveListRoot();
            if (listRoot == null || count <= 0)
            {
                return;
            }

            var template = listRoot.Find(ItemTemplateName);
            if (template == null)
            {
                return;
            }

            for (var slot = 2; slot <= count; slot++)
            {
                var nodeName = GetItemNodeName(slot - 1);
                if (listRoot.Find(nodeName) != null)
                {
                    continue;
                }

                var clone = UnityEngine.Object.Instantiate(template.gameObject, listRoot);
                clone.name = nodeName;
                clone.SetActive(true);
            }
        }

        private void RefreshCatalogItem(int index, Achievement achievement)
        {
            var itemPath = GetItemPath(index);
            var imagePath = GetItemImagePath(index);
            SetNodeVisible(itemPath, true);

            var dataManager = DataManager.Instance;
            var completed = dataManager != null && dataManager.IsAchievementCompleted(achievement.Id);
            var claimed = dataManager != null && dataManager.IsAchievementClaimed(achievement.Id);
            var canClaim = dataManager != null && dataManager.CanClaimAchievement(achievement.Id);
            var displayed = dataManager != null && dataManager.IsAchievementDisplayed(achievement.Id);

            SetText($"{imagePath}/Frame/Name", achievement.Name, "Name");
            SetText($"{imagePath}/Desc", completed ? achievement.Desc : HiddenText, "Desc");
            SetText($"{imagePath}/Reward/RewardDesc", "奖励：", "RewardDesc");
            SetText($"{imagePath}/Reward/RewardNum", achievement.RewardCoin.ToString(), "RewardNum");

            ApplyTextColor(ResolveText($"{imagePath}/Desc", "Desc"), completed, CompletedSecondaryTextColor);

            var frame = ResolveImage($"{imagePath}/Frame");
            if (frame != null)
            {
                AchievementDisplayAssetCatalog.ApplyAchievementBackground(frame, achievement, completed);
            }

            RefreshActionButtons(imagePath, achievement.Id, claimed, canClaim, displayed);
            BindAchievementItemClick(imagePath);
        }

        private void BindAchievementItemClick(string imagePath)
        {
            var cardImage = ResolveImage(imagePath);
            if (cardImage == null)
            {
                return;
            }

            cardImage.raycastTarget = true;
            var cardButton = cardImage.GetComponent<Button>();
            if (cardButton == null)
            {
                cardButton = cardImage.gameObject.AddComponent<Button>();
                cardButton.targetGraphic = cardImage;
                cardButton.transition = Selectable.Transition.None;
            }

            BindButton(cardButton, () => { });
        }

        private void RefreshActionButtons(
            string imagePath,
            int achievementId,
            bool claimed,
            bool canClaim,
            bool displayed)
        {
            var btnGet = ResolveButton($"{imagePath}/BtnGet", "BtnGet");
            var btnShow = ResolveButton($"{imagePath}/BtnShow", "BtnShow");
            var capturedId = achievementId;

            if (claimed)
            {
                // 已领奖 / 展示：只显示 BtnShow，隐藏领取区与 Lock。
                SetNodeVisible(btnShow, true);
                SetNodeVisible(btnGet, false);
                SetNodeVisible($"{imagePath}/Reward", false, "Reward");
                SetNodeVisible($"{imagePath}/BtnGet/Lock", false, "Lock");

                if (btnShow != null)
                {
                    btnShow.interactable = !displayed;
                    SetButtonLabel(btnShow, displayed ? "展示中" : "展示");
                }

                BindButton(btnShow, () => TryDisplay(capturedId));
                return;
            }

            // 未领奖：显示 BtnGet + Reward；未达成时叠加 Lock。
            SetNodeVisible(btnShow, false);
            SetNodeVisible(btnGet, true);
            SetNodeVisible($"{imagePath}/Reward", true, "Reward");
            SetNodeVisible($"{imagePath}/BtnGet/Lock", !canClaim, "Lock");

            if (btnGet != null)
            {
                btnGet.interactable = canClaim;
                SetButtonLabel(btnGet, "领取");
            }

            BindButton(btnGet, () =>
            {
                if (canClaim)
                {
                    TryClaim(capturedId);
                }
            });
        }

        private static void ApplyTextColor(TMP_Text text, bool completed, Color completedColor)
        {
            if (text == null)
            {
                return;
            }

            text.color = completed ? completedColor : HiddenTextColor;
        }

        private static void SetButtonLabel(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            var text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = label;
            }
        }

        private void SetNodeVisible(Button button, bool visible)
        {
            if (button != null)
            {
                button.gameObject.SetActive(visible);
            }
        }

        private void TryDisplay(int achievementId)
        {
            if (DataManager.Instance == null)
            {
                return;
            }

            if (!DataManager.Instance.TrySetDisplayedAchievement(achievementId, out var message))
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    HudOverlayService.ShowFloatingWarning(message);
                }

                return;
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                HudOverlayService.ShowFloatingWarning(message);
            }

            RefreshPanel();
        }

        private void TryClaim(int achievementId)
        {
            if (DataManager.Instance == null)
            {
                return;
            }

            if (!DataManager.Instance.TryClaimAchievement(achievementId, out var message))
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    HudOverlayService.ShowFloatingWarning(message);
                }

                return;
            }

            GameAudioManager.PlayAchievementRewardCoin();

            if (!string.IsNullOrWhiteSpace(message))
            {
                HudOverlayService.ShowFloatingWarning(message);
            }

            RefreshPanel();
        }
    }
}
