using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using ZuoraMagic.Attributes;
using ZuoraMagic.ORM.BaseRequestTemplates;
using ZuoraMagic.SoapApi.Responses;

namespace ZuoraMagic
{
    public class ZuoraResponse
    {
        private bool? _success;

        public ZuoraResponse()
        {
            Errors = new List<string>();
            Results = new List<RecordResult>();
        }

        [ZuoraName("ns1:Success")]
        public bool Success
        {
            get
            {
                if ((_success != null && (bool) _success)) return (bool)_success;
                return Results.Count > 0 && Results.Any(x => x.Success);
            }
            set { _success = value; }
        }

        public IList<string> Errors { get; set; }
        public IList<RecordResult> Results { get; set; }
        public RecordResult Result { get; set; } 
    }
}