using System.Collections.Generic;
using cfg;
using JN.Client.Manager;
using JN.Client.Model;
using JN.Client.Scene;
using UnityEngine;

namespace JN.Client.Config
{
    public enum StaffRecruitTalentTier
    {
        Negative = 0,
        Normal = 1,
        Excellent = 2
    }

    /// <summary>
    /// StaffTalent 表读取与天赋效果解析（缩减范围：1001–1006、1302–1312、2001–2002）。
    /// </summary>
    public static class StaffTalentConfigUtility
    {
        public struct ManagementTalentMultipliers
        {
            public float WaiterOrderTimeMul;
            public float WaiterServeTimeMul;
            public float WaiterCheckoutTimeMul;
            public float WaiterCleanTimeMul;
            public float WaiterMoveSpeedMul;
            public float ChefCookSpeedMul;
            public float RecruitmentCostMul;
            public float DailyWageMul;
            public int VipSpawnChanceBonusPercent;

            public static ManagementTalentMultipliers Identity => new()
            {
                WaiterOrderTimeMul = 1f,
                WaiterServeTimeMul = 1f,
                WaiterCheckoutTimeMul = 1f,
                WaiterCleanTimeMul = 1f,
                WaiterMoveSpeedMul = 1f,
                ChefCookSpeedMul = 1f,
                RecruitmentCostMul = 1f,
                DailyWageMul = 1f,
                VipSpawnChanceBonusPercent = 0
            };
        }

        public static StaffTalent GetTalent(int talentId)
        {
            return talentId <= 0 ? null : LubanTablesRuntime.GetStaffTalent(talentId);
        }

        public static StaffTalent GetTalent(Staff staff)
        {
            return staff == null ? null : GetTalent(staff.StaffTalent);
        }

        /// <summary>
        /// 招聘刷新：按天赋 param[0] 划分负/平/优三档。
        /// </summary>
        public static StaffRecruitTalentTier GetRecruitTalentTier(Staff staff)
        {
            var talent = GetTalent(staff);
            if (talent?.Param == null || talent.Param.Count == 0)
            {
                return StaffRecruitTalentTier.Normal;
            }

            var param0 = talent.Param[0];
            if (param0 < 0)
            {
                return StaffRecruitTalentTier.Negative;
            }

            return param0 > 0 ? StaffRecruitTalentTier.Excellent : StaffRecruitTalentTier.Normal;
        }

        public static StaffRecruitTalentTier RollRecruitTalentTier()
        {
            TbConfigRuntime.GetStaffRecruitTalentWeights(out var negative, out var normal, out var excellent);
            var total = negative + normal + excellent;
            if (total <= 0)
            {
                return StaffRecruitTalentTier.Normal;
            }

            var roll = Random.Range(0, total);
            if (roll < negative)
            {
                return StaffRecruitTalentTier.Negative;
            }

            roll -= negative;
            return roll < normal ? StaffRecruitTalentTier.Normal : StaffRecruitTalentTier.Excellent;
        }

        /// <summary>
        /// 员工信息展示：优先天赋名，缺省回退 remark。
        /// </summary>
        public static string GetStaffDisplayInfoText(Staff staff)
        {
            if (staff == null)
            {
                return string.Empty;
            }

            var talent = GetTalent(staff);
            if (talent != null && !string.IsNullOrWhiteSpace(talent.Name))
            {
                return talent.Name.Trim();
            }

            return string.IsNullOrWhiteSpace(staff.Remark) ? string.Empty : staff.Remark.Trim();
        }

        /// <summary>
        /// 天赋描述弹窗文案：标题为天赋名，内容为 StaffTalent.desc。
        /// </summary>
        public static bool TryGetStaffTalentDescPopup(Staff staff, out string title, out string content)
        {
            title = string.Empty;
            content = string.Empty;
            if (staff == null)
            {
                return false;
            }

            var talent = GetTalent(staff);
            if (talent == null)
            {
                return false;
            }

            title = GetStaffDisplayInfoText(staff);
            if (string.IsNullOrWhiteSpace(title))
            {
                title = "天赋";
            }

            content = string.IsNullOrWhiteSpace(talent.Desc) ? "暂无描述" : talent.Desc.Trim();
            return true;
        }

