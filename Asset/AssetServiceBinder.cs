using System;
using COL.UnityGameWheels.Core;
using COL.UnityGameWheels.Core.Asset;
using COL.UnityGameWheels.Core.Ioc;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Asset
{
    public static class AssetServiceBinder
    {
        public static IBindingData Bind(Container container, AssetServiceConfig config, MonoBehaviourEx mb)
        {
            Guard.RequireNotNull<ArgumentNullException>(container, $"Invalid '{nameof(container)}'.");
            Guard.RequireNotNull<ArgumentNullException>(config, $"Invalid '{nameof(config)}'.");
            Guard.RequireNotNull<ArgumentNullException>(mb, $"Invalid '{nameof(mb)}'.");
            IBindingData bindingData;
#if UNITY_EDITOR
            bindingData = config.EditorMode ? container.BindSingleton<IAssetService, EditorModeAssetService>() : BindRealAssetService(container, config, mb);
#else
            bindingData = BindRealAssetService(container, config, mb);
#endif
            return bindingData;
        }

        private static IBindingData BindRealAssetService(Container container, AssetServiceConfig config,
            MonoBehaviourEx mb)
        {
            container.BindInstance<IAssetServiceConfigReader>(config);
            container.BindInstance<IAssetIndexForInstallerLoader>(new AssetIndexForInstallerLoader(mb));
            container.BindSingleton<IAssetLoadingTaskImpl, AssetLoadingTaskImpl>();
            container.BindSingleton<ISimpleFactory<IAssetLoadingTaskImpl>, AssetLoadingTaskImplFactory>();
            container.BindSingleton<ISimpleFactory<IResourceLoadingTaskImpl>, ResourceLoadingTaskImplFactory>();
            container.BindSingleton<IObjectDestroyer<object>, ResourceDestroyer>();

            return container.BindSingleton<IAssetService, AssetService>()
                .OnInstanceCreated(serviceInstance =>
                {
                    ((AssetService)serviceInstance).BundleVersion = Application.version;
                })
                .OnDisposed(() => { AssetBundle.UnloadAllAssetBundles(true); });
        }
    }
}