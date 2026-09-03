using cfg;
using UnityEngine;

namespace JN.Client.Config
{
    /// <summary>
    /// 负责在运行时加载 TbConfig 表并提供统一读取入口。
    /// </summary>
    public static class TbConfigRuntime
    {
        private const float UrgeONCoefficient = 1.5f;//鞭策系数

        /// <summary>
        /// 顾客刷新时间配置名。
        /// </summary>
        public const string CustomerRefreshTimeKey = "customerRefreshTime";
        public const string CustomerRefreshTimeMinKey = "customerRefreshTimeMin";
        public const string CustomerFlowRampSecondsKey = "customerFlowRampSeconds";
        public const string BusinessHoursKey = "businessHours";

        /// <summary>
        /// 厨师做菜时间配置名。
        /// </summary>
        public const string ChefCookTimeKey = "chefCookTime";

        /// <summary>
        /// 顾客用餐时间配置名。
        /// </summary>
        public const string CustomerEatTimeKey = "customerEatTime";

        /// <summary>
        /// 桌子清理时间配置名。
        /// </summary>
        public const string TableCleanTimeKey = "tableCleanTime";

        /// <summary>
        /// 小二唤醒后服务加速持续时长配置名。
        /// </summary>
        public const string WeakSpeedUpTimeKey = "weakSpeedUpTime";

        /// <summary>小二体力上限（按酒楼等级取数组：1/2/3 级）。</summary>
        public const string WaiterMaxStaminaKey = "waiterMaxStamina";

        /// <summary>小二每清完一桌消耗体力。</summary>
        public const string WaiterTableStaminaCostKey = "waiterTableStaminaCost";

        /// <summary>小二非打盹时恢复 1 点体力所需间隔（秒）。</summary>
        public const string WaiterStaminaRecoverIntervalKey = "waiterStaminaRecoverInterval";

        /// <summary>叫醒偷懒小二后的服务速度倍率。</summary>
        public const string WaiterWakeSpeedMultiplierKey = "waiterWakeSpeedMultiplier";

        /// <summary>叫醒偷懒小二时恢复的体力值。</summary>
        public const string WaiterWakeStaminaRecoverKey = "waiterWakeStaminaRecover";

        /// <summary>厨师体力上限（按酒楼等级取数组：1/2/3 级）。</summary>
        public const string ChefMaxStaminaKey = "chefMaxStamina";

        /// <summary>厨师每做完一单消耗体力。</summary>
        public const string ChefCookStaminaCostKey = "chefCookStaminaCost";

        /// <summary>厨师非打盹时恢复 1 点体力所需间隔（秒）。</summary>
        public const string ChefStaminaRecoverIntervalKey = "chefStaminaRecoverInterval";

        /// <summary>叫醒厨师后的做菜速率倍率。</summary>
        public const string ChefWakeSpeedMultiplierKey = "chefWakeSpeedMultiplier";

        /// <summary>叫醒厨师时恢复的体力值。</summary>
        public const string ChefWakeStaminaRecoverKey = "chefWakeStaminaRecover";

        /// <summary>叫醒厨师后做菜加速持续时长（秒）。</summary>
        public const string ChefWeakSpeedUpTimeKey = "chefWeakSpeedUpTime";

        /// <summary>轿子拉客容量（任意客人均占 1）。</summary>
        public const string JiaoziCapacityKey = "jiaoziCapacity";

        /// <summary>回店卸客后的拉客冷却（秒，按真实时间）。</summary>
        public const string PullCustomerCooldownSecondsKey = "pullCustomerCooldownSeconds";

        /// <summary>菜单切换冷却（秒）。</summary>
        public const string MenuSwitchCooldownSecondsKey = "menuSwitchCooldownSeconds";

        /// <summary>拉回自家后，被拉客人进店间隔（秒）。</summary>
        public const string OtherTavernPullEnterIntervalKey = "otherTavernPullEnterInterval";

        /// <summary>进入他人酒楼时初始排队人数。</summary>
        public const string OtherTavernInitialQueueCountKey = "otherTavernInitialQueueCount";

        /// <summary>
        /// 前台/点单统一时长配置名（秒）。
        /// </summary>
        public const string OrderTimeKey = "orderTime";

        /// <summary>
        /// 兼容旧键名；优先读 <see cref="OrderTimeKey"/>。
        /// </summary>
        [System.Obsolete("请使用 OrderTimeKey")]
        public const string WaiterOrderTimeKey = "waiterOrderTime";

        /// <summary>
        /// 店小二桌边上菜服务时间配置名（不含寻路，固定秒数）。
        /// </summary>
        public const string WaiterServeTimeKey = "waiterServeTime";

        public const string WaiterCheckoutTimeKey = "waiterCheckoutTime";
        public const string WaiterStealTimeKey = "waiterStealTime";
        public const string WaiterStealCooldownKey = "waiterStealCooldown";
        public const string PriceIncreaseCostKey = "priceIncreaseCost";
        public const string PriceIncreaseProfitPercentKey = "priceIncreaseProfitPercent";
        public const string PriceIncreaseDurationKey = "priceIncreaseDuration";
        public const string InspireCostKey = "inspireCost";
        public const string InspireCustomerPercentKey = "inspireCustomerPercent";
        public const string InspireDurationKey = "inspireDuration";
        public const string SpeedUpCostKey = "speedUpCost";

        /// <summary>
        /// 引导开业前需要购买的桌子数量配置名。
        /// </summary>
        public const string GuideOpeningTableCountKey = "guideOpeningTableCount";

        /// <summary>
        /// 引导开业前需要购买的基础设施数量配置名。
        /// </summary>
        public const string GuideRequiredBasicEquipmentCountKey = "guideRequiredBasicEquipmentCount";

        /// <summary>
        /// 引导开业前需要购买的厨房设施数量配置名。
        /// </summary>
        public const string GuideRequiredKitchenEquipmentCountKey = "guideRequiredKitchenEquipmentCount";

        /// <summary>
        /// 引导开业前需要招聘的掌柜数量配置名。
        /// </summary>
        public const string GuideRequiredShopkeeperCountKey = "guideRequiredShopkeeperCount";

