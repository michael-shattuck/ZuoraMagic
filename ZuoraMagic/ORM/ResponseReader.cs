using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using FastMember;
using LumenWorks.Framework.IO.Csv;
using ZuoraMagic.Entities;
using ZuoraMagic.Extensions;
using ZuoraMagic.ORM.BaseRequestTemplates;
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
            return ReadSimpleResponse<T>(GetNamedNodes(document, "result")[0], document);
        }

        internal static T ReadSimpleResponse<T>(XmlNode node, XmlDocument document)
        {
            Type type = typeof(T);
            bool ns = type.BaseType == typeof(ZObject);
            T obj = Activator.CreateInstance<T>();
            TypeAccessor accessor = ObjectHydrator.GetAccessor(type);

            foreach (PropertyInfo property in type.GetProperties())
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
            return (from XmlNode node in GetNamedNodes(document, "records") select ReadSimpleResponse<T>(node, document)).ToArray();
        }

        internal static T[] ReadArrayResponse<T>(T dummy, XmlDocument document)
        {
            return (from XmlNode node in GetNamedNodes(document, "records") select ReadSimpleResponse<T>(node, document)).ToArray();
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
            return document.GetElementsByTagName(name, ZuoraNamespaces.Request);
        }

        private static XElement[] GetNamedNodes(XmlNode node, string name)
        {
            XDocument document = XDocument.Parse(node.OuterXml);
            return document.Descendants().Where(x => x.Name.LocalName == name).ToArray();
        }

        internal static IEnumerable<IDictionary<string, object>> ReadExportData(Stream stream)
        {
            using (StreamReader streamReader = new StreamReader(stream))
            using (CsvReader parser = new CsvReader(streamReader, true))
            {
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                string[] headers = parser.GetFieldHeaders();
                int fieldCount = parser.FieldCount;

                while (parser.ReadNextRecord())
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    for (int i = 0; i < fieldCount; i++)
                    {
                        row.Add(headers[i], parser[i]);
                    }
                    data.Add(row);
                }

                stream.Close();
                return data;
            }
        }

        internal static IEnumerable<T> ReadExportRecords<T>(Stream stream) where T : ZObject
        {
            using (StreamReader streamReader = new StreamReader(stream))
            using (CsvReader parser = new CsvReader(streamReader, true))
            {
                Type type = typeof (T);
                TypeAccessor accessor = ObjectHydrator.GetAccessor(type);
                Dictionary<string, T> records = new Dictionary<string, T>();

                while (parser.ReadNextRecord())
                {
                    string id = parser[type.GetName() + ".Id"];
                    if (!records.ContainsKey(id))
                    {
                        T item = ObjectHydrator.ParseItem<T>(type, parser, accessor);
                        records.Add(id, item);
                    }
                    else
                    {
                        ObjectHydrator.ParseItem(records[id], type, parser, accessor);
                    }
                }

                stream.Close();
                return records.Select(x => x.Value);
            }
        }
    }
}