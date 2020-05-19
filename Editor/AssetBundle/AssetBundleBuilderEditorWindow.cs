using COL.UnityGameWheels.Unity.Asset;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace COL.UnityGameWheels.Unity.Editor
{
    public class AssetBundleBuilderEditorWindow : EditorWindow
    {
        private const int TargetPlatformColumnWidth = 160;
        private const int SkipBuildColumnWidth = 100;
        private const int InternalResourceVersionColumnWidth = 160;
        private const int IncrementVersionColumnWidth = 160;

        private AssetBundleBuilder m_AssetBundleBuilder = null;

        private AssetBundleBuilderConfig Config => m_AssetBundleBuilder?.Config;

        private Vector2 m_ScrollPosition;

        private readonly Dictionary<ResourcePlatform, int> m_InternalResourceVersions = new Dictionary<ResourcePlatform, int>();

        public static void Open()
        {
            var window = GetWindow<AssetBundleBuilderEditorWindow>(true, "Asset Bundle Builder");
            window.minSize = new Vector2(240f, 360f);
        }

        #region EditorWindow

        private void OnEnable()
        {
            m_AssetBundleBuilder = new AssetBundleBuilder();
            InitInternalResourceVersions();
        }

        private void OnGUI()
        {
            var shouldBuild = false;
            EditorGUILayout.BeginVertical();
            {
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
                {
                    DrawConfigPath();
                    DrawWorkingDirectory();
                    DrawOverriddenInternalDirectory();
                    DrawCleanUpWorkingDirectoryAfterBuild();
                    DrawBuildOptions();
                    DrawPlatformConfigs();
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndScrollView();
                DrawButtonSection(ref shouldBuild);
            }
            EditorGUILayout.EndVertical();

            if (shouldBuild)
            {
                m_AssetBundleBuilder.BuildAll();
            }
        }

        private void DrawCleanUpWorkingDirectoryAfterBuild()
        {
            Config.CleanUpWorkingDirectoryAfterBuild =
                EditorGUILayout.ToggleLeft("Clean Up Working Directory After Build", Config.CleanUpWorkingDirectoryAfterBuild);
        }

        #endregion EditorWindow

        private void InitInternalResourceVersions()
        {
            if (m_AssetBundleBuilder == null)
            {
                return;
            }

            foreach (var platformConfig in m_AssetBundleBuilder.Config.PlatformConfigs)
            {
                m_InternalResourceVersions[platformConfig.TargetPlatform] =
                    AssetBundleBuilder.GetInternalResourceVersion(PlayerSettings.bundleVersion, platformConfig.TargetPlatform);
            }
        }

        private void DrawConfigPath()
        {
            EditorGUILayout.LabelField("Config path: " + AssetBundleBuilder.ConfigPath);
        }

        private void DrawWorkingDirectory()
        {
            var newWorkingDirectory = EditorGUILayout.DelayedTextField("Working Directory", Config.WorkingDirectory);

            if (string.IsNullOrEmpty(Config.WorkingDirectory))
            {
                EditorGUILayout.HelpBox(Core.Utility.Text.Format("Default value '{0}' will be used",
                    AssetBundleBuilder.DefaultWorkingDirectory), MessageType.Info);
            }

            if (Config.WorkingDirectory == newWorkingDirectory)
            {
                return;
            }

            if (newWorkingDirectory.Any(ch => Path.GetInvalidPathChars().Contains(ch)))
            {
                EditorUtility.DisplayDialog(titleContent.text, "Illegal working directory.", "Okay");
                return;
            }

            Config.WorkingDirectory = newWorkingDirectory;
        }

        private void DrawOverriddenInternalDirectory()
        {
            var newOverriddenInternalDirectory = EditorGUILayout.DelayedTextField("Overridden Internal Directory", Config.OverriddenInternalDirectory);

            if (Config.OverriddenInternalDirectory == newOverriddenInternalDirectory)
            {
                return;
            }

            if (newOverriddenInternalDirectory.Any(ch => Path.GetInvalidPathChars().Contains(ch)))
            {
                EditorUtility.DisplayDialog(titleContent.text, "Illegal overridden internal directory.", "Okay");
                return;
            }

            Config.OverriddenInternalDirectory = newOverriddenInternalDirectory;
        }

        private void DrawBuildOptions()
        {
            EditorGUILayout.LabelField("Build Options:");
            EditorGUILayout.BeginVertical("box");
            {
                foreach (var rawValue in System.Enum.GetValues(typeof(BuildAssetBundleOptions)))
                {
                    BuildAssetBundleOptions value = (BuildAssetBundleOptions)rawValue;
                    if (!AssetBundleBuilder.ValidBuildOptions.Contains(value))
                    {
                        continue;
                    }

                    var displayName = ObjectNames.NicifyVariableName(value.ToString());
                    var oldToggle = (Config.BuildAssetBundleOptions & value) != 0;
                    var toggle = EditorGUILayout.ToggleLeft(displayName, oldToggle);
                    if (toggle != oldToggle)
                    {
                        Config.BuildAssetBundleOptions ^= value;
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawPlatformConfigs()
        {
            EditorGUILayout.LabelField("Platform configs:");
            EditorGUILayout.BeginVertical("box");
            {
                DrawPlatformConfigHeader();
                EditorGUILayout.Space();
                foreach (var platformConfig in Config.PlatformConfigs)
                {
                    DrawOnePlatformConfig(platformConfig);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawPlatformConfigHeader()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Target Platform", GUILayout.Width(TargetPlatformColumnWidth));
                EditorGUILayout.LabelField("Skip Build", GUILayout.Width(SkipBuildColumnWidth));
                EditorGUILayout.LabelField("Increment Version", GUILayout.Width(IncrementVersionColumnWidth));
                EditorGUILayout.LabelField("Internal Resource Version", GUILayout.Width(InternalResourceVersionColumnWidth));
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawOnePlatformConfig(AssetBundleBuilderConfig.PlatformConfig platformConfig)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(platformConfig.TargetPlatform.ToString()),
                    GUILayout.Width(TargetPlatformColumnWidth));
                DrawSkipBuildToggle(platformConfig);
                DrawIncrementVersionToggle(platformConfig);
                DrawInternalResourceVersion(platformConfig);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawIncrementVersionToggle(AssetBundleBuilderConfig.PlatformConfig platformConfig)
        {
            platformConfig.AutomaticIncrementResourceVersion = EditorGUILayout.Toggle(platformConfig.AutomaticIncrementResourceVersion,
                GUILayout.Width(IncrementVersionColumnWidth));
        }

        private static void DrawSkipBuildToggle(AssetBundleBuilderConfig.PlatformConfig platformConfig)
        {
            platformConfig.SkipBuild = EditorGUILayout.Toggle(platformConfig.SkipBuild, GUILayout.Width(SkipBuildColumnWidth));
        }

        private void DrawInternalResourceVersion(AssetBundleBuilderConfig.PlatformConfig platformConfig)
        {
            var internalResourceVersion = m_InternalResourceVersions[platformConfig.TargetPlatform];
            var newInternalResourceVersion = EditorGUILayout.DelayedIntField(internalResourceVersion,
                GUILayout.Width(InternalResourceVersionColumnWidth));
            if (newInternalResourceVersion != internalResourceVersion)
            {
                if (newInternalResourceVersion <= 0)
                {
                    EditorUtility.DisplayDialog(titleContent.text, "Illegal internal resource version.", "Okay");
                }
                else
                {
                    m_InternalResourceVersions[platformConfig.TargetPlatform] = newInternalResourceVersion;
                    AssetBundleBuilder.SetInternalResourceVersion(PlayerSettings.bundleVersion,
                        platformConfig.TargetPlatform, newInternalResourceVersion);
                }
            }
        }

        private void DrawButtonSection(ref bool shouldBuild)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Build"))
                {
                    shouldBuild = true;
                }

                if (GUILayout.Button("Save Config"))
                {
                    m_AssetBundleBuilder.SaveConfig();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}