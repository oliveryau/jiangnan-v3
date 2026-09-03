using System.Collections.Generic;
using cfg;
using JN.Client.Config;
using JN.Client.Model;
using QFramework;
using UnityEngine;

namespace JN.Client.Manager
{
    public partial class DataManager
    {
        /// <summary>
        /// 按职位从员工表随机抽取招聘候选（排除已入职）。
        /// </summary>
        public List<Staff> GetHireCandidatesForRole(StaffPosition position, int maxCount = 3)
        {
            return StaffConfigUtility.GetHireCandidates(position, maxCount, excludeHired: true);
        }

        /// <summary>
        /// 固定招聘槽是否存在可购买项：星级已解锁且尚未入职（与招聘卡 btn_Recruit 一致）。
        /// </summary>
        public bool HasPurchasableFixedHireStaff(StaffPosition position, int maxSlots = 3)
        {
            return TryGetNextPurchasableFixedHireStaff(position, maxSlots, out _, out _);
        }

        /// <summary>
        /// 下一个可直招固定槽员工（与招聘界面槽位顺序一致：unlockLevel 1/2/3）。
        /// slotIndex 从 0 起，对应场景挂点「小二雇佣/厨师雇佣{slotIndex+1}」。
        /// </summary>
        public bool TryGetNextPurchasableFixedHireStaff(
            StaffPosition position,
            out Staff staff,
            out int slotIndex)
        {
            return TryGetNextPurchasableFixedHireStaff(position, maxSlots: 3, out staff, out slotIndex);
        }

