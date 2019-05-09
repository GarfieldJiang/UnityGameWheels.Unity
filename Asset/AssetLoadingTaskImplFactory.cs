namespace COL.UnityGameWheels.Unity.Asset
{
    using Core;
    using Core.Asset;

    public class ResourceLoadingTaskImplFactory : ISimpleFactory<IResourceLoadingTaskImpl>
    {
        public IResourceLoadingTaskImpl Get()
        {
            return new ResourceLoadingTaskImpl();
        }
    }
}
