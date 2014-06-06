using ZuoraMagic.Configuration;
using ZuoraMagic.ExportApi.Models;
using ZuoraMagic.Http;
using ZuoraMagic.SoapApi;
using ZuoraMagic.SoapApi.Enum;

namespace ZuoraMagic.ExportApi
{
    internal class ExportCommands : SoapCommands
    {
        internal static string CreateExport(Export export, string sessionId)
        {
            return XmlRequestGenerator.GenerateRequest(GetCrudBody(new CrudOperation<Export>
            {
                OperationType = CrudOperations.Insert,
                Items = new[] { export }
            }), GenerateHeader(sessionId));
        }
    }
}