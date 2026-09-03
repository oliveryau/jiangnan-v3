using System;
using QFramework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using JN.Client.Manager;

namespace JN.Client.UI
{
    public class VideoWindowControllerData : UIPanelData
    {
        public VideoClip clip;
        public Action onFinished;
        public bool pauseOnLastFrame = true;
    }

    public class VideoWindowController : QFrameworkPanel<VideoWindowControllerData>
    {
        private const int VideoWindowSortingOrder = 1000000;

        [SerializeField] private RectTransform group_Main;
        [SerializeField] private TownIntroVideoPlayer townIntroVideoPlayer;

        private Action m_OnFinished;
        private Canvas m_RootCanvas;
        private bool m_HasPausedBgmForCurrentPlayback;

        /// <summary>
        /// 打开视频窗口并播放指定视频。
        /// </summary>
        public static void Show(VideoClip clip, Action onFinished, bool pauseOnLastFrame = true)
        {
            var data = new VideoWindowControllerData
            {
                clip = clip,
                onFinished = onFinished,
                pauseOnLastFrame = pauseOnLastFrame
            };

            var panel = UIKit.GetPanel<VideoWindowController>();
            if (panel == null)
            {
                UIKit.OpenPanel<VideoWindowController>(
                    JiangNanUIPanelLayerConfig.Resolve<VideoWindowController>(UILevel.PopUI),
                    data);
                return;
            }

            panel.Open(data);
        }

        /// <summary>
        /// 关闭当前正在播放的视频窗口。
        /// </summary>
        public static void HideActiveWindow()
        {
            if (UIKit.GetPanel<VideoWindowController>() == null)
            {
                return;
            }

            UIKit.ClosePanel<VideoWindowController>();
        }

        /// <summary>
        /// 初始化窗口内引用。
        /// </summary>
        protected override void OnPanelInit()
        {
            CacheReferences();
        }

        /// <summary>
        /// 窗口打开时按传入数据开始播放。
        /// </summary>
        protected override void OnPanelOpen(VideoWindowControllerData data)
        {
            CacheReferences();
            ConfigureRootCanvas();
            PlayClip(data);
        }

        /// <summary>
        /// 显示时确保视频层在最上方。
        /// </summary>
        protected override void OnPanelShow()
        {
            ConfigureRootCanvas();
            transform.SetAsLastSibling();
            if (group_Main != null)
            {
                group_Main.SetAsLastSibling();
            }
        }

        /// <summary>
        /// 关闭窗口时清理播放器状态。
        /// </summary>
        protected override void OnPanelClose()
        {
            m_OnFinished = null;
            townIntroVideoPlayer?.HideIntro();
            if (m_HasPausedBgmForCurrentPlayback)
            {
                GameAudioManager.ResumeBgmForVideo();
                m_HasPausedBgmForCurrentPlayback = false;
            }
        }

        /// <summary>
        /// 缓存视频窗口依赖组件。
        /// </summary>
        private void CacheReferences()
        {
            if (group_Main == null)
            {
                group_Main = transform.Find("group_Main") as RectTransform;
            }

            if (townIntroVideoPlayer == null)
            {
                townIntroVideoPlayer = GetComponent<TownIntroVideoPlayer>();
            }
        }

        private void ConfigureRootCanvas()
        {
            m_RootCanvas ??= GetComponent<Canvas>();
            if (m_RootCanvas == null)
            {
                m_RootCanvas = gameObject.AddComponent<Canvas>();
            }

            var parentCanvas = transform.parent != null ? transform.parent.GetComponentInParent<Canvas>() : null;
            m_RootCanvas.overrideSorting = true;
            m_RootCanvas.sortingOrder = VideoWindowSortingOrder;
            if (parentCanvas != null)
            {
                m_RootCanvas.sortingLayerID = parentCanvas.sortingLayerID;
                m_RootCanvas.worldCamera = parentCanvas.worldCamera;
                m_RootCanvas.planeDistance = parentCanvas.planeDistance;
            }

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        /// <summary>
        /// 根据播放参数启动视频。
        /// </summary>
        private void PlayClip(VideoWindowControllerData data)
        {
            if (data == null)
            {
                return;
            }

            if (group_Main != null)
            {
                group_Main.SetAsLastSibling();
            }

            m_OnFinished = data.onFinished;
            if (townIntroVideoPlayer == null)
            {
                Debug.LogWarning("[VideoWindowController] Missing TownIntroVideoPlayer component.");
                InvokeCallbackImmediately();
                return;
            }

            // 每次打开窗口都重置本次视频，传空时使用播放器组件上的默认视频。
            townIntroVideoPlayer.SetClip(data.clip);

            if (!townIntroVideoPlayer.HasPlayableClip)
            {
                Debug.LogWarning("[VideoWindowController] Video clip reference is missing.");
                InvokeCallbackImmediately();
                return;
            }

            if (!m_HasPausedBgmForCurrentPlayback)
            {
                GameAudioManager.PauseBgmForVideo();
                m_HasPausedBgmForCurrentPlayback = true;
            }

            if (data.pauseOnLastFrame)
            {
                townIntroVideoPlayer.PlayAndPauseOnLastFrame(HandlePlaybackFinished);
                return;
            }

            townIntroVideoPlayer.PlayToEnd(HandlePlaybackFinished);
        }

        /// <summary>
        /// 视频自然结束后通知业务流程。
        /// </summary>
        private void HandlePlaybackFinished()
        {
            var callback = m_OnFinished;
            m_OnFinished = null;
            callback?.Invoke();
            CloseSelf();
        }

        /// <summary>
        /// 缺少视频时直接继续后续流程。
        /// </summary>
        private void InvokeCallbackImmediately()
        {
            var callback = m_OnFinished;
            m_OnFinished = null;
            callback?.Invoke();
            CloseSelf();
        }
    }
}
