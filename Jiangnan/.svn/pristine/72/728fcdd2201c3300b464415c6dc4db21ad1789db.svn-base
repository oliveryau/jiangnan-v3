using System.Collections.Generic;
using cfg;

namespace JN.Client.Config
{
    /// <summary>
    /// GuideTask.param / unlock 按 taskType 解读的辅助方法。
    /// </summary>
    public static class GuideTaskConfigUtility
    {
        /// <summary>
        /// 按任务类型查找第一条配置（表按 id 升序时即默认主线任务）。
        /// </summary>
        public static GuideTask FindByType(GuideTaskType taskType)
        {
            var tasks = LubanTablesRuntime.GetGuideTasks();
            for (var index = 0; index < tasks.Count; index++)
            {
                var task = tasks[index];
                if (task != null && task.TaskType == taskType)
                {
                    return task;
                }
            }

            return null;
        }

        /// <summary>
        /// 读取描述文案，表缺失时回退。
        /// </summary>
        public static string GetDesc(GuideTaskType taskType, string fallback)
        {
            var task = FindByType(taskType);
            return task != null && !string.IsNullOrWhiteSpace(task.Desc) ? task.Desc : fallback;
        }

        /// <summary>
        /// Buy*：param[0]=目标数量；Hire*：param[0]=目标数量，param[1]=员工Id。
        /// </summary>
        public static int GetTargetCount(GuideTask task, int defaultValue = 1)
        {
            if (task?.Param == null || task.Param.Count <= 0)
            {
                return defaultValue;
            }

            return task.Param[0];
        }

        /// <summary>
        /// 按类型读取目标数量。
        /// </summary>
        public static int GetTargetCount(GuideTaskType taskType, int defaultValue = 1)
        {
            return GetTargetCount(FindByType(taskType), defaultValue);
        }

        /// <summary>
        /// Hire*：param[1]=员工 Id；其他类型返回 0。
        /// </summary>
        public static int GetStaffId(GuideTask task)
        {
            if (task?.Param == null || task.Param.Count < 2)
            {
                return 0;
            }

            return IsHireTask(task.TaskType) ? task.Param[1] : 0;
        }

        /// <summary>
        /// 按类型读取关联员工 Id。
        /// </summary>
        public static int GetStaffId(GuideTaskType taskType, int defaultStaffId)
        {
            var fromTable = GetStaffId(FindByType(taskType));
            return fromTable > 0 ? fromTable : defaultStaffId;
        }

        /// <summary>
        /// unlock 为前置任务 Id 列表；空表示无前置。
        /// </summary>
        public static IReadOnlyList<int> GetUnlockPrevTaskIds(GuideTask task)
        {
            if (task?.Unlock == null)
            {
                return System.Array.Empty<int>();
            }

            return task.Unlock;
        }

        /// <summary>
        /// 招聘类任务由 taskType 暗示处于 Recruit 阶段。
        /// </summary>
        public static bool RequiresRecruitStage(GuideTask task)
        {
            return task != null && IsHireTask(task.TaskType);
        }

        public static bool IsHireTask(GuideTaskType taskType)
        {
            return taskType == GuideTaskType.HireShopkeeper
                   || taskType == GuideTaskType.HireChef
                   || taskType == GuideTaskType.HireWaiter;
        }

        public static bool IsBuyTask(GuideTaskType taskType)
        {
            return taskType == GuideTaskType.BuyBasicEquipment
                   || taskType == GuideTaskType.BuyTables
                   || taskType == GuideTaskType.BuyKitchenEquipment;
        }
    }
}
