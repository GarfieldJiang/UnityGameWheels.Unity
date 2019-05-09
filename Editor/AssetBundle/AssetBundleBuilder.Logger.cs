using System;
using System.IO;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleBuilder
    {
        private class Logger : IDisposable
        {
            private const string TagFormat = "[{0}][{1}]";
            private StreamWriter m_Writer = null;
            private enum LogType
            {
                Info,
                Warning,
                Error,
            }

            public Logger(AssetBundleBuilder assetBundleBuilder)
            {
                var prevPath = Path.Combine(assetBundleBuilder.LogDirectory, PreviousLogFileName);
                var curPath = Path.Combine(assetBundleBuilder.LogDirectory, CurrentLogFileName);

                if (File.Exists(prevPath))
                {
                    File.Delete(prevPath);
                }

                if (File.Exists(curPath))
                {
                    File.Move(curPath, prevPath);
                }

                m_Writer = new FileInfo(curPath).CreateText();
            }

            public void Dispose()
            {
                if (m_Writer != null)
                {
                    m_Writer.Dispose();
                    m_Writer = null;
                }
            }

            private void WriteLog(LogType logType, string messageFormat, params object[] args)
            {
                var logTag = Core.Utility.Text.Format(TagFormat, DateTime.Now.ToString("HH:mm:ss.fff"), logType.ToString().ToUpper());
                var logContent = (args == null || args.Length <= 0) ? messageFormat : string.Format(messageFormat, args);
                m_Writer.WriteLine(Core.Utility.Text.Format("{0} {1}", logTag, logContent));
            }

            public void Info(string messageFormat, params object[] args)
            {
                WriteLog(LogType.Info, messageFormat, args);
            }

            public void Warning(string messageFormat, params object[] args)
            {
                WriteLog(LogType.Warning, messageFormat, args);
            }

            public void Error(string messageFormat, params object[] args)
            {
                WriteLog(LogType.Error, messageFormat, args);
            }
        }
    }
}
