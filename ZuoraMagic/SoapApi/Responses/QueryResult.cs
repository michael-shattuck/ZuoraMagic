using System.Collections.Generic;
using ZuoraMagic.Attributes;
using ZuoraMagic.Entities;

namespace ZuoraMagic.SoapApi.Responses
{
    public class QueryResult<T> where T : ZObject
    {
        [ZuoraName("records")]
        public IEnumerable<T> Records { get; set; }

        [ZuoraName("queryLocator")]
        public string QueryLocator { get; set; }

        [ZuoraName("done")]
        public bool Done { get; set; }
    }
}