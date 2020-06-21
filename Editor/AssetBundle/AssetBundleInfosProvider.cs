using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleInfosProvider
    {
        private readonly AssetBundleOrganizer m_Organizer = null;

        private readonly Dictionary<string, AssetBundleInfo> m_AssetBundleInfos =
            new Dictionary<string, AssetBundleInfo>();

        public IDictionary<string, AssetBundleInfo> AssetBundleInfos => m_AssetBundleInfos;

        private readonly List<KeyValuePair<AssetInfo, AssetInfo>> m_IllegalGroupDependencies =
            new List<KeyValuePair<AssetInfo, AssetInfo>>();

        public IList<KeyValuePair<AssetInfo, AssetInfo>> IllegalGroupDependencies => m_IllegalGroupDependencies.AsReadOnly();

        private readonly List<IList<AssetInfo>> m_CycleAssetDependencies = new List<IList<AssetInfo>>();

        public IList<IList<AssetInfo>> CycleAssetDependencies => m_CycleAssetDependencies;

        private readonly Dictionary<string, AssetInfo> m_AssetInfos = new Dictionary<string, AssetInfo>();

        public IDictionary<string, AssetInfo> AssetInfos => m_AssetInfos;

        private readonly Dictionary<string, UncollectedAssetInfo> m_UncollectedAssetInfos = new Dictionary<string, UncollectedAssetInfo>();

        public IReadOnlyDictionary<string, UncollectedAssetInfo> UncollectedAssetInfos =>
            new ReadOnlyDictionary<string, UncollectedAssetInfo>(m_UncollectedAssetInfos);


        public AssetBundleInfosProvider(AssetBundleOrganizer organizer)
        {
            m_Organizer = organizer ?? throw new ArgumentNullException(nameof(organizer));
        }

        public void PopulateData()
        {
            m_AssetBundleInfos.Clear();
            m_AssetInfos.Clear();
            m_UncollectedAssetInfos.Clear();
            m_IllegalGroupDependencies.Clear();
            PopulateAssetBundleInfos();
            PopulateAssetInfos();
            RemoveEmptyAssetBundleInfos();
        }

        public void CheckIllegalGroupDependencies()
        {
            m_IllegalGroupDependencies.Clear();
            foreach (var assetInfo in m_AssetInfos.Values)
            {
                var abGroup = m_AssetBundleInfos[assetInfo.AssetBundlePath].GroupId;
                foreach (var guid in assetInfo.DependencyAssetGuids)
                {
                    var dependencyAssetInfo = m_AssetInfos[guid];
                    var dependencyABGroup = m_AssetBundleInfos[dependencyAssetInfo.AssetBundlePath].GroupId;
                    if (abGroup == Core.Asset.Constant.CommonResourceGroupId && dependencyABGroup != Core.Asset.Constant.CommonResourceGroupId ||
                        abGroup != Core.Asset.Constant.CommonResourceGroupId && dependencyABGroup != abGroup &&
                        dependencyABGroup != Core.Asset.Constant.CommonResourceGroupId)
                    {
                        m_IllegalGroupDependencies.Add(new KeyValuePair<AssetInfo, AssetInfo>(assetInfo, dependencyAssetInfo));
                    }
                }
            }
        }

        public void CheckCycleAssetDependencies()
        {
            m_CycleAssetDependencies.Clear();
            var sccs = Core.Algorithm.Graph.TarjanScc(m_AssetInfos,
                (x, y) => x == y,
                assetInfo => assetInfo.DependencyAssetGuids,
                false);

            foreach (var scc in sccs)
            {
                m_CycleAssetDependencies.Add(scc.ToList().ConvertAll(guid => m_AssetInfos[guid]));
            }
        }

        private void PopulateAssetInfos()
        {
            foreach (var assetInfo in m_AssetInfos.Values)
            {
                var rawDeps = AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(assetInfo.Guid), false)
                    .Select(AssetDatabase.AssetPathToGUID)
                    .Where(guid => guid != assetInfo.Guid);
                var guidQueue = new Queue<string>(rawDeps);
                var guidSet = new HashSet<string>(guidQueue);

                while (guidQueue.Count > 0)
                {
                    var guid = guidQueue.Dequeue();
                    if (m_AssetInfos.ContainsKey(guid))
                    {
                        assetInfo.DependencyAssetGuids.Add(guid);
                        continue;
                    }

                    UpdateUncollectedAssets(guid, assetInfo);

                    var deps = AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(guid), false)
                        .Where(path => !path.ToLower().EndsWith(".unity"))
                        .Select(AssetDatabase.AssetPathToGUID);
                    foreach (var dep in deps)
                    {
                        if (guidSet.Contains(dep))
                        {
                            continue;
                        }

                        guidSet.Add(dep);
                        guidQueue.Enqueue(dep);
                    }
                }
            }
        }

        private void UpdateUncollectedAssets(string assetGuid, AssetInfo dependingAssetInfo)
        {
            if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GUIDToAssetPath(assetGuid)) == typeof(MonoScript))
            {
                return;
            }

            if (!m_UncollectedAssetInfos.TryGetValue(assetGuid, out var uncollectedAssetInfo))
            {
                uncollectedAssetInfo = new UncollectedAssetInfo {Guid = assetGuid};
                m_UncollectedAssetInfos.Add(assetGuid, uncollectedAssetInfo);
            }

            uncollectedAssetInfo.AssetGuidsDependingOnThis.Add(dependingAssetInfo.Guid);
        }

        private AssetBundleInfo PopulateSingleAssetBundleInfo(AssetBundleOrganizerConfig.AssetBundleInfo abInfo)
        {
            var ret = new AssetBundleInfo {Path = abInfo.AssetBundlePath, GroupId = abInfo.AssetBundleGroup, DontPack = abInfo.DontPack};
            var guidQueue = new Queue<string>(abInfo.AssetGuids);
            var guidSet = new HashSet<string>(guidQueue);
            //Debug.LogFormat("[AssetBundleDependencyChecker PopulateSingleAssetBundleInfo] ab path: {0}", abInfo.AssetBundlePath);
            while (guidQueue.Count > 0)
            {
                var assetGuid = guidQueue.Dequeue();
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                var organizerAssetInfo = m_Organizer.GetAssetInfo(assetGuid);
                if (organizerAssetInfo == null)
                {
                    Debug.LogWarningFormat("In asset bundle '{0}', asset '{1}' at path '{2}' seems not included by any root directory.",
                        abInfo.AssetBundlePath, assetGuid, assetPath);
                    continue;
                }

                // Asset belongs to another asset bundle.
                if (!string.IsNullOrEmpty(organizerAssetInfo.AssetBundlePath) &&
                    organizerAssetInfo.AssetBundlePath != abInfo.AssetBundlePath)
                {
                    continue;
                }

                if (File.Exists(assetPath))
                {
                    if (m_AssetInfos.ContainsKey(assetGuid))
                    {
                        throw new InvalidOperationException(
                            $"Asset '{assetPath}' already assigned to asset bundle '{m_AssetInfos[assetGuid].AssetBundlePath}'. Now trying to add it into '{ret.Path}'.");
                    }

                    ret.AssetGuids.Add(assetGuid);
                    var assetInfo = new AssetInfo();
                    assetInfo.Guid = assetGuid;
                    assetInfo.AssetBundlePath = ret.Path;
                    m_AssetInfos.Add(assetGuid, assetInfo);
                    //Debug.LogFormat("[AssetBundleDependencyChecker PopulateSingleAssetBundleInfo] asset path: {0}", assetPath);
                }
                else if (Directory.Exists(assetPath))
                {
                    foreach (var childAssetInfo in organizerAssetInfo.Children.Values)
                    {
                        if (guidSet.Add(childAssetInfo.Guid))
                        {
                            guidQueue.Enqueue(childAssetInfo.Guid);
                        }
                    }
                }
            }

            return ret;
        }

        private void PopulateAssetBundleInfos()
        {
            foreach (var rawABInfo in m_Organizer.ConfigCache.AssetBundleInfos.Values)
            {
                //Debug.Log("rawABInfo: " + rawABInfo.AssetBundlePath);
                var abInfo = PopulateSingleAssetBundleInfo(rawABInfo);
                m_AssetBundleInfos.Add(abInfo.Path, abInfo);
            }
        }

        private void RemoveEmptyAssetBundleInfos()
        {
            var keysToRemove = new List<string>();
            foreach (var kv in m_AssetBundleInfos)
            {
                if (kv.Value.AssetGuids.Count <= 0)
                {
                    keysToRemove.Add(kv.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                m_AssetBundleInfos.Remove(key);
            }
        }
    }
}