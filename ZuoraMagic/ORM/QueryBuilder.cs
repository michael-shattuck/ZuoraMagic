using System;
using System.Linq.Expressions;
using ZuoraMagic.Entities;
using ZuoraMagic.Extensions;
using ZuoraMagic.LinqProvider;

namespace ZuoraMagic.ORM
{
    public static class QueryBuilder
    {
        public static string GenerateQuery<T>() where T : ZObject
        {
            Type type = typeof(T);
            string query = CompileSelectStatements(type);

            return query;
        }

        public static string GenerateQuery<T>(Expression<Func<T, bool>> predicate)
            where T : ZObject
        {
            Type type = typeof(T);
            string query = CompileSelectStatements(type);
            if (predicate != null) AddConditionsSet(ref query, predicate);

            return query;
        }

        private static string CompileSelectStatements(Type type)
        {
            return string.Format("SELECT {0} FROM {1}", CompilePropertyNames(type), type.GetName());
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

        public static string ValidateAndFlattenQuery<T>(string query)  where T : ZObject
        {
            Type type = typeof(T);
            if (query.Contains("*"))
            {
                query = query.Replace("*", CompilePropertyNames(type));
            }

            return query;
        }
    }
}