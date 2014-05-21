using ZuoraMagic.Attributes;

namespace ZuoraMagic.SoapApi.Responses
{
    public class RecordResult
    {
        [ZuoraName("id")]
        public string Id { get; set; }

        [ZuoraName("success")]
        public bool Success { get; set; }

        [ZuoraName("message")]
        public string Message { get; set; }
    }
}