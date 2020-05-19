using UnityEditor;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizer
    {
        public class BaseAssetInfo : IAssetInfo
        {
            private string m_Guid = string.Empty;

            public string Guid
            {
                get => m_Guid;

                set
                {
                    m_Guid = value;
                    Path = AssetDatabase.GUIDToAssetPath(m_Guid);
                    m_IsEditor = !string.IsNullOrEmpty(Path) && Utility.Path.IsEditor(Path);
                    Name = !string.IsNullOrEmpty(Path) ? System.IO.Path.GetFileName(Path) : string.Empty;
                }
            }

            public string Path { get; private set; }

            public string Name { get; private set; }

            public bool IsNullOrMissing => string.IsNullOrEmpty(Path) || AssetDatabase.GetMainAssetTypeAtPath(Path) == null || m_IsEditor;

            public bool IsFile => !IsNullOrMissing && !AssetDatabase.IsValidFolder(Path);

            public bool IsDirectory => !IsNullOrMissing && AssetDatabase.IsValidFolder(Path);

            private bool m_IsEditor;
        }
    }
}