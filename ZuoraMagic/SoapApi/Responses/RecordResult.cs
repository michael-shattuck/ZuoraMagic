using ZuoraMagic.Attributes;

namespace ZuoraMagic.SoapApi.Responses
{
    public class RecordResult
    {
        [ZuoraName("ns1:Id")]
        public string Id { get; set; }

        [ZuoraName("ns1:Success")]
        public bool Success { get; set; }

        [ZuoraName("ns1:Message")]
        public string Message { get; set; }
    }
}