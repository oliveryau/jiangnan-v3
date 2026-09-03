using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace JN.Client.Editor
{
    /// <summary>
    /// Hierarchy / Project 右键：复制层级路径、资源路径。
    /// </summary>
    public static class CopyPathContextMenus
    {
        private const int HierarchyMenuPriority = 49;
        private const int AssetMenuPriority = 19;

        [MenuItem("GameObject/复制层级路径", false, HierarchyMenuPriority)]
        private static void CopyHierarchyPath()
        {
            var selected = Selection.transforms;
            if (selected == null || selected.Length == 0)
            {
                return;
            }

            var builder = new StringBuilder();
            for (var i = 0; i < selected.Length; i++)
            {
                if (i > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(BuildHierarchyPath(selected[i]));
            }

            CopyToClipboard(builder.ToString(), $"已复制层级路径（{selected.Length}）");
        }

        [MenuItem("GameObject/复制层级路径", true)]
        private static bool ValidateCopyHierarchyPath()
        {
            return Selection.transforms != null && Selection.transforms.Length > 0;
        }

        [MenuItem("Assets/复制资源路径", false, AssetMenuPriority)]
        private static void CopyAssetPath()
        {
            var paths = CollectSelectedAssetPaths();
            if (paths.Count == 0)
            {
                return;
            }

            var builder = new StringBuilder();
            for (var i = 0; i < paths.Count; i++)
            {
                if (i > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(paths[i]);
            }

            CopyToClipboard(builder.ToString(), $"已复制资源路径（{paths.Count}）");
        }

        [MenuItem("Assets/复制资源路径", true)]
        private static bool ValidateCopyAssetPath()
        {
            return CollectSelectedAssetPaths().Count > 0;
        }

        private static string BuildHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            var segments = new List<string>(8);
            var current = transform;
            while (current != null)
            {
                // Prefab 编辑模式下根节点常带此名，复制路径时省略。
                if (!string.Equals(current.name, "Canvas (Environment)", System.StringComparison.Ordinal))
                {
                    segments.Add(current.name);
                }

                current = current.parent;
            }

            segments.Reverse();
            return string.Join("/", segments);
        }

        private static List<string> CollectSelectedAssetPaths()
        {
            var results = new List<string>();
            var guids = Selection.assetGUIDs;
            if (guids == null || guids.Length == 0)
            {
                return results;
            }

            var seen = new HashSet<string>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(path) || !seen.Add(path))
                {
                    continue;
                }

                results.Add(path);
            }

            results.Sort(System.StringComparer.OrdinalIgnoreCase);
            return results;
        }

        private static void CopyToClipboard(string text, string logMessage)
        {
            EditorGUIUtility.systemCopyBuffer = text;
            Debug.Log($"[CopyPath] {logMessage}\n{text}");
        }
    }
}
