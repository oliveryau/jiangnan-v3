using System;
using JN.Client;
using JN.Client.Manager;
using JN.Client.Scene;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ApiProtocols = global::JN.Client.Protocols.Protocols;

namespace JN.Client.UI
{
    public class NewBuildingWindowControllerData : UIPanelData
    {
        /// <summary>
        /// 保存目标地块编号。
        /// </summary>
        public int tileId;

        /// <summary>
        /// 保存确认操作回调。
        /// </summary>
        public Action confirmAction;
    }

    /// <summary>
    /// 负责新建建筑窗口逻辑。
    /// </summary>
    public class NewBuildingWindowController : QFrameworkPanel<NewBuildingWindowControllerData>
    {
        private const int DefaultLevel1CoinChange = 0;
        /// <summary>开局建造酒楼的锤子/烟雾动画时长（秒）。</summary>
        private const int DefaultBuildDuration = 3;

        private int SelfPlayerId => ResolveSelfPlayerId();

        [SerializeField] private TextMeshProUGUI txt_Title;
        [SerializeField] private Button btn_Close;
        [SerializeField] private Button btn_SelectBuilding_1;
        [SerializeField] private Button btn_SelectBuilding_2;
        [SerializeField] private Button btn_SelectBuilding_3;

        /// <summary>
        /// 面板初始化时绑定控件和事件。
        /// </summary>
        protected override void OnPanelInit()
        {
            btn_Close.onClick.AddListener(OnClickClose);
            btn_SelectBuilding_1.onClick.AddListener(OnClickSelectBuilding1);
            btn_SelectBuilding_2.onClick.AddListener(OnClickSelectBuilding2);
            btn_SelectBuilding_3.onClick.AddListener(OnClickSelectBuilding3);
        }

        /// <summary>
        /// 面板打开时读取数据并刷新显示。
        /// </summary>
        /// <param name="data">数据。</param>
        protected override void OnPanelOpen(NewBuildingWindowControllerData data)
        {
            RefreshView();
        }

        /// <summary>
        /// 面板关闭时清理临时状态和监听。
        /// </summary>
        protected override void OnPanelClose()
        {
            btn_Close.onClick.RemoveListener(OnClickClose);
            btn_SelectBuilding_1.onClick.RemoveListener(OnClickSelectBuilding1);
            btn_SelectBuilding_2.onClick.RemoveListener(OnClickSelectBuilding2);
            btn_SelectBuilding_3.onClick.RemoveListener(OnClickSelectBuilding3);
        }

        /// <summary>
        /// 刷新窗口显示。
        /// </summary>
        private void RefreshView()
        {
            var buildingInfo = DataManager.Instance.GetTownBuildingInfos().Find(info => info.tileId == Data.tileId);
            var canBuildOnThisLand = buildingInfo != null
                                     && buildingInfo.playerId == SelfPlayerId
                                     && buildingInfo.buildingLevel <= 0
                                     && buildingInfo.status == 0;

            btn_SelectBuilding_1.interactable = canBuildOnThisLand;
            btn_SelectBuilding_2.interactable = canBuildOnThisLand;
            btn_SelectBuilding_3.interactable = canBuildOnThisLand;

            if (!canBuildOnThisLand)
            {
                txt_Title.text = "请先购买自己的地块";
                return;
            }

            if (Data.tileId != 0)
            {
               // txt_Title.text = $"地块：{Data.tileId} 选择新建建筑";
               txt_Title.text = $"我要经营";
            }
        }

        /// <summary>
        /// 处理关闭点击事件。
        /// </summary>
        private void OnClickClose()
        {
            CloseSelf();
        }

        /// <summary>
        /// 解析当前玩家编号。
        /// </summary>
        /// <returns>返回方法执行后的结果。</returns>
        private int ResolveSelfPlayerId()
        {
            if (DataManager.Instance?.PlayerData == null)
            {
                return 0;
            }

            if (int.TryParse(DataManager.Instance.PlayerData.playerId, out var playerId))
            {
                return playerId;
            }

            return 0;
        }

