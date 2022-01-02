using System;

namespace COL.UnityGameWheels.Unity.Editor
{
    /// <summary>
    /// Attribute to mark assets to be ignored by the <see cref="AssetBundleOrganizer"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AssetBundleOrganizerIgnoreAssetLabelAttribute : ConfigReadAttribute
    {
        // Empty.
    }
}