using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using FastMember;
using LumenWorks.Framework.IO.Csv;
using ZuoraMagic.Entities;
using ZuoraMagic.Exceptions;
using ZuoraMagic.Extensions;
using ZuoraMagic.ORM.BaseRequestTemplates;
using ZuoraMagic.ORM.Models;
using ZuoraMagic.SoapApi.Responses;

namespace ZuoraMagic.ORM
{
    internal static class ResponseReader
    {
        internal static ZuoraResponse ReadSimpleResponse(XmlDocument document)
        {
            ZuoraResponse response = new ZuoraResponse();
            XmlNodeList results = GetNamedNodes(document, "result");

            if (results.Count > 1)
            {
                foreach (XmlNode node in results)
                {
                    RecordResult result = ReadSimpleResponse<RecordResult>(node, document);
                    response.Results.Add(result);

                    if (!result.Success)
                    {
                        response.Errors.Add(result.Message);
                    }
                }
            }
            else
            {
                response.Result = ReadSimpleResponse<RecordResult>(results[0], document);
            }

            return response;
        }

        internal static T ReadGenericResponse<T>(XmlDocument document)
        {
            ValidateDocument(document);
            return ReadSimpleResponse<T>(GetNamedNodes(document, "result")[0], document);
        }

        private static void ValidateDocument(XmlDocument document)
        {
            XmlNode node = GetNamedNode(document, "Success");
            if (node != null && node.InnerText == "false")
            {
                XmlNodeList errorMessageNodes = GetNamedNodes(document, "Message");
                IList<XmlNode> errors = new List<XmlNode>(errorMessageNodes.Cast<XmlNode>());
                string message = errorMessageNodes.Count > 0
                    ? "The following errors occurred: " + string.Join(", ", errors.Select(x => x.InnerText))
                    : null;

                throw new ZuoraRequestException(message);
            }
        }

        internal static T ReadSimpleResponse<T>(XmlNode node, XmlDocument document)
        {
            Type type = typeof (T);
            bool ns = type.BaseType == typeof (ZObject);
            T obj = Activator.CreateInstance<T>();
            TypeAccessor accessor = ObjectHydrator.GetAccessor(type);

            foreach (PropertyInfo property in type.GetCachedProperties())
            {
                Type propertyType = property.PropertyType;
                string name = property.GetName();
                if (name == "records")
                {
                    // Not sure I like this. I might want to create a new response reader
                    accessor[obj, property.Name] = ReadArrayResponse(Activator.CreateInstance(propertyType), document);
                }
                else
                {
                    if (ns) name = "ns2:" + name;
                    string value = node.GetValue(name);

                    if (value == null)
                    {
                        IEnumerable<XElement> nodes = GetNamedNodes(node, name);
                        if (nodes == null || !nodes.Any()) continue;
                        XElement child = nodes.FirstOrDefault();
                        if (child == null) continue;

                        value = child.Value;
                    }

                    ObjectHydrator.SetProperty(obj, value, property, accessor);
                }
            }

            return obj;
        }

        internal static T[] ReadArrayResponse<T>(XmlDocument document)
        {
            return
                (from XmlNode node in GetNamedNodes(document, "records") select ReadSimpleResponse<T>(node, document))
                    .ToArray();
        }

        internal static T[] ReadArrayResponse<T>(T dummy, XmlDocument document)
        {
            return
                (from XmlNode node in GetNamedNodes(document, "records") select ReadSimpleResponse<T>(node, document))
                    .ToArray();
        }

        internal static Stream ReadStream(string url, string username, string password)
        {
            WebClient webClient = new WebClient
            {
                Credentials = new NetworkCredential(username, password)
            };

            return webClient.OpenRead(url);
        }

        private static XmlNodeList GetNamedNodes(XmlDocument document, string name)
        {
            var xnsMgr = new XmlNamespaceManager(document.NameTable);
            xnsMgr.AddNamespace("z", ZuoraNamespaces.Request);
            xnsMgr.PushScope();
            return document.SelectNodes(string.Format("//z:{0}[count(*)>0]", name), xnsMgr);
        }

        private static XmlNode GetNamedNode(XmlDocument document, string name)
        {
            XmlNodeList nodes = GetNamedNodes(document, name);
            return nodes.Count > 0 ? nodes[0] : null;
        }

        private static XElement[] GetNamedNodes(XmlNode node, string name)
        {
            XDocument document = XDocument.Parse(node.OuterXml);
            return document.Descendants().Where(x => x.Name.LocalName == name).ToArray();
        }

        internal static IEnumerable<CsvRow> ReadExportData(Stream stream)
        {
            using (StreamReader streamReader = new StreamReader(stream))
            using (CsvReader parser = new CsvReader(streamReader, true))
            {
                string[] headers = parser.GetFieldHeaders();
                IEnumerable<CsvRow> data = parser.Select(x => new CsvRow(x, headers)).ToArray();
                stream.Close();
                
                return data;
            }
        }

        internal static IEnumerable<T> ReadExportRecords<T>(Stream stream, bool retrieveRelated) where T : ZObject
        {
            IEnumerable<CsvRow>  data = ReadExportData(stream);

            Type type = typeof(T);
            string name = type.GetName();
            TypeAccessor accessor = ObjectHydrator.GetAccessor(type);
            List<T> records = new List<T>();

            Parallel.ForEach(data.GroupBy(x => x[name + ".Id"]), itemData =>
            {
                T item = null;
                foreach (CsvRow row in itemData)
                {
                    if (item == null) item = ObjectHydrator.ParseItem<T>(type, row, accessor, retrieveRelated);
                    ObjectHydrator.CombineRelations(item, type, row, accessor);
                }
                records.Add(item);
            });

            return records;
        }
    }
}