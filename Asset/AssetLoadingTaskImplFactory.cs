namespace COL.UnityGameWheels.Unity.Asset
{
    using Core;
    using Core.Asset;

    public class AssetLoadingTaskImplFactory : ISimpleFactory<IAssetLoadingTaskImpl>
    {
        public IAssetLoadingTaskImpl Get()
        {
            return new AssetLoadingTaskImpl();
        }
    }
}
