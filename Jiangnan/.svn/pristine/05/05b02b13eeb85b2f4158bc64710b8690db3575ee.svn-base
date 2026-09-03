using JN.Client.UI;
using QFramework;
using UnityEngine;

namespace JN.Client.Manager
{
    public partial class DataManager
    {
        public const int TechEntryUnlockCheckoutCount = 4;

        /// <summary>
        /// 成就底栏入口是否已解锁显示。
        /// </summary>
        public bool IsAchievementEntryUnlocked()
        {
            EnsureTavernDefaults();
            MigrateFeatureEntryUnlocksIfNeeded();
            return SaveData.tavern.achievementEntryUnlocked;
        }

        /// <summary>
        /// 科技底栏入口是否已解锁显示（店内常驻）。
        /// </summary>
        public bool IsTechTopEntryUnlocked()
        {
            return true;
        }

        /// <summary>
        /// 成就入口满足条件时直接解锁（不再弹 NewFeatureOpen 提示）。
        /// </summary>
        public bool TryUnlockAchievementEntryDirectly()
        {
            EnsureTavernDefaults();
            MigrateFeatureEntryUnlocksIfNeeded();
            if (IsAchievementEntryUnlocked())
            {
                return false;
            }

            if (!IsAchievementEntryRevealPending() && !CanRevealAchievementEntry())
            {
                return false;
            }

            UnlockAchievementEntry();
            return true;
        }

        /// <summary>
        /// 科技入口满足条件时直接解锁（不再弹 NewFeatureOpen 提示）。
        /// </summary>
        public bool TryUnlockTechEntryDirectly()
        {
            EnsureTavernDefaults();
            MigrateFeatureEntryUnlocksIfNeeded();
            if (SaveData.tavern.techEntryUnlocked)
            {
                return false;
            }

            if (!IsTechEntryRevealPending() && !CanRevealTechEntry())
            {
                return false;
            }

            UnlockTechEntry();
            return true;
        }

        /// <summary>
        /// 是否满足成就入口直接解锁的条件。
        /// </summary>
        public bool CanRevealAchievementEntry()
        {
            EnsureTavernDefaults();
            MigrateFeatureEntryUnlocksIfNeeded();
            return !SaveData.tavern.achievementEntryUnlocked
                   && !SaveData.tavern.achievementEntryRevealPending
                   && (HasAnyAchievementCompleted()
                       || SaveData.tavern.totalServedCustomers >= TechEntryUnlockCheckoutCount);
        }

        /// <summary>
        /// 是否满足科技入口直接解锁的条件。
        /// </summary>
        public bool CanRevealTechEntry()
        {
            EnsureTavernDefaults();
            MigrateFeatureEntryUnlocksIfNeeded();
            return !SaveData.tavern.techEntryUnlocked
                   && !SaveData.tavern.techEntryRevealPending
                   && SaveData.tavern.totalServedCustomers >= TechEntryUnlockCheckoutCount;
        }

        /// <summary>
        /// 标记成就入口解锁弹窗即将展示（防止 OnPanelShow 等重复入队）。
        /// </summary>
        public bool TryBeginAchievementEntryReveal()
        {
            if (!CanRevealAchievementEntry())
            {
                return false;
            }

            SaveData.tavern.achievementEntryRevealPending = true;
            SaveGame();
            return true;
        }

        /// <summary>
        /// 标记科技入口解锁弹窗即将展示。
        /// </summary>
        public bool TryBeginTechEntryReveal()
        {
            if (!CanRevealTechEntry())
            {
                return false;
            }

            SaveData.tavern.techEntryRevealPending = true;
            SaveGame();
            return true;
        }

        /// <summary>
        /// 成就入口解锁弹窗是否已预约（含队列中/展示中）。
        /// </summary>
        public bool IsAchievementEntryRevealPending()
        {
            EnsureTavernDefaults();
            return SaveData.tavern.achievementEntryRevealPending;
        }

        /// <summary>
        /// 科技入口解锁弹窗是否已预约。
        /// </summary>
        public bool IsTechEntryRevealPending()
        {
            EnsureTavernDefaults();
            return SaveData.tavern.techEntryRevealPending;
        }

        /// <summary>
        /// 标记成就入口已解锁。
        /// </summary>
        public void UnlockAchievementEntry()
        {
            EnsureTavernDefaults();
            SaveData.tavern.achievementEntryRevealPending = false;
            if (SaveData.tavern.achievementEntryUnlocked)
            {
                SaveGame();
                return;
            }

            SaveData.tavern.achievementEntryUnlocked = true;
            SaveData.tavern.achievementEntryAttentionPending = true;
            SaveGame();
            Signals.Get<AchievementProgressSignal>().Dispatch();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
        }

        /// <summary>
        /// 标记科技入口已解锁。
        /// </summary>
        public void UnlockTechEntry()
        {
            EnsureTavernDefaults();
            SaveData.tavern.techEntryRevealPending = false;
            if (SaveData.tavern.techEntryUnlocked)
            {
                SaveGame();
                return;
            }

            SaveData.tavern.techEntryUnlocked = true;
            SaveGame();
            Signals.Get<TavernRuntimeChangedSignal>().Dispatch();
        }

        /// <summary>成就入口解锁后是否应显示引导红点（直至玩家打开成就面板）。</summary>
        public bool ShouldShowAchievementEntryAttention()
        {
            EnsureTavernDefaults();
            return SaveData.tavern.achievementEntryAttentionPending;
        }

        /// <summary>玩家打开成就面板后清除入口引导红点。</summary>
        public void ClearAchievementEntryAttention()
        {
            EnsureTavernDefaults();
            if (!SaveData.tavern.achievementEntryAttentionPending)
            {
                return;
            }

            SaveData.tavern.achievementEntryAttentionPending = false;
            SaveGame();
            Signals.Get<AchievementProgressSignal>().Dispatch();
        }

        private void MigrateFeatureEntryUnlocksIfNeeded()
        {
            if (SaveData.tavern.featureEntryUnlockMigrated)
            {
                return;
            }

            EnsureGameplayDefaults();
            if (HasAnyAchievementCompleted())
            {
                SaveData.tavern.achievementEntryUnlocked = true;
            }

            if (GetBusinessOpenCount() >= 1
                || SaveData.tavern.totalServedCustomers >= TechEntryUnlockCheckoutCount)
            {
                SaveData.tavern.techEntryUnlocked = true;
            }

            SaveData.tavern.featureEntryUnlockMigrated = true;
            SaveGame();
        }
    }
}
