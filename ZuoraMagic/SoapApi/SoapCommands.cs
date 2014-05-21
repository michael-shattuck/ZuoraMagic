using ZuoraMagic.Configuration;
using ZuoraMagic.Entities;
using ZuoraMagic.Http;
using ZuoraMagic.ORM.BaseRequestTemplates;
using ZuoraMagic.SoapApi.Enum;
using ZuoraMagic.SoapApi.Models;
using ZuoraMagic.SoapApi.RequestTemplates;

namespace ZuoraMagic.SoapApi
{
    internal static class SoapCommands
    {
        internal static string Login(string username, string password)
        {
            return XmlRequestGenerator.GenerateRequest(new XmlBody
            {
                LoginTemplate = new LoginRequestTemplate(username, password)
            });
        }

        internal static string Query(QueryRequest query, string sessionId)
        {
            return XmlRequestGenerator.GenerateRequest(new XmlBody
            {
                QueryTemplate = new QueryTemplate(query.QueryString)
            },
            new XmlHeader
            {
                SessionHeader = new SessionHeader
                {
                    SessionId = sessionId,
                },
                QueryOptions = new QueryOptions
                {
                    BatchSize = query.BatchSize,
                    CaseSensitive = query.CaseSensative
                }
            });
        }

        public static string CrudOperation<T>(CrudOperation<T> operation, string sessionId) where T : ZObject
        {
            XmlBody body = GetCrudBody(operation);
            return XmlRequestGenerator.GenerateRequest(body, new XmlHeader
            {
                SessionHeader = new SessionHeader
                {
                    SessionId = sessionId
                }
            });
        }

        private static XmlBody GetCrudBody<T>(CrudOperation<T> operation) where T : ZObject
        {
            XmlBody body = new XmlBody();

            switch (operation.OperationType)
            {
                case CrudOperations.Insert:
                    body.InsertTemplate = new BasicCrudTemplate
                    {
                        ZObjects = operation.Items
                    };
                    break;
                case CrudOperations.Upsert:
                    body.UpsertTemplate = new UpsertTemplate
                    {
                        ZObjects = operation.Items,
                        ExternalIdFieldName = operation.ExternalIdField
                    };
                    break;
                case CrudOperations.Update:
                    body.UpdateTemplate = new BasicCrudTemplate
                    {
                        ZObjects = operation.Items
                    };
                    break;
                case CrudOperations.Delete:
                    body.DeleteTemplate = new DeleteTemplate
                    {
                        ZObjects = operation.Items
                    };
                    break;
            }

            return body;
        }
    }
}
