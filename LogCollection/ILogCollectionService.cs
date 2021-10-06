using COL.UnityGameWheels.Core;

namespace COL.UnityGameWheels.Unity
{
    public interface ILogCollectionService
    {
        void AddLogCollector(ILogCollector collector);

        void RemoveLogCollector(ILogCollector collector);
    }
}