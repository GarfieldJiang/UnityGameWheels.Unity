using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace COL.UnityGameWheels.Unity.Editor
{
    public class AssetInfo
    {
        public string Guid;
        public string AssetBundlePath;

        public HashSet<string> DependencyAssetGuids { get; } = new HashSet<string>();

        public static explicit operator Core.Asset.AssetInfo(AssetInfo self)
        {
            var ret = new Core.Asset.AssetInfo {Path = AssetDatabase.GUIDToAssetPath(self.Guid), ResourcePath = self.AssetBundlePath};
            ret.DependencyAssetPaths.UnionWith(self.DependencyAssetGuids.Select(AssetDatabase.GUIDToAssetPath));
            return ret;
        }
    }
}
