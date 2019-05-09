namespace COL.UnityGameWheels.Unity
{
    using Core;

    public static partial class Utility
    {
        public static class Time
        {
            public static TimeStruct GetTimeStruct()
            {
                return new TimeStruct(
                    deltaTime: UnityEngine.Time.deltaTime,
                    unscaledDeltaTime: UnityEngine.Time.unscaledDeltaTime,
                    time: UnityEngine.Time.time,
                    unscaledTime: UnityEngine.Time.unscaledTime
                    );
            }
        }
    }
}
