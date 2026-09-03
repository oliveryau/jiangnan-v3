using System;
using System.Collections.Generic;
using UnityEngine;

namespace JN.Client.Model
{
    /// <summary>
    /// 他人酒楼拉客剩余次数（按地块独立）。
    /// </summary>
    [Serializable]
    public class OtherTavernPullSaveEntry
    {
        public int tileId;
        /// <summary>剩余可拉次数；&lt;0 表示未初始化，读取时按 Config 上限补齐。</summary>
        public int remainingCount = -1;
    }
}
