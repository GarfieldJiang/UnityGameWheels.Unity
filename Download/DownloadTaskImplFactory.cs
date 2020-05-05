using COL.UnityGameWheels.Core;

namespace COL.UnityGameWheels.Unity
{
    public class DownloadTaskImplFactory : ISimpleFactory<IDownloadTaskImpl>
    {
        public IDownloadTaskImpl Get()
        {
            return new DownloadTaskImpl();
        }
    }
}