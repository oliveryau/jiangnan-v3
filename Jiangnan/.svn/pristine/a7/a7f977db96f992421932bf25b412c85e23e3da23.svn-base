using System;
using UnityEngine;

namespace JN.Client.UI
{
    public enum RecruitPanelRole
    {
        Chef,
        Waiter
    }

    /// <summary>
    /// 旧版 Tavern 运行时 HUD 兼容入口。
    /// 新逻辑统一转发到 HudOverlayService，避免继续在这里堆叠实现细节。
    /// </summary>
    public static class TavernRuntimeModalUI
    {
        /// <summary>
        /// 显示功能解锁提示。
        /// </summary>
        public static void ShowNewFeatureOpenToast()
        {
            HudOverlayService.ShowNewFeatureOpenToast();
        }

        /// <summary>
        /// 显示桌位二级解锁提示。
        /// </summary>
        public static void ShowNewFeatureOpenTableLv2Panel(Action onComplete = null)
        {
            HudOverlayService.ShowNewFeatureOpenTableLv2Panel(onComplete);
        }

        /// <summary>
        /// 显示招募确认面板。
        /// </summary>
        public static void ShowRecruitPanel(string displayName, string roleText, Sprite portrait, int cost, Action onConfirm)
        {
            HudOverlayService.ShowRecruitPanel(displayName, roleText, portrait, cost, onConfirm);
        }

        /// <summary>
        /// 显示招募列表面板。
        /// </summary>
        public static void ShowRecruitListPanel(RecruitPanelRole defaultRole = RecruitPanelRole.Chef)
        {
            HudOverlayService.ShowRecruitListPanel(defaultRole);
        }

        /// <summary>
        /// 显示厨师进度条。
        /// </summary>
        public static void ShowChefCookProgress(Transform target, float duration, Vector3 worldOffset)
        {
            HudOverlayService.ShowChefCookProgress(target, duration, worldOffset);
        }

        /// <summary>
        /// 显示小二进度条。
        /// </summary>
        public static void ShowWaiterTaskProgress(Transform target, float duration, Vector3 worldOffset)
        {
            HudOverlayService.ShowWaiterTaskProgress(target, duration, worldOffset);
        }

        /// <summary>
        /// 显示带图标的小二进度条。
        /// </summary>
        public static void ShowWaiterTaskProgress(Transform target, float duration, Vector3 worldOffset, Sprite icon)
        {
            HudOverlayService.ShowWaiterTaskProgress(target, duration, worldOffset, icon);
        }

        /// <summary>
        /// 显示小二状态图标。
        /// </summary>
        public static GameObject ShowWaiterStateIcon(Transform target, Sprite icon, Action onClick, Vector3 worldOffset)
        {
            return HudOverlayService.ShowWaiterStateIcon(target, icon, onClick, worldOffset);
        }

        /// <summary>
        /// 显示订单烹饪进度条。
        /// </summary>
        public static GameObject ShowWaiterOrderCookProgress(Transform target, Sprite icon, Func<float> progressProvider, Vector3 worldOffset)
        {
            return HudOverlayService.ShowWaiterOrderCookProgress(target, icon, progressProvider, worldOffset);
        }

        /// <summary>
        /// 显示桌位升级确认面板。
        /// </summary>
        public static void ShowTableUpgradePanel(Scene.TableArea table, Action onConfirm)
        {
            HudOverlayService.ShowTableUpgradePanel(table, onConfirm);
        }

        /// <summary>
        /// 显示通用信息面板。
        /// </summary>
        public static void ShowInfoPanel(string title, string content)
        {
            HudOverlayService.ShowInfoPanel(title, content);
        }

        /// <summary>
        /// 显示通用二次确认弹窗。
        /// </summary>
        public static void ShowConfirmBox(string title, string content, Action onConfirm)
        {
            HudOverlayService.ShowConfirmBox(title, content, onConfirm);
        }

        /// <summary>
        /// 显示短时警告提示。
        /// </summary>
        public static void ShowFloatingWarning(string content)
        {
            HudOverlayService.ShowFloatingWarning(content);
        }
    }
}
