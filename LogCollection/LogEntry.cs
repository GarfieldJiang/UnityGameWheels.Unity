namespace COL.UnityGameWheels.Unity
{
    using UnityEngine;

    public struct LogEntry
    {
        public readonly string LogMessage;
        public readonly string StackTrace;
        public readonly LogType LogType;

        public LogEntry(string logMessage, string stackTrace, LogType logType)
        {
            LogMessage = logMessage ?? string.Empty;
            StackTrace = stackTrace ?? string.Empty;
            LogType = logType;
        }
    }
}
