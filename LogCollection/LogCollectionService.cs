using COL.UnityGameWheels.Core;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Unity
{
    public partial class LogCollectionService : ILogCollectionService, IDisposable
    {
        private readonly ILogCallbackRegistrar m_LogCallbackRegistrar = null;
        private readonly Queue<Cmd> m_Commands = new Queue<Cmd>();
        private readonly List<ILogCollector> m_LogCollectors = new List<ILogCollector>();
        private bool m_ReceivingLog = false;

        public LogCollectionService(ILogCallbackRegistrar logCallbackRegistrar)
        {
            m_LogCallbackRegistrar = logCallbackRegistrar ?? throw new ArgumentNullException(nameof(logCallbackRegistrar));
            m_LogCallbackRegistrar.AddLogCallback(OnReceiveLog);
        }

        public void AddLogCollector(ILogCollector collector)
        {
            if (collector == null)
            {
                throw new ArgumentNullException(nameof(collector));
            }

            if (m_ReceivingLog)
            {
                m_Commands.Enqueue(new Cmd { CmdType = CmdType.Add, Collector = collector });
            }
            else
            {
                m_LogCollectors.Add(collector);
            }
        }

        public void RemoveLogCollector(ILogCollector collector)
        {
            if (collector == null)
            {
                throw new ArgumentNullException(nameof(collector));
            }

            if (m_ReceivingLog)
            {
                m_Commands.Enqueue(new Cmd { CmdType = CmdType.Remove, Collector = collector });
            }
            else
            {
                m_LogCollectors.Remove(collector);
            }
        }

        public void Dispose()
        {
            m_LogCallbackRegistrar.RemoveLogCallback(OnReceiveLog);
        }

        private void OnReceiveLog(string logMessage, string stackTrace, LogType type)
        {
            m_ReceivingLog = true;
            var logEntry = new LogEntry(logMessage, stackTrace, type);
            try
            {
                foreach (var collector in m_LogCollectors)
                {
                    collector.OnReceiveLogEntry(logEntry);
                }
            }
            finally
            {
                m_ReceivingLog = false;
            }

            while (m_Commands.Count > 0)
            {
                ExecuteCmd(m_Commands.Dequeue());
            }
        }

        private void ExecuteCmd(Cmd cmd)
        {
            switch (cmd.CmdType)
            {
                case CmdType.Add:
                    m_LogCollectors.Add(cmd.Collector);
                    break;
                case CmdType.Remove:
                default:
                    m_LogCollectors.Remove(cmd.Collector);
                    break;
            }
        }
    }
}