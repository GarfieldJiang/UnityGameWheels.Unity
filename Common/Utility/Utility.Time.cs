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
                    UnityEngine.Time.deltaTime,
                    UnityEngine.Time.unscaledDeltaTime,
                    UnityEngine.Time.time,
                    UnityEngine.Time.unscaledTime
                );
            }
        }
    }
}