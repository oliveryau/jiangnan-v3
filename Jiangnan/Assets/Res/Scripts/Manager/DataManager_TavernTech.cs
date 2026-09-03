using System;
using System.Collections.Generic;
using cfg;
using JN.Client;
using JN.Client.Config;
using JN.Client.Model;
using JN.Client.Scene;
using JN.Client.UI;
using QFramework;
using UnityEngine;

namespace JN.Client.Manager
{
    public partial class DataManager
    {
        /// <summary>
        /// 已研究科技 Id 列表（只读视图）。
        /// </summary>
        public IReadOnlyList<int> ResearchedTechIds
        {
            get
            {
                EnsureGameplayDefaults();
                SaveData.gameplay.researchedTechIds ??= new List<int>();
                return SaveData.gameplay.researchedTechIds;
            }
        }

        public bool IsTechResearched(int techId)
        {
            if (techId <= 0)
            {
                return false;
            }

            EnsureGameplayDefaults();
            var list = SaveData.gameplay.researchedTechIds;
            return list != null && list.Contains(techId);
        }

        private IEnumerable<int> GetResearchedTechIdsOrEmpty()
        {
            EnsureGameplayDefaults();
            return SaveData.gameplay.researchedTechIds ?? (IEnumerable<int>)System.Array.Empty<int>();
        }

        public int GetTechExtraStaffCap(StaffPosition position)
        {
            var techType = TavernTechConfigUtility.CapTypeForPosition(position);
            if (techType == TavernTechType.Custom)
            {
                return 0;
            }

            return TavernTechConfigUtility.SumExtraCap(techType, GetResearchedTechIdsOrEmpty());
        }

        public int GetMaxWaiterHireCount()
        {
            return Mathf.Max(1, StaffConfigUtility.GetStaffPoolCount(StaffPosition.Waiter))
                   + GetTechExtraStaffCap(StaffPosition.Waiter);
        }

        public int GetMaxChefHireCount()
        {
            return Mathf.Max(1, StaffConfigUtility.GetStaffPoolCount(StaffPosition.Chef))
                   + GetTechExtraStaffCap(StaffPosition.Chef);
        }

        public int GetMaxShopkeeperHireCount()
        {
            return 1;
        }

        /// <summary>排队容量加成。</summary>
        public int GetTechQueueCapBonus()
        {
            return Mathf.Max(0, TavernTechConfigUtility.SumAdditive(TavernTechType.QueueCap, GetResearchedTechIdsOrEmpty()));
        }

        /// <summary>营业时长额外秒数。</summary>
        public float GetTechBusinessHoursBonusSeconds()
        {
            return Mathf.Max(0, TavernTechConfigUtility.SumAdditive(TavernTechType.BusinessHoursBonus, GetResearchedTechIdsOrEmpty()));
        }

        /// <summary>刷客间隔减秒（研究后从 customerRefreshTime 扣除）。</summary>
        public float GetTechCustomerRefreshSecondsBonus()
        {
            return Mathf.Max(0f, TavernTechConfigUtility.SumAdditive(TavernTechType.CustomerRefreshSecondsBonus, GetResearchedTechIdsOrEmpty()));
        }

        /// <summary>营业时长倍率（千分比相乘，默认 1）。</summary>
        public float GetTechBusinessHoursMul()
        {
            return TavernTechConfigUtility.ProductPermilleMul(TavernTechType.BusinessHoursMul, GetResearchedTechIdsOrEmpty());
        }

        public bool IsVipCustomerEnabled()
        {
            return TavernTechConfigUtility.HasResearchedTechType(TavernTechType.EnableVipCustomer, GetResearchedTechIdsOrEmpty());
        }

        public bool IsVisitCustomerEnabled()
        {
            return TavernTechConfigUtility.HasResearchedTechType(TavernTechType.EnableVisitCustomer, GetResearchedTechIdsOrEmpty());
        }

        public bool IsCounterRandomRewardEnabled()
        {
            return StaffTechEffectMerger.IsCounterRandomRewardEnabled(GetResearchedTechIdsOrEmpty());
        }

