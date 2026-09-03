using System.Collections;
using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 角色基类，统一当前任务、状态机与常用表现缓存。
    /// </summary>
    public abstract class CharacterActorBase<TActor, TTask> : MonoBehaviour
        where TActor : CharacterActorBase<TActor, TTask>
    {
        private CharacterStateMachine<TActor> stateMachine;

        protected virtual void Awake()
        {
            Presentation = new CharacterPresentationContext(gameObject);
            stateMachine = new CharacterStateMachine<TActor>(
                (TActor)this,
                ApplyStateKey,
                StartStateRoutine,
                HandlePassiveStateApplied);
        }

        protected CharacterPresentationContext Presentation { get; private set; }

        internal TTask CurrentTask { get; set; }

        internal string CurrentStateKey => stateMachine != null ? stateMachine.CurrentStateKey : string.Empty;

        internal ICharacterState<TActor> CurrentState => stateMachine?.CurrentState;

        internal abstract CharacterRuntimeTag RuntimeTag { get; }

        internal void TransitionTo(ICharacterState<TActor> nextState)
        {
            stateMachine?.TransitionTo(nextState);
        }

        internal void SetPassiveState(ICharacterState<TActor> nextState)
        {
            stateMachine?.SetPassiveState(nextState);
        }

        protected abstract void ApplyStateKey(TActor actor, string stateKey);

        protected abstract void StartStateRoutine(TActor actor, IEnumerator routine);

        protected virtual void HandlePassiveStateApplied(TActor actor)
        {
        }
    }
}
