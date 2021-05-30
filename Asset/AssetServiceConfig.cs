using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using COL.UnityGameWheels.Core.Asset;

namespace COL.UnityGameWheels.Unity.Asset
{
    public class AssetServiceConfig : ScriptableObject, IAssetServiceConfigReader
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

        [SerializeField]
        private bool m_UpdateIsEnabled = true;

        public string RunningPlatform => GetRunningPlatform(this);

        public bool UpdateIsEnabled => m_UpdateIsEnabled;

        [SerializeField]
        private int m_DownloadRetryCount = 2;

        public int DownloadRetryCount => m_DownloadRetryCount;

        [SerializeField]
        private int m_ConcurrentAssetLoaderCount = 16;

        public int ConcurrentAssetLoaderCount => m_ConcurrentAssetLoaderCount;

        [SerializeField]
        private int m_ConcurrentResourceLoaderCount = 4;

        public int ConcurrentResourceLoaderCount => m_ConcurrentResourceLoaderCount;

        [SerializeField]
        private int m_AssetCachePoolCapacity = 1024;

        public int AssetCachePoolCapacity => m_AssetCachePoolCapacity;

        [SerializeField]
        public int m_ResourceCachePoolCapacity = 128;

        public int ResourceCachePoolCapacity => m_ResourceCachePoolCapacity;

        [SerializeField]
        private int m_AssetAccessorPoolCapacity = 1024;

        public int AssetAccessorPoolCapacity => m_AssetAccessorPoolCapacity;

        [SerializeField]
        [Tooltip("{0} is the platform name; {1} is the resource version.")]
        private string m_UpdateRelativePathFormat = "{0}/{1}";

        public string UpdateRelativePathFormat => m_UpdateRelativePathFormat;

        [SerializeField]
        private string m_ReadWriteRelativePath = "Resources";

        public string ReadWritePath => Path.Combine(Application.persistentDataPath, m_ReadWriteRelativePath);

        [SerializeField]
        private string m_InstallerRelativePath = string.Empty;

        public string InstallerPath => Path.Combine(Application.streamingAssetsPath, m_InstallerRelativePath);

        [SerializeField]
        public string[] m_UpdateServerRootUrls = null;

        public IEnumerable<string> UpdateServerRootUrls => m_UpdateServerRootUrls;

        [SerializeField]
        private float m_ReleaseResourceInterval = 3f;

        public float ReleaseResourceInterval => m_ReleaseResourceInterval;

        [SerializeField]
        [Tooltip("Bytes to update before triggering a saving operation of the read-write index.")]
        private int m_UpdateSizeBeforeSavingReadWriteIndex = 1024 * 1024;

        public int UpdateSizeBeforeSavingReadWriteIndex => m_UpdateSizeBeforeSavingReadWriteIndex;


        private static string GetRunningPlatform(AssetServiceConfig config)
        {
#if UNITY_EDITOR
            if (config.UpdateStandaloneResourcesInEditor)
            {
                return ResourcePlatform.Standalone.ToString();
            }

            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return ResourcePlatform.Standalone.ToString();
                case BuildTarget.iOS:
                    return ResourcePlatform.iOS.ToString();
                case BuildTarget.Android:
                    return ResourcePlatform.Android.ToString();

                default:
                    throw new InvalidOperationException(
                        Core.Utility.Text.Format("Unsupported build target '{0}'.", EditorUserBuildSettings.activeBuildTarget));
            }

#else
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.OSXPlayer:
                    return ResourcePlatform.Standalone.ToString();
                case RuntimePlatform.IPhonePlayer:
                    return ResourcePlatform.iOS.ToString();
                case RuntimePlatform.Android:
                    return ResourcePlatform.Android.ToString();
                default:
                    throw new InvalidOperationException(
                        Core.Utility.Text.Format("Unsupported runtime platform '{0}'.", Application.platform));
            }
#endif
        }
    }
}