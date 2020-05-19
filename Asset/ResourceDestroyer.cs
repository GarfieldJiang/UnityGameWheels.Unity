namespace COL.UnityGameWheels.Unity.Asset
{
    using Core;
    using UnityEngine;

    internal class ResourceDestroyer : IObjectDestroyer<object>
    {
        public void Destroy(object obj)
        {
            var assetBundle = (AssetBundle)obj;
            if (assetBundle == null)
            {
                throw new System.ArgumentException("Invalid AssetBundle.", nameof(obj));
            }

            assetBundle.Unload(true);
        }
    }
}