using System.Collections.Generic;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizer
    {
        public abstract class PathTreeNode<T>
        {
            public string Name { get; internal set; } = string.Empty;
            public T Parent { get; internal set; } = default(T);

            public readonly IDictionary<string, T> Children;
            public readonly bool IsSorted;

            public bool IsRoot => Parent == null;

            public PathTreeNode(bool sorted)
            {
                if (sorted)
                {
                    IsSorted = true;
                    Children = new SortedDictionary<string, T>();
                }
                else
                {
                    IsSorted = false;
                    Children = new Dictionary<string, T>();
                }
            }
        }
    }
}