        /// <summary>刷客间隔倍率（相乘，默认 1）。</summary>
        public float GetTechCustomerRefreshMul()
        {
            return TavernTechConfigUtility.ProductPermilleMul(TavernTechType.CustomerRefreshMul, GetResearchedTechIdsOrEmpty());
        }

        /// <summary>研究耗时倍率（相乘，默认 1）。</summary>
        public float GetTechResearchSpeedMul()
        {
            return TavernTechConfigUtility.ProductPermilleMul(TavernTechType.ResearchSpeedMul, GetResearchedTechIdsOrEmpty());
        }

        /// <summary>全店小费加成百分比之和。</summary>
        public int GetTechTipBonusPercent()
        {
            return Mathf.Max(0, TavernTechConfigUtility.SumAdditive(TavernTechType.TipGlobalBonus, GetResearchedTechIdsOrEmpty()));
        }

        /// <summary>涨价额外利润百分比之和。</summary>
        public int GetTechPriceProfitBonusPercent()
        {
            return Mathf.Max(0, TavernTechConfigUtility.SumAdditive(TavernTechType.PriceProfitBonus, GetResearchedTechIdsOrEmpty()));
        }

        /// <summary>鼓舞客流额外百分比之和。</summary>
        public int GetTechInspireBonusPercent()
        {
            return Mathf.Max(0, TavernTechConfigUtility.SumAdditive(TavernTechType.InspireBonus, GetResearchedTechIdsOrEmpty()));
        }

        /// <summary>
        /// 除铜钱外是否满足开始研究条件（用于研究按钮可点，点击后再校验铜钱并提示）。
        /// </summary>
        public bool CanAttemptTechResearch(int techId, out string message)
        {
            message = string.Empty;
            EnsureGameplayDefaults();
            var tech = TavernTechConfigUtility.Get(techId);
            if (tech == null)
            {
                message = "生财策不存在";
                return false;
            }

            if (techId == TavernTechConfigUtility.LockedSecondFloorTechId)
            {
                message = "暂未开放";
                return false;
            }

            if (IsTechResearched(techId))
            {
                message = "已投资该生财策";
                return false;
            }

            if (SaveData.gameplay.researchingTechId > 0)
            {
                message = "已有生财策正在投资中";
                return false;
            }

            if (!TavernTechConfigUtility.MeetsPrerequisites(tech, IsTechResearched))
            {
                message = "前置生财策未完成";
                return false;
            }

            return true;
        }

