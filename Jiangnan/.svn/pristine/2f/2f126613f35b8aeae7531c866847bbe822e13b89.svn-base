using JN.Client.Manager;
using JN.Client.Model;
using JN.Client.UI;
using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 菜单解锁后，一楼客人入座到上菜前的菜单反馈文字气泡。
    /// 贵客菜单普通客的「菜品太贵了」离店由点单入座后的专用流程处理。
    /// </summary>
    internal static class TavernMenuGuestReactionService
    {
        private const float TriggerChance = 0.2f;
        private const float TipDurationSeconds = 2f;

        /// <summary>
        /// 客人入座后按概率弹出菜单反馈气泡（上菜前有效）。
        /// </summary>
        public static void TryShowSeatedReactionTip(TavernCustomerRuntimeController customer)
        {
            if (!CanShowReactionTip(customer))
            {
                return;
            }

            var dataManager = DataManager.Instance;
            var vipMenu = dataManager != null && dataManager.IsVipMenuSelected();
            // 贵客菜单普通客走专用「太贵离店」，此处不弹随机反馈以免叠字。
            if (vipMenu && customer != null && !customer.IsVip)
            {
                return;
            }

            if (Random.value >= TriggerChance)
            {
                return;
            }

            var line = ResolveReactionLine(vipMenu, customer.IsVip);
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            HudOverlayService.ShowCustomerReviewTip(
                customer.transform,
                line,
                durationSeconds: TipDurationSeconds);
        }

        private static bool CanShowReactionTip(TavernCustomerRuntimeController customer)
        {
            if (customer == null || !customer.IsSeated || customer.IsLeavingTavern)
            {
                return false;
            }

            if (SceneFlowCoordinator.IsOnTavernSecondFloor())
            {
                return false;
            }

            var dataManager = DataManager.Instance;
            if (dataManager == null
                || dataManager.IsVisitingOtherTavern
                || !dataManager.IsTavernMenuEntryUnlocked())
            {
                return false;
            }

            var tableData = dataManager.GetTableData(customer.TableId);
            if (tableData == null)
            {
                return false;
            }

            var tableState = (TavernTableRuntimeState)tableData.runtimeState;
            return tableState == TavernTableRuntimeState.Idle
                   || tableState == TavernTableRuntimeState.WaitingOrder
                   || tableState == TavernTableRuntimeState.WaitingServe;
        }

        /// <summary>
        /// 按当前菜单与客人类型取反馈文案。
        /// </summary>
        internal static string ResolveReactionLine(bool vipMenuSelected, bool isVipGuest)
        {
            if (vipMenuSelected)
            {
                return isVipGuest ? "可惜大堂太吵" : "太贵了";
            }

            return isVipGuest ? "都是些粗茶淡饭" : "合我的口味";
        }
    }
}
