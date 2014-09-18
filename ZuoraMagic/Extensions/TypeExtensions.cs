using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using ZuoraMagic.Attributes;
using ZuoraMagic.Entities;

namespace ZuoraMagic.Extensions
{
    internal static class TypeExtensions
    {
        internal static object Lock = new object();
        private static readonly IDictionary<string, IEnumerable<PropertyInfo>> PropertyInfos = new Dictionary<string, IEnumerable<PropertyInfo>>();

        internal static IEnumerable<string> GetPropertyNames(this Type type)
        {
            return type.GetPrimitiveProperties().ToArray().GetNames();
        }

        internal static IEnumerable<string> GetNames(this PropertyInfo[] infos)
        {
            // TODO: There has to be a better way to do this, the Id field needs to be first.
            List<string> names = new List<string> { GetName(infos.FirstOrDefault(x => x.Name == "Id")) };
            names.AddRange(infos.Where(x => x.Name != "Id").Select(x => x.GetName()));

            return names;
        }

        internal static string GetName(this PropertyInfo info)
        {
            return info.GetCustomAttribute<ZuoraNameAttribute>() != null
                ? info.GetCustomAttribute<ZuoraNameAttribute>().Name ?? info.Name
                : info.Name;
        }

        internal static string GetSelectName(this PropertyInfo info, string typeName)
        {
            string propertyName = info.Name;
            ZuoraNameAttribute attribute = info.GetCustomAttribute<ZuoraNameAttribute>();

            if (attribute != null)
            {
                propertyName = attribute.Name;
                if (!string.IsNullOrEmpty(attribute.MappingOverride)) typeName = attribute.MappingOverride;
            }

            return typeName + "." + propertyName;
        }

        internal static string GetName(this Type type)
        {
            ZuoraNameAttribute attribute = type.GetCustomAttribute<ZuoraNameAttribute>();
            return attribute != null ? attribute.Name ?? type.Name : type.Name;
        }

        internal static string GetMappingName(this Type type)
        {
            ZuoraNameAttribute attribute = type.GetCustomAttribute<ZuoraNameAttribute>();
            if (attribute == null) return type.Name;

            return attribute.MappingOverride ?? (attribute.Name ?? type.Name);
        }

        internal static string GetMappingName(this PropertyInfo info, string defaultName = null)
        {
            ZuoraNameAttribute attribute = info.GetCustomAttribute<ZuoraNameAttribute>();
            if (attribute != null && !string.IsNullOrEmpty(attribute.MappingOverride)) 
                return attribute.MappingOverride;

            return defaultName;
        }

        internal static IEnumerable<PropertyInfo> GetObjectProperties(this Type type)
        {
            return from property in type.GetCachedProperties()
                   let propertyType = property.PropertyType
                   let zObjectType = typeof(ZObject)
                   where zObjectType.IsAssignableFrom(propertyType)
                   || (propertyType.IsGenericType && zObjectType.IsAssignableFrom(propertyType.GetGenericArguments()[0]))
                   select property;
        }

        internal static IEnumerable<string> GetRelationNames(this Type type)
        {
            IList<string> list = type
                .GetObjectProperties()
                .Select(GetPropertyMappingName)
                .ToList();

            foreach (
                ZuoraMappingAdditionAttribute attribute in
                    type.GetCustomAttributes(typeof(ZuoraMappingAdditionAttribute)))
            {
                list.Add(attribute.MappingName);
            }

            return list;
        }

        private static string GetPropertyMappingName(PropertyInfo info)
        {
            ZuoraNameAttribute attribute = info.GetCustomAttribute<ZuoraNameAttribute>();
            if (attribute == null || string.IsNullOrEmpty(attribute.MappingOverride))
                return info.PropertyType.IsGenericType
                    ? info.PropertyType.GenericTypeArguments[0].GetMappingName()
                    : info.PropertyType.GetMappingName();

            return attribute.MappingOverride ?? attribute.Name;
        }

        internal static IEnumerable<PropertyInfo> GetPrimitiveProperties(this Type type)
        {
            return from property in type.GetCachedProperties()
                let propertyType = property.PropertyType
                let zObjectType = typeof(ZObject)
                where !zObjectType.IsAssignableFrom(propertyType) && !propertyType.IsGenericType
                select property;
        }

        internal static string GetValue(this XmlNode node, string name)
        {
            var result = node[name];
            return result != null ? result.InnerText : null;
        }

        internal static PropertyInfo[] GetCachedProperties(this Type type)
        {
            lock (Lock)
            {
                PropertyInfo[] propertyInfos;

                if (typeof(ZObject).IsAssignableFrom(type))
                {
                    string name = type.Name;
                    if (PropertyInfos.ContainsKey(type.Name))
                    {
                        propertyInfos = PropertyInfos[name].ToArray();
                    }
                    else
                    {
                        propertyInfos = type.GetProperties()
                            .Where(x => x.GetCustomAttribute<ZuoraIgnoreAttribute>() == null)
                            .ToArray();
                        PropertyInfos.Add(name, propertyInfos);
                    }
                }
                else
                {
                    propertyInfos = type.GetProperties();
                }

                return propertyInfos;
            }
        }
    }
}