        /// <summary>
        /// 下一个可直招固定槽员工。
        /// </summary>
        public bool TryGetNextPurchasableFixedHireStaff(
            StaffPosition position,
            int maxSlots,
            out Staff staff,
            out int slotIndex)
        {
            staff = null;
            slotIndex = -1;
            if (position is not (StaffPosition.Chef or StaffPosition.Waiter))
            {
                return false;
            }

            var tavernLevel = Mathf.Max(1, GetTavernLevel());
            var slots = StaffConfigUtility.GetFixedHireSlotStaffs(position, maxSlots);
            for (var index = 0; index < slots.Count; index++)
            {
                var candidate = slots[index];
                if (candidate == null || candidate.UnlockLevel > tavernLevel)
                {
                    continue;
                }

                if (StaffConfigUtility.FindOwnedStaffSave(candidate.Id, preferNonTemporary: true) is { temporary: false })
                {
                    continue;
                }

                staff = candidate;
                slotIndex = index;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 按天赋权重随机抽取招聘候选（排除已入职，不计入刷新次数）。
        /// </summary>
        public List<Staff> RollHireCandidatesForRole(StaffPosition position, int maxCount = -1)
        {
            EnsureStaffRecruitRefreshCountReset();
            if (maxCount <= 0)
            {
                maxCount = TbConfigRuntime.GetStaffRecruitCandidateCount(3);
            }

            return StaffConfigUtility.RollWeightedHireCandidates(position, maxCount, excludeHired: true);
        }

        public void EnsureStaffRecruitRefreshCountReset()
        {
            EnsureGameplayDefaults();
            var gameplay = SaveData.gameplay;
            if (gameplay.staffRecruitRefreshCount <= 0 || gameplay.staffRecruitLastRefreshUnscaledTime <= 0f)
            {
                return;
            }

            var refreshTime = TbConfigRuntime.GetStaffRecruitRefreshTime(300f);
            if (Time.unscaledTime - gameplay.staffRecruitLastRefreshUnscaledTime < refreshTime)
            {
                return;
            }

            gameplay.staffRecruitRefreshCount = 0;
            gameplay.staffRecruitLastRefreshUnscaledTime = 0f;
            SaveGame();
        }

        public int GetStaffRecruitRefreshCost()
        {
            EnsureStaffRecruitRefreshCountReset();
            EnsureGameplayDefaults();
            return TbConfigRuntime.GetStaffRecruitRefreshCostAt(SaveData.gameplay.staffRecruitRefreshCount);
        }

        /// <summary>
        /// 刷新招聘三选一：扣费后按天赋权重重 roll 候选。
        /// </summary>
        public bool TryRefreshStaffRecruitCandidates(
            StaffPosition position,
            out List<Staff> candidates,
            out string message,
            IReadOnlyList<Staff> currentCandidates = null)
        {
            EnsureGameplayDefaults();
            EnsureStaffRecruitRefreshCountReset();
            candidates = null;
            message = string.Empty;

            var maxCount = TbConfigRuntime.GetStaffRecruitCandidateCount(3);
            var cost = GetStaffRecruitRefreshCost();
            if (PlayerData == null || PlayerData.coinNum < cost)
            {
                message = cost > 0 ? $"铜钱不足，刷新需要 {cost}" : "铜钱不足";
                return false;
            }

            candidates = RollRefreshHireCandidates(position, maxCount, currentCandidates);
            if (candidates.Count == 0)
            {
                message = "当前没有可刷新的员工";
                return false;
            }

            if (cost > 0)
            {
                ChangeCoinNum(-cost);
            }

            SaveData.gameplay.staffRecruitRefreshCount += 1;
            SaveData.gameplay.staffRecruitLastRefreshUnscaledTime = Time.unscaledTime;
            SaveGame();
            message = "已刷新员工列表";
            return true;
        }

        private static List<Staff> RollRefreshHireCandidates(
            StaffPosition position,
            int maxCount,
            IReadOnlyList<Staff> currentCandidates)
        {
            if (currentCandidates != null && currentCandidates.Count > 0)
            {
                var extraExcludeIds = new List<int>(currentCandidates.Count);
                for (var index = 0; index < currentCandidates.Count; index++)
                {
                    var staff = currentCandidates[index];
                    if (staff != null && staff.Id > 0)
                    {
                        extraExcludeIds.Add(staff.Id);
                    }
                }

                if (extraExcludeIds.Count > 0)
                {
                    var excludeCurrent = StaffConfigUtility.BuildRecruitExcludeIds(extraExcludeIds);
                    var refreshed = StaffConfigUtility.RollWeightedHireCandidates(position, maxCount, excludeCurrent);
                    if (refreshed.Count > 0)
                    {
                        return refreshed;
                    }
                }
            }

            return StaffConfigUtility.RollWeightedHireCandidates(position, maxCount, excludeHired: true);
        }

        /// <summary>
        /// 场景雇佣 HUD：按职位直接招入下一名未入职员工，使用固定价格覆盖配置招聘费。
        /// </summary>
        public bool TryDirectHireByPosition(StaffPosition position, int cost, out string message, out int hiredStaffId)
        {
            EnsureGameplayDefaults();
            message = string.Empty;
            hiredStaffId = 0;
            if (IsVisitingOtherTavern)
            {
                message = "访客模式下不可雇佣员工";
                return false;
            }

            var maxCount = position switch
            {
                StaffPosition.Chef => Mathf.Min(3, GetGuideUnlockedChefHireCount()),
                StaffPosition.Waiter => Mathf.Min(4, GetGuideUnlockedWaiterHireCount()),
                StaffPosition.Shopkeeper => 1,
                _ => 1
            };
            var hiredCount = CountHiredByPosition(position);
            if (position == StaffPosition.Chef && GetGuideUnlockedChefHireCount() <= 0)
            {
                message = "请先建造灶台后再雇佣厨师";
                return false;
            }

            if (position == StaffPosition.Waiter && GetGuideUnlockedWaiterHireCount() <= 0)
            {
                message = "请先建造桌子后再雇佣小二";
                return false;
            }

            if (hiredCount >= Mathf.Max(1, maxCount))
            {
                message = ResolveStaffHireCapMessage(position);
                return false;
            }

            var candidates = StaffConfigUtility.GetHireCandidates(position, 1, excludeHired: true);
            if (candidates == null || candidates.Count <= 0)
            {
                message = $"暂无可雇佣的{ResolvePositionDisplayName(position)}";
                return false;
            }

            return TryHireConfiguredStaff(candidates[0].Id, out message, out hiredStaffId, cost);
        }

        /// <summary>
        /// 招聘指定 StaffId（固定从 1 级入职），计入对应职位上限。
        /// </summary>
        public bool TryHireConfiguredStaff(int staffId, out string message)
        {
            return TryHireConfiguredStaff(staffId, out message, out _, overrideCost: null);
        }

        /// <summary>
        /// 招聘指定 StaffId；可选覆盖招聘费用（场景雇佣 HUD 用固定价）。
        /// </summary>
        public bool TryHireConfiguredStaff(int staffId, out string message, out int hiredStaffId, int? overrideCost)
        {
            EnsureGameplayDefaults();
            message = string.Empty;
            hiredStaffId = 0;
            if (IsVisitingOtherTavern)
            {
                message = "访客模式下不可雇佣员工";
                return false;
            }

            var config = StaffConfigUtility.GetOrNull(staffId);
            if (config == null)
            {
                message = "员工配置不存在";
                return false;
            }

            if (StaffConfigUtility.FindOwnedStaffSave(staffId, preferNonTemporary: true) is { temporary: false })
            {
                message = $"{(string.IsNullOrWhiteSpace(config.Name) ? "该员工" : config.Name)}已入职";
                return false;
            }

            var position = config.Position;
            // 小二/厨师固定槽：unlockLevel 高于当前酒楼有效等级则不可招（0 级按 1 级算，与名额 cap 一致）。
            if (position is StaffPosition.Chef or StaffPosition.Waiter
                && config.UnlockLevel > 0
                && config.UnlockLevel > Mathf.Max(1, GetTavernLevel()))
            {
                message = $"{config.UnlockLevel}级酒楼解锁";
                return false;
            }

            var displayName = string.IsNullOrWhiteSpace(config.Name) ? "员工" : config.Name;
            var maxCount = position switch
            {
                StaffPosition.Chef => GetGuideUnlockedChefHireCount(),
                StaffPosition.Waiter => GetGuideUnlockedWaiterHireCount(),
                StaffPosition.Shopkeeper => GetMaxShopkeeperHireCount(),
                _ => 1
            };

            var hiredCount = CountHiredByPosition(position);
            if (position == StaffPosition.Chef && maxCount <= 0)
            {
                message = "请先建造灶台后再雇佣厨师";
                return false;
            }

            if (position == StaffPosition.Waiter && maxCount <= 0)
            {
                message = "请先建造桌子后再雇佣小二";
                return false;
            }

            if (hiredCount >= Mathf.Max(1, maxCount))
            {
                message = ResolveStaffHireCapMessage(position);
                return false;
            }

            var cost = overrideCost ?? StaffConfigUtility.GetRecruitmentCost(staffId, 0);
            if (PlayerData == null || PlayerData.coinNum < cost)
            {
                message = $"铜钱不足，招聘{displayName}需要 {cost}";
                return false;
            }

            ChangeCoinNum(-cost);
            if (!SaveData.gameplay.hiredStaffIds.Contains(staffId))
            {
                SaveData.gameplay.hiredStaffIds.Add(staffId);
            }

            var hired = StaffConfigUtility.CreateOwnedStaffSave(staffId, temporary: false);
            SaveData.gameplay.ownedStaff.Add(hired);
            hiredStaffId = staffId;

            // 兼容引导进度标记（同职位任一入职即可）
            var guide = SaveData.gameplay.gameplayGuide;
            if (guide != null)
            {
                if (position == StaffPosition.Shopkeeper)
                {
                    guide.hiredShopkeeper = true;
                }
                else if (position == StaffPosition.Chef)
                {
                    guide.hiredChef = true;
                }
                else if (position == StaffPosition.Waiter)
                {
                    guide.hiredWaiter = true;
                }
            }

            SyncGameplayGuideProgress();
            Signals.Get<GameplayGuideProgressSignal>().Dispatch();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            if (config.StaffTalent > 0)
            {
                RecordStaffTalentUnlocked(config.StaffTalent);
            }

            NotifyAchievementStatsChanged();
            SaveGame();
            message = $"已招聘{displayName}";
            return true;
        }

        /// <summary>
        /// 开场视频结束后赠送默认掌柜（免费、无需招聘界面）。
        /// </summary>
        public void EnsureStarterShopkeeperOwned()
        {
            if (SaveData?.gameplay == null || IsVisitingOtherTavern || isGrantingStarterShopkeeper)
            {
                return;
            }

            if (CountHiredByPosition(StaffPosition.Shopkeeper) > 0)
            {
                return;
            }

            isGrantingStarterShopkeeper = true;
            try
            {
                TryHireConfiguredStaff(StaffConfigUtility.DefaultShopkeeperId, out _, out _, overrideCost: 0);
            }
            finally
            {
                isGrantingStarterShopkeeper = false;
            }
        }

        private static string ResolvePositionDisplayName(StaffPosition position)
        {
            return position switch
            {
                StaffPosition.Shopkeeper => "掌柜",
                StaffPosition.Chef => "厨师",
                StaffPosition.Waiter => "小二",
                _ => "员工"
            };
        }

        /// <summary>
        /// 招聘达上限时的提示：优先说明酒楼星级限制。
        /// </summary>
        private string ResolveStaffHireCapMessage(StaffPosition position)
        {
            var roleName = ResolvePositionDisplayName(position);
            if (position is StaffPosition.Chef or StaffPosition.Waiter)
            {
                var levelCap = GetTavernLevelStaffHireSlotCap();
                var hired = CountHiredByPosition(position);
                if (hired >= levelCap)
                {
                    return "请先升级酒楼";
                }
            }

            if (position == StaffPosition.Waiter && GetUnlockedTableCount() <= 0)
            {
                return "请先建造桌子后再雇佣小二";
            }

            return $"{roleName}已达到招聘上限";
        }

        /// <summary>
        /// 校验是否可解雇指定小二（仅校验，不改存档）。
        /// </summary>
        public bool CanFireWaiter(int staffId, out string message)
        {
            EnsureGameplayDefaults();
            message = string.Empty;
            var config = StaffConfigUtility.GetOrNull(staffId);
            if (config == null)
            {
                message = "员工配置不存在";
                return false;
            }

            if (config.Position != StaffPosition.Waiter)
            {
                message = "只能解雇小二";
                return false;
            }

            if (!TryFindOwnedWaiterSaveIndex(staffId, out _))
            {
                message = "未找到该员工";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 解雇指定小二：从 ownedStaff 移除，允许解雇至 0 人，不退还招聘费。
        /// </summary>
        public bool TryFireWaiter(int staffId, out string message)
        {
            if (!CanFireWaiter(staffId, out message))
            {
                return false;
            }

            var config = StaffConfigUtility.GetOrNull(staffId);
            var displayName = config != null && !string.IsNullOrWhiteSpace(config.Name) ? config.Name : "小二";
            if (!TryFindOwnedWaiterSaveIndex(staffId, out var removeIndex))
            {
                message = "未找到该员工";
                return false;
            }

            SaveData.gameplay.ownedStaff.RemoveAt(removeIndex);
            if (!HasOwnedStaffEntry(staffId))
            {
                SaveData.gameplay.hiredStaffIds.Remove(staffId);
            }

            SyncGameplayGuideProgress();
            Signals.Get<GameplayGuideProgressSignal>().Dispatch();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
            NotifyAchievementStatsChanged();
            SaveGame();
            message = $"已解雇{displayName}";
            return true;
        }

        private bool TryFindOwnedWaiterSaveIndex(int staffId, out int index)
        {
            index = -1;
            var owned = SaveData?.gameplay?.ownedStaff;
            if (owned == null || staffId <= 0)
            {
                return false;
            }

            for (var i = 0; i < owned.Count; i++)
            {
                var save = owned[i];
                if (save == null || save.staffId != staffId)
                {
                    continue;
                }

                var config = StaffConfigUtility.GetOrNull(save.staffId);
                if (config != null && config.Position == StaffPosition.Waiter)
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }

        private bool HasOwnedStaffEntry(int staffId)
        {
            var owned = SaveData?.gameplay?.ownedStaff;
            if (owned == null || staffId <= 0)
            {
                return false;
            }

            for (var i = 0; i < owned.Count; i++)
            {
                var save = owned[i];
                if (save != null && save.staffId == staffId)
                {
                    return true;
                }
            }

            return false;
        }

        public int CountHiredByPosition(StaffPosition position)
        {
            return GetOwnedStaffIdsByPosition(position, includeTemporary: false).Count;
        }

        /// <summary>
        /// 场上可用人数：正式员工 +（小二）临时工。
        /// </summary>
        public int CountActiveFloorStaffByPosition(StaffPosition position)
        {
            return GetOwnedStaffIdsByPosition(position, includeTemporary: true).Count;
        }

        /// <summary>
        /// 按入职顺序返回指定职位的 staffId（用于场景绑定）。
        /// 不调用 EnsureGameplayDefaults，避免与 SyncGameplayGuideProgress 形成递归。
        /// </summary>
        public List<int> GetOwnedStaffIdsByPosition(StaffPosition position, bool includeTemporary = false)
        {
            var result = new List<int>();
            var owned = SaveData?.gameplay?.ownedStaff;
            if (owned == null)
            {
                return result;
            }

            for (var index = 0; index < owned.Count; index++)
            {
                var save = owned[index];
                if (save == null || save.staffId <= 0)
                {
                    continue;
                }

                if (save.temporary && !includeTemporary)
                {
                    continue;
                }

                var config = StaffConfigUtility.GetOrNull(save.staffId);
                if (config != null && config.Position == position)
                {
                    result.Add(save.staffId);
                }
            }

            return result;
        }

        public LocalStaffSaveData GetOwnedStaffSaveAt(int index)
        {
            EnsureGameplayDefaults();
            var list = SaveData.gameplay.ownedStaff;
            if (list == null || index < 0 || index >= list.Count)
            {
                return null;
            }

            return list[index];
        }

        public IReadOnlyList<LocalStaffSaveData> GetOwnedStaffList()
        {
            EnsureGameplayDefaults();
            SaveData.gameplay.ownedStaff ??= new List<LocalStaffSaveData>();
            return SaveData.gameplay.ownedStaff;
        }
    }
}
