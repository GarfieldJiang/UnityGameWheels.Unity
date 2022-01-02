using UnityEditor;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizerEditorWindow
    {
        private class RootDirsSection : BaseSection
        {
            private Vector2 m_ScrollPosition;

            public RootDirsSection(AssetBundleOrganizerEditorWindow editorWindow)
                : base(editorWindow)
            {
                // Empty.
            }

            public override void Draw()
            {
                EditorGUILayout.LabelField("Root Directories", EditorStyles.boldLabel, MinWidthOne);
                var rootDirInfos = Organizer.ConfigCache.RootDirectoryInfos;
                if (rootDirInfos.Count == 0)
                {
                    EditorGUILayout.HelpBox("No root directories.", MessageType.Info);
                }

                int indexToRemove = -1;
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, ExpandHeight, ExpandWidth);
                {
                    for (int i = 0; i < rootDirInfos.Count; i++)
                    {
                        var info = rootDirInfos[i];
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Index " + i);
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Delete", GUILayout.Width(50f)))
                            {
                                if (EditorUtility.DisplayDialog("Confirm to delete", 
                                    "Are you sure to delete the selected root directory?",
                                    "Confirm", "Cancel"))
                                {
                                    indexToRemove = i;
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(DefaultIndentWidth);
                            DrawRootDirInfo(info);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();

                if (indexToRemove >= 0)
                {
                    rootDirInfos.RemoveAt(indexToRemove);
                }

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(new GUIContent("Add")))
                    {
                        rootDirInfos.Add(new AssetBundleOrganizerConfig.RootDirectoryInfo());
                    }

                    if (GUILayout.Button(new GUIContent("Normalize", "Remove redundancies and null directories. and sort the rest.")))
                    {
                        Organizer.NormalizeRootAssetDirectories();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            private static void DrawRootDirInfo(AssetBundleOrganizerConfig.RootDirectoryInfo info)
            {
                EditorGUILayout.BeginVertical();
                {
                    var cachedLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = DefaultLabelWidth;
                    var assetObj = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(info.DirectoryGuid));
                    var newAssetObj = EditorGUILayout.ObjectField("Root Dir", assetObj, typeof(Object), false);
                    if (newAssetObj != assetObj)
                    {
                        var newAssetPath = AssetDatabase.GetAssetPath(newAssetObj);
                        if (AssetDatabase.IsValidFolder(newAssetPath) && !Utility.Asset.IsEditorPath(newAssetPath))
                        {
                            info.DirectoryGuid = AssetDatabase.AssetPathToGUID(newAssetPath);
                        }
                        else if (newAssetObj == null)
                        {
                            info.DirectoryGuid = string.Empty;
                        }
                    }

                    info.Filter = EditorGUILayout.DelayedTextField("Filter", info.Filter);
                    EditorGUIUtility.labelWidth = cachedLabelWidth;
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}
