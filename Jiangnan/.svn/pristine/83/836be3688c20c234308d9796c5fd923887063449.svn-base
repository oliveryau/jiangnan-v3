using System.Collections;

namespace JN.Client.Scene
{
    /// <summary>
    /// 通用角色状态接口。
    /// </summary>
    internal interface ICharacterState<TActor>
    {
        string StateKey { get; }

        void Enter(TActor actor);

        IEnumerator Execute(TActor actor);

        void Exit(TActor actor);
    }
}
