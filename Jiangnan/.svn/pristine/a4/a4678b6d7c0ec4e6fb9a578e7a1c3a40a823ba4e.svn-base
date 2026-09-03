using System;
using System.Collections.Generic;
using System.IO;
using JN.Client;
using JN.Client.Manager;
using UnityEditor;
using UnityEngine;

namespace JN.Client.Editor
{
    /// <summary>
    /// 本地存档查看与删除工具。
    /// </summary>
    public sealed class LocalSaveBrowserWindow : EditorWindow
    {
        private Vector2 _listScroll;
        private Vector2 _detailScroll;
        private List<LocalSaveFileInfo> _saves = new();
        private int _selectedIndex = -1;
        private string _rawJson = string.Empty;
        private string _statusMessage = string.Empty;

        [MenuItem("Tools/江南/本地存档管理")]
        public static void Open()
        {
            var window = GetWindow<LocalSaveBrowserWindow>("本地存档管理");
            window.minSize = new Vector2(720, 420);
            window.RefreshList();
            window.Show();
        }

        private void OnEnable()
        {
            RefreshList();
        }

        private void OnFocus()
        {
            RefreshList();
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawSaveList();
                DrawSaveDetail();
            }

            if (!string.IsNullOrWhiteSpace(_statusMessage))
            {
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    RefreshList();
                }

                if (GUILayout.Button("打开存档目录", EditorStyles.toolbarButton, GUILayout.Width(100)))
                {
                    OpenPersistentDataFolder();
                }

                GUILayout.FlexibleSpace();

                var deleteAllContent = new GUIContent("删除全部存档");
                var oldColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0.55f, 0.55f);
                if (GUILayout.Button(deleteAllContent, EditorStyles.toolbarButton, GUILayout.Width(110)))
                {
                    DeleteAllSaves();
                }

