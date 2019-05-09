
namespace COL.UnityGameWheels.Unity
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    public partial class LogCollectionManager : MonoBehaviourEx, ILogCollectionManager
    {
        private ILogCallbackRegistrar m_LogCallbackRegistrar = null;

        public ILogCallbackRegistrar LogCallbackRegistrar
        {
            get
            {
                if (m_LogCallbackRegistrar == null)
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_LogCallbackRegistrar;
            }

            set
            {
                if (m_LogCallbackRegistrar != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_LogCallbackRegistrar = value;
            }
        }

        private Queue<Cmd> m_Commands = new Queue<Cmd>();
        private List<ILogCollector> m_LogCollectors = new List<ILogCollector>();
        private bool m_ReceivingLog = false;

        public void Init()
        {
            LogCallbackRegistrar.AddLogCallback(OnReceiveLog);
        }

        public void ShutDown()
        {
            LogCallbackRegistrar.RemoveLogCallback(OnReceiveLog);
        }

        public void AddLogCollector(ILogCollector collector)
        {
            if (collector == null)
            {
                throw new ArgumentNullException("collector");
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
                throw new ArgumentNullException("collector");
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
