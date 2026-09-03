using System;
using System.Collections;
using System.Collections.Generic;
using JN.Client.Model;
using JN.Client.Scene;
using JN.Client.UI;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JN.Client.Manager
{
    /// <summary>
    /// 负责游戏相关的运行时逻辑。
    /// </summary>
    public class GameManager : MonoSingleton<GameManager>
    {
        private bool sceneLoadedSubscribed;

        /// <summary>
        /// 在场景加载前自动初始化模块。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitializeBeforeSceneLoad()
        {
            Instance.Init();
            SceneFlowCoordinator.SyncHudForScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// 初始化模块依赖和默认状态。
        /// </summary>
        public void Init()
        {
            SO_Product.GetAll();
            SO_Equipment.GetAll();
            SO_Customer.GetAll();
            SO_Staff.GetAll();
            SO_Shop.GetAll();
            SO_Routine.GetAll();

            EnsureSceneHudListener();
            GameAudioManager.Instance.Init();
        }

        /// <summary>
        /// 加载场景异步。
        /// </summary>
        /// <param name="sceneName">名称。</param>
        /// <param name="on加载完成">参数值。</param>
        /// <returns>协程迭代器。</returns>
        public IEnumerator LoadSceneAsync(string sceneName, Action onLoaded = null)
        {
            yield return SceneFlowCoordinator.LoadSceneAsync(sceneName, onLoaded);
        }

        /// <summary>
        /// 确保场景状态栏监听器存在。
        /// </summary>
        private void EnsureSceneHudListener()
        {
            if (sceneLoadedSubscribed)
            {
                return;
            }

            SceneManager.sceneLoaded += HandleSceneLoaded;
            sceneLoadedSubscribed = true;
        }

        /// <summary>
        /// 处理场景加载完成。
        /// </summary>
        /// <param name="scene">参数值。</param>
        /// <param name="mode">参数值。</param>
        private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            SceneFlowCoordinator.SyncHudForScene(scene.name);
        }
    }

    /// <summary>
    /// 负责全局背景音乐、通用音效和按钮点击音效挂接。
    /// </summary>
    public class GameAudioManager : MonoSingleton<GameAudioManager>
    {
        private const string IncomeAssetPath = "Assets/Res/Resources/Audios/收钱音效.mp3";
        private const string TownBuildAssetPath = "Assets/Res/Resources/Audios/建造店铺音效.mp3";
        private const string TableMoveAssetPath = "Assets/Res/Resources/Audios/搬家具.mp3";
        private const string BuildPutDownAssetPath = "Assets/Res/Resources/Audios/putdown.mp3";
        private const string WaiterCookStealAssetPath = "Assets/Res/Resources/Audios/偷吃音效.mp3";
        private const string WaiterCheckoutStealAssetPath = "Assets/Res/Resources/Audios/偷钱音效.mp3";
        private const string WaiterNapAssetPath = "Assets/Res/Resources/Audios/打盹音效.mp3";
        private const string SlapAssetPath = "Assets/Res/Resources/Audios/扇巴掌音效.mp3";
        private const string ScreamAssetPath = "Assets/Res/Resources/Audios/被打惨叫音效.mp3";
        private const string CoinBurstAssetPath = "Assets/Res/Resources/Audios/巴掌打爆金币.mp3";
        private const string CleanupAssetPath = "Assets/Res/Resources/Audios/打扫音效.mp3";
        private const string RecruitShopkeeperAssetPath = "Assets/Res/Resources/Audios/招募掌柜音效.mp3";
        private const string RecruitWaiterAssetPath = "Assets/Res/Resources/Audios/招募小二音效.mp3";
        private const string RecruitChefAssetPath = "Assets/Res/Resources/Audios/招募厨师音效.mp3";
        private const string ChefCookAssetPath = "Assets/Res/Resources/Audios/炒菜音效.mp3";
        private const string SettlementSuccessAssetPath = "Assets/Res/Resources/Audios/结算成功.mp3";
        private const string DishGuessCorrectAssetPath = "Assets/Res/Resources/Audios/true.mp3";
        private const string DishGuessWrongAssetPath = "Assets/Res/Resources/Audios/false.mp3";
        private const string DishGuessPremiumProfitAssetPath = "Assets/Res/Resources/Audios/coinfirst.mp3";
        private const string VipSatisfiedAssetPath = "Assets/Res/Resources/Audios/贵客满意.mp3";
        private const string VipCheckoutCoinAssetPath = "Assets/Res/Resources/Audios/贵客收钱金币音效.mp3";
        private const string VipArrivalAssetPath = "Assets/Res/Resources/Audios/贵客临门.mp3";
        private const string MenuPlaqueFlipAssetPath = "Assets/Res/Resources/Audios/木牌.mp3";
        private const string PeakTimeWaiterShoutAssetPath = "Assets/Res/Resources/Audios/客流加成-小二吆喝.mp3";
        private const string FacilityPurchaseSuccessAssetPath = "Assets/Res/Resources/Audios/设备购买成功.mp3";
        private const string SettlementIncomeRiseAssetPath = "Assets/Res/Resources/Audios/up.mp3";
        private const string TaskAssetPath = "Assets/Res/Resources/Audios/Effects/Task.mp3";
        private const string FeatureUnlockAssetPath = "Assets/Res/Resources/Audios/Effects/UI/QuickOpen.mp3";
        private const string GetAchievementAssetPath = "Assets/Res/Resources/Audios/getachievement.mp3";
        private const string AchievementRewardCoinAssetPath = "Assets/Res/Resources/Audios/achievementcoin.mp3";
        private const string UnlockAssetPath = "Assets/Res/Resources/Audios/unlock.mp3";
        private const string UiClickAssetPath = "Assets/Res/Resources/Audios/通用点击音效2.mp3";
        private const float ButtonScanInterval = 0.5f;
        private const float InterruptScreamDelay = 0.18f;
        private const float CheckoutComboWindowSeconds = 2f;
        private const float CoinSfxVolume = 0.8f;
        private const float ChefCookSfxVolume = 0.48f;
        private static readonly float[] CheckoutComboPitches = { 0.95f, 1f, 1.1f, 1.15f, 1.2f };
        private const string LegacySfxAudioSourceNodeName = "SfxAudioSource";
        private const string OneShotSfxAudioSourceNodeName = "OneShotSfxAudioSource";
        private const string SettlementIncomeRiseChannelKey = "SettlementIncomeRiseSfx";
        private const float SettlementIncomeRisePitchMin = 0.8f;
        private const float SettlementIncomeRisePitchMax = 1.2f;

        private readonly TavernAmbientAudioController ambientAudio = new();
        private AudioSource sfxSource;
        private AudioSource pitchedSfxSource;
        private readonly Dictionary<string, AudioSource> interruptibleSfxSources = new();
        private float checkoutComboWindowEndUnscaledTime = float.NegativeInfinity;
        private int checkoutComboStep;
        private float nextButtonScanTime;
        private float nextTechResearchTickTime;
        private int lastCompletedGuideTaskCount = -1;
        private Coroutine settlementIncomeRiseRoutine;

        /// <summary>
        /// 初始化音频源与监听。
        /// </summary>
        public void Init()
        {
            sfxSource = null;
            ambientAudio.Bind(transform);
            EnsureAudioSources();
            EnsureSceneListener();
            EnsureGuideTaskListener();
            ambientAudio.Initialize();
            AttachClickSoundHooksInActiveScene();
        }

        /// <summary>
        /// 每帧补挂当前场景中新出现的按钮点击音效组件。
        /// </summary>
        private void Update()
        {
            ambientAudio.Tick(Time.unscaledDeltaTime);
            TickTechResearchIfNeeded();

            if (Time.unscaledTime < nextButtonScanTime)
            {
                return;
            }

            nextButtonScanTime = Time.unscaledTime + ButtonScanInterval;
            AttachClickSoundHooksInActiveScene();
        }

        private void TickTechResearchIfNeeded()
        {
            if (Time.unscaledTime < nextTechResearchTickTime)
            {
                return;
            }

            nextTechResearchTickTime = Time.unscaledTime + 0.25f;
            var dataManager = DataManager.Instance;
            if (dataManager?.SaveData?.gameplay == null || dataManager.SaveData.gameplay.researchingTechId <= 0)
            {
                return;
            }

            dataManager.TickTechResearch();
        }

        /// <summary>
        /// 播放场景背景音乐。
        /// </summary>
        public static void PlaySceneBgm()
        {
            Instance.ambientAudio.PlayBgm();
        }

        /// <summary>
        /// 按店内顾客人数刷新循环背景人声音量。
        /// </summary>
        public static void RefreshTavernBackgroundCrowdVolume(int inStoreCustomerCount)
        {
            Instance.ambientAudio.SetCustomerCount(inStoreCustomerCount);
        }

        /// <summary>
        /// 播放结账金币音效；窗口期内连续收账时 Pitch 依次 1.0 → 1.05 → 1.1 → 1.15 → 1.2（5 档）。
        /// </summary>
        public static void PlayCheckoutCoins()
        {
            Instance.PlayCheckoutCoinsWithCombo();
        }

        /// <summary>
        /// 播放建造相关音效。
        /// </summary>
        public static void PlayConstruction()
        {
            PlayTableMove();
        }

        /// <summary>
        /// 建造落位完成音效（桌子、灶台、前台等）。
        /// </summary>
        public static void PlayBuildPutDown()
        {
            Instance.PlayOneShot(BuildPutDownAssetPath, CoinSfxVolume);
        }

        /// <summary>
        /// 播放任务完成音效。
        /// </summary>
        public static void PlayTaskComplete()
        {
            Instance.PlayOneShot(TaskAssetPath, 1f);
        }

        /// <summary>
        /// 播放成就/科技等解锁提示音效。
        /// </summary>
        public static void PlayFeatureUnlock()
        {
            Instance.PlayOneShot(FeatureUnlockAssetPath, 0.95f);
        }

        /// <summary>
        /// 播放成就获得横幅展开音效。
        /// </summary>
        public static void PlayGetAchievement()
        {
            Instance.PlayOneShot(GetAchievementAssetPath, 0.95f);
        }

        /// <summary>
        /// 播放成就奖励领取音效。
        /// </summary>
        public static void PlayAchievementRewardCoin()
        {
            Instance.PlayOneShot(AchievementRewardCoinAssetPath, CoinSfxVolume);
        }

        /// <summary>
        /// 播放科技解锁动画配套音效。
        /// </summary>
        public static void PlayUnlock()
        {
            Instance.PlayOneShot(UnlockAssetPath, 1f);
        }

        /// <summary>
        /// 播放清扫音效。
        /// </summary>
        public static void PlayWiping(int tableId)
        {
            PlayCleanup(tableId);
        }

        /// <summary>
        /// 停止指定桌位的清扫音效。
        /// </summary>
        public static void StopWiping(int tableId)
        {
            StopCleanup(tableId);
        }

        /// <summary>
        /// 播放普通按钮点击音效。
        /// </summary>
        public static void PlayButtonClick()
        {
            Instance.PlayOneShot(UiClickAssetPath, 0.8f);
        }

        /// <summary>
        /// 播放正常收钱音效（营业收账等，固定 Pitch 1）。
        /// </summary>
        public static void PlayIncomeCollection()
        {
            Instance.PlayOneShot(IncomeAssetPath, CoinSfxVolume);
        }

        /// <summary>
        /// 成就/UI 领奖等非营业收账的金币音效：固定 Pitch 1，不走结账连击升调。
        /// </summary>
        public static void PlayRewardCoins()
        {
            Instance.PlayRewardCoinsInternal();
        }

        /// <summary>
        /// 每次收账预留连击窗口；窗口内再次收账时音调逐级升高（5 档，最高 1.2）。
        /// </summary>
        private void PlayCheckoutCoinsWithCombo()
        {
            var now = Time.unscaledTime;
            if (now > checkoutComboWindowEndUnscaledTime)
            {
                checkoutComboStep = 0;
            }

            var pitchIndex = Mathf.Clamp(checkoutComboStep, 0, CheckoutComboPitches.Length - 1);
            checkoutComboStep = Mathf.Min(checkoutComboStep + 1, CheckoutComboPitches.Length - 1);
            checkoutComboWindowEndUnscaledTime = now + CheckoutComboWindowSeconds;
            PlayOneShot(IncomeAssetPath, CoinSfxVolume, CheckoutComboPitches[pitchIndex]);
        }

        private void PlayRewardCoinsInternal()
        {
            EnsureAudioSources();
            EnsureOneShotSfxChannelActive();
            var clip = LoadAudioClip(IncomeAssetPath);
            if (clip == null)
            {
                return;
            }

            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(clip, CoinSfxVolume);
        }

        /// <summary>
        /// 播放城镇盖店铺音效。
        /// </summary>
        public static void PlayTownBuild()
        {
            Instance.PlayOneShot(TownBuildAssetPath, 0.9f);
        }

        /// <summary>
        /// 播放酒楼内部搬桌与配送落位音效。
        /// </summary>
        public static void PlayTableMove()
        {
            Instance.PlayOneShot(TableMoveAssetPath, 0.9f);
        }

        /// <summary>
        /// 播放指定桌位的可中断搬桌音效。
        /// </summary>
        public static void PlayTableMove(int tableId)
        {
            if (tableId <= 0)
            {
                PlayTableMove();
                return;
            }

            Instance.PlayInterruptibleClip(GetTableMoveChannelKey(tableId), TableMoveAssetPath, 0.9f);
        }

        /// <summary>
        /// 停止指定桌位的搬桌音效。
        /// </summary>
        public static void StopTableMove(int tableId)
        {
            if (tableId <= 0)
            {
                return;
            }

            Instance.StopInterruptibleClip(GetTableMoveChannelKey(tableId));
        }

        /// <summary>
        /// 播放小二偷吃音效。
        /// </summary>
        public static void PlayWaiterCookSteal()
        {
            Instance.PlayOneShot(WaiterCookStealAssetPath, 0.95f);
        }

        /// <summary>
        /// 播放指定小二的可中断偷吃音效。
        /// </summary>
        public static void PlayWaiterCookSteal(GameObject waiter)
        {
            if (waiter == null)
            {
                PlayWaiterCookSteal();
                return;
            }

            Instance.PlayInterruptibleClip(GetWaiterCookStealChannelKey(waiter), WaiterCookStealAssetPath, 0.95f, true);
        }

        /// <summary>
        /// 停止指定小二的偷吃音效。
        /// </summary>
        public static void StopWaiterCookSteal(GameObject waiter)
        {
            if (waiter == null)
            {
                return;
            }

            Instance.StopInterruptibleClip(GetWaiterCookStealChannelKey(waiter));
        }

        /// <summary>
        /// 播放小二偷钱音效。
        /// </summary>
        public static void PlayWaiterCheckoutSteal()
        {
            Instance.PlayOneShot(WaiterCheckoutStealAssetPath, 0.95f);
        }

        /// <summary>
        /// 播放指定小二的可中断偷钱音效。
        /// </summary>
        public static void PlayWaiterCheckoutSteal(GameObject waiter)
        {
            if (waiter == null)
            {
                PlayWaiterCheckoutSteal();
                return;
            }

            Instance.PlayInterruptibleClip(GetWaiterCheckoutStealChannelKey(waiter), WaiterCheckoutStealAssetPath, 0.95f);
        }

        /// <summary>
        /// 停止指定小二的偷钱音效。
        /// </summary>
        public static void StopWaiterCheckoutSteal(GameObject waiter)
        {
            if (waiter == null)
            {
                return;
            }

            Instance.StopInterruptibleClip(GetWaiterCheckoutStealChannelKey(waiter));
        }

        /// <summary>
        /// 播放小二打盹音效。
        /// </summary>
        public static void PlayWaiterNap()
        {
            Instance.PlayOneShot(WaiterNapAssetPath, 0.9f);
        }

        /// <summary>
        /// 播放指定小二的可中断打盹音效。
        /// </summary>
        public static void PlayWaiterNap(GameObject waiter)
        {
            if (waiter == null)
            {
                PlayWaiterNap();
                return;
            }

            Instance.PlayInterruptibleClip(GetWaiterNapChannelKey(waiter), WaiterNapAssetPath, 0.9f, true);
        }

        /// <summary>
        /// 停止指定小二的打盹音效。
        /// </summary>
        public static void StopWaiterNap(GameObject waiter)
        {
            if (waiter == null)
            {
                return;
            }

            Instance.StopInterruptibleClip(GetWaiterNapChannelKey(waiter));
        }

        /// <summary>
        /// 停止指定小二所有可中断状态音效。
        /// </summary>
        public static void StopWaiterInterruptibleSounds(GameObject waiter)
        {
            StopWaiterNap(waiter);
            StopWaiterCheckoutSteal(waiter);
            StopWaiterCookSteal(waiter);
        }

        /// <summary>
        /// 停止全部可中断循环/占用音效（切场景前调用；通道挂在 GameAudioManager 上，场景销毁不会自动停）。
        /// </summary>
        public static void StopAllInterruptibleSfx()
        {
            if (Instance == null)
            {
                return;
            }

            Instance.StopAllInterruptibleClipsInternal();
        }

        /// <summary>
        /// 播放打断偷吃、偷钱、打盹时的连击音效。
        /// </summary>
        /// <param name="playCoinBurst">是否播放「打爆金币」；叫醒打盹不产钱时传 false。</param>
        /// <param name="playScream">是否播放惨叫；叫醒小二不播叫声。</param>
        public static void PlayInterruptCombo(bool playCoinBurst = true, bool playScream = true)
        {
            Instance.PlayOneShot(SlapAssetPath, 0.95f);
            if (!playCoinBurst && !playScream)
            {
                return;
            }

            Instance.StartCoroutine(Instance.PlayInterruptComboRoutine(playCoinBurst, playScream));
        }

        /// <summary>
        /// 播放打扫音效；按桌位循环播放，清理结束时需调用 <see cref="StopCleanup"/>。
        /// </summary>
        public static void PlayCleanup(int tableId)
        {
            if (tableId <= 0)
            {
                Instance.PlayOneShot(CleanupAssetPath, 0.85f);
                return;
            }

            Instance.PlayInterruptibleClip(GetTableCleanChannelKey(tableId), CleanupAssetPath, 0.85f, true);
        }

        /// <summary>
        /// 停止指定桌位的打扫音效。
        /// </summary>
        public static void StopCleanup(int tableId)
        {
            if (tableId <= 0)
            {
                return;
            }

            Instance.StopInterruptibleClip(GetTableCleanChannelKey(tableId));
        }

        /// <summary>
        /// 播放招募掌柜音效。
        /// </summary>
        public static void PlayRecruitShopkeeper()
        {
            Instance.PlayOneShot(RecruitShopkeeperAssetPath, 0.95f);
        }

        /// <summary>
        /// 播放招募小二音效。
        /// </summary>
        public static void PlayRecruitWaiter()
        {
            Instance.PlayOneShot(RecruitWaiterAssetPath, 0.95f);
        }

        /// <summary>
        /// 播放招募厨师音效。
        /// </summary>
        public static void PlayRecruitChef()
        {
            Instance.PlayOneShot(RecruitChefAssetPath, 0.95f);
        }

        /// <summary>
        /// 播放指定厨师的循环炒菜音效。
        /// </summary>
        public static void PlayChefCook(GameObject chef)
        {
            if (chef == null)
            {
                return;
            }

            Instance.PlayInterruptibleClip(GetChefCookChannelKey(chef), ChefCookAssetPath, ChefCookSfxVolume, true);
        }

        /// <summary>
        /// 停止指定厨师的炒菜音效。
        /// </summary>
        public static void StopChefCook(GameObject chef)
        {
            if (chef == null)
            {
                return;
            }

            Instance.StopInterruptibleClip(GetChefCookChannelKey(chef));
        }

        /// <summary>
        /// 播放结算成功音效。
        /// </summary>
        public static void PlaySettlementSuccess()
        {
            Instance.PlayOneShot(SettlementSuccessAssetPath, 1f);
        }

        /// <summary>
        /// 贵客猜菜选对了。
        /// </summary>
        public static void PlayDishGuessCorrect()
        {
            Instance.PlayOneShot(DishGuessCorrectAssetPath, 0.95f);
        }

        /// <summary>
        /// 贵客猜菜选错了。
        /// </summary>
        public static void PlayDishGuessWrong()
        {
            Instance.PlayOneShot(DishGuessWrongAssetPath, 0.95f);
        }

        /// <summary>
        /// 贵客猜菜：口味不对但选了最贵。
        /// </summary>
        public static void PlayDishGuessPremiumProfit()
        {
            Instance.PlayOneShot(DishGuessPremiumProfitAssetPath, 0.95f);
        }

        /// <summary>
        /// 贵客满意音效（二楼用餐文字气泡等）。
        /// </summary>
        public static void PlayVipSatisfied()
        {
            Instance.PlayOneShot(VipSatisfiedAssetPath, 0.95f);
        }

        /// <summary>
        /// 贵客点包厢成功：贵客临门。
        /// </summary>
        public static void PlayVipArrival()
        {
            Instance.PlayOneShot(VipArrivalAssetPath, 0.95f);
        }

        /// <summary>
        /// 菜单木牌翻转音效（每张牌一次）。
        /// </summary>
        public static void PlayMenuPlaqueFlip()
        {
            Instance.PlayOneShot(MenuPlaqueFlipAssetPath, 0.95f);
        }

        /// <summary>
        /// 小二吆喝（高峰提示弹出、回自家店卸客共用）。
        /// </summary>
        public static void PlayPeakTimeWaiterShout()
        {
            Instance.PlayOneShot(PeakTimeWaiterShoutAssetPath, 0.95f);
        }

        /// <summary>
        /// 贵客结账金币音效。
        /// </summary>
        public static void PlayVipCheckoutCoins()
        {
            Instance.PlayOneShot(VipCheckoutCoinAssetPath, CoinSfxVolume);
        }

        /// <summary>
        /// 设施购买成功音效（桌子、柜台、厨房设施、轿子、楼梯、戏台等）。
        /// </summary>
        public static void PlayFacilityPurchaseSuccess()
        {
            Instance.PlayOneShot(FacilityPurchaseSuccessAssetPath, 0.95f);
        }

        /// <summary>
        /// 营业结算收入条填充：up.mp3，Pitch 从 0.8 线性升至 1.2。
        /// </summary>
        public static void PlaySettlementIncomeRise(float durationSeconds)
        {
            if (Instance == null)
            {
                return;
            }

            Instance.StartSettlementIncomeRise(durationSeconds);
        }

        /// <summary>
        /// 停止结算收入条音效（面板关闭或动画被打断时）。
        /// </summary>
        public static void StopSettlementIncomeRise()
        {
            Instance.StopSettlementIncomeRiseInternal();
        }

        /// <summary>
        /// 播放鼓舞音效。
        /// </summary>
        public static void PlayInspire()
        {
            Instance.ambientAudio.NotifyBusinessOpened();
        }

        /// <summary>
        /// 视频播放期间暂停背景音乐，支持嵌套请求。
        /// </summary>
        public static void PauseBgmForVideo()
        {
            Instance.ambientAudio.PauseBgm();
        }

        /// <summary>
        /// 视频播放结束后恢复背景音乐，支持嵌套请求。
        /// </summary>
        public static void ResumeBgmForVideo()
        {
            Instance.ambientAudio.ResumeBgm();
        }

        /// <summary>
        /// 主界面 PopUI 打开时降低 BGM（支持多层叠加）。
        /// </summary>
        public static void DuckBgmForOverlay()
        {
            if (Instance == null)
            {
                return;
            }

            Instance.ambientAudio.DuckBgmForOverlay();
        }

        /// <summary>
        /// 主界面 PopUI 关闭时恢复 BGM。
        /// </summary>
        public static void UnduckBgmForOverlay()
        {
            if (Instance == null)
            {
                return;
            }

            Instance.ambientAudio.UnduckBgmForOverlay();
        }

        /// <summary>
        /// 处理场景加载完成事件，刷新 BGM 与按钮音效挂接。
        /// </summary>
        /// <param name="scene">已加载场景。</param>
        /// <param name="mode">加载模式。</param>
        private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            sfxSource = null;
            ambientAudio.Bind(transform);
            EnsureAudioSources();
            ambientAudio.OnSceneLoaded();
            AttachClickSoundHooksInActiveScene();
            CacheGuideTaskProgress();

            if (!SceneFlowCoordinator.IsTavernGameplaySceneName(scene.name))
            {
                ambientAudio.StopCrowdImmediate();
            }
        }

        /// <summary>
        /// 在玩法引导任务完成数增加时播放任务完成音效。
        /// </summary>
        private void HandleGuideProgressChanged()
        {
            if (DataManager.Instance == null)
            {
                return;
            }

            var snapshot = DataManager.Instance.GetGameplayGuideSnapshot();
            if (snapshot == null)
            {
                return;
            }

            var completedCount = CountCompletedGuideTasks(snapshot);
            if (lastCompletedGuideTaskCount >= 0 && completedCount > lastCompletedGuideTaskCount)
            {
                PlayTaskComplete();
            }

            lastCompletedGuideTaskCount = completedCount;
        }

        /// <summary>
        /// 缓存当前引导任务完成数量，避免首次进入场景时误播完成音效。
        /// </summary>
        private void CacheGuideTaskProgress()
        {
            if (DataManager.Instance == null)
            {
                lastCompletedGuideTaskCount = -1;
                return;
            }

            var snapshot = DataManager.Instance.GetGameplayGuideSnapshot();
            lastCompletedGuideTaskCount = snapshot != null ? CountCompletedGuideTasks(snapshot) : -1;
        }

        /// <summary>
        /// 统计当前快照中已完成的任务数量。
        /// </summary>
        /// <param name="snapshot">玩法引导快照。</param>
        /// <returns>已完成任务数。</returns>
        private static int CountCompletedGuideTasks(GameplayGuideSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return 0;
            }

            var completedCount = 0;
            for (var index = 0; index < snapshot.ActiveTasks.Count; index++)
            {
                if (snapshot.ActiveTasks[index] != null && snapshot.ActiveTasks[index].IsCompleted)
                {
                    completedCount++;
                }
            }

            return completedCount;
        }

        /// <summary>
        /// 给当前激活场景中的所有按钮补挂点击音效组件。
        /// </summary>
        private void AttachClickSoundHooksInActiveScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            var roots = scene.GetRootGameObjects();
            for (var rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                var root = roots[rootIndex];
                if (root == null)
                {
                    continue;
                }

                var buttons = root.GetComponentsInChildren<UnityEngine.UI.Button>(true);
                for (var buttonIndex = 0; buttonIndex < buttons.Length; buttonIndex++)
                {
                    var button = buttons[buttonIndex];
                    if (button == null || button.gameObject.GetComponent<UIButtonClickSoundHook>() != null)
                    {
                        continue;
                    }

                    button.gameObject.AddComponent<UIButtonClickSoundHook>();
                }
            }
        }

        private static void ConfigureOneShotSfxSource(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.pitch = 1f;
            source.enabled = true;
            source.mute = false;
        }

        private void SanitizeOneShotSfxSource(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            if (!ambientAudio.IsCrowdClip(source.clip))
            {
                return;
            }

            source.Stop();
            source.clip = null;
            source.loop = false;
            source.playOnAwake = false;
        }

        private bool IsLegacySfxAudioSourceNode(Transform nodeTransform)
        {
            if (nodeTransform == null)
            {
                return false;
            }

            var legacyNode = transform.Find(LegacySfxAudioSourceNodeName);
            return legacyNode != null && nodeTransform == legacyNode;
        }

        private AudioSource EnsureDedicatedOneShotSfxSource()
        {
            var child = transform.Find(OneShotSfxAudioSourceNodeName);
            if (child == null)
            {
                var childObject = new GameObject(OneShotSfxAudioSourceNodeName);
                childObject.transform.SetParent(transform, false);
                child = childObject.transform;
            }

            child.gameObject.SetActive(true);
            var source = child.GetComponent<AudioSource>();
            if (source == null)
            {
                source = child.gameObject.AddComponent<AudioSource>();
            }

            ConfigureOneShotSfxSource(source);
            SanitizeOneShotSfxSource(source);
            return source;
        }

        /// <summary>
        /// 旧版 SfxAudioSource 节点曾误挂循环人声；统一停用，避免与专用背景人声/一次性音效混播。
        /// </summary>
        private void EnsureLegacySfxNodeSilenced()
        {
            var legacyNode = transform.Find(LegacySfxAudioSourceNodeName);
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
        /// 打烊或背景人声归零时，仍保持一次性音效通道可用于收账等音效。
        /// </summary>
        private void EnsureOneShotSfxChannelActive()
        {
            EnsureAudioSources();
            if (sfxSource == null)
            {
                return;
            }

            var oneShotNode = sfxSource.transform;
            if (oneShotNode != null && !oneShotNode.gameObject.activeSelf)
            {
                oneShotNode.gameObject.SetActive(true);
            }

            sfxSource.enabled = true;
            sfxSource.mute = false;
            if (sfxSource.volume <= 0f)
            {
                sfxSource.volume = 1f;
            }

            ConfigureOneShotSfxSource(sfxSource);
            SanitizeOneShotSfxSource(sfxSource);
        }

        private bool IsProtectedOneShotSource(AudioSource source)
        {
            return source != null && (source == sfxSource || source == pitchedSfxSource);
        }

        /// <summary>
        /// 播放一次性音效。
        /// </summary>
        /// <param name="assetPath">音频资源路径。</param>
        /// <param name="volume">音量。</param>
        private void PlayOneShot(string assetPath, float volume)
        {
            PlayOneShot(assetPath, volume, 1f);
        }

        /// <summary>
        /// 播放带 Pitch 的一次性音效（使用独立音源，避免影响其它 SFX）。
        /// </summary>
        private void PlayOneShot(string assetPath, float volume, float pitch)
        {
            EnsureAudioSources();
            EnsureOneShotSfxChannelActive();
            var clip = LoadAudioClip(assetPath);
            if (clip == null)
            {
                return;
            }

            if (Mathf.Approximately(pitch, 1f))
            {
                sfxSource.pitch = 1f;
                sfxSource.PlayOneShot(clip, volume);
                return;
            }

            pitchedSfxSource.pitch = pitch;
            pitchedSfxSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// 在独立通道播放可中断音效，便于后续按对象精确停止。
        /// </summary>
        private void PlayInterruptibleClip(string channelKey, string assetPath, float volume, bool loop = false)
        {
            EnsureAudioSources();
            if (string.IsNullOrEmpty(channelKey))
            {
                PlayOneShot(assetPath, volume);
                return;
            }

            var clip = LoadAudioClip(assetPath);
            if (clip == null)
            {
                return;
            }

            var source = GetOrCreateInterruptibleSource(channelKey);
            if (source.clip == clip && source.isPlaying && source.loop == loop)
            {
                source.volume = volume;
                return;
            }

            source.Stop();
            source.clip = clip;
            source.volume = volume;
            source.loop = loop;
            source.Play();
        }

        /// <summary>
        /// 停止指定独立通道上的可中断音效。
        /// </summary>
        private void StopInterruptibleClip(string channelKey)
        {
            if (string.IsNullOrEmpty(channelKey) || !interruptibleSfxSources.TryGetValue(channelKey, out var source) || source == null)
            {
                return;
            }

            source.Stop();
            source.clip = null;
            source.loop = false;
        }

        private void StopAllInterruptibleClipsInternal()
        {
            foreach (var pair in interruptibleSfxSources)
            {
                var source = pair.Value;
                if (source == null)
                {
                    continue;
                }

                source.Stop();
                source.clip = null;
                source.loop = false;
            }
        }

        /// <summary>
        /// 延迟播放打断后的惨叫（及可选金币爆开），给巴掌落点留一点节奏。
        /// </summary>
        private IEnumerator PlayInterruptComboRoutine(bool playCoinBurst, bool playScream)
        {
            yield return new WaitForSecondsRealtime(InterruptScreamDelay);
            if (playScream)
            {
                PlayOneShot(ScreamAssetPath, 0.9f);
            }

            if (playCoinBurst)
            {
                PlayOneShot(CoinBurstAssetPath, CoinSfxVolume);
            }
        }

        private void StartSettlementIncomeRise(float durationSeconds)
        {
            StopSettlementIncomeRiseInternal();
            var safeDuration = Mathf.Max(MinSettlementIncomeRiseDuration, durationSeconds);
            settlementIncomeRiseRoutine = StartCoroutine(PlaySettlementIncomeRiseRoutine(safeDuration));
        }

        private const float MinSettlementIncomeRiseDuration = 1f;
        private const float SettlementIncomeRiseFallbackStepSeconds = 0.15f;

        private void StopSettlementIncomeRiseInternal()
        {
            if (settlementIncomeRiseRoutine != null)
            {
                StopCoroutine(settlementIncomeRiseRoutine);
                settlementIncomeRiseRoutine = null;
            }

            StopInterruptibleClip(SettlementIncomeRiseChannelKey);
            if (interruptibleSfxSources.TryGetValue(SettlementIncomeRiseChannelKey, out var source) && source != null)
            {
                source.pitch = 1f;
            }
        }

        private void EnsureInterruptibleSfxChannelActive(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            if (!source.gameObject.activeSelf)
            {
                source.gameObject.SetActive(true);
            }

            source.enabled = true;
            source.mute = false;
            source.spatialBlend = 0f;
            if (source.volume <= 0f)
            {
                source.volume = 0.95f;
            }
        }

        private IEnumerator PlaySettlementIncomeRiseRoutine(float durationSeconds)
        {
            EnsureAudioSources();
            EnsureOneShotSfxChannelActive();
            var clip = LoadAudioClip(SettlementIncomeRiseAssetPath);
            if (clip == null)
            {
                settlementIncomeRiseRoutine = null;
                yield break;
            }

            var source = GetOrCreateInterruptibleSource(SettlementIncomeRiseChannelKey);
            EnsureInterruptibleSfxChannelActive(source);
            var safeDuration = Mathf.Max(MinSettlementIncomeRiseDuration, durationSeconds);
            source.Stop();
            source.clip = clip;
            source.volume = 0.95f;
            source.loop = safeDuration > clip.length + 0.01f;
            source.pitch = SettlementIncomeRisePitchMin;
            source.time = 0f;
            source.Play();

            if (!source.isPlaying)
            {
                yield return null;
                source.Play();
            }

            if (!source.isPlaying)
            {
                yield return PlaySettlementIncomeRiseOneShotFallback(safeDuration);
                settlementIncomeRiseRoutine = null;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < safeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / safeDuration);
                source.pitch = Mathf.Lerp(SettlementIncomeRisePitchMin, SettlementIncomeRisePitchMax, t);
                yield return null;
            }

            source.Stop();
            source.clip = null;
            source.loop = false;
            source.pitch = 1f;
            settlementIncomeRiseRoutine = null;
        }

        private IEnumerator PlaySettlementIncomeRiseOneShotFallback(float durationSeconds)
        {
            var steps = Mathf.Max(1, Mathf.CeilToInt(durationSeconds / SettlementIncomeRiseFallbackStepSeconds));
            var stepDuration = durationSeconds / steps;
            for (var index = 0; index < steps; index++)
            {
                var t = steps <= 1 ? 1f : index / (float)(steps - 1);
                var pitch = Mathf.Lerp(SettlementIncomeRisePitchMin, SettlementIncomeRisePitchMax, t);
                PlayOneShot(SettlementIncomeRiseAssetPath, 0.95f, pitch);
                yield return new WaitForSecondsRealtime(stepDuration);
            }
        }

        /// <summary>
        /// 确保背景音乐与音效音源存在。
        /// </summary>
        private void EnsureAudioSources()
        {
            if (sfxSource == null || IsLegacySfxAudioSourceNode(sfxSource.transform))
            {
                sfxSource = EnsureDedicatedOneShotSfxSource();
            }

            if (pitchedSfxSource != null)
            {
                return;
            }

            var pitchedChild = transform.Find("PitchedSfxAudioSource");
            if (pitchedChild == null)
            {
                var pitchedObject = new GameObject("PitchedSfxAudioSource");
                pitchedObject.transform.SetParent(transform, false);
                pitchedChild = pitchedObject.transform;
            }

            pitchedSfxSource = pitchedChild.GetComponent<AudioSource>();
            if (pitchedSfxSource == null)
            {
                pitchedSfxSource = pitchedChild.gameObject.AddComponent<AudioSource>();
            }

            pitchedSfxSource.playOnAwake = false;
            pitchedSfxSource.loop = false;
            pitchedSfxSource.spatialBlend = 0f;
        }

        /// <summary>
        /// 获取或创建指定通道的独立音效源。
        /// </summary>
        private AudioSource GetOrCreateInterruptibleSource(string channelKey)
        {
            if (interruptibleSfxSources.TryGetValue(channelKey, out var existingSource) && existingSource != null)
            {
                return existingSource;
            }

            var child = transform.Find(channelKey);
            if (child == null)
            {
                var childObject = new GameObject(channelKey);
                childObject.transform.SetParent(transform, false);
                child = childObject.transform;
            }

            var source = child.GetComponent<AudioSource>();
            if (source == null)
            {
                source = child.gameObject.AddComponent<AudioSource>();
            }

            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            interruptibleSfxSources[channelKey] = source;
            return source;
        }

        private static string GetTableMoveChannelKey(int tableId)
        {
            return $"InterruptibleSfx_TableMove_{tableId}";
        }

        private static string GetTableCleanChannelKey(int tableId)
        {
            return $"InterruptibleSfx_TableClean_{tableId}";
        }

        private static string GetWaiterCookStealChannelKey(GameObject waiter)
        {
            return GetWaiterChannelKey("CookSteal", waiter);
        }

        private static string GetWaiterCheckoutStealChannelKey(GameObject waiter)
        {
            return GetWaiterChannelKey("CheckoutSteal", waiter);
        }

        private static string GetWaiterNapChannelKey(GameObject waiter)
        {
            return GetWaiterChannelKey("Nap", waiter);
        }

        private static string GetChefCookChannelKey(GameObject chef)
        {
            return chef == null ? string.Empty : $"InterruptibleSfx_Chef_Cook_{chef.GetInstanceID()}";
        }

        private static string GetWaiterChannelKey(string prefix, GameObject waiter)
        {
            return waiter == null ? string.Empty : $"InterruptibleSfx_Waiter_{prefix}_{waiter.GetInstanceID()}";
        }

        /// <summary>
        /// 确保已注册场景切换监听。
        /// </summary>
        private void EnsureSceneListener()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        /// <summary>
        /// 确保已注册玩法引导进度监听。
        /// </summary>
        private void EnsureGuideTaskListener()
        {
            Signals.Get<GameplayGuideProgressSignal>().RemoveListener(HandleGuideProgressChanged);
            Signals.Get<GameplayGuideProgressSignal>().AddListener(HandleGuideProgressChanged);
            CacheGuideTaskProgress();
        }

        /// <summary>
        /// 按资源路径读取音频片段。
        /// </summary>
        /// <param name="assetPath">Unity 资源路径。</param>
        /// <returns>读取到的音频片段；失败时返回 null。</returns>
        private static AudioClip LoadAudioClip(string assetPath)
        {
            return GameplayResourceStore.LoadAsset<AudioClip>(assetPath);
        }
    }

    /// <summary>
    /// 统一管理 Town / Tavern 的场景切换与 HUD 显隐，避免入口散落在多个 UI 和世界对象中。
    /// </summary>
    public static class SceneFlowCoordinator
    {
        private const string TownSceneName = "Town";
        private const string TavernSceneName = "GamePlay_TavernWJ";
        private const string TavernSecondFloorSceneName = "GamePlay_Tavern2WJ";

        /// <summary>
        /// 上下楼切场景会复用一楼营业快照；恢复时不应走「离线回来全体打盹」。
        /// </summary>
        private static bool skipForceWaiterNapOnNextSnapshotRestore;

        /// <summary>
        /// 从城镇回自家酒楼时，需要按桌掷「被拉客」提示（含拜访拉客后卸客回来）。
        /// </summary>
        private static bool pendingOwnTavernPulledTipRoll;

        /// <summary>标记下一次快照恢复跳过强制打盹（仅上下楼）。</summary>
        public static void RequestSkipForceWaiterNapOnNextSnapshotRestore()
        {
            skipForceWaiterNapOnNextSnapshotRestore = true;
        }

        /// <summary>取消跳过标记（回城 / 拜访等离开时调用，避免误带到下次进店）。</summary>
        public static void ClearSkipForceWaiterNapOnNextSnapshotRestore()
        {
            skipForceWaiterNapOnNextSnapshotRestore = false;
        }

        /// <summary>消费「跳过强制打盹」标记；回城后再进店仍会打盹。</summary>
        public static bool ConsumeSkipForceWaiterNapOnNextSnapshotRestore()
        {
            var skip = skipForceWaiterNapOnNextSnapshotRestore;
            skipForceWaiterNapOnNextSnapshotRestore = false;
            return skip;
        }

        /// <summary>标记：下次进入自家酒楼需掷被拉客提示（从城镇回来，含拜访卸客）。</summary>
        public static void RequestOwnTavernPulledTipRoll()
        {
            pendingOwnTavernPulledTipRoll = true;
        }

        /// <summary>取消被拉客提示掷骰标记。</summary>
        public static void ClearOwnTavernPulledTipRoll()
        {
            pendingOwnTavernPulledTipRoll = false;
        }

        /// <summary>消费被拉客提示掷骰标记。</summary>
        public static bool ConsumeOwnTavernPulledTipRoll()
        {
            var pending = pendingOwnTavernPulledTipRoll;
            pendingOwnTavernPulledTipRoll = false;
            return pending;
        }

        public static IEnumerator EnterTown(Action onLoaded = null)
        {
            // 回城再进店仍要打盹；清掉上下楼留下的跳过标记。
            ClearSkipForceWaiterNapOnNextSnapshotRestore();

            // 从自家或拜访回城后，下次进自家店掷被拉客提示（含拉客卸客回来）；上下楼仍 Clear。
            var leavingTavernScene = TavernSceneManager.Instance != null;
            if (leavingTavernScene)
            {
                RequestOwnTavernPulledTipRoll();
            }
            else
            {
                ClearOwnTavernPulledTipRoll();
            }

            // 拜访回城：Halt 后写入他人店桌快照；自家营业回城：停协程后写自家快照。
            PrepareTavernSceneLeaveForTown();

            // 必须等城镇场景加载完成后再结束访客会话，否则酒楼 Update 会短暂把开业按钮刷出来。
            yield return LoadSceneAsync(TownSceneName, () =>
            {
                DataManager.Instance?.EndVisitOtherTavern();
                onLoaded?.Invoke();
            });
        }

        public static IEnumerator EnterTavern(int tileId, int buildingLevel, Action onLoaded = null)
        {
            // 进自家店前先结束访客：若等 SyncHud 之后再清，顶栏会按访客隐藏金币且不再刷新。
            // 此时已在城镇场景，提前结束不会闪酒楼开业按钮。
            DataManager.Instance?.EndVisitOtherTavern();
            DataManager.Instance?.NotifyEnteredOwnTavernSessionTurn();
            DataManager.Instance.SetActiveOwnedBuilding(tileId, buildingLevel);
            yield return LoadSceneAsync(TavernSceneName, onLoaded);
        }

        /// <summary>
        /// 自家三星上楼：先停协程并写一楼快照，再进二楼场景。
        /// 注意：Prepare 会 Halt TavernSceneManager 全部协程，因此实际切场景必须由 GameManager 承载。
        /// </summary>
        public static IEnumerator EnterTavernSecondFloor(Action onLoaded = null)
        {
            if (DataManager.Instance != null && DataManager.Instance.IsVisitingOtherTavern)
            {
                yield break;
            }

            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                // 若调用方是 TavernSceneManager.StartCoroutine，随后 Halt 会掐断本协程；先转到 GameManager。
                gameManager.StartCoroutine(EnterTavernSecondFloorRoutine(onLoaded));
                yield break;
            }

            yield return EnterTavernSecondFloorRoutine(onLoaded);
        }

        private static IEnumerator EnterTavernSecondFloorRoutine(Action onLoaded)
        {
            // 已确定上楼：销毁上楼/拉客场景按钮后再 Halt / 切场景。
            HudOverlayService.ClearUpStairButtons();
            HudOverlayService.ClearMyDrumUpButton();
            var shouldShowUnlockMenu = DataManager.Instance != null
                                       && !DataManager.Instance.IsVisitingOtherTavern
                                       && !DataManager.Instance.IsTavernMenuEntryUnlocked();
            // 上下楼只暂停一楼会话，回来时不要强制全体小二打盹。
            RequestSkipForceWaiterNapOnNextSnapshotRestore();
            ClearOwnTavernPulledTipRoll();
            PrepareTavernSceneLeaveForTown();
            yield return LoadSceneAsync(TavernSecondFloorSceneName, onLoaded);
            // SyncHudForScene 内也会启动会话；此处保留兜底。
            BeginSecondFloorVipSessionIfNeeded();
            if (shouldShowUnlockMenu)
            {
                var host = GameManager.Instance;
                if (host != null)
                {
                    host.StartCoroutine(HudOverlayService.DeferredTryShowUnlockMenuDialog());
                }
            }
        }

        /// <summary>
        /// 二楼下楼回一楼（保留营业会话与快照恢复）。
        /// </summary>
        public static IEnumerator EnterTavernFirstFloorFromSecond(Action onLoaded = null)
        {
            // 切场景前销毁二楼会话生成的小二等运行时对象，并停掉一楼残留的打盹循环音。
            GameAudioManager.StopAllInterruptibleSfx();
            TavernSecondFloorVipSessionController.CleanupBeforeLeaveFirstFloor();
            // 与上楼成对：确保下楼恢复快照时跳过强制打盹。
            RequestSkipForceWaiterNapOnNextSnapshotRestore();
            ClearOwnTavernPulledTipRoll();
            yield return LoadSceneAsync(TavernSceneName, onLoaded);
        }

        /// <summary>
        /// 访问他人酒楼：加载与自家相同的酒楼场景副本，不改写自家 activeShop 存档。
        /// </summary>
        public static IEnumerator EnterOtherTavernVisit(int tileId, int buildingLevel, string shopName, Action onLoaded = null)
        {
            if (DataManager.Instance == null || !DataManager.Instance.CanVisitOtherTavern())
            {
                HudOverlayService.ShowFloatingWarning("二星酒楼解锁");
                yield break;
            }

            // 若当前仍在自家店场景，先停协程+Capture 再进入拜访会话。
            ClearSkipForceWaiterNapOnNextSnapshotRestore();
            PrepareTavernSceneLeaveForTown();
            DataManager.Instance.BeginVisitOtherTavern(tileId, shopName, buildingLevel);
            yield return LoadSceneAsync(TavernSceneName, onLoaded);
        }

        /// <summary>
        /// 离开酒楼场景前：先中断全部运行时协程，再按需写入快照（避免 Continue failure）。
        /// </summary>
        private static void PrepareTavernSceneLeaveForTown()
        {
            // 打盹/做菜等循环音挂在 GameAudioManager，场景卸载不会停，必须先掐掉。
            GameAudioManager.StopAllInterruptibleSfx();

            var tavern = TavernSceneManager.Instance;
            // Unity 销毁后 Instance 可能仍是 C# 非 null，需用 == null 走假空。
            if (tavern == null)
            {
                return;
            }

            // 必须先 Halt 再 Capture：采样期间不再有协程改桌态/顾客。
            tavern.HaltAllRuntimeCoroutinesForSceneLeave();
            if (DataManager.IsInOtherTavernVisitSession)
            {
                tavern.CaptureOtherTavernVisitSnapshot();
            }
            else
            {
                tavern.CaptureOwnTavernRuntimeSnapshot();
            }
        }

        public static IEnumerator LoadSceneAsync(string sceneName, Action onLoaded = null)
        {
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            if (asyncOperation == null)
            {
                yield break;
            }

            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            SyncHudForScene(sceneName);
            onLoaded?.Invoke();
        }

        public static void SyncHudForScene(string sceneName)
        {
            if (IsTownScene(sceneName))
            {
                if (UIKit.GetPanel<TownStatusBarPanelController>() == null)
                {
                    UIKit.OpenPanel<TownStatusBarPanelController>(
                        JiangNanUIPanelLayerConfig.Resolve<TownStatusBarPanelController>());
                }

                UIKit.ClosePanel<TavernStatusBarPanelController>();
                UIKit.ClosePanel<StartOpeningWindowController>();
                return;
            }

            if (!IsTavernScene(sceneName))
            {
                return;
            }

            UIKit.ClosePanel<TownStatusBarPanelController>();

            if (UIKit.GetPanel<TavernStatusBarPanelController>() == null)
            {
                UIKit.OpenPanel<TavernStatusBarPanelController>(
                    JiangNanUIPanelLayerConfig.Resolve<TavernStatusBarPanelController>());
            }

            UIKit.ClosePanel<StartOpeningWindowController>();

            // 二楼：清上楼/拉客场景按钮；刷新桌子/戏台购买；有包厢贵客则启动厨工/六次结账会话。
            if (IsTavernSecondFloorSceneName(sceneName))
            {
                HudOverlayService.ClearUpStairButtons();
                HudOverlayService.ClearMyDrumUpButton();
                TavernSecondFloorFacilityPurchaseController.FindOrCreate().RefreshAll();
                BeginSecondFloorVipSessionIfNeeded();
            }
            else
            {
                TavernSecondFloorVipService.ClearSpawnedVipRuntime();
            }

            UIKit.GetPanel<TavernStatusBarPanelController>()?.RefreshAllPanels();
        }

        /// <summary>
        /// 二楼包厢：生成落座贵客并启动串行厨工会话。
        /// </summary>
        private static void BeginSecondFloorVipSessionIfNeeded()
        {
            if (!TavernSecondFloorVipService.HasSecondFloorVipGuest())
            {
                return;
            }

            var session = TavernSecondFloorVipSessionController.FindOrCreate();
            session.TryBeginSession();
        }

        private static bool IsTownScene(string sceneName)
        {
            return string.Equals(sceneName, TownSceneName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsTavernScene(string sceneName)
        {
            return IsTavernGameplaySceneName(sceneName);
        }

        /// <summary>
        /// 是否为酒馆玩法场景（含旧场景名与二楼兼容）。
        /// </summary>
        public static bool IsTavernGameplaySceneName(string sceneName)
        {
            return string.Equals(sceneName, TavernSceneName, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(sceneName, "GamePlay_Tavern", StringComparison.OrdinalIgnoreCase)
                   || IsTavernSecondFloorSceneName(sceneName);
        }

        /// <summary>是否为酒楼二楼场景。</summary>
        public static bool IsTavernSecondFloorSceneName(string sceneName)
        {
            return string.Equals(sceneName, TavernSecondFloorSceneName, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsOnTavernSecondFloor()
        {
            return IsTavernSecondFloorSceneName(SceneManager.GetActiveScene().name);
        }
    }

    /// <summary>
    /// 统一管理营业期的价格、客流与服务速度修正，替代零散的临时倍率补丁。
    /// </summary>
    public sealed class TavernBusinessModifierService
    {
        public const string PriceIncreaseSource = "price_increase";
        public const string InspireSource = "inspire";
        public const string SpeedUpButtonSource = "speed_up_button";
        public const string WaiterWakeBoostSource = "waiter_wake_boost";

        private readonly Dictionary<string, float> customerFlowModifiers = new();
        private readonly Dictionary<string, float> priceModifiers = new();
        private readonly Dictionary<string, float> serviceSpeedModifiers = new();
        private readonly Dictionary<string, Coroutine> timedServiceSpeedCoroutines = new();
        /// <summary>与协程同宿主；Stop 必须落在 Start 的那个 MonoBehaviour 上。</summary>
        private readonly Dictionary<string, MonoBehaviour> timedServiceSpeedOwners = new();

        public static TavernBusinessModifierService Instance { get; } = new();

        public void SetCustomerFlowModifier(string source, float coefficient)
        {
            SetModifier(customerFlowModifiers, source, coefficient);
            ApplySceneModifiers();
        }

        public void ClearCustomerFlowModifier(string source)
        {
            ClearModifier(customerFlowModifiers, source);
            ApplySceneModifiers();
        }

        public void SetPriceModifier(string source, float coefficient)
        {
            SetModifier(priceModifiers, source, coefficient);
            ApplySceneModifiers();
        }

        public void ClearPriceModifier(string source)
        {
            ClearModifier(priceModifiers, source);
            ApplySceneModifiers();
        }

        public void SetServiceSpeedModifier(string source, float coefficient)
        {
            SetModifier(serviceSpeedModifiers, source, coefficient);
            ApplySceneModifiers();
        }

        public void ClearServiceSpeedModifier(string source)
        {
            ClearTimedServiceSpeedModifier(source);
            ClearModifier(serviceSpeedModifiers, source);
            ApplySceneModifiers();
        }

        public void ApplyTimedServiceSpeedModifier(MonoBehaviour owner, string source, float coefficient, float durationSeconds)
        {
            if (owner == null)
            {
                return;
            }

            ClearTimedServiceSpeedModifier(source);
            SetModifier(serviceSpeedModifiers, source, coefficient);
            timedServiceSpeedOwners[source] = owner;
            timedServiceSpeedCoroutines[source] = owner.StartCoroutine(ClearServiceSpeedModifierAfterDelay(source, durationSeconds));
            ApplySceneModifiers();
        }

        public void ResetAll()
        {
            foreach (var pair in timedServiceSpeedCoroutines)
            {
                StopTimedServiceSpeedCoroutine(pair.Key, pair.Value);
            }

            timedServiceSpeedCoroutines.Clear();
            timedServiceSpeedOwners.Clear();
            customerFlowModifiers.Clear();
            priceModifiers.Clear();
            serviceSpeedModifiers.Clear();
            ApplySceneModifiers();
        }

        public void ApplySceneModifiers()
        {
            if (TavernSceneManager.Instance == null)
            {
                return;
            }

            TavernSceneManager.Instance.SetBusinessAdjustment(
                GetEffectiveCoefficient(customerFlowModifiers),
                GetEffectiveCoefficient(priceModifiers));
            TavernSceneManager.Instance.SetServiceSpeedCoefficient(GetEffectiveCoefficient(serviceSpeedModifiers));
        }

        private IEnumerator ClearServiceSpeedModifierAfterDelay(string source, float durationSeconds)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, durationSeconds));
            timedServiceSpeedCoroutines.Remove(source);
            timedServiceSpeedOwners.Remove(source);
            ClearModifier(serviceSpeedModifiers, source);
            ApplySceneModifiers();
        }

        private void ClearTimedServiceSpeedModifier(string source)
        {
            if (!timedServiceSpeedCoroutines.TryGetValue(source, out var coroutine))
            {
                timedServiceSpeedOwners.Remove(source);
                return;
            }

            timedServiceSpeedCoroutines.Remove(source);
            StopTimedServiceSpeedCoroutine(source, coroutine);
            timedServiceSpeedOwners.Remove(source);
        }

        private void StopTimedServiceSpeedCoroutine(string source, Coroutine coroutine)
        {
            if (coroutine == null)
            {
                return;
            }

            // 必须在 StartCoroutine 的同一 MonoBehaviour 上 Stop，否则会 Coroutine continue failure。
            if (timedServiceSpeedOwners.TryGetValue(source, out var owner) && owner != null)
            {
                owner.StopCoroutine(coroutine);
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StopCoroutine(coroutine);
            }
        }

        private static void SetModifier(IDictionary<string, float> modifiers, string source, float coefficient)
        {
            if (modifiers == null || string.IsNullOrWhiteSpace(source))
            {
                return;
            }

            if (coefficient <= 1f)
            {
                modifiers.Remove(source);
                return;
            }

            modifiers[source] = coefficient;
        }

        private static void ClearModifier(IDictionary<string, float> modifiers, string source)
        {
            if (modifiers == null || string.IsNullOrWhiteSpace(source))
            {
                return;
            }

            modifiers.Remove(source);
        }

        private static float GetEffectiveCoefficient(Dictionary<string, float> modifiers)
        {
            if (modifiers == null || modifiers.Count == 0)
            {
                return 1f;
            }

            var coefficient = 1f;
            foreach (var pair in modifiers)
            {
                coefficient = Mathf.Max(coefficient, pair.Value);
            }

            return coefficient;
        }
    }
}
