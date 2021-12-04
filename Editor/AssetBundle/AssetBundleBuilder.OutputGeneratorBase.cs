namespace COL.UnityGameWheels.Unity.Editor
{
    using Asset;
    using Core.Asset;
    using System.Collections.Generic;
    using System.IO;

    public partial class AssetBundleBuilder
    {
        private abstract class OutputGeneratorBase
        {
            protected readonly AssetBundleBuilder m_Builder = null;

            private string GeneratorDirectoryName { get; }

            public OutputGeneratorBase(AssetBundleBuilder builder, string generatorDirectoryName)
            {
                m_Builder = builder;
                GeneratorDirectoryName = generatorDirectoryName;
            }

            protected abstract void GenerateIndex(string bundleVersion, ResourcePlatform targetPlatform, int internalResourceVersion,
                IList<AssetBundleInfoForIndex> assetBundleInfosForIndex,
                IDictionary<string, AssetInfo> assetInfos, string indexPath);

            protected abstract void CopyFiles(ResourcePlatform targetPlatform, IList<AssetBundleInfoForIndex> assetBundleInfosForIndex,
                string directoryPath);

            public void Run(string bundleVersion, ResourcePlatform targetPlatform, int internalResourceVersion,
                IList<AssetBundleInfoForIndex> assetBundleInfosForIndex,
                IDictionary<string, AssetInfo> assetInfos)
            {
                var directoryPath = Path.Combine(m_Builder.GetOutputDirectory(targetPlatform, bundleVersion, internalResourceVersion), GeneratorDirectoryName);

                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }

                Directory.CreateDirectory(directoryPath);
                string indexPath = Path.Combine(directoryPath, IndexFileName);
                GenerateIndex(bundleVersion, targetPlatform, internalResourceVersion,
                    assetBundleInfosForIndex, assetInfos, indexPath);
                CopyFiles(targetPlatform, assetBundleInfosForIndex, directoryPath);
            }

            protected static void GenerateResourceGroupInfos(IDictionary<string, ResourceBasicInfo> resourceBasicInfos,
                IList<ResourceGroupInfo> resourceGroupInfos)
            {
                resourceGroupInfos.Clear();
                List<ResourceGroupInfo> internalResourceGroupInfos = new List<ResourceGroupInfo>();
                foreach (var resourceInfo in resourceBasicInfos.Values)
                {
                    int index = internalResourceGroupInfos.FindIndex(resourceGroupInfo => resourceGroupInfo.GroupId == resourceInfo.GroupId);
                    if (index < 0)
                    {
                        internalResourceGroupInfos.Add(new ResourceGroupInfo {GroupId = resourceInfo.GroupId});
                        index = internalResourceGroupInfos.Count - 1;
                    }

                    internalResourceGroupInfos[index].ResourcePaths.Add(resourceInfo.Path);
                }

                internalResourceGroupInfos.Sort((x, y) => x.GroupId.CompareTo(y.GroupId));
                foreach (var resourceGroupInfo in internalResourceGroupInfos)
                {
                    resourceGroupInfos.Add(resourceGroupInfo);
                }
            }

            protected static void GenerateResourceDependencyInfos(IDictionary<string, Core.Asset.AssetInfo> assetInfos,
                IDictionary<string, ResourceBasicInfo> resourceBasicInfos)
            {
                foreach (var assetInfo in assetInfos.Values)
                {
                    var resPath = assetInfo.ResourcePath;
                    var resBasicInfo = resourceBasicInfos[resPath];
                    foreach (var depAssetPath in assetInfo.DependencyAssetPaths)
                    {
                        if (!assetInfos.ContainsKey(depAssetPath))
                        {
                            throw new KeyNotFoundException($"'{assetInfo.Path} depends on '{depAssetPath}' but the latter cannot be found.");
                        }

                        var depResPath = assetInfos[depAssetPath].ResourcePath;
                        if (resPath == depResPath)
                        {
                            continue;
                        }

                        resBasicInfo.DependencyResourcePaths.Add(depResPath);
                    }
                }
            }
        }
    }
}
