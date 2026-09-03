using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// Tavern 角色基类，统一持有场景门面引用。
    /// </summary>
    public abstract class TavernCharacterBase<TActor, TTask> : CharacterActorBase<TActor, TTask>
        where TActor : TavernCharacterBase<TActor, TTask>
    {
        protected TavernSceneManager Owner { get; private set; }

        internal virtual void InitializeOwner(TavernSceneManager owner)
        {
            Owner = owner;
        }
    }
}
