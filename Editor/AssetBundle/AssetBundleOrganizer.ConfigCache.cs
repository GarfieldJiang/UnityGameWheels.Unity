using System.Collections.Generic;

namespace COL.UnityGameWheels.Unity.Editor
{
    public class AssetBundleOrganizerConfigCache
    {
        private AssetBundleOrganizerConfig m_Config = null;

        private Dictionary<string, AssetBundleOrganizerConfig.AssetInfo> m_AssetInfos =
            new Dictionary<string, AssetBundleOrganizerConfig.AssetInfo>();

        private Dictionary<string, AssetBundleOrganizerConfig.AssetBundleInfo> m_AssetBundleInfos =
            new Dictionary<string, AssetBundleOrganizerConfig.AssetBundleInfo>();

        internal AssetBundleOrganizerConfigCache(AssetBundleOrganizerConfig config)
        {
            m_Config = config;
        }

        public void SyncToCache()
        {
            SyncToAssetInfosCache();
            SyncToAssetBundleInfosCache();
        }

        public void SyncFromCache()
        {
            SyncFromAssetInfosCache();
            SyncFromAssetBundleInfosCache();
        }

        private void SyncFromAssetBundleInfosCache()
        {
            var list = new List<AssetBundleOrganizerConfig.AssetBundleInfo>(m_AssetBundleInfos.Values);
            list.Sort((a, b) => (a.AssetBundlePath.CompareTo(b.AssetBundlePath)));
            m_Config.AssetBundleInfos.Clear();
            m_Config.AssetBundleInfos.AddRange(list);
        }

        private void SyncFromAssetInfosCache()
        {
            var list = new List<AssetBundleOrganizerConfig.AssetInfo>(m_AssetInfos.Values);
            list.Sort((a, b) => (a.Guid.CompareTo(b.Guid)));
            m_Config.AssetInfos.Clear();
            m_Config.AssetInfos.AddRange(list);
        }

        private void SyncToAssetBundleInfosCache()
        {
            m_AssetBundleInfos.Clear();
            foreach (var assetBundleInfo in m_Config.AssetBundleInfos)
            {
                m_AssetBundleInfos.Add(assetBundleInfo.AssetBundlePath, assetBundleInfo);
            }
        }

        private void SyncToAssetInfosCache()
        {
            m_AssetInfos.Clear();
            foreach (var assetInfo in m_Config.AssetInfos)
            {
                m_AssetInfos.Add(assetInfo.Guid, assetInfo);
            }
        }

        public List<AssetBundleOrganizerConfig.RootDirectoryInfo> RootDirectoryInfos
        {
            get
            {
                return m_Config.RootDirectoryInfos;
            }
        }

        public IDictionary<string, AssetBundleOrganizerConfig.AssetInfo> AssetInfos
        {
            get
            {
                return m_AssetInfos;
            }
        }

        public IDictionary<string, AssetBundleOrganizerConfig.AssetBundleInfo> AssetBundleInfos
        {
            get
            {
                return m_AssetBundleInfos;
            }
        }
    }
}
