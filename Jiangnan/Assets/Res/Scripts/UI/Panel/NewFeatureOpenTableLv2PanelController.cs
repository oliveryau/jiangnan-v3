using UnityEngine.UI;

namespace JN.Client.UI
{
    /// <summary>
    /// 二级桌位解锁提示的数据载体。
    /// </summary>
    public class NewFeatureOpenTableLv2PanelControllerData : NewFeatureOpenToastPanelControllerData
    {
        /// <summary>可选：运行时替换顶部横幅图（为空则沿用 Prefab 默认）。</summary>
        public string TitleSpritePath;

        /// <summary>可选：运行时替换中部功能说明图（为空则沿用 Prefab 默认）。</summary>
        public string ContentSpritePath;
    }

    /// <summary>
    /// 功能解锁提示面板（如二级桌位解锁），沿用通用解锁 toast 逻辑。
    /// </summary>
    public class NewFeatureOpenTableLv2PanelController : NewFeatureOpenToastPanelController
    {
        protected override void OnPanelShow()
        {
            ApplyOptionalSprites();
            base.OnPanelShow();
        }

        private void ApplyOptionalSprites()
        {
            if (Data is not NewFeatureOpenTableLv2PanelControllerData unlockData)
            {
                return;
            }

            ApplySpriteIfProvided(unlockData.TitleSpritePath, "img_Text");
            ApplySpriteIfProvided(unlockData.ContentSpritePath, "img_Content");
        }

        private void ApplySpriteIfProvided(string spritePath, string nodeName)
        {
            if (string.IsNullOrWhiteSpace(spritePath))
            {
                return;
            }

            var sprite = HudOverlayAssetCatalog.LoadSprite(spritePath);
            if (sprite == null)
            {
                return;
            }

            var image = ResolveImage(nodeName, nodeName);
            if (image != null)
            {
                image.sprite = sprite;
                image.enabled = true;
            }
        }
    }
}
