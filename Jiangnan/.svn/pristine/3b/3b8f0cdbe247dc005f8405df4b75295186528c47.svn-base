using JN.Client.Manager;
using JN.Client.Model;
using JN.Client.Scene;
using QFramework;
using UnityEngine;
using UnityEngine.Video;

namespace JN.Client.UI
{
    /// <summary>
    /// Town HUD Root 面板数据。
    /// </summary>
    public class TownStatusBarPanelControllerData : UIPanelData
    {
    }

    /// <summary>
    /// Town HUD Root，只负责子 panel 生命周期、贷款演出状态与整体协同。
    /// </summary>
    /// <summary>
    /// Town HUD Root。
    /// 只负责子面板生命周期、贷款演出状态和整体流程协调。
    /// </summary>
    public class TownStatusBarPanelController : HudPanelController<TownStatusBarPanelControllerData>
    {
        public static bool IsOpeningLoanPresentationActive { get; private set; }

        private const string EnterVideoAssetPath = "Assets/Res/Resources/Videos/enterVideo.mp4";

        [SerializeField] private VideoClip openingLoanVideoResourcePath;
        [SerializeField] private VideoClip guidedBuildVideoResourcePath;

        private readonly HudRootCoordinator hudRootCoordinator = new();
        private bool hasTriggeredTownIntroSequence;

        /// <summary>
        /// 初始化时打开 Town HUD 子面板。
        /// </summary>
        protected override void OnPanelInit()
        {
            if (DisableIfNestedInsideAnotherPanel())
            {
                return;
            }

            OpenChildPanels();
        }

        /// <summary>
        /// 面板显示时注册信号并刷新全量状态。
        /// </summary>
        protected override void OnPanelShow()
        {
            if (DisableIfNestedInsideAnotherPanel())
            {
                return;
            }

            Signals.Get<UpdateCoinNumSignal>().AddListener(HandleCoinChanged);
            Signals.Get<GameplayGuideProgressSignal>().AddListener(HandleTownStateChanged);
            Signals.Get<TavernBusinessStateSignal>().AddListener(HandleTavernBusinessStateChanged);

            OpenChildPanels();
            RefreshAllPanels();
            TryStartTownOpeningLoanSequence();
        }

        /// <summary>
        /// 面板关闭时清理监听和演出状态。
        /// </summary>
        protected override void OnPanelClose()
        {
            if (DisableIfNestedInsideAnotherPanel())
            {
                return;
            }

            Signals.Get<UpdateCoinNumSignal>().RemoveListener(HandleCoinChanged);
            Signals.Get<GameplayGuideProgressSignal>().RemoveListener(HandleTownStateChanged);
            Signals.Get<TavernBusinessStateSignal>().RemoveListener(HandleTavernBusinessStateChanged);

            CloseChildPanels();
            VideoWindowController.HideActiveWindow();
            SetOpeningLoanPresentationActive(false);
        }

        /// <summary>
        /// 刷新所有 Town 子面板。
        /// </summary>
        public void RefreshAllPanels()
        {
            UIKit.GetPanel<TownTopStatusPanelController>()?.RefreshPanel();
            UIKit.GetPanel<TownBottomNavPanelController>()?.RefreshPanel();
        }

        /// <summary>
        /// 判断当前玩家是否拥有已完工的店铺。
        /// </summary>
        public bool HasCompletedOwnedBuilding()
        {
            return DataManager.Instance != null && DataManager.Instance.HasCompletedOwnedSelfTownBuilding();
        }

        /// <summary>
        /// 处理从 Town 进入 Tavern 的请求：二星及以上先二次确认（展示拉客占用），一星直接进店。
        /// </summary>
        public void HandleEnterTavernRequest()
        {
            var ownedBuilding = DataManager.Instance != null
                ? DataManager.Instance.GetCompletedOwnedSelfTownBuilding()
                : null;
            if (ownedBuilding == null)
            {
                return;
            }

            var tileId = ownedBuilding.tileId;
            var buildingLevel = ownedBuilding.buildingLevel;
            var tavernLevel = DataManager.Instance != null ? DataManager.Instance.GetTavernLevel() : 1;
            if (tavernLevel < 2)
            {
                StartCoroutine(SceneFlowCoordinator.EnterTavern(tileId, buildingLevel));
                return;
            }

            var used = DataManager.Instance.GetJiaoziUsedCapacity();
            var max = DataManager.Instance.GetJiaoziCapacity();
            HudOverlayService.ShowConfirmBox(
                "确定返回酒楼吗？",
                $"拉客 {used}/{max}",
                () => StartCoroutine(SceneFlowCoordinator.EnterTavern(tileId, buildingLevel)));
        }

