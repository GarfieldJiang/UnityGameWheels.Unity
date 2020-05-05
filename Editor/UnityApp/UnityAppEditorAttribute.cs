using System;

namespace COL.UnityGameWheels.Unity.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UnityAppEditorAttribute : Attribute
    {
        public Type TargetType { get; }

        public UnityAppEditorAttribute(Type targetType)
        {
            TargetType = targetType;
        }
    }
}