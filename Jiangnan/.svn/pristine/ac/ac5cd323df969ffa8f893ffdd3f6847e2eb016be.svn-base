using System.Collections;
using JN.Client.Config;
using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 小二角色组件，承接任务与状态机上下文。
    /// </summary>
    public sealed class WaiterCharacter : TavernCharacterBase<WaiterCharacter, WaiterTask>
    {
        private IWaiterRuntimeHost host;

        internal override CharacterRuntimeTag RuntimeTag => CharacterRuntimeTag.Waiter;

        internal IWaiterRuntimeHost Host => host;

        /// <summary>
        /// 绑定的 Staff 配表 / 存档 Id。
        /// </summary>
        public int StaffId { get; private set; }

        internal int HomeIndex { get; set; }

        internal GameObject PendingDishPrefab { get; set; }

        /// <summary>
        /// 上菜途中挂在小二手上的餐盘+菜表现根节点。
        /// </summary>
        internal GameObject PendingDishVisual { get; set; }

        internal void InitializeWaiter(TavernSceneManager owner, IWaiterRuntimeHost waiterHost)
        {
            InitializeOwner(owner);
            host = waiterHost;
        }

        public void BindStaffId(int staffId)
        {
            StaffId = Mathf.Max(0, staffId);
        }

        public StaffRuntimeProfile GetRuntimeProfile()
        {
            return StaffId > 0 ? StaffConfigUtility.GetProfile(StaffId) : null;
        }

        /// <summary>
        /// 小二状态协程代数：递增后旧协程在下一 yield 点自行退出，避免 StopCoroutine continue failure。
        /// </summary>
        internal int RoutineEpoch { get; private set; }

        internal int BeginNewRoutineEpoch()
        {
            RoutineEpoch++;
            return RoutineEpoch;
        }

        internal bool IsRoutineEpochCurrent(int epoch)
        {
            return RoutineEpoch == epoch;
        }

        protected override void ApplyStateKey(WaiterCharacter actor, string stateKey)
        {
            host?.ApplyWaiterPresentation(gameObject, stateKey);
        }

        protected override void StartStateRoutine(WaiterCharacter actor, IEnumerator routine)
        {
            host?.StartWaiterStateRoutine(actor, routine);
        }
    }
}
