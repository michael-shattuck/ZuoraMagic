using System.Collections.Generic;
using ZuoraMagic.SoapApi.Responses;

namespace ZuoraMagic
{
    public class ZuoraResponse
    {
        public ZuoraResponse()
        {
            Errors = new List<string>();
            Results = new List<RecordResult>();
        }

        public bool Success { get; set; }
        public IList<string> Errors { get; set; }
        public IList<RecordResult> Results { get; set; }
        public RecordResult Result { get; set; } 
    }
}