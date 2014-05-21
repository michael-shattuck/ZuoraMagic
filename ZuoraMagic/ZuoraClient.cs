using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using ZuoraMagic.Configuration;
using ZuoraMagic.Configuration.Abstract;
using ZuoraMagic.Entities;
using ZuoraMagic.Http;
using ZuoraMagic.Http.Models;
using ZuoraMagic.ORM;
using ZuoraMagic.SoapApi;

namespace ZuoraMagic
{
    public class ZuoraClient : IDisposable
    {
        #region Private Fields
        private readonly ISessionStoreProvider _sessionStore;
        private readonly ZuoraConfig _config;
        private static readonly object Lock = new object();
        #endregion

        /// <summary>
        ///     Constructor
        ///      - Uses default memory store
        ///      - Allows auto login
        /// </summary>
        /// <param name="config"></param>
        /// <param name="login"></param>
        public ZuoraClient(ZuoraConfig config, bool login = false)
            : this(config, new MemoryCacheProvider(), login)
        {
        }

        /// <summary>
        ///     Custom Constructor
        ///      - Uses user defined store
        ///      - Allows auto login
        /// </summary>
        /// <param name="config"></param>
        /// <param name="sessionStore"></param>
        /// <param name="login"></param>
        public ZuoraClient(ZuoraConfig config, ISessionStoreProvider sessionStore, bool login = false)
        {
            _sessionStore = sessionStore;
            _config = config;
            if (login) Login();
        }

        /// <summary>
        ///     Login Action
        ///      - Stores session data for re-use
        /// </summary>
        /// <returns></returns>
        public ZuoraSession Login()
        {
            lock (Lock)
            {
                ZuoraSession session;
                if (_config.UseSessionStore)
                {
                    session = _sessionStore.RetrieveSession(_config.Environment ?? "Default");
                    if (session != null) return session;
                }

                if (_config.Session != null) return _config.Session;

                using (HttpClient httpClient = new HttpClient())
                {
                    XmlDocument response = httpClient.PerformRequest(SoapRequestManager.GetLoginRequest(_config));
                    SimpleLogin result = ResponseReader.ReadGenericResponse<SimpleLogin>(response);

                    Uri instanceUrl = new Uri(result.ServerUrl);
                    session = new ZuoraSession
                    {
                        InstanceUrl = instanceUrl.Scheme + "://" + instanceUrl.Host,
                        SessionId = result.SessionId
                    };

                    if (_config.UseSessionStore) _sessionStore.StoreSession(session);
                    _config.Session = session;

                    return session;
                }
            }
        }

        public virtual IEnumerable<T> Query<T>(Expression<Func<T, bool>> predicate, int limit = 0) where T : ZObject
        {
            return PerformArrayRequest<T>(SoapRequestManager.GetQueryRequest(predicate, limit, Login()));
        }

        public virtual IEnumerable<T> Query<T>(string query)
        {
            // TODO: Validate query
            return PerformArrayRequest<T>(SoapRequestManager.GetQueryRequest(query, Login()));
        }

        public virtual T QuerySingle<T>(Expression<Func<T, bool>> predicate) where T : ZObject
        {
            return Query(predicate).FirstOrDefault();
        }

        public virtual T QuerySingle<T>(string query)
        {
            return Query<T>(query).FirstOrDefault();
        }

        #region Private Methods

        private T PerformGenericRequest<T>(HttpRequest request)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                XmlDocument response = httpClient.PerformRequest(request);
                return ResponseReader.ReadGenericResponse<T>(response);
            }
        }

        private ZuoraResponse PerformSimpleRequest(HttpRequest request)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                XmlDocument response = httpClient.PerformRequest(request);
                return ResponseReader.ReadSimpleResponse(response);
            }
        }

        private IEnumerable<T> PerformArrayRequest<T>(HttpRequest request)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                XmlDocument response = httpClient.PerformRequest(request);
                return ResponseReader.ReadArrayResponse<T>(response);
            }
        }

        #endregion

        #region Implementation of IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool safe)
        {
            if (safe && _config.LogoutOnDisposal)
            {
                // TODO: Logout
            }
        }

        ~ZuoraClient()
        {
            Dispose(false); // TODO: Logout anyway?
        }
        #endregion
    }
}