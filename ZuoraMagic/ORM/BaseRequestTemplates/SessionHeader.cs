using System;
using System.Xml.Serialization;

namespace ZuoraMagic.ORM.BaseRequestTemplates
{
    [Serializable]
    public class SessionHeader
    {
        [XmlElement("session", Namespace = ZuoraNamespaces.Request)]
        public string SessionId { get; set; } 
    }
}