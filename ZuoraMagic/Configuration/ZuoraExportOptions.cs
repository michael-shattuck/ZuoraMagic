namespace ZuoraMagic.Configuration
{
    public class ZuoraExportOptions
    {
        public bool RetrieveSpecificData { get; set; }
        public int WaitTime { get; set; }
        public int? Timeout { get; set; }
        public bool ReRunOnTimeout { get; set; }
        public bool ReRunOnFailure { get; set; }
        public int Limit { get; set; }
        public int? Index { get; set; }
        public bool RetrieveRelated { get; set; }

        public ZuoraExportOptions()
        {
            WaitTime = 10000;
        }
    }
}