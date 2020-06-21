using System.Collections.Generic;
using UnityEditor;

namespace COL.UnityGameWheels.Unity.Editor
{
    /// <summary>
    /// Asset that are not explicitly collected into any asset bundle.
    /// </summary>
    /// <remarks>Can be used in redundancy checking.</remarks>
    public class UncollectedAssetInfo
    {
        public string Guid { get; internal set; }

        private string m_AssetPath;

        public string AssetPath
        {
            get
            {
                if (string.IsNullOrEmpty(m_AssetPath))
                {
                    m_AssetPath = AssetDatabase.GUIDToAssetPath(Guid);
                }

                return m_AssetPath;
            }
        }

        internal readonly HashSet<string> AssetGuidsDependingOnThis = new HashSet<string>();

        /// <summary>
        /// Get (collected) asset GUIDs that depends on the this asset.
        /// </summary>
        /// <returns>(Collected) asset GUIDs that depends on the this asset.</returns>
        public IReadOnlyCollection<string> GetAssetGuidsDependingOnThis()
        {
            return AssetGuidsDependingOnThis;
        }
    }
}