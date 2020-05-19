using System.Collections.Generic;
using UnityEditor;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleBuilderConfig
    {
        public string WorkingDirectory = string.Empty;

        /// <summary>
        /// Use this to override the internal directory where asset bundles are generated.
        /// </summary>
        public string OverriddenInternalDirectory = string.Empty;

        public BuildAssetBundleOptions BuildAssetBundleOptions;

        /// <summary>
        /// Whether we should clean up all unused asset bundles after building.
        /// </summary>
        public bool CleanUpWorkingDirectoryAfterBuild = true;

        public readonly List<PlatformConfig> PlatformConfigs = new List<PlatformConfig>();
    }
}
