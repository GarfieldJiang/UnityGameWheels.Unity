namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizer
    {
        internal interface IAssetInfo
        {
            string Guid { get; set; }

            string Path { get; }

            string Name { get; }

            bool IsNullOrMissing { get; }

            bool IsFile { get; }

            bool IsDirectory { get; }
        }
    }
}