        /// <summary>
        /// 引导开业前需要招聘的厨师数量配置名。
        /// </summary>
        public const string GuideRequiredChefCountKey = "guideRequiredChefCount";

        /// <summary>
        /// 引导开业前需要招聘的小二数量配置名。
        /// </summary>
        public const string GuideRequiredWaiterCountKey = "guideRequiredWaiterCount";

        /// <summary>场景雇佣 HUD：掌柜固定价格。</summary>
        public const string ShopkeeperEmployCostKey = "shopkeeperEmployCost";

        /// <summary>场景雇佣 HUD：厨师固定价格。</summary>
        public const string ChefEmployCostKey = "chefEmployCost";

        /// <summary>场景雇佣 HUD：小二固定价格。</summary>
        public const string WaiterEmployCostKey = "waiterEmployCost";

        /// <summary>酒楼墙体扩建费用（当前仅 wall01 一档）。</summary>
        public const string TavernExpandCostsKey = "tavernExpandCosts";

        /// <summary>高峰触发：开业/续轮后经过多少秒触发本轮唯一高峰（按 1~5 轮数组）。</summary>
        public const string PeakCustomerWaveSecondsKey = "peakCustomerWaveSeconds";

        /// <summary>高峰进店批次数（按开业轮次取 5 档数组；总人数 = 批次 × peakCustomerBatchSize）。</summary>
        public const string PeakCustomerBatchCountKey = "peakCustomerBatchCount";

        /// <summary>按酒楼等级的常规刷客间隔（秒，LV1~LV3 → 下标 0~2）。</summary>
        public const string CustomerRefreshTimeByLevelKey = "customerRefreshTimeByLevel";

        /// <summary>单桌结账基础价（每人，按酒楼 LV1~LV3）。</summary>
        public const string TableCheckoutIncomeByLevelKey = "tableCheckoutIncomeByLevel";

        /// <summary>结账基础价随机上下浮动绝对值（最终每人价 = 基础价 ± 该范围）。</summary>
        public const string TableCheckoutIncomeFloatRangeKey = "tableCheckoutIncomeFloatRange";

        /// <summary>高峰分批：每批进客人数。</summary>
        public const string PeakCustomerBatchSizeKey = "peakCustomerBatchSize";

        /// <summary>高峰分批：批次间隔（秒）。</summary>
        public const string PeakCustomerBatchIntervalSecondsKey = "peakCustomerBatchIntervalSeconds";

        /// <summary>低谷：经营经过多少秒触发（数组每一项为一次终身低谷阈值，单位秒）。</summary>
        public const string ValleyCustomerSecondWaveSecondsKey = "valleyCustomerSecondWaveSeconds";

        /// <summary>低谷进店批次数（与低谷阈值同下标；总人数 = 批次 × valleyCustomerBatchSize）。</summary>
        public const string ValleyCustomerBatchCountKey = "valleyCustomerBatchCount";

        /// <summary>低谷分批：每批进客人数。</summary>
        public const string ValleyCustomerBatchSizeKey = "valleyCustomerBatchSize";

        /// <summary>低谷分批：批次间隔（秒）。</summary>
        public const string ValleyCustomerBatchIntervalSecondsKey = "valleyCustomerBatchIntervalSeconds";

        /// <summary>开业轮次配置档位数（数组长度）。</summary>
        public const int BusinessRoundConfigSlotCount = 5;

        /// <summary>
        /// 解锁二级桌升级功能所需的累计结账次数配置名。
        /// </summary>
        public const string TableLv2UpgradeUnlockCheckoutCountKey = "tableLv2UpgradeUnlockCheckoutCount";

        /// <summary>
        /// 掌柜柜台随机收益弹出间隔配置名（秒）。
        /// </summary>
        public const string CounterRandomRewardIntervalKey = "counterRandomRewardInterval";

        /// <summary>
        /// 掌柜柜台随机收益金币下限配置名。
        /// </summary>
        public const string CounterRandomRewardCoinMinKey = "counterRandomRewardCoinMin";

        /// <summary>
        /// 掌柜柜台随机收益金币上限配置名。
        /// </summary>
        public const string CounterRandomRewardCoinMaxKey = "counterRandomRewardCoinMax";

        /// <summary>
        /// 开局贷款（游戏开始时领取）金额配置名。
        /// </summary>
        public const string OpeningLoanAmountKey = "openingLoanAmount";

        /// <summary>
        /// 顾客等待 HUD：各阶段合理等待（秒，此时间内不显示气泡）。
        /// </summary>
        public const string CustomerWaitQueueGraceTimeKey = "customerWaitQueueGraceTime";
        public const string CustomerWaitOrderGraceTimeKey = "customerWaitOrderGraceTime";
        public const string CustomerWaitServeGraceTimeKey = "customerWaitServeGraceTime";
        public const string CustomerWaitCheckoutGraceTimeKey = "customerWaitCheckoutGraceTime";

        /// <summary>
        /// 顾客等待 HUD：各阶段气泡倒计时（秒，进度条走满后离场）。
        /// </summary>
        public const string CustomerWaitQueueBubbleTimeKey = "customerWaitQueueBubbleTime";
        public const string CustomerWaitOrderBubbleTimeKey = "customerWaitOrderBubbleTime";
        public const string CustomerWaitServeBubbleTimeKey = "customerWaitServeBubbleTime";
        public const string CustomerWaitCheckoutBubbleTimeKey = "customerWaitCheckoutBubbleTime";

        public const string WaiterAttractIntervalKey = "waiterAttractInterval";
        public const string WaiterAttractMaxWaitersKey = "waiterAttractMaxWaiters";
        public const string WaiterAttractMinTableFillsKey = "waiterAttractMinTableFills";
        public const string VipSpawnChancePermilleKey = "vipSpawnChancePermille";
        public const string RareSpawnChancePermilleKey = "rareSpawnChancePermille";
        public const string VipAttractSpawnChanceMultiplierPermilleKey = "vipAttractSpawnChanceMultiplierPermille";

