namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizer
    {
        public class AssetBundleInfo : PathTreeNode<AssetBundleInfo>
        {
            public string Path { get; internal set; }
            public int GroupId { get; internal set; }
            public bool DontPack { get; internal set; }
            public bool IsDirectory { get; internal set; }

            public override string ToString()
            {
                return Core.Utility.Text.Format("[AssetBundleInfo Path={0}, Name={1}, Group={2}, DontContainInInstaller={3}, IsDirectory={4}, IsRoot={5}]",
                    Path, Name, GroupId, DontPack, IsDirectory, IsRoot);
            }
        }
    }
}