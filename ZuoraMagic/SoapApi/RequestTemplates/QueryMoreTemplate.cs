using System;
using System.Xml.Serialization;
using ZuoraMagic.ORM.BaseRequestTemplates;

namespace ZuoraMagic.SoapApi.RequestTemplates
{
    [Serializable]
    public class QueryMoreTemplate
    {
        public QueryMoreTemplate()
        {
        }

        public QueryMoreTemplate(string querylocator)
        {
            QueryLocator = querylocator;
        }

        [XmlElement("queryLocator", Namespace = ZuoraNamespaces.Request)]
        public string QueryLocator { get; set; }
    }
}