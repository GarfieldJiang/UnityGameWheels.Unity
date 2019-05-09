namespace COL.UnityGameWheels.Unity.Editor
{
    using Asset;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;
    using UnityEditor;
    using UnityEngine;

    public partial class AssetBundleBuilder
    {
        private const string DefaultConfigPath = "Assets/AssetBundleBuilderConfig.xml";
        private const string InternalDirectoryName = "Internal";
        private const string OutputDirectoryName = "Out";
        private const string LogDirectoryName = "Log";
        private const string CurrentLogFileName = "current.log";
        private const string PreviousLogFileName = "prev.log";

        private const string InternalResourceVersionKeyFormat =
            "COL.UnityGameWheels.Unity.Editor.AssetBundleBuilder.InternalResourceVersion_{0}_{1}";

        private const string IndexFileName = Core.Asset.Constant.IndexFileName;

        private static string s_ConfigPath = null;

        internal static string ConfigPath
        {
            get
            {
                if (s_ConfigPath == null)
                {
                    s_ConfigPath = Utility.Config.Read<AssetBundleBuilderConfigPathAttribute, string>() ?? DefaultConfigPath;
                }

                return s_ConfigPath;
            }
        }

        private static string s_DefaultWorkingDirectory = null;

        internal static string DefaultWorkingDirectory
        {
            get
            {
                if (s_DefaultWorkingDirectory == null)
                {
                    var currectDirName = Path.GetFileName(Directory.GetCurrentDirectory());
                    var parentDir = Directory.GetParent(Directory.GetCurrentDirectory());
                    s_DefaultWorkingDirectory = Path.Combine(parentDir.FullName, currectDirName + "_AssetBundles");
                }

                return s_DefaultWorkingDirectory;
            }
        }

        private string WorkingDirectory
        {
            get { return string.IsNullOrEmpty(m_Config.WorkingDirectory) ? DefaultWorkingDirectory : m_Config.WorkingDirectory; }
        }

        private string InternalDirectory
        {
            get { return Path.Combine(WorkingDirectory, InternalDirectoryName); }
        }

        private string GetPlatformInternalDirectory(ResourcePlatform targetPlatform)
        {
            return Path.Combine(InternalDirectory, targetPlatform.ToString());
        }

        private string OutputDirectory
        {
            get { return Path.Combine(WorkingDirectory, OutputDirectoryName); }
        }

        private string LogDirectory
        {
            get { return Path.Combine(WorkingDirectory, LogDirectoryName); }
        }

        private AssetBundleBuilderConfig m_Config = new AssetBundleBuilderConfig();
        private XmlSerializer m_ConfigSerializer = new XmlSerializer(typeof(AssetBundleBuilderConfig));

        public AssetBundleBuilderConfig Config
        {
            get { return m_Config; }
        }

        public AssetBundleBuilder()
        {
            LoadConfig();
            EnsureDirectories();
        }

        private void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                using (var sw = new StreamWriter(ConfigPath))
                {
                    m_ConfigSerializer.Serialize(sw, m_Config);
                }

                AssetDatabase.ImportAsset(ConfigPath);
            }

            using (var sr = new StreamReader(ConfigPath))
            {
                m_Config = (AssetBundleBuilderConfig)m_ConfigSerializer.Deserialize(sr);
            }

            if (m_Config.PlatformConfigs.Count <= 0)
            {
                m_Config.PlatformConfigs.Add(new AssetBundleBuilderConfig.PlatformConfig {TargetPlatform = ResourcePlatform.Standalone});
                m_Config.PlatformConfigs.Add(new AssetBundleBuilderConfig.PlatformConfig {TargetPlatform = ResourcePlatform.Android});
                m_Config.PlatformConfigs.Add(new AssetBundleBuilderConfig.PlatformConfig {TargetPlatform = ResourcePlatform.iOS});
            }
        }

        public void SaveConfig()
        {
            using (var sw = new StreamWriter(ConfigPath))
            {
                m_ConfigSerializer.Serialize(sw, m_Config);
            }

            AssetDatabase.ImportAsset(ConfigPath);
        }

        /// <summary>
        /// Build asset bundles for a given target resource platform.
        /// </summary>
        /// <param name="targetPlatform">The target resource platform.</param>
        /// <param name="cleanUpWorkingDirectoryAfterBuild">Whether to clean up working directory after build.</param>
        /// <param name="autoIncrementResourceVersion">Whether to increment resource version.</param>
        /// <param name="buildOptions">Unity BuildAssetBundleOptions.</param>
        public void BuildPlatform(ResourcePlatform targetPlatform, bool cleanUpWorkingDirectoryAfterBuild,
            bool autoIncrementResourceVersion, BuildAssetBundleOptions buildOptions)
        {
            var logger = new Logger(this);
            try
            {
                IDictionary<string, AssetInfo> assetInfos;
                AssetBundleBuild[] buildMaps;
                var assetBundleInfos = BeforeBuild(logger, out assetInfos, out buildMaps);

                DoBuildPlatform(targetPlatform, cleanUpWorkingDirectoryAfterBuild, autoIncrementResourceVersion,
                    buildOptions, logger, buildMaps, assetBundleInfos, assetInfos);
            }
            catch (Exception e)
            {
                logger.Error("{0}: {1}\n{2}", e.GetType().FullName, e.Message, e.StackTrace);
                throw;
            }
            finally
            {
                logger.Dispose();
            }
        }

        /// <summary>
        /// Build all assetbundles according to config.
        /// </summary>
        public void BuildAll()
        {
            var logger = new Logger(this);
            try
            {
                IDictionary<string, AssetInfo> assetInfos;
                AssetBundleBuild[] buildMaps;
                var assetBundleInfos = BeforeBuild(logger, out assetInfos, out buildMaps);
                var cleanUpWorkingDirectoryAfterBuild = m_Config.CleanUpWorkingDirectoryAfterBuild;

                foreach (var platformConfig in m_Config.PlatformConfigs)
                {
                    logger.Info(new string('-', 50));

                    if (platformConfig.SkipBuild)
                    {
                        logger.Info("Skip building for '{0}' target.", platformConfig.TargetPlatform);
                        continue;
                    }

                    DoBuildPlatform(platformConfig.TargetPlatform, cleanUpWorkingDirectoryAfterBuild,
                        platformConfig.AutomaticIncrementResourceVersion, m_Config.BuildAssetBundleOptions,
                        logger, buildMaps, assetBundleInfos, assetInfos);
                }
            }
            catch (Exception e)
            {
                logger.Error("{0}: {1}\n{2}", e.GetType().FullName, e.Message, e.StackTrace);
                throw;
            }
            finally
            {
                logger.Dispose();
            }
        }

        private void DoBuildPlatform(ResourcePlatform targetPlatform,
            bool cleanUpWorkingDirectoryAfterBuild,
            bool autoIncVersion,
            BuildAssetBundleOptions buildOptions,
            Logger logger, AssetBundleBuild[] buildMaps,
            IDictionary<string, AssetBundleInfo> assetBundleInfos,
            IDictionary<string, AssetInfo> assetInfos)
        {
            logger.Info("Start building for '{0}' target.", targetPlatform);
            logger.Info("Start building asset bundles.");
            var manifest = BuildAssetBundles(buildMaps, targetPlatform, buildOptions);

            if (manifest == null)
            {
                logger.Error("Failed to build asset bundles for '{0}' target.", targetPlatform);
            }

            logger.Info("Finish building asset bundles.");

            if (cleanUpWorkingDirectoryAfterBuild)
            {
                logger.Info("Start cleaning up internal directory.");
                CleanUpInternalDirectory(targetPlatform, assetBundleInfos);
                logger.Info("Finish cleaning up internal directory.");
            }

            int resourceVersion = GetInternalResourceVersion(PlayerSettings.bundleVersion, targetPlatform);
            logger.Info("Internal resource version for bundle version '{0}' and target platform '{1}' is {2}.",
                PlayerSettings.bundleVersion, targetPlatform, resourceVersion);

            logger.Info("Start generating information used to build the index file.");
            IList<AssetBundleInfoForIndex> assetBundleInfosForIndex =
                GenerateAssetBundleInfosForIndex(assetBundleInfos, manifest, targetPlatform);
            logger.Info("Finish generating information used to build the index file.");

            logger.Info("Start generating output.");
            GenerateOutput(PlayerSettings.bundleVersion, targetPlatform, resourceVersion,
                assetBundleInfosForIndex, assetInfos);
            logger.Info("Finish generating output.");

            if (autoIncVersion)
            {
                SetInternalResourceVersion(PlayerSettings.bundleVersion, targetPlatform, resourceVersion + 1);
                logger.Info("Increment internal resource version for next build.");
            }

            logger.Info("Finish building for '{0}' target.", targetPlatform);
        }

        private IDictionary<string, AssetBundleInfo> BeforeBuild(Logger logger, out IDictionary<string, AssetInfo> assetInfos,
            out AssetBundleBuild[] buildMaps)
        {
            logger.Info("Start populating asset bundle infos.");
            var provider = PopulateAssetBundleInfos();
            var assetBundleInfos = provider.AssetBundleInfos;
            assetInfos = provider.AssetInfos;
            logger.Info("Finish populating asset bundle infos.");

            logger.Info("Start generating unity build maps.");
            buildMaps = GenerateBuildMaps(assetBundleInfos);
            logger.Info("Finish generating unity build maps.");
            return assetBundleInfos;
        }

        private AssetBundleInfosProvider PopulateAssetBundleInfos()
        {
            var organizer = new AssetBundleOrganizer();
            organizer.RefreshAssetForest();
            organizer.RefreshAssetBundleTree();
            var provider = new AssetBundleInfosProvider(organizer);
            provider.PopulateData();
            return provider;
        }

        private IList<AssetBundleInfoForIndex> GenerateAssetBundleInfosForIndex(
            IDictionary<string, AssetBundleInfo> assetBundleInfos,
            AssetBundleManifest manifest,
            ResourcePlatform targetPlatform)
        {
            var ret = new List<AssetBundleInfoForIndex>();
            foreach (var assetBundleInfo in assetBundleInfos.Values)
            {
                var assetBundlePath = assetBundleInfo.Path;
                var fileInfo = new FileInfo(Path.Combine(
                    GetPlatformInternalDirectory(targetPlatform), assetBundlePath));
                var fileSize = fileInfo.Length;
                int groupId = assetBundleInfo.GroupId;
                uint crc32;
                using (var fs = fileInfo.OpenRead())
                {
                    crc32 = Core.Algorithm.Crc32.Sum(fs);
                }

                Hash128 hash = manifest.GetAssetBundleHash(assetBundlePath);

                ret.Add(new AssetBundleInfoForIndex
                {
                    Path = assetBundlePath,
                    GroupId = groupId,
                    Size = fileSize,
                    Crc32 = crc32,
                    Hash = hash.ToString(),
                    DontPack = assetBundleInfo.DontPack,
                });
            }

            return ret;
        }

        private void EnsureDirectories()
        {
            Directory.CreateDirectory(InternalDirectory);
            Directory.CreateDirectory(OutputDirectory);
            Directory.CreateDirectory(LogDirectory);
        }

        private AssetBundleBuild[] GenerateBuildMaps(IDictionary<string, AssetBundleInfo> assetBundleInfos)
        {
            var buildMaps = new List<AssetBundleBuild>();
            foreach (var abInfo in assetBundleInfos.Values)
            {
                if (abInfo.AssetGuids.Count <= 0)
                {
                    continue;
                }

                var buildMap = new AssetBundleBuild
                {
                    assetBundleName = abInfo.Path,
                    assetNames = abInfo.AssetGuids.ConvertAll(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray()
                };
                buildMaps.Add(buildMap);
            }

            return buildMaps.ToArray();
        }

        private BuildTarget GetBuildTargetFromResourcePlatform(ResourcePlatform ResourcePlatform)
        {
            switch (ResourcePlatform)
            {
                case ResourcePlatform.Android:
                    return BuildTarget.Android;
                case ResourcePlatform.iOS:
                    return BuildTarget.iOS;
                case ResourcePlatform.Standalone:
                    return Application.platform == RuntimePlatform.WindowsEditor ? BuildTarget.StandaloneWindows64 :
                        Application.platform == RuntimePlatform.LinuxEditor ? BuildTarget.StandaloneLinuxUniversal :
                        BuildTarget.StandaloneOSX;
                default:
                    throw new ArgumentOutOfRangeException("ResourcePlatform", "Unsupported asset bundle platform " + ResourcePlatform);
            }
        }

        private AssetBundleManifest BuildAssetBundles(AssetBundleBuild[] buildMaps, ResourcePlatform targetPlatform,
            BuildAssetBundleOptions buildOptions)
        {
            var platformInternalDir = GetPlatformInternalDirectory(targetPlatform);
            Directory.CreateDirectory(platformInternalDir);

            var newBuildOptions = BuildAssetBundleOptions.DeterministicAssetBundle;
            foreach (var validOption in ValidBuildOptions)
            {
                newBuildOptions |= buildOptions & validOption;
            }

            return BuildPipeline.BuildAssetBundles(
                platformInternalDir, buildMaps, newBuildOptions,
                GetBuildTargetFromResourcePlatform(targetPlatform));
        }

        private void CleanUpInternalDirectory(ResourcePlatform targetPlatform, IDictionary<string, AssetBundleInfo> assetBundleInfos)
        {
            var platformInternalDir = GetPlatformInternalDirectory(targetPlatform);
            var directoryInfo = Directory.CreateDirectory(platformInternalDir);
            var directoryUrl = new Uri(directoryInfo.FullName + Path.DirectorySeparatorChar, UriKind.Absolute);
            foreach (var fileInfo in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                if (fileInfo.Extension == ".manifest" || fileInfo.FullName == Path.Combine(directoryInfo.FullName, directoryInfo.Name))
                {
                    continue;
                }

                var fileInfoUrl = new Uri(fileInfo.FullName, UriKind.Absolute);
                var relativePath = directoryUrl.MakeRelativeUri(fileInfoUrl).ToString();
                if (!assetBundleInfos.ContainsKey(relativePath))
                {
                    fileInfo.Delete();
                    File.Delete(fileInfo.FullName + ".manifest");
                }
            }

            Core.Utility.IO.DeleteEmptyFolders(directoryInfo);
        }

        private void GenerateOutput(string bundleVersion, ResourcePlatform targetPlatform, int internalResourceVersion,
            IList<AssetBundleInfoForIndex> assetBundleInfosForIndex, IDictionary<string, AssetInfo> assetInfos)
        {
            new OutputGeneratorInstaller(this).Run(
                bundleVersion,
                targetPlatform,
                internalResourceVersion,
                assetBundleInfosForIndex,
                assetInfos);
            new OutputGeneratorRemote(this).Run(
                bundleVersion,
                targetPlatform,
                internalResourceVersion,
                assetBundleInfosForIndex,
                assetInfos);
        }

        public static int GetInternalResourceVersion(string bundleVersion, ResourcePlatform targetPlatform)
        {
            var ret = PlayerPrefs.GetInt(Core.Utility.Text.Format(InternalResourceVersionKeyFormat, bundleVersion,
                targetPlatform.ToString()));
            return Mathf.Max(ret, 1);
        }

        public static void SetInternalResourceVersion(string bundleVersion, ResourcePlatform targetPlatform, int internalResourceVersion)
        {
            if (internalResourceVersion <= 0)
            {
                throw new ArgumentOutOfRangeException("internalResourceVersion",
                    Core.Utility.Text.Format("Should be positive, but is {0}.", internalResourceVersion));
            }

            PlayerPrefs.SetInt(Core.Utility.Text.Format(InternalResourceVersionKeyFormat, bundleVersion, targetPlatform.ToString()),
                internalResourceVersion);
            PlayerPrefs.Save();
        }

        public static readonly BuildAssetBundleOptions[] ValidBuildOptions = new BuildAssetBundleOptions[]
        {
            BuildAssetBundleOptions.UncompressedAssetBundle,
            BuildAssetBundleOptions.ChunkBasedCompression,
            BuildAssetBundleOptions.StrictMode,
            BuildAssetBundleOptions.DryRunBuild,
            BuildAssetBundleOptions.DisableWriteTypeTree,
            BuildAssetBundleOptions.ForceRebuildAssetBundle,
            BuildAssetBundleOptions.IgnoreTypeTreeChanges,
        };
    }
}