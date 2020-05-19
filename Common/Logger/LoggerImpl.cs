using System;
using COL.UnityGameWheels.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace COL.UnityGameWheels.Unity
{
    public class LoggerImpl : ILoggerImpl
    {
        private static readonly LogType[] LogTypes =
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
                throw new System.ArgumentException("Must be UnityEngine.Object", nameof(context));
            }

            Debug.unityLogger.Log(LogTypes[(int)logLevel], message, (Object)context);
        }

        public void WriteException(Exception exception)
        {
            Debug.unityLogger.LogException(exception);
        }

        public void WriteException(Exception exception, object context)
        {
            if (!(context is Object))
            {
                throw new System.ArgumentException("Must be UnityEngine.Object", nameof(context));
            }

            Debug.unityLogger.LogException(exception, (Object)context);
        }
    }
}