        /// <summary>
        /// 退出地块聚焦，并同步底部按钮状态。
        /// </summary>
        public void HandleExitFocusRequest()
        {
            CameraController.Instance?.ExitTileFocusMode();
            UIKit.GetPanel<TownBottomNavPanelController>()?.RefreshPanel();
        }

        /// <summary>
        /// 切换贷款演出状态，并联动世界物件显隐。
        /// </summary>
        public static void SetOpeningLoanPresentationActive(bool isActive)
        {
            IsOpeningLoanPresentationActive = isActive;
            UIKit.GetPanel<BuildingItemSceneController>()?.SetSceneItemsVisible(!isActive);
        }

        /// <summary>
        /// 播放建造引导视频（已关闭：直接回调完成建造/进店）。
        /// </summary>
        public static void PlayGuidedBuildVideo(System.Action onFinished)
        {
            onFinished?.Invoke();
        }

        /// <summary>
        /// 组装传给 Town 子面板的共享数据。
        /// </summary>
        private TownHudPanelData BuildHudData()
        {
            return new TownHudPanelData
            {
                HudRoot = transform,
                RootController = this
            };
        }

        /// <summary>
        /// 打开 Town 顶部和底部两个子面板。
        /// </summary>
        private void OpenChildPanels()
        {
            var hudData = BuildHudData();
            hudRootCoordinator.EnsureOpened<TownTopStatusPanelController>(uiData: hudData);
            hudRootCoordinator.EnsureOpened<TownBottomNavPanelController>(uiData: hudData);
        }

        /// <summary>
        /// 关闭 Town 顶部和底部两个子面板。
        /// </summary>
        private void CloseChildPanels()
        {
            hudRootCoordinator.Close<TownTopStatusPanelController>();
            hudRootCoordinator.Close<TownBottomNavPanelController>();
        }

        /// <summary>
        /// 金币变动时只刷新顶部状态面板。
        /// </summary>
        private void HandleCoinChanged(int changeNum)
        {
            UIKit.GetPanel<TownTopStatusPanelController>()?.HandleCoinChanged(changeNum);
        }

        /// <summary>
        /// Town 引导状态变化时刷新 HUD。
        /// </summary>
        private void HandleTownStateChanged()
        {
            RefreshAllPanels();
        }

        /// <summary>
        /// 酒楼营业状态变化时刷新底部入口状态。
        /// </summary>
        private void HandleTavernBusinessStateChanged(bool isOpen)
        {
            UIKit.GetPanel<TownBottomNavPanelController>()?.RefreshPanel();
        }

        /// <summary>
        /// 新账号取名后首次进 Town：播放开场视频，结束后打开贷款窗。
        /// </summary>
        private void TryStartTownOpeningLoanSequence()
        {
            if (hasTriggeredTownIntroSequence || !DataManager.Instance.ShouldShowOpeningLoanWindow())
            {
                return;
            }

            hasTriggeredTownIntroSequence = true;
            SetOpeningLoanPresentationActive(true);

            var clip = ResolveEnterVideoClip();
            if (clip == null)
            {
                Debug.LogWarning($"[TownStatusBarPanelController] 缺少开场视频：{EnterVideoAssetPath}，直接打开贷款窗。");
                OpenOpeningLoanWindow();
                return;
            }

            VideoWindowController.Show(clip, OpenOpeningLoanWindow, pauseOnLastFrame: false);
        }

        /// <summary>
        /// 加载开场进镇视频 enterVideo.mp4（不读旧的 grandOpening 序列化引用）。
        /// </summary>
        private static VideoClip ResolveEnterVideoClip()
        {
            return GameplayResourceStore.LoadAsset<VideoClip>(EnterVideoAssetPath);
        }

        /// <summary>
        /// 打开贷款窗口。
        /// </summary>
        private static void OpenOpeningLoanWindow()
        {
            // 开场视频结束：直接拥有掌柜，无需招聘界面。
            DataManager.Instance?.EnsureStarterShopkeeperOwned();

            if (UIKit.GetPanel<LoanWindowController>() != null)
            {
                return;
            }

            UIKit.OpenPanel<LoanWindowController>(JiangNanUIPanelLayerConfig.Resolve<LoanWindowController>(UILevel.PopUI));
        }
    }
}
