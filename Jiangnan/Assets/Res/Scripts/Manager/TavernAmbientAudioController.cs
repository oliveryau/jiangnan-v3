using UnityEngine;
using UnityEngine.SceneManagement;
using JN.Client.Scene;

namespace JN.Client.Manager
{
    /// <summary>
    /// 统一管理 BGM 与酒楼循环背景人声：单音源、去重、按店内顾客数调节音量。
    /// </summary>
    internal sealed class TavernAmbientAudioController
    {
        private const string BgmAssetPath = "Assets/Res/Resources/Audios/bgm.mp3";
        private const string CrowdAssetPath = "Assets/Res/Resources/Audios/开店人群嘈杂声.mp3";
        private const string BgmNodeName = "BgmAudioSource";
        private const string CrowdNodeName = "BackgroundCrowdAudioSource";
        private const string LegacySfxNodeName = "SfxAudioSource";

        /// <summary>BGM 目标音量。</summary>
        private const float BgmVolume = 0.6f;
        /// <summary>指定 PopUI 打开时在目标音量基础上降低的幅度。</summary>
        private const float BgmOverlayDuckAmount = 0.4f;
        /// <summary>8 名在店顾客时达到的人声峰值（再乘人数比例）。</summary>
        private const int CrowdFullVolumeCustomerCount = 8;
        /// <summary>峰值人声音量上限，避免多轨叠加后过响。</summary>
        private const float CrowdMaxVolume = 0.42f;
        private const float CrowdVolumeBlendSpeed = 1.8f;
        private const float LegacySuppressInterval = 1f;

        private Transform host;
        private AudioSource bgmSource;
        private AudioSource crowdSource;
        private AudioClip cachedCrowdClip;
        private int lastCustomerCount = int.MinValue;
        private float targetCrowdVolume;
        private float currentCrowdVolume;
        private int bgmPauseRequestCount;
        private int bgmOverlayDuckRequestCount;
        private float nextLegacySuppressTime;

        public void Bind(Transform audioHost)
        {
            host = audioHost;
        }

        public void Initialize()
        {
            EnsureSources();
            SilenceLegacySfxNode();
            SuppressDuplicateCrowdSources(force: true);
            lastCustomerCount = int.MinValue;
            targetCrowdVolume = 0f;
            currentCrowdVolume = 0f;
            bgmOverlayDuckRequestCount = 0;
            PlayBgm();
        }

        public void OnSceneLoaded()
        {
            crowdSource = null;
            bgmSource = null;
            cachedCrowdClip = null;
            lastCustomerCount = int.MinValue;
            targetCrowdVolume = 0f;
            currentCrowdVolume = 0f;
            Initialize();
        }

        public void Tick(float deltaTime)
        {
            if (!Mathf.Approximately(currentCrowdVolume, targetCrowdVolume))
            {
                currentCrowdVolume = Mathf.MoveTowards(
                    currentCrowdVolume,
                    targetCrowdVolume,
                    CrowdVolumeBlendSpeed * deltaTime);
                ApplyCrowdVolume(currentCrowdVolume);
            }

            if (Time.unscaledTime >= nextLegacySuppressTime)
            {
                nextLegacySuppressTime = Time.unscaledTime + LegacySuppressInterval;
                SuppressDuplicateCrowdSources(force: false);
            }
        }

        public void PlayBgm()
        {
            EnsureSources();
            var clip = LoadClip(BgmAssetPath);
            if (clip == null || bgmSource == null)
            {
                return;
            }

            if (bgmSource.clip == clip && (bgmSource.isPlaying || bgmPauseRequestCount > 0))
            {
                ApplyBgmVolume();
                return;
            }

            bgmSource.clip = clip;
            bgmSource.loop = true;
            ApplyBgmVolume();
            bgmSource.spatialBlend = 0f;
            bgmSource.playOnAwake = false;

            if (bgmPauseRequestCount > 0)
            {
                bgmSource.Play();
                bgmSource.Pause();
                return;
            }

            bgmSource.Play();
        }

        /// <summary>
        /// PopUI 打开时降低 BGM，支持多层叠加。
        /// </summary>
        public void DuckBgmForOverlay()
        {
            bgmOverlayDuckRequestCount++;
            ApplyBgmVolume();
        }

        /// <summary>
        /// PopUI 关闭时恢复 BGM，与 <see cref="DuckBgmForOverlay"/> 成对调用。
        /// </summary>
        public void UnduckBgmForOverlay()
        {
            if (bgmOverlayDuckRequestCount <= 0)
            {
                bgmOverlayDuckRequestCount = 0;
                return;
            }

            bgmOverlayDuckRequestCount--;
            ApplyBgmVolume();
        }

