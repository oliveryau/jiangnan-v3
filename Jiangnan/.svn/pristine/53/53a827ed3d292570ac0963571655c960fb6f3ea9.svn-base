using System.Collections.Generic;
using cfg;
using SimpleJSON;
using UnityEngine;

namespace JN.Client.Config
{
    /// <summary>
    /// 统一加载 Resources/Config 下的 Luban JSON 表。
    /// 优先复用 <see cref="JN.Client.Manager.LubanManager"/> 已初始化的 Tables。
    /// </summary>
    public static class LubanTablesRuntime
    {
        private const string ConfigFolder = "Config";

        private static Tables cachedTables;
        private static bool loadAttempted;

        /// <summary>
        /// 获取已加载的 Tables；失败时返回 null。
        /// </summary>
        public static Tables GetTables()
        {
            var manager = JN.Client.Manager.LubanManager.Instance;
            if (manager != null && manager.IsInitialized())
            {
                try
                {
                    return manager.GetTables();
                }
                catch (System.Exception exception)
                {
                    Debug.LogWarning($"[LubanTablesRuntime] LubanManager 取表失败：{exception.Message}");
                }
            }

            EnsureTablesLoaded();
            return cachedTables;
        }

        /// <summary>
        /// 按资源名加载 JSON（不含扩展名），供 Luban Tables 构造使用。
        /// </summary>
        public static JSONNode LoadJson(string tableFileName)
        {
            var path = $"{ConfigFolder}/{tableFileName}";
            var textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null || string.IsNullOrWhiteSpace(textAsset.text))
            {
                throw new System.InvalidOperationException($"未找到配置资源 {path}");
            }

            return JSON.Parse(textAsset.text);
        }

        /// <summary>
        /// 读取引导任务表。
        /// </summary>
        public static TbGuideTask GetGuideTaskTable()
        {
            return GetTables()?.TbGuideTask;
        }

        /// <summary>
        /// 按 Id 读取引导任务。
        /// </summary>
        public static GuideTask GetGuideTask(int id)
        {
            return GetGuideTaskTable()?.GetOrDefault(id);
        }

        /// <summary>
        /// 引导任务列表（可能为空列表）。
        /// </summary>
        public static IReadOnlyList<GuideTask> GetGuideTasks()
        {
            var table = GetGuideTaskTable();
            return table != null ? table.DataList : System.Array.Empty<GuideTask>();
        }

        /// <summary>
        /// 读取员工表。
        /// </summary>
        public static TbStaff GetStaffTable()
        {
            return GetTables()?.TbStaff;
        }

        /// <summary>
        /// 按 Id 读取员工。
        /// </summary>
        public static Staff GetStaff(int id)
        {
            return GetStaffTable()?.GetOrDefault(id);
        }

        /// <summary>
        /// 员工列表（可能为空列表）。
        /// </summary>
        public static IReadOnlyList<Staff> GetStaffList()
        {
            var table = GetStaffTable();
            return table != null ? table.DataList : System.Array.Empty<Staff>();
        }

        /// <summary>
        /// 按职位筛选员工。
        /// </summary>
        public static List<Staff> GetStaffByPosition(StaffPosition position)
        {
            var result = new List<Staff>();
            var table = GetStaffTable();
            if (table == null)
            {
                return result;
            }

            for (var index = 0; index < table.DataList.Count; index++)
            {
                var staff = table.DataList[index];
                if (staff != null && staff.Position == position)
                {
                    result.Add(staff);
                }
            }

            return result;
        }

        /// <summary>
        /// 招聘候选：从该职位员工池中随机抽取最多 maxCount 名。
        /// </summary>
        /// <param name="excludeStaffIds">已入职等需排除的 Id；可为 null。</param>
        public static List<Staff> GetHireCandidates(
            StaffPosition position,
            int maxCount = 3,
            ICollection<int> excludeStaffIds = null)
        {
            var pool = GetStaffByPosition(position);
            if (excludeStaffIds != null && excludeStaffIds.Count > 0)
            {
                for (var index = pool.Count - 1; index >= 0; index--)
                {
                    var staff = pool[index];
                    if (staff != null && excludeStaffIds.Contains(staff.Id))
                    {
                        pool.RemoveAt(index);
                    }
                }
            }

            if (pool.Count <= 0 || maxCount <= 0)
            {
                return new List<Staff>();
            }

            if (pool.Count <= maxCount)
            {
                ShuffleInPlace(pool);
                return pool;
            }

            // 部分 Fisher–Yates：只打乱前 maxCount 个并截取
            for (var index = 0; index < maxCount; index++)
            {
                var swap = UnityEngine.Random.Range(index, pool.Count);
                (pool[index], pool[swap]) = (pool[swap], pool[index]);
            }

            return pool.GetRange(0, maxCount);
        }

