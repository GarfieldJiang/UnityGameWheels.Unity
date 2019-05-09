using System;

namespace COL.UnityGameWheels.Unity.Editor
{
    /// <summary>
    /// Attribute to mark the readable configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class ConfigReadAttribute : Attribute
    {

    }

}
