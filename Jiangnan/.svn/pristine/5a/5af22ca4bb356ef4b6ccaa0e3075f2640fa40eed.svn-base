using JN.Client.Manager;
using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 小二走路拖尾：预制体 <c>Tail</c> 节点（粒子 + TrailRenderer），
    /// 在移速科技 103 解锁后随移速倍率同步显隐与强度。
    /// </summary>
    public sealed class WaiterMoveTrailView : MonoBehaviour
    {
        private const string TailNodeName = "Tail";
        private const int WaiterMoveSpeedTechId = 103;
        /// <summary>场景小二 NavMesh 移速基准（与 WaiterService 一致），用于强度换算。</summary>
        private const float SceneWaiterMoveSpeed = 1.15f;
        /// <summary>tbtaverntech 103 staffEffectValue=1300 → ×1.3。</summary>
        private const float Tech103MoveSpeedMultiplier = 1.3f;

        private Transform tailRoot;
        private ParticleSystem[] particleSystems = System.Array.Empty<ParticleSystem>();
        private TrailRenderer[] trailRenderers = System.Array.Empty<TrailRenderer>();
        private float[] defaultEmissionRates = System.Array.Empty<float>();
        private float[] defaultTrailWidths = System.Array.Empty<float>();
        private bool initialized;

        /// <summary>
        /// 解析或挂载小二拖尾控制器。
        /// </summary>
        public static WaiterMoveTrailView Resolve(Transform waiterRoot)
        {
            if (waiterRoot == null)
            {
                return null;
            }

            var existing = waiterRoot.GetComponentInChildren<WaiterMoveTrailView>(true);
            if (existing != null)
            {
                existing.EnsureInitialized(waiterRoot);
                return existing;
            }

            var tail = FindChildRecursive(waiterRoot, TailNodeName);
            if (tail == null)
            {
                return null;
            }

            var view = waiterRoot.gameObject.AddComponent<WaiterMoveTrailView>();
            view.EnsureInitialized(waiterRoot);
            return view;
        }

        /// <summary>
        /// 开始移动时启用拖尾，强度与当前移速倍率同步。
        /// </summary>
        public void BeginMove(float moveSpeed, GameObject waiterObject)
        {
            if (!ShouldShowTrail() || !EnsureInitialized(waiterObject != null ? waiterObject.transform : transform))
            {
                EndMove();
                return;
            }

            var intensity = ResolveTrailIntensity(moveSpeed);
            ApplyIntensity(intensity);
            tailRoot.gameObject.SetActive(true);
            PlayEffects();
        }

        /// <summary>
        /// 停止移动时关闭拖尾并清轨迹。
        /// </summary>
        public void EndMove()
        {
            if (tailRoot != null)
            {
                tailRoot.gameObject.SetActive(false);
            }

            StopEffects();
            ClearTrails();
        }

        private bool EnsureInitialized(Transform waiterRoot)
        {
            if (initialized)
            {
                return tailRoot != null;
            }

            var root = waiterRoot != null ? waiterRoot : transform;
            tailRoot = FindChildRecursive(root, TailNodeName);
            if (tailRoot == null)
            {
                return false;
            }

            particleSystems = tailRoot.GetComponentsInChildren<ParticleSystem>(true);
            trailRenderers = tailRoot.GetComponentsInChildren<TrailRenderer>(true);
            defaultEmissionRates = new float[particleSystems.Length];
            for (var index = 0; index < particleSystems.Length; index++)
            {
                var emission = particleSystems[index].emission;
                defaultEmissionRates[index] = emission.rateOverTime.constant;
            }

            defaultTrailWidths = new float[trailRenderers.Length];
            for (var index = 0; index < trailRenderers.Length; index++)
            {
                defaultTrailWidths[index] = trailRenderers[index].widthMultiplier;
            }

            tailRoot.gameObject.SetActive(false);
            initialized = true;
            return true;
        }

        private static bool ShouldShowTrail()
        {
            return DataManager.Instance != null && DataManager.Instance.IsTechResearched(WaiterMoveSpeedTechId);
        }

        /// <summary>
        /// 用场景实际移速（已含科技/天赋/情绪）换算拖尾强度，避免画像倍率与 DefaultMoveSpeed 基准不一致。
        /// </summary>
        private static float ResolveTrailIntensity(float moveSpeed)
        {
            if (moveSpeed <= 0.01f)
            {
                return Tech103MoveSpeedMultiplier;
            }

            var speedRatio = moveSpeed / SceneWaiterMoveSpeed;
            return Mathf.Max(Tech103MoveSpeedMultiplier, speedRatio);
        }

        private void ApplyIntensity(float moveSpeedMultiplier)
        {
            var intensity = Mathf.Clamp(
                moveSpeedMultiplier / Tech103MoveSpeedMultiplier,
                1f,
                2.5f);

            for (var index = 0; index < particleSystems.Length; index++)
            {
                var particle = particleSystems[index];
                if (particle == null)
                {
                    continue;
                }

                var emission = particle.emission;
                emission.rateOverTime = defaultEmissionRates[index] * intensity;

                var main = particle.main;
                main.simulationSpeed = intensity;
            }

            for (var index = 0; index < trailRenderers.Length; index++)
            {
                var trail = trailRenderers[index];
                if (trail == null)
                {
                    continue;
                }

                trail.widthMultiplier = defaultTrailWidths[index] * intensity;
            }
        }

        private void PlayEffects()
        {
            for (var index = 0; index < particleSystems.Length; index++)
            {
                var particle = particleSystems[index];
                if (particle == null)
                {
                    continue;
                }

                particle.Clear(true);
                particle.Play(true);
            }
        }

        private void StopEffects()
        {
            for (var index = 0; index < particleSystems.Length; index++)
            {
                var particle = particleSystems[index];
                if (particle == null)
                {
                    continue;
                }

                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void ClearTrails()
        {
            for (var index = 0; index < trailRenderers.Length; index++)
            {
                trailRenderers[index]?.Clear();
            }
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (var index = 0; index < root.childCount; index++)
            {
                var found = FindChildRecursive(root.GetChild(index), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
