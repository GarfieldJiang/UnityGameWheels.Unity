using COL.UnityGameWheels.Core;

namespace COL.UnityGameWheels.Unity
{
    internal class DownloadTaskImplFactory : ISimpleFactory<IDownloadTaskImpl>
    {
        public IDownloadTaskImpl Get()
        {
            return new DownloadTaskImpl();
        }
    }
}
