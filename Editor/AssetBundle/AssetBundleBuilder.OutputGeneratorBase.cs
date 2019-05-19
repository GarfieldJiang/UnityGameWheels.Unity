namespace COL.UnityGameWheels.Unity.Editor
{
    using Asset;
    using Core.Asset;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public partial class AssetBundleBuilder
    {
        private abstract class OutputGeneratorBase
        {
            protected readonly AssetBundleBuilder m_Builder = null;

            protected abstract int IndexVersion { get; }

            private string GeneratorDirectoryName { get; }

            public OutputGeneratorBase(AssetBundleBuilder builder, string generatorDirectoryName)
            {
                m_Builder = builder;
                GeneratorDirectoryName = generatorDirectoryName;
            }

            private string CalculateDirectoryPath(string bundleVersion, ResourcePlatform targetPlatform, int internalResourceVersion)
            {
                var ret = Path.Combine(m_Builder.OutputDirectory, targetPlatform.ToString());

                var bundleVersionSB = Core.StringBuilderCache.Acquire();
                foreach (var ch in bundleVersion)
                {
                    bundleVersionSB.Append(Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch);
                }

                ret = Path.Combine(ret, Core.Utility.Text.Format("{0}.{1}",
                    Core.StringBuilderCache.GetStringAndRelease(bundleVersionSB),
                    internalResourceVersion));

                ret = Path.Combine(ret, GeneratorDirectoryName);

                return ret;
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
                var directoryPath = CalculateDirectoryPath(bundleVersion, targetPlatform, internalResourceVersion);

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

                        resourceBasicInfos[depResPath].DependingResourcePaths.Add(resPath);
                    }
                }
            }
        }
    }
}