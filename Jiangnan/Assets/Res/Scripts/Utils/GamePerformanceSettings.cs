using UnityEngine;

namespace JN.Client.Utils
{
    /// <summary>
    /// 帧率与画质档位策略：SetQualityLevel 会覆盖 vSync，需在切换画质后重新应用帧率。
    /// </summary>
    public static class GamePerformanceSettings
    {
        public const int TargetFrameRate = 60;

        /// <summary>
        /// 首次安装默认画质：手机偏低档保 60 帧，PC 维持高画质。
        /// </summary>
        public static int GetDefaultGraphicsQuality()
        {
            return Application.isMobilePlatform ? 0 : 2;
        }

        /// <summary>
        /// 手机最高 Balanced，避免 High Fidelity 拖帧。
        /// </summary>
        public static int ClampGraphicsQuality(int quality)
        {
            quality = Mathf.Clamp(quality, 0, 2);
            if (Application.isMobilePlatform)
            {
                quality = Mathf.Min(quality, 1);
            }

            return quality;
        }

        public static void ApplyFrameRate()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFrameRate;
        }

        public static void ApplyGraphicsQuality(int qualityLevel)
        {
            qualityLevel = ClampGraphicsQuality(qualityLevel);
            QualitySettings.SetQualityLevel(qualityLevel, true);
            ApplyFrameRate();
        }
    }
}
