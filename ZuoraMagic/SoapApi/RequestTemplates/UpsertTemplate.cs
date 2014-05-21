using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using ZuoraMagic.Entities;
using ZuoraMagic.ORM.BaseRequestTemplates;

namespace ZuoraMagic.SoapApi.RequestTemplates
{
    [Serializable]
    public class UpsertTemplate
    {
        [XmlElement("ExternalIDFieldName", Namespace = ZuoraNamespaces.Request, Order = 1)]
        public string ExternalIdFieldName { get; set; }

        [XmlIgnore]
        public IEnumerable<ZObject> ZObjects { get; set; }

        [XmlElement("ZObjects", Namespace = ZuoraNamespaces.Request, Order = 2)]
        public List<ZObject> Items
        {
            get
            {
                return ZObjects.ToList();
            }
        }
    }
}   