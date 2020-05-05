namespace COL.UnityGameWheels.Unity
{
    public partial class LogCollectionService
    {
        private struct Cmd
        {
            internal CmdType CmdType;
            internal ILogCollector Collector;
        }
    }
}
