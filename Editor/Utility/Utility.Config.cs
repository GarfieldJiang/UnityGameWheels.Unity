namespace COL.UnityGameWheels.Unity.Editor
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static partial class Utility
    {
        public static class Config
        {
            public static TConfig Read<TAttribute, TConfig>() where TAttribute : ConfigReadAttribute
            {
                var field = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .SelectMany(t => t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    .FirstOrDefault(fi => fi.FieldType.IsAssignableFrom(typeof(TConfig)) && Attribute.IsDefined(fi, typeof(TAttribute)));

                if (field != null)
                {
                    return (TConfig)field.GetValue(null);
                }

                var property = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .SelectMany(t => t.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    .FirstOrDefault(pi => pi.PropertyType.IsAssignableFrom(typeof(TConfig)) && Attribute.IsDefined(pi, typeof(TAttribute)));

                if (property != null)
                {
                    if (!property.CanRead)
                    {
                        throw new InvalidOperationException(
                            Core.Utility.Text.Format("Attribute '{0}' should not be used on an unreadable property '{1}' of class '{2}'.",
                            typeof(TAttribute), property.Name, property.DeclaringType.FullName));
                    }

                    return (TConfig)property.GetValue(null, null);
                }

                return default(TConfig);
            }
        }
    }
}