        public void PauseBgm()
        {
            EnsureSources();
            bgmPauseRequestCount++;
            if (bgmPauseRequestCount == 1 && bgmSource != null && bgmSource.isPlaying)
            {
                bgmSource.Pause();
            }
        }

        public void ResumeBgm()
        {
            EnsureSources();
            if (bgmPauseRequestCount <= 0)
            {
                bgmPauseRequestCount = 0;
                return;
            }

            bgmPauseRequestCount--;
            if (bgmPauseRequestCount > 0 || bgmSource == null || bgmSource.clip == null)
            {
                return;
            }

            if (bgmSource.time > 0f)
            {
                bgmSource.UnPause();
            }
            else if (!bgmSource.isPlaying)
            {
                bgmSource.Play();
            }
        }

        /// <summary>
        /// 开业时不叠加播放同一条人声片段，仅刷新目标音量（随顾客入场渐强）。
        /// </summary>
        public void NotifyBusinessOpened()
        {
            SuppressDuplicateCrowdSources(force: true);
        }

        public void SetCustomerCount(int inStoreCustomerCount)
        {
            if (inStoreCustomerCount == lastCustomerCount)
            {
                return;
            }

            lastCustomerCount = inStoreCustomerCount;
            targetCrowdVolume = 0f;
            if (inStoreCustomerCount > 0)
            {
                var ratio = Mathf.Clamp01(inStoreCustomerCount / (float)CrowdFullVolumeCustomerCount);
                targetCrowdVolume = ratio * CrowdMaxVolume;
            }

            if (inStoreCustomerCount <= 0)
            {
                currentCrowdVolume = 0f;
                ApplyCrowdVolume(0f);
            }
        }

        public void StopCrowdImmediate()
        {
            lastCustomerCount = 0;
            targetCrowdVolume = 0f;
            currentCrowdVolume = 0f;
            ApplyCrowdVolume(0f);
        }

        public bool IsCrowdClip(AudioClip clip)
        {
            return IsCrowdClipInternal(clip, GetCrowdClip());
        }

        public bool IsManagedSource(AudioSource source)
        {
            if (source == null || host == null)
            {
                return false;
            }

            return source.transform.IsChildOf(host);
        }

        private void ApplyBgmVolume()
        {
            if (bgmSource == null)
            {
                return;
            }

            var volume = BgmVolume;
            if (bgmOverlayDuckRequestCount > 0)
            {
                volume = Mathf.Max(0f, BgmVolume - BgmOverlayDuckAmount);
            }

            bgmSource.volume = volume;
        }

        private void EnsureSources()
        {
            if (host == null)
            {
                return;
            }

            bgmSource = EnsureLoopSource(BgmNodeName, bgmSource);
            crowdSource = EnsureLoopSource(CrowdNodeName, crowdSource);
            SilenceLegacySfxNode();
        }

        private AudioSource EnsureLoopSource(string nodeName, AudioSource cached)
        {
            if (cached != null)
            {
                return cached;
            }

            var child = host.Find(nodeName);
            if (child == null)
            {
                var childObject = new GameObject(nodeName);
                childObject.transform.SetParent(host, false);
                child = childObject.transform;
            }

            var source = child.GetComponent<AudioSource>();
            if (source == null)
            {
                source = child.gameObject.AddComponent<AudioSource>();
            }

            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.enabled = true;
            return source;
        }

        private void ApplyCrowdVolume(float volume)
        {
            EnsureSources();
            if (crowdSource == null)
            {
                return;
            }

            var crowdClip = GetCrowdClip();
            crowdSource.playOnAwake = false;
            crowdSource.loop = true;
            crowdSource.spatialBlend = 0f;
            crowdSource.mute = false;
            crowdSource.enabled = true;

            if (crowdSource.clip == null && crowdClip != null)
            {
                crowdSource.clip = crowdClip;
            }

            crowdSource.volume = volume;
            if (volume <= 0f)
            {
                crowdSource.Stop();
                return;
            }

            if (!crowdSource.isPlaying)
            {
                crowdSource.Play();
            }
        }

        private AudioClip GetCrowdClip()
        {
            if (cachedCrowdClip == null)
            {
                cachedCrowdClip = LoadClip(CrowdAssetPath);
            }

            return cachedCrowdClip;
        }

