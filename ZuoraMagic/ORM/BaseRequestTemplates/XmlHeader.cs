using System;
using System.Xml.Serialization;

namespace ZuoraMagic.ORM.BaseRequestTemplates
{
    [Serializable]
    public partial class XmlHeader
    {
        [XmlElement("SessionHeader", Namespace = ZuoraNamespaces.Request)]
        public SessionHeader SessionHeader { get; set; }
    }
}