using System;
using System.ComponentModel;

namespace ZuoraMagic.Configuration
{
    public class ZuoraConfig
    {
        private string soapUrl;

        public ZuoraSession Session { get; set; }
        public bool IsSandbox { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string InstanceUrl { get; set; }

        public string SoapUrl
        {
            get
            {
                // Default value present to prevent deprecating developers using the 2.9.9 and earlier implementations

                if (string.IsNullOrWhiteSpace(this.soapUrl))
                {
                    return "/apps/services/a/54.0";
                }

                return this.soapUrl;
            }

            set
            {
                this.soapUrl = value;
            }
        }

        public string EnvironmentName { get; set; }
        public bool LogoutOnDisposal { get; set; }
        public bool UseSessionStore { get; set; }
    }
}