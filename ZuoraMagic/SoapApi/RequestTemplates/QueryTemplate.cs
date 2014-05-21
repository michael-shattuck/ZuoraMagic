using System;
using System.Xml.Serialization;
using ZuoraMagic.ORM.BaseRequestTemplates;

namespace ZuoraMagic.SoapApi.RequestTemplates
{
    [Serializable]
    public class QueryTemplate
    {
        public QueryTemplate()
        {
        }

        public QueryTemplate(string queryString)
        {
            QueryString = queryString;
        }

        [XmlElement("queryString", Namespace = ZuoraNamespaces.Request)]
        public string QueryString { get; set; }
    }
}