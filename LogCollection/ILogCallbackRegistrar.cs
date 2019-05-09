namespace COL.UnityGameWheels.Unity
{
    using UnityEngine;

    public interface ILogCallbackRegistrar
    {
        void AddLogCallback(Application.LogCallback logCallback);

        void RemoveLogCallback(Application.LogCallback logCallback);
    }
}