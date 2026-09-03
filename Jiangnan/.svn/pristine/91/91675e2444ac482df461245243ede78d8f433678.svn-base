using System.Collections.Generic;
using cfg;
using JN.Client.Manager;
using JN.Client.Model;
using UnityEngine;

namespace JN.Client.Config
{
    /// <summary>
    /// Staff 表读取 + 科技合成 StaffRuntimeProfile。
    /// </summary>
    public static class StaffConfigUtility
    {
        public const int DefaultShopkeeperId = 1;
        public const int DefaultChefId = 4;
        public const int DefaultWaiterId = 5;

        /// <summary>
        /// 招聘随机候选；默认排除已入职员工。
        /// </summary>
        public static List<Staff> GetHireCandidates(StaffPosition position, int maxCount = 3, bool excludeHired = true)
        {
            ICollection<int> exclude = null;
            if (excludeHired)
            {
                exclude = CollectHiredStaffIds();
            }

            return LubanTablesRuntime.GetHireCandidates(position, maxCount, exclude);
        }

        /// <summary>
        /// 按天赋三档权重随机抽取招聘候选（排除已入职）。
        /// </summary>
        public static List<Staff> RollWeightedHireCandidates(StaffPosition position, int maxCount = 3, bool excludeHired = true)
        {
            ICollection<int> exclude = null;
            if (excludeHired)
            {
                exclude = CollectHiredStaffIds();
            }

            return RollWeightedHireCandidates(position, maxCount, exclude);
        }

        public static List<Staff> RollWeightedHireCandidates(
            StaffPosition position,
            int maxCount,
            ICollection<int> excludeStaffIds)
        {
            var pool = BuildAvailableStaffPool(position, excludeStaffIds);
            if (pool.Count == 0 || maxCount <= 0)
            {
                return new List<Staff>();
            }

            var result = new List<Staff>(maxCount);
            var pickedIds = new HashSet<int>();
            for (var index = 0; index < maxCount; index++)
            {
                var tier = StaffTalentConfigUtility.RollRecruitTalentTier();
                var staff = PickRandomStaffForTier(pool, tier, pickedIds)
                            ?? PickRandomStaff(pool, pickedIds);
                if (staff == null)
                {
                    continue;
                }

                result.Add(staff);
                pickedIds.Add(staff.Id);
            }

            return result;
        }

        private static List<Staff> BuildAvailableStaffPool(StaffPosition position, ICollection<int> excludeStaffIds)
        {
            var pool = LubanTablesRuntime.GetStaffByPosition(position);
            if (excludeStaffIds == null || excludeStaffIds.Count == 0)
            {
                return new List<Staff>(pool);
            }

            var available = new List<Staff>(pool.Count);
            for (var index = 0; index < pool.Count; index++)
            {
                var staff = pool[index];
                if (staff != null && !excludeStaffIds.Contains(staff.Id))
                {
                    available.Add(staff);
                }
            }

            return available;
        }

        private static Staff PickRandomStaffForTier(
            IReadOnlyList<Staff> pool,
            StaffRecruitTalentTier tier,
            HashSet<int> pickedIds)
        {
            var matches = new List<Staff>(pool.Count);
            for (var index = 0; index < pool.Count; index++)
            {
                var staff = pool[index];
                if (staff == null || pickedIds.Contains(staff.Id))
                {
                    continue;
                }

                if (StaffTalentConfigUtility.GetRecruitTalentTier(staff) == tier)
                {
                    matches.Add(staff);
                }
            }

            if (matches.Count == 0)
            {
                return null;
            }

            return matches[Random.Range(0, matches.Count)];
        }

        private static Staff PickRandomStaff(IReadOnlyList<Staff> pool, HashSet<int> pickedIds)
        {
            var matches = new List<Staff>(pool.Count);
            for (var index = 0; index < pool.Count; index++)
            {
                var staff = pool[index];
                if (staff != null && !pickedIds.Contains(staff.Id))
                {
                    matches.Add(staff);
                }
            }

            if (matches.Count == 0)
            {
                return null;
            }

            return matches[Random.Range(0, matches.Count)];
        }

        /// <summary>
        /// 指定职位在 Staff 表中的可招聘人数（不同 staffId）。
        /// </summary>
        public static int GetStaffPoolCount(StaffPosition position)
        {
            return LubanTablesRuntime.GetStaffByPosition(position).Count;
        }

