using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using ZuoraMagic.Entities;
using ZuoraMagic.ORM.BaseRequestTemplates;

namespace ZuoraMagic.SoapApi.RequestTemplates
{
    [Serializable]
    public class DeleteTemplate
    {
        [XmlIgnore]
        public IEnumerable<ZObject> ZObjects { get; set; }

        [XmlElement("ids", Namespace = ZuoraNamespaces.Request)]
        public string[] Ids
        {
            get { return ZObjects.Select(x => x.Id).ToArray(); }
            set { } // Apparently this is needed for XML serialization...
        }
    }
}