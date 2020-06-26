using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using COL.UnityGameWheels.Core;
using UnityEditor;

namespace COL.UnityGameWheels.Unity.Editor
{
    public class AssetBundleRedundancyAnalyser
    {
        private readonly List<UncollectedAssetInfoDataEntry> m_UncollectedAssetInfoDataEntries = new List<UncollectedAssetInfoDataEntry>();
        private readonly AssetBundleInfosProvider m_AssetBundleInfosProvider;

        public class UncollectedAssetInfoDataEntry : IComparable<UncollectedAssetInfoDataEntry>
        {
            public UncollectedAssetInfo UncollectedAssetInfo { get; internal set; }
            public IList<string> GetAssetBundlePaths() => new ReadOnlyCollection<string>(AssetBundlePaths);
            public int AssetBundleCount => AssetBundlePaths.Count;
            public IList<AssetInfo> GetAssetInfos() => new ReadOnlyCollection<AssetInfo>(AssetInfos);

            internal readonly List<string> AssetBundlePaths = new List<string>();
            internal readonly List<AssetInfo> AssetInfos = new List<AssetInfo>();

            public int CompareTo(UncollectedAssetInfoDataEntry other)
            {
                return string.CompareOrdinal(UncollectedAssetInfo.AssetPath, other.UncollectedAssetInfo.AssetPath);
            }
        }

        public IList<UncollectedAssetInfoDataEntry> GetDataEntries() =>
            new ReadOnlyCollection<UncollectedAssetInfoDataEntry>(m_UncollectedAssetInfoDataEntries);

        public AssetBundleRedundancyAnalyser(AssetBundleInfosProvider assetBundleInfosProvider)
        {
            Guard.RequireNotNull<ArgumentNullException>(assetBundleInfosProvider, $"Invalid '{nameof(assetBundleInfosProvider)}'.");
            m_AssetBundleInfosProvider = assetBundleInfosProvider;
        }

        public void Refresh()
        {
            m_UncollectedAssetInfoDataEntries.Clear();
            foreach (var uncollectedAssetInfo in m_AssetBundleInfosProvider.UncollectedAssetInfos.Values)
            {
                var dataEntry = new UncollectedAssetInfoDataEntry
                {
                    UncollectedAssetInfo = uncollectedAssetInfo,
                };
                var assetInfos = m_AssetBundleInfosProvider.AssetInfos;
                dataEntry.AssetBundlePaths.AddRange(uncollectedAssetInfo.GetAssetGuidsDependingOnThis()
                    .Select(guid => assetInfos[guid].AssetBundlePath)
                    .Distinct());
                dataEntry.AssetBundlePaths.Sort(string.CompareOrdinal);
                foreach (var dependingAssetGuid in uncollectedAssetInfo.GetAssetGuidsDependingOnThis())
                {
                    dataEntry.AssetInfos.Add(assetInfos[dependingAssetGuid]);
                }

                m_UncollectedAssetInfoDataEntries.Add(dataEntry);
            }

            m_UncollectedAssetInfoDataEntries.Sort();
        }
    }
}