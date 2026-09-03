using DG.Tweening;
using JN.Client.Manager;
using UnityEngine;
using UnityEngine.UI;
using Action = System.Action;

namespace JN.Client
{
    /// <summary>
    /// 负责游戏特效相关的运行时逻辑。
    /// </summary>
    public static class GameUIEffects
    {
        private const float CoinsHeadLiftOffset = 110f;
        private const float CoinsHeadScatterMinRadius = 58f;
        private const float CoinsHeadScatterMaxRadius = 185f;
        private const float CoinsHeadScatterArcDegrees = 165f;
        private const float CoinsMinHeadScatterDuration = 0.1f;
        private const float CoinsMaxHeadScatterDuration = 0.2f;
        /// <summary>散开后停留时长；收钱飞金币不停留，直接飞向顶部。</summary>
        private const float CoinsHeadHoldDuration = 0f;
        private const float CoinsBurstOnlyFadeDuration = 0.22f;
        private const float CoinsMinFlyDuration = 0.55f;
        private const float CoinsMaxFlyDuration = 0.82f;
        private const float CoinsMaxZRotation = 25f;
        private const float CoinsStartScale = 1f;
        private const float CoinsScatterScale = 1.25f;
        private const float CoinsEndScale = 0.4f;
        private const float MaxTrailDelay = 0.38f;
        private const int CoinCount = 8;
        private const int PrestigeIconCount = 6;
        private const string CoinPrefabPath = "Assets/Res/Resources/UI/Item/CoinItem.prefab";
        private const string PrestigeSpritePath = "Assets/Res/Resources/Textures/UI/Common/presitige.png";

        /// <summary>金币散开/飞行节奏；最终结账用慢速依次出现 + 慢飞。</summary>
        public readonly struct CoinFlyTiming
        {
            public float MinScatterDuration { get; }
            public float MaxScatterDuration { get; }
            public float MinFlyDuration { get; }
            public float MaxFlyDuration { get; }
            public float MaxStartDelaySpread { get; }
            public float HeadHoldDuration { get; }
            public float ScatterMinRadius { get; }
            public float ScatterMaxRadius { get; }
            public float ScatterArcDegrees { get; }
            public float ScatterJitter { get; }

            public CoinFlyTiming(
                float minScatterDuration,
                float maxScatterDuration,
                float minFlyDuration,
                float maxFlyDuration,
                float maxStartDelaySpread,
                float headHoldDuration,
                float scatterMinRadius,
                float scatterMaxRadius,
                float scatterArcDegrees,
                float scatterJitter)
            {
                MinScatterDuration = minScatterDuration;
                MaxScatterDuration = maxScatterDuration;
                MinFlyDuration = minFlyDuration;
                MaxFlyDuration = maxFlyDuration;
                MaxStartDelaySpread = maxStartDelaySpread;
                HeadHoldDuration = headHoldDuration;
                ScatterMinRadius = scatterMinRadius;
                ScatterMaxRadius = scatterMaxRadius;
                ScatterArcDegrees = scatterArcDegrees;
                ScatterJitter = scatterJitter;
            }

            public static CoinFlyTiming Standard { get; } = new(
                CoinsMinHeadScatterDuration,
                CoinsMaxHeadScatterDuration,
                CoinsMinFlyDuration,
                CoinsMaxFlyDuration,
                MaxTrailDelay,
                CoinsHeadHoldDuration,
                CoinsHeadScatterMinRadius,
                CoinsHeadScatterMaxRadius,
                CoinsHeadScatterArcDegrees,
                scatterJitter: 0f);

            /// <summary>二楼贵客最终结账：金币依次慢出、散得更开、慢飞向顶栏。</summary>
            public static CoinFlyTiming FinalCheckout { get; } = new(
                minScatterDuration: 0.22f,
                maxScatterDuration: 0.38f,
                minFlyDuration: 1.05f,
                maxFlyDuration: 1.5f,
                maxStartDelaySpread: 3.2f,
                headHoldDuration: 0.1f,
                scatterMinRadius: 110f,
                scatterMaxRadius: 280f,
                scatterArcDegrees: 210f,
                scatterJitter: 70f);
        }

