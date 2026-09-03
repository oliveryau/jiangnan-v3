using System.Collections.Generic;
using cfg;

namespace JN.Client.Config
{
    /// <summary>
    /// 对话台词配置读取：按 dialogId 取组内有序行。
    /// </summary>
    public static class DialogConfigUtility
    {
        /// <summary>新建酒楼后首次进店。</summary>
        public const string DialogIdFirstEnter = "first_enter";

        /// <summary>招募员工成就任务出现。</summary>
        public const string DialogIdEmploy = "employ";

        /// <summary>当前任务为「升级酒楼」。</summary>
        public const string DialogIdUpdate = "update";

        /// <summary>当前任务为「新店开张」。</summary>
        public const string DialogIdOpening = "opening";

        /// <summary>当前任务为「外出揽客」。</summary>
        public const string DialogIdHireStaffEnter = "HireStaff_enter";

        /// <summary>首次上二楼后解锁菜单入口。</summary>
        public const string DialogIdUnlockMenu = "UnlockMenu";

        /// <summary>
        /// 立绘 Resources 路径格式（headPic 为键，如 fushang）。
        /// </summary>
        public const string HeadPicPathFormat =
            "Assets/Res/Resources/Textures/UI/Dialog/headPic/{0}.png";

        public static Dialog Get(int id)
        {
            return LubanTablesRuntime.GetDialog(id);
        }

        public static IReadOnlyList<Dialog> GetAll()
        {
            return LubanTablesRuntime.GetDialogList();
        }

        /// <summary>
        /// 按对话组 Id 取台词，按 order 升序。
        /// </summary>
        public static List<Dialog> GetLines(string dialogId)
        {
            var result = new List<Dialog>();
            if (string.IsNullOrWhiteSpace(dialogId))
            {
                return result;
            }

            var key = dialogId.Trim();
            var all = GetAll();
            for (var index = 0; index < all.Count; index++)
            {
                var line = all[index];
                if (line == null || string.IsNullOrWhiteSpace(line.DialogId))
                {
                    continue;
                }

                if (line.DialogId.Trim() != key)
                {
                    continue;
                }

                result.Add(line);
            }

            result.Sort((a, b) =>
            {
                var orderCompare = a.Order.CompareTo(b.Order);
                return orderCompare != 0 ? orderCompare : a.Id.CompareTo(b.Id);
            });
            return result;
        }

        /// <summary>
        /// 拼成立绘完整资源路径。
        /// </summary>
        public static string ResolveHeadPicPath(string headPicKey)
        {
            if (string.IsNullOrWhiteSpace(headPicKey))
            {
                return null;
            }

            return string.Format(HeadPicPathFormat, headPicKey.Trim());
        }
    }
}