        /// <summary>
        /// 小二/厨师固定招聘槽：unlockLevel=1/2/3 各取一人，按等级升序（最多 3 个）。
        /// </summary>
        public static List<Staff> GetFixedHireSlotStaffs(StaffPosition position, int maxSlots = 3)
        {
            var result = new List<Staff>(maxSlots);
            if (position is not (StaffPosition.Waiter or StaffPosition.Chef) || maxSlots <= 0)
            {
                return result;
            }

            var pool = LubanTablesRuntime.GetStaffByPosition(position);
            var byLevel = new Dictionary<int, Staff>();
            for (var index = 0; index < pool.Count; index++)
            {
                var staff = pool[index];
                if (staff == null || staff.UnlockLevel <= 0 || staff.UnlockLevel > maxSlots)
                {
                    continue;
                }

                if (!byLevel.ContainsKey(staff.UnlockLevel))
                {
                    byLevel[staff.UnlockLevel] = staff;
                }
            }

            for (var level = 1; level <= maxSlots; level++)
            {
                if (byLevel.TryGetValue(level, out var staff))
                {
                    result.Add(staff);
                }
            }

            return result;
        }

        /// <summary>
        /// 构建招聘 roll 排除集：已入职 + 可选额外 staffId（如当前展示候选）。
        /// </summary>
        public static HashSet<int> BuildRecruitExcludeIds(IEnumerable<int> extraExcludeStaffIds = null)
        {
            var result = CollectHiredStaffIds();
            if (extraExcludeStaffIds == null)
            {
                return result;
            }

            foreach (var staffId in extraExcludeStaffIds)
            {
                if (staffId > 0)
                {
                    result.Add(staffId);
                }
            }

            return result;
        }

        private static HashSet<int> CollectHiredStaffIds()
        {
            var result = new HashSet<int>();
            var list = DataManager.Instance?.SaveData?.gameplay?.ownedStaff;
            if (list == null)
            {
                return result;
            }

            for (var index = 0; index < list.Count; index++)
            {
                var save = list[index];
                if (save != null && !save.temporary && save.staffId > 0)
                {
                    result.Add(save.staffId);
                }
            }

            return result;
        }

        public static Staff GetOrNull(int staffId)
        {
            return staffId <= 0 ? null : LubanTablesRuntime.GetStaff(staffId);
        }

        public static string GetName(int staffId, string fallback)
        {
            var staff = GetOrNull(staffId);
            return staff != null && !string.IsNullOrWhiteSpace(staff.Name) ? staff.Name : fallback;
        }

        public static string GetVisual(int staffId, string fallback)
        {
            var staff = GetOrNull(staffId);
            return staff != null && !string.IsNullOrWhiteSpace(staff.Visual) ? staff.Visual : fallback;
        }

        public static int GetSalary(int staffId, int fallback)
        {
            var staff = GetOrNull(staffId);
            return staff != null && staff.Salary > 0 ? staff.Salary : fallback;
        }

        /// <summary>
        /// 汇总在职员工当日工资（开业扣款用）。
        /// </summary>
        public static int SumDailySalary(IReadOnlyList<LocalStaffSaveData> ownedStaff)
        {
            if (ownedStaff == null || ownedStaff.Count == 0)
            {
                return 0;
            }

            var total = 0;
            for (var index = 0; index < ownedStaff.Count; index++)
            {
                var save = ownedStaff[index];
                if (save == null || save.staffId <= 0)
                {
                    continue;
                }

                total += StaffTalentConfigUtility.ApplyDailyWageMultiplier(GetSalary(save.staffId, 0));
            }

            return total;
        }

        public static int GetRecruitmentCost(int staffId, int fallback = 0)
        {
            var staff = GetOrNull(staffId);
            var baseCost = staff != null && staff.RecruitmentCosts > 0 ? staff.RecruitmentCosts : fallback;
            return StaffTalentConfigUtility.ApplyRecruitmentCostMultiplier(baseCost);
        }

        public static StaffRole ToStaffRole(StaffPosition position)
        {
            return position switch
            {
                StaffPosition.Chef => StaffRole.Chef,
                StaffPosition.Shopkeeper => StaffRole.Waiter,
                _ => StaffRole.Waiter
            };
        }

