using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizerEditorWindow
    {
        private class AssetBundlesSection : BaseSection
        {
            private Vector2 m_ScrollPosition;
            private string m_InputAssetBundlePath = string.Empty;
            private int m_InputAssetBundleGroup = 0;
            private bool m_InputDontContainDontPack = false;

            private enum Status
            {
                Normal,
                Create,
                Edit,
            }

            private Status m_Status = Status.Normal;

            public AssetBundlesSection(AssetBundleOrganizerEditorWindow editorWindow)
                : base(editorWindow)
            {
                // Empty.
            }

            public override void Draw()
            {
                EditorGUILayout.LabelField("Asset Bundles", EditorStyles.boldLabel, MinWidthOne);
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, ExpandHeight);
                {
                    if (Organizer.AssetBundleInfoTreeRoot.Children.Count == 0)
                    {
                        EditorGUILayout.HelpBox("No asset bundles.", MessageType.Info);
                    }
                    else
                    {
                        DrawAssetBundleTreeRecursively(Organizer.AssetBundleInfoTreeRoot, 0);
                    }
                }
                EditorGUILayout.EndScrollView();

                switch (m_Status)
                {
                    case Status.Create:
                        DrawCreatingAssetBundleBottomView();
                        break;
                    case Status.Edit:
                        DrawEditingAssetBundleBottomView();
                        break;
                    case Status.Normal:
                    default:
                        DrawNormalBottomView();
                        break;
                }
            }

            private void DrawNormalBottomView()
            {
                EditorGUILayout.BeginHorizontal();
                {
                    DrawNewOrEditButton();
                    DrawAssignToButton();
                    DrawDeleteButton();
                }
                EditorGUILayout.EndHorizontal();
            }

            private void DrawNewOrEditButton()
            {
                if (m_EditorWindow.m_SelectedAssetBundleInfo == null)
                {
                    if (GUILayout.Button(new GUIContent("New", "Create a new asset bundle."), MinWidthOne))
                    {
                        m_Status = Status.Create;
                    }
                }
                else
                {
                    if (GUILayout.Button(new GUIContent("Edit", "Edit the selected asset bundle."), MinWidthOne))
                    {
                        m_InputAssetBundlePath = m_EditorWindow.m_SelectedAssetBundleInfo.Path;
                        m_InputAssetBundleGroup = m_EditorWindow.m_SelectedAssetBundleInfo.GroupId;
                        m_InputDontContainDontPack = m_EditorWindow.m_SelectedAssetBundleInfo.DontPack;
                        m_Status = Status.Edit;
                    }

                    if (GUILayout.Button(new GUIContent("Deselect"), MinWidthOne))
                    {
                        m_EditorWindow.m_SelectedAssetBundleInfo = null;
                    }
                }
            }

            private void DrawAssignToButton()
            {
                EditorGUI.BeginDisabledGroup(m_EditorWindow.m_SelectedAssetBundleInfo == null
                                             || m_EditorWindow.m_SelectedAssetInfos.Count <= 0);
                {
                    if (GUILayout.Button(new GUIContent("Assign To", "Assign selected assets (folders) to some existing asset bundle."),
                        MinWidthOne))
                    {
                        Organizer.AssignAssetsToBundle(new List<AssetBundleOrganizer.AssetInfo>(m_EditorWindow.m_SelectedAssetInfos),
                            m_EditorWindow.m_SelectedAssetBundleInfo.Path);
                        m_EditorWindow.ClearSelectedAssetInfos();
                        m_EditorWindow.m_AssetBundleContentsSection.Refresh();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }

            private void DrawDeleteButton()
            {
                EditorGUI.BeginDisabledGroup(m_EditorWindow.m_SelectedAssetBundleInfo == null);
                {
                    if (GUILayout.Button(new GUIContent("Delete", "Delete an asset bundle."), MinWidthOne))
                    {
                        if (EditorUtility.DisplayDialog("Confirm to delete", "Are you sure to delete the selected asset bundle",
                            "Confirm", "Cancel"))
                        {
                            var abPath = m_EditorWindow.m_SelectedAssetBundleInfo.Path;
                            m_EditorWindow.m_SelectedAssetBundleInfo = null;
                            Organizer.DeleteAssetBundle(abPath);
                            m_EditorWindow.m_AssetBundleInfoSatelliteDatas.Remove(abPath);
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
            }

            private void DrawCreatingAssetBundleBottomView()
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("New", EditorStyles.boldLabel);
                    var cachedLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = DefaultLabelWidth;
                    var newAssetBundlePath = EditorGUILayout.DelayedTextField("Name", m_InputAssetBundlePath);
                    newAssetBundlePath = newAssetBundlePath.Trim();
                    if (m_InputAssetBundlePath != newAssetBundlePath)
                    {
                        m_InputAssetBundlePath = newAssetBundlePath.ToLower();
                        if (m_InputAssetBundlePath.StartsWith("/"))
                        {
                            m_InputAssetBundlePath = m_InputAssetBundlePath.Substring(1);
                        }
                    }

                    var newNewAssetBundleGroup = EditorGUILayout.DelayedIntField("Group", m_InputAssetBundleGroup);
                    if (newNewAssetBundleGroup != m_InputAssetBundleGroup)
                    {
                        m_InputAssetBundleGroup = Mathf.Max(newNewAssetBundleGroup, 0);
                    }

                    m_InputDontContainDontPack = EditorGUILayout.ToggleLeft("Don't Pack In Installer", m_InputDontContainDontPack);

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Create"))
                        {
                            TryCreateNewAssetBundle();
                        }

                        if (GUILayout.Button("Cancel"))
                        {
                            ClearInput();
                            m_Status = Status.Normal;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUIUtility.labelWidth = cachedLabelWidth;
                }
                EditorGUILayout.EndVertical();
            }

            private void DrawEditingAssetBundleBottomView()
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Edit", EditorStyles.boldLabel);

                    var cachedLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = DefaultLabelWidth;
                    var newAssetBundlePath = EditorGUILayout.DelayedTextField("Name", m_InputAssetBundlePath);
                    newAssetBundlePath = newAssetBundlePath.Trim();
                    if (m_InputAssetBundlePath != newAssetBundlePath)
                    {
                        m_InputAssetBundlePath = newAssetBundlePath.ToLower();
                        if (m_InputAssetBundlePath.StartsWith("/"))
                        {
                            m_InputAssetBundlePath = m_InputAssetBundlePath.Substring(1);
                        }
                    }

                    var newNewAssetBundleGroup = EditorGUILayout.DelayedIntField("Group", m_InputAssetBundleGroup);
                    if (newNewAssetBundleGroup != m_InputAssetBundleGroup)
                    {
                        m_InputAssetBundleGroup = Mathf.Max(newNewAssetBundleGroup, 0);
                    }

                    m_InputDontContainDontPack = EditorGUILayout.ToggleLeft("Don't Contain In Installer", m_InputDontContainDontPack);

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Confirm"))
                        {
                            TryEditingAssetBundle();
                        }

                        if (GUILayout.Button("Cancel"))
                        {
                            ClearInput();
                            m_Status = Status.Normal;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUIUtility.labelWidth = cachedLabelWidth;
                }
                EditorGUILayout.EndVertical();
            }

            private void DrawAssetBundleTreeRecursively(AssetBundleOrganizer.AssetBundleInfo abInfo, int indentCount)
            {
                var satelliteData = EnsureSatelliteData(abInfo);
                var horizontalRect = EditorGUILayout.BeginHorizontal(ExpandWidth);
                {
                    var indentWidth = indentCount * DefaultIndentWidth;
                    GUILayout.Space(indentWidth);
                    GUIContent label = new GUIContent();
                    if (abInfo.IsDirectory)
                    {
                        label.image = EditorGUIUtility.FindTexture("Folder Icon");
                        var newFoldOut = EditorGUI.Foldout(new Rect(horizontalRect.x + indentWidth,
                            horizontalRect.y, FoldoutWidth, horizontalRect.height), satelliteData.Foldout, string.Empty);
                        if (newFoldOut != satelliteData.Foldout)
                        {
                            if (Event.current.alt)
                            {
                                FoldoutRecursively(abInfo, newFoldOut);
                            }
                            else
                            {
                                satelliteData.Foldout = newFoldOut;
                            }
                        }

                        GUILayout.Space(FoldoutWidth);
                        label.text = abInfo.Name;
                    }
                    else
                    {
                        // Use the built-in "DefaultAsset Icon", which cannot be displayed by calling
                        // EditorGUIUtility.FindTexture("DefaultAsset Icon").
                        label.image = UnityEditorInternal.InternalEditorUtility.GetIconForFile(string.Empty);

                        var oldSelected = m_EditorWindow.m_SelectedAssetBundleInfo == abInfo;
                        var newSelected = EditorGUILayout.Toggle(oldSelected, GUILayout.Width(ToggleWidth));

                        if (m_Status == Status.Edit)
                        {
                            // Do nothing, which means not to change the selected asset bundle path during renaming it.
                        }
                        else if (!oldSelected && newSelected)
                        {
                            m_EditorWindow.m_SelectedAssetBundleInfo = abInfo;
                        }
                        else if (oldSelected && !newSelected)
                        {
                            m_EditorWindow.m_SelectedAssetBundleInfo = null;
                        }

                        label.text = Core.Utility.Text.Format("{0} [{1}{2}]", abInfo.Name, abInfo.GroupId,
                            abInfo.DontPack ? string.Empty : ", Packed");
                    }

                    EditorGUILayout.LabelField(label, MinWidthOne);
                }
                EditorGUILayout.EndHorizontal();

                if (abInfo.IsDirectory && satelliteData.Foldout)
                {
                    foreach (var child in abInfo.Children.Values)
                    {
                        DrawAssetBundleTreeRecursively(child, indentCount + 1);
                    }
                }
            }

            private void FoldoutRecursively(AssetBundleOrganizer.AssetBundleInfo rootABInfo, bool foldout)
            {
                EnsureSatelliteData(rootABInfo).Foldout = foldout;
                foreach (var child in rootABInfo.Children.Values)
                {
                    if (child.IsDirectory)
                    {
                        FoldoutRecursively(child, foldout);
                    }
                }
            }

            private AssetBundleInfoSatelliteData EnsureSatelliteData(AssetBundleOrganizer.AssetBundleInfo abInfo)
            {
                AssetBundleInfoSatelliteData satelliteData;
                if (!m_EditorWindow.m_AssetBundleInfoSatelliteDatas.TryGetValue(abInfo.Path, out satelliteData))
                {
                    satelliteData = new AssetBundleInfoSatelliteData();
                    m_EditorWindow.m_AssetBundleInfoSatelliteDatas.Add(abInfo.Path, satelliteData);
                }

                return satelliteData;
            }

            private void TryCreateNewAssetBundle()
            {
                if (!CheckInputAssetBundleName())
                {
                    return;
                }

                var assetBundleInfo =
                    Organizer.CreateNewAssetBundle(m_InputAssetBundlePath, m_InputAssetBundleGroup, m_InputDontContainDontPack);
                EnsureSatelliteData(assetBundleInfo);

                m_EditorWindow.m_SelectedAssetBundleInfo = assetBundleInfo;
                for (var node = assetBundleInfo; node != null; node = node.Parent)
                {
                    var satelliteData = EnsureSatelliteData(node);
                    satelliteData.Foldout = true;
                }

                ClearInput();
                m_Status = Status.Normal;
            }

            private void TryEditingAssetBundle()
            {
                var oldABPath = m_EditorWindow.m_SelectedAssetBundleInfo.Path;
                if (oldABPath == m_InputAssetBundlePath)
                {
                    Organizer.RegroupAssetBundle(oldABPath, m_InputAssetBundleGroup);
                    Organizer.SetAssetBundleDontPack(oldABPath, m_InputDontContainDontPack);
                    ClearInput();
                    m_Status = Status.Normal;
                    return;
                }

                if (!CheckInputAssetBundleName())
                {
                    return;
                }

                Organizer.RegroupAssetBundle(oldABPath, m_InputAssetBundleGroup);
                Organizer.SetAssetBundleDontPack(oldABPath, m_InputDontContainDontPack);
                Organizer.RenameAssetBundle(oldABPath, m_InputAssetBundlePath);
                m_EditorWindow.m_AssetBundleInfoSatelliteDatas.Remove(oldABPath);
                EnsureSatelliteData(Organizer.GetAssetBundleInfo(m_InputAssetBundlePath));
                ClearInput();
                m_Status = Status.Normal;
            }

            private bool CheckInputAssetBundleNameValid()
            {
                if (!AssetBundleOrganizer.AssetBundlePathIsValid(m_InputAssetBundlePath))
                {
                    EditorUtility.DisplayDialog("Oops!", Core.Utility.Text.Format(
                        "Asset bundle path '{0}' is not valid. It should use '/' to delimit segments, each of which should match '{1}'",
                        m_InputAssetBundlePath, AssetBundleOrganizer.AssetBundlePathSegmentRegex), "Okay");
                    return false;
                }

                return true;
            }

            private bool CheckInputAssetBundleNameAvailable()
            {
                if (!Organizer.AssetBundlePathIsAvailable(m_InputAssetBundlePath))
                {
                    EditorUtility.DisplayDialog("Oops!", Core.Utility.Text.Format(
                        "Asset bundle path '{0}' or part of it is already used.", m_InputAssetBundlePath), "Okay");
                    return false;
                }

                return true;
            }

            private bool CheckInputAssetBundleName()
            {
                return CheckInputAssetBundleNameValid() && CheckInputAssetBundleNameAvailable();
            }

            private void ClearInput()
            {
                m_InputAssetBundleGroup = 0;
                m_InputAssetBundlePath = string.Empty;
                m_InputDontContainDontPack = false;
            }
        }
    }
}