using System.Collections.Generic;

namespace COL.UnityGameWheels.Unity.Editor
{
    public class AssetBundleInfo
    {
        public string Path;

        public int GroupId;

        public bool DontPack = false;

        public List<string> AssetGuids { get; } = new List<string>();
    }
}