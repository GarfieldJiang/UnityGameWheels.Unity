﻿using COL.UnityGameWheels.Core;

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
        private class OutputGeneratorRemote : OutputGeneratorBase
        {
            private IZipImpl m_ZipImpl;

            public OutputGeneratorRemote(AssetBundleBuilder builder, string generatorDirectoryName, IZipImpl zipImpl)
                : base(builder, generatorDirectoryName)
            {
                m_ZipImpl = zipImpl;
            }

            protected override void CopyFiles(ResourcePlatform targetPlatform, IList<AssetBundleInfoForIndex> assetBundleInfosForIndex,
                string directoryPath)
            {
                foreach (var assetBundleInfoForIndex in assetBundleInfosForIndex)
                {
                    var src = Path.Combine(m_Builder.GetPlatformInternalDirectory(targetPlatform), assetBundleInfoForIndex.Path + AssetBundleSuffix);
                    var dst = Core.Utility.Text.Format("{0}_{1}{2}", Path.Combine(directoryPath, assetBundleInfoForIndex.Path),
                        assetBundleInfoForIndex.Crc32, Core.Asset.Constant.ResourceFileExtension);
                    Directory.CreateDirectory(Path.GetDirectoryName(dst));
                    File.Copy(src, dst);
                }
            }

            protected override void GenerateIndex(string bundleVersion, ResourcePlatform targetPlatform, int internalResourceVersion,
                IList<AssetBundleInfoForIndex> assetBundleInfosForIndex,
                IDictionary<string, AssetInfo> assetInfos, string indexPath)
            {
                var assetIndex = new Core.Asset.AssetIndexForRemote();
                assetIndex.BundleVersion = bundleVersion;
                assetIndex.Platform = targetPlatform.ToString();
                assetIndex.InternalAssetVersion = internalResourceVersion;

                foreach (var abi in assetBundleInfosForIndex)
                {
                    assetIndex.ResourceInfos.Add(abi.Path, (Core.Asset.ResourceInfo)abi);
                    assetIndex.ResourceBasicInfos.Add(abi.Path, (Core.Asset.ResourceBasicInfo)abi);
                }

                GenerateResourceGroupInfos(assetIndex.ResourceBasicInfos, assetIndex.ResourceGroupInfos);

                foreach (var originalAssetInfo in assetInfos.Values)
                {
                    var assetInfo = (Core.Asset.AssetInfo)originalAssetInfo;
                    assetIndex.AssetInfos.Add(assetInfo.Path, assetInfo);
                }

                GenerateResourceDependencyInfos(assetIndex.AssetInfos, assetIndex.ResourceBasicInfos);

                using (var fs = File.Create(indexPath))
                {
                    using (var bw = new BinaryWriter(fs, Encoding.UTF8))
                    {
                        new AssetIndexSerializerV2().ToBinary(bw, assetIndex);
                    }
                }

                uint crc32;
                using (var fs = File.OpenRead(indexPath))
                {
                    crc32 = Core.Algorithm.Crc32.Sum(fs);
                }

                var newIndexName = Core.Utility.Text.Format(
                    "{0}_{1}{2}", Path.GetFileNameWithoutExtension(IndexFileName), (uint)crc32, Path.GetExtension(IndexFileName));
                var newIndexPath = Path.Combine(Path.GetDirectoryName(indexPath), newIndexName);
                File.Move(indexPath, newIndexPath);
                File.WriteAllText(newIndexPath + ".json", JsonConvert.SerializeObject(assetIndex, Formatting.Indented));

                using (var ms = new MemoryStream())
                {
                    using (var indexFs = File.OpenRead(newIndexPath))
                    {
                        m_ZipImpl.Zip(indexFs, ms);
                    }

                    ms.Seek(0L, SeekOrigin.Begin);
                    var zippedCrc32 = Algorithm.Crc32.Sum(ms);
                    var zippedIndexName = Core.Utility.Text.Format("{0}_{1}{2}",
                        Path.GetFileNameWithoutExtension(IndexFileName), (uint)zippedCrc32,
                        Path.GetExtension(Constant.CachedRemoteIndexZipFileName));
                    var zippedIndexPath = Path.Combine(Path.GetDirectoryName(indexPath), zippedIndexName);
                    ms.Seek(0L, SeekOrigin.Begin);
                    using (var zippedIndexFs = File.OpenWrite(zippedIndexPath))
                    {
                        ms.WriteTo(zippedIndexFs);
                    }
                }
            }
        }
    }
}