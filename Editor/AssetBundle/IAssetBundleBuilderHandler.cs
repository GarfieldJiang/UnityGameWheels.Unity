using COL.UnityGameWheels.Unity.Asset;
using UnityEditor;

namespace COL.UnityGameWheels.Unity.Editor
{
    public interface IAssetBundleBuilderHandler
    {
        void OnPreBeforeBuild();

        void OnPostBeforeBuild(AssetBundleBuild[] assetBundleBuilds);

        void OnPreBuildPlatform(ResourcePlatform targetPlatform);

        void OnPostBuildPlatform(ResourcePlatform targetPlatform, string outputDirectory);

        void OnBuildSuccess();

        void OnBuildFailure();
    }
}