using System.Collections.Generic;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizer
    {
        public abstract class PathTreeNode<T>
        {
            public string Name = string.Empty;
            public T Parent = default(T);
            public SortedDictionary<string, T> Children = new SortedDictionary<string, T>();

            public bool IsRoot
            {
                get
                {
                    return Parent == null;
                }
            }
        }
    }
}