                GUI.backgroundColor = oldColor;
            }

            EditorGUILayout.LabelField("存档根目录", LocalSaveStore.PersistentRootPath);
            EditorGUILayout.LabelField(
                "模式",
                LocalSaveMode.Enabled ? "本地存档（已屏蔽服务器）" : "联网模式");
        }

        private void DrawSaveList()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(280)))
            {
                EditorGUILayout.LabelField($"存档列表（{_saves.Count}）", EditorStyles.boldLabel);
                _listScroll = EditorGUILayout.BeginScrollView(_listScroll);

                if (_saves.Count == 0)
                {
                    EditorGUILayout.HelpBox("当前没有本地存档。", MessageType.None);
                }

                for (var i = 0; i < _saves.Count; i++)
                {
                    var save = _saves[i];
                    var selected = i == _selectedIndex;
                    var label = $"{save.DisplayTitle}\n铜钱 {save.CoinNum}  |  {save.FormatLastSavedLocal()}";
                    if (GUILayout.Toggle(selected, label, "Button", GUILayout.Height(44)) && !selected)
                    {
                        SelectSave(i);
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawSaveDetail()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("存档详情", EditorStyles.boldLabel);

                if (_selectedIndex < 0 || _selectedIndex >= _saves.Count)
                {
                    EditorGUILayout.HelpBox("请选择左侧存档查看内容。", MessageType.Info);
                    return;
                }

                var save = _saves[_selectedIndex];
                EditorGUILayout.LabelField("文件", save.FileName);
                EditorGUILayout.LabelField("路径", save.Path);
                EditorGUILayout.LabelField("玩家", string.IsNullOrWhiteSpace(save.PlayerName) ? "--" : save.PlayerName);
                EditorGUILayout.LabelField("玩家 ID", string.IsNullOrWhiteSpace(save.PlayerId) ? "--" : save.PlayerId);
                EditorGUILayout.LabelField("铜钱", save.CoinNum.ToString());
                EditorGUILayout.LabelField("上次场景", string.IsNullOrWhiteSpace(save.LastSceneName) ? "--" : save.LastSceneName);
                EditorGUILayout.LabelField("酒楼开业", save.IsTavernOpen ? "是" : "否");
                EditorGUILayout.LabelField("版本", save.Version.ToString());
                EditorGUILayout.LabelField("保存时间", save.FormatLastSavedLocal());
                EditorGUILayout.LabelField("文件大小", FormatFileSize(save.FileSizeBytes));

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("在资源管理器中显示", GUILayout.Height(24)))
                    {
                        EditorUtility.RevealInFinder(save.Path);
                    }

                    var oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(1f, 0.55f, 0.55f);
                    if (GUILayout.Button("删除该存档", GUILayout.Height(24)))
                    {
                        DeleteSelectedSave();
                    }

                    GUI.backgroundColor = oldColor;
                }

                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("JSON 内容", EditorStyles.boldLabel);
                _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
                EditorGUILayout.TextArea(_rawJson, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        private void RefreshList()
        {
            var previousPath = _selectedIndex >= 0 && _selectedIndex < _saves.Count
                ? _saves[_selectedIndex].Path
                : null;

            _saves = LocalSaveStore.ListSaves();
            _selectedIndex = -1;
            _rawJson = string.Empty;

            if (!string.IsNullOrWhiteSpace(previousPath))
            {
                for (var i = 0; i < _saves.Count; i++)
                {
                    if (string.Equals(_saves[i].Path, previousPath, StringComparison.OrdinalIgnoreCase))
                    {
                        SelectSave(i);
                        break;
                    }
                }
            }

            _statusMessage = $"已刷新，共 {_saves.Count} 个存档。";
            Repaint();
        }

        private void SelectSave(int index)
        {
            _selectedIndex = index;
            _rawJson = string.Empty;

            if (index < 0 || index >= _saves.Count)
            {
                return;
            }

            if (LocalSaveStore.TryReadRawJson(_saves[index].Path, out var json, out var error))
            {
                _rawJson = json;
                _statusMessage = $"已加载：{_saves[index].DisplayTitle}";
            }
            else
            {
                _rawJson = $"读取失败：{error}";
                _statusMessage = _rawJson;
            }
        }

        private void DeleteSelectedSave()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _saves.Count)
            {
                return;
            }

            var save = _saves[_selectedIndex];
            if (!EditorUtility.DisplayDialog(
                    "删除本地存档",
                    $"确认删除存档？\n\n{save.DisplayTitle}\n{save.Path}",
                    "删除",
                    "取消"))
            {
                return;
            }

            if (Application.isPlaying && DataManager.Instance != null)
            {
                if (!DataManager.Instance.DeleteLocalSave(save.Path, out var message))
                {
                    _statusMessage = message;
                    return;
                }

                _statusMessage = message;
            }
            else if (!LocalSaveStore.DeleteSave(save.Path, out var error))
            {
                _statusMessage = error;
                return;
            }
            else
            {
                _statusMessage = $"已删除：{save.DisplayTitle}";
            }

            RefreshList();
        }

        private void DeleteAllSaves()
        {
            if (!EditorUtility.DisplayDialog(
                    "删除全部本地存档",
                    $"将删除活动槽与 saves 目录下全部 JSON 存档。\n目录：{LocalSaveStore.PersistentRootPath}",
                    "全部删除",
                    "取消"))
            {
                return;
            }

            int deleted;
            if (Application.isPlaying && DataManager.Instance != null)
            {
                deleted = DataManager.Instance.DeleteAllLocalSaves();
            }
            else
            {
                deleted = LocalSaveStore.DeleteAllSaves();
            }

            _statusMessage = $"已删除 {deleted} 个本地存档。";
            RefreshList();
        }

        private static void OpenPersistentDataFolder()
        {
            var path = LocalSaveStore.PersistentRootPath;
            Directory.CreateDirectory(path);
            EditorUtility.RevealInFinder(path);
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
            {
                return $"{bytes} B";
            }

            if (bytes < 1024 * 1024)
            {
                return $"{bytes / 1024f:0.0} KB";
            }

            return $"{bytes / (1024f * 1024f):0.00} MB";
        }
    }
}
