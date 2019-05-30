namespace COL.UnityGameWheels.Unity.Asset
{
    using Core;
    using Core.Asset;
    using System.Collections.Generic;

    public interface IAssetManager : IManager
    {
        IAssetModule Module { get; }

        IDownloadModule DownloadModule { get; set; }

        IRefPoolModule RefPoolModule { get; set; }

        IResourceUpdater ResourceUpdater { get; }

        void Prepare(AssetManagerPrepareCallbackSet callbackSet, object context);

        void CheckUpdate(AssetIndexRemoteFileInfo remoteIndexFileInfo, UpdateCheckCallbackSet callbackSet, object context);

        IAssetAccessor LoadAsset(string assetPath, LoadAssetCallbackSet callbackSet, object context);

        IAssetAccessor LoadSceneAsset(string sceneAssetPath, LoadAssetCallbackSet callbackSet, object context);

        void UnloadAsset(IAssetAccessor assetAccessor);

        bool IsLoadingAnyAsset { get; }

        void RequestUnloadUnusedAssetBundles();

        void UnloadUnusedAssets();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>For debug use.</remarks>
        IDictionary<string, AssetCacheQuery> GetAssetCacheQueries();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>For debug use.</remarks>
        IDictionary<string, ResourceCacheQuery> GetResourceCacheQueries();
    }
}