using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 小二状态基类。
    /// </summary>
    internal abstract class WaiterStateBase : CharacterStateBase<WaiterCharacter>
    {
        protected static IWaiterRuntimeHost GetHost(WaiterCharacter actor)
        {
            return actor?.Host;
        }

        protected static WaiterTask GetTask(WaiterCharacter actor)
        {
            return actor?.CurrentTask;
        }

        protected static GameObject GetWaiter(WaiterCharacter actor)
        {
            return actor != null ? actor.gameObject : null;
        }
    }
}
