namespace COL.UnityGameWheels.Unity
{
    using UnityEngine;

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