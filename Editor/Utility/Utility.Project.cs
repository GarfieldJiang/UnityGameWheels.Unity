namespace COL.UnityGameWheels.Unity.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public static partial class Utility
    {
        public static class Project
        {
            public static void SaveProject()
            {
                AssetDatabase.SaveAssets();
                Debug.Log("[ProjectSaver.SaveProject] You've saved the project.");
            }

            public static void OpenDataPath()
            {
                OpenFolder(Application.dataPath);
            }

            public static void OpenPersistentDataPath()
            {
                OpenFolder(Application.persistentDataPath);
            }

            public static void OpenStreamingAssetsPath()
            {
                OpenFolder(Application.streamingAssetsPath);
            }

            public static void OpenTemporaryCachePath()
            {
                OpenFolder(Application.temporaryCachePath);
            }

            public static void OpenFolder(string folder)
            {
                folder = string.Format("\"{0}\"", folder);
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                        System.Diagnostics.Process.Start("Explorer.exe", folder.Replace('/', '\\'));
                        break;
                    case RuntimePlatform.OSXEditor:
                        System.Diagnostics.Process.Start("open", folder);
                        break;
                    default:
                        throw new NotSupportedException(
                            string.Format("Opening folder on '{0}' platform is not supported.", Application.platform.ToString()));
                }
            }
        }
    }
}
