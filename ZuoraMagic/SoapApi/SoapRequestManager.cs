using System;
using System.Linq.Expressions;
using ZuoraMagic.Configuration;
using ZuoraMagic.Entities;
using ZuoraMagic.Http.Enums;
using ZuoraMagic.Http.Models;
using ZuoraMagic.ORM;
using ZuoraMagic.SoapApi.Models;

namespace ZuoraMagic.SoapApi
{
    internal class SoapRequestManager
    {
        internal static HttpRequest GetLoginRequest(ZuoraConfig config)
        {
            HttpRequest request = new HttpRequest
            {
                Url = config.InstanceUrl + config.SoapUrl,
                Body = SoapCommands.Login(config.Username, config.Password),
                Method = RequestType.POST,
            };
            request.Headers.Add("SOAPAction", "login");

            return request;
        }

        public static HttpRequest GetQueryRequest<T>(Expression<Func<T, bool>> predicate, int limit, ZuoraSession session) where T : ZObject
        {
            string query = QueryBuilder.GenerateQuery(predicate);
            return GetQueryRequest(query, limit, session);
        }

        public static HttpRequest GetQueryRequest(string query, int limit, ZuoraSession session)
        {
            HttpRequest request = new HttpRequest
            {
                Url = session.InstanceUrl + session.SoapUrl,
                Body = SoapCommands.Query(query, limit, session.SessionId),
                Method = RequestType.POST,
            };
            request.Headers.Add("SOAPAction", "query");

            return request;
        }

        public static HttpRequest GetCrudRequest<T>(CrudOperation<T> operation, ZuoraSession session) where T : ZObject
        {
            string body = SoapCommands.CrudOperation(operation, session.SessionId);
            HttpRequest request = new HttpRequest
            {
                Url = session.InstanceUrl + session.SoapUrl,
                Body = body,
                Method = RequestType.POST,
            };
            request.Headers.Add("SOAPAction", operation.OperationType.ToString().ToLower());

            return request;
        }

        public static HttpRequest GetQueryMoreRequest(string queryLocator, int limit, ZuoraSession session)
        {
            HttpRequest request = new HttpRequest
            {
                Url = session.InstanceUrl + session.SoapUrl,
                Body = SoapCommands.QueryMore(queryLocator, limit, session.SessionId),
                Method = RequestType.POST,
            };
            request.Headers.Add("SOAPAction", "queryMore");

            return request;
        }
    }
}