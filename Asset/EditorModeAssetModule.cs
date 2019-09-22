#if UNITY_EDITOR

namespace COL.UnityGameWheels.Unity.Asset
{
    using Core;
    using Core.Asset;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    internal partial class EditorModeAssetModule : IAssetModule
    {
        private AssetManager m_AssetManager;

        public EditorModeAssetModule(AssetManager assetManager)
        {
            m_AssetManager = assetManager;
        }

        private MonoBehaviour MonoBehaviour
        {
            get { return m_AssetManager; }
        }

        public IDownloadModule DownloadModule { get; set; }

        public IRefPoolModule RefPoolModule { get; set; }

        public ISimpleFactory<IAssetLoadingTaskImpl> AssetLoadingTaskImplFactory { get; set; }

        public ISimpleFactory<IResourceLoadingTaskImpl> ResourceLoadingTaskImplFactory { get; set; }

        public IObjectDestroyer<object> ResourceDestroyer { get; set; }

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

        private IResourceUpdater m_ResourceUpdater = null;

        public IResourceUpdater ResourceUpdater
        {
            get
            {
                if (m_ResourceUpdater == null)
                {
                    return new DummyResourceUpdater();
                }

                return m_ResourceUpdater;
            }
        }

        public void AddUpdateServerRootUrl(string updateServerRootUrl)
        {
            // Empty.
        }

        public void CheckUpdate(AssetIndexRemoteFileInfo remoteIndexFileInfo, UpdateCheckCallbackSet callbackSet, object context)
        {
            if (callbackSet.OnSuccess != null)
            {
                callbackSet.OnSuccess(context);
            }
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

        public void Init()
        {
            // Empty.
        }

        public IAssetAccessor LoadAsset(string assetPath, LoadAssetCallbackSet callbackSet, object context)
        {
            var assetObj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
            var ret = new DummyAssetAccessor {AssetPath = assetPath, AssetObject = assetObj};

            if (assetObj == null)
            {
                var errorMessage = Utility.Text.Format("Failed to load asset at path '{0}'.", assetPath);
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
                if (callbackSet.OnSuccess != null)
                {
                    callbackSet.OnSuccess(ret, context);
                }
            }

            return ret;
        }

        public IAssetAccessor LoadSceneAsset(string sceneAssetPath, LoadAssetCallbackSet callbackSet, object context)
        {
            var assetObj = AssetDatabase.LoadAssetAtPath(sceneAssetPath, typeof(UnityEngine.Object));
            var ret = new DummyAssetAccessor {AssetPath = sceneAssetPath, AssetObject = assetObj};
            if (assetObj == null)
            {
                string errorMessage = Utility.Text.Format("Fail to load scene '{0}'.", sceneAssetPath);
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
                if (callbackSet.OnSuccess != null)
                {
                    callbackSet.OnSuccess(ret, context);
                }
            }

            return ret;
        }

        public int GetAssetResourceGroupId(string assetPath)
        {
            return Constant.CommonResourceGroupId;
        }

        public bool IsLoadingAnyAsset => false;

        public void Prepare(AssetModulePrepareCallbackSet callbackSet, object context)
        {
            MonoBehaviour.StartCoroutine(PrepareCo(callbackSet, context));
        }

        private IEnumerator PrepareCo(AssetModulePrepareCallbackSet callbackSet, object context)
        {
            yield return null;
            if (callbackSet.OnSuccess != null)
            {
                callbackSet.OnSuccess(context);
            }
        }

        public void ShutDown()
        {
            // Empty.
        }

        public void UnloadAsset(IAssetAccessor assetAccessor)
        {
            var dummyAssetAccessor = (DummyAssetAccessor)assetAccessor;
            dummyAssetAccessor.AssetObject = null;
            dummyAssetAccessor.AssetPath = null;
            dummyAssetAccessor.Status = AssetAccessorStatus.None;
        }

        public void Update(TimeStruct timeStruct)
        {
            // Empty.
        }
    }
}

#endif