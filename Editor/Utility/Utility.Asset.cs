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
                    field = AssetDatabase.AssetPathToGUID(guid);
                }

                return field;
            }
        }
    }
}