using System;
using System.Collections.Generic;

namespace JN.Client.Model
{
    [Serializable]
    /// <summary>
    /// 负责本地玩法存档数据相关的运行时逻辑。
    /// </summary>
    public class LocalGameplaySaveData
    {
        public ushort localPlayerNumericId = 1;
        public byte playerLevel = 1;
        public byte activeShopId;
        public bool openingLoanClaimed;
        public int loanCount;
        public int pendingLoanAmount;
        public bool waitingForLoanApproval;
        public bool shopOpened;
        public bool waitingForSettlement;
        public bool firstShopEntryPending = true;
        public bool tutorialEnabled = true;
        public float shopOpenDuration;
        public float reopenCooldown;
        public int pendingSettlementIncome;
        public int pendingSettlementCosts;
        public int dailyRevenue;
        public int totalDepositedIncome;

        /// <summary>
        /// 累计开业轮次（首次点开业 +1，之后每 3 分钟循环续轮再 +1）。用于高峰配置档位与入口解锁。
        /// </summary>
        public int businessOpenCount;

        /// <summary>
        /// 从他人酒楼拉回、待进入自家酒楼的客人数量（回店后一次卸完）。
        /// 与 pendingPulledCustomerKinds.Count 同步，供旧逻辑读取。
        /// </summary>
        public int pendingPulledCustomerCount;

        /// <summary>
        /// 待卸客队列：客人类型（0 普通 / 1 稀客 / 2 贵客），与拉客时一致。
        /// </summary>
        public List<int> pendingPulledCustomerKinds = new();

        /// <summary>
        /// 旧存档兼容：每项为占用容量（现规则任意客人占 1）；迁移进 kinds 后可空。
        /// </summary>
        public List<int> pendingPulledCustomerCapacities = new();

        /// <summary>
        /// 当前已占用的轿子容量（与待卸客队列容量之和一致）。
        /// </summary>
        public int jiaoziUsedCapacity;

        /// <summary>
        /// 已摆放满 10 张桌后的累计开业次数；每次开业菜价 +20%。
        /// </summary>
        public int postTenTableBusinessOpenCount;

        /// <summary>
        /// 本营业日已结账桌次数（含未收款）。
        /// </summary>
        public int sessionServedCustomers;

        /// <summary>
        /// 本营业日结账被偷等未收款次数。
        /// </summary>
        public int sessionUnpaidCheckouts;

        /// <summary>
        /// 本营业日满意度样本总分（用于求平均）。
        /// </summary>
        public float sessionSatisfactionSum;

        /// <summary>
        /// 本营业日满意度样本数。
        /// </summary>
        public int sessionSatisfactionSamples;

        /// <summary>
        /// 本营业日因各阶段等待过久中途离场的顾客人数。
        /// </summary>
        public int sessionWaitWalkoutCustomers;

        /// <summary>
        /// 本营业日不满意原因统计。
        /// </summary>
        public List<ClosingDissatisfactionEntry> sessionDissatisfactionReasons = new();

        /// <summary>
        /// 最近打烊记录（新在前，最多保留 5 条；界面对比取前 2 条）。
        /// </summary>
        public List<ClosingSessionRecord> recentClosings = new();
        public float peakTimeRemaining;
        public float peakTimeCooldown;
        public bool inPeakTime;
        public byte remainingPeakCustomers;
        public List<byte> purchasedLandSlots = new();
        public List<int> hiredStaffIds = new();
        public List<LocalStaffSaveData> ownedStaff = new();
        public bool[] unlockedFeatures = { false, false, false, false, false };
        public List<LocalShopSaveData> ownedShops = new();
        public List<LocalEquipmentSaveData> ownedEquipment = new();
        public GameplayGuideSaveData gameplayGuide = new();

        /// <summary>
        /// 已研究完成的酒馆科技 Id 列表。
        /// </summary>
        public List<int> researchedTechIds = new();

        /// <summary>
        /// 当前正在研究的科技 Id；0 表示空闲。
        /// </summary>
        public int researchingTechId;

