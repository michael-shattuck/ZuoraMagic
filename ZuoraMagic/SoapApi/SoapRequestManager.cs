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
        internal static string SoapUrl = "/apps/services/a/54.0";

        // TODO: Add configuration of endpoint
        internal static HttpRequest GetLoginRequest(ZuoraConfig config)
        {
            string url = config.IsSandbox ? "https://perfapps.zuora.com" : "https://www.zuora.com";
            HttpRequest request = new HttpRequest
            {
                Url = url + SoapUrl,
                Body = SoapCommands.Login(config.Username, config.Password + config.SecurityToken),
                Method = RequestType.POST,
            };
            request.Headers.Add("SOAPAction", "login");

            return request;
        }

        public static HttpRequest GetQueryRequest<T>(Expression<Func<T, bool>> predicate, int limit, ZuoraSession session) where T : ZObject
        {
            string query = QueryBuilder.GenerateQuery(predicate, limit);
            return GetQueryRequest(query, session);
        }

        public static HttpRequest GetQueryRequest(string query, ZuoraSession session)
        {
            HttpRequest request = new HttpRequest
            {
                Url = session.InstanceUrl + SoapUrl,
                Body = SoapCommands.Query(query, session.SessionId),
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
                Url = session.InstanceUrl + SoapUrl,
                Body = body,
                Method = RequestType.POST,
            };
            request.Headers.Add("SOAPAction", operation.OperationType.ToString().ToLower());

            return request;
        }
    }
}