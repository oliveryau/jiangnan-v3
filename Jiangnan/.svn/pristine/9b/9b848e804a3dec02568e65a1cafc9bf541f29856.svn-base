using System;
using System.Collections.Generic;
using System.IO;
using JN.Client.Model;
using Newtonsoft.Json;
using UnityEngine;

namespace JN.Client.Manager
{
    /// <summary>
    /// 本地存档文件的读写与枚举，供运行时与编辑器工具共用。
    /// </summary>
    public static class LocalSaveStore
    {
        public const string ActiveSaveFileName = "gamesave.json";
        public const string NamedSaveDirectoryName = "saves";

        public static string ActiveSavePath => Path.Combine(Application.persistentDataPath, ActiveSaveFileName);

        public static string SaveDirectoryPath => Path.Combine(Application.persistentDataPath, NamedSaveDirectoryName);

        public static string PersistentRootPath => Application.persistentDataPath;

        /// <summary>
        /// 按玩家名生成命名存档路径。
        /// </summary>
        public static string GetNamedSavePath(string playerName)
        {
            var safeName = Uri.EscapeDataString(NormalizePlayerName(playerName) ?? "player");
            return Path.Combine(SaveDirectoryPath, $"{safeName}.json");
        }

        /// <summary>
        /// 列出当前机器上的全部本地存档（含活动存档与命名存档）。
        /// </summary>
        public static List<LocalSaveFileInfo> ListSaves()
        {
            var results = new List<LocalSaveFileInfo>();
            TryAddSaveInfo(results, ActiveSavePath, isActiveSlot: true);

            if (!Directory.Exists(SaveDirectoryPath))
            {
                return results;
            }

            foreach (var path in Directory.GetFiles(SaveDirectoryPath, "*.json", SearchOption.TopDirectoryOnly))
            {
                TryAddSaveInfo(results, path, isActiveSlot: false);
            }

            results.Sort((left, right) => right.LastWriteUtcTicks.CompareTo(left.LastWriteUtcTicks));
            return results;
        }

        /// <summary>
        /// 尝试读取存档数据。
        /// </summary>
        public static bool TryReadSave(string path, out GameSaveData saveData, out string error)
        {
            saveData = null;
            error = null;

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                error = "存档文件不存在";
                return false;
            }

            try
            {
                saveData = JsonConvert.DeserializeObject<GameSaveData>(File.ReadAllText(path));
                if (saveData == null)
                {
                    error = "存档内容为空或无法解析";
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        /// <summary>
        /// 读取存档原始 JSON 文本。
        /// </summary>
        public static bool TryReadRawJson(string path, out string json, out string error)
        {
            json = null;
            error = null;

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                error = "存档文件不存在";
                return false;
            }

            try
            {
                json = File.ReadAllText(path);
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        /// <summary>
        /// 删除指定存档文件。
        /// </summary>
        public static bool DeleteSave(string path, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                error = "存档文件不存在";
                return false;
            }

            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        /// <summary>
        /// 删除全部本地存档（活动槽 + saves 目录）。
        /// </summary>
        public static int DeleteAllSaves()
        {
            var deleted = 0;

            if (File.Exists(ActiveSavePath))
            {
                try
                {
                    File.Delete(ActiveSavePath);
                    deleted++;
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[LocalSaveStore] 删除活动存档失败：{exception.Message}");
                }
            }

            if (!Directory.Exists(SaveDirectoryPath))
            {
                return deleted;
            }

            foreach (var path in Directory.GetFiles(SaveDirectoryPath, "*.json", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    File.Delete(path);
                    deleted++;
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[LocalSaveStore] 删除命名存档失败：{path}\n{exception.Message}");
                }
            }

            return deleted;
        }

        private static void TryAddSaveInfo(List<LocalSaveFileInfo> results, string path, bool isActiveSlot)
        {
            if (!File.Exists(path))
            {
                return;
            }

            var info = new LocalSaveFileInfo
            {
                Path = path,
                FileName = Path.GetFileName(path),
                IsActiveSlot = isActiveSlot,
                LastWriteUtcTicks = File.GetLastWriteTimeUtc(path).Ticks,
                FileSizeBytes = new FileInfo(path).Length
            };

            if (TryReadSave(path, out var saveData, out _))
            {
                info.PlayerName = saveData.player?.playerName ?? string.Empty;
                info.PlayerId = saveData.player?.playerId ?? string.Empty;
                info.CoinNum = saveData.player?.coinNum ?? 0;
                info.LastSceneName = saveData.lastSceneName ?? string.Empty;
                info.Version = saveData.version;
                info.LastSavedUtcTicks = saveData.lastSavedUtcTicks;
                info.IsTavernOpen = saveData.tavern != null && saveData.tavern.isOpen;
                info.HasCreatedPlayer = !string.IsNullOrWhiteSpace(info.PlayerName);
            }
            else
            {
                info.PlayerName = "(解析失败)";
            }

            results.Add(info);
        }

        private static string NormalizePlayerName(string playerName)
        {
            return string.IsNullOrWhiteSpace(playerName) ? null : playerName.Trim();
        }
    }

    /// <summary>
    /// 本地存档文件摘要，供列表展示使用。
    /// </summary>
    public sealed class LocalSaveFileInfo
    {
        public string Path;
        public string FileName;
        public bool IsActiveSlot;
        public string PlayerName;
        public string PlayerId;
        public int CoinNum;
        public string LastSceneName;
        public int Version;
        public long LastSavedUtcTicks;
        public long LastWriteUtcTicks;
        public long FileSizeBytes;
        public bool IsTavernOpen;
        public bool HasCreatedPlayer;

        public string DisplayTitle
        {
            get
            {
                var name = string.IsNullOrWhiteSpace(PlayerName) ? FileName : PlayerName;
                return IsActiveSlot ? $"{name} [活动槽]" : name;
            }
        }

        public string FormatLastSavedLocal()
        {
            if (LastSavedUtcTicks <= 0)
            {
                return File.Exists(Path)
                    ? File.GetLastWriteTime(Path).ToString("yyyy-MM-dd HH:mm:ss")
                    : "--";
            }

            return new DateTime(LastSavedUtcTicks, DateTimeKind.Utc).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