        private void SilenceLegacySfxNode()
        {
            if (host == null)
            {
                return;
            }

            var legacyNode = host.Find(LegacySfxNodeName);
            if (legacyNode == null)
            {
                return;
            }

            var sources = legacyNode.GetComponents<AudioSource>();
            for (var index = 0; index < sources.Length; index++)
            {
                var source = sources[index];
                if (source == null)
                {
                    continue;
                }

                source.Stop();
                source.clip = null;
                source.volume = 0f;
                source.loop = false;
                source.playOnAwake = false;
                source.enabled = false;
            }

            if (legacyNode.gameObject.activeSelf)
            {
                legacyNode.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 停用场景内 SceneMusic 及一切非托管的人声循环，避免与专用音源叠播。
        /// </summary>
        private void SuppressDuplicateCrowdSources(bool force)
        {
            if (!force && !IsTavernGameplayScene())
            {
                return;
            }

            var crowdClip = GetCrowdClip();
            DisableSceneMusicObjects();
            DisableRedundantCustomerCrowdSources(crowdClip);

            var sources = Object.FindObjectsOfType<AudioSource>(true);
            for (var index = 0; index < sources.Length; index++)
            {
                var source = sources[index];
                if (source == null || IsManagedSource(source))
                {
                    continue;
                }

                if (!ShouldSuppressAsDuplicateCrowd(source, crowdClip))
                {
                    continue;
                }

                source.Stop();
                source.volume = 0f;
                source.mute = true;
                source.loop = false;
                source.playOnAwake = false;
                source.clip = null;
                source.enabled = false;
            }
        }

        private void DisableSceneMusicObjects()
        {
            var sceneMusicObjects = Object.FindObjectsOfType<Transform>(true);
            for (var index = 0; index < sceneMusicObjects.Length; index++)
            {
                var node = sceneMusicObjects[index];
                if (node == null || !string.Equals(node.name, "SceneMusic", System.StringComparison.Ordinal))
                {
                    continue;
                }

                var sources = node.GetComponents<AudioSource>();
                for (var sourceIndex = 0; sourceIndex < sources.Length; sourceIndex++)
                {
                    var source = sources[sourceIndex];
                    if (source == null)
                    {
                        continue;
                    }

                    source.Stop();
                    source.clip = null;
                    source.volume = 0f;
                    source.enabled = false;
                }

                node.gameObject.SetActive(false);
            }
        }

        private static void DisableRedundantCustomerCrowdSources(AudioClip crowdClip)
        {
            var customers = Object.FindObjectsOfType<TavernCustomerRuntimeController>(true);
            for (var index = 0; index < customers.Length; index++)
            {
                var customer = customers[index];
                if (customer == null)
                {
                    continue;
                }

                var sources = customer.GetComponentsInChildren<AudioSource>(true);
                for (var sourceIndex = 0; sourceIndex < sources.Length; sourceIndex++)
                {
                    var source = sources[sourceIndex];
                    if (source == null)
                    {
                        continue;
                    }

                    if (source.isPlaying || IsCrowdClipInternal(source.clip, crowdClip))
                    {
                        source.Stop();
                        source.clip = null;
                    }

                    source.volume = 0f;
                    source.loop = false;
                    source.playOnAwake = false;
                    source.enabled = false;
                }
            }
        }

        private static bool ShouldSuppressAsDuplicateCrowd(AudioSource source, AudioClip crowdClip)
        {
            if (source == null)
            {
                return false;
            }

            if (string.Equals(source.gameObject.name, "SceneMusic", System.StringComparison.Ordinal))
            {
                return true;
            }

            if (source.loop && IsCrowdClipInternal(source.clip, crowdClip))
            {
                return true;
            }

            return IsCrowdClipInternal(source.clip, crowdClip) && source.isPlaying;
        }

        private static bool IsCrowdClipInternal(AudioClip clip, AudioClip crowdClip)
        {
            if (clip == null)
            {
                return false;
            }

            if (crowdClip != null && clip == crowdClip)
            {
                return true;
            }

            return clip.name.Contains("开店人群") || clip.name.Contains("人群嘈杂");
        }

        private static bool IsTavernGameplayScene()
        {
            return SceneFlowCoordinator.IsTavernGameplaySceneName(SceneManager.GetActiveScene().name);
        }

        private static AudioClip LoadClip(string assetPath)
        {
            return GameplayResourceStore.LoadAsset<AudioClip>(assetPath);
        }
    }
}
