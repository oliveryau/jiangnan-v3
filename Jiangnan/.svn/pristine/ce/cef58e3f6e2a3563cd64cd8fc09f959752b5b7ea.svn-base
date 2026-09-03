using System;
using System.Collections.Generic;
using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 统一处理顾客运行时对象的创建与失败回滚。
    /// 贵客扩展点：研究「拉客贵客」后可由 <see cref="VipCustomerService"/> 介入刷客流程。
    /// </summary>
    internal sealed class TavernCustomerSpawnService
    {
        public TavernCustomerRuntimeController SpawnSingleCustomer(
            TavernSceneManager owner,
            List<GameObject> customerTemplates,
            Transform customerEntryPoint,
            Vector3 spawnPosition,
            Func<Vector3, Vector3> exitPositionResolver,
            Action<GameObject, Vector3> prepareSpawnedCustomer,
            TavernCustomerFlowService customerFlowService,
            List<TavernCustomerRuntimeController> activeCustomers)
        {
            if (owner == null
                || customerTemplates == null
                || customerTemplates.Count == 0
                || customerEntryPoint == null
                || exitPositionResolver == null
                || prepareSpawnedCustomer == null)
            {
                return null;
            }

            var template = customerTemplates[UnityEngine.Random.Range(0, customerTemplates.Count)];
            var customerObj = UnityEngine.Object.Instantiate(template, spawnPosition, customerEntryPoint.rotation);
            customerObj.name = $"{template.name}_Runtime";

            prepareSpawnedCustomer(customerObj, spawnPosition);

            var runtimeController = customerObj.GetComponent<TavernCustomerRuntimeController>();
            if (runtimeController == null)
            {
                runtimeController = customerObj.AddComponent<TavernCustomerRuntimeController>();
            }

            runtimeController.Initialize(owner, spawnPosition, exitPositionResolver(spawnPosition));
            customerFlowService.RegisterActiveCustomer(activeCustomers, runtimeController);
            return runtimeController;
        }

        public List<TavernCustomerRuntimeController> SpawnCustomerGroup(
            int groupSize,
            Vector3 baseSpawnPosition,
            Func<Vector3, int, int, Vector3> groupSpawnResolver,
            TavernSceneManager owner,
            List<GameObject> customerTemplates,
            Transform customerEntryPoint,
            Func<Vector3, Vector3> exitPositionResolver,
            Action<GameObject, Vector3> prepareSpawnedCustomer,
            TavernCustomerFlowService customerFlowService,
            List<TavernCustomerRuntimeController> activeCustomers)
        {
            if (groupSize <= 0 || groupSpawnResolver == null)
            {
                return null;
            }

            var spawnedCustomers = new List<TavernCustomerRuntimeController>(groupSize);
            for (var memberIndex = 0; memberIndex < groupSize; memberIndex++)
            {
                var runtimeController = SpawnSingleCustomer(
                    owner,
                    customerTemplates,
                    customerEntryPoint,
                    groupSpawnResolver(baseSpawnPosition, memberIndex, groupSize),
                    exitPositionResolver,
                    prepareSpawnedCustomer,
                    customerFlowService,
                    activeCustomers);

                if (runtimeController == null)
                {
                    RollbackSpawnedCustomers(spawnedCustomers, activeCustomers);
                    return null;
                }

                spawnedCustomers.Add(runtimeController);
            }

            return spawnedCustomers;
        }

        public void RollbackSpawnedCustomers(
            List<TavernCustomerRuntimeController> spawnedCustomers,
            List<TavernCustomerRuntimeController> activeCustomers)
        {
            if (spawnedCustomers == null)
            {
                return;
            }

            for (var index = 0; index < spawnedCustomers.Count; index++)
            {
                var customer = spawnedCustomers[index];
                if (customer == null)
                {
                    continue;
                }

                activeCustomers?.Remove(customer);
                UnityEngine.Object.Destroy(customer.gameObject);
            }
        }
    }
}