        /// <summary>
        /// 天赋品质档次：0 灰 / 1 绿 / 2 紫 / 3 金。
        /// </summary>
        public static int GetTalentQualityGrade(Staff staff)
        {
            return GetTalentQualityGrade(GetTalent(staff));
        }

        public static int GetTalentQualityGrade(StaffTalent talent)
        {
            if (talent == null || talent.Id <= 0 || talent.Id == 1000)
            {
                return 1;
            }

            var param0 = GetParam0(talent);
            if (param0 < 0)
            {
                return 0;
            }

            if (param0 == 0)
            {
                return 1;
            }

            return IsGoldTalentGrade(talent) ? 3 : 2;
        }

        public static Color GetTalentQualityColor(int grade, bool selected = false)
        {
            var clamped = Mathf.Clamp(grade, 0, 3);
            var color = clamped switch
            {
                0 => new Color(0.52f, 0.52f, 0.52f, 1f),
                1 => new Color(0.22f, 0.62f, 0.28f, 1f),
                2 => new Color(0.58f, 0.32f, 0.78f, 1f),
                _ => new Color(0.86f, 0.62f, 0.14f, 1f)
            };

            if (selected)
            {
                color = Color.Lerp(color, Color.white, 0.15f);
            }

            return color;
        }

        private static bool IsGoldTalentGrade(StaffTalent talent)
        {
            if (talent.Id >= 10001)
            {
                return true;
            }

            if (talent.Id is >= 1302 and <= 1312)
            {
                return true;
            }

            if (GetParam0(talent) >= 20)
            {
                return true;
            }

            return talent.TalentType >= StaffTalentType.CarryDishCapacitySet;
        }

        /// <summary>
        /// 天赋效果是否已在运行时接通（用于 UI 区分展示）。
        /// </summary>
        public static bool IsRuntimeImplemented(StaffTalent talent)
        {
            if (talent == null)
            {
                return false;
            }

            switch (talent.TalentType)
            {
                case StaffTalentType.MoveSpeedBonusPercent:
                case StaffTalentType.OrderSpeedBonusPercent:
                case StaffTalentType.ServeSpeedBonusPercent:
                case StaffTalentType.CheckoutSpeedBonusPercent:
                case StaffTalentType.CleanSpeedBonusPercent:
                case StaffTalentType.AllServiceSpeedBonusPercent:
                case StaffTalentType.WaiterAllServiceSpeedBonusPercent:
                case StaffTalentType.ChefAllCookingSpeedBonusPercent:
                case StaffTalentType.TeamOccupancyEfficiencyBonus:
                case StaffTalentType.TeamCleanSpeedBonusPercent:
                case StaffTalentType.TeamServeSpeedBonusPercent:
                case StaffTalentType.TeamCheckoutSpeedBonusPercent:
                case StaffTalentType.TeamOrderSpeedBonusPercent:
                case StaffTalentType.RecruitmentCostReductionPercent:
                case StaffTalentType.DailyWageReductionPercent:
                case StaffTalentType.VipAttractionBonusPercent:
                case StaffTalentType.TavernAllWorkSpeedBonusPercent:
                case StaffTalentType.ChefPrepSpeedBonusPercent:
                case StaffTalentType.ExtraDishChancePercent:
                    return true;
                default:
                    return false;
            }
        }

