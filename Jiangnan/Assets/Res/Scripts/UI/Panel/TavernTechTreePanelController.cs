using System.Collections;
using System.Collections.Generic;
using cfg;
using DG.Tweening;
using JN.Client;
using JN.Client.Config;
using JN.Client.Manager;
using JN.Client.Tools;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JN.Client.UI
{
    public class TavernTechTreePanelControllerData : QFramework.UIPanelData
    {
        public int InitialSelectedTechId;
    }

    /// <summary>
    /// 酒店科技树主界面：按预制体 node_{techId} 展示 101–107 号科技。
    /// </summary>
    public class TavernTechTreePanelController : OverlayPanelController<TavernTechTreePanelControllerData>
    {
        private const string TechNodeGroupPath = "Panel/group_Branch_Staff";
        private const float UnlockHintCwDuration = 0.42f;
        private const float UnlockHintCcwDuration = 0.38f;
        private const float UnlockHintResetDuration = 0.3f;
        private const float UnlockHintPauseDuration = 0.28f;
        private const float UnlockHintCwAngle = -12f;
        private const float UnlockHintCcwAngle = 2f;
        private const float TechResearchRefreshInterval = 0.25f;
        private const string InsufficientCoinAmountColorHex = "#FF4D4F";

        private int lastActiveResearchTechId;
        private int unlockAnimatingTechId;
        private Coroutine researchProgressRoutine;
        private readonly Dictionary<int, Sequence> unlockHintTweens = new();

        protected override void OnPanelInit()
        {
            BindButton(ResolveButton("Panel/btn_Close", "btn_Close"), CloseSelf);
            SetNodeVisible("Panel/group_Detail", false);
        }

        protected override void OnPanelOpen(TavernTechTreePanelControllerData data)
        {
            Signals.Get<TavernRuntimeChangedSignal>().RemoveListener(HandleRuntimeChanged);
            Signals.Get<TavernRuntimeChangedSignal>().AddListener(HandleRuntimeChanged);
            Signals.Get<TechResearchCompletedSignal>().RemoveListener(HandleTechResearchCompleted);
            Signals.Get<TechResearchCompletedSignal>().AddListener(HandleTechResearchCompleted);

            CacheActiveResearchTechId();
            RefreshPanel();
            SyncResearchProgressRoutine();
        }

        protected override void OnPanelShow()
        {
            CacheActiveResearchTechId();
            RefreshPanel();
            SyncResearchProgressRoutine();
        }

        protected override void OnPanelClose()
        {
            Signals.Get<TavernRuntimeChangedSignal>().RemoveListener(HandleRuntimeChanged);
            Signals.Get<TechResearchCompletedSignal>().RemoveListener(HandleTechResearchCompleted);
            StopResearchProgressRoutine();
            KillAllUnlockHintAnimations();
            ResetAllTechNodeResearchFills();
            unlockAnimatingTechId = 0;
            lastActiveResearchTechId = 0;
        }

        private void HandleRuntimeChanged()
        {
            CacheActiveResearchTechId();
            SyncResearchProgressRoutine();
            if (unlockAnimatingTechId > 0)
            {
                RefreshResearchNodeFill();
                return;
            }

            RefreshMainTechNodes();
        }

        private void HandleTechResearchCompleted(string techName)
        {
            var techId = lastActiveResearchTechId;
            PlayTechNodeUnlockAnimation(techId, techName);
        }

        private void CacheActiveResearchTechId()
        {
            var researchId = DataManager.Instance?.SaveData?.gameplay?.researchingTechId ?? 0;
            if (researchId > 0)
            {
                lastActiveResearchTechId = researchId;
            }
        }

        private void SyncResearchProgressRoutine()
        {
            var dataManager = DataManager.Instance;
            var active = dataManager != null
                           && dataManager.SaveData?.gameplay?.researchingTechId > 0
                           && dataManager.TryGetTechResearchProgress(out _, out _);

            if (active)
            {
                if (researchProgressRoutine == null)
                {
                    researchProgressRoutine = StartCoroutine(ResearchProgressLoop());
                }

                return;
            }

            StopResearchProgressRoutine();
        }

        private void StopResearchProgressRoutine()
        {
            if (researchProgressRoutine == null)
            {
                return;
            }

            StopCoroutine(researchProgressRoutine);
            researchProgressRoutine = null;
        }

        private IEnumerator ResearchProgressLoop()
        {
            var wait = new WaitForSecondsRealtime(TechResearchRefreshInterval);
            while (true)
            {
                var dataManager = DataManager.Instance;
                if (dataManager == null || dataManager.SaveData?.gameplay?.researchingTechId <= 0)
                {
                    break;
                }

                RefreshResearchNodeFill();
                yield return wait;
            }

            researchProgressRoutine = null;
        }

        private void RefreshPanel()
        {
            SetNodeVisible("Panel/group_Detail", false);
            RefreshMainTechNodes();
            RefreshResearchNodeFill();
        }

        private void RefreshMainTechNodes()
        {
            var techIds = TavernTechConfigUtility.CollectMainPanelTechIds();
            for (var index = 0; index < techIds.Count; index++)
            {
                var techId = techIds[index];
                if (unlockAnimatingTechId == techId)
                {
                    continue;
                }

                var nodePath = GetTechNodePath(techId);
                var node = ResolveTransform(nodePath);
                if (node == null)
                {
                    continue;
                }

                var tech = TavernTechConfigUtility.Get(techId);
                if (tech == null)
                {
                    SetNodeVisible(nodePath, false);
                    continue;
                }

                SetNodeVisible(nodePath, true);
                RefreshNode(nodePath, tech);
            }
        }

        private static string GetTechNodePath(int techId)
        {
            return $"{TechNodeGroupPath}/node_{techId}";
        }

        private void RefreshNode(string nodePath, TavernTech tech)
        {
            var dataManager = DataManager.Instance;
            var researched = dataManager != null && dataManager.IsTechResearched(tech.Id);
            var lockedSecondFloor = tech.Id == TavernTechConfigUtility.LockedSecondFloorTechId;
            var canResearchNow = !lockedSecondFloor
                                 && dataManager != null
                                 && dataManager.CanResearchTech(tech.Id, out _);
            var researchingTechId = dataManager?.SaveData?.gameplay?.researchingTechId ?? 0;
            var isResearching = researchingTechId > 0 && tech.Id == researchingTechId;

            SetText($"{nodePath}/txt_Name", tech.Name);
            SetNodeVisible($"{nodePath}/txt_State", false);

            ApplyTechNodeIcon($"{nodePath}/img_Bg", tech.Icon, 1);
            ApplyTechNodeIcon($"{nodePath}/img_Unlock", tech.Icon, 2);

            var showLock = !researched;
            var showBg = true;
            var showUnlock = !researched;

            SetNodeVisible($"{nodePath}/img_Lock", showLock);
            SetNodeVisible($"{nodePath}/img_Unlock", showUnlock);
            SetNodeVisible($"{nodePath}/img_Bg", showBg);

            if (showLock)
            {
                ResetTechNodeLockAnimation(nodePath);
            }

            if (isResearching)
            {
                StopUnlockHintAnimation(tech.Id, nodePath);
                ApplyTechNodeResearchVisuals(nodePath, GetCurrentResearchFillAmount(dataManager));
            }
            else
            {
                ResetTechNodeResearchVisuals(nodePath);
            }

            RefreshNodeResearchButton(nodePath, tech, dataManager, researched, lockedSecondFloor, isResearching);
            EnsureResearchButtonOnTop(nodePath);

            SyncUnlockHintAnimation(
                tech.Id,
                nodePath,
                canResearchNow && showUnlock && !isResearching && researchingTechId <= 0);
        }

        private void RefreshNodeResearchButton(
            string nodePath,
            TavernTech tech,
            DataManager dataManager,
            bool researched,
            bool lockedSecondFloor,
            bool isResearching)
        {
            var buttonPath = $"{nodePath}/btn_Research";
            var btn = ResolveButton(buttonPath);
            if (btn == null)
            {
                return;
            }

            var label = btn.GetComponentInChildren<TMP_Text>(true);
            if (researched)
            {
                btn.gameObject.SetActive(true);
                btn.interactable = false;
                if (label != null)
                {
                    label.text = "已完成";
                }

                BindButton(btn, null);
                return;
            }

            if (lockedSecondFloor)
            {
                btn.gameObject.SetActive(true);
                btn.interactable = false;
                if (label != null)
                {
                    label.text = "暂未开放";
                }

                BindButton(btn, null);
                return;
            }

            if (isResearching)
            {
                btn.gameObject.SetActive(true);
                btn.interactable = false;
                if (label != null)
                {
                    label.text = "投资中…";
                }

                BindButton(btn, null);
                return;
            }

            btn.gameObject.SetActive(true);
            var canAttempt = dataManager != null && dataManager.CanAttemptTechResearch(tech.Id, out _);
            if (!canAttempt)
            {
                btn.interactable = false;
                if (label != null)
                {
                    label.text = FormatInvestLabel(tech.Cost);
                }

                BindButton(btn, null);
                return;
            }

            var canAfford = dataManager != null && dataManager.CanResearchTech(tech.Id, out _);
            btn.interactable = canAfford;
            if (label != null)
            {
                if (canAfford)
                {
                    label.text = FormatInvestLabel(tech.Cost);
                }
                else
                {
                    label.richText = true;
                    label.text = FormatInsufficientCoinLabel(tech.Cost);
                }
            }

            var capturedId = tech.Id;
            BindButton(btn, () => TryResearchTech(capturedId));
        }

        private static string FormatInvestLabel(int cost)
        {
            return $"投资{cost}";
        }

        private static string FormatInsufficientCoinLabel(int cost)
        {
            return $"铜钱不足<color={InsufficientCoinAmountColorHex}>{cost}</color>";
        }

        private void EnsureResearchButtonOnTop(string nodePath)
        {
            var buttonTransform = ResolveTransform($"{nodePath}/btn_Research");
            buttonTransform?.SetAsLastSibling();
        }

        private void ApplyTechNodeResearchVisuals(string nodePath, float fillAmount)
        {
            var unlockTransform = ResolveTransform($"{nodePath}/img_Unlock");
            var bgTransform = ResolveTransform($"{nodePath}/img_Bg");
            var lockTransform = ResolveTransform($"{nodePath}/img_Lock");
            if (unlockTransform != null)
            {
                unlockTransform.SetSiblingIndex(0);
            }

            if (bgTransform != null)
            {
                bgTransform.SetAsLastSibling();
            }

            if (lockTransform != null)
            {
                lockTransform.SetAsLastSibling();
            }

            ApplyTechNodeResearchFill($"{nodePath}/img_Bg", fillAmount);
            ResetTechNodeResearchFill($"{nodePath}/img_Unlock");
            EnsureResearchButtonOnTop(nodePath);
        }

        private void ResetTechNodeResearchVisuals(string nodePath)
        {
            ResetTechNodeResearchFill($"{nodePath}/img_Bg");
            ResetTechNodeResearchFill($"{nodePath}/img_Unlock");
        }

        private static float GetCurrentResearchFillAmount(DataManager dataManager)
        {
            return dataManager != null && dataManager.TryGetTechResearchFillAmount(out var fillAmount)
                ? fillAmount
                : 0f;
        }

        private void RefreshResearchNodeFill()
        {
            var dataManager = DataManager.Instance;
            var researchingTechId = dataManager?.SaveData?.gameplay?.researchingTechId ?? 0;
            if (researchingTechId <= 0)
            {
                return;
            }

            ApplyTechNodeResearchFill(
                $"{GetTechNodePath(researchingTechId)}/img_Bg",
                GetCurrentResearchFillAmount(dataManager));
        }

        /// <summary>
        /// 与 btn_tech_suggest/img_BtnIcon 一致：Radial360、Top 起点、逆时针填充。
        /// </summary>
        private void ApplyTechNodeResearchFill(string path, float fillAmount)
        {
            var image = ResolveImage(path);
            if (image == null)
            {
                return;
            }

            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillOrigin = (int)Image.Origin360.Top;
            image.fillClockwise = false;
            image.fillAmount = Mathf.Clamp01(fillAmount);
        }

        private void ResetTechNodeResearchFill(string path)
        {
            var image = ResolveImage(path);
            if (image == null)
            {
                return;
            }

            image.type = Image.Type.Simple;
            image.fillAmount = 1f;
        }

        private void ResetTechNodeLockAnimation(string nodePath)
        {
            var lockRoot = ResolveTransform($"{nodePath}/img_Lock");
            var lockImage = lockRoot != null ? lockRoot.GetComponent<Image>() : null;
            if (lockImage != null)
            {
                lockImage.enabled = true;
                lockImage.color = Color.white;
                lockImage.raycastTarget = false;
            }

            var lockAnimation = lockRoot != null
                ? lockRoot.GetComponent<AnimateTexture>()
                  ?? lockRoot.GetComponentInChildren<AnimateTexture>(true)
                : null;
            lockAnimation?.ResetToFirstFrame();
        }

        private static void PrepareTechNodeLockForUnlock(string nodePath, Transform lockRoot)
        {
            if (lockRoot == null)
            {
                return;
            }

            lockRoot.gameObject.SetActive(true);

            var lockImage = lockRoot.GetComponent<Image>();
            if (lockImage != null)
            {
                lockImage.enabled = true;
                lockImage.color = Color.white;
                lockImage.raycastTarget = false;
            }
        }

        private void PlayTechNodeUnlockAnimation(int techId, string techName)
        {
            StopResearchProgressRoutine();

            if (techId <= 0)
            {
                FinishTechNodeUnlock(techId);
                return;
            }

            var nodePath = GetTechNodePath(techId);
            var lockRoot = ResolveTransform($"{nodePath}/img_Lock");
            var lockAnimation = lockRoot != null
                ? lockRoot.GetComponent<AnimateTexture>()
                  ?? lockRoot.GetComponentInChildren<AnimateTexture>(true)
                : null;

            if (lockAnimation == null)
            {
                FinishTechNodeUnlock(techId);
                return;
            }

            unlockAnimatingTechId = techId;
            PrepareTechNodeLockForUnlock(nodePath, lockRoot);
            lockAnimation.ResetToFirstFrame();
            lockAnimation.PlayOnce(() => FinishTechNodeUnlock(techId));
        }

        private void FinishTechNodeUnlock(int techId)
        {
            unlockAnimatingTechId = 0;
            lastActiveResearchTechId = 0;

            if (techId > 0)
            {
                SetNodeVisible($"{GetTechNodePath(techId)}/img_Lock", false);
            }

            RefreshPanel();
            SyncResearchProgressRoutine();
        }

        private void SyncUnlockHintAnimation(int techId, string nodePath, bool shouldAnimate)
        {
            if (!shouldAnimate)
            {
                StopUnlockHintAnimation(techId, nodePath);
                return;
            }

            if (unlockHintTweens.TryGetValue(techId, out var existing)
                && existing != null
                && existing.IsActive())
            {
                return;
            }

            var unlockTransform = ResolveTransform($"{nodePath}/img_Unlock");
            if (unlockTransform == null)
            {
                return;
            }

            StopUnlockHintAnimation(techId, nodePath, resetRotation: false);
            unlockTransform.localRotation = Quaternion.identity;

            var sequence = DOTween.Sequence()
                .Append(unlockTransform
                    .DORotate(new Vector3(0f, 0f, UnlockHintCwAngle), UnlockHintCwDuration)
                    .SetEase(Ease.InOutSine))
                .Append(unlockTransform
                    .DORotate(new Vector3(0f, 0f, UnlockHintCcwAngle), UnlockHintCcwDuration)
                    .SetEase(Ease.InOutSine))
                .Append(unlockTransform
                    .DORotate(Vector3.zero, UnlockHintResetDuration)
                    .SetEase(Ease.InOutSine))
                .AppendInterval(UnlockHintPauseDuration)
                .SetLoops(-1);

            unlockHintTweens[techId] = sequence;
        }

        private void StopUnlockHintAnimation(int techId, string nodePath, bool resetRotation = true)
        {
            if (unlockHintTweens.TryGetValue(techId, out var tween))
            {
                tween?.Kill();
                unlockHintTweens.Remove(techId);
            }

            if (!resetRotation)
            {
                return;
            }

            var unlockTransform = ResolveTransform($"{nodePath}/img_Unlock");
            if (unlockTransform != null)
            {
                unlockTransform.localRotation = Quaternion.identity;
            }
        }

        private void KillAllUnlockHintAnimations()
        {
            foreach (var pair in unlockHintTweens)
            {
                pair.Value?.Kill();
            }

            unlockHintTweens.Clear();

            var techIds = TavernTechConfigUtility.CollectMainPanelTechIds();
            for (var index = 0; index < techIds.Count; index++)
            {
                var nodePath = GetTechNodePath(techIds[index]);
                var unlockTransform = ResolveTransform($"{nodePath}/img_Unlock");
                if (unlockTransform != null)
                {
                    unlockTransform.localRotation = Quaternion.identity;
                }
            }
        }

        private void ResetAllTechNodeResearchFills()
        {
            var techIds = TavernTechConfigUtility.CollectMainPanelTechIds();
            for (var index = 0; index < techIds.Count; index++)
            {
                ResetTechNodeResearchVisuals(GetTechNodePath(techIds[index]));
            }
        }

        private void ApplyTechNodeIcon(string path, string iconBase, int variant)
        {
            var image = ResolveImage(path);
            if (image == null || string.IsNullOrWhiteSpace(iconBase))
            {
                return;
            }

            var sprite = HudOverlayAssetCatalog.LoadTechTreeIcon(iconBase, variant);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
            }
        }

        private void TryResearchTech(int techId)
        {
            var dataManager = DataManager.Instance;
            if (dataManager == null || techId <= 0)
            {
                return;
            }

            lastActiveResearchTechId = techId;
            if (!dataManager.TryStartTechResearch(techId, out var message))
            {
                lastActiveResearchTechId = dataManager.SaveData?.gameplay?.researchingTechId ?? 0;
                HudOverlayService.ShowFloatingWarning(message);
                RefreshPanel();
                return;
            }

            HudOverlayService.ShowFloatingWarning(message);
            RefreshPanel();
            SyncResearchProgressRoutine();
        }
    }
}
