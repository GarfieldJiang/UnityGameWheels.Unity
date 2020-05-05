using COL.UnityGameWheels.Core;

namespace COL.UnityGameWheels.Unity
{
    public interface ILogCollectionService : ILifeCycle
    {
        ILogCallbackRegistrar LogCallbackRegistrar { get; set; }

        void AddLogCollector(ILogCollector collector);

        void RemoveLogCollector(ILogCollector collector);
    }
}