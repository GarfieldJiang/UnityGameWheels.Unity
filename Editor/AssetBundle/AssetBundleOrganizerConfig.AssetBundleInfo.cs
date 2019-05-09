using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizerConfig
    {
        [Serializable]
        public class AssetBundleInfo
        {
            public string AssetBundlePath = string.Empty;
            public int AssetBundleGroup = 0;
            public bool DontPack = false;
            public List<string> AssetGuids = new List<string>();
        }
    }
}
