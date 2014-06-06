using System;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using FastMember;
using ZuoraMagic.Extensions;
using ZuoraMagic.ORM;
using ZuoraMagic.ORM.BaseRequestTemplates;

namespace ZuoraMagic.Entities
{
    public abstract class ZObject : IXmlSerializable
    {
        public string Id { get; set; }

        public XmlSchema GetSchema() { return null; }

        public void ReadXml(XmlReader reader) { }

        public void WriteXml(XmlWriter writer)
        {
            // TODO: Implement robust serialization
            Type type = GetType();
            TypeAccessor accessor = ObjectHydrator.GetAccessor(type);
            writer.WriteAttributeString("type", ZuoraNamespaces.Type, "obj:" + type.GetName());

            foreach (PropertyInfo info in type.GetCachedProperties())
            {
                var value = accessor[this, info.Name];
                if (value != null) writer.WriteElementString(info.GetName(), ZuoraNamespaces.ZObject, value.ToString());
            }
        }
    }
}