        public static int GetDefaultStaffId(StaffPosition position)
        {
            switch (position)
            {
                case StaffPosition.Shopkeeper:
                    return DefaultShopkeeperId;
                case StaffPosition.Chef:
                    return DefaultChefId;
                case StaffPosition.Waiter:
                    return DefaultWaiterId;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 创建入职存档。
        /// </summary>
        public static LocalStaffSaveData CreateOwnedStaffSave(int staffId, bool temporary = false)
        {
            var save = new LocalStaffSaveData
            {
                staffId = (byte)Mathf.Clamp(staffId, 0, 255),
                temporary = temporary,
                remainingHireTime = 0f,
                emotion = StaffRuntimeProfile.DefaultEmotion
            };
            ApplyConfigSkillsToSave(save);
            return save;
        }

        /// <summary>
        /// 从当前已研究科技写入技能解锁字段（与 GetProfile 一致）。
        /// </summary>
        public static void ApplyConfigSkillsToSave(LocalStaffSaveData save)
        {
            if (save == null)
            {
                return;
            }

            var config = GetOrNull(save.staffId);
            var researched = DataManager.Instance?.ResearchedTechIds;
            if (config != null && config.Position is StaffPosition.Waiter or StaffPosition.Chef)
            {
                var snapshot = StaffTechEffectMerger.Merge(config, researched);
                save.skillOrderUnlocked = snapshot.CanOrder;
                save.skillServeUnlocked = snapshot.CanServe;
                save.skillCheckoutUnlocked = snapshot.CanCheckout;
            }
            else
            {
                save.skillOrderUnlocked = false;
                save.skillServeUnlocked = false;
                save.skillCheckoutUnlocked = false;
            }

            if (save.emotion <= 0f)
            {
                save.emotion = StaffRuntimeProfile.DefaultEmotion;
            }

            save.skillsInitialized = true;
        }

        /// <summary>
        /// 按当前已研究科技，同步全体在职员工存档技能字段。
        /// </summary>
        public static void RefreshAllOwnedStaffSkillsFromTech()
        {
            var list = DataManager.Instance?.SaveData?.gameplay?.ownedStaff;
            if (list == null)
            {
                return;
            }

            for (var index = 0; index < list.Count; index++)
            {
                var save = list[index];
                if (save == null || save.staffId <= 0)
                {
                    continue;
                }

                ApplyConfigSkillsToSave(save);
            }
        }

        public static LocalStaffSaveData FindOwnedStaffSave(int staffId, bool preferNonTemporary = true)
        {
            var list = DataManager.Instance?.SaveData?.gameplay?.ownedStaff;
            if (list == null || staffId <= 0)
            {
                return null;
            }

            LocalStaffSaveData temporaryMatch = null;
            for (var index = 0; index < list.Count; index++)
            {
                var save = list[index];
                if (save == null || save.staffId != staffId)
                {
                    continue;
                }

                EnsureSaveSkillsInitialized(save);
                if (!preferNonTemporary || !save.temporary)
                {
                    return save;
                }

                temporaryMatch ??= save;
            }

            return temporaryMatch;
        }

        public static void EnsureSaveSkillsInitialized(LocalStaffSaveData save)
        {
            if (save == null || save.skillsInitialized)
            {
                return;
            }

            ApplyConfigSkillsToSave(save);
        }

        /// <summary>
        /// 合成运行时画像：Staff 基线 × 已研究员工科技。
        /// </summary>
        public static StaffRuntimeProfile GetProfile(int staffId, LocalStaffSaveData saveOverride = null)
        {
            if (staffId <= 0)
            {
                return CreateFallbackProfile(0);
            }

            var config = GetOrNull(staffId);
            var save = saveOverride ?? FindOwnedStaffSave(staffId);
            if (save != null)
            {
                EnsureSaveSkillsInitialized(save);
            }

            if (config == null && save == null)
            {
                return CreateFallbackProfile(staffId);
            }

            var position = config != null ? config.Position : StaffPosition.Waiter;
            var researched = DataManager.Instance?.ResearchedTechIds;
            bool canOrder;
            bool canServe;
            bool canCheckout;
            float cookMul = 1f;
            float orderMul = 1f;
            float serveMul = 1f;
            float checkoutMul = 1f;
            float cleanMul = 1f;
            float moveMul = 1f;

            if (config != null && config.Position is StaffPosition.Waiter or StaffPosition.Chef)
            {
                var snapshot = StaffTechEffectMerger.Merge(config, researched);
                canOrder = snapshot.CanOrder;
                canServe = snapshot.CanServe;
                canCheckout = snapshot.CanCheckout;
                cookMul = snapshot.CookSpeedMul;
                orderMul = snapshot.OrderTimeMul;
                serveMul = snapshot.ServeTimeMul;
                checkoutMul = snapshot.CheckoutTimeMul;
                cleanMul = snapshot.CleanTimeMul;
                moveMul = snapshot.MoveSpeedMul;
            }
            else if (save != null)
            {
                canOrder = save.skillOrderUnlocked;
                canServe = save.skillServeUnlocked;
                canCheckout = save.skillCheckoutUnlocked;
            }
            else
            {
                canOrder = false;
                canServe = false;
                canCheckout = false;
            }

            var baseMove = config != null ? config.MoveSpeed : StaffRuntimeProfile.DefaultMoveSpeed;
            var management = StaffTalentConfigUtility.BuildManagementMultipliers();
            var talentMoveMul = 1f;
            var orderTalentMul = 1f;
            var serveTalentMul = 1f;
            var checkoutTalentMul = 1f;
            var cleanTalentMul = 1f;
            var cookTalentMul = 1f;

            if (config != null && config.Position == StaffPosition.Waiter)
            {
                var allServiceTimeMul = StaffTalentConfigUtility.GetPersonalAllServiceTimeMultiplier(config);
                orderTalentMul = StaffTalentConfigUtility.GetPersonalOrderTimeMultiplier(config)
                                 * allServiceTimeMul
                                 * management.WaiterOrderTimeMul;
                serveTalentMul = StaffTalentConfigUtility.GetPersonalServeTimeMultiplier(config)
                                 * allServiceTimeMul
                                 * management.WaiterServeTimeMul;
                checkoutTalentMul = StaffTalentConfigUtility.GetPersonalCheckoutTimeMultiplier(config)
                                    * allServiceTimeMul
                                    * management.WaiterCheckoutTimeMul;
                cleanTalentMul = StaffTalentConfigUtility.GetPersonalCleanTimeMultiplier(config)
                                 * allServiceTimeMul
                                 * management.WaiterCleanTimeMul;
                talentMoveMul = StaffTalentConfigUtility.GetWaiterMoveSpeedMultiplier(config)
                                * management.WaiterMoveSpeedMul;
            }
            else if (config != null && config.Position == StaffPosition.Chef)
            {
                cookTalentMul = StaffTalentConfigUtility.GetChefCookSpeedMultiplier(config)
                                * management.ChefCookSpeedMul;
            }

            orderMul *= orderTalentMul;
            serveMul *= serveTalentMul;
            checkoutMul *= checkoutTalentMul;
            cleanMul *= cleanTalentMul;
            cookMul *= cookTalentMul;

            var moveSpeed = baseMove * talentMoveMul * moveMul;
            const int defaultAttitude = 50;
            const int defaultPersonality = 50;
            var attitude = defaultAttitude;
            var personality = defaultPersonality;

            var emotion = save != null ? save.emotion : StaffRuntimeProfile.DefaultEmotion;
            var temporary = save != null && save.temporary;
            var name = config != null && !string.IsNullOrWhiteSpace(config.Name)
                ? config.Name
                : $"员工{staffId}";

            return new StaffRuntimeProfile(
                staffId,
                name,
                position,
                ToStaffRole(position),
                canOrder,
                canServe,
                canCheckout,
                moveSpeed,
                attitude,
                personality,
                emotion,
                1,
                temporary,
                cookMul,
                orderMul,
                serveMul,
                checkoutMul,
                cleanMul,
                0,
                1f);
        }

        public static string BuildNextStaffTechHint(StaffPosition position)
        {
            var dataManager = DataManager.Instance;
            if (dataManager == null)
            {
                return "前往生财策学习下一项能力";
            }

            var next = StaffTechEffectMerger.GetNextStaffTech(position, dataManager.IsTechResearched);
            if (next == null)
            {
                if (position == StaffPosition.Shopkeeper)
                {
                    return dataManager.IsCounterRandomRewardEnabled()
                        ? "柜台随机进账已开启，营业中点击掌柜头顶气泡领取"
                        : "柜台随机进账请在生财策中学习「柜台进账」";
                }

                return "该职位生财策已全部学习";
            }

            return $"下一项生财策：{next.Name}（{StaffTechEffectMerger.DescribeStaffEffect(next)}）";
        }

        /// <summary>
        /// 解锁一项服务技能（供调试或特殊事件）。
        /// </summary>
        public static bool UnlockSkill(int staffId, string skillKey)
        {
            var save = FindOwnedStaffSave(staffId);
            if (save == null)
            {
                return false;
            }

            EnsureSaveSkillsInitialized(save);
            switch (skillKey)
            {
                case "Order":
                    save.skillOrderUnlocked = true;
                    break;
                case "Serve":
                    save.skillServeUnlocked = true;
                    break;
                case "Checkout":
                    save.skillCheckoutUnlocked = true;
                    break;
                default:
                    return false;
            }

            DataManager.Instance?.SaveGame();
            return true;
        }

        public static void SetEmotion(int staffId, float emotion)
        {
            var save = FindOwnedStaffSave(staffId);
            if (save == null)
            {
                return;
            }

            EnsureSaveSkillsInitialized(save);
            save.emotion = Mathf.Clamp(emotion, 0f, 100f);
            DataManager.Instance?.SaveGame();
        }

        private static StaffRuntimeProfile CreateFallbackProfile(int staffId)
        {
            return new StaffRuntimeProfile(
                staffId,
                staffId > 0 ? $"员工{staffId}" : "员工",
                StaffPosition.Waiter,
                StaffRole.Waiter,
                false,
                false,
                false,
                StaffRuntimeProfile.DefaultMoveSpeed,
                50,
                50,
                StaffRuntimeProfile.DefaultEmotion,
                1,
                false);
        }
    }
}
