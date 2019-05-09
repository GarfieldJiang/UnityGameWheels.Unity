using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace COL.UnityGameWheels.Unity.Editor
{
    public class AssetInfo
    {
        public string Guid;
        public string AssetBundlePath;

        private HashSet<string> m_DependencyAssetGuids = new HashSet<string>();

        public HashSet<string> DependencyAssetGuids
        {
            get
            {
                return m_DependencyAssetGuids;
            }
        }

        public static explicit operator Core.Asset.AssetInfo(AssetInfo self)
        {
            var ret = new Core.Asset.AssetInfo();
            ret.Path = AssetDatabase.GUIDToAssetPath(self.Guid);
            ret.ResourcePath = self.AssetBundlePath;
            ret.DependencyAssetPaths.UnionWith(self.DependencyAssetGuids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)));
            return ret;
        }
    }
}
