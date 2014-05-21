using System;
using System.Xml.Serialization;

namespace ZuoraMagic.ORM.BaseRequestTemplates
{
    [Serializable]
    [XmlRoot("Envelope", Namespace = ZuoraNamespaces.Envelope)]
    public class XmlRequest
    {
        [XmlElement("Header", Namespace = ZuoraNamespaces.Envelope)]
        public XmlHeader Header { get; set; }

        [XmlElement("Body", Namespace = ZuoraNamespaces.Envelope)]
        public XmlBody Body { get; set; }
    }
}