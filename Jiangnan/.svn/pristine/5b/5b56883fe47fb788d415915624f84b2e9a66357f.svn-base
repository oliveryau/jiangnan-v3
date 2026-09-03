using cfg;
using JN.Client.Config;
using JN.Client.Model;

namespace JN.Client.Manager
{
    /// <summary>
    /// 员工成长提示（能力由科技树驱动，不再行为计数升级）。
    /// </summary>
    public static class StaffProgressionService
    {
        public static string BuildStaffTechHint(LocalStaffSaveData save, StaffPosition position)
        {
            if (save == null)
            {
                return "-";
            }

            return StaffConfigUtility.BuildNextStaffTechHint(position);
        }
    }
}
