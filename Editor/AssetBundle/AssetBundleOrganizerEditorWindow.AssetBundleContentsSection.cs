using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizerEditorWindow
    {
        private class AssetBundleContentsSection : BaseSection
        {
            private enum SortAssetsBy
            {
                Name,
                Path,
            }

            private Vector2 m_ScrollPosition = Vector2.zero;
            private SortAssetsBy m_SortAssetsBy = SortAssetsBy.Name;
            private AssetBundleOrganizer.AssetBundleInfo m_LastSelectedAssetBundleInfo = null;
            private List<AssetInfoInBundleSatelliteData> m_AssetsInBundle = new List<AssetInfoInBundleSatelliteData>();
            private int m_SelectedAssetCount = 0;
            private bool m_NeedRefreshData = true;

            public AssetBundleContentsSection(AssetBundleOrganizerEditorWindow editorWindow) : base(editorWindow)
            {
                // Empty.
            }

            public void Refresh()
            {
                m_NeedRefreshData = true;
            }

            public override void Draw()
            {
                EditorGUILayout.LabelField("Asset Bundle Contents", EditorStyles.boldLabel, MinWidthOne);
                EnsureData();

                if (m_EditorWindow.m_SelectedAssetBundleInfo != null)
                {
                    EditorGUILayout.LabelField("Asset Bundle: " + m_EditorWindow.m_SelectedAssetBundleInfo.Path, MinWidthOne);
                    DrawSelectAllSection();
                }
                else
                {
                    EditorGUILayout.HelpBox("No asset bundle selected.", MessageType.Info);
                }

                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, ExpandHeight, ExpandWidth);
                {
                    DrawAssetInfoList();
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginHorizontal();
                {
                    DrawRemoveButton();
                    DrawSwitchSortByButton();
                }
                EditorGUILayout.EndHorizontal();
                m_LastSelectedAssetBundleInfo = m_EditorWindow.m_SelectedAssetBundleInfo;
            }

            private void DrawRemoveButton()
            {
                EditorGUI.BeginDisabledGroup(m_SelectedAssetCount <= 0);
                {
                    if (GUILayout.Button(new GUIContent("Remove", "Remove selected assets from the current asset bundle.")))
                    {
                        foreach (var ai in m_AssetsInBundle.Where(a => a.IsSelected))
                        {
                            Organizer.UnassignAssetFromBundle(ai.RawData, m_EditorWindow.m_SelectedAssetBundleInfo.Path);
                        }

                        Refresh();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }

            private void DrawSwitchSortByButton()
            {
                EditorGUI.BeginDisabledGroup(m_EditorWindow.m_SelectedAssetBundleInfo == null);
                {
                    if (GUILayout.Button(m_SortAssetsBy == SortAssetsBy.Name ? "Sort by Path" : "Sort by Name"))
                    {
                        m_SortAssetsBy = m_SortAssetsBy == SortAssetsBy.Name ? SortAssetsBy.Path : SortAssetsBy.Name;
                        SortAssetsInBundle();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }

            private void DrawAssetInfoList()
            {
                if (m_EditorWindow.m_SelectedAssetBundleInfo != null && m_AssetsInBundle.Count <= 0)
                {
                    EditorGUILayout.HelpBox("No asset in this asset bundle.", MessageType.Info);
                    return;
                }

                foreach (var assetInBundle in m_AssetsInBundle)
                {
                    var label = new GUIContent();
                    var assetInfo = Organizer.GetAssetInfo(assetInBundle.RawData.Guid);
                    if (assetInBundle.RawData.IsNullOrMissing || assetInfo == null)
                    {
                        label.image = EditorGUIUtility.FindTexture("console.warnicon.sml");
                    }
                    else if (assetInBundle.RawData.IsFile)
                    {
                        label.image = UnityEditorInternal.InternalEditorUtility.GetIconForFile(assetInBundle.RawData.Path);
                    }
                    else
                    {
                        label.image = EditorGUIUtility.FindTexture("Folder Icon");
                    }

                    if (assetInBundle.RawData.IsNullOrMissing)
                    {
                        label.text = assetInBundle.RawData.Guid;
                    }
                    else
                    {
                        label.text = m_SortAssetsBy == SortAssetsBy.Name ? assetInBundle.RawData.Name : assetInBundle.RawData.Path;
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        var newIsSelected = EditorGUILayout.Toggle(assetInBundle.IsSelected, GUILayout.Width(ToggleWidth));
                        if (newIsSelected && !assetInBundle.IsSelected)
                        {
                            assetInBundle.IsSelected = true;
                            m_SelectedAssetCount++;
                        }
                        else if (assetInBundle.IsSelected && !newIsSelected)
                        {
                            assetInBundle.IsSelected = false;
                            m_SelectedAssetCount--;
                        }

                        EditorGUILayout.LabelField(label, MinWidthOne);
                        if (GUI.Button(GUILayoutUtility.GetLastRect(), string.Empty, EditorStyles.label))
                        {
                            m_EditorWindow.ClearSelectedAssetInfos();
                            m_EditorWindow.m_AssetsSection.SelectAndScrollToAsset(assetInBundle.RawData.Guid);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            private void DrawSelectAllSection()
            {
                var allSelected = m_SelectedAssetCount > 0 && m_SelectedAssetCount >= m_AssetsInBundle.Count;
                var newAllSelected = EditorGUILayout.ToggleLeft("Select All", allSelected, EditorStyles.boldLabel, MinWidthOne);
                if (newAllSelected && !allSelected)
                {
                    SelectAll();
                }
                else if (!newAllSelected && allSelected)
                {
                    DeselectAll();
                }
            }

            private void DeselectAll()
            {
                m_SelectedAssetCount = 0;
                foreach (var assetInfo in m_AssetsInBundle)
                {
                    assetInfo.IsSelected = false;
                }
            }

            private void SelectAll()
            {
                m_SelectedAssetCount = m_AssetsInBundle.Count;
                foreach (var assetInfo in m_AssetsInBundle)
                {
                    assetInfo.IsSelected = true;
                }
            }

            private void EnsureData()
            {
                if (!m_NeedRefreshData && m_EditorWindow.m_SelectedAssetBundleInfo == m_LastSelectedAssetBundleInfo)
                {
                    return;
                }

                m_NeedRefreshData = false;
                m_AssetsInBundle.Clear();
                m_SelectedAssetCount = 0;
                if (m_EditorWindow.m_SelectedAssetBundleInfo == null)
                {
                    return;
                }

                m_AssetsInBundle.AddRange(Organizer.GetAssetInfosFromBundle(m_EditorWindow.m_SelectedAssetBundleInfo).ToList()
                    .ConvertAll(ai => new AssetInfoInBundleSatelliteData {IsSelected = false, RawData = ai}));
                SortAssetsInBundle();
            }

            private void SortAssetsInBundle()
            {
                if (m_SortAssetsBy == SortAssetsBy.Name)
                {
                    m_AssetsInBundle.Sort(CompareAssetInBundleByName);
                }
                else
                {
                    m_AssetsInBundle.Sort(CompareAssetInBundleByPath);
                }
            }

            private int CompareAssetInBundleByName(AssetInfoInBundleSatelliteData a, AssetInfoInBundleSatelliteData b)
            {
                if (!string.IsNullOrEmpty(a.RawData.Name) && !string.IsNullOrEmpty(b.RawData.Name))
                {
                    return a.RawData.Name.CompareTo(b.RawData.Name);
                }

                if (string.IsNullOrEmpty(a.RawData.Name) && string.IsNullOrEmpty(b.RawData.Name))
                {
                    return CompareAssetInBundleByGuid(a, b);
                }

                if (string.IsNullOrEmpty(a.RawData.Name))
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }

            private int CompareAssetInBundleByPath(AssetInfoInBundleSatelliteData a, AssetInfoInBundleSatelliteData b)
            {
                if (!string.IsNullOrEmpty(a.RawData.Path) && !string.IsNullOrEmpty(b.RawData.Path))
                {
                    return a.RawData.Path.CompareTo(b.RawData.Path);
                }

                if (string.IsNullOrEmpty(a.RawData.Path) && string.IsNullOrEmpty(b.RawData.Path))
                {
                    return CompareAssetInBundleByGuid(a, b);
                }

                if (string.IsNullOrEmpty(a.RawData.Path))
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }

            private int CompareAssetInBundleByGuid(AssetInfoInBundleSatelliteData a, AssetInfoInBundleSatelliteData b)
            {
                return a.RawData.Guid.CompareTo(b.RawData.Guid);
            }

            private class AssetInfoInBundleSatelliteData
            {
                public AssetBundleOrganizer.AssetInfoInBundle RawData;
                public bool IsSelected = false;
            }
        }
    }
}