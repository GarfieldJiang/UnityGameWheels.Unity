namespace COL.UnityGameWheels.Unity
{
    using Core;
    using UnityEngine;

    public class LoggerImpl : ILoggerImpl
    {
        private readonly static LogType[] LogTypes =
        {
            LogType.Log,
            LogType.Log,
            LogType.Warning,
            LogType.Error,
            LogType.Error,
        };

        public void WriteLog(LogLevel logLevel, object message)
        {
            if (logLevel == LogLevel.Fatal)
            {
                throw new System.Exception(message == null ? string.Empty : message.ToString());
            }

            Debug.unityLogger.Log(LogTypes[(int)logLevel], message);
        }

        public void WriteLog(LogLevel logLevel, object message, object context)
        {
            if (logLevel == LogLevel.Fatal)
            {
                throw new System.Exception(message == null ? string.Empty : message.ToString());
            }

            if (context == null)
            {
                Debug.unityLogger.Log(LogTypes[(int)logLevel], message);
                return;
            }

            if (!(context is Object))
            {
                throw new System.ArgumentException("Must be UnityEngine.Object", "context");
            }

            Debug.unityLogger.Log(LogTypes[(int)logLevel], message, (Object)message);
        }
    }
}
