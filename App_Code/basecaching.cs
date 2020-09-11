namespace ApiBase.Globals
{
    using System;
    using System.Runtime.Caching;
    using ApiBase.Interfaces;

    public class MemoryCacher : ICachingProvider
    {
        public bool AddCache(string key, object value)
        {
            return SetCacheValue(key,value);
        }

        public bool RemoveCache(string key)
        {
            var cache = MemoryCache.Default;
            var res = cache.Remove(key);
            return res != null;
        }

        public object GetCacheValue(string key)
        {
            var cache = MemoryCache.Default;
            return cache.Get(key);
        }

        public bool SetCacheValue(string key, object value)
        {
            CacheItem item = new CacheItem(key,value);
            var cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy();
            try
            {
                cache.Set(item,policy);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CacheKeyExist(string key)
        {
            var cache = MemoryCache.Default;
            return cache.Contains(key);
        }
    }
}