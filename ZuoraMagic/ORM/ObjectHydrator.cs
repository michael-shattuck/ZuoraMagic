using System;
using System.Collections.Generic;
using System.Reflection;
using FastMember;
using LumenWorks.Framework.IO.Csv;
using ZuoraMagic.Entities;
using ZuoraMagic.Extensions;

namespace ZuoraMagic.ORM
{
    public static class ObjectHydrator
    {
        internal static IDictionary<string, TypeAccessor> CachedAccessors = new Dictionary<string, TypeAccessor>();

        internal static TypeAccessor GetAccessor(Type type)
        {
            string name = type.Name;
            if (CachedAccessors.ContainsKey(name)) return CachedAccessors[name];
            TypeAccessor accessor = TypeAccessor.Create(type);
            CachedAccessors.Add(name, accessor);

            return accessor;
        }

        internal static T ParseItem<T>(Type type, CsvReader parser, TypeAccessor accessor, bool retrieveRelated)
            where T : ZObject
        {
            T obj = Activator.CreateInstance<T>();

            foreach (PropertyInfo property in type.GetCachedProperties())
            {
                string name = type.GetName() + "." + property.GetName();
                string value;
                try
                {
                    value = parser[name];
                    if (value == null) continue;
                }
                catch (Exception)
                {
                    continue;
                }

                SetProperty(obj, value, property, accessor);
            }

            if (retrieveRelated) ParseRelations(obj, type, parser, accessor);

            return obj;
        }

        private static void ParseRelations<T>(T item, Type type, CsvReader parser, TypeAccessor accessor)
            where T : ZObject
        {
            foreach (PropertyInfo property in type.GetObjectProperties())
            {
                string propertyName = property.Name;
                Type propertyType = property.PropertyType;
                object value;

                if (property.PropertyType.IsGenericType)
                {
                    propertyType = propertyType.GetGenericArguments()[0];
                    value = accessor[item, propertyName] ?? CreateGenericList(propertyType);
                    dynamic obj = ParseItem(propertyType, parser, GetAccessor(propertyType));
                    dynamic actualValue = Cast(value, CreateGenericList(obj, CastList(obj, value)));
                    accessor[item, propertyName] = actualValue;
                    continue;
                }

                if (accessor[item, propertyName] != null) continue;
                value = ParseItem(propertyType, parser, GetAccessor(propertyType));
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

        private static ZObject ParseItem(Type type, CsvReader parser, TypeAccessor accessor)
        {
            object obj = Activator.CreateInstance(type);

            foreach (PropertyInfo property in type.GetPrimitiveProperties())
            {
                string name = type.GetName() + "." + property.GetName();
                string value;
                try
                {
                    value = parser[name];
                    if (property.Name == "Id" && string.IsNullOrEmpty(value)) return null;
                    if (value == null) continue;
                }
                catch (Exception)
                {
                    if (property.Name == "Id") return null;
                    continue;
                }

                SetProperty(obj, value, property, accessor);
            }

            ParseRelations((ZObject)obj, type, parser, accessor);

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

        public static void CombineRelations<T>(T obj, Type type, CsvReader parser, TypeAccessor accessor)
            where T : ZObject
        {
            ParseRelations(obj, type, parser, accessor);
        }
    }
}