        public static string BuildTalentEffectSummary(Staff staff)
        {
            var talent = GetTalent(staff);
            if (talent == null)
            {
                return string.Empty;
            }

            if (!IsRuntimeImplemented(talent))
            {
                return string.IsNullOrWhiteSpace(talent.Desc) ? talent.Name : talent.Desc;
            }

            var param0 = GetParam0(talent);
            return talent.TalentType switch
            {
                StaffTalentType.MoveSpeedBonusPercent => $"移速 +{param0}%",
                StaffTalentType.OrderSpeedBonusPercent => $"点单耗时 -{param0}%",
                StaffTalentType.ServeSpeedBonusPercent => $"上菜耗时 -{param0}%",
                StaffTalentType.CheckoutSpeedBonusPercent => $"收账耗时 -{param0}%",
                StaffTalentType.CleanSpeedBonusPercent => $"清扫耗时 -{param0}%",
                StaffTalentType.AllServiceSpeedBonusPercent => $"全服务耗时 -{param0}%",
                StaffTalentType.WaiterAllServiceSpeedBonusPercent => $"全员小二全服务 -{param0}%",
                StaffTalentType.ChefAllCookingSpeedBonusPercent => $"全员厨师烹饪 +{param0}%",
                StaffTalentType.TeamOccupancyEfficiencyBonus =>
                    $"上座率≥{GetParam0(talent)}% 时团队效率 +{GetParam1(talent)}%",
                StaffTalentType.TeamCleanSpeedBonusPercent => $"团队清扫 -{param0}%",
                StaffTalentType.TeamServeSpeedBonusPercent => $"团队上菜 -{param0}%",
                StaffTalentType.TeamCheckoutSpeedBonusPercent => $"团队收账 -{param0}%",
                StaffTalentType.TeamOrderSpeedBonusPercent => $"团队点单 -{param0}%",
                StaffTalentType.RecruitmentCostReductionPercent => $"招聘费 -{param0}%",
                StaffTalentType.DailyWageReductionPercent => $"日薪 -{param0}%",
                StaffTalentType.VipAttractionBonusPercent => $"贵客吸引力 +{param0}%",
                StaffTalentType.TavernAllWorkSpeedBonusPercent => $"全店效率 +{param0}%",
                StaffTalentType.ChefPrepSpeedBonusPercent => $"烹饪 +{param0}%",
                StaffTalentType.ExtraDishChancePercent => $"待做订单≥2时，{param0}% 一次出2份",
                _ => string.IsNullOrWhiteSpace(talent.Desc) ? talent.Name : talent.Desc
            };
        }

        public static float GetWaiterMoveSpeedMultiplier(Staff staff)
        {
            if (staff == null || staff.Position != StaffPosition.Waiter)
            {
                return 1f;
            }

            var talent = GetTalent(staff);
            if (talent == null)
            {
                return 1f;
            }

            var mul = 1f;
            if (talent.TalentType == StaffTalentType.MoveSpeedBonusPercent
                || talent.TalentType == StaffTalentType.AllServiceSpeedBonusPercent)
            {
                mul *= PercentToMoveSpeedMul(GetParam0(talent));
            }

            return mul;
        }

        public static float GetPersonalOrderTimeMultiplier(Staff staff)
        {
            return GetPersonalWaiterTimeMultiplier(staff, StaffTalentType.OrderSpeedBonusPercent);
        }

        public static float GetPersonalServeTimeMultiplier(Staff staff)
        {
            return GetPersonalWaiterTimeMultiplier(staff, StaffTalentType.ServeSpeedBonusPercent);
        }

        public static float GetPersonalCheckoutTimeMultiplier(Staff staff)
        {
            return GetPersonalWaiterTimeMultiplier(staff, StaffTalentType.CheckoutSpeedBonusPercent);
        }

        public static float GetPersonalCleanTimeMultiplier(Staff staff)
        {
            return GetPersonalWaiterTimeMultiplier(staff, StaffTalentType.CleanSpeedBonusPercent);
        }

        public static float GetPersonalAllServiceTimeMultiplier(Staff staff)
        {
            if (staff == null || staff.Position != StaffPosition.Waiter)
            {
                return 1f;
            }

            var talent = GetTalent(staff);
            return talent != null && talent.TalentType == StaffTalentType.AllServiceSpeedBonusPercent
                ? PercentToTimeMul(GetParam0(talent))
                : 1f;
        }

        public static float GetChefCookSpeedMultiplier(Staff staff)
        {
            if (staff == null || staff.Position != StaffPosition.Chef)
            {
                return 1f;
            }

            var talent = GetTalent(staff);
            return talent != null && talent.TalentType == StaffTalentType.ChefPrepSpeedBonusPercent
                ? PercentToCookSpeedMul(GetParam0(talent))
                : 1f;
        }

        public static int GetExtraDishChancePercent(Staff staff)
        {
            if (staff == null || staff.Position != StaffPosition.Chef)
            {
                return 0;
            }

            var talent = GetTalent(staff);
            return talent != null && talent.TalentType == StaffTalentType.ExtraDishChancePercent
                ? Mathf.Max(0, GetParam0(talent))
                : 0;
        }

