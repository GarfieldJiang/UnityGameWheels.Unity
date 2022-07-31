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

            public AssetBundleInfo(bool sorted) : base(sorted)
            {
            }

            public override string ToString()
            {
                return $"[AssetBundleInfo Path={Path}, Name={Name}, Group={GroupId}, DontContainInInstaller={DontPack}, " +
                       $"IsDirectory={IsDirectory}, IsRoot={IsRoot}, IsSorted={IsSorted}]";
            }
        }
    }
}