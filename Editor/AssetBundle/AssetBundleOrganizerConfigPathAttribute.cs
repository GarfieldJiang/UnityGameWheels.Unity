using System;

namespace COL.UnityGameWheels.Unity.Editor
{
    /// <summary>
    /// Attribute to mark the configuration file path for <see cref="AssetBundleOrganizerConfig"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AssetBundleOrganizerConfigPathAttribute : ConfigReadAttribute
    {
        // Empty.
    }
}