        private static GameObject coinPrefab;
        private static Sprite prestigeSprite;
        private static Sprite coinDefaultSprite;

        /// <summary>
        /// 播放铜钱爆开动画（仅头顶发散，不飞向目标）。
        /// </summary>
        /// <param name="start">起点对象。</param>
        public static void PlayCoinsBurst(Transform start)
        {
            if (!TryResolveParentCanvas(start, out var parent))
            {
                return;
            }

            var fromScreen = start.position;
            EnsureCoinPrefabLoaded();
            if (coinPrefab == null || parent == null)
            {
                return;
            }

            for (var i = 0; i < CoinCount; i++)
            {
                if (!TryAllocateCoin(parent, out var coinGo, out var rt, out var cg))
                {
                    continue;
                }

                var scatterDuration = Random.Range(CoinsMinHeadScatterDuration, CoinsMaxHeadScatterDuration);
                var scatterPos = ResolveHeadScatterPosition(fromScreen, i, CoinCount);
                var startDelay = ResolveCoinStartDelay(i, CoinCount);

                PrepareCoin(rt, cg, fromScreen);

                var seq = DOTween.Sequence();
                seq.PrependInterval(startDelay);
                seq.AppendCallback(() => cg.alpha = 1f);
                AppendCoinsHeadScatter(seq, rt, scatterPos, scatterDuration);
                seq.Join(cg.DOFade(0f, CoinsBurstOnlyFadeDuration).SetDelay(scatterDuration));
                seq.OnComplete(() => RecycleCoin(coinGo));
            }
        }

        /// <summary>
        /// 播放铜钱飞行动画：散到头顶后直接飞向目标金币栏（不停留）。
        /// </summary>
        /// <param name="start">起点对象。</param>
        /// <param name="target">目标对象。</param>
        /// <param name="onAllCoinsSettled">全部金币飞抵目标后的回调（取最慢一枚）。</param>
        public static void PlayCoinsFly(Transform start, Transform target, Action onAllCoinsSettled = null)
        {
            PlayCoinsScatterThenFly(start, target, CoinsHeadHoldDuration, onAllCoinsSettled);
        }

        /// <summary>
        /// 飞声望图标：散开后飞向顶部 group_presitige。
        /// </summary>
        /// <param name="start">起点（UI 或世界物体）；为空则屏幕随机起点。</param>
        /// <param name="target">声望栏目标。</param>
        /// <param name="onAllSettled">全部飞抵后的回调。</param>
        public static void PlayPrestigeFly(Transform start, Transform target, Action onAllSettled = null)
        {
            if (target == null)
            {
                onAllSettled?.Invoke();
                return;
            }

            if (start == null)
            {
                PlayPrestigeFlyFromScreen(ResolveRandomScreenPosition(), target, onAllSettled);
                return;
            }

            PlayPrestigeFlyFromScreen(ResolveFlyStartScreenPosition(start), target, onAllSettled);
        }

        /// <summary>
        /// 从屏幕坐标起点飞声望图标到目标。
        /// </summary>
        public static void PlayPrestigeFlyFromScreen(Vector3 screenPosition, Transform target, Action onAllSettled = null)
        {
            PlayIconScatterThenFlyFromScreen(
                screenPosition,
                target,
                CoinsHeadHoldDuration,
                PrestigeIconCount,
                usePrestigeSprite: true,
                onAllSettled);
        }

        /// <summary>
        /// 从屏幕坐标起点播放铜钱飞行动画（用于世界空间 UI 经相机投影后的起点）。
        /// </summary>
        public static void PlayCoinsFlyFromScreen(Vector3 screenPosition, Transform target, Action onAllCoinsSettled = null)
        {
            PlayCoinsFlyFromScreen(screenPosition, target, CoinCount, onAllCoinsSettled);
        }

        /// <summary>
        /// 从屏幕坐标起点播放指定数量的铜钱飞行动画。
        /// </summary>
        public static void PlayCoinsFlyFromScreen(
            Vector3 screenPosition,
            Transform target,
            int coinCount,
            Action onAllCoinsSettled = null,
            CoinFlyTiming? timing = null)
        {
            var resolvedTiming = timing ?? CoinFlyTiming.Standard;
            PlayCoinsScatterThenFlyFromScreen(
                screenPosition,
                target,
                resolvedTiming.HeadHoldDuration,
                coinCount,
                onAllCoinsSettled,
                resolvedTiming);
        }