        public static bool TryRollDoubleOrderDishCook(Staff staff, int pendingOrderDishCount)
        {
            if (staff == null
                || staff.Position != StaffPosition.Chef
                || pendingOrderDishCount < 2)
            {
                return false;
            }

            var chance = GetExtraDishChancePercent(staff);
            return chance > 0 && Random.value < chance / 100f;
        }

        public static bool IsShopkeeperGlobalTalentType(StaffTalentType talentType)
        {
            switch (talentType)
            {
                case StaffTalentType.WaiterAllServiceSpeedBonusPercent:
                case StaffTalentType.ChefAllCookingSpeedBonusPercent:
                case StaffTalentType.TeamOccupancyEfficiencyBonus:
                case StaffTalentType.TeamCleanSpeedBonusPercent:
                case StaffTalentType.TeamServeSpeedBonusPercent:
                case StaffTalentType.TeamCheckoutSpeedBonusPercent:
                case StaffTalentType.TeamOrderSpeedBonusPercent:
                case StaffTalentType.RecruitmentCostReductionPercent:
                case StaffTalentType.DailyWageReductionPercent:
                case StaffTalentType.VipAttractionBonusPercent:
                case StaffTalentType.TavernAllWorkSpeedBonusPercent:
                    return true;
                default:
                    return false;
            }
        }

        public static ManagementTalentMultipliers BuildManagementMultipliers(float tableOccupancyPercent = -1f)
        {
            if (tableOccupancyPercent < 0f)
            {
                tableOccupancyPercent = ResolveTableOccupancyPercent();
            }

            var result = ManagementTalentMultipliers.Identity;
            var owned = DataManager.Instance?.GetOwnedStaffList();
            if (owned == null || owned.Count == 0)
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

                var staff = StaffConfigUtility.GetOrNull(save.staffId);
                var talent = GetTalent(staff);
                if (staff == null
                    || staff.Position != StaffPosition.Shopkeeper
                    || talent == null
                    || talent.Position != StaffPosition.Shopkeeper
                    || !IsShopkeeperGlobalTalentType(talent.TalentType))
                {
                    continue;
                }

                ApplyManagementTalent(ref result, talent, tableOccupancyPercent);
            }

            return result;
        }

        public static int ApplyRecruitmentCostMultiplier(int baseCost)
        {
            if (baseCost <= 0)
            {
                return baseCost;
            }

            var mul = BuildManagementMultipliers().RecruitmentCostMul;
            return Mathf.Max(1, Mathf.RoundToInt(baseCost * mul));
        }

        public static int ApplyDailyWageMultiplier(int baseSalary)
        {
            if (baseSalary <= 0)
            {
                return baseSalary;
            }

            var mul = BuildManagementMultipliers().DailyWageMul;
            return Mathf.Max(0, Mathf.RoundToInt(baseSalary * mul));
        }

        public static float ApplyVipSpawnChanceBonus(float baseChance)
        {
            var bonus = BuildManagementMultipliers().VipSpawnChanceBonusPercent;
            if (bonus <= 0)
            {
                return baseChance;
            }

            // 贵客盈门按百分比放大基础概率，避免与基础值相加后后期接近 50%。
            return Mathf.Clamp01(baseChance * (1f + bonus / 100f));
        }

        public static float ResolveTableOccupancyPercent()
        {
            var instance = TavernSceneManager.Instance;
            return instance != null ? instance.GetTableOccupancyPercent() : 0f;
        }

        private static float GetPersonalWaiterTimeMultiplier(Staff staff, StaffTalentType expectedType)
        {
            if (staff == null || staff.Position != StaffPosition.Waiter)
            {
                return 1f;
            }

            var talent = GetTalent(staff);
            if (talent == null)
            {
                return 1f;
            }

            if (talent.TalentType == expectedType)
            {
                return PercentToTimeMul(GetParam0(talent));
            }

            return 1f;
        }

        private static void ApplyManagementTalent(
            ref ManagementTalentMultipliers result,
            StaffTalent talent,
            float tableOccupancyPercent)
        {
            var percent = GetParam0(talent);
            switch (talent.TalentType)
            {
                case StaffTalentType.WaiterAllServiceSpeedBonusPercent:
                    result.WaiterOrderTimeMul *= PercentToTimeMul(percent);
                    result.WaiterServeTimeMul *= PercentToTimeMul(percent);
                    result.WaiterCheckoutTimeMul *= PercentToTimeMul(percent);
                    result.WaiterCleanTimeMul *= PercentToTimeMul(percent);
                    result.WaiterMoveSpeedMul *= PercentToMoveSpeedMul(percent);
                    break;
                case StaffTalentType.TeamOrderSpeedBonusPercent:
                    result.WaiterOrderTimeMul *= PercentToTimeMul(percent);
                    break;
                case StaffTalentType.TeamServeSpeedBonusPercent:
                    result.WaiterServeTimeMul *= PercentToTimeMul(percent);
                    break;
                case StaffTalentType.TeamCheckoutSpeedBonusPercent:
                    result.WaiterCheckoutTimeMul *= PercentToTimeMul(percent);
                    break;
                case StaffTalentType.TeamCleanSpeedBonusPercent:
                    result.WaiterCleanTimeMul *= PercentToTimeMul(percent);
                    break;
                case StaffTalentType.ChefAllCookingSpeedBonusPercent:
                    result.ChefCookSpeedMul *= PercentToCookSpeedMul(percent);
                    break;
                case StaffTalentType.TeamOccupancyEfficiencyBonus:
                    var threshold = GetParam0(talent);
                    var bonus = GetParam1(talent);
                    if (tableOccupancyPercent >= threshold && bonus > 0)
                    {
                        result.WaiterOrderTimeMul *= PercentToTimeMul(bonus);
                        result.WaiterServeTimeMul *= PercentToTimeMul(bonus);
                        result.WaiterCheckoutTimeMul *= PercentToTimeMul(bonus);
                        result.WaiterCleanTimeMul *= PercentToTimeMul(bonus);
                        result.WaiterMoveSpeedMul *= PercentToMoveSpeedMul(bonus);
                        result.ChefCookSpeedMul *= PercentToCookSpeedMul(bonus);
                    }

                    break;
                case StaffTalentType.RecruitmentCostReductionPercent:
                    result.RecruitmentCostMul *= PercentToCostMul(percent);
                    break;
                case StaffTalentType.DailyWageReductionPercent:
                    result.DailyWageMul *= PercentToCostMul(percent);
                    break;
                case StaffTalentType.VipAttractionBonusPercent:
                    result.VipSpawnChanceBonusPercent += percent;
                    break;
                case StaffTalentType.TavernAllWorkSpeedBonusPercent:
                    result.WaiterOrderTimeMul *= PercentToTimeMul(percent);
                    result.WaiterServeTimeMul *= PercentToTimeMul(percent);
                    result.WaiterCheckoutTimeMul *= PercentToTimeMul(percent);
                    result.WaiterCleanTimeMul *= PercentToTimeMul(percent);
                    result.WaiterMoveSpeedMul *= PercentToMoveSpeedMul(percent);
                    result.ChefCookSpeedMul *= PercentToCookSpeedMul(percent);
                    break;
            }
        }

        private static int GetParam0(StaffTalent talent)
        {
            return talent?.Param != null && talent.Param.Count > 0 ? talent.Param[0] : 0;
        }

        private static int GetParam1(StaffTalent talent)
        {
            return talent?.Param != null && talent.Param.Count > 1 ? talent.Param[1] : 0;
        }

        private static float PercentToTimeMul(int percent)
        {
            return Mathf.Max(0.1f, 1f - percent / 100f);
        }

        private static float PercentToMoveSpeedMul(int percent)
        {
            return Mathf.Max(0.1f, 1f + percent / 100f);
        }

        private static float PercentToCookSpeedMul(int percent)
        {
            return Mathf.Max(0.1f, 1f + percent / 100f);
        }

        private static float PercentToCostMul(int reductionPercent)
        {
            return Mathf.Max(0.1f, 1f - reductionPercent / 100f);
        }
    }
}
