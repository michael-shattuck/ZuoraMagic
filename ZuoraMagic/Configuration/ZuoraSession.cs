using System;

namespace ZuoraMagic.Configuration
{
    public class ZuoraSession
    {
        public string SessionId { get; set; }
        public string InstanceUrl { get; set; }
        public string SoapUrl { get; set; }
        public string Environment { get; set; }
        public bool IsSandbox { get; set; }
        public DateTime LastLogin { get; set; }
    }
}