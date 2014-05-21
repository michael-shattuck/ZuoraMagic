using System;
using System.Runtime.Caching;
using ZuoraMagic.Configuration.Abstract;

namespace ZuoraMagic.Configuration
{
    public class MemoryCacheProvider : ISessionStoreProvider
    {
        public SessionStoragePolicy SessionStoragePolicy;
        private readonly MemoryCache _store;

        public MemoryCacheProvider(SessionStoragePolicy policy)
        {
            SessionStoragePolicy = policy;
            _store = MemoryCache.Default;
        }

        public MemoryCacheProvider()
        {
            _store = MemoryCache.Default;
            SessionStoragePolicy = new SessionStoragePolicy(TimeSpan.FromHours(1));
        }

        public ZuoraSession RetrieveSession(string environment)
        {
            string key = GenerateCacheKey(environment);
            if (!_store.Contains(key)) return null;

            ZuoraSession session = (ZuoraSession) _store[key];
            return CheckSessionValidity(key, session) ? session : null;
        }

        public bool StoreSession(ZuoraSession session)
        {
            string key = GenerateCacheKey(session.Environment);
            if (_store.Contains(key)) _store.Remove(key);

            return _store.Add(key, session, new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now.Add(SessionStoragePolicy.SessionStorageExpiration)
            });
        }

        public bool FindAvailableSession(string environment)
        {
            string key = GenerateCacheKey(environment);
            return _store.Contains(key) && CheckSessionValidity(key, (ZuoraSession) _store[key]);
        }

        private bool CheckSessionValidity(string key, ZuoraSession session)
        {
            bool valid = DateTime.Now - session.LastLogin > SessionStoragePolicy.SessionStorageExpiration;
            if (!valid) _store.Remove(key);

            return valid;
        }

        private string GenerateCacheKey(string environment)
        {
            string storageName = SessionStoragePolicy.StorageKeyPrefix ?? "ZuoraMagic_" + AppDomain.CurrentDomain.FriendlyName;
            return string.Format("{0}_{1}", storageName, environment);
        }
    }
}