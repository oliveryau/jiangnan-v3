using System.Collections;

namespace JN.Client.Scene
{
    internal abstract class ChefStateBase : CharacterStateBase<ChefCharacter>
    {
    }

    internal sealed class ChefIdleState : ChefStateBase
    {
        public override string StateKey => ChefStateKeys.Idle;
    }

    internal sealed class ChefBlockedState : ChefStateBase
    {
        public override string StateKey => ChefStateKeys.Blocked;
    }

    /// <summary>
    /// 做菜表现态：计时与完成由 TavernSceneManager 会话驱动，不再跑 WaitForSeconds 协程。
    /// </summary>
    internal sealed class ChefCookingState : ChefStateBase
    {
        public override string StateKey => ChefStateKeys.Cooking;

        public override IEnumerator Execute(ChefCharacter actor)
        {
            yield break;
        }
    }

    /// <summary>
    /// 厨师打盹：无 Sleep 动画时保持默认站姿，仅靠头顶 UI 表达。
    /// </summary>
    internal sealed class ChefNappingState : ChefStateBase
    {
        public override string StateKey => ChefStateKeys.Napping;

        public override IEnumerator Execute(ChefCharacter actor)
        {
            yield break;
        }
    }

    internal sealed class ChefReturningHomeState : ChefStateBase
    {
        public override string StateKey => ChefStateKeys.ReturningHome;
    }
}
