using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using ZuoraMagic.Entities;
using ZuoraMagic.ORM.BaseRequestTemplates;

namespace ZuoraMagic.SoapApi.RequestTemplates
{
    [Serializable]
    public class BasicCrudTemplate
    {
        [XmlIgnore]
        public IEnumerable<ZObject> ZObjects { get; set; }

        [XmlElement("ZObjects", Namespace = ZuoraNamespaces.Request)]
        public List<ZObject> Items
        {
            get
            {
                return ZObjects.ToList();
            } 
        }
    }
}