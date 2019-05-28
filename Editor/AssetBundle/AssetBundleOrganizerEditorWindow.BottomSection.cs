using System.Text;
using UnityEditor;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizerEditorWindow
    {
        private class BottomSection : BaseSection
        {
            public BottomSection(AssetBundleOrganizerEditorWindow editorWindow)
                : base(editorWindow)
            {
                // Empty.
            }

            public override void Draw()
            {
                EditorGUILayout.BeginHorizontal();
                {
                    //EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Config path: " + AssetBundleOrganizer.ConfigPath);
                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.Space();

                    if (GUILayout.Button(new GUIContent("Clean Up", "Clean up missing assets.")))
                    {
                        CleanUp();
                    }

                    if (GUILayout.Button(new GUIContent("Check Consistency", "Check config data consistency. For debug use.")))
                    {
                        CheckConsistency();
                    }

                    if (GUILayout.Button("Check Dependency Legality"))
                    {
                        CheckDependencyLegality();
                    }

                    if (GUILayout.Button("Save Config"))
                    {
                        Organizer.SaveConfig();
                        GUIUtility.ExitGUI();
                    }

                    if (GUILayout.Button(new GUIContent("Build...", "Open the build window.")))
                    {
                        m_EditorWindow.m_ShouldClose = true;
                        OpenBuildWindow();
                    }

                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndHorizontal();
            }

            private void CheckConsistency()
            {
                var sb = new StringBuilder();
                sb.AppendLine("Consistency problems:");
                bool everythingOkay = CheckConsistencyFromAssetBundleSide(sb);

                if (!everythingOkay)
                {
                    Debug.LogWarning(sb.ToString());
                    EditorUtility.DisplayDialog("Consistency checking", "Problems found. See console log for details.", "Okay");
                }
                else
                {
                    EditorUtility.DisplayDialog("Consistency checking", "Seems everything is fine.", "Okay");
                }
            }

            private bool CheckConsistencyFromAssetBundleSide(StringBuilder logBuilder)
            {
                var everythingOkay = true;
                foreach (var kv in Organizer.ConfigCache.AssetBundleInfos)
                {
                    var assetBundleInfo = kv.Value;
                    foreach (var assetGuid in assetBundleInfo.AssetGuids)
                    {
                        var assetInfo = Organizer.GetAssetInfo(assetGuid);
                        if (assetInfo == null)
                        {
                            everythingOkay = false;
                            logBuilder.AppendLine(
                                $"Asset with GUID '{assetGuid}' not found in asset infos but is in asset bundle '{assetBundleInfo.AssetBundlePath}'");
                        }
                        else if (assetInfo.AssetBundlePath != assetBundleInfo.AssetBundlePath)
                        {
                            everythingOkay = false;
                            var assetPath = assetInfo.Path;
                            logBuilder.AppendLine(
                                $"Asset '{assetPath}' (GUID='{assetGuid}') thinks its in asset bundle '{assetInfo.AssetBundlePath}', " +
                                $"but asset bundle '{assetBundleInfo.AssetBundlePath}' claims to include it.");
                        }
                    }
                }

                return everythingOkay;
            }

            private void CleanUp()
            {
                int removeCount = Organizer.CleanUpInvalidAssets();
                string message = Core.Utility.Text.Format("Removed {0} invalid asset(s).", removeCount);
                m_EditorWindow.m_AssetBundleContentsSection.Refresh();

                Debug.Log(message);
                EditorUtility.DisplayDialog("Clean Up Invalid Assets", message, "Okay");
            }

            private void OpenBuildWindow()
            {
                AssetBundleBuilderEditorWindow.Open();
            }

            private void CheckDependencyLegality()
            {
                var abInfosProvider = new AssetBundleInfosProvider(Organizer);
                abInfosProvider.PopulateData();
                var abInfos = abInfosProvider.AssetBundleInfos;

                abInfosProvider.CheckCycleAssetDependencies();
                var cycleDeps = abInfosProvider.CycleAssetDependencies;

                if (cycleDeps.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Cycle asset dependencies:");
                    foreach (var scc in cycleDeps)
                    {
                        sb.Append("[");
                        sb.Append(AssetDatabase.GUIDToAssetPath(scc[0].Guid));
                        for (int i = 1; i < scc.Count; i++)
                        {
                            sb.Append(", ");
                            sb.Append(AssetDatabase.GUIDToAssetPath(scc[i].Guid));
                        }

                        sb.Append("]\n");
                    }

                    Debug.LogWarning(sb.ToString());
                    EditorUtility.DisplayDialog(m_EditorWindow.titleContent.text,
                        Core.Utility.Text.Format("There are cycle asset dependencies. See console log for details."), "Okay");
                    return;
                }

                abInfosProvider.CheckIllegalGroupDependencies();
                var illegalGroupDeps = abInfosProvider.IllegalGroupDependencies;

                if (illegalGroupDeps.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Illegal group dependencies:");
                    foreach (var pair in illegalGroupDeps)
                    {
                        var firstAssetInfo = pair.Key;
                        var firstABInfo = abInfos[firstAssetInfo.AssetBundlePath];
                        var secondAssetInfo = pair.Value;
                        var secondABInfo = abInfos[secondAssetInfo.AssetBundlePath];
                        sb.AppendFormat("'{0}' (AssetBundle '{1}' in Group {2}) --> '{3}' (AssetBundle '{4}' in Group {5})\n",
                            AssetDatabase.GUIDToAssetPath(firstAssetInfo.Guid), firstAssetInfo.AssetBundlePath, firstABInfo.GroupId,
                            AssetDatabase.GUIDToAssetPath(secondAssetInfo.Guid), secondAssetInfo.AssetBundlePath, secondABInfo.GroupId);
                    }

                    Debug.LogWarning(sb.ToString());
                    EditorUtility.DisplayDialog(m_EditorWindow.titleContent.text,
                        Core.Utility.Text.Format("{0} illegal group dependency(ies) detected. See console log for details.", illegalGroupDeps.Count),
                        "Okay");
                    return;
                }

                EditorUtility.DisplayDialog(m_EditorWindow.titleContent.text, "Depedencies are all legal.", "Okay");
            }
        }
    }
}