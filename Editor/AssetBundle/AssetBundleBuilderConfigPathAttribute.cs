using System;

namespace COL.UnityGameWheels.Unity.Editor
{
    /// <summary>
    /// Attribute to mark the configuration file path for <see cref="AssetBundleBuilderConfig"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AssetBundleBuilderConfigPathAttribute : ConfigReadAttribute
    {
        // Empty.
    }
}
