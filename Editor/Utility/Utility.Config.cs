namespace COL.UnityGameWheels.Unity.Editor
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static partial class Utility
    {
        public static class Config
        {
            public static ConfigType Read<AttributeType, ConfigType>() where AttributeType : ConfigReadAttribute
            {
                var field = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .SelectMany(t => t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    .FirstOrDefault(fi => fi.FieldType.IsAssignableFrom(typeof(ConfigType)) && Attribute.IsDefined(fi, typeof(AttributeType)));

                if (field != null)
                {
                    return (ConfigType)field.GetValue(null);
                }

                var property = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .SelectMany(t => t.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    .FirstOrDefault(pi => pi.PropertyType.IsAssignableFrom(typeof(ConfigType)) && Attribute.IsDefined(pi, typeof(AttributeType)));

                if (property != null)
                {
                    if (!property.CanRead)
                    {
                        throw new InvalidOperationException(
                            Core.Utility.Text.Format("Attribute '{0}' should not be used on an unreadable property '{1}' of class '{2}'.",
                            typeof(AttributeType), property.Name, property.DeclaringType.FullName));
                    }

                    return (ConfigType)property.GetValue(null, null);
                }

                return default(ConfigType);
            }
        }
    }
}
