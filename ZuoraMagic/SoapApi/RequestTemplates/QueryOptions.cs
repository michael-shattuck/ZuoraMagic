using System;
using System.Xml.Serialization;
using ZuoraMagic.ORM.BaseRequestTemplates;

namespace ZuoraMagic.SoapApi.RequestTemplates
{
    [Serializable]
    public class QueryOptions
    {
        [XmlElement("batchSize", Namespace = ZuoraNamespaces.Request)]
        public int BatchSize { get; set; }

        [XmlElement("caseSensitive", Namespace = ZuoraNamespaces.Request)]
        public bool CaseSensitive { get; set; } 
    }
}