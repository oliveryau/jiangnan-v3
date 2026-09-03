using System.Collections;
using JN.Client;
using JN.Client.Manager;
using JN.Client.UI;
using UnityEngine;

namespace JN.Client.Scene
{
    /// <summary>
    /// 二楼贵客收钱表现：飞钱动画 + 收钱音效，与结账按钮/入账逻辑解耦。
    /// 调用方在 <see cref="PlayRoutine"/> 结束后再自行加钱。
    /// </summary>
    public static class SecondFloorVipCoinCollectionPresenter
    {
        public enum Profile
        {
            /// <summary>单道菜自动结账：一次飞钱 + 收钱音效。</summary>
            PerDish,

            /// <summary>六道吃完最终结账：大量金币依次慢出、慢飞向顶栏。</summary>
            FinalCheckout,

            /// <summary>小费结账每次点击：固定 40 枚金币。</summary>
            TipCheckoutClick
        }

        private const float PerDishFlyWaitSeconds = 0.75f;
        private const float FinalCheckoutIncomeSfxInterval = 0.95f;
        private const int PerDishCoinCount = 8;
        private const int FinalCheckoutCoinCount = 200;
        private const int TipCheckoutClickCoinCount = 40;
        private const float WorldSourceYOffset = 0.35f;

        /// <summary>默认从二楼贵客根节点起飞；无贵客时返回 null。</summary>
        public static Transform ResolveDefaultFlySource()
        {
            return TavernSecondFloorVipService.SpawnedVipRoot != null
                ? TavernSecondFloorVipService.SpawnedVipRoot.transform
                : null;
        }

        /// <summary>
        /// 播放飞钱与收钱音效（不含入账）。
        /// </summary>
        public static IEnumerator PlayRoutine(Transform worldSource, Profile profile)
        {
            switch (profile)
            {
                case Profile.PerDish:
                    yield return PlayPerDishRoutine(worldSource);
                    yield break;
                case Profile.FinalCheckout:
                    yield return PlayFinalCheckoutRoutine(worldSource);
                    yield break;
                case Profile.TipCheckoutClick:
                    PlayTipCheckoutClick(worldSource);
                    yield break;
            }
        }

        /// <summary>
        /// 立即播放一次飞钱与收钱音效（无等待，适合与其它流程并行）。
        /// </summary>
        public static void PlayInstant(Transform worldSource, Profile profile = Profile.PerDish)
        {
            PlayCheckoutSounds(includeVipCoinLayer: profile == Profile.FinalCheckout);
            PlayCoinBurst(
                worldSource,
                coinCount: ResolveCoinCount(profile),
                timing: profile == Profile.FinalCheckout ? GameUIEffects.CoinFlyTiming.FinalCheckout : null);
        }

        /// <summary>小费结账每次点击：立刻飞 40 枚金币。</summary>
        public static void PlayTipCheckoutClick(Transform worldSource)
        {
            PlayCheckoutSounds(includeVipCoinLayer: true);
            PlayCoinBurst(worldSource, coinCount: TipCheckoutClickCoinCount);
        }

        private static int ResolveCoinCount(Profile profile)
        {
            return profile switch
            {
                Profile.FinalCheckout => FinalCheckoutCoinCount,
                Profile.TipCheckoutClick => TipCheckoutClickCoinCount,
                _ => PerDishCoinCount
            };
        }

        private static IEnumerator PlayPerDishRoutine(Transform worldSource)
        {
            CoinDisplayRefreshCoordinator.DeferGoldRefreshUntilFlyComplete();
            PlayCheckoutSounds(includeVipCoinLayer: true);
            PlayCoinBurst(worldSource, coinCount: PerDishCoinCount);
            yield return new WaitForSeconds(PerDishFlyWaitSeconds);
            CoinDisplayRefreshCoordinator.NotifyFlyComplete();
        }

        private static IEnumerator PlayFinalCheckoutRoutine(Transform worldSource)
        {
            CoinDisplayRefreshCoordinator.DeferGoldRefreshUntilFlyComplete();
            PlayCheckoutSounds(includeVipCoinLayer: true);

            var flyCompleted = false;
            PlayCoinBurst(
                worldSource,
                coinCount: FinalCheckoutCoinCount,
                timing: GameUIEffects.CoinFlyTiming.FinalCheckout,
                onAllCoinsSettled: () => flyCompleted = true);

            var elapsed = 0f;
            var nextIncomeSfxAt = FinalCheckoutIncomeSfxInterval;
            while (!flyCompleted)
            {
                elapsed += Time.deltaTime;
                if (elapsed >= nextIncomeSfxAt)
                {
                    GameAudioManager.PlayVipCheckoutCoins();
                    nextIncomeSfxAt += FinalCheckoutIncomeSfxInterval;
                }

                yield return null;
            }

            CoinDisplayRefreshCoordinator.NotifyFlyComplete();
        }

        private static void PlayCheckoutSounds(bool includeVipCoinLayer)
        {
            GameAudioManager.PlayCheckoutCoins();
            if (includeVipCoinLayer)
            {
                GameAudioManager.PlayVipCheckoutCoins();
            }
        }

        /// <summary>世界坐标投影到屏幕后飞金币，避免把世界坐标当成屏幕坐标。</summary>
        private static void PlayCoinBurst(
            Transform worldSource,
            Transform coinTarget = null,
            int coinCount = PerDishCoinCount,
            GameUIEffects.CoinFlyTiming? timing = null,
            System.Action onAllCoinsSettled = null)
        {
            coinTarget ??= TavernTopStatusPanelController.ResolveCoinFlyTarget();
            if (coinTarget == null)
            {
                onAllCoinsSettled?.Invoke();
                return;
            }

            if (worldSource != null)
            {
                var camera = Camera.main;
                if (camera != null)
                {
                    var screen = camera.WorldToScreenPoint(worldSource.position + Vector3.up * WorldSourceYOffset);
                    if (screen.z > 0f)
                    {
                        GameUIEffects.PlayCoinsFlyFromScreen(
                            screen,
                            coinTarget,
                            coinCount,
                            onAllCoinsSettled,
                            timing);
                        return;
                    }
                }

                GameUIEffects.PlayCoinsFly(worldSource, coinTarget, onAllCoinsSettled);
                return;
            }

            GameUIEffects.PlayCoinsFlyFromRandomScreen(coinTarget, coinCount, onAllCoinsSettled);
        }
    }
}
