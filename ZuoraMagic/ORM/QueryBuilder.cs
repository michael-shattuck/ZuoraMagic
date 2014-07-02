using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ZuoraMagic.Configuration;
using ZuoraMagic.Entities;
using ZuoraMagic.Extensions;
using ZuoraMagic.LinqProvider;

namespace ZuoraMagic.ORM
{
    public static class QueryBuilder
    {
        internal static string GenerateExportQuery<T>(bool retrieveRelated, int limit = 0) where T : ZObject
        {
            Type type = typeof(T);
            string query = CompileExportSelectStatements(type, retrieveRelated);
            if (limit > 0) AddLimit(ref query, limit);

            return query;
        }


        internal static string GenerateExportQuery<T>(Expression<Func<T, bool>> predicate, ZuoraExportOptions options)
            where T : ZObject
        {
            Type type = typeof(T);
            string query = CompileExportSelectStatements(type, options.RetrieveRelated);
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

        private static string CompileExportSelectStatements(Type type, bool retrieveRelated)
        {
            if (retrieveRelated)
            {
                string[] properties = GetObjectNames(type, new List<string>()).ToArray();
                string propertySelectString = string.Empty;

                for (int i = 0; i < properties.Length; i++)
                {
                    if (i < properties.Length - 1)
                        propertySelectString = propertySelectString + string.Format("{0}.*, ", properties[i]);
                    else
                        propertySelectString = propertySelectString + string.Format("{0}.*", properties[i]);
                }

                return string.Format("SELECT {0} FROM {1}", propertySelectString, type.Name);
            }

            return string.Format("SELECT {0}.* FROM {0}", type.Name);
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

        private static void AddConditionsSet<T>(ref string query, Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
                query = query + " WHERE " + ZOQLVisitor.ConvertToSOQL(predicate);
        }

        private static string CompilePropertyNames(Type type)
        {
            return string.Join(", ", type.GetPropertyNames());
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