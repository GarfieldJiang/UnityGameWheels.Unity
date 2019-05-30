namespace COL.UnityGameWheels.Unity.Asset
{
    using Core;
    using Core.Asset;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class AssetManager : MonoBehaviourEx, IAssetManager
    {
#pragma warning disable 414
        [SerializeField]
        private bool m_EditorMode = false;

        [SerializeField]
        private bool m_UpdateStandaloneResourcesInEditor = true;
#pragma warning restore 414

        [SerializeField]
        private bool m_UpdateIsEnabled = true;

        [SerializeField]
        private int m_DownloadRetryCount = 2;

        [SerializeField]
        private int m_ConcurrentAssetLoaderCount = 16;

        [SerializeField]
        private int m_ConcurrentResourceLoaderCount = 4;

        [SerializeField]
        private int m_AssetCachePoolCapcity = 1024;

        [SerializeField]
        private int m_ResourceCachePoolCapacity = 128;

        [SerializeField]
        private int m_AssetAccessorPoolCapacity = 1024;

        [SerializeField, Tooltip("{0} is the platform name; {1} is the resource version.")]
        private string m_UpdateRelativePathFormat = "{0}/{1}";

        [SerializeField]
        private string m_ReadWriteRelativePath = string.Empty;

        [SerializeField]
        private string m_InstallerRelativePath = string.Empty;

        [SerializeField]
        private string[] m_UpdateServerRootUrls = null;

        [SerializeField]
        private float m_ReleaseResourceInterval = 3f;

        [SerializeField]
        private float m_InspectorQueryRefreshInterval = .5f;

        [SerializeField, Tooltip("Bytes to update before triggering a saving operation of the read-write index.")]
        private int m_UpdateSizeBeforeSavingReadWriteIndex = 1024 * 1024;

        public float InspectorQueryRefreshInterval
        {
            get { return m_InspectorQueryRefreshInterval; }
        }

        public IAssetModule Module { get; private set; }

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

            set { m_EditorMode = value; }
        }

        public IDownloadModule DownloadModule
        {
            get { return Module.DownloadModule; }
            set { Module.DownloadModule = value; }
        }

        public IRefPoolModule RefPoolModule
        {
            get { return Module.RefPoolModule; }
            set { Module.RefPoolModule = value; }
        }

        public IResourceUpdater ResourceUpdater
        {
            get { return Module.ResourceUpdater; }
        }

        public void CheckUpdate(AssetIndexRemoteFileInfo remoteIndexFileInfo, UpdateCheckCallbackSet callbackSet, object context)
        {
            Module.CheckUpdate(remoteIndexFileInfo, callbackSet, context);
        }

        public void Init()
        {
            Module.Init();
        }

        public void Prepare(AssetManagerPrepareCallbackSet callbackSet, object context)
        {
            var internalCallbackSet = new AssetModulePrepareCallbackSet();
            internalCallbackSet.OnSuccess = theContext =>
            {
                if (callbackSet.OnSuccess != null)
                {
                    callbackSet.OnSuccess(theContext);
                }
            };

            internalCallbackSet.OnFailure = (errorMessage, theContext) =>
            {
                if (callbackSet.OnFailure != null)
                {
                    callbackSet.OnFailure(errorMessage, theContext);
                }
            };

            Module.Prepare(internalCallbackSet, context);
        }

        public void ShutDown()
        {
            Module.ShutDown();
        }

        protected override void Awake()
        {
            base.Awake();
            CreateModule();
            Module.UpdateIsEnabled = m_UpdateIsEnabled;
            Module.DownloadRetryCount = m_DownloadRetryCount;
            Module.ConcurrentAssetLoaderCount = m_ConcurrentAssetLoaderCount;
            Module.ConcurrentResourceLoaderCount = m_ConcurrentResourceLoaderCount;
            Module.AssetCachePoolCapacity = m_AssetCachePoolCapcity;
            Module.ResourceCachePoolCapacity = m_ResourceCachePoolCapacity;
            Module.AssetAccessorPoolCapacity = m_AssetAccessorPoolCapacity;
            Module.AssetLoadingTaskImplFactory = new AssetLoadingTaskImplFactory();
            Module.ResourceLoadingTaskImplFactory = new ResourceLoadingTaskImplFactory();
            Module.ResourceDestroyer = new ResourceDestroyer();
            Module.UpdateRelativePathFormat = m_UpdateRelativePathFormat;
            Module.ReadWritePath = Path.Combine(Application.persistentDataPath, m_ReadWriteRelativePath);
            Module.InstallerPath = Path.Combine(Application.streamingAssetsPath, m_InstallerRelativePath);
            Module.IndexForInstallerLoader = new AssetIndexForInstallerLoader(this);
            Module.BundleVersion = Application.version;
            Module.ReleaseResourceInterval = m_ReleaseResourceInterval;
            Module.UpdateSizeBeforeSavingReadWriteIndex = m_UpdateSizeBeforeSavingReadWriteIndex;

            SetRunningPlatform();

            foreach (var url in m_UpdateServerRootUrls)
            {
                Module.AddUpdateServerRootUrl(url);
            }
        }

        private void SetRunningPlatform()
        {
#if UNITY_EDITOR
            if (m_UpdateStandaloneResourcesInEditor)
            {
                Module.RunningPlatform = ResourcePlatform.Standalone.ToString();
            }
            else
            {
                switch (EditorUserBuildSettings.activeBuildTarget)
                {
                    case BuildTarget.StandaloneLinux:
                    case BuildTarget.StandaloneLinux64:
                    case BuildTarget.StandaloneLinuxUniversal:
                    case BuildTarget.StandaloneOSX:
                    case BuildTarget.StandaloneWindows:
                    case BuildTarget.StandaloneWindows64:
                        Module.RunningPlatform = ResourcePlatform.Standalone.ToString();
                        break;
                    case BuildTarget.iOS:
                        Module.RunningPlatform = ResourcePlatform.iOS.ToString();
                        break;
                    case BuildTarget.Android:
                        Module.RunningPlatform = ResourcePlatform.Android.ToString();
                        break;
                    default:
                        throw new InvalidOperationException(
                            Utility.Text.Format("Unsupported build target '{0}'.", EditorUserBuildSettings.activeBuildTarget));
                }
            }
#else
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.OSXPlayer:
                    Module.RunningPlatform = ResourcePlatform.Standalone.ToString();
                    break;
                case RuntimePlatform.IPhonePlayer:
                    Module.RunningPlatform = ResourcePlatform.iOS.ToString();
                    break;
                case RuntimePlatform.Android:
                    Module.RunningPlatform = ResourcePlatform.Android.ToString();
                    break;
                default:
                    throw new InvalidOperationException(
                        Core.Utility.Text.Format("Unsupported runtime platform '{0}'.", Application.platform));
            }
#endif
        }

        private void CreateModule()
        {
#if UNITY_EDITOR
            if (m_EditorMode)
            {
                var editorModule = new EditorModeAssetModule(this);
                Module = editorModule;
            }
            else
            {
                Module = new AssetModule();
            }
#else
            Module = new AssetModule();
#endif
        }

        private void Update()
        {
            Module.Update(Unity.Utility.Time.GetTimeStruct());
        }

        public IAssetAccessor LoadAsset(string assetPath, LoadAssetCallbackSet callbackSet, object context)
        {
            return Module.LoadAsset(assetPath, callbackSet, context);
        }

        public IAssetAccessor LoadSceneAsset(string sceneAssetPath, LoadAssetCallbackSet callbackSet, object context)
        {
            return Module.LoadSceneAsset(sceneAssetPath, callbackSet, context);
        }

        public void UnloadAsset(IAssetAccessor assetAccessor)
        {
            Module.UnloadAsset(assetAccessor);
        }

        public bool IsLoadingAnyAsset => Module?.IsLoadingAnyAsset ?? false;

        public void RequestUnloadUnusedAssetBundles()
        {
            Module.RequestUnloadUnusedResources();
        }

        public void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
        }

        public IDictionary<string, AssetCacheQuery> GetAssetCacheQueries()
        {
            return Module.GetAssetCacheQueries();
        }

        public IDictionary<string, ResourceCacheQuery> GetResourceCacheQueries()
        {
            return Module.GetResourceCacheQueries();
        }
    }
}