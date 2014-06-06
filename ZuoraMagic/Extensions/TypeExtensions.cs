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
        internal static IEnumerable<string> GetPropertyNames(this Type type)
        {
            return type.GetProperties().GetNames();
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
                ? info.GetCustomAttribute<ZuoraNameAttribute>().Name
                : info.Name;
        }

        internal static string GetName(this Type type)
        {
            ZuoraNameAttribute attribute = type.GetCustomAttribute<ZuoraNameAttribute>();
            return attribute != null ? attribute.Name : type.Name;
        }

        internal static IEnumerable<PropertyInfo> GetObjectProperties(this Type type)
        {
            return from property in type.GetProperties()
                let propertyType = property.PropertyType
                let zObjectType = typeof(ZObject)
                where zObjectType.IsAssignableFrom(propertyType)
                || (propertyType.IsGenericType && zObjectType.IsAssignableFrom(propertyType.GetGenericArguments()[0]))
                select property;
        }

        internal static IEnumerable<PropertyInfo> GetPrimitiveProperties(this Type type)
        {
            return from property in type.GetProperties()
                   let propertyType = property.PropertyType
                   let zObjectType = typeof(ZObject)
                   where !zObjectType.IsAssignableFrom(propertyType) || !propertyType.IsGenericType
                   select property;
        }

        internal static string GetValue(this XmlNode node, string name)
        {
            var result = node[name];
            return result != null ? result.InnerText : null;
        }
    }
}