        public const string StaffRecruitCandidateCountKey = "staffRecruitCandidateCount";
        public const string StaffRecruitRefreshCostsKey = "staffRecruitRefreshCosts";
        public const string StaffRecruitRefreshTimeKey = "staffRecruitRefreshTime";
        public const string StaffTalentNegativeWeightKey = "staffTalentNegativeWeight";
        public const string StaffTalentNormalWeightKey = "staffTalentNormalWeight";
        public const string StaffTalentExcellentWeightKey = "staffTalentExcellentWeight";

        /// <summary>
        /// 酒楼声望升级阈值：index0=升到 1 星，index1=升到 2 星，index2=升到 3 星。
        /// </summary>
        public const string UpgradeLevelPrestigeKey = "upgradeLevelPresitige";

        /// <summary>
        /// 酒楼星级升级金币：index0=升到 1 星，index1=升到 2 星，index2=升到 3 星（与声望阈值同档）。
        /// </summary>
        public const string UpgradeLevelMoneyKey = "upgradeLevelMoney";

        /// <summary>普通客人完成一桌获得的声望。</summary>
        public const string NormalCustomerTablePrestigeKey = "normalCustomerTablePrestige";

        /// <summary>贵客完成一桌获得的声望。</summary>
        public const string VipCustomerTablePrestigeKey = "vipCustomerTablePrestige";

        /// <summary>贵客结账收入倍率（千分比，1500 = 1.5×基础价）。</summary>
        public const string VipCheckoutIncomeMultiplierPermilleKey = "vipCheckoutIncomeMultiplierPermille";

        /// <summary>二楼贵客单道做菜时长（秒）。</summary>
        public const string SecondFloorVipCookDurationSecondsKey = "secondFloorVipCookDurationSeconds";

        /// <summary>二楼贵客单道用餐时长（秒）。</summary>
        public const string SecondFloorVipDineDurationSecondsKey = "secondFloorVipDineDurationSeconds";

        /// <summary>二楼贵客单道结账基础收入（再乘贵客倍率）。</summary>
        public const string SecondFloorVipDishIncomeKey = "secondFloorVipDishIncome";

        /// <summary>二楼贵客菜单六道菜后的最终结账金额。</summary>
        public const string SecondFloorVipFinalCheckoutIncomeKey = "secondFloorVipFinalCheckoutIncome";

        /// <summary>回自家酒楼时，每桌独立触发「被拉客」提示的概率（千分比）。</summary>
        public const string OwnTavernPulledTipChancePermilleKey = "ownTavernPulledTipChancePermille";

        /// <summary>拜访他人酒楼时，每桌独立触发「被拉客」提示的概率（千分比）。</summary>
        public const string OtherTavernPulledTipChancePermilleKey = "otherTavernPulledTipChancePermille";

        /// <summary>点击自家被拉客提示获得的声望。</summary>
        public const string PulledTipDismissPrestigeKey = "pulledTipDismissPrestige";

        /// <summary>大众菜单：常时进客间隔缩短千分比（250 = 缩短 25%）。</summary>
        public const string PopularMenuCustomerRefreshReducePermilleKey = "popularMenuCustomerRefreshReducePermille";

        /// <summary>贵客菜单：结账基础单价增加千分比（500 = 增加 50%）。</summary>
        public const string VipMenuCheckoutUnitPriceBonusPermilleKey = "vipMenuCheckoutUnitPriceBonusPermille";

        /// <summary>
        /// 读取整型配置值（列表 index=0），不存在时返回兜底值。
        /// </summary>
        public static int GetInt(string configName, int defaultValue)
        {
            return GetIntAt(configName, 0, defaultValue);
        }

        /// <summary>
        /// 读取指定下标的整型配置值。
        /// </summary>
        public static int GetIntAt(string configName, int index, int defaultValue)
        {
            var configTable = GetConfigTable();
            return configTable == null ? defaultValue : configTable.GetIntAt(configName, index, defaultValue);
        }

        /// <summary>
        /// 读取浮点配置值，不存在时返回兜底值。
        /// </summary>
        /// <param name="configName">配置名称。</param>
        /// <param name="defaultValue">兜底数值。</param>
        /// <returns>配置值或兜底值。</returns>
        public static float GetFloat(string configName, float defaultValue)
        {
            return GetInt(configName, Mathf.RoundToInt(defaultValue));
        }

        /// <summary>
        /// 读取顾客刷新起始间隔（秒）。无等级表时回退用本键。
        /// </summary>
        public static float GetCustomerRefreshTime(float defaultValue)
        {
            return Mathf.Max(0.5f, GetFloat(CustomerRefreshTimeKey, defaultValue));
        }

        /// <summary>
        /// 按酒楼星级读取常规刷客间隔（秒）。0/1 星→6、2 星→5、3 星→4。
        /// </summary>
        public static float GetCustomerRefreshTimeForLevel(int tavernLevel, float defaultValue)
        {
            // 星级 0 与 1 共用第一档间隔。
            var index = Mathf.Clamp(Mathf.Max(tavernLevel, 1), 1, 3) - 1;
            var fallback = index switch
            {
                0 => 6,
                1 => 5,
                _ => 4,
            };
            var fromTable = GetIntAt(CustomerRefreshTimeByLevelKey, index, fallback);
            if (fromTable <= 0)
            {
                fromTable = fallback;
            }

            return Mathf.Max(0.5f, fromTable);
        }

        /// <summary>
        /// 按酒楼等级读取单人结账基础价（LV1~LV3 → 下标 0~2）。
        /// 配置异常（如误写成无逗号的超大数）时回退默认档。
        /// </summary>
        public static int GetTableCheckoutIncomeForLevel(int tavernLevel, int defaultValue)
        {
            var index = Mathf.Clamp(Mathf.Max(tavernLevel, 1), 1, 3) - 1;
            var fallback = index switch
            {
                0 => Mathf.Max(1, defaultValue),
                1 => Mathf.Max(1, defaultValue + 40),
                _ => Mathf.Max(1, defaultValue + 80),
            };
            var fromTable = GetIntAt(TableCheckoutIncomeByLevelKey, index, fallback);
            // 单人基础价合理上限，防止 Excel 把 120,160,200 写成 120160200 一类脏数据。
            const int maxSaneUnitPrice = 5000;
            if (fromTable <= 0 || fromTable > maxSaneUnitPrice)
            {
                return fallback;
            }

            return fromTable;
        }

