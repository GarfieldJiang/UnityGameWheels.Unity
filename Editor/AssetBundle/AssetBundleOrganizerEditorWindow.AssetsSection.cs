using UnityEditor;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizerEditorWindow
    {
        private class AssetsSection : BaseSection
        {
            private Vector2 m_ScrollPosition;
            private string m_ScrollToAssetGuid = null;
            private Rect? m_ScrollToAssetDisplayRect = null;

            public AssetsSection(AssetBundleOrganizerEditorWindow editorWindow)
                : base(editorWindow)
            {
                // Empty.
            }

            public override void Draw()
            {
                EditorGUILayout.LabelField("Assets", EditorStyles.boldLabel, MinWidthOne);

                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, ExpandHeight, ExpandWidth);
                {
                    var assetInfoForest = Organizer.AssetInfoForestRoots;
                    if (assetInfoForest.Count == 0)
                    {
                        EditorGUILayout.HelpBox("No asset to build asset bundles.", MessageType.Info);
                    }
                    else
                    {
                        foreach (var rootDirInfo in assetInfoForest)
                        {
                            DrawAssetTreeRecursively(rootDirInfo, 0);
                        }
                    }
                }
                EditorGUILayout.EndScrollView();

                if (m_ScrollToAssetDisplayRect != null)
                {
                    m_ScrollPosition.y = m_ScrollToAssetDisplayRect.Value.y;
                    m_ScrollToAssetDisplayRect = null;
                }

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(new GUIContent("Refresh", "Normalize root directories and refresh assets.")))
                    {
                        m_EditorWindow.RefreshAssetForest();
                    }

                    EditorGUI.BeginDisabledGroup(m_EditorWindow.m_SelectedAssetInfos.Count == 0);
                    {
                        if (GUILayout.Button(new GUIContent("Deselect All")))
                        {
                            m_EditorWindow.ClearSelectedAssetInfos();
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUILayout.EndHorizontal();
            }

            private void DrawAssetTreeRecursively(AssetBundleOrganizer.AssetInfo assetInfo, int indentCount)
            {
                AssetInfoSatelliteData satelliteData = EnsureSatelliteData(assetInfo);

                var horizontalRect = EditorGUILayout.BeginHorizontal(ExpandWidth);
                {
                    var indentWidth = indentCount * DefaultIndentWidth;
                    GUILayout.Space(indentWidth);
                    var label = new GUIContent();
                    if (assetInfo.IsFile)
                    {
                        label.image = UnityEditorInternal.InternalEditorUtility.GetIconForFile(assetInfo.Path);
                    }
                    else
                    {
                        label.image = EditorGUIUtility.FindTexture(assetInfo.Children.Count > 0 ? "Folder Icon" : "FolderEmpty Icon");

                        var newFoldout = EditorGUI.Foldout(new Rect(horizontalRect.x + indentWidth,
                            horizontalRect.y, FoldoutWidth, horizontalRect.height), satelliteData.Foldout, string.Empty);
                        if (newFoldout != satelliteData.Foldout)
                        {
                            if (Event.current.alt)
                            {
                                FoldoutRecursively(assetInfo, newFoldout);
                            }
                            else
                            {
                                satelliteData.Foldout = newFoldout;
                            }
                        }
                    }

                    GUILayout.Space(FoldoutWidth);
                    var oldSelected = AssetInfoIsSelected(satelliteData);
                    var newSelected = EditorGUILayout.Toggle(oldSelected, GUILayout.Width(ToggleWidth));
                    if (newSelected != oldSelected)
                    {
                        SelectAssetInfo(assetInfo, newSelected);
                    }

                    label.text = (assetInfo.IsRoot ? assetInfo.Path : assetInfo.Name)
                                 + (string.IsNullOrEmpty(assetInfo.AssetBundlePath)
                                     ? string.Empty
                                     : Core.Utility.Text.Format(" [{0}]", assetInfo.AssetBundlePath));
                    EditorGUILayout.LabelField(label, MinWidthOne);
                    
                }
                EditorGUILayout.EndHorizontal();

                if (Event.current.type == EventType.Repaint && m_ScrollToAssetGuid != null && m_ScrollToAssetGuid == assetInfo.Guid)
                {
                    m_ScrollToAssetGuid = null;
                    m_ScrollToAssetDisplayRect = horizontalRect;
                }

                if (!assetInfo.IsFile && satelliteData.Foldout)
                {
                    foreach (var child in assetInfo.Children.Values)
                    {
                        DrawAssetTreeRecursively(child, indentCount + 1);
                    }
                }
            }

            private AssetInfoSatelliteData EnsureSatelliteData(AssetBundleOrganizer.AssetInfo assetInfo)
            {
                AssetInfoSatelliteData satelliteData;
                if (!m_EditorWindow.m_AssetInfoSatelliteDatas.TryGetValue(assetInfo.Guid, out satelliteData)
                    || satelliteData.RawAssetInfo != assetInfo)
                {
                    satelliteData = new AssetInfoSatelliteData {RawAssetInfo = assetInfo};
                    m_EditorWindow.m_AssetInfoSatelliteDatas.Add(assetInfo.Guid, satelliteData);
                }

                return satelliteData;
            }

            private void SelectAssetInfo(AssetBundleOrganizer.AssetInfo assetInfo, bool selected)
            {
                if (selected)
                {
                    EnsureSatelliteData(assetInfo).Selected = true;
                    m_EditorWindow.m_SelectedAssetInfos.Add(assetInfo);
                }
                else
                {
                    EnsureSatelliteData(assetInfo).Selected = false;
                    m_EditorWindow.m_SelectedAssetInfos.Remove(assetInfo);
                }
            }

            private bool AssetInfoIsSelected(AssetInfoSatelliteData satelliteData)
            {
                return satelliteData.Selected;
            }

            private void FoldoutRecursively(AssetBundleOrganizer.AssetInfo rootAssetInfo, bool foldout)
            {
                EnsureSatelliteData(rootAssetInfo).Foldout = foldout;
                foreach (var child in rootAssetInfo.Children.Values)
                {
                    if (!child.IsFile)
                    {
                        FoldoutRecursively(child, foldout);
                    }
                }
            }

            public void SelectAndScrollToAsset(string guid)
            {
                var assetInfo = Organizer.GetAssetInfo(guid);
                if (assetInfo == null)
                {
                    Debug.LogWarningFormat("Asset with GUID '{0}' not found.", guid);
                    return;
                }

                SelectAssetInfo(assetInfo, true);
                while (assetInfo != null)
                {
                    EnsureSatelliteData(assetInfo).Foldout = true;
                    assetInfo = assetInfo.Parent;
                }

                m_ScrollToAssetGuid = guid;
            }
        }
    }
}