        public bool CanResearchTech(int techId, out string message)
        {
            if (!CanAttemptTechResearch(techId, out message))
            {
                return false;
            }

            var tech = TavernTechConfigUtility.Get(techId);
            if (PlayerData == null || PlayerData.coinNum < tech.Cost)
            {
                message = $"铜钱不足，投资需要 {tech.Cost}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 是否存在可立即开始研究的科技（无进行中研究、前置与铜钱均满足）。
        /// </summary>
        public bool HasResearchableTech()
        {
            return TryGetRecommendedResearchableTech(out _);
        }

        /// <summary>
        /// 生财策入口红点：至少四桌客人结账后，且存在可立即研究的科技。
        /// </summary>
        public bool ShouldShowTechEntryRedDot()
        {
            EnsureTavernDefaults();
            if (SaveData.tavern.totalServedCustomers < TechEntryUnlockCheckoutCount)
            {
                return false;
            }

            return HasResearchableTech();
        }

        /// <summary>
        /// 按主面板顺序返回首个可立即研究的科技 Id。
        /// </summary>
        public bool TryGetRecommendedResearchableTech(out int techId)
        {
            techId = 0;
            EnsureGameplayDefaults();
            if (SaveData.gameplay.researchingTechId > 0)
            {
                return false;
            }

            var techIds = TavernTechConfigUtility.CollectMainPanelTechIds();
            for (var index = 0; index < techIds.Count; index++)
            {
                var candidate = techIds[index];
                if (CanResearchTech(candidate, out _))
                {
                    techId = candidate;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 开始研究科技：扣费后按 researchTime 倒计时；需求受 ResearchSpeedMul 影响，0 秒则立即完成。
        /// </summary>
        public bool TryStartTechResearch(int techId, out string message)
        {
            if (!CanResearchTech(techId, out message))
            {
                return false;
            }

            var tech = TavernTechConfigUtility.Get(techId);
            PlayerData.coinNum -= tech.Cost;
            var durationSeconds = GetTechResearchDurationSeconds(tech);
            if (durationSeconds <= 0)
            {
                CompleteTechResearch(techId);
                message = "投资完成";
                return true;
            }

            SaveData.gameplay.researchingTechId = techId;
            SaveData.gameplay.researchEndUnixTime = GetUtcNowSeconds() + durationSeconds;
            SaveGame();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            message = "开始投资";
            return true;
        }

        /// <summary>
        /// 立即完成科技研究（扣费并跳过倒计时）。
        /// </summary>
        public bool TryInstantUnlockTech(int techId, out string message)
        {
            if (!CanResearchTech(techId, out message))
            {
                return false;
            }

            var tech = TavernTechConfigUtility.Get(techId);
            PlayerData.coinNum -= tech.Cost;
            CompleteTechResearch(techId);
            message = tech != null && !string.IsNullOrWhiteSpace(tech.Name)
                ? $"已解锁{tech.Name}"
                : "投资完成";
            return true;
        }

        /// <summary>
        /// 推进研究倒计时；时间到达后自动完成科技。
        /// </summary>
        public void TickTechResearch()
        {
            EnsureInitializedCore();
            SaveData.gameplay ??= new LocalGameplaySaveData();
            SaveData.gameplay.researchedTechIds ??= new List<int>();

            var techId = SaveData.gameplay.researchingTechId;
            if (techId <= 0)
            {
                return;
            }

            var tech = TavernTechConfigUtility.Get(techId);
            if (tech == null)
            {
                SaveData.gameplay.researchingTechId = 0;
                SaveData.gameplay.researchEndUnixTime = 0d;
                SaveGame();
                return;
            }

            EnsureTechResearchEndTimeInitialized(techId, tech);
            if (GetTechResearchRemainingSeconds() <= 0f)
            {
                CompleteTechResearch(techId);
            }
        }

        /// <summary>
        /// 当前是否有进行中的科技研究，并返回 progress/required（已过秒数/总秒数）。
        /// </summary>
        public bool TryGetTechResearchProgress(out int progress, out int required)
        {
            progress = 0;
            required = 0;
            EnsureGameplayDefaults();
            var techId = SaveData.gameplay.researchingTechId;
            if (techId <= 0)
            {
                return false;
            }

            var tech = TavernTechConfigUtility.Get(techId);
            if (tech == null)
            {
                return false;
            }

            required = GetTechResearchDurationSeconds(tech);
            if (required <= 0)
            {
                return false;
            }

            EnsureTechResearchEndTimeInitialized(techId, tech);
            var remaining = GetTechResearchRemainingSeconds();
            var elapsed = Mathf.Clamp(GetTechResearchDurationSeconds(tech) - remaining, 0f, required);
            progress = Mathf.Clamp(Mathf.FloorToInt(elapsed), 0, required);
            return true;
        }

        /// <summary>
        /// 当前研究进度（0–1，基于剩余时间连续插值，供 UI fillAmount 平滑刷新）。
        /// </summary>
        public bool TryGetTechResearchFillAmount(out float fillAmount)
        {
            fillAmount = 0f;
            EnsureGameplayDefaults();
            var techId = SaveData.gameplay.researchingTechId;
            if (techId <= 0)
            {
                return false;
            }

            var tech = TavernTechConfigUtility.Get(techId);
            if (tech == null)
            {
                return false;
            }

            var duration = GetTechResearchDurationSeconds(tech);
            if (duration <= 0f)
            {
                return false;
            }

            EnsureTechResearchEndTimeInitialized(techId, tech);
            var remaining = GetTechResearchRemainingSeconds();
            var elapsed = Mathf.Clamp(duration - remaining, 0f, duration);
            fillAmount = Mathf.Clamp01(elapsed / duration);
            return true;
        }

        public int GetTechResearchDurationSeconds(TavernTech tech)
        {
            if (tech == null)
            {
                return 0;
            }

            return Mathf.Max(0, Mathf.CeilToInt(tech.ResearchTime * GetTechResearchSpeedMul()));
        }

        public int GetTechResearchDurationSeconds(int techId)
        {
            return GetTechResearchDurationSeconds(TavernTechConfigUtility.Get(techId));
        }

        public float GetTechResearchRemainingSeconds()
        {
            EnsureGameplayDefaults();
            if (SaveData.gameplay.researchingTechId <= 0)
            {
                return 0f;
            }

            return Mathf.Max(0f, (float)(SaveData.gameplay.researchEndUnixTime - GetUtcNowSeconds()));
        }

        public void CompleteTechResearch(int techId)
        {
            EnsureGameplayDefaults();
            var isNewResearch = techId > 0 && !SaveData.gameplay.researchedTechIds.Contains(techId);
            var tech = TavernTechConfigUtility.Get(techId);
            SaveData.gameplay.researchedTechIds ??= new List<int>();
            if (techId > 0 && !SaveData.gameplay.researchedTechIds.Contains(techId))
            {
                SaveData.gameplay.researchedTechIds.Add(techId);
            }

            if (SaveData.gameplay.researchingTechId == techId)
            {
                SaveData.gameplay.researchingTechId = 0;
                SaveData.gameplay.researchEndUnixTime = 0d;
            }

            StaffConfigUtility.RefreshAllOwnedStaffSkillsFromTech();
            SaveGame();
            if (techId == 202)
            {
                TavernSceneManager.Instance?.RefreshTimingConfig();
            }

            if (IsCounterRandomRewardEnabled())
            {
                TavernSceneManager.Instance?.RefreshCounterRandomReward();
            }

            if (isNewResearch)
            {
                var displayName = string.IsNullOrWhiteSpace(tech?.Name) ? "生财策" : tech.Name;
                Signals.Get<TechResearchCompletedSignal>().Dispatch(displayName);
                if (tech != null && tech.TechType == TavernTechType.EnableVisitCustomer)
                {
                    TavernSceneManager.Instance?.NotifyVisitCustomerTechUnlocked();
                }

                if (tech != null && tech.TechType == TavernTechType.EnableVipCustomer)
                {
                    TavernSceneManager.Instance?.NotifyVipCustomerTechUnlocked();
                }

                TavernSceneManager.Instance?.PlayTechUnlockStaffFootEffect(tech);
            }

            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
        }

        /// <summary>
        /// 移除无效科技 Id，并重置无效中的 researchingTechId。
        /// </summary>
        public int SanitizeResearchedTechIds()
        {
            EnsureGameplayDefaults();
            var list = SaveData.gameplay.researchedTechIds;
            if (list == null || list.Count == 0)
            {
                return 0;
            }

            var removed = 0;
            for (var index = list.Count - 1; index >= 0; index--)
            {
                var techId = list[index];
                if (techId > 0 && TavernTechConfigUtility.Get(techId) != null)
                {
                    continue;
                }

                list.RemoveAt(index);
                removed++;
            }

            SanitizeResearchingTechId();
            return removed;
        }

        private void SanitizeResearchingTechId()
        {
            EnsureGameplayDefaults();
            var researchingId = SaveData.gameplay.researchingTechId;
            if (researchingId <= 0)
            {
                return;
            }

            if (TavernTechConfigUtility.Get(researchingId) != null)
            {
                return;
            }

            SaveData.gameplay.researchingTechId = 0;
            SaveData.gameplay.researchEndUnixTime = 0d;
        }

        private void EnsureTechResearchEndTimeInitialized(int techId, TavernTech tech)
        {
            if (SaveData.gameplay.researchEndUnixTime > 0d)
            {
                return;
            }

            var durationSeconds = GetTechResearchDurationSeconds(tech);
            SaveData.gameplay.researchEndUnixTime = GetUtcNowSeconds() + durationSeconds;
            SaveGame();
        }

        private static double GetUtcNowSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
        }
    }
}
