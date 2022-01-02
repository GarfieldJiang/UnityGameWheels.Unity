using UnityEditor;

namespace COL.UnityGameWheels.Unity.Editor
{
    public static partial class Utility
    {
        public static class Asset
        {
            public static string GetAssetPathFromGUID(string guid, ref string field)
            {
                if (string.IsNullOrEmpty(field))
                {
                    field = AssetDatabase.GUIDToAssetPath(guid);
                }

                return field;
            }

            public static bool IsEditorPath(string assetPath)
            {
                return assetPath.Contains("/Editor/")
                       || assetPath.Contains("/Editor Default Resources/")
                       || assetPath.EndsWith("/Editor")
                       || assetPath.EndsWith("/Editor Default Resources");
            }
        }
    }
}