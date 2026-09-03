using System.Collections;

namespace JN.Client.Scene
{
    /// <summary>
    /// 通用角色状态基类。
    /// </summary>
    internal abstract class CharacterStateBase<TActor> : ICharacterState<TActor>
    {
        public abstract string StateKey { get; }

        public virtual void Enter(TActor actor)
        {
        }

        public virtual IEnumerator Execute(TActor actor)
        {
            yield break;
        }

        public virtual void Exit(TActor actor)
        {
        }
    }
}
