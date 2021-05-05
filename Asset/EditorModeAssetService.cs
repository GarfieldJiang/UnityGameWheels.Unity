#if UNITY_EDITOR
using COL.UnityGameWheels.Core;
using COL.UnityGameWheels.Core.Asset;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace COL.UnityGameWheels.Unity.Asset
{
    internal partial class EditorModeAssetService : TickableLifeCycleService, IAssetService
    {
        public int ConcurrentAssetLoaderCount { get; set; }

        public int ConcurrentResourceLoaderCount { get; set; }

        public int AssetCachePoolCapacity { get; set; }

        public int ResourceCachePoolCapacity { get; set; }

        public int AssetAccessorPoolCapacity { get; set; }

        public int DownloadRetryCount { get; set; }

        public float ReleaseResourceInterval { get; set; }

        public IAssetIndexForInstallerLoader IndexForInstallerLoader { get; set; }

        public bool UpdateIsEnabled { get; set; }

        public string UpdateRelativePathFormat { get; set; }

        public string BundleVersion { get; set; }

        public string ReadWritePath { get; set; }

        public string InstallerPath { get; set; }

        public string RunningPlatform { get; set; }

        public int UpdateSizeBeforeSavingReadWriteIndex { get; set; }

        private readonly IResourceUpdater m_ResourceUpdater = null;

        public IResourceUpdater ResourceUpdater => m_ResourceUpdater ?? new DummyResourceUpdater();

        public void AddUpdateServerRootUrl(string updateServerRootUrl)
        {
            // Empty.
        }

        public void CheckUpdate(AssetIndexRemoteFileInfo remoteIndexFileInfo, UpdateCheckCallbackSet callbackSet, object context)
        {
            callbackSet.OnSuccess?.Invoke(context);
        }

        public void RequestUnloadUnusedResources()
        {
            // Empty.
        }

        public IDictionary<string, AssetCacheQuery> GetAssetCacheQueries()
        {
            throw new NotSupportedException();
        }

        public IDictionary<string, ResourceCacheQuery> GetResourceCacheQueries()
        {
            throw new NotSupportedException();
        }

        public IAssetAccessor LoadAsset(string assetPath, LoadAssetCallbackSet callbackSet, object context)
        {
            var assetObj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
            var ret = new DummyAssetAccessor { AssetPath = assetPath, AssetObject = assetObj };

            if (assetObj == null)
            {
                var errorMessage = $"Failed to load asset at path '{assetPath}'.";
                ret.Status = AssetAccessorStatus.Failure;
                if (callbackSet.OnFailure != null)
                {
                    callbackSet.OnFailure(ret, errorMessage, context);
                }
                else
                {
                    throw new InvalidOperationException(errorMessage);
                }
            }
            else
            {
                ret.Status = AssetAccessorStatus.Ready;
                callbackSet.OnSuccess?.Invoke(ret, context);
            }

            return ret;
        }

        public IAssetAccessor LoadSceneAsset(string sceneAssetPath, LoadAssetCallbackSet callbackSet, object context)
        {
            var assetObj = AssetDatabase.LoadAssetAtPath(sceneAssetPath, typeof(UnityEngine.Object));
            var ret = new DummyAssetAccessor { AssetPath = sceneAssetPath, AssetObject = assetObj };
            if (assetObj == null)
            {
                string errorMessage = $"Fail to load scene '{sceneAssetPath}'.";
                if (callbackSet.OnFailure != null)
                {
                    callbackSet.OnFailure(ret, errorMessage, context);
                }
                else
                {
                    throw new InvalidOperationException(errorMessage);
                }
            }
            else
            {
                callbackSet.OnSuccess?.Invoke(ret, context);
            }

            return ret;
        }

        public int GetAssetResourceGroupId(string assetPath)
        {
            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(assetPath)))
            {
                return Core.Asset.Constant.InvalidResourceGroupId;
            }

            return Core.Asset.Constant.CommonResourceGroupId;
        }

        public bool IsLoadingAnyAsset => false;

        private bool m_IsPreparing = false;
        private AssetServicePrepareCallbackSet m_PrepareCallbackSet;
        private object m_PrepareContext = null;

        public void Prepare(AssetServicePrepareCallbackSet callbackSet, object context)
        {
            m_IsPreparing = true;
            m_PrepareCallbackSet = callbackSet;
            m_PrepareContext = context;
        }

        public void UnloadAsset(IAssetAccessor assetAccessor)
        {
            var dummyAssetAccessor = (DummyAssetAccessor)assetAccessor;
            dummyAssetAccessor.AssetObject = null;
            dummyAssetAccessor.AssetPath = null;
            dummyAssetAccessor.Status = AssetAccessorStatus.None;
        }

        protected override void OnUpdate(TimeStruct timeStruct)
        {
            if (!m_IsPreparing)
            {
                return;
            }

            m_IsPreparing = false;
            m_PrepareCallbackSet.OnSuccess?.Invoke(m_PrepareContext);
            m_PrepareCallbackSet = default;
            m_PrepareContext = null;
        }
    }
}

#endif