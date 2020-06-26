using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace COL.UnityGameWheels.Unity.Editor
{
    public class AssetInfo
    {
        public string Guid;
        public string AssetBundlePath;

        private string m_AssetPath;

        public string AssetPath => Utility.Asset.GetAssetPathFromGUID(Guid, ref m_AssetPath);

        public HashSet<string> DependencyAssetGuids { get; } = new HashSet<string>();

        public static explicit operator Core.Asset.AssetInfo(AssetInfo self)
        {
            var ret = new Core.Asset.AssetInfo {Path = self.AssetPath, ResourcePath = self.AssetBundlePath};
            ret.DependencyAssetPaths.UnionWith(self.DependencyAssetGuids.Select(AssetDatabase.GUIDToAssetPath));
            return ret;
        }
    }
}