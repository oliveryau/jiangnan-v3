using JN.Client.Model;
using UnityEngine;

namespace JN.Client.Manager
{
    /// <summary>
    /// 自家酒楼运行时快照：Save / Clear / 查询；拜访会话不读写。
    /// </summary>
    public partial class DataManager
    {
        /// <summary>拜访前备份的桌 runtimeState，结束拜访时若无有效快照则回写。</summary>
        private int[] visitTableRuntimeStateBackup;

        /// <summary>
        /// 是否有可恢复的自家营业运行时快照。
        /// </summary>
        public bool HasValidTavernRuntimeSnapshot()
        {
            EnsureTavernDefaults();
            var snapshot = SaveData.tavern.runtimeSnapshot;
            return snapshot != null && snapshot.valid;
        }

        /// <summary>
        /// 读取当前快照（可能为 null 或 invalid）。
        /// </summary>
        public TavernRuntimeSnapshotSaveData GetTavernRuntimeSnapshot()
        {
            EnsureTavernDefaults();
            return SaveData.tavern.runtimeSnapshot;
        }

        /// <summary>
        /// 写入自家营业运行时快照并落盘。拜访会话中禁止调用侧应自行 return。
        /// </summary>
        public void SaveTavernRuntimeSnapshot(TavernRuntimeSnapshotSaveData snapshot)
        {
            EnsureTavernDefaults();
            // 拜访中禁止落盘（含静态会话标记，防止 Instance 重建后误写）。
            if (IsInOtherTavernVisitSession || IsVisitingOtherTavern)
            {
                return;
            }

            if (snapshot == null)
            {
                ClearTavernRuntimeSnapshot(save: true);
                return;
            }

            snapshot.tables ??= new System.Collections.Generic.List<TavernTablePhaseSnapshot>();
            snapshot.frontOrders ??= new System.Collections.Generic.List<TavernFrontOrderSnapshot>();
            snapshot.unseatedQueue ??= new System.Collections.Generic.List<TavernUnseatedCustomerSnapshot>();
            SaveData.tavern.runtimeSnapshot = snapshot;
            SaveGame();
        }

        /// <summary>
        /// 清除运行时快照（正式打烊 / 开业重置时调用）。
        /// </summary>
        public void ClearTavernRuntimeSnapshot(bool save = true)
        {
            EnsureTavernDefaults();
            SaveData.tavern.runtimeSnapshot = null;
            if (save)
            {
                SaveGame();
            }
        }

        /// <summary>
        /// 拜访开始：备份当前桌 runtimeState，避免拜访模拟污染后无法还原。
        /// </summary>
        internal void BackupTableRuntimeStatesForVisit()
        {
            EnsureTavernDefaults();
            var tables = SaveData.tavern.tables;
            if (tables == null || tables.Count <= 0)
            {
                visitTableRuntimeStateBackup = null;
                return;
            }

            var maxId = 0;
            for (var index = 0; index < tables.Count; index++)
            {
                if (tables[index] != null)
                {
                    maxId = Mathf.Max(maxId, tables[index].tableId);
                }
            }

            visitTableRuntimeStateBackup = new int[maxId + 1];
            for (var index = 0; index < visitTableRuntimeStateBackup.Length; index++)
            {
                visitTableRuntimeStateBackup[index] = -1;
            }

            for (var index = 0; index < tables.Count; index++)
            {
                var table = tables[index];
                if (table == null || table.tableId <= 0 || table.tableId >= visitTableRuntimeStateBackup.Length)
                {
                    continue;
                }

                visitTableRuntimeStateBackup[table.tableId] = table.runtimeState;
            }
        }

        /// <summary>
        /// 拜访结束：优先用快照桌态还原，否则用拜访前备份，避免污染自家桌态落盘。
        /// </summary>
        internal void RestoreTableRuntimeStatesAfterVisit()
        {
            EnsureTavernDefaults();
            var tables = SaveData.tavern.tables;
            if (tables == null)
            {
                visitTableRuntimeStateBackup = null;
                return;
            }

            var snapshot = SaveData.tavern.runtimeSnapshot;
            if (snapshot != null && snapshot.valid && snapshot.tables != null && snapshot.tables.Count > 0)
            {
                for (var index = 0; index < snapshot.tables.Count; index++)
                {
                    var phase = snapshot.tables[index];
                    if (phase == null)
                    {
                        continue;
                    }

                    var tableData = GetTableData(phase.tableId);
                    if (tableData == null)
                    {
                        continue;
                    }

                    tableData.runtimeState = phase.runtimeState;
                }

                // 快照未覆盖的桌：若有备份则回写，避免拜访中改过的空桌残留。
                if (visitTableRuntimeStateBackup != null)
                {
                    for (var index = 0; index < tables.Count; index++)
                    {
                        var table = tables[index];
                        if (table == null || table.tableId <= 0)
                        {
                            continue;
                        }

                        var covered = false;
                        for (var snapIndex = 0; snapIndex < snapshot.tables.Count; snapIndex++)
                        {
                            if (snapshot.tables[snapIndex] != null
                                && snapshot.tables[snapIndex].tableId == table.tableId)
                            {
                                covered = true;
                                break;
                            }
                        }

                        if (covered || table.tableId >= visitTableRuntimeStateBackup.Length)
                        {
                            continue;
                        }

                        var backed = visitTableRuntimeStateBackup[table.tableId];
                        if (backed >= 0)
                        {
                            table.runtimeState = backed;
                        }
                    }
                }
            }
            else if (visitTableRuntimeStateBackup != null)
            {
                for (var index = 0; index < tables.Count; index++)
                {
                    var table = tables[index];
                    if (table == null
                        || table.tableId <= 0
                        || table.tableId >= visitTableRuntimeStateBackup.Length)
                    {
                        continue;
                    }

                    var backed = visitTableRuntimeStateBackup[table.tableId];
                    if (backed >= 0)
                    {
                        table.runtimeState = backed;
                    }
                }
            }

            visitTableRuntimeStateBackup = null;
        }
    }
}
