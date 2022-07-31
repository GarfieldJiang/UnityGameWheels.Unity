namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizer
    {
        public class AssetInfo : PathTreeNode<AssetInfo>, IAssetInfo
        {
            private readonly BaseAssetInfo m_Base = new BaseAssetInfo();
            public string AssetBundlePath = string.Empty;

            public string Guid
            {
                get => m_Base.Guid;

                set => m_Base.Guid = value;
            }

            public string Path => m_Base.Path;

            public bool IsNullOrMissing => m_Base.IsNullOrMissing;

            public bool IsFile => m_Base.IsFile;

            public bool IsDirectory => m_Base.IsDirectory;

            string IAssetInfo.Name => m_Base.Name;

            public AssetInfo(bool sorted) : base(sorted)
            {
            }

            public override string ToString()
            {
                return
                    $"[AssetInfo Path='{Path}', Name='{Name}', GuidStr={Guid}, IsRoot={IsRoot}, AssetBundlePath='{AssetBundlePath}', IsSorted={IsSorted}.]";
            }
        }
    }
}