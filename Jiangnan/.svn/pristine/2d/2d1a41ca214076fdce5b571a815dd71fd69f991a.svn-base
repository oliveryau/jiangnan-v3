using System.Collections.Generic;
using cfg;

namespace JN.Client.Config
{
    /// <summary>
    /// 贵客猜菜提示语配置读取。
    /// </summary>
    public static class VipGuestDemandHintConfigUtility
    {
        public static VipGuestDemandHint Get(int id)
        {
            return LubanTablesRuntime.GetVipGuestDemandHint(id);
        }

        public static IReadOnlyList<VipGuestDemandHint> GetAll()
        {
            return LubanTablesRuntime.GetVipGuestDemandHintList();
        }

        public static VipGuestDemandHint GetRandom()
        {
            var list = GetAll();
            if (list == null || list.Count == 0)
            {
                return null;
            }

            return list[UnityEngine.Random.Range(0, list.Count)];
        }
    }
}
