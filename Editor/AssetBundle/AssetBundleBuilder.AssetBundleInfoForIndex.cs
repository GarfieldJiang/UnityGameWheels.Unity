namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleBuilder
    {
        private class AssetBundleInfoForIndex
        {
            public string Path;
            public int GroupId;
            public uint Crc32;
            public long Size;
            public string Hash;

            public bool DontPack;

            public static explicit operator Core.Asset.ResourceInfo(AssetBundleInfoForIndex self)
            {
                return new Core.Asset.ResourceInfo
                {
                    Path = self.Path,
                    Crc32 = self.Crc32,
                    Size = self.Size,
                    Hash = self.Hash,
                };
            }

            public static explicit operator Core.Asset.ResourceBasicInfo(AssetBundleInfoForIndex self)
            {
                return new Core.Asset.ResourceBasicInfo
                {
                    Path = self.Path,
                    GroupId = self.GroupId,
                };
            }
        }
    }
}
