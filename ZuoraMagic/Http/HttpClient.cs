using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using ZuoraMagic.Exceptions;
using ZuoraMagic.Http.Enums;
using ZuoraMagic.Http.Models;

namespace ZuoraMagic.Http
{
    /// <summary>
    ///     Internal abstraction class for making
    ///     http calls.
    /// </summary>
    internal class HttpClient : IDisposable
    {
        /// <summary>
        ///     Abstraction method for performing a http request.
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="ZuoraRequestException"></exception>
        /// <returns></returns>
        internal XmlDocument PerformRequest(HttpRequest request)
        {
            WebRequest webRequest = GenerateWebRequest(request);

            using (WebResponse response = webRequest.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            {
                if (responseStream == null) throw new ZuoraRequestException("The request to {0} returned no response.", request.Url);

                XmlDocument xml = new XmlDocument();
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    xml.LoadXml(reader.ReadToEnd());

                    return xml;
                }
            }
        }

        /// <summary>
        ///     Method for generating a generic http request.
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="ZuoraRequestException"></exception>
        /// <returns></returns>
        private WebRequest GenerateWebRequest(HttpRequest request)
        {
            if (request.IsValid)
            {
                WebRequest webRequest = WebRequest.Create(request.Url);
                webRequest.Method = request.Method.ToString();
                foreach (KeyValuePair<string, string> header in request.Headers)
                {
                    webRequest.Headers.Add(header.Key, header.Value);
                }

                if (request.Method == RequestType.POST || request.Method == RequestType.PUT)
                    AddRequestBody(ref webRequest, request);

                return webRequest;
            }

            throw new ZuoraRequestException("Malformed request.");
        }

        /// <summary>
        ///     Method for adding a request body for put and post
        ///     http calls.
        /// </summary>
        /// <param name="webRequest"></param>
        /// <param name="request"></param>
        private void AddRequestBody(ref WebRequest webRequest, HttpRequest request)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(request.Body);
            webRequest.ContentType = request.ContentType ?? "text/xml; charset=UTF-8";
            webRequest.ContentLength = byteArray.Length;

            using (Stream requestBody = webRequest.GetRequestStream())
            {
                requestBody.Write(byteArray, 0, byteArray.Length);
                requestBody.Close();
            }
        }

        #region IDisposable Implementation
        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool safe)
        {
            // TODO: Implement IDisposable
        }

        ~HttpClient()
        {
            Dispose(false);
        }
        #endregion
    }
}