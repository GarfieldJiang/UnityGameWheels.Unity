namespace COL.UnityGameWheels.Unity
{
    public partial class LogCollectionManager
    {
        private struct Cmd
        {
            internal CmdType CmdType;
            internal ILogCollector Collector;
        }
    }
}