        /// <summary>
        /// 从屏幕随机位置生成多枚金币，飞向目标 UI 点后消失。
        /// </summary>
        public static void PlayCoinsFlyFromRandomScreen(Transform target, Action onAllCoinsSettled = null)
        {
            PlayCoinsFlyFromRandomScreen(target, CoinCount, onAllCoinsSettled);
        }

        /// <summary>
        /// 从屏幕随机位置生成指定数量金币，飞向目标 UI 点后消失。
        /// </summary>
        public static void PlayCoinsFlyFromRandomScreen(Transform target, int coinCount, Action onAllCoinsSettled = null)
        {
            if (!TryResolveFlyTargetContext(target, out var parent, out var toScreen))
            {
                onAllCoinsSettled?.Invoke();
                return;
            }

            EnsureCoinPrefabLoaded();
            if (coinPrefab == null || parent == null)
            {
                onAllCoinsSettled?.Invoke();
                return;
            }

            var spawnedCount = 0;
            var settledCount = 0;
            Action notifyOneSettled = null;
            notifyOneSettled = () =>
            {
                settledCount++;
                if (settledCount >= spawnedCount)
                {
                    onAllCoinsSettled?.Invoke();
                }
            };

            var safeCount = Mathf.Max(1, coinCount);
            for (var i = 0; i < safeCount; i++)
            {
                if (!TryAllocateCoin(parent, out var coinGo, out var rt, out var cg))
                {
                    continue;
                }

                spawnedCount++;
                var fromScreen = ResolveRandomScreenPosition();
                var flyDuration = Random.Range(CoinsMinFlyDuration, CoinsMaxFlyDuration);
                var startDelay = ResolveCoinStartDelay(i, safeCount);

                PrepareCoin(rt, cg, fromScreen);

                var seq = DOTween.Sequence();
                seq.PrependInterval(startDelay);
                seq.AppendCallback(() => cg.alpha = 1f);
                AppendCoinsFly(seq, rt, cg, toScreen, flyDuration);
                seq.OnComplete(() =>
                {
                    RecycleCoin(coinGo);
                    notifyOneSettled();
                });
            }

            if (spawnedCount <= 0)
            {
                onAllCoinsSettled?.Invoke();
            }
        }

        /// <summary>
        /// 读取 Transform 可视中心（优先 RectTransform 世界四角中心）。
        /// </summary>
        public static Vector3 GetTransformWorldCenter(Transform transform)
        {
            return ResolveTransformWorldCenter(transform);
        }

        /// <summary>
        /// 播放铜钱先头顶发散、再飞向目标的组合动画。
        /// </summary>
        /// <param name="start">起点对象。</param>
        /// <param name="target">目标对象。</param>
        /// <param name="headHoldDuration">散到头顶后的停留秒数。</param>
        /// <param name="onAllCoinsSettled">全部金币飞抵目标后的回调。</param>
        public static void PlayCoinsBurstThenFly(
            Transform start,
            Transform target,
            float headHoldDuration = CoinsHeadHoldDuration,
            Action onAllCoinsSettled = null)
        {
            PlayCoinsScatterThenFly(start, target, headHoldDuration, onAllCoinsSettled);
        }

        private static void PlayCoinsScatterThenFly(
            Transform start,
            Transform target,
            float headHoldDuration,
            Action onAllCoinsSettled = null)
        {
            if (start == null)
            {
                onAllCoinsSettled?.Invoke();
                return;
            }

            PlayCoinsScatterThenFlyFromScreen(
                ResolveTransformWorldCenter(start),
                target,
                headHoldDuration,
                onAllCoinsSettled);
        }

        private static void PlayCoinsScatterThenFlyFromScreen(
            Vector3 fromScreen,
            Transform target,
            float headHoldDuration,
            Action onAllCoinsSettled = null)
        {
            PlayCoinsScatterThenFlyFromScreen(fromScreen, target, headHoldDuration, CoinCount, onAllCoinsSettled);
        }

