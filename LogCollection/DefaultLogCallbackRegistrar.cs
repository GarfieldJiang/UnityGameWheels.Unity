using UnityEngine;

namespace COL.UnityGameWheels.Unity
{
    public class DefaultLogCallbackRegistrar : ILogCallbackRegistrar
    {
        public void AddLogCallback(Application.LogCallback logCallback)
        {
            Application.logMessageReceived += logCallback;
        }

        public void RemoveLogCallback(Application.LogCallback logCallback)
        {
            Application.logMessageReceived -= logCallback;
        }
    }
}