using System;
using cfg;
using JN.Client.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.Config
{
    /// <summary>
    /// 成就图鉴 / 城镇展示共用的图标资源。
    /// </summary>
    public static class AchievementDisplayAssetCatalog
    {
        private const string DefaultFramePath = "Assets/Res/Resources/Textures/UI/Panel/Recruit/frame.png";
        private const string DefaultIconPath = "Assets/Res/Resources/Textures/UI/TechTree/diandan1.png";
        private const string AchievementBackgroundPathFormat =
            "Assets/Res/Resources/Textures/UI/AchievementUI/DT{0}.png";
        private static readonly Color FrameHiddenColor = new(0.75f, 0.75f, 0.75f, 1f);

        public static Sprite ResolveAchievementIcon(Achievement achievement)
        {
            if (achievement == null)
            {
                return null;
            }

            // 表已去掉 Category：图标只读配表 icon，缺省用默认图。
            return LoadConfiguredSprite(achievement.Icon, LoadConfiguredSprite(DefaultIconPath));
        }

        public static Sprite ResolveAchievementFrame(Achievement achievement)
        {
            var fallback = LoadConfiguredSprite(DefaultFramePath);
            if (achievement == null)
            {
                return fallback;
            }

            return LoadConfiguredSprite(achievement.Frame, fallback);
        }

        /// <summary>
        /// 按成就配表 level 加载城镇展示底图（AchievementUI/DT+level）。
        /// </summary>
        public static Sprite ResolveAchievementBackground(Achievement achievement)
        {
            if (achievement == null)
            {
                return null;
            }

            var level = Mathf.Clamp(achievement.Level, 1, 5);
            return LoadConfiguredSprite(string.Format(AchievementBackgroundPathFormat, level));
        }

        public static void ApplyAchievementBackground(Image background, Achievement achievement, bool completed = true)
        {
            if (background == null)
            {
                return;
            }

            var sprite = ResolveAchievementBackground(achievement);
            background.sprite = sprite;
            background.enabled = sprite != null;
            background.color = completed ? Color.white : FrameHiddenColor;
        }

        /// <summary>
        /// 按成就配表 frame 字段刷新边框（铜/银/金/翠等天赋档次边框图）。
        /// </summary>
        public static void ApplyAchievementFrame(Image frame, Achievement achievement, bool completed = true)
        {
            if (frame == null)
            {
                return;
            }

            var frameSprite = ResolveAchievementFrame(achievement);
            frame.sprite = frameSprite;
            frame.enabled = frameSprite != null;
            frame.color = completed ? Color.white : FrameHiddenColor;
        }

        /// <summary>
        /// 成就边框品质：0 铜 / 1 银 / 2 金 / 3 翠（由配表 frame 路径推断）。
        /// </summary>
        public static int GetAchievementQualityGrade(Achievement achievement)
        {
            if (achievement == null || string.IsNullOrWhiteSpace(achievement.Frame))
            {
                return 0;
            }

            var frame = achievement.Frame;
            if (frame.IndexOf("005cui", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 3;
            }

            if (frame.IndexOf("004jin", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 2;
            }

            if (frame.IndexOf("003yin", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 1;
            }

            return 0;
        }

        public static Color GetAchievementQualityColor(int grade)
        {
            return Mathf.Clamp(grade, 0, 3) switch
            {
                0 => new Color(0.75f, 0.45f, 0.22f, 1f),
                1 => new Color(0.55f, 0.60f, 0.70f, 1f),
                2 => new Color(0.90f, 0.68f, 0.12f, 1f),
                _ => new Color(0.18f, 0.68f, 0.42f, 1f)
            };
        }

        /// <summary>
        /// 按成就边框品质刷新称号文字颜色。
        /// </summary>
        public static void ApplyAchievementNameColor(TMP_Text text, Achievement achievement)
        {
            if (text == null)
            {
                return;
            }

            text.color = GetAchievementQualityColor(GetAchievementQualityGrade(achievement));
        }

        public static Sprite ResolveAchievementIcon(int achievementId)
        {
            return ResolveAchievementIcon(AchievementConfigUtility.Get(achievementId));
        }

        private static Sprite LoadConfiguredSprite(string path, Sprite fallback = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return fallback;
            }

            var normalizedPath = path.Trim().Replace('\\', '/');
            var sprite = HudOverlayAssetCatalog.LoadSprite(normalizedPath);
            return sprite != null ? sprite : fallback;
        }
    }
}
