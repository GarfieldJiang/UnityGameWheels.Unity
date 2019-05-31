using COL.UnityGameWheels.Unity.Asset;
using UnityEditor;

namespace COL.UnityGameWheels.Unity.Editor
{
    public interface IAssetBundleBuilderHandler
    {
        void OnPreBeforeBuild();

        void OnPostBeforeBuild(AssetBundleBuild[] assetBundleBuilds);

        void OnPreBuildPlatform(ResourcePlatform targetPlatform, int internalResourceVersion);

        void OnPostBuildPlatform(ResourcePlatform targetPlatform, int internalResourceVersion, string outputDirectory);

        void OnBuildSuccess();

        void OnBuildFailure();
    }
}