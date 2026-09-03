using System.Collections;
using System.Collections.Generic;
using JN.Client;
using JN.Client.Config;
using JN.Client.Manager;
using JN.Client.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JN.Client.Scene
{
    /// <summary>
    /// 二楼设施：桌子随二楼解锁默认建成；戏台与装饰（Facility 27~33）走 BuyRoot 建造挂点 + 价签 UI。
    /// 购买成功后立刻切建成态，不走一楼小二/门口搬家具流程。
    /// </summary>
    public sealed class TavernSecondFloorFacilityPurchaseController : MonoBehaviour
    {
        private const string ShopInteriorName = "Shop_Interior";
        private const string BuyRootName = "BuyRoot";
        private const string TableNodeName = "桌子";
        private const string XitaiNodeName = "xitai";
        private const string TableBuildAnchorName = "桌子建造";
        private const string WunvNodeName = "wunv";
        private const string XitaiTaijieName = "xitai_taijie";
        private const string XitaiTaijie1Name = "xitai_taijie_1";

        private const float GuideBuildBaseColliderMinSize = 0.2f;
        private const float GuideBuildBaseColliderPadding = 1.15f;
        private const float PurchaseClickCooldownSeconds = 0.25f;

        private readonly struct SecondFloorPurchaseEntry
        {
            public readonly string GuideKey;
            public readonly string BuildAnchorName;
            public readonly string BindNodeName;
            public readonly string PurchaseHudKey;
            public readonly bool IsXitai;

            public SecondFloorPurchaseEntry(
                string guideKey,
                string buildAnchorName,
                string bindNodeName,
                string purchaseHudKey,
                bool isXitai = false)
            {
                GuideKey = guideKey;
                BuildAnchorName = buildAnchorName;
                BindNodeName = bindNodeName;
                PurchaseHudKey = purchaseHudKey;
                IsXitai = isXitai;
            }
        }

        /// <summary>
        /// BuyRoot 建造挂点与 Facility.bindNode 同名；戏台挂点仍为「戏台建造」。
        /// Shop_Interior 下 bindNode 负责半透/建成视觉。
        /// </summary>
        private static readonly SecondFloorPurchaseEntry[] PurchaseEntries =
        {
            new(DataManager.GuideXitai, "戏台建造", "xitai", "second_floor_xitai_purchase", isXitai: true),
            new(DataManager.GuideDecoration1, "装饰墙", "装饰墙", "second_floor_decoration_1_purchase"),
            new(DataManager.GuideDecoration2, "贵妃椅子", "贵妃椅子", "second_floor_decoration_2_purchase"),
            new(DataManager.GuideDecoration3, "植物", "植物", "second_floor_decoration_3_purchase"),
            new(DataManager.GuideDecoration4, "huaping_1", "huaping_1", "second_floor_decoration_4_purchase"),
            new(DataManager.GuideDecoration5, "松盆景", "松盆景", "second_floor_decoration_5_purchase"),
            new(DataManager.GuideDecoration6, "松盆景_1", "松盆景_1", "second_floor_decoration_6_purchase"),
        };

        private Transform shopInteriorRoot;
        private Transform buyRoot;
        private Transform tableRoot;
        private Transform xitaiRoot;
        private Transform tableBuildAnchor;
        private readonly Dictionary<string, Transform> bindNodeRoots = new();
        private readonly Dictionary<string, Transform> buildAnchors = new();
        private float lastPurchaseClickUnscaledTime = -999f;

        public static TavernSecondFloorFacilityPurchaseController FindOrCreate()
        {
            var existing = FindFirstObjectByType<TavernSecondFloorFacilityPurchaseController>();
            if (existing != null)
            {
                return existing;
            }

            var host = new GameObject("TavernSecondFloorFacilityPurchase");
            return host.AddComponent<TavernSecondFloorFacilityPurchaseController>();
        }

        private void OnEnable()
        {
            Signals.Get<GameplayGuideProgressSignal>().RemoveListener(RefreshAll);
            Signals.Get<GameplayGuideProgressSignal>().AddListener(RefreshAll);
            RefreshAll();
            StartCoroutine(RefreshAllNextFrameRoutine());
        }

        private void OnDisable()
        {
            Signals.Get<GameplayGuideProgressSignal>().RemoveListener(RefreshAll);
            StopAllCoroutines();
            for (var index = 0; index < PurchaseEntries.Length; index++)
            {
                HudOverlayService.UnregisterPurchaseActionHud(PurchaseEntries[index].PurchaseHudKey);
            }
        }

        private IEnumerator RefreshAllNextFrameRoutine()
        {
            yield return null;
            RefreshAll();
        }

        /// <summary>
        /// 二楼无 TavernSceneManager：由 CameraController / 一楼购买点击入口回落到此，处理建造板世界射线。
        /// </summary>
        public static bool TryHandlePurchasePointerClick(Vector2 pointerPosition)
        {
            if (!SceneFlowCoordinator.IsOnTavernSecondFloor() || Camera.main == null)
            {
                return false;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            var controller = FindFirstObjectByType<TavernSecondFloorFacilityPurchaseController>();
            if (controller == null)
            {
                controller = FindOrCreate();
                controller.RefreshAll();
            }

            return controller.TryHandleWorldBuildBaseClick(pointerPosition);
        }

        /// <summary>刷新半透/建成与购买 UI。</summary>
        public void RefreshAll()
        {
            if (!SceneFlowCoordinator.IsOnTavernSecondFloor())
            {
                return;
            }

            ResolveSceneAnchors();
            RefreshTableFacility();

            var dataManager = DataManager.Instance;
            var visiting = dataManager != null && dataManager.IsVisitingOtherTavern;
            var allowsPurchase = dataManager != null && dataManager.AllowsFacilityPurchaseNow();

            for (var index = 0; index < PurchaseEntries.Length; index++)
            {
                RefreshPurchaseEntry(PurchaseEntries[index], visiting, allowsPurchase);
            }
        }

        private void ResolveSceneAnchors()
        {
            shopInteriorRoot = FindNamedTransform(ShopInteriorName);
            buyRoot = FindNamedTransform(BuyRootName);

            tableRoot = FindChildNamed(shopInteriorRoot, TableNodeName)
                        ?? FindNamedTransform(TableNodeName);
            xitaiRoot = FindChildNamed(shopInteriorRoot, XitaiNodeName)
                        ?? FindNamedTransform(XitaiNodeName);
            tableBuildAnchor = FindChildNamed(buyRoot, TableBuildAnchorName);

            bindNodeRoots.Clear();
            buildAnchors.Clear();
            for (var index = 0; index < PurchaseEntries.Length; index++)
            {
                var entry = PurchaseEntries[index];
                if (!string.IsNullOrWhiteSpace(entry.BindNodeName))
                {
                    bindNodeRoots[entry.GuideKey] = ResolveVisualBindRoot(entry.BindNodeName);
                }

                if (!string.IsNullOrWhiteSpace(entry.BuildAnchorName))
                {
                    // 与 bindNode 同名时必须只取 BuyRoot 直接子节点，避免 DFS 误拿到 3D 模型。
                    buildAnchors[entry.GuideKey] = FindDirectChildNamed(buyRoot, entry.BuildAnchorName);
                }
            }
        }

        private void RefreshTableFacility()
        {
            if (tableRoot != null)
            {
                FacilityBuildVisualUtility.ApplyBuiltState(tableRoot.gameObject);
            }

            if (tableBuildAnchor != null)
            {
                tableBuildAnchor.gameObject.SetActive(false);
            }
        }

        private void RefreshPurchaseEntry(SecondFloorPurchaseEntry entry, bool visiting, bool allowsPurchase)
        {
            var dataManager = DataManager.Instance;
            var purchased = dataManager != null && dataManager.IsSecondFloorFacilityPurchased(entry.GuideKey);
            var facility = FacilityConfigUtility.GetByGuideKey(entry.GuideKey);
            var meetsLevel = facility == null
                             || dataManager == null
                             || FacilityConfigUtility.MeetsUnlockLevel(facility, dataManager.GetTavernLevel());

            ApplyFacilityVisual(entry, purchased);

            buildAnchors.TryGetValue(entry.GuideKey, out var buildAnchor);
            // 价签显示不挡在戏台前置上：未购装饰同时出入口，点击购买时再校验前置。
            var showPurchase = allowsPurchase
                               && !purchased
                               && !visiting
                               && meetsLevel;
            // BuyRoot 建造图：未购显示 Sprite；建造成功后整节点隐藏（拜访他人店也隐藏）。
            var showBuildPad = !purchased && !visiting;
            if (buildAnchor != null)
            {
                buildAnchor.gameObject.SetActive(showBuildPad);
                if (showBuildPad)
                {
                    SetBuildPadSpriteVisible(buildAnchor.gameObject, visible: true);
                    PrepareBuildBaseForClick(buildAnchor.gameObject);
                }
            }

            var showHud = showPurchase && buildAnchor != null && buildAnchor.gameObject.activeInHierarchy;
            var cost = dataManager != null
                ? dataManager.GetSecondFloorFacilityCost(entry.GuideKey)
                : 0;
            RefreshPurchaseHud(
                entry.PurchaseHudKey,
                showHud,
                buildAnchor,
                cost,
                () => TryBuy(entry.GuideKey));
        }

        /// <summary>
        /// 世界射线命中任意建造板时触发对应购买。
        /// </summary>
        private bool TryHandleWorldBuildBaseClick(Vector2 pointerPosition)
        {
            ResolveSceneAnchors();
            for (var index = 0; index < PurchaseEntries.Length; index++)
            {
                var entry = PurchaseEntries[index];
                if (!buildAnchors.TryGetValue(entry.GuideKey, out var buildAnchor)
                    || buildAnchor == null
                    || !buildAnchor.gameObject.activeInHierarchy)
                {
                    continue;
                }

                PrepareBuildBaseForClick(buildAnchor.gameObject);
            }

            var ray = Camera.main.ScreenPointToRay(pointerPosition);
            var hits = Physics.RaycastAll(ray, 1000f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
            if (hits == null || hits.Length == 0)
            {
                return false;
            }

            System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
            for (var hitIndex = 0; hitIndex < hits.Length; hitIndex++)
            {
                var hitCollider = hits[hitIndex].collider;
                if (hitCollider == null)
                {
                    continue;
                }

                for (var entryIndex = 0; entryIndex < PurchaseEntries.Length; entryIndex++)
                {
                    var entry = PurchaseEntries[entryIndex];
                    if (!buildAnchors.TryGetValue(entry.GuideKey, out var buildAnchor))
                    {
                        continue;
                    }

                    if (IsHitOnBuildAnchor(hitCollider, buildAnchor))
                    {
                        TryBuy(entry.GuideKey);
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsHitOnBuildAnchor(Collider hitCollider, Transform buildAnchor)
        {
            return buildAnchor != null
                   && buildAnchor.gameObject.activeInHierarchy
                   && hitCollider.transform.IsChildOf(buildAnchor);
        }

        /// <summary>
        /// 为建造板补齐点击用 BoxCollider；保留 Sprite 显示（未购占位图）。
        /// </summary>
        private static void PrepareBuildBaseForClick(GameObject buildBase)
        {
            if (buildBase == null)
            {
                return;
            }

            var spriteRenderer = buildBase.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return;
            }

            spriteRenderer.enabled = true;

            var localBounds = spriteRenderer.sprite.bounds;
            var boxCollider = buildBase.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = buildBase.AddComponent<BoxCollider>();
            }

            boxCollider.enabled = true;
            boxCollider.isTrigger = true;
            boxCollider.center = localBounds.center;
            boxCollider.size = new Vector3(
                Mathf.Max(GuideBuildBaseColliderMinSize, localBounds.size.x * GuideBuildBaseColliderPadding),
                Mathf.Max(GuideBuildBaseColliderMinSize, localBounds.size.y * GuideBuildBaseColliderPadding),
                Mathf.Max(GuideBuildBaseColliderMinSize, localBounds.size.z * GuideBuildBaseColliderPadding));
        }

        private static void SetBuildPadSpriteVisible(GameObject buildBase, bool visible)
        {
            if (buildBase == null)
            {
                return;
            }

            var spriteRenderer = buildBase.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = visible;
            }
        }

        private void ApplyFacilityVisual(SecondFloorPurchaseEntry entry, bool purchased)
        {
            if (entry.IsXitai)
            {
                ApplyXitaiVisualState(purchased);
                return;
            }

            if (!bindNodeRoots.TryGetValue(entry.GuideKey, out var bindRoot)
                || bindRoot == null
                || IsUnderBuyRoot(bindRoot))
            {
                return;
            }

            ApplyDecorationVisual(bindRoot, purchased);
        }

        /// <summary>
        /// 装饰 bindNode：根节点 + 各嵌套 Prefab 子树分别刷材质（松盆景/盆景凳、贵妃椅子模型等）。
        /// </summary>
        private static void ApplyDecorationVisual(Transform bindRoot, bool purchased)
        {
            if (bindRoot == null)
            {
                return;
            }

            if (purchased)
            {
                FacilityBuildVisualUtility.ApplyBuiltState(bindRoot.gameObject);
            }
            else
            {
                FacilityBuildVisualUtility.ApplyPreviewState(bindRoot.gameObject);
            }

            for (var index = 0; index < bindRoot.childCount; index++)
            {
                var child = bindRoot.GetChild(index);
                if (child == null)
                {
                    continue;
                }

                if (purchased)
                {
                    FacilityBuildVisualUtility.ApplyBuiltState(child.gameObject);
                }
                else
                {
                    FacilityBuildVisualUtility.ApplyPreviewState(child.gameObject);
                }
            }
        }

        /// <summary>
        /// 解析 3D 视觉 bindNode：排除 BuyRoot 点击挂点，优先 Shop_Interior 下有 Mesh 的嵌套 Prefab 根。
        /// </summary>
        private Transform ResolveVisualBindRoot(string bindNodeName)
        {
            if (string.IsNullOrWhiteSpace(bindNodeName))
            {
                return null;
            }

            Transform best = null;
            var bestScore = int.MinValue;
            var all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var index = 0; index < all.Length; index++)
            {
                var current = all[index];
                if (current == null
                    || current.name != bindNodeName
                    || IsUnderBuyRoot(current))
                {
                    continue;
                }

                var score = ScoreVisualBindRoot(current);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = current;
                }
            }

            return PreferOuterSameNameRoot(best);
        }

        /// <summary>
        /// 嵌套 Prefab 常有外层空根与内层同名网格，统一落到最外层同名根。
        /// </summary>
        private Transform PreferOuterSameNameRoot(Transform found)
        {
            if (found == null)
            {
                return null;
            }

            var current = found;
            while (current.parent != null
                   && current.parent.name == current.name
                   && !IsUnderBuyRoot(current.parent))
            {
                current = current.parent;
            }

            return current;
        }

        private static bool IsSpriteOnlyBuildPad(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            var renderers = target.GetComponentsInChildren<Renderer>(true);
            for (var index = 0; index < renderers.Length; index++)
            {
                if (renderers[index] is MeshRenderer or SkinnedMeshRenderer)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsUnderBuyRoot(Transform target)
        {
            return buyRoot != null && target != null && target.IsChildOf(buyRoot);
        }

        private bool IsUnderShopInterior(Transform target)
        {
            return shopInteriorRoot != null && target != null && target.IsChildOf(shopInteriorRoot);
        }

        private int ScoreVisualBindRoot(Transform candidate)
        {
            if (candidate == null)
            {
                return int.MinValue;
            }

            var score = 0;
            if (IsUnderShopInterior(candidate))
            {
                score += 100;
            }

            var renderers = candidate.GetComponentsInChildren<Renderer>(true);
            var meshRendererCount = 0;
            for (var index = 0; index < renderers.Length; index++)
            {
                var renderer = renderers[index];
                if (renderer is MeshRenderer or SkinnedMeshRenderer)
                {
                    meshRendererCount++;
                }
            }

            if (meshRendererCount == 0)
            {
                return score;
            }

            score += meshRendererCount * 10;

            // 嵌套 Prefab 常见空根 + 子网格：优先选外层根（如松盆景、贵妃椅子）。
            if (candidate.GetComponent<Renderer>() == null)
            {
                score += 50;
            }

            return score;
        }

        /// <summary>
        /// 未购：xitai / xitai_taijie / xitai_taijie_1 半透，wunv 隐藏；已购则相反。
        /// </summary>
        private void ApplyXitaiVisualState(bool purchased)
        {
            if (xitaiRoot == null)
            {
                return;
            }

            var wunv = FindChildNamed(xitaiRoot, WunvNodeName);
            if (wunv != null)
            {
                wunv.gameObject.SetActive(purchased);
            }

            ApplyNamedChildVisual(xitaiRoot, XitaiTaijieName, purchased);
            ApplyNamedChildVisual(xitaiRoot, XitaiTaijie1Name, purchased);

            for (var index = 0; index < xitaiRoot.childCount; index++)
            {
                var child = xitaiRoot.GetChild(index);
                if (child == null || child.name != XitaiNodeName)
                {
                    continue;
                }

                if (purchased)
                {
                    FacilityBuildVisualUtility.ApplyBuiltState(child.gameObject);
                }
                else
                {
                    FacilityBuildVisualUtility.ApplyPreviewState(child.gameObject);
                }
            }
        }

        private static void ApplyNamedChildVisual(Transform root, string childName, bool purchased)
        {
            var child = FindChildNamed(root, childName);
            if (child == null)
            {
                return;
            }

            if (purchased)
            {
                FacilityBuildVisualUtility.ApplyBuiltState(child.gameObject);
            }
            else
            {
                FacilityBuildVisualUtility.ApplyPreviewState(child.gameObject);
            }
        }

        private void TryBuy(string guideKey)
        {
            if (Time.unscaledTime - lastPurchaseClickUnscaledTime < PurchaseClickCooldownSeconds)
            {
                return;
            }

            lastPurchaseClickUnscaledTime = Time.unscaledTime;

            var dataManager = DataManager.Instance;
            if (dataManager == null)
            {
                return;
            }

            if (!dataManager.TryPurchaseSecondFloorFacility(guideKey, out var message))
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    HudOverlayService.ShowFloatingWarning(message);
                }

                return;
            }

            GameAudioManager.PlayButtonClick();
            RefreshAll();
            PlayPurchaseSuccessEffect(guideKey);
        }

        /// <summary>购买成功后在家具位置播一楼同款建造完成特效。</summary>
        private void PlayPurchaseSuccessEffect(string guideKey)
        {
            Transform effectTarget = null;
            if (guideKey == DataManager.GuideXitai)
            {
                effectTarget = xitaiRoot;
            }
            else if (bindNodeRoots.TryGetValue(guideKey, out var bindRoot))
            {
                effectTarget = bindRoot;
            }

            if (effectTarget == null && buildAnchors.TryGetValue(guideKey, out var buildAnchor))
            {
                effectTarget = buildAnchor;
            }

            if (effectTarget == null)
            {
                return;
            }

            // 购买音效已由 GrantFacilityBuildPrestige 播放，这里只播光柱。
            TavernSceneManager.PlayGuideBuildingSuccessEffectAt(effectTarget, playAudio: false);
        }

        private static bool RefreshPurchaseHud(
            string purchaseKey,
            bool visible,
            Transform target,
            int cost,
            System.Action onPurchase)
        {
            if (!visible || target == null)
            {
                HudOverlayService.UnregisterPurchaseActionHud(purchaseKey);
                return false;
            }

            var purchaseUi = HudOverlayService.RegisterPurchaseActionHud(purchaseKey, target, onPurchase);
            if (purchaseUi == null)
            {
                return false;
            }

            purchaseUi.SetUnlockPrompt(true, cost);
            return true;
        }

        private static Transform FindDirectChildNamed(Transform parent, string childName)
        {
            if (parent == null || string.IsNullOrEmpty(childName))
            {
                return null;
            }

            for (var index = 0; index < parent.childCount; index++)
            {
                var child = parent.GetChild(index);
                if (child != null && child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }

        private static Transform FindNamedTransform(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return null;
            }

            var all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var index = 0; index < all.Length; index++)
            {
                var current = all[index];
                if (current != null && current.name == objectName)
                {
                    return current;
                }
            }

            return null;
        }

        private static Transform FindChildNamed(Transform root, string childName)
        {
            if (root == null || string.IsNullOrEmpty(childName))
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (var index = 0; index < root.childCount; index++)
            {
                var nested = FindChildNamed(root.GetChild(index), childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }
    }
}
