using System;
using System.Runtime.Serialization;
using System.Xml.Linq;
using ZuoraMagic.Entities;

namespace ZuoraMagic.ExportApi.Models
{
    [Serializable]
    internal class Export : ZObject
    {
        public string Format { get; set; }
        public string Name { get; set; }
        public string Query { get; set; }
        public string Status { get; set; }
        public string FileId { get; set; }
    }
}