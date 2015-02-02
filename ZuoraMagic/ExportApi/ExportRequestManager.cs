using System;
using System.Linq.Expressions;
using ZuoraMagic.Configuration;
using ZuoraMagic.Entities;
using ZuoraMagic.ExportApi.Models;
using ZuoraMagic.Http.Enums;
using ZuoraMagic.Http.Models;
using ZuoraMagic.ORM;
using ZuoraMagic.SoapApi;

namespace ZuoraMagic.ExportApi
{
    internal class ExportRequestManager : SoapRequestManager
    {
        public static HttpRequest GetCreateExportRequest(string query, ZuoraSession session, string name = "Export")
        {
            HttpRequest request = new HttpRequest
            {
                Url = session.InstanceUrl + session.SoapUrl,
                Body = ExportCommands.CreateExport(new Export
                {
                    Format = "csv",
                    Name = name,
                    Query = query
                }, session.SessionId),
                Method = RequestType.POST,
            };
            request.Headers.Add("SOAPAction", "create");

            return request;
        }
    }
}