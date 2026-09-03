using System;
using System.Collections.Generic;
using cfg;

namespace JN.Client.Config
{
    /// <summary>
    /// TownBuilding 表读取：城镇他人店铺配置。
    /// </summary>
    public static class TownBuildingConfigUtility
    {
        public static TbTownBuilding GetTable()
        {
            return LubanTablesRuntime.GetTownBuildingTable();
        }

        public static TownBuilding Get(int id)
        {
            return id <= 0 ? null : GetTable()?.GetOrDefault(id);
        }

        public static IReadOnlyList<TownBuilding> GetAll()
        {
            var table = GetTable();
            return table != null ? table.DataList : Array.Empty<TownBuilding>();
        }

        public static TownBuilding GetByFieldId(int fieldId)
        {
            if (fieldId <= 0)
            {
                return null;
            }

            var list = GetAll();
            for (var index = 0; index < list.Count; index++)
            {
                var row = list[index];
                if (row != null && row.FieldId == fieldId)
                {
                    return row;
                }
            }

            return null;
        }

        /// <summary>
        /// 该地块对应 TownBuilding 是否标记为火爆（hotMark≠0）。
        /// </summary>
        public static bool IsHotByFieldId(int fieldId)
        {
            var row = GetByFieldId(fieldId);
            return row != null && row.HotMark != 0;
        }

        /// <summary>
        /// 读取他人店主头像 Id（1~8）；无配置或非法时返回 0。
        /// </summary>
        public static int GetHeadIconIdByFieldId(int fieldId)
        {
            var row = GetByFieldId(fieldId);
            if (row == null)
            {
                return 0;
            }

            return row.HeadIconId >= 1 && row.HeadIconId <= 8 ? row.HeadIconId : 0;
        }
    }
}
