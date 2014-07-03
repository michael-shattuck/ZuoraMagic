using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using ZuoraMagic.Attributes;
using ZuoraMagic.Configuration;
using ZuoraMagic.Entities;
using ZuoraMagic.Extensions;
using ZuoraMagic.LinqProvider;

namespace ZuoraMagic.ORM
{
    public static class QueryBuilder
    {
        internal static string GenerateExportQuery<T>(Expression<Func<T, bool>> predicate, ZuoraExportOptions options)
            where T : ZObject
        {
            Type type = typeof(T);
            string query = CompileExportSelectStatements(type, options.RetrieveRelated, options.RetrieveSpecificData);
            if (predicate != null) AddConditionsSet(ref query, predicate);
            if (options.Index != null && options.Limit > 0) AddOffsetLimit(ref query, (int)options.Index, options.Limit);
            else if (options.Limit > 0) AddLimit(ref query, options.Limit);

            return query;
        }

        internal static string GenerateQuery<T>()
            where T : ZObject
        {
            Type type = typeof (T);
            string query = CompileSelectStatements(type);

            return query;
        }

        internal static string GenerateQuery<T>(Expression<Func<T, bool>> predicate)
            where T : ZObject
        {
            Type type = typeof (T);
            string query = CompileSelectStatements(type);
            if (predicate != null) AddConditionsSet(ref query, predicate);

            return query;
        }

        private static void AddOffsetLimit(ref string query, int index, int limit)
        {
            query = query + " LIMIT " + index + "," + limit;
        }

        private static void AddLimit(ref string query, int limit)
        {
            query = query + " LIMIT " + limit;
        }

        private static string CompileSelectStatements(Type type)
        {
            return string.Format("SELECT {0} FROM {1}", CompilePropertyNames(type), type.GetName());
        }

        private static string CompileExportSelectStatements(Type type, bool retrieveRelated, bool retrieveSpecific)
        {
            string name = type.GetName();
            if (retrieveRelated)
            {
                string propertySelectString;

                if (retrieveSpecific)
                {
                    string[] properties = GetSpecificObjectNames(type, new List<string>()).ToArray();
                    propertySelectString = CompilePropertyNames(properties);
                }
                else
                {
                    string[] properties = GetObjectNames(type, new List<string>()).ToArray();
                    propertySelectString = name + ".*";
                    propertySelectString = properties.Aggregate(propertySelectString, (current, t) => current + string.Format(", {0}.*", t));
                }

                return string.Format("SELECT {0} FROM {1}", propertySelectString, type.GetName());
            }

            return string.Format("SELECT {0}.* FROM {0}", name);
        }

        private static IEnumerable<string> GetObjectNames(Type type, List<string> list, Type parent = null)
        {
            list.AddRange(type.GetRelationNames().Where(x => !list.Contains(x)).ToList());
            foreach (Type propertyType in type.GetObjectProperties().Select(property => property.PropertyType.IsGenericType
                ? property.PropertyType.GetGenericArguments()[0]
                : property.PropertyType).Where(propertyType => parent == null || propertyType != parent))
            {
                list.AddRange(GetObjectNames(propertyType, list, type).Where(x => !list.Contains(x)));
            }

            return list;
        }

        private static IEnumerable<string> GetSpecificObjectNames(Type type, List<string> list, Type parent = null)
        {
            list.AddRange(GetSpecificProperties(type).Where(x => !list.Contains(x)));
            foreach (Type propertyType in type.GetObjectProperties().Select(property => property.PropertyType.IsGenericType
                ? property.PropertyType.GetGenericArguments()[0]
                : property.PropertyType).Where(propertyType => parent == null || propertyType != parent))
            {
                list.AddRange(GetSpecificObjectNames(propertyType, list, type));
            }

            return list;
        }

        private static void AddConditionsSet<T>(ref string query, Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
                query = query + " WHERE " + ZOQLVisitor.ConvertToSOQL(predicate);
        }

        private static string CompilePropertyNames(Type type)
        {
            return string.Join(", ", type.GetPropertyNames());
        }

        private static string CompilePropertyNames(IEnumerable<string> names)
        {
            return string.Join(", ", names);
        }

        private static IEnumerable<string> GetSpecificProperties(Type type)
        {
            string name = type.GetName();
            IList<string> names = new List<string>();
            PropertyInfo[] propertyNames = type.GetPrimitiveProperties().ToArray();
            foreach (string selectName in propertyNames.Select(info => info.GetSelectName(name)).Where(selectName => !names.Contains(selectName)))
            {
                names.Add(selectName);
            }

            return names;
        }

        internal static string ValidateAndFlattenQuery<T>(string query) where T : ZObject
        {
            Type type = typeof (T);
            if (query.Contains("*"))
            {
                query = query.Replace("*", CompilePropertyNames(type));
            }

            return query;
        }
    }
}