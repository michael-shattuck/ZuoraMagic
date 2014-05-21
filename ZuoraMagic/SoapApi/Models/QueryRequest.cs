namespace ZuoraMagic.SoapApi.Models
{
    public class QueryRequest
    {
        public string QueryString { get; set; }
        public bool CaseSensative { get; set; }
        public int BatchSize { get; set; }
    }
}