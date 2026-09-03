using System.Collections.Generic;
using JN.Client.Manager;
using UnityEngine;
/// <summary>
/// 负责菜品配置相关的运行时逻辑。
/// </summary>
[CreateAssetMenu(menuName = "SongSim/Product", fileName = "SO_Product")]
public class SO_Product : ScriptableObject
{
    private const string ResourceFolder = "Products";

    [Header("ID / Name / Icon")]
    public int productId;
    public string displayName;
    public Sprite icon;

    [Header("Food Type")]
    public FoodCategory foodCategory;

    [Header("Economy")]
    public int basePrice;
    public int baseCost;

    [Header("Required Equipment")]
    public SO_Equipment requiredEquipment;
    public int requiredEquipmentLevel = 1;

    [Header("Production")]
    public float baseConsumeTime = 5f;

    [Tooltip("0=默认解锁；否则需研究对应科技 Id")]
    public int unlockTechId;

    [Header("Cleaning")]
    public float cleanTime = 3f;

    [Header("IF Staff Production")]
    public float productionTime = 5f;

    /// <summary>
    /// 获取当前配置表中的全部数据。
    /// </summary>
    public static IReadOnlyList<SO_Product> GetAll()
    {
        return GameplayResourceStore.LoadAll<SO_Product>(ResourceFolder);
    }

    /// <summary>
    /// 按配置编号查找对应数据。
    /// </summary>
    public static SO_Product GetById(int productId)
    {
        return GameplayResourceStore.Find<SO_Product>(ResourceFolder, product => product.productId == productId);
    }

    /// <summary>
    /// 按已研究科技过滤可点菜品。
    /// </summary>
    public static IReadOnlyList<SO_Product> GetUnlockedForResearchedTech()
    {
        var dataManager = DataManager.Instance;
        return GetUnlockedForResearchedTech(dataManager?.ResearchedTechIds);
    }

    public static IReadOnlyList<SO_Product> GetUnlockedForResearchedTech(IEnumerable<int> researchedTechIds)
    {
        var products = GetAll();
        if (products == null || products.Count == 0)
        {
            return System.Array.Empty<SO_Product>();
        }

        var researched = researchedTechIds != null ? new HashSet<int>(researchedTechIds) : null;
        var filtered = new List<SO_Product>();
        SO_Product fallback = null;
        for (var index = 0; index < products.Count; index++)
        {
            var product = products[index];
            if (product == null)
            {
                continue;
            }

            if (IsUnlocked(product, researched))
            {
                filtered.Add(product);
            }

            if (product.unlockTechId <= 0)
            {
                fallback ??= product;
            }
        }

        if (filtered.Count == 0 && fallback != null)
        {
            filtered.Add(fallback);
        }

        return filtered;
    }

    /// <summary>
    /// 兼容旧调用。
    /// </summary>
    public static IReadOnlyList<SO_Product> GetUnlockedForCurrentChefLevel()
    {
        return GetUnlockedForResearchedTech();
    }

    private static bool IsUnlocked(SO_Product product, HashSet<int> researchedTechIds)
    {
        if (product.unlockTechId <= 0)
        {
            return true;
        }

        return researchedTechIds != null && researchedTechIds.Contains(product.unlockTechId);
    }
}
public enum FoodCategory
{
    /// <summary>
    /// 处理当前属性相关逻辑。
    /// </summary>
    MainMeal = 0,

    /// <summary>
    /// 处理当前属性相关逻辑。
    /// </summary>
    SideMeal = 1
}
