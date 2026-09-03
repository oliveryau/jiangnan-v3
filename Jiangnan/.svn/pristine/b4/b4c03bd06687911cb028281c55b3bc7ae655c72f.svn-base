using System;
using cfg;
using JN.Client.Config;
using JN.Client.Manager;
using JN.Client.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    public class VipGuestDishGuessPanelControllerData : QFramework.UIPanelData
    {
        /// <summary>
        /// 0 表示纯调试随机，不绑定桌位。
        /// </summary>
        public int TableId;

        public bool ForceRegenerate;

        /// <summary>
        /// 面板关闭后回调（确认或关闭均会触发）。
        /// </summary>
        public Action OnClosed;
    }

    /// <summary>
    /// 贵客猜菜三选一面板：txt_Hint 展示需求，item 按钮直接提交选择。
    /// </summary>
    public class VipGuestDishGuessPanelController : OverlayPanelController<VipGuestDishGuessPanelControllerData>
    {
        private const int OptionCount = 3;

        private VipGuestDishGuessSession session;

        protected override void OnPanelInit()
        {
            EnsureNodes();
            BindButton(ResolveButton("Panel/btn_Close", "btn_Close"), CloseSelf);

            for (var index = 0; index < OptionCount; index++)
            {
                var captured = index;
                BindButton(ResolveButton($"Panel/group_List/item_{index + 1}/btn_Recruit"), () => SubmitSelection(captured));
            }
        }

        protected override void OnPanelOpen(VipGuestDishGuessPanelControllerData data)
        {
            var tableId = data != null ? data.TableId : 0;
            var forceRegenerate = data != null && data.ForceRegenerate;
            session = VipGuestDishGuessService.GetOrCreateSession(tableId, forceRegenerate);
            RefreshPanel();
        }

        protected override void OnPanelShow()
        {
            RefreshPanel();
        }

        protected override void OnPanelClose()
        {
            var onClosed = Data?.OnClosed;
            if (Data != null)
            {
                Data.OnClosed = null;
            }

            onClosed?.Invoke();
        }

        private void EnsureNodes()
        {
            SetNodeVisible("Panel/group_List", true);
            SetNodeVisible("Panel/btn_ConfirmHire", false);
        }

        private void RefreshPanel()
        {
            EnsureNodes();

            if (session == null)
            {
                SetText("Panel/txt_Hint", "暂无可用菜品配置", "txt_Hint");
                for (var index = 0; index < OptionCount; index++)
                {
                    RefreshDishRow(index, null);
                }

                return;
            }

            SetText("Panel/txt_Hint", session.Demand.DisplayText, "txt_Hint");

            for (var index = 0; index < OptionCount; index++)
            {
                var dish = index < session.Options.Count ? session.Options[index] : null;
                RefreshDishRow(index, dish);
            }
        }

        private void RefreshDishRow(int index, Dish dish)
        {
            var rowPath = $"Panel/group_List/item_{index + 1}";
            SetNodeVisible(rowPath, true);

            var recruitButton = ResolveButton($"{rowPath}/btn_Recruit");
            if (dish == null)
            {
                SetText($"{rowPath}/txt_Name", $"候选 {index + 1}");
                SetText($"{rowPath}/txt_Cost", string.Empty);
                SetText($"{rowPath}/btn_Recruit/txt_Label", "-");
                SetNodeVisible($"{rowPath}/btn_Recruit", false);
                if (recruitButton != null)
                {
                    recruitButton.interactable = false;
                }

                return;
            }

            SetText($"{rowPath}/txt_Name", dish.Name);
            SetText($"{rowPath}/txt_Cost", BuildDishSummaryText(dish));
            SetText($"{rowPath}/btn_Recruit/txt_Label", "选择");
            SetNodeVisible($"{rowPath}/btn_Recruit", !session.HasAnswered);
            if (recruitButton != null)
            {
                recruitButton.interactable = !session.HasAnswered;
            }

            var portrait = ResolveImage($"{rowPath}/img_Portrait");
            if (portrait != null)
            {
                var sprite = DishIconResolver.TryResolve(dish.Name, dish.Icon);
                portrait.sprite = sprite;
                portrait.enabled = sprite != null;
            }
        }

        private void SubmitSelection(int index)
        {
            if (session == null || session.HasAnswered)
            {
                return;
            }

            if (index < 0 || index >= session.Options.Count)
            {
                HudOverlayService.ShowFloatingWarning("暂无可用菜品");
                return;
            }

            session.SelectedIndex = index;
            if (!VipGuestDishGuessService.TryConfirm(session))
            {
                HudOverlayService.ShowFloatingWarning("提交失败，请重试");
                return;
            }

            switch (session.Outcome)
            {
                case VipGuestGuessOutcome.Satisfied:
                    GameAudioManager.PlayDishGuessCorrect();
                    HudOverlayService.ShowFloatingWarning("贵客很满意！！！");
                    break;
                case VipGuestGuessOutcome.PremiumMismatch:
                    GameAudioManager.PlayDishGuessPremiumProfit();
                    HudOverlayService.ShowFloatingWarning("恭喜赚到更多钱~");
                    break;
                default:
                    GameAudioManager.PlayDishGuessWrong();
                    HudOverlayService.ShowFloatingWarning("没有满足贵客需求，也没有赚到更多钱~");
                    break;
            }

            CloseSelf();
        }

        private static string BuildDishSummaryText(Dish dish)
        {
            if (dish == null || dish.Price <= 0)
            {
                return string.Empty;
            }

            return $"价格: {dish.Price}";
        }
    }
}
