using System.Collections.Generic;
using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 统一维护酒楼顾客在活跃列表、排队列表和桌位分组中的登记状态。
    /// </summary>
    internal sealed class TavernCustomerFlowService
    {
        public void RegisterActiveCustomer(List<TavernCustomerRuntimeController> activeCustomers, TavernCustomerRuntimeController customer)
        {
            if (activeCustomers == null || customer == null || activeCustomers.Contains(customer))
            {
                return;
            }

            activeCustomers.Add(customer);
        }

        public void EnqueueCustomer(List<TavernCustomerRuntimeController> queuedCustomers, TavernCustomerRuntimeController customer)
        {
            if (queuedCustomers == null || customer == null || queuedCustomers.Contains(customer))
            {
                return;
            }

            queuedCustomers.Add(customer);
        }

        public void DequeueLeadingGroup(List<TavernCustomerRuntimeController> queuedCustomers, int groupSize)
        {
            if (queuedCustomers == null || groupSize <= 0)
            {
                return;
            }

            var removeCount = Mathf.Min(groupSize, queuedCustomers.Count);
            if (removeCount > 0)
            {
                queuedCustomers.RemoveRange(0, removeCount);
            }
        }

        public void RegisterTableGroup(
            Dictionary<int, TavernCustomerRuntimeController> tableCustomers,
            Dictionary<int, List<TavernCustomerRuntimeController>> tableCustomerGroups,
            int tableId,
            List<TavernCustomerRuntimeController> customers)
        {
            if (tableCustomers == null || tableCustomerGroups == null || customers == null || customers.Count == 0)
            {
                return;
            }

            customers.RemoveAll(item => item == null);
            if (customers.Count == 0)
            {
                tableCustomers.Remove(tableId);
                tableCustomerGroups.Remove(tableId);
                return;
            }

            tableCustomerGroups[tableId] = customers;
            tableCustomers[tableId] = customers[0];
        }

        public void ClearTableAssignments(
            Dictionary<int, TavernCustomerRuntimeController> tableCustomers,
            Dictionary<int, List<TavernCustomerRuntimeController>> tableCustomerGroups,
            int tableId)
        {
            tableCustomers?.Remove(tableId);
            tableCustomerGroups?.Remove(tableId);
        }

        public void ClearTrackingForClosing(
            List<TavernCustomerRuntimeController> queuedCustomers,
            Dictionary<int, TavernCustomerRuntimeController> tableCustomers,
            Dictionary<int, List<TavernCustomerRuntimeController>> tableCustomerGroups)
        {
            queuedCustomers?.Clear();
            tableCustomers?.Clear();
            tableCustomerGroups?.Clear();
        }

        public void HandleCustomerExited(
            TavernCustomerRuntimeController customer,
            List<TavernCustomerRuntimeController> activeCustomers,
            List<TavernCustomerRuntimeController> queuedCustomers,
            Dictionary<int, TavernCustomerRuntimeController> tableCustomers,
            Dictionary<int, List<TavernCustomerRuntimeController>> tableCustomerGroups)
        {
            if (customer != null && customer.TableId > 0)
            {
                RemoveCustomerFromTableAssignments(customer, tableCustomers, tableCustomerGroups);
            }

            activeCustomers?.Remove(customer);
            queuedCustomers?.Remove(customer);
        }

        public bool TryGetTableCustomerGroup(
            Dictionary<int, List<TavernCustomerRuntimeController>> tableCustomerGroups,
            int tableId,
            out List<TavernCustomerRuntimeController> customers)
        {
            if (tableCustomerGroups != null
                && tableCustomerGroups.TryGetValue(tableId, out customers)
                && customers != null)
            {
                customers.RemoveAll(item => item == null);
                return true;
            }

            customers = null;
            return false;
        }

        private void RemoveCustomerFromTableAssignments(
            TavernCustomerRuntimeController customer,
            Dictionary<int, TavernCustomerRuntimeController> tableCustomers,
            Dictionary<int, List<TavernCustomerRuntimeController>> tableCustomerGroups)
        {
            if (customer == null || tableCustomers == null || tableCustomerGroups == null)
            {
                return;
            }

            if (TryGetTableCustomerGroup(tableCustomerGroups, customer.TableId, out var customers))
            {
                customers.Remove(customer);
                customers.RemoveAll(item => item == null);
                if (customers.Count == 0)
                {
                    tableCustomerGroups.Remove(customer.TableId);
                }
                else
                {
                    tableCustomers[customer.TableId] = customers[0];
                }
            }

            if (tableCustomers.TryGetValue(customer.TableId, out var currentCustomer) && currentCustomer == customer)
            {
                tableCustomers.Remove(customer.TableId);
            }
        }
    }
}
