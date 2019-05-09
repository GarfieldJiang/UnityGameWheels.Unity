namespace COL.UnityGameWheels.Unity
{
    public interface ILogCollectionManager : IManager
    {
        ILogCallbackRegistrar LogCallbackRegistrar { get; set; }

        void AddLogCollector(ILogCollector collector);

        void RemoveLogCollector(ILogCollector collector);
    }
}
