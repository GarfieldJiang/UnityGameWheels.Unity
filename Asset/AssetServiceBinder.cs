using System;
using System.IO;
using COL.UnityGameWheels.Core;
using COL.UnityGameWheels.Core.Asset;
using COL.UnityGameWheels.Core.Ioc;
using UnityEditor;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Asset
{
    public static class AssetServiceBinder
    {
        public static IBindingData Bind(IContainer container, AssetServiceConfig config, MonoBehaviourEx mb)
        {
            Guard.RequireNotNull<ArgumentNullException>(container, $"Invalid '{container}'.");
            Guard.RequireNotNull<ArgumentNullException>(config, $"Invalid '{config}'.");
            Guard.RequireNotNull<ArgumentNullException>(mb, $"Invalid '{mb}'.");
            IBindingData bindingData;
#if UNITY_EDITOR
            if (config.EditorMode)
            {
                bindingData = container.BindSingleton<IAssetService, EditorModeAssetService>();
            }
            else
            {
                bindingData = BindRealAssetService(container, config, mb);
            }
#else
            bindingData = BindRealAssetService(container, config, mb);
#endif
            return bindingData;
        }

        private static IBindingData BindRealAssetService(IContainer container, AssetServiceConfig config,
            MonoBehaviourEx mb)
        {
            container.BindSingleton<IAssetLoadingTaskImpl, AssetLoadingTaskImpl>();
            container.BindSingleton<ISimpleFactory<IAssetLoadingTaskImpl>, AssetLoadingTaskImplFactory>();
            container.BindSingleton<ISimpleFactory<IResourceLoadingTaskImpl>, ResourceLoadingTaskImplFactory>();

            return container.BindSingleton<IAssetService, AssetService>(new PropertyInjection
                {
                    PropertyName = "IndexForInstallerLoader",
                    Value = new AssetIndexForInstallerLoader(mb),
                })
                .OnPreInit(serviceInstance =>
                {
                    var assetService = (AssetService)serviceInstance;
                    assetService.UpdateIsEnabled = config.UpdateIsEnabled;
                    assetService.DownloadRetryCount = config.DownloadRetryCount;
                    assetService.ConcurrentAssetLoaderCount = config.ConcurrentAssetLoaderCount;
                    assetService.ConcurrentResourceLoaderCount = config.ConcurrentResourceLoaderCount;
                    assetService.AssetCachePoolCapacity = config.AssetCachePoolCapacity;
                    assetService.ResourceCachePoolCapacity = config.ResourceCachePoolCapacity;
                    assetService.AssetAccessorPoolCapacity = config.AssetAccessorPoolCapacity;
                    assetService.UpdateRelativePathFormat = config.UpdateRelativePathFormat;
                    assetService.ReadWritePath = Path.Combine(Application.persistentDataPath, config.ReadWriteRelativePath);
                    assetService.InstallerPath = Path.Combine(Application.streamingAssetsPath, config.InstallerRelativePath);
                    assetService.ResourceDestroyer = new ResourceDestroyer();
                    assetService.BundleVersion = Application.version;
                    assetService.ReleaseResourceInterval = config.ReleaseResourceInterval;
                    assetService.UpdateSizeBeforeSavingReadWriteIndex = config.UpdateSizeBeforeSavingReadWriteIndex;
                    assetService.RunningPlatform = GetRunningPlatform(config);
                    foreach (var url in config.UpdateServerRootUrls)
                    {
                        assetService.AddUpdateServerRootUrl(url);
                    }
                }).OnPostShutdown(() => { AssetBundle.UnloadAllAssetBundles(true); });
        }

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