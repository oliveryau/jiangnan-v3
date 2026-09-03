using System;
using System.Collections;
using JN.Client.Manager;
using JN.Client.UI;
using JN.Client.Utils;
using QFramework;
using UnityEngine;
using UnityEngine.Networking;

namespace JN.Client
{
    /// <summary>
    /// 负责入口相关的运行时逻辑。
    /// </summary>
    public class Main : MonoBehaviour
    {
        

        /// <summary>
        /// 初始化组件引用和运行时状态。
        /// </summary>
        protected void Awake()
        {
            // 尽早锁定 60 帧；GameSetting.Apply 切换画质后会再次应用帧率策略。
            GamePerformanceSettings.ApplyFrameRate();

            // 按依赖顺序初始化基础系统，避免后续 界面 打开时访问到未准备好的数据。
            LocalizationManager.Instance.Init();
            LubanManager.Instance.Init();
            GameManager.Instance.Init();
            DataManager.Instance.Init();
            GOReferenceManager.Instance.Init();

            // 安装业务侧对通用框架的适配逻辑。
            JiangNanLocalizationInstaller.Install();
            JiangNanUIKitBootstrap.Initialize();

            GamePerformanceSettings.ApplyFrameRate();
            Application.runInBackground = true;




            UIKit.OpenPanel<LoginPanelController>(JiangNanUIPanelLayerConfig.Resolve<LoginPanelController>());
        }

    }
}