        private static void ShuffleInPlace(List<Staff> list)
        {
            for (var index = list.Count - 1; index > 0; index--)
            {
                var swap = UnityEngine.Random.Range(0, index + 1);
                (list[index], list[swap]) = (list[swap], list[index]);
            }
        }

        /// <summary>
        /// 读取设施表。
        /// </summary>
        public static TbFacility GetFacilityTable()
        {
            return GetTables()?.TbFacility;
        }

        /// <summary>
        /// 按 Id 读取设施。
        /// </summary>
        public static Facility GetFacility(int id)
        {
            return GetFacilityTable()?.GetOrDefault(id);
        }

        /// <summary>
        /// 读取酒馆科技树表。
        /// </summary>
        public static TbTavernTech GetTavernTechTable()
        {
            return GetTables()?.TbTavernTech;
        }

        /// <summary>
        /// 按 Id 读取科技。
        /// </summary>
        public static TavernTech GetTavernTech(int id)
        {
            return GetTavernTechTable()?.GetOrDefault(id);
        }

        /// <summary>
        /// 读取成就表。
        /// </summary>
        public static TbAchievement GetAchievementTable()
        {
            return GetTables()?.TbAchievement;
        }

        /// <summary>
        /// 按 Id 读取成就。
        /// </summary>
        public static Achievement GetAchievement(int id)
        {
            return GetAchievementTable()?.GetOrDefault(id);
        }

        /// <summary>
        /// 读取城镇他人建筑表。
        /// </summary>
        public static TbTownBuilding GetTownBuildingTable()
        {
            return GetTables()?.TbTownBuilding;
        }

        /// <summary>
        /// 按 Id 读取城镇他人建筑。
        /// </summary>
        public static TownBuilding GetTownBuilding(int id)
        {
            return GetTownBuildingTable()?.GetOrDefault(id);
        }

        /// <summary>
        /// 读取菜品表。
        /// </summary>
        public static TbDish GetDishTable()
        {
            return GetTables()?.TbDish;
        }

        /// <summary>
        /// 按 Id 读取菜品。
        /// </summary>
        public static Dish GetDish(int id)
        {
            return GetDishTable()?.GetOrDefault(id);
        }

        /// <summary>
        /// 菜品列表（可能为空列表）。
        /// </summary>
        public static IReadOnlyList<Dish> GetDishList()
        {
            var table = GetDishTable();
            return table != null ? table.DataList : System.Array.Empty<Dish>();
        }

        /// <summary>
        /// 读取贵客猜菜提示语表。
        /// </summary>
        public static TbVipGuestDemandHint GetVipGuestDemandHintTable()
        {
            return GetTables()?.TbVipGuestDemandHint;
        }

        /// <summary>
        /// 按 Id 读取贵客猜菜提示语。
        /// </summary>
        public static VipGuestDemandHint GetVipGuestDemandHint(int id)
        {
            return GetVipGuestDemandHintTable()?.GetOrDefault(id);
        }

        /// <summary>
        /// 贵客猜菜提示语列表（可能为空列表）。
        /// </summary>
        public static IReadOnlyList<VipGuestDemandHint> GetVipGuestDemandHintList()
        {
            var table = GetVipGuestDemandHintTable();
            return table != null ? table.DataList : System.Array.Empty<VipGuestDemandHint>();
        }

        /// <summary>
        /// 读取对话台词表。
        /// </summary>
        public static TbDialog GetDialogTable()
        {
            return GetTables()?.TbDialog;
        }

        /// <summary>
        /// 按行 Id 读取对话台词。
        /// </summary>
        public static Dialog GetDialog(int id)
        {
            return GetDialogTable()?.GetOrDefault(id);
        }

        /// <summary>
        /// 全部对话台词行（可能为空列表）。
        /// </summary>
        public static IReadOnlyList<Dialog> GetDialogList()
        {
            var table = GetDialogTable();
            return table != null ? table.DataList : System.Array.Empty<Dialog>();
        }

        /// <summary>
        /// 读取员工天赋表。
        /// </summary>
        public static TbStaffTalent GetStaffTalentTable()
        {
            return GetTables()?.TbStaffTalent;
        }

        /// <summary>
        /// 按 Id 读取员工天赋。
        /// </summary>
        public static StaffTalent GetStaffTalent(int id)
        {
            return GetStaffTalentTable()?.GetOrDefault(id);
        }

        private static void EnsureTablesLoaded()
        {
            if (loadAttempted)
            {
                return;
            }

            loadAttempted = true;
            try
            {
                cachedTables = new Tables(LoadJson);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"[LubanTablesRuntime] 读取配置表失败，将使用代码默认值。异常：{exception.Message}");
                cachedTables = null;
            }
        }
    }
}
