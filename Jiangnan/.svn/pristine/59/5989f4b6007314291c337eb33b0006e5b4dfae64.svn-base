using System;
using System.Collections.Generic;

namespace JN.Client.Model
{
    /// <summary>
    /// 一次拉客行程内，他人酒楼某一桌的轻量快照（不落盘）。
    /// 客人类型与轿子队列一致：0 普 / 1 稀 / 2 贵。
    /// </summary>
    [Serializable]
    public class OtherTavernVisitTableSnapshot
    {
        public int tableId;
        public int runtimeState = (int)TavernTableRuntimeState.Idle;
        public List<int> guestKinds = new();
        public bool hasPulledTip;
        public bool pulledTipIsSelf;
        public string pullerName;
        public int headIconId;
    }

    /// <summary>
    /// 一次拉客行程内，按地块缓存的他人酒楼拜访快照。
    /// </summary>
    [Serializable]
    public class OtherTavernVisitSnapshot
    {
        public int tileId;
        public List<OtherTavernVisitTableSnapshot> tables = new();
    }
}
