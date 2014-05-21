using ZuoraMagic.Attributes;

namespace ZuoraMagic.Http.Models
{
    public class SimpleLogin
    {
        [ZuoraName("Session")]
        public string SessionId { get; set; }
        
        public string ServerUrl { get; set; } 
    }
}