using UnityEngine;

namespace COL.UnityGameWheels.Unity.Asset
{
    public class AssetServiceConfig : ScriptableObject
    {
#pragma warning disable 414
        [SerializeField]
        private bool m_EditorMode = false;
#pragma warning restore 414

        public bool EditorMode
        {
            get
            {
#if UNITY_EDITOR
                return m_EditorMode;
#else
                return false;
#endif
            }
        }

        public bool UpdateStandaloneResourcesInEditor = true;

        public bool UpdateIsEnabled = true;

        public int DownloadRetryCount = 2;

        public int ConcurrentAssetLoaderCount = 16;

        public int ConcurrentResourceLoaderCount = 4;

        public int AssetCachePoolCapacity = 1024;

        public int ResourceCachePoolCapacity = 128;

        public int AssetAccessorPoolCapacity = 1024;

        [Tooltip("{0} is the platform name; {1} is the resource version.")]
        public string UpdateRelativePathFormat = "{0}/{1}";

        public string ReadWriteRelativePath = string.Empty;

        public string InstallerRelativePath = string.Empty;

        public string[] UpdateServerRootUrls = null;

        public float ReleaseResourceInterval = 3f;

        [Tooltip("Bytes to update before triggering a saving operation of the read-write index.")]
        public int UpdateSizeBeforeSavingReadWriteIndex = 1024 * 1024;
    }
}