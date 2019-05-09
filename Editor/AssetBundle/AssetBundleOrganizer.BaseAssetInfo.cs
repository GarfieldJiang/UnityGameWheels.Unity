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
                get
                {
                    return m_Guid;
                }

                set
                {
                    m_Guid = value;
                    Path = AssetDatabase.GUIDToAssetPath(m_Guid);
                    if (!string.IsNullOrEmpty(Path))
                    {
                        Name = System.IO.Path.GetFileName(Path);
                    }
                    else
                    {
                        Name = string.Empty;
                    }
                }
            }

            public string Path { get; private set; }

            public string Name { get; private set; }

            public bool IsNullOrMissing
            {
                get
                {
                    return string.IsNullOrEmpty(Path) || AssetDatabase.LoadAssetAtPath<Object>(Path) == null;
                }
            }

            public bool IsFile
            {
                get
                {
                    return !IsNullOrMissing && !AssetDatabase.IsValidFolder(Path);
                }
            }

            public bool IsDirectory
            {
                get
                {
                    return !IsNullOrMissing && AssetDatabase.IsValidFolder(Path);
                }
            }
        }
    }
}
