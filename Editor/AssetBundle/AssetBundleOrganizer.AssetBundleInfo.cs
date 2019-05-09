namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizer
    {
        public class AssetBundleInfo : PathTreeNode<AssetBundleInfo>
        {
            public string Path;
            public int GroupId;
            public bool DontPack;
            public bool IsDirectory;

            public override string ToString()
            {
                return Core.Utility.Text.Format("[AssetBundleInfo Path={0}, Name={1}, Group={2}, DontContainInInstaller={3}, IsDirectory={4}, IsRoot={5}]",
                                                Path, Name, GroupId, DontPack, IsDirectory, IsRoot);
            }
        }
    }
}