        /// <summary>
        /// 处理选择 1 级建筑按钮点击。
        /// </summary>
        private void OnClickSelectBuilding1()
        {
            // 新店 1 级建造免费。
            ConfirmSelection(DefaultLevel1CoinChange, 1);
        }

        /// <summary>
        /// 处理选择 2 级建筑按钮点击。
        /// </summary>
        private void OnClickSelectBuilding2()
        {
            ConfirmSelection(-4000, 2);
        }

        /// <summary>
        /// 处理选择 3 级建筑按钮点击。
        /// </summary>
        private void OnClickSelectBuilding3()
        {
            ConfirmSelection(-5000, 3);
        }

        /// <summary>
        /// 处理确认选择建筑。
        /// </summary>
        /// <param name="coinChange">参数值。</param>
        /// <param name="buildingLevel">等级。</param>
        private void ConfirmSelection(int coinChange, int buildingLevel)
        {
            if (!DataManager.Instance.TryStartTownBuilding(Data.tileId, buildingLevel, coinChange, DefaultBuildDuration, out var message))
            {
                txt_Title.text = message;
                return;
            }

            GameAudioManager.PlayTownBuild();
            // 外部回调存在时由调用方接管视频流程，否则在本窗口内兜底触发。
            Data.confirmAction?.Invoke();
            var buildingInfo = DataManager.Instance.GetTownBuildingInfos().Find(info => info.tileId == Data.tileId);
            TileManager.Instance.UpdateTile(Data.tileId, buildingInfo);
            TileManager.Instance.RefreshAllTileViews();
            SyncTownBuildingToServer(Data.tileId, buildingLevel);
            CloseSelf();
        }

        /// <summary>
        /// 将本地建造结果同步到服务端，保证其他玩家进入大世界时能看到最新地块与建筑等级。
        /// </summary>
        /// <param name="tileId">地块编号。</param>
        /// <param name="buildingLevel">建筑等级。</param>
        private static void SyncTownBuildingToServer(int tileId, int buildingLevel)
        {
            if (LocalSaveMode.Enabled)
            {
                return;
            }

            var token = DataManager.Instance.AuthToken;
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            var tileIdText = tileId.ToString();
            ApiProtocols.Instance.SelectLand(
                token,
                tileIdText,
                onSuccess: _ =>
                {
                    Debug.Log($"[NewBuildingWindowController] SelectLand 成功：tileId={tileIdText}");
                },
                onError: error =>
                {
                    Debug.LogWarning($"[NewBuildingWindowController] SelectLand 失败：tileId={tileIdText}, error={error}");
                });

            // 当前大世界读取其它玩家建筑等级时使用 player.level，建造后同步一份等级。
            ApiProtocols.Instance.UpdatePlayerData(
                token,
                new { level = Mathf.Clamp(buildingLevel, 1, 3) },
                onSuccess: _ =>
                {
                    Debug.Log($"[NewBuildingWindowController] UpdatePlayerData 成功：level={buildingLevel}");
                },
                onError: error =>
                {
                    Debug.LogWarning($"[NewBuildingWindowController] UpdatePlayerData 失败：level={buildingLevel}, error={error}");
                });
        }

        /// <summary>
        /// 没有外部建造回调时，按地块补发建造视频流程。
        /// </summary>
        public static bool TryBuildDefaultLevel1(int tileId, Action confirmAction, out string message)
        {
            if (!DataManager.Instance.TryStartTownBuilding(tileId, 1, DefaultLevel1CoinChange, DefaultBuildDuration, out message))
            {
                return false;
            }

            GameAudioManager.PlayTownBuild();
            // 默认建造也保留无回调兜底，避免新入口漏掉建造视频。
            confirmAction?.Invoke();
            var buildingInfo = DataManager.Instance.GetTownBuildingInfos().Find(info => info.tileId == tileId);
            TileManager.Instance.UpdateTile(tileId, buildingInfo);
            TileManager.Instance.RefreshAllTileViews();
            SyncTownBuildingToServer(tileId, 1);
            return true;
        }
    }
}
