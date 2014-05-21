using System;
using System.Xml.Serialization;
using ZuoraMagic.ORM.BaseRequestTemplates;

namespace ZuoraMagic.SoapApi.RequestTemplates
{
    [Serializable]
    public class CallOptions
    {
        [XmlElement("useSingleTransaction", Namespace = ZuoraNamespaces.Request)]
        public bool UseSingleTransaction { get; set; }
    }
}