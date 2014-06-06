using ZuoraMagic.Configuration;
using ZuoraMagic.Entities;
using ZuoraMagic.Http;
using ZuoraMagic.ORM.BaseRequestTemplates;
using ZuoraMagic.SoapApi.Enum;
using ZuoraMagic.SoapApi.RequestTemplates;

namespace ZuoraMagic.SoapApi
{
    /// <summary>
    ///     Command generation for xml
    ///     soap commands.
    /// </summary>
    internal class SoapCommands
    {
        /// <summary>
        ///     Soap Command Generator for the
        ///     login command.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal static string Login(string username, string password)
        {
            return XmlRequestGenerator.GenerateRequest(new XmlBody
            {
                LoginTemplate = new LoginRequestTemplate(username, password)
            });
        }

        internal static string Query(string query, int limit, string sessionId)
        {
            XmlHeader header = GenerateHeader(sessionId);

            if (limit > 0)
            {
                header.QueryOptions = new QueryOptions
                {
                    BatchSize = limit
                };
            }

            return XmlRequestGenerator.GenerateRequest(new XmlBody
            {
                QueryTemplate = new QueryTemplate(query)
            }, header);
        }

        internal static string QueryMore(string queryLocator, int limit, string sessionId)
        {
            XmlHeader header = GenerateHeader(sessionId);

            if (limit > 0)
            {
                header.QueryOptions = new QueryOptions
                {
                    BatchSize = limit
                };
            }

            return XmlRequestGenerator.GenerateRequest(new XmlBody
            {
                QueryMoreTemplate = new QueryMoreTemplate(queryLocator)
            }, header);
        }

        internal static string CrudOperation<T>(CrudOperation<T> operation, string sessionId) where T : ZObject
        {
            return XmlRequestGenerator.GenerateRequest(GetCrudBody(operation), GenerateHeader(sessionId));
        }

        protected static XmlBody GetCrudBody<T>(CrudOperation<T> operation) where T : ZObject
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

        internal static XmlHeader GenerateHeader(string sessionId)
        {
            return new XmlHeader
            {
                SessionHeader = new SessionHeader
                {
                    SessionId = sessionId,
                }
            };
        }
    }
}
