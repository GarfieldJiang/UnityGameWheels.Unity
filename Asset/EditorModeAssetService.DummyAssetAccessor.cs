#if UNITY_EDITOR
using COL.UnityGameWheels.Core.Asset;

namespace COL.UnityGameWheels.Unity.Asset
{
    internal partial class EditorModeAssetService
    {
        private class DummyAssetAccessor : IAssetAccessor
        {
            public string AssetPath { get; internal set; }

            public object AssetObject { get; internal set; }

            public AssetAccessorStatus Status { get; internal set; }
        }
    }
}

#endif