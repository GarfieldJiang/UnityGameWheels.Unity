using System.Collections.Generic;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizer
    {
        public abstract class PathTreeNode<T>
        {
            public string Name { get; internal set; } = string.Empty;
            public T Parent { get; internal set; } = default(T);
            public SortedDictionary<string, T> Children { get; internal set; } = new SortedDictionary<string, T>();

            public bool IsRoot => Parent == null;
        }
    }
}