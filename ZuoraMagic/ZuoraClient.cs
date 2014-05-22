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
using ZuoraMagic.SoapApi.Responses;

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

        /// <summary>
        ///     Generic predicate query method.
        ///      - Defaults to a limit of 2000. If a higher
        ///        limit is specified, ZuoraMagic will implement the
        ///        'queryMore' SOAPAction to capture all results.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> Query<T>(Expression<Func<T, bool>> predicate, int limit = 0) where T : ZObject
        {
            return Query<T>(QueryBuilder.GenerateQuery(predicate), limit);
        }

        /// <summary>
        ///     Generic string query method
        ///      - Defaults to a limit of 2000. If a higher
        ///        limit is specified, ZuoraMagic will implement the
        ///        'queryMore' SOAPAction to capture all results.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> Query<T>(string query, int limit = 0) where T : ZObject
        {
            query = QueryBuilder.ValidateAndFlattenQuery<T>(query);

            return 2000 > limit 
                ? PerformArrayRequest<T>(SoapRequestManager.GetQueryRequest(query, limit, Login())) 
                : PerformAdvancedQuery<T>(query, limit);
        }

        /// <summary>
        ///     Generic predicate query method.
        ///     Returns one record
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual T QuerySingle<T>(Expression<Func<T, bool>> predicate) where T : ZObject
        {
            return Query(predicate, 1).FirstOrDefault();
        }

        /// <summary>
        ///     Generic string query method.
        ///     Returns one record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual T QuerySingle<T>(string query) where T : ZObject
        {
            return Query<T>(query, 1).FirstOrDefault();
        }

        /// <summary>
        ///     Generic string query method implementing
        ///     deeper Zuora API features. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public virtual QueryResult<T> PerformQuery<T>(string query, int limit = 0) where T : ZObject
        {
            return PerformGenericRequest<QueryResult<T>>(SoapRequestManager.GetQueryRequest(query, limit, Login()));
        }

        /// <summary>
        ///     Generic predicate query method implementing
        ///     deeper Zuora API features. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public virtual QueryResult<T> PerformQuery<T>(Expression<Func<T, bool>> predicate, int limit = 0) where T : ZObject
        {
            return PerformGenericRequest<QueryResult<T>>(SoapRequestManager.GetQueryRequest(predicate, limit, Login()));
        }

        /// <summary>
        ///     Generic query more method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryLocator"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public virtual QueryResult<T> PerformQueryMore<T>(string queryLocator, int limit = 0) where T : ZObject
        {
            return PerformGenericRequest<QueryResult<T>>(SoapRequestManager.GetQueryMoreRequest(queryLocator, limit, Login()));
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

        private IEnumerable<T> PerformAdvancedQuery<T>(string query, int limit) where T : ZObject
        {
            List<T> records = new List<T>();
            QueryResult<T> result = PerformQuery<T>(query);
            if (!result.Done)
            {
                records.AddRange(CompleteQuery<T>(result.QueryLocator, limit));
            }

            return records;
        }

        private IEnumerable<T> CompleteQuery<T>(string queryLocator, int limit, int index = 0) where T : ZObject
        {
            QueryResult<T> result = PerformQueryMore<T>(queryLocator);
            index = index + result.Records.Count();

            List<T> records = new List<T>(result.Records);
            if (!result.Done && index < limit)
            {
                records.AddRange(CompleteQuery<T>(result.QueryLocator, limit, index));
            }

            return records;
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