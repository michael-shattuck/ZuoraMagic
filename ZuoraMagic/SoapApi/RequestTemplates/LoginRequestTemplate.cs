using System;
using System.Xml.Serialization;
using ZuoraMagic.ORM.BaseRequestTemplates;

namespace ZuoraMagic.SoapApi.RequestTemplates
{
    [Serializable]
    public class LoginRequestTemplate
    {
        public LoginRequestTemplate()
        {
        }

        public LoginRequestTemplate(string username, string password)
        {
            Username = username;
            Password = password;
        }

        [XmlElement("username", Namespace = ZuoraNamespaces.Request)]
        public string Username { get; set; }

        [XmlElement("password", Namespace = ZuoraNamespaces.Request)]
        public string Password { get; set; }
    }
}