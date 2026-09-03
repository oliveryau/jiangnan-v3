using System.Collections;

namespace JN.Client.Scene
{
    /// <summary>
    /// 通用角色状态机，负责状态切换与执行协程。
    /// </summary>
    internal sealed class CharacterStateMachine<TActor>
    {
        private readonly TActor owner;
        private readonly System.Action<TActor, string> onStateApplied;
        private readonly System.Action<TActor, IEnumerator> onRoutineStarted;
        private readonly System.Action<TActor> onPassiveStateApplied;

        public CharacterStateMachine(
            TActor owner,
            System.Action<TActor, string> onStateApplied,
            System.Action<TActor, IEnumerator> onRoutineStarted,
            System.Action<TActor> onPassiveStateApplied = null)
        {
            this.owner = owner;
            this.onStateApplied = onStateApplied;
            this.onRoutineStarted = onRoutineStarted;
            this.onPassiveStateApplied = onPassiveStateApplied;
        }

        public ICharacterState<TActor> CurrentState { get; private set; }

        public string CurrentStateKey { get; private set; }

        public void TransitionTo(ICharacterState<TActor> nextState)
        {
            if (nextState == null)
            {
                return;
            }

            CurrentState?.Exit(owner);
            CurrentState = nextState;
            CurrentStateKey = nextState.StateKey;
            onStateApplied?.Invoke(owner, CurrentStateKey);
            nextState.Enter(owner);
            onRoutineStarted?.Invoke(owner, nextState.Execute(owner));
        }

        public void SetPassiveState(ICharacterState<TActor> nextState)
        {
            if (nextState == null)
            {
                return;
            }

            CurrentState?.Exit(owner);
            CurrentState = nextState;
            CurrentStateKey = nextState.StateKey;
            onStateApplied?.Invoke(owner, CurrentStateKey);
            nextState.Enter(owner);
            onPassiveStateApplied?.Invoke(owner);
        }
    }
}
