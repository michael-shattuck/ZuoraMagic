using System;
using System.Collections.Generic;
using System.Reflection;
using FastMember;
using ZuoraMagic.Attributes;
using ZuoraMagic.Entities;
using ZuoraMagic.Extensions;
using ZuoraMagic.ORM.Models;

namespace ZuoraMagic.ORM
{
    public static class ObjectHydrator
    {
        internal static object Lock = new object();
        internal static IDictionary<string, TypeAccessor> CachedAccessors = new Dictionary<string, TypeAccessor>();

        internal static TypeAccessor GetAccessor(Type type)
        {
            lock (Lock)
            {
                string name = type.Name;
                if (CachedAccessors.ContainsKey(name)) return CachedAccessors[name];
                TypeAccessor accessor = TypeAccessor.Create(type);
                CachedAccessors.Add(name, accessor);

                return accessor;
            }
        }

        internal static T ParseItem<T>(Type type, CsvRow row, TypeAccessor accessor, bool retrieveRelated)
            where T : ZObject
        {
            T obj = Activator.CreateInstance<T>();
            string typeName = type.GetMappingName();

            foreach (PropertyInfo property in type.GetCachedProperties())
            {
                string name = property.GetMappingName(typeName) + "." + property.GetName();
                if (!row.ContainsKey(name) || string.IsNullOrEmpty(row[name])) continue;
                SetProperty(obj, row[name], property, accessor);
            }

            if (retrieveRelated) ParseRelations(obj, type, row, accessor);

            return obj;
        }

        private static void ParseRelations<T>(T item, Type type, CsvRow row, TypeAccessor accessor, Type parent = null)
            where T : ZObject
        {
            foreach (PropertyInfo property in type.GetObjectProperties())
            {
                string mappingName = null;
                string propertyName = property.Name;
                Type propertyType = property.PropertyType;
                ZuoraNameAttribute attribute = property.GetCustomAttribute<ZuoraNameAttribute>();
                if (attribute != null && !string.IsNullOrEmpty(attribute.MappingOverride)) mappingName = attribute.MappingOverride;
                
                if (parent != null && property.PropertyType == parent) continue;
                object value;

                if (property.PropertyType.IsGenericType)
                {
                    propertyType = propertyType.GetGenericArguments()[0];
                    if (parent != null && property.PropertyType == parent) continue;
                    value = accessor[item, propertyName] ?? CreateGenericList(propertyType);
                    dynamic obj = ParseItem(propertyType, row, GetAccessor(propertyType), mappingName, type);
                    if (obj == null) continue;
                    dynamic actualValue = Cast(value, CreateGenericList(obj, CastList(obj, value)));
                    accessor[item, propertyName] = actualValue;
                    continue;
                }

                if (accessor[item, propertyName] != null) continue;
                value = ParseItem(propertyType, row, GetAccessor(propertyType), mappingName, type);
                if (value != null) accessor[item, propertyName] = value;
            }
        }

        private static dynamic CreateGenericList(Type type)
        {
            Type listType = typeof(List<>).MakeGenericType(type);
            return Activator.CreateInstance(listType);
        }

        private static IEnumerable<T> CreateGenericList<T>(T obj, IEnumerable<T> value)
        {
            return value != null
                ? new List<T>(value) { obj }
                : new List<T> { obj };
        }

        private static ZObject ParseItem(Type type, CsvRow row, TypeAccessor accessor, string typeName, Type parent = null)
        {
            object obj = Activator.CreateInstance(type);
            if (typeName == null) typeName = type.GetName();

            foreach (PropertyInfo property in type.GetCachedProperties())
            {
                string name = property.GetMappingName(typeName) + "." + property.GetName();
                if (property.Name == "Id" && !row.ContainsKey(name)) continue;
                if (!row.ContainsKey(name)) continue;

                string value = row[name];
                if (property.Name == "Id" && string.IsNullOrEmpty(value)) return null;
                if (string.IsNullOrEmpty(value)) continue;

                SetProperty(obj, value, property, accessor);
            }

            ParseRelations((ZObject)obj, type, row, accessor, parent);

            return (ZObject)obj;
        }

        internal static void SetProperty<T>(T obj, string value, PropertyInfo property, TypeAccessor accessor)
        {
            Type propertyType = property.PropertyType;
            string propertyName = property.Name;
            if (propertyType == typeof(string)) accessor[obj, propertyName] = value;
            if (propertyType == typeof(bool)) accessor[obj, propertyName] = Convert.ToBoolean(value);
            if (propertyType == typeof(int)) accessor[obj, propertyName] = Convert.ToInt32(value);
            if (propertyType == typeof(double)) accessor[obj, propertyName] = Convert.ToDouble(value);
            if (propertyType == typeof(decimal)) accessor[obj, propertyName] = Convert.ToDecimal(value);
            if (propertyType == typeof(DateTime)) accessor[obj, propertyName] = Convert.ToDateTime(value);
        }

        public static T Cast<T>(T previous, object obj)
        {
            return (T)obj;
        }

        public static IEnumerable<T> CastList<T>(T previous, object obj)
        {
            return (IEnumerable<T>)obj;
        }

        public static void CombineRelations<T>(T obj, Type type, CsvRow parser, TypeAccessor accessor)
            where T : ZObject
        {
            ParseRelations(obj, type, parser, accessor);
        }
    }
}