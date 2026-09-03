using System.Collections;
using JN.Client.Config;
using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 厨师角色组件，统一承接厨师状态与任务。
    /// </summary>
    public sealed class ChefCharacter : TavernCharacterBase<ChefCharacter, ChefTask>
    {
        private IChefRuntimeHost host;

        internal override CharacterRuntimeTag RuntimeTag => CharacterRuntimeTag.Chef;

        internal IChefRuntimeHost Host => host;

        /// <summary>
        /// 绑定的 Staff 配表 / 存档 Id。
        /// </summary>
        public int StaffId { get; private set; }

        internal void InitializeChef(TavernSceneManager owner, IChefRuntimeHost chefHost)
        {
            InitializeOwner(owner);
            host = chefHost;
        }

        public void BindStaffId(int staffId)
        {
            StaffId = Mathf.Max(0, staffId);
        }

        public StaffRuntimeProfile GetRuntimeProfile()
        {
            return StaffId > 0 ? StaffConfigUtility.GetProfile(StaffId) : null;
        }

        protected override void ApplyStateKey(ChefCharacter actor, string stateKey)
        {
            host?.ApplyChefPresentation(gameObject, stateKey);
        }

        protected override void StartStateRoutine(ChefCharacter actor, IEnumerator routine)
        {
            // 厨师做菜已改为计时会话，状态机不再启动 WaitForSeconds 协程（避免 StopCoroutine continue failure）。
        }
    }

    /// <summary>
    /// 厨师状态机对 Tavern 场景门面的最小依赖接口。
    /// </summary>
    internal interface IChefRuntimeHost
    {
        bool IsBusinessOpen { get; }

        GameObject GetAvailableChefForTask(CookDishTask task);

        bool TryStartChefTask(GameObject chef, CookDishTask task, ICharacterState<ChefCharacter> initialState);

        void StartChefStateRoutine(ChefCharacter context, IEnumerator routine);

        void ApplyChefPresentation(GameObject chef, string stateKey);

        bool IsCookTicketActive(int tableId);

        float GetChefCookDuration(ChefCharacter chef, CookDishTask task);

        void ShowChefCookProgress(GameObject chef, float duration);

        void PlayChefCookAnimation(GameObject chef);

        void CompleteChefCookTask(ChefCharacter context, int tableId);

        void AbortChefTask(ChefCharacter context);
    }
}
