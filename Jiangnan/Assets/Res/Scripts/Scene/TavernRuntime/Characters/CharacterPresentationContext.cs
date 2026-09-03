using UnityEngine;
using UnityEngine.AI;

namespace JN.Client.Scene
{
    /// <summary>
    /// 统一缓存角色常用表现组件。
    /// </summary>
    public sealed class CharacterPresentationContext
    {
        public CharacterPresentationContext(GameObject owner)
        {
            if (owner == null)
            {
                return;
            }

            Root = owner;
            Transform = owner.transform;
            Animator = owner.GetComponent<Animator>() ?? owner.GetComponentInChildren<Animator>(true);
            Agent = owner.GetComponent<NavMeshAgent>() ?? owner.GetComponentInChildren<NavMeshAgent>(true);
        }

        public GameObject Root { get; }

        public Transform Transform { get; }

        public Animator Animator { get; }

        public NavMeshAgent Agent { get; }
    }
}
