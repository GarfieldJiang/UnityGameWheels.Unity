using System.Collections.Generic;
using UnityEditor;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleBuilderConfig
    {
        public string WorkingDirectory = string.Empty;

        public BuildAssetBundleOptions BuildAssetBundleOptions;

        /// <summary>
        /// Whether we should clean up all unused asset bundles after building.
        /// </summary>
        public bool CleanUpWorkingDirectoryAfterBuild = true;

        public List<PlatformConfig> PlatformConfigs = new List<PlatformConfig>();
    }
}
