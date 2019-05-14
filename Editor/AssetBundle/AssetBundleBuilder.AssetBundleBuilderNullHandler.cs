using COL.UnityGameWheels.Unity.Asset;
using UnityEditor;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleBuilder
    {
        private class AssetBundleBuilderNullHandler : IAssetBundleBuilderHandler
        {
            public void OnPreBeforeBuild()
            {
            }

            public void OnPostBeforeBuild(AssetBundleBuild[] assetBundleBuilds)
            {
            }

            public void OnPreBuildPlatform(ResourcePlatform targetPlatform)
            {
            }

            public void OnPostBuildPlatform(ResourcePlatform targetPlatform, string outputDirectory)
            {
            }

            public void OnBuildSuccess()
            {
            }

            public void OnBuildFailure()
            {
            }
        }
    }
}