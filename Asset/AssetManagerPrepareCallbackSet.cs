namespace COL.UnityGameWheels.Unity.Asset
{
    public delegate void OnPrepareAssetManagerSuccess(object context);

    public delegate void OnPrepareAssetManagerFailure(string errorMessage, object context);

    public struct AssetManagerPrepareCallbackSet
    {
        public OnPrepareAssetManagerSuccess OnSuccess;

        public OnPrepareAssetManagerFailure OnFailure;
    }
}
