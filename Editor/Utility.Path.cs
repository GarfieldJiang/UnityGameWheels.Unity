using System.Linq;

namespace COL.UnityGameWheels.Unity.Editor
{
    public static partial class Utility
    {
        public static class Path
        {
            public static bool IsEditor(string path)
            {
                var segments = path.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
                return segments.Any(s => s.ToLower() == "editor");
            }
        }
    }
}