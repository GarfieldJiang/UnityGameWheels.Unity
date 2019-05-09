using System.Collections.Generic;

namespace COL.UnityGameWheels.Unity.Editor
{
    public class AssetBundleInfo
    {
        public string Path;

        public int GroupId;

        public bool DontPack = false;

        private List<string> m_AssetGuids = new List<string>();

        public List<string> AssetGuids
        {
            get
            {
                return m_AssetGuids;
            }
        }
    }
}
