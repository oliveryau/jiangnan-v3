using UnityEngine;

namespace JN.Client.Config
{
    /// <summary>
    /// 解析 Dish 表图标：优先配置路径，其次 SO_Product 同名，最后占位图。
    /// </summary>
    public static class DishIconResolver
    {
        private const string PlaceholderResourcePath = "Textures/UI/Icons 1/dingDan";

        public static Sprite TryResolve(string dishName, string iconResourcePath)
        {
            if (!string.IsNullOrWhiteSpace(iconResourcePath))
            {
                var configured = LoadSprite(iconResourcePath);
                if (configured != null)
                {
                    return configured;
                }
            }

            if (!string.IsNullOrWhiteSpace(dishName))
            {
                var products = SO_Product.GetAll();
                for (var index = 0; index < products.Count; index++)
                {
                    var product = products[index];
                    if (product != null
                        && product.icon != null
                        && string.Equals(product.displayName, dishName, System.StringComparison.Ordinal))
                    {
                        return product.icon;
                    }
                }
            }

            return LoadSprite(PlaceholderResourcePath);
        }

        private static Sprite LoadSprite(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return null;
            }

            var normalized = resourcePath.Replace('\\', '/').Trim();
            if (normalized.StartsWith("Assets/Res/Resources/", System.StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized["Assets/Res/Resources/".Length..];
            }

            if (normalized.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)
                || normalized.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized[..normalized.LastIndexOf('.')];
            }

            return Resources.Load<Sprite>(normalized);
        }
    }
}
