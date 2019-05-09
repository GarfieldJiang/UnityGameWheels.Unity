namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizer
    {
        public class AssetInfo : PathTreeNode<AssetInfo>, IAssetInfo
        {
            private BaseAssetInfo m_Base = new BaseAssetInfo();
            public string AssetBundlePath = string.Empty;

            public string Guid
            {
                get
                {
                    return m_Base.Guid;
                }

                set
                {
                    m_Base.Guid = value;
                }
            }

            public string Path
            {
                get
                {
                    return m_Base.Path;
                }
            }

            public bool IsNullOrMissing
            {
                get
                {
                    return m_Base.IsNullOrMissing;
                }
            }

            public bool IsFile
            {
                get
                {
                    return m_Base.IsFile;
                }
            }

            public bool IsDirectory
            {
                get
                {
                    return m_Base.IsDirectory;
                }
            }

            string IAssetInfo.Name
            {
                get
                {
                    return m_Base.Name;
                }
            }

            public override string ToString()
            {
                return Core.Utility.Text.Format("[AssetInfo AssetPath='{0}', Name='{1}', GuidStr={2}, IsRoot={3}, AssetBundlePath='{4}']",
                                                Path, Name, Guid, IsRoot, AssetBundlePath);
            }
        }
    }
}
