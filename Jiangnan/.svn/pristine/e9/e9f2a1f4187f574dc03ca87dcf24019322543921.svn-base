using System;
using System.Collections.Generic;
using JN.Client.Manager;
using JN.Client.Model;
using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 统一处理顾客与桌位、排队站位之间的分配逻辑。
    /// </summary>
    internal readonly struct TavernQueueTarget
    {
        public TavernQueueTarget(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
    }

    internal sealed class TavernCustomerPlacementService
    {
        public void UpdateQueuePositions(
            List<TavernCustomerRuntimeController> queuedCustomers,
            Func<int, TavernQueueTarget> queueTargetResolver)
        {
            if (queuedCustomers == null || queueTargetResolver == null)
            {
                return;
            }

            for (var index = 0; index < queuedCustomers.Count; index++)
            {
                var customer = queuedCustomers[index];
                if (customer == null || customer.IsLeavingTavern)
                {
                    continue;
                }

                customer.MoveToQueue(queueTargetResolver(index));
            }
        }

        public bool TryAssignSingleCustomerToFreeTable(
            Dictionary<int, TableArea> allTables,
            HashSet<int> pendingUpgradeTableIds,
            TavernCustomerRuntimeController customer,
            Func<TableArea, int, (bool success, Vector3 position)> approachResolver,
            Func<int, bool> extraTableBlockResolver,
            TavernTableStateService tableStateService,
            TavernCustomerFlowService customerFlowService,
            Dictionary<int, TavernCustomerRuntimeController> tableCustomers,
            Dictionary<int, List<TavernCustomerRuntimeController>> tableCustomerGroups)
        {
            if (customer == null || allTables == null || approachResolver == null)
            {
                return false;
            }

            foreach (var tablePair in allTables)
            {
                var tableData = DataManager.Instance.GetTableData(tablePair.Key);
                if (tableData == null
                    || !tableData.isUnlocked
                    || (TavernTableRuntimeState)tableData.runtimeState != TavernTableRuntimeState.Idle
                    || pendingUpgradeTableIds.Contains(tablePair.Key)
                    || extraTableBlockResolver != null && extraTableBlockResolver(tablePair.Key)
                    || tablePair.Value == null)
                {
                    continue;
                }

                var approach = approachResolver(tablePair.Value, 0);
                if (!approach.success)
                {
                    continue;
                }

                tableStateService.SetReserved(tablePair.Key, tablePair.Value, dispatchRuntimeChanged: false);
                customerFlowService.RegisterTableGroup(
                    tableCustomers,
                    tableCustomerGroups,
                    tablePair.Key,
                    new List<TavernCustomerRuntimeController> { customer });
                customer.AssignToTable(tablePair.Key, approach.position, 0);
                return true;
            }

            return false;
        }

        public bool TryAssignSpawnedGroupToFreeTable(
            Dictionary<int, TableArea> allTables,
            HashSet<int> pendingUpgradeTableIds,
            List<TavernCustomerRuntimeController> spawnedCustomers,
            int requiredSeatCapacity,
            Func<TableArea, int, (bool success, Vector3 position)> approachResolver,
            Func<int, bool> extraTableBlockResolver,
            TavernTableStateService tableStateService,
            TavernCustomerFlowService customerFlowService,
            Dictionary<int, TavernCustomerRuntimeController> tableCustomers,
            Dictionary<int, List<TavernCustomerRuntimeController>> tableCustomerGroups)
        {
            if (spawnedCustomers == null
                || spawnedCustomers.Count == 0
                || allTables == null
                || approachResolver == null)
            {
                return false;
            }

            foreach (var tablePair in allTables)
            {
                var tableData = DataManager.Instance.GetTableData(tablePair.Key);
                if (tableData == null
                    || !tableData.isUnlocked
                    || (TavernTableRuntimeState)tableData.runtimeState != TavernTableRuntimeState.Idle
                    || pendingUpgradeTableIds.Contains(tablePair.Key)
                    || extraTableBlockResolver != null && extraTableBlockResolver(tablePair.Key)
                    || tablePair.Value == null
                    || tablePair.Value.GetSeatCapacity() < requiredSeatCapacity)
                {
                    continue;
                }

                var assignedCustomers = new List<TavernCustomerRuntimeController>(requiredSeatCapacity);
                for (var seatIndex = 0; seatIndex < requiredSeatCapacity; seatIndex++)
                {
                    var customer = spawnedCustomers[seatIndex];
                    var approach = approachResolver(tablePair.Value, seatIndex);
                    if (customer == null || !approach.success)
                    {
                        return false;
                    }

                    customer.AssignToTable(tablePair.Key, approach.position, seatIndex);
                    assignedCustomers.Add(customer);
                }

                tableStateService.SetReserved(tablePair.Key, tablePair.Value, dispatchRuntimeChanged: false);
                customerFlowService.RegisterTableGroup(
                    tableCustomers,
                    tableCustomerGroups,
                    tablePair.Key,
                    assignedCustomers);
                return true;
            }

            return false;
        }

        public bool TryAssignQueuedGroupToFreeTable(
            List<TavernCustomerRuntimeController> queuedCustomers,
            int[] preferredGroupSizes,
            Dictionary<int, TableArea> allTables,
            HashSet<int> pendingUpgradeTableIds,
            Func<TableArea, int, (bool success, Vector3 position)> approachResolver,
            Func<int, bool> extraTableBlockResolver,
            TavernTableStateService tableStateService,
            TavernCustomerFlowService customerFlowService,
            Dictionary<int, TavernCustomerRuntimeController> tableCustomers,
            Dictionary<int, List<TavernCustomerRuntimeController>> tableCustomerGroups)
        {
            if (queuedCustomers == null
                || preferredGroupSizes == null
                || allTables == null
                || approachResolver == null)
            {
                return false;
            }

            for (var preferredIndex = 0; preferredIndex < preferredGroupSizes.Length; preferredIndex++)
            {
                var expectedGroupSize = preferredGroupSizes[preferredIndex];
                if (queuedCustomers.Count < expectedGroupSize)
                {
                    continue;
                }

                foreach (var tablePair in allTables)
                {
                    var tableData = DataManager.Instance.GetTableData(tablePair.Key);
                    if (tableData == null
                        || !tableData.isUnlocked
                        || (TavernTableRuntimeState)tableData.runtimeState != TavernTableRuntimeState.Idle
                        || pendingUpgradeTableIds.Contains(tablePair.Key)
                        || extraTableBlockResolver != null && extraTableBlockResolver(tablePair.Key)
                        || tablePair.Value == null)
                    {
                        continue;
                    }

                    var seatCapacity = tablePair.Value.GetSeatCapacity();
                    if (seatCapacity < expectedGroupSize)
                    {
                        continue;
                    }

                    // 先校验全部座位接近点，避免半分配后 TableId 残留导致仍排队却不计耐心。
                    var approachPositions = new Vector3[expectedGroupSize];
                    var canAssign = true;
                    for (var seatIndex = 0; seatIndex < expectedGroupSize; seatIndex++)
                    {
                        if (queuedCustomers[seatIndex] == null)
                        {
                            canAssign = false;
                            break;
                        }

                        var approach = approachResolver(tablePair.Value, seatIndex);
                        if (!approach.success)
                        {
                            canAssign = false;
                            break;
                        }

                        approachPositions[seatIndex] = approach.position;
                    }

                    if (!canAssign)
                    {
                        continue;
                    }

                    var assignedCustomers = new List<TavernCustomerRuntimeController>(expectedGroupSize);
                    for (var seatIndex = 0; seatIndex < expectedGroupSize; seatIndex++)
                    {
                        var customer = queuedCustomers[seatIndex];
                        customer.AssignToTable(tablePair.Key, approachPositions[seatIndex], seatIndex);
                        assignedCustomers.Add(customer);
                    }

                    customerFlowService.DequeueLeadingGroup(queuedCustomers, expectedGroupSize);
                    tableStateService.SetReserved(tablePair.Key, tablePair.Value, dispatchRuntimeChanged: false);
                    customerFlowService.RegisterTableGroup(
                        tableCustomers,
                        tableCustomerGroups,
                        tablePair.Key,
                        assignedCustomers);
                    return true;
                }
            }

            return false;
        }

        public bool TryAssignSpawnedGroupToTable(
            int tableId,
            TableArea table,
            HashSet<int> pendingUpgradeTableIds,
            List<TavernCustomerRuntimeController> spawnedCustomers,
            int groupSize,
            Func<TableArea, int, (bool success, Vector3 position)> approachResolver,
            TavernTableStateService tableStateService,
            TavernCustomerFlowService customerFlowService,
            Dictionary<int, TavernCustomerRuntimeController> tableCustomers,
            Dictionary<int, List<TavernCustomerRuntimeController>> tableCustomerGroups)
        {
            if (table == null
                || spawnedCustomers == null
                || spawnedCustomers.Count < groupSize
                || groupSize <= 0
                || approachResolver == null
                || pendingUpgradeTableIds != null && pendingUpgradeTableIds.Contains(tableId))
            {
                return false;
            }

            var tableData = DataManager.Instance.GetTableData(tableId);
            if (tableData == null
                || !tableData.isUnlocked
                || (TavernTableRuntimeState)tableData.runtimeState != TavernTableRuntimeState.Idle
                || table.GetSeatCapacity() < groupSize)
            {
                return false;
            }

            var assignedCustomers = new List<TavernCustomerRuntimeController>(groupSize);
            for (var seatIndex = 0; seatIndex < groupSize; seatIndex++)
            {
                var customer = spawnedCustomers[seatIndex];
                var approach = approachResolver(table, seatIndex);
                if (customer == null || !approach.success)
                {
                    return false;
                }

                customer.AssignToTable(tableId, approach.position, seatIndex);
                assignedCustomers.Add(customer);
            }

            tableStateService.SetReserved(tableId, table, dispatchRuntimeChanged: false);
            customerFlowService.RegisterTableGroup(
                tableCustomers,
                tableCustomerGroups,
                tableId,
                assignedCustomers);
            return true;
        }
    }
}