        /// <summary>
        /// 当前研究结束 UTC 时间戳（秒）；到达后科技完成。
        /// </summary>
        public double researchEndUnixTime;

        /// <summary>
        /// 已领取奖励的成就 Id 列表。
        /// </summary>
        public List<int> claimedAchievementIds = new();

        /// <summary>
        /// 已弹出「成就达成」通用提示的 Id（避免重复弹窗）。
        /// </summary>
        public List<int> achievementCompletionToastShownIds = new();

        /// <summary>
        /// 是否已将当前已完成成就写入 toast 记录（老存档迁移一次）。
        /// </summary>
        public bool achievementCompletionToastSeeded;

        /// <summary>
        /// 城镇建筑上展示的成就 Id；0 表示未设置。全局同时只能展示一个。
        /// </summary>
        public int displayedAchievementId;

        /// <summary>
        /// 本营业日是否发生过玩家手动派工。
        /// </summary>
        public bool sessionHadManualDispatch;

        /// <summary>
        /// 本营业日是否已有顾客因等待等原因中途离场（完美的一天成就判定）。
        /// </summary>
        public bool sessionPerfectDayViolated;

        public int sessionPeakQueueLength;
        public int sessionPeakPendingServeDishes;
        public int sessionPeakPendingCheckoutTables;
        public int sessionPeakDirtyTables;

        /// <summary>员工招聘刷新累计次数（用于 staffRecruitRefreshCosts 阶梯计费）。</summary>
        public int staffRecruitRefreshCount;

        /// <summary>上次员工招聘刷新的 unscaled 时间；超过 staffRecruitRefreshTime 后重置次数。</summary>
        public float staffRecruitLastRefreshUnscaledTime;

        /// <summary>
        /// 本账号已触发过的低谷期配置下标（valleyCustomerSecondWaveSeconds 数组索引，终身不重复）。
        /// </summary>
        public List<int> triggeredValleyWaveIndices = new();

        /// <summary>
        /// 拉客冷却结束 UTC Unix 秒；0 表示无冷却。按真实时间计算。
        /// </summary>
        public double pullCustomerCooldownEndUnixTime;
    }

    [Serializable]
    /// <summary>
    /// 负责本地店铺存档数据相关的运行时逻辑。
    /// </summary>
    public class LocalShopSaveData
    {
        public byte mapSlotIndex;
        public byte shopLevel = 1;
        public sbyte shopTypeId = -1;
        public float constructionFinishTime;
        public int totalShopValue;
        public int totalShopSpendings;
        public bool openedForCustomers;
        public float nextCustomerInSeconds;
        public List<LocalRuntimeCustomerSaveData> currentCustomers = new();
        public List<LocalStaffSaveData> ownedStaff = new();
    }

    [Serializable]
    /// <summary>
    /// 负责本地设备存档数据相关的运行时逻辑。
    /// </summary>
    public class LocalEquipmentSaveData
    {
        public byte equipmentId;
        public byte currentLevel;
        public byte physicalSlotIndex;
    }

    [Serializable]
    /// <summary>
    /// 负责本地员工存档数据相关的运行时逻辑。
    /// </summary>
    public class LocalStaffSaveData
    {
        public byte staffId;
        public bool temporary;
        public float remainingHireTime;

        /// <summary>
        /// 是否已用科技合成初始化过技能字段。
        /// </summary>
        public bool skillsInitialized;

        public bool skillOrderUnlocked;
        public bool skillServeUnlocked;
        public bool skillCheckoutUnlocked;

        /// <summary>
        /// 情绪 0~100。
        /// </summary>
        public float emotion = 100f;
    }

    [Serializable]
    /// <summary>
    /// 负责本地运行时顾客存档数据相关的运行时逻辑。
    /// </summary>
    public class LocalRuntimeCustomerSaveData
    {
        public ushort runtimeId;
        public byte customerTypeId;
        public bool peakCustomer;
        public string sourcePlayerName;
    }
}
