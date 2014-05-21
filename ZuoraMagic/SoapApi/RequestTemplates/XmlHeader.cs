using System.Xml.Serialization;
using ZuoraMagic.SoapApi.RequestTemplates;

namespace ZuoraMagic.ORM.BaseRequestTemplates
{
    public partial class XmlHeader
    {
        [XmlElement("QueryOptions", Namespace = ZuoraNamespaces.Request)]
        public QueryOptions QueryOptions { get; set; }

        [XmlElement("CallOptions", Namespace = ZuoraNamespaces.Request)]
        public CallOptions CallOptions { get; set; }
    }
}