        /// <summary>
        /// 结账基础价随机上下浮动绝对值（0 表示不浮动）。
        /// </summary>
        public static int GetTableCheckoutIncomeFloatRange(int defaultValue)
        {
            return Mathf.Max(0, GetInt(TableCheckoutIncomeFloatRangeKey, defaultValue));
        }

        /// <summary>高峰分批进客人数。</summary>
        public static int GetPeakCustomerBatchSize(int defaultValue)
        {
            return Mathf.Max(1, GetInt(PeakCustomerBatchSizeKey, defaultValue));
        }

        /// <summary>高峰分批间隔（秒）。</summary>
        public static float GetPeakCustomerBatchIntervalSeconds(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(PeakCustomerBatchIntervalSecondsKey, defaultValue));
        }

        /// <summary>低谷分批进客人数。</summary>
        public static int GetValleyCustomerBatchSize(int defaultValue)
        {
            return Mathf.Max(1, GetInt(ValleyCustomerBatchSizeKey, defaultValue));
        }

        /// <summary>低谷分批间隔（秒）。</summary>
        public static float GetValleyCustomerBatchIntervalSeconds(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(ValleyCustomerBatchIntervalSecondsKey, defaultValue));
        }

        /// <summary>低谷进店批次数（取数组第 0 项；按低谷下标请用 GetValleyCustomerBatchCountAt）。</summary>
        public static int GetValleyCustomerBatchCount(int defaultValue)
        {
            return GetValleyCustomerBatchCountAt(0, defaultValue);
        }

        /// <summary>读取指定低谷下标的进店批次数。</summary>
        public static int GetValleyCustomerBatchCountAt(int valleyIndex, int defaultValue)
        {
            return Mathf.Max(1, GetIntAt(ValleyCustomerBatchCountKey, Mathf.Max(0, valleyIndex), defaultValue));
        }

        /// <summary>
        /// 读取顾客刷新最短间隔（秒）。已废弃，固定间隔不再使用本键。
        /// </summary>
        public static float GetCustomerRefreshTimeMin(float defaultValue)
        {
            return Mathf.Max(0.5f, GetFloat(CustomerRefreshTimeMinKey, defaultValue));
        }

        /// <summary>
        /// 读取客流组规模随营业时间放开的渐变秒数（1→2→4），与刷客间隔无关。
        /// </summary>
        public static float GetCustomerFlowRampSeconds(float defaultValue)
        {
            return Mathf.Max(1f, GetFloat(CustomerFlowRampSecondsKey, defaultValue));
        }

        /// <summary>
        /// 旧版：按营业时长插值刷客间隔。已废弃。
        /// </summary>
        [System.Obsolete("刷客间隔已改为固定 customerRefreshTime，不再随营业时间或桌数变化。")]
        public static float GetCustomerRefreshTimeAtElapsed(
            float businessElapsedSeconds,
            float defaultStart = 8f,
            float defaultMin = 3f,
            float defaultRampSeconds = 90f)
        {
            var start = GetCustomerRefreshTime(defaultStart);
            var min = GetCustomerRefreshTimeMin(defaultMin);
            if (min > start)
            {
                min = start;
            }

            var ramp = GetCustomerFlowRampSeconds(defaultRampSeconds);
            var t = Mathf.Clamp01(businessElapsedSeconds / ramp);
            return Mathf.Lerp(start, min, t);
        }

        /// <summary>
        /// 读取单次营业时长（秒）。
        /// </summary>
        public static float GetBusinessHours(float defaultValue)
        {
            return Mathf.Max(1f, GetFloat(BusinessHoursKey, defaultValue));
        }

        /// <summary>
        /// 读取厨师做菜时间。
        /// </summary>
        public static float GetChefCookTime(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(ChefCookTimeKey, defaultValue));
        }

        /// <summary>
        /// 读取顾客用餐时间。
        /// </summary>
        public static float GetCustomerEatTime(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(CustomerEatTimeKey, defaultValue));
        }

        /// <summary>
        /// 读取桌子清理时间。
        /// </summary>
        public static float GetTableCleanTime(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(TableCleanTimeKey, defaultValue));
        }

        /// <summary>
        /// 读取唤醒加速时间。
        /// </summary>
        public static float GetWeakSpeedUpTime(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(WeakSpeedUpTimeKey, defaultValue));
        }

        public static int GetJiaoziCapacity(int defaultValue = 10)
        {
            // 兼容旧键 otherTavernPullMaxCount。
            var value = GetInt(JiaoziCapacityKey, -1);
            if (value < 0)
            {
                value = GetInt("otherTavernPullMaxCount", defaultValue);
            }

            return Mathf.Max(0, value);
        }

        /// <summary>读取拉客冷却时长（秒，真实时间）。</summary>
        public static float GetPullCustomerCooldownSeconds(float defaultValue = 300f)
        {
            return Mathf.Max(0f, GetFloat(PullCustomerCooldownSecondsKey, defaultValue));
        }

        /// <summary>读取菜单切换冷却时长（秒）。</summary>
        public static float GetMenuSwitchCooldownSeconds(float defaultValue = 30f)
        {
            return Mathf.Max(0f, GetFloat(MenuSwitchCooldownSecondsKey, defaultValue));
        }

        public static float GetOtherTavernPullEnterInterval(float defaultValue = 2f)
        {
            return Mathf.Max(0.1f, GetFloat(OtherTavernPullEnterIntervalKey, defaultValue));
        }

        public static int GetOtherTavernInitialQueueCount(int defaultValue = 5)
        {
            return Mathf.Max(0, GetInt(OtherTavernInitialQueueCountKey, defaultValue));
        }

        public static float GetWaiterMaxStamina(float defaultValue)
        {
            return GetWaiterMaxStaminaForLevel(1, defaultValue);
        }

        /// <summary>按酒楼等级读取小二体力上限（数组下标 0/1/2 对应 1/2/3 级）。</summary>
        public static float GetWaiterMaxStaminaForLevel(int tavernLevel, float defaultValue)
        {
            var index = ResolveTavernLevelStaminaIndex(tavernLevel);
            return Mathf.Max(0.1f, GetIntAt(WaiterMaxStaminaKey, index, Mathf.RoundToInt(defaultValue)));
        }

        public static float GetWaiterTableStaminaCost(float defaultValue)
        {
            return Mathf.Max(0f, GetFloat(WaiterTableStaminaCostKey, defaultValue));
        }

        public static float GetWaiterStaminaRecoverInterval(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(WaiterStaminaRecoverIntervalKey, defaultValue));
        }

        public static float GetWaiterWakeSpeedMultiplier(float defaultValue)
        {
            return Mathf.Max(1f, GetFloat(WaiterWakeSpeedMultiplierKey, defaultValue));
        }

        public static float GetWaiterWakeStaminaRecover(float defaultValue)
        {
            return Mathf.Max(0f, GetFloat(WaiterWakeStaminaRecoverKey, defaultValue));
        }

        public static float GetChefMaxStamina(float defaultValue)
        {
            return GetChefMaxStaminaForLevel(1, defaultValue);
        }

        /// <summary>按酒楼等级读取厨师体力上限（数组下标 0/1/2 对应 1/2/3 级）。</summary>
        public static float GetChefMaxStaminaForLevel(int tavernLevel, float defaultValue)
        {
            var index = ResolveTavernLevelStaminaIndex(tavernLevel);
            return Mathf.Max(0.1f, GetIntAt(ChefMaxStaminaKey, index, Mathf.RoundToInt(defaultValue)));
        }

        public static float GetChefCookStaminaCost(float defaultValue)
        {
            return Mathf.Max(0f, GetFloat(ChefCookStaminaCostKey, defaultValue));
        }

        public static float GetChefStaminaRecoverInterval(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(ChefStaminaRecoverIntervalKey, defaultValue));
        }

        public static float GetChefWakeSpeedMultiplier(float defaultValue)
        {
            return Mathf.Max(1f, GetFloat(ChefWakeSpeedMultiplierKey, defaultValue));
        }

        public static float GetChefWakeStaminaRecover(float defaultValue)
        {
            return Mathf.Max(0f, GetFloat(ChefWakeStaminaRecoverKey, defaultValue));
        }

        /// <summary>读取叫醒厨师后做菜加速持续时长（秒）。</summary>
        public static float GetChefWeakSpeedUpTime(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(ChefWeakSpeedUpTimeKey, defaultValue));
        }

        /// <summary>酒楼等级 → 体力档位下标：1→0，2→1，3+→2；0 级按 1 级。</summary>
        public static int ResolveTavernLevelStaminaIndex(int tavernLevel)
        {
            return Mathf.Clamp(Mathf.Max(1, tavernLevel) - 1, 0, 2);
        }

        /// <summary>
        /// 读取点单时长（秒，固定值，前台掌柜与业务统一使用）。
        /// </summary>
        public static float GetOrderTime(float defaultValue)
        {
            var fromNew = GetIntAt(OrderTimeKey, 0, -1);
            if (fromNew > 0)
            {
                return Mathf.Max(0.1f, fromNew);
            }

            // 兼容旧配置 waiterOrderTime
            return Mathf.Max(0.1f, GetIntAt("waiterOrderTime", 0, Mathf.RoundToInt(defaultValue)));
        }

        /// <summary>
        /// 兼容旧调用：等同 <see cref="GetOrderTime"/>。
        /// </summary>
        public static float GetWaiterOrderTime(float defaultValue)
        {
            return GetOrderTime(defaultValue);
        }

        /// <summary>
        /// 兼容旧调用：点单已改为单一固定值，忽略 index。
        /// </summary>
        public static float GetWaiterOrderTimeAt(int index, float defaultValue)
        {
            return GetOrderTime(defaultValue);
        }

        /// <summary>
        /// 读取上菜服务时长（秒，固定值，不再分科技档）。
        /// </summary>
        public static float GetWaiterServeTime(float defaultValue)
        {
            return Mathf.Max(0.1f, GetIntAt(WaiterServeTimeKey, 0, Mathf.RoundToInt(defaultValue)));
        }

        /// <summary>
        /// 兼容旧调用：上菜已改为单一固定值，忽略 index。
        /// </summary>
        public static float GetWaiterServeTimeAt(int index, float defaultValue)
        {
            return GetWaiterServeTime(defaultValue);
        }

        public static float GetWaiterAttractInterval(float defaultValue)
        {
            return Mathf.Max(0.5f, GetFloat(WaiterAttractIntervalKey, defaultValue));
        }

        public static int GetWaiterAttractMaxWaiters(int defaultValue)
        {
            return Mathf.Clamp(GetInt(WaiterAttractMaxWaitersKey, defaultValue), 0, 8);
        }

        public static int GetWaiterAttractMinTableFills(int defaultValue)
        {
            return Mathf.Max(1, GetInt(WaiterAttractMinTableFillsKey, defaultValue));
        }

        /// <summary>
        /// 读取千分比配置并转为 0~1 概率（350 → 0.35）。
        /// </summary>
        public static float GetPermilleAsFraction(string configName, float defaultFraction)
        {
            var defaultPermille = Mathf.RoundToInt(Mathf.Clamp01(defaultFraction) * 1000f);
            var permille = GetInt(configName, defaultPermille);
            return Mathf.Clamp01(permille / 1000f);
        }

        /// <summary>
        /// 按酒楼等级读取贵客刷出基础概率（千分比→0~1）。
        /// 配置三档对应等级 1/2/3（下标 0/1/2）；0 星与 1 星共用第一档。
        /// 定时刷客用全量；拉客再乘 vipAttractSpawnChanceMultiplierPermille。
        /// </summary>
        public static float GetVipSpawnChanceForLevel(int tavernLevel, float defaultValue)
        {
            var index = Mathf.Clamp(Mathf.Max(tavernLevel, 1), 1, 3) - 1;
            var defaultPermille = Mathf.RoundToInt(Mathf.Clamp01(defaultValue) * 1000f);
            var permille = Mathf.Max(0, GetIntAt(VipSpawnChancePermilleKey, index, defaultPermille));
            return Mathf.Clamp01(permille / 1000f);
        }

        /// <summary>
        /// 兼容旧调用：按第一档（等级 1）读取贵客概率。
        /// </summary>
        public static float GetVipSpawnChance(float defaultValue)
        {
            return GetVipSpawnChanceForLevel(1, defaultValue);
        }

        /// <summary>
        /// 按酒楼等级读取稀客刷出基础概率（千分比→0~1）。
        /// 配置三档对应等级 1/2/3；0 星与 1 星共用第一档。模型固定 CustomerM6。
        /// </summary>
        public static float GetRareSpawnChanceForLevel(int tavernLevel, float defaultValue)
        {
            var index = Mathf.Clamp(Mathf.Max(tavernLevel, 1), 1, 3) - 1;
            var defaultPermille = Mathf.RoundToInt(Mathf.Clamp01(defaultValue) * 1000f);
            var permille = Mathf.Max(0, GetIntAt(RareSpawnChancePermilleKey, index, defaultPermille));
            return Mathf.Clamp01(permille / 1000f);
        }

        /// <summary>
        /// 拉客刷客时相对基础贵客概率的倍率（1000 = 1.0）。
        /// </summary>
        public static float GetVipAttractSpawnChanceMultiplier(float defaultValue)
        {
            return GetPermilleAsFraction(VipAttractSpawnChanceMultiplierPermilleKey, defaultValue);
        }

        public static int GetStaffRecruitCandidateCount(int defaultValue)
        {
            return Mathf.Max(1, GetInt(StaffRecruitCandidateCountKey, defaultValue));
        }

        public static int GetStaffRecruitRefreshCostAt(int refreshCount, int defaultCost = 0)
        {
            return Mathf.Max(0, GetIntAt(StaffRecruitRefreshCostsKey, Mathf.Max(0, refreshCount), defaultCost));
        }

        public static float GetStaffRecruitRefreshTime(float defaultValue)
        {
            return Mathf.Max(1f, GetFloat(StaffRecruitRefreshTimeKey, defaultValue));
        }

        public static void GetStaffRecruitTalentWeights(out int negative, out int normal, out int excellent)
        {
            negative = Mathf.Max(0, GetInt(StaffTalentNegativeWeightKey, 30));
            normal = Mathf.Max(0, GetInt(StaffTalentNormalWeightKey, 50));
            excellent = Mathf.Max(0, GetInt(StaffTalentExcellentWeightKey, 20));
        }

        public static float GetWaiterCheckoutTime(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(WaiterCheckoutTimeKey, defaultValue));
        }

        public static float GetWaiterStealTime(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(WaiterStealTimeKey, defaultValue));
        }

        public static float GetWaiterStealCooldown(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(WaiterStealCooldownKey, defaultValue));
        }

        public static int GetPriceIncreaseCost(int defaultValue)
        {
            return Mathf.Max(0, GetInt(PriceIncreaseCostKey, defaultValue));
        }

        public static int GetPriceIncreaseProfitPercent(int defaultValue)
        {
            return Mathf.Max(0, GetInt(PriceIncreaseProfitPercentKey, defaultValue));
        }

        public static float GetPriceIncreaseDuration(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(PriceIncreaseDurationKey, defaultValue));
        }

        public static int GetInspireCost(int defaultValue)
        {
            return Mathf.Max(0, GetInt(InspireCostKey, defaultValue));
        }

        public static int GetSpeedUpCost(int defaultValue)
        {
            return Mathf.Max(0, GetInt(SpeedUpCostKey, defaultValue));
        }

        public static int GetInspireCustomerPercent(int defaultValue)
        {
            return Mathf.Max(0, GetInt(InspireCustomerPercentKey, defaultValue));
        }

        public static float GetInspireDuration(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(InspireDurationKey, defaultValue));
        }

        /// <summary>
        /// 读取酒楼墙体扩建费用（tavernExpandCosts 列表下标）。
        /// </summary>
        public static int GetTavernExpandCost(int index = 0, int defaultValue = 500)
        {
            return Mathf.Max(0, GetIntAt(TavernExpandCostsKey, index, defaultValue));
        }

        /// <summary>
        /// 读取引导开业前需要购买的桌子数量。
        /// </summary>
        public static int GetGuideOpeningTableCount(int defaultValue)
        {
            return Mathf.Clamp(GetInt(GuideOpeningTableCountKey, defaultValue), 0, 12);
        }

        /// <summary>
        /// 读取引导开业前需要购买的基础设施数量。
        /// </summary>
        public static int GetGuideRequiredBasicEquipmentCount(int defaultValue)
        {
            return Mathf.Clamp(GetInt(GuideRequiredBasicEquipmentCountKey, defaultValue), 0, 3);
        }

        /// <summary>
        /// 读取引导开业前需要购买的厨房设施数量。
        /// </summary>
        public static int GetGuideRequiredKitchenEquipmentCount(int defaultValue)
        {
            return Mathf.Clamp(GetInt(GuideRequiredKitchenEquipmentCountKey, defaultValue), 0, 4);
        }

        /// <summary>
        /// 读取引导开业前需要招聘的掌柜数量。
        /// </summary>
        public static int GetGuideRequiredShopkeeperCount(int defaultValue)
        {
            return Mathf.Clamp(GetInt(GuideRequiredShopkeeperCountKey, defaultValue), 0, 1);
        }

        /// <summary>
        /// 读取引导开业前需要招聘的厨师数量。
        /// </summary>
        public static int GetGuideRequiredChefCount(int defaultValue)
        {
            return Mathf.Clamp(GetInt(GuideRequiredChefCountKey, defaultValue), 0, 3);
        }

        /// <summary>
        /// 读取引导开业前需要招聘的小二数量。
        /// </summary>
        public static int GetGuideRequiredWaiterCount(int defaultValue)
        {
            return Mathf.Clamp(GetInt(GuideRequiredWaiterCountKey, defaultValue), 0, 3);
        }

        /// <summary>读取场景雇佣掌柜价格。</summary>
        public static int GetShopkeeperEmployCost(int defaultValue)
        {
            return Mathf.Max(0, GetInt(ShopkeeperEmployCostKey, defaultValue));
        }

        /// <summary>读取场景雇佣厨师价格。</summary>
        public static int GetChefEmployCost(int defaultValue)
        {
            return Mathf.Max(0, GetInt(ChefEmployCostKey, defaultValue));
        }

        /// <summary>读取场景雇佣小二价格。</summary>
        public static int GetWaiterEmployCost(int defaultValue)
        {
            return Mathf.Max(0, GetInt(WaiterEmployCostKey, defaultValue));
        }

        /// <summary>
        /// 开业轮次 → 配置数组下标：第 1~5 轮对应 0~4，第 6 轮起按 1~5 循环回绕。
        /// </summary>
        public static int ResolveBusinessRoundConfigIndex(int businessOpenRound)
        {
            var round = Mathf.Max(1, businessOpenRound);
            return (round - 1) % BusinessRoundConfigSlotCount;
        }

        /// <summary>
        /// 读取本轮高峰触发时间（秒）：严格取表值（可为 0=开轮即触发），按 1~5 轮回绕。
        /// </summary>
        public static float GetPeakCustomerWaveSeconds(int businessOpenRound, float defaultValue)
        {
            var index = ResolveBusinessRoundConfigIndex(businessOpenRound);
            return Mathf.Max(0f, GetIntAt(PeakCustomerWaveSecondsKey, index, Mathf.RoundToInt(defaultValue)));
        }

        /// <summary>兼容旧调用：默认按第 1 轮读取。</summary>
        public static float GetPeakCustomerWaveSeconds(float defaultValue)
        {
            return GetPeakCustomerWaveSeconds(1, defaultValue);
        }

        /// <summary>读取高峰进店批次数；节奏微调后固定取配置数组第 1 项。</summary>
        public static int GetPeakCustomerBatchCount(int businessOpenRound, int defaultValue)
        {
            return Mathf.Max(1, GetIntAt(PeakCustomerBatchCountKey, 0, Mathf.Max(1, defaultValue)));
        }

        /// <summary>
        /// 低谷阈值档位数（valleyCustomerSecondWaveSeconds 数组长度）。
        /// </summary>
        public static int GetValleyCustomerSecondWaveSlotCount()
        {
            var configTable = GetConfigTable();
            var values = configTable?.GetValueList(ValleyCustomerSecondWaveSecondsKey);
            return values == null ? 0 : values.Count;
        }

        /// <summary>
        /// 读取指定下标的低谷触发经营时间（秒）；可为 0 表示开业即触发。
        /// </summary>
        public static float GetValleyCustomerSecondWaveSecondsAt(int valleyIndex, float defaultValue)
        {
            return Mathf.Max(0f, GetIntAt(ValleyCustomerSecondWaveSecondsKey, Mathf.Max(0, valleyIndex), Mathf.RoundToInt(defaultValue)));
        }

        /// <summary>
        /// 兼容旧调用：按开业轮次回绕读阈值（新逻辑请用 GetValleyCustomerSecondWaveSecondsAt）。
        /// </summary>
        public static float GetValleyCustomerSecondWaveSeconds(int businessOpenRound, float defaultValue)
        {
            var index = ResolveBusinessRoundConfigIndex(businessOpenRound);
            return GetValleyCustomerSecondWaveSecondsAt(index, defaultValue);
        }

        /// <summary>
        /// 读取解锁二级桌升级功能所需的累计结账次数。
        /// </summary>
        public static int GetTableLv2UpgradeUnlockCheckoutCount(int defaultValue)
        {
            return Mathf.Max(1, GetInt(TableLv2UpgradeUnlockCheckoutCountKey, defaultValue));
        }

        /// <summary>
        /// 读取掌柜柜台随机收益弹出间隔（秒）。
        /// </summary>
        public static float GetCounterRandomRewardInterval(float defaultValue)
        {
            return Mathf.Max(1f, GetFloat(CounterRandomRewardIntervalKey, defaultValue));
        }

        /// <summary>
        /// 读取掌柜柜台随机收益金币区间。
        /// </summary>
        public static void GetCounterRandomRewardCoinRange(int defaultMin, int defaultMax, out int min, out int max)
        {
            min = Mathf.Max(0, GetInt(CounterRandomRewardCoinMinKey, defaultMin));
            max = Mathf.Max(0, GetInt(CounterRandomRewardCoinMaxKey, defaultMax));
            if (min > max)
            {
                (min, max) = (max, min);
            }
        }

        /// <summary>
        /// 读取开局贷款金额。
        /// </summary>
        public static int GetOpeningLoanAmount(int defaultValue)
        {
            return Mathf.Max(0, GetInt(OpeningLoanAmountKey, defaultValue));
        }

        public static float GetCustomerWaitQueueGraceTime(float defaultValue)
        {
            return Mathf.Max(0f, GetFloat(CustomerWaitQueueGraceTimeKey, defaultValue));
        }

        public static float GetCustomerWaitOrderGraceTime(float defaultValue)
        {
            return Mathf.Max(0f, GetFloat(CustomerWaitOrderGraceTimeKey, defaultValue));
        }

        public static float GetCustomerWaitServeGraceTime(float defaultValue)
        {
            return Mathf.Max(0f, GetFloat(CustomerWaitServeGraceTimeKey, defaultValue));
        }

        public static float GetCustomerWaitCheckoutGraceTime(float defaultValue)
        {
            return Mathf.Max(0f, GetFloat(CustomerWaitCheckoutGraceTimeKey, defaultValue));
        }

        public static float GetCustomerWaitQueueBubbleTime(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(CustomerWaitQueueBubbleTimeKey, defaultValue));
        }

        public static float GetCustomerWaitOrderBubbleTime(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(CustomerWaitOrderBubbleTimeKey, defaultValue));
        }

        public static float GetCustomerWaitServeBubbleTime(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(CustomerWaitServeBubbleTimeKey, defaultValue));
        }

        public static float GetCustomerWaitCheckoutBubbleTime(float defaultValue)
        {
            return Mathf.Max(0.1f, GetFloat(CustomerWaitCheckoutBubbleTimeKey, defaultValue));
        }

        /// <summary>
        /// 读取升到目标星级所需累计声望（配置下标 0~3 对应升到 1~4 星）。
        /// </summary>
        public static int GetUpgradeLevelPrestige(int targetPrestigeLevel, int defaultValue)
        {
            if (targetPrestigeLevel < 1)
            {
                return 0;
            }

            var index = Mathf.Clamp(targetPrestigeLevel, 1, 4) - 1;
            var fallback = targetPrestigeLevel switch
            {
                1 => 700,
                2 => 1700,
                3 => 3100,
                4 => 99999,
                _ => defaultValue
            };
            return Mathf.Max(0, GetIntAt(UpgradeLevelPrestigeKey, index, fallback));
        }

        /// <summary>
        /// 读取升到目标星级所需金币（与 upgradeLevelPresitige 同档下标）。
        /// </summary>
        public static int GetUpgradeLevelMoney(int targetPrestigeLevel, int defaultValue = 0)
        {
            if (targetPrestigeLevel < 1)
            {
                return 0;
            }

            var index = Mathf.Clamp(targetPrestigeLevel, 1, 4) - 1;
            var fallback = targetPrestigeLevel switch
            {
                1 => 0,
                2 => 100,
                3 => 500,
                4 => 0,
                _ => defaultValue
            };
            return Mathf.Max(0, GetIntAt(UpgradeLevelMoneyKey, index, fallback));
        }

        public static int GetNormalCustomerTablePrestige(int defaultValue = 10)
        {
            return Mathf.Max(0, GetInt(NormalCustomerTablePrestigeKey, defaultValue));
        }

        public static int GetVipCustomerTablePrestige(int defaultValue = 50)
        {
            return Mathf.Max(0, GetInt(VipCustomerTablePrestigeKey, defaultValue));
        }

        /// <summary>
        /// 贵客结账倍率：千分比转倍数（1500 → 1.5）。
        /// </summary>
        public static float GetVipCheckoutIncomeMultiplier(float defaultValue = 1.5f)
        {
            var defaultPermille = Mathf.RoundToInt(Mathf.Max(0f, defaultValue) * 1000f);
            var permille = Mathf.Max(0, GetInt(VipCheckoutIncomeMultiplierPermilleKey, defaultPermille));
            return Mathf.Max(0f, permille / 1000f);
        }

        /// <summary>二楼贵客做菜时长（秒）。</summary>
        public static float GetSecondFloorVipCookDurationSeconds(float defaultValue = 3f)
        {
            return Mathf.Max(0.2f, GetFloat(SecondFloorVipCookDurationSecondsKey, defaultValue));
        }

        /// <summary>二楼贵客单道用餐时长（秒）。</summary>
        public static float GetSecondFloorVipDineDurationSeconds(float defaultValue = 4f)
        {
            return Mathf.Max(0.2f, GetFloat(SecondFloorVipDineDurationSecondsKey, defaultValue));
        }

        /// <summary>二楼贵客单道结账基础收入（再乘贵客倍率）。</summary>
        public static int GetSecondFloorVipDishIncome(int defaultValue = 40)
        {
            return Mathf.Max(1, GetInt(SecondFloorVipDishIncomeKey, defaultValue));
        }

        /// <summary>二楼贵客菜单六道菜后的最终结账金额。</summary>
        public static int GetSecondFloorVipFinalCheckoutIncome(int defaultValue = 3000)
        {
            return Mathf.Max(1, GetInt(SecondFloorVipFinalCheckoutIncomeKey, defaultValue));
        }

        /// <summary>自家酒楼被拉客提示概率（0~1）。</summary>
        public static float GetOwnTavernPulledTipChance(float defaultValue = 0.2f)
        {
            return GetPermilleAsFraction(OwnTavernPulledTipChancePermilleKey, defaultValue);
        }

        /// <summary>拜访他人酒楼被拉客提示概率（0~1）。</summary>
        public static float GetOtherTavernPulledTipChance(float defaultValue = 0.25f)
        {
            return GetPermilleAsFraction(OtherTavernPulledTipChancePermilleKey, defaultValue);
        }

        /// <summary>点击自家被拉客提示获得的声望。</summary>
        public static int GetPulledTipDismissPrestige(int defaultValue = 30)
        {
            return Mathf.Max(0, GetInt(PulledTipDismissPrestigeKey, defaultValue));
        }

        /// <summary>大众菜单缩短常时刷客间隔的千分比（0~900）。</summary>
        public static int GetPopularMenuCustomerRefreshReducePermille(int defaultValue = 250)
        {
            return Mathf.Clamp(GetInt(PopularMenuCustomerRefreshReducePermilleKey, defaultValue), 0, 900);
        }

        /// <summary>大众菜单刷客间隔倍率（250 → 0.75）。</summary>
        public static float GetPopularMenuCustomerRefreshMul()
        {
            return Mathf.Max(0.1f, (1000 - GetPopularMenuCustomerRefreshReducePermille()) / 1000f);
        }

        /// <summary>贵客菜单结账基础单价加成千分比。</summary>
        public static int GetVipMenuCheckoutUnitPriceBonusPermille(int defaultValue = 500)
        {
            return Mathf.Max(0, GetInt(VipMenuCheckoutUnitPriceBonusPermilleKey, defaultValue));
        }

        /// <summary>贵客菜单结账基础单价倍率（500 → 1.5）。</summary>
        public static float GetVipMenuCheckoutUnitPriceMul()
        {
            return Mathf.Max(1f, (1000 + GetVipMenuCheckoutUnitPriceBonusPermille()) / 1000f);
        }

        /// <summary>自家可购买/建造的地块 fieldId（对应 Town 场景 Tile_N）。</summary>
        public const string SelfBuildingFieldIdKey = "selfBuildingFieldId";

        /// <summary>
        /// 自家建筑地块 Id；≤0 表示未配置。
        /// </summary>
        public static int GetSelfBuildingFieldId(int defaultValue = 1)
        {
            return Mathf.Clamp(GetInt(SelfBuildingFieldIdKey, defaultValue), 0, 32);
        }

        /// <summary>
        /// 获取已加载的 TbConfig 表。
        /// </summary>
        /// <returns>配置表对象。</returns>
        private static TbConfig GetConfigTable()
        {
            return LubanTablesRuntime.GetTables()?.TbConfig;
        }
    }
}
