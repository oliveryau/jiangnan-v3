using JN.Client;
using JN.Client.Manager;
using JN.Client.Messages;
using JN.Client.Scene;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    public class LoanWindowControllerData : UIPanelData
    {
    }

    public class LoanWindowController : QFrameworkPanel<LoanWindowControllerData>
    {
        /// <summary>开局点确定贷款后，城镇相机目标世界 X。</summary>
        private const float OpeningLoanConfirmCameraX = 35f;

        [SerializeField] private Button btn_Loan;
        [SerializeField] private TextMeshProUGUI txt_LoanNum;

        /// <summary>
        /// 初始化借贷窗口按钮与金额显示。
        /// </summary>
        protected override void OnPanelInit()
        {
            CacheReferences();
            btn_Loan?.onClick.AddListener(OnClickBtnLoan);
            RefreshLoanAmount();
        }

        /// <summary>
        /// 关闭借贷窗口时清理监听和视频窗口。
        /// </summary>
        protected override void OnPanelClose()
        {
            btn_Loan?.onClick.RemoveListener(OnClickBtnLoan);
            VideoWindowController.HideActiveWindow();
        }

        /// <summary>
        /// 借贷成功后自动买默认地块，并保持 Town 全局视角。
        /// </summary>
        private void OnClickBtnLoan()
        {
            if (!DataManager.Instance.TryTakeLoan(out _))
            {
                return;
            }

            var coinTarget = GOReferenceManager.Instance != null ? GOReferenceManager.Instance.GetCoinTransform() : null;
            if (coinTarget != null)
            {
                GameAudioManager.PlayCheckoutCoins();
                GameUIEffects.PlayCoinsFly(btn_Loan.transform, coinTarget);
            }

            if (!TryAcquireDefaultTile(out _, out var message))
            {
                Debug.LogWarning($"[LoanWindowController] Auto purchase default tile failed: {message}");
                HudOverlayService.ShowFloatingWarning(string.IsNullOrWhiteSpace(message) ? "领取地块失败" : message);
                // 贷款已发放：仍结束开场态并关窗，避免流程卡死。
                FinishOpeningLoanFlow();
                return;
            }

            FinishOpeningLoanFlow();
        }

        /// <summary>结束开场贷款态：关聚焦、镜头移到指定 X、刷新底栏并关窗。</summary>
        private void FinishOpeningLoanFlow()
        {
            TownStatusBarPanelController.SetOpeningLoanPresentationActive(false);
            CameraController.Instance?.ExitTileFocusMode();
            MoveTownCameraToOpeningLoanX();
            UIKit.GetPanel<TownBottomNavPanelController>()?.RefreshPanel();
            CloseSelf();
        }

        /// <summary>贷款确定后将城镇相机平滑移到世界 X=35。</summary>
        private static void MoveTownCameraToOpeningLoanX()
        {
            var camera = CameraController.Instance;
            if (camera == null)
            {
                return;
            }

            var pos = camera.transform.position;
            camera.SetTargetPosition(new Vector3(OpeningLoanConfirmCameraX, pos.y, pos.z));
        }

        /// <summary>
        /// 缓存借贷金额文本引用。
        /// </summary>
        private void CacheReferences()
        {
            if (txt_LoanNum == null)
            {
                txt_LoanNum = transform.Find("group_Main/@btn_Loan/txt_LoanNum")?.GetComponent<TextMeshProUGUI>();
            }
        }

        /// <summary>
        /// 借贷后免费领取默认地块（首块地费用为 0）。
        /// </summary>
        private static bool TryAcquireDefaultTile(out Tile tile, out string message)
        {
            tile = ResolveDefaultOwnedTile();
            if (tile != null)
            {
                message = string.Empty;
                return true;
            }

            var tileId = DataManager.Instance != null
                ? DataManager.Instance.ResolveDefaultPurchasableTownTileId()
                : ResolveDefaultPurchasableTileId();
            if (tileId <= 0)
            {
                message = "未找到可领取的默认地块";
                return false;
            }

            if (!IsTilePurchasableInScene(tileId))
            {
                message = "默认地块已被占用";
                return false;
            }

            // 必须真正写入存档归属，否则后续点击「建造」会因「请先购买该地块」失败。
            if (!DataManager.Instance.TryPurchaseTownLand(tileId, out message))
            {
                return false;
            }

            tile = TileManager.Instance != null ? TileManager.Instance.GetTile(tileId) : null;
            var buildingInfo = DataManager.Instance.GetTownBuildingInfos()
                .Find(info => info != null && info.tileId == tileId);
            if (TileManager.Instance != null)
            {
                TileManager.Instance.UpdateTile(tileId, buildingInfo);
                TileManager.Instance.RefreshAllTileViews();
            }

            message = "地块领取成功";
            return tile != null;
        }

        /// <summary>
        /// 查找当前玩家已拥有但尚未建造的地块。
        /// </summary>
        private static Tile ResolveDefaultOwnedTile()
        {
            if (TileManager.Instance == null || DataManager.Instance?.PlayerData == null)
            {
                return null;
            }

            if (!int.TryParse(DataManager.Instance.PlayerData.playerId, out var selfPlayerId))
            {
                return null;
            }

            var preferredTileId = DataManager.Instance.PlayerData.buildId;
            if (preferredTileId > 0
                && TryGetBuildableOwnedTile(preferredTileId, selfPlayerId, out var preferredTile))
            {
                return preferredTile;
            }

            foreach (var info in DataManager.Instance.GetTownBuildingInfos())
            {
                if (info == null
                    || info.playerId != selfPlayerId
                    || info.buildingLevel > 0
                    || info.status != 0)
                {
                    continue;
                }

                if (!TryGetSceneBuildingInfo(info.tileId, out var sceneInfo)
                    || sceneInfo.playerId != selfPlayerId
                    || sceneInfo.buildingLevel > 0
                    || sceneInfo.status != 0)
                {
                    continue;
                }

                if (TileManager.Instance.AllTiles.TryGetValue(info.tileId, out var tile) && tile != null)
                {
                    return tile;
                }
            }

            return null;
        }

        /// <summary>
        /// 从场景实时地块里挑选可购的自家地块（Config.selfBuildingFieldId）。
        /// </summary>
        private static int ResolveDefaultPurchasableTileId()
        {
            if (DataManager.Instance != null)
            {
                return DataManager.Instance.ResolveDefaultPurchasableTownTileId();
            }

            return 0;
        }

        /// <summary>
        /// 判断指定地块是否是自己的可建造空地。
        /// </summary>
        private static bool TryGetBuildableOwnedTile(int tileId, int selfPlayerId, out Tile tile)
        {
            tile = null;
            if (TileManager.Instance == null || !TileManager.Instance.AllTiles.TryGetValue(tileId, out tile) || tile == null)
            {
                return false;
            }

            if (!TryGetSceneBuildingInfo(tileId, out var info))
            {
                return false;
            }

            return info != null && info.playerId == selfPlayerId && info.buildingLevel <= 0 && info.status == 0;
        }

        /// <summary>
        /// 以场景 Tile 数据为准判断地块是否可购买。
        /// </summary>
        private static bool IsTilePurchasableInScene(int tileId)
        {
            if (TileManager.Instance == null || !TileManager.Instance.AllTiles.TryGetValue(tileId, out var tile) || tile == null)
            {
                return false;
            }

            var info = ResolveSceneBuildingInfo(tileId);
            return info == null || info.playerId == 0;
        }

        /// <summary>
        /// 优先读取 Tile 当前绑定的地块数据，避免误用过期存档。
        /// </summary>
        private static BuildingInfo ResolveSceneBuildingInfo(int tileId)
        {
            if (TileManager.Instance == null)
            {
                return null;
            }

            if (TileManager.Instance.AllTiles.TryGetValue(tileId, out var tile) && tile != null && tile.buildingInfo != null)
            {
                return tile.buildingInfo;
            }

            if (TileManager.Instance.AllBuildingDatas.TryGetValue(tileId, out var sceneInfo) && sceneInfo != null)
            {
                return sceneInfo;
            }

            return DataManager.Instance.GetTownBuildingInfos().Find(buildingInfo => buildingInfo != null && buildingInfo.tileId == tileId);
        }

        /// <summary>
        /// 获取场景中指定地块的最新业务数据。
        /// </summary>
        private static bool TryGetSceneBuildingInfo(int tileId, out BuildingInfo info)
        {
            info = ResolveSceneBuildingInfo(tileId);
            return info != null;
        }

        /// <summary>
        /// 刷新当前可领取的借贷金额。
        /// </summary>
        private void RefreshLoanAmount()
        {
            if (txt_LoanNum == null)
            {
                return;
            }

            txt_LoanNum.text = DataManager.Instance.GetNextLoanAmount().ToString();
        }
    }
}
