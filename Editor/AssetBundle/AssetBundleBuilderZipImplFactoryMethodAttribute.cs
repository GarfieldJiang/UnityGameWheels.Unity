using System;

namespace COL.UnityGameWheels.Unity.Editor
{
    /// <summary>
    /// Attribute to mark the factory method to create <see cref="COL.UnityGameWheels.Core.IZipImpl"/> instances for <see cref="AssetBundleBuilder"/>.
    /// </summary>
    /// <remarks>The method should return a <see cref="COL.UnityGameWheels.Core.IZipImpl"/> when called.</remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class AssetBundleBuilderZipImplFactoryMethodAttribute : ConfigReadAttribute
    {
        // Empty;
    }
}