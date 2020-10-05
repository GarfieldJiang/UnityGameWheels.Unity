namespace COL.UnityGameWheels.Unity.Editor
{
    using Asset;
    using Core.Asset;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public partial class AssetBundleBuilder
    {
        private class OutputGeneratorInstaller : OutputGeneratorBase
        {
            private readonly bool m_ConsiderDontPackFlag;

            public OutputGeneratorInstaller(AssetBundleBuilder builder,
                string generatorDirectoryName, bool considerDontPackFlag) : base(builder, generatorDirectoryName)
            {
                m_ConsiderDontPackFlag = considerDontPackFlag;
            }

            protected override void CopyFiles(ResourcePlatform targetPlatform, IList<AssetBundleInfoForIndex> assetBundleInfosForIndex,
                string directoryPath)
            {
                foreach (var assetBundleInfoForIndex in assetBundleInfosForIndex)
                {
                    if (m_ConsiderDontPackFlag && assetBundleInfoForIndex.DontPack)
                    {
                        continue;
                    }

                    CopyAssetBundle(targetPlatform, directoryPath, assetBundleInfoForIndex);
                }
            }

            private void CopyAssetBundle(ResourcePlatform targetPlatform, string directoryPath, AssetBundleInfoForIndex assetBundleInfoForIndex)
            {
                var src = Path.Combine(m_Builder.GetPlatformInternalDirectory(targetPlatform), assetBundleInfoForIndex.Path + AssetBundleSuffix);
                var dst = Core.Utility.Text.Format("{0}{1}", Path.Combine(directoryPath, assetBundleInfoForIndex.Path),
                    Core.Asset.Constant.ResourceFileExtension);
                Directory.CreateDirectory(Path.GetDirectoryName(dst));
                File.Copy(src, dst);
            }

            protected override void GenerateIndex(string bundleVersion, ResourcePlatform targetPlatform, int internalResourceVersion,
                IList<AssetBundleInfoForIndex> assetBundleInfosForIndex,
                IDictionary<string, AssetInfo> assetInfos, string indexPath)
            {
                var assetIndex = new Core.Asset.AssetIndexForInstaller();
                assetIndex.BundleVersion = bundleVersion;
                assetIndex.Platform = targetPlatform.ToString();
                assetIndex.InternalAssetVersion = internalResourceVersion;

                foreach (var abi in assetBundleInfosForIndex)
                {
                    if (!m_ConsiderDontPackFlag || !abi.DontPack)
                    {
                        assetIndex.ResourceInfos.Add(abi.Path, (Core.Asset.ResourceInfo)abi);
                    }

                    assetIndex.ResourceBasicInfos.Add(abi.Path, (Core.Asset.ResourceBasicInfo)abi);
                }

                GenerateResourceGroupInfos(assetIndex.ResourceBasicInfos, assetIndex.ResourceGroupInfos);
                var fullAssetInfos = new Dictionary<string, Core.Asset.AssetInfo>();

                foreach (var originalAssetInfo in assetInfos.Values)
                {
                    var assetInfo = (Core.Asset.AssetInfo)originalAssetInfo;
                    fullAssetInfos.Add(assetInfo.Path, assetInfo);

                    if (!assetIndex.ResourceInfos.ContainsKey(originalAssetInfo.AssetBundlePath))
                    {
                        continue;
                    }

                    assetIndex.AssetInfos.Add(assetInfo.Path, assetInfo);
                }

                GenerateResourceDependencyInfos(fullAssetInfos, assetIndex.ResourceBasicInfos);

                using (var fs = File.Create(indexPath))
                {
                    using (var bw = new BinaryWriter(fs, Encoding.UTF8))
                    {
                        new AssetIndexSerializerV2().ToBinary(bw, assetIndex);
                    }
                }

                File.WriteAllText(indexPath + ".json", JsonConvert.SerializeObject(assetIndex, Formatting.Indented));
            }
        }
    }
}