        private static void PlayCoinsScatterThenFlyFromScreen(
            Vector3 fromScreen,
            Transform target,
            float headHoldDuration,
            int coinCount,
            Action onAllCoinsSettled = null,
            CoinFlyTiming? timing = null)
        {
            PlayIconScatterThenFlyFromScreen(
                fromScreen,
                target,
                headHoldDuration,
                coinCount,
                usePrestigeSprite: false,
                onAllCoinsSettled,
                timing);
        }

        /// <summary>
        /// 通用散开→停留→飞向目标（金币 / 声望共用轨迹）。
        /// </summary>
        private static void PlayIconScatterThenFlyFromScreen(
            Vector3 fromScreen,
            Transform target,
            float headHoldDuration,
            int iconCount,
            bool usePrestigeSprite,
            Action onAllSettled = null,
            CoinFlyTiming? timing = null)
        {
            if (!TryResolveFlyTargetContext(target, out var parent, out var toScreen))
            {
                onAllSettled?.Invoke();
                return;
            }

            EnsureCoinPrefabLoaded();
            if (usePrestigeSprite)
            {
                EnsurePrestigeSpriteLoaded();
            }

            if (coinPrefab == null || parent == null)
            {
                onAllSettled?.Invoke();
                return;
            }

            var safeCount = Mathf.Max(1, iconCount);
            var resolvedTiming = timing ?? CoinFlyTiming.Standard;
            var spawnedCount = 0;
            var settledCount = 0;
            Action notifyOneSettled = null;
            notifyOneSettled = () =>
            {
                settledCount++;
                if (settledCount >= spawnedCount)
                {
                    onAllSettled?.Invoke();
                }
            };

            for (var i = 0; i < safeCount; i++)
            {
                if (!TryAllocateCoin(parent, out var coinGo, out var rt, out var cg))
                {
                    continue;
                }

                if (usePrestigeSprite)
                {
                    ApplyPrestigeAppearance(coinGo);
                }

                spawnedCount++;
                var scatterDuration = Random.Range(
                    resolvedTiming.MinScatterDuration,
                    resolvedTiming.MaxScatterDuration);
                var flyDuration = Random.Range(resolvedTiming.MinFlyDuration, resolvedTiming.MaxFlyDuration);
                var scatterPos = ResolveHeadScatterPosition(fromScreen, i, safeCount, resolvedTiming);
                var startDelay = ResolveCoinStartDelay(i, safeCount, resolvedTiming.MaxStartDelaySpread);

                PrepareCoin(rt, cg, fromScreen);

                var seq = DOTween.Sequence();
                seq.PrependInterval(startDelay);
                seq.AppendCallback(() => cg.alpha = 1f);
                AppendCoinsHeadScatter(seq, rt, scatterPos, scatterDuration);
                seq.AppendInterval(Mathf.Max(0f, headHoldDuration));
                AppendCoinsFly(seq, rt, cg, toScreen, flyDuration);
                seq.OnComplete(() =>
                {
                    RecycleCoin(coinGo);
                    notifyOneSettled();
                });
            }

            if (spawnedCount <= 0)
            {
                onAllSettled?.Invoke();
            }
        }

        /// <summary>
        /// 解析飞行起点屏幕坐标：UI 节点用中心；世界物体经主相机投影。
        /// </summary>
        private static Vector3 ResolveFlyStartScreenPosition(Transform start)
        {
            if (start == null)
            {
                return ResolveRandomScreenPosition();
            }

            var rectTransform = start as RectTransform ?? start.GetComponent<RectTransform>();
            if (rectTransform != null && rectTransform.GetComponentInParent<Canvas>() != null)
            {
                return ResolveTransformWorldCenter(rectTransform);
            }

            var camera = Camera.main;
            if (camera != null)
            {
                var screen = camera.WorldToScreenPoint(start.position);
                if (screen.z > 0f)
                {
                    return screen;
                }
            }

            return ResolveTransformWorldCenter(start);
        }

        private static void PrepareCoin(RectTransform rt, CanvasGroup cg, Vector3 startPos)
        {
            cg.alpha = 0f;
            rt.position = startPos;
            rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-CoinsMaxZRotation, CoinsMaxZRotation));
            rt.localScale = Vector3.one * CoinsStartScale;
        }

        private static float ResolveCoinStartDelay(int index, int total, float maxTrailDelay = MaxTrailDelay)
        {
            if (total <= 1)
            {
                return 0f;
            }

            return (float)index / (total - 1) * maxTrailDelay;
        }

        /// <summary>
        /// 在起点上方生成扇形发散目标点，模拟头顶朝外缓慢散开。
        /// </summary>
        private static Vector3 ResolveHeadScatterPosition(Vector3 fromScreen, int index, int total)
        {
            return ResolveHeadScatterPosition(fromScreen, index, total, CoinFlyTiming.Standard);
        }

        private static Vector3 ResolveHeadScatterPosition(
            Vector3 fromScreen,
            int index,
            int total,
            CoinFlyTiming timing)
        {
            var headAnchor = fromScreen + Vector3.up * CoinsHeadLiftOffset;
            var t = total <= 1 ? 0.5f : (float)index / (total - 1);
            var arc = Mathf.Max(1f, timing.ScatterArcDegrees);
            var angle = Mathf.Lerp(-arc * 0.5f, arc * 0.5f, t);
            angle += Random.Range(-18f, 18f);
            var minRadius = Mathf.Max(1f, timing.ScatterMinRadius);
            var maxRadius = Mathf.Max(minRadius, timing.ScatterMaxRadius);
            var radius = Random.Range(minRadius, maxRadius);
            var rad = angle * Mathf.Deg2Rad;
            var offset = new Vector3(Mathf.Sin(rad), Mathf.Abs(Mathf.Cos(rad)) * 0.55f + 0.18f, 0f) * radius;
            var jitter = Mathf.Max(0f, timing.ScatterJitter);
            if (jitter > 0f)
            {
                offset += new Vector3(
                    Random.Range(-jitter, jitter),
                    Random.Range(-jitter * 0.45f, jitter * 0.65f),
                    0f);
            }

            return headAnchor + offset;
        }

        /// <summary>
        /// 在屏幕可视区域内随机取一点（留边距，避免贴边生成）。
        /// </summary>
        private static Vector3 ResolveRandomScreenPosition()
        {
            const float padding = 96f;
            var maxX = Mathf.Max(padding + 1f, Screen.width - padding);
            var maxY = Mathf.Max(padding + 1f, Screen.height - padding);
            return new Vector3(Random.Range(padding, maxX), Random.Range(padding, maxY), 0f);
        }

        private static void AppendCoinsHeadScatter(Sequence seq, RectTransform rt, Vector3 scatterPos, float scatterDuration)
        {
            seq.Append(rt.DOMove(scatterPos, scatterDuration).SetEase(Ease.OutSine));
            seq.Join(rt.DOScale(CoinsScatterScale, scatterDuration).SetEase(Ease.OutQuad));
        }

        private static void AppendCoinsBurst(Sequence seq, RectTransform rt, Vector3 scatterPos, float scatterDuration)
        {
            AppendCoinsHeadScatter(seq, rt, scatterPos, scatterDuration);
        }

        private static void AppendCoinsFly(
            Sequence seq,
            RectTransform rt,
            CanvasGroup cg,
            Vector3 toScreen,
            float flyDuration)
        {
            seq.Append(rt.DOMove(toScreen, flyDuration).SetEase(Ease.InOutQuad));
            seq.Join(rt.DOScale(CoinsEndScale, flyDuration).SetEase(Ease.OutQuad));
            seq.Join(cg.DOFade(0f, Mathf.Max(0.1f, flyDuration - 0.2f)).SetDelay(0.2f));
        }

        private static bool TryResolveParentCanvas(Transform reference, out Transform parent)
        {
            parent = null;
            if (reference == null)
            {
                return false;
            }

            var canvas = reference.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return false;
            }

            parent = canvas.transform;
            return parent != null;
        }

        private static bool TryResolveFlyTargetContext(
            Transform target,
            out Transform parent,
            out Vector3 toScreen)
        {
            parent = null;
            toScreen = Vector3.zero;

            if (target == null)
            {
                return false;
            }

            var targetRect = target as RectTransform ?? target.GetComponent<RectTransform>();
            if (targetRect == null)
            {
                return false;
            }

            var targetCanvas = targetRect.GetComponentInParent<Canvas>();
            if (targetCanvas == null)
            {
                return false;
            }

            parent = targetCanvas.transform;
            toScreen = ResolveTransformWorldCenter(targetRect);
            return true;
        }

        private static bool TryResolveFlyContext(
            Transform start,
            Transform target,
            out Transform parent,
            out Vector3 fromScreen,
            out Vector3 toScreen)
        {
            parent = null;
            fromScreen = Vector3.zero;
            toScreen = Vector3.zero;

            if (start == null || target == null)
            {
                return false;
            }

            if (!TryResolveFlyTargetContext(target, out parent, out toScreen))
            {
                return false;
            }

            fromScreen = ResolveTransformWorldCenter(start);
            return true;
        }

        private static Vector3 ResolveTransformWorldCenter(Transform transform)
        {
            if (transform == null)
            {
                return Vector3.zero;
            }

            var rectTransform = transform as RectTransform ?? transform.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                return transform.position;
            }

            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return (corners[0] + corners[2]) * 0.5f;
        }

        private static bool TryAllocateCoin(Transform parent, out GameObject coinGo, out RectTransform rt, out CanvasGroup cg)
        {
            coinGo = Lyf.ObjectPool.ObjectPool.Instance.Allocate(coinPrefab, parent);
            rt = null;
            cg = null;
            if (coinGo == null)
            {
                return false;
            }

            rt = coinGo.GetComponent<RectTransform>();
            if (rt == null)
            {
                RecycleCoin(coinGo);
                return false;
            }

            if (!coinGo.TryGetComponent<CanvasGroup>(out cg))
            {
                cg = coinGo.AddComponent<CanvasGroup>();
            }

            return true;
        }

        private static void RecycleCoin(GameObject coinGo)
        {
            if (coinGo != null)
            {
                RestoreCoinAppearance(coinGo);
                Lyf.ObjectPool.ObjectPool.Instance.Recycle(coinGo);
            }
        }

        private static void ApplyPrestigeAppearance(GameObject coinGo)
        {
            if (coinGo == null)
            {
                return;
            }

            var image = coinGo.GetComponent<Image>();
            if (image == null)
            {
                return;
            }

            if (coinDefaultSprite == null)
            {
                coinDefaultSprite = image.sprite;
            }

            if (prestigeSprite != null)
            {
                image.sprite = prestigeSprite;
            }
        }

        private static void RestoreCoinAppearance(GameObject coinGo)
        {
            if (coinGo == null || coinDefaultSprite == null)
            {
                return;
            }

            var image = coinGo.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = coinDefaultSprite;
            }
        }

        /// <summary>
        /// 确保铜钱预制体加载完成。
        /// </summary>
        /// <returns>返回方法执行后的结果。</returns>
        private static void EnsureCoinPrefabLoaded()
        {
            if (coinPrefab != null)
            {
                return;
            }

            // 特效 预制体 只首次加载一次，后续由对象池复用实例。
            coinPrefab = GameplayResourceStore.LoadAsset<GameObject>(CoinPrefabPath);
        }

        private static void EnsurePrestigeSpriteLoaded()
        {
            if (prestigeSprite != null)
            {
                return;
            }

            prestigeSprite = GameplayResourceStore.LoadAsset<Sprite>(PrestigeSpritePath);
        }
    }
}

namespace JN.Client.UI
{
    /// <summary>
    /// 负责为单个按钮补挂通用点击音效。
    /// </summary>
    [DisallowMultipleComponent]
    public class UIButtonClickSoundHook : MonoBehaviour
    {
        private Button button;

        /// <summary>
        /// 缓存按钮组件。
        /// </summary>
        private void Awake()
        {
            button = GetComponent<Button>();
        }

        /// <summary>
        /// 激活时绑定点击音效。
        /// </summary>
        private void OnEnable()
        {
            button ??= GetComponent<Button>();
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }

        /// <summary>
        /// 失活时移除点击音效监听，避免重复绑定。
        /// </summary>
        private void OnDisable()
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(HandleClick);
        }

        /// <summary>
        /// 处理按钮点击音效播放。
        /// </summary>
        private static void HandleClick()
        {
            GameAudioManager.PlayButtonClick();
        }
    }
}
