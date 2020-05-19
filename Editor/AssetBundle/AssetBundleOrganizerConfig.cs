using System.Collections.Generic;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizerConfig
    {
        public List<RootDirectoryInfo> RootDirectoryInfos { get; } = new List<RootDirectoryInfo>();

        public List<AssetInfo> AssetInfos { get; } = new List<AssetInfo>();

        public List<AssetBundleInfo> AssetBundleInfos { get; } = new List<AssetBundleInfo>();
    }
}