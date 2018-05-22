#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using ProtoBuf;

#endregion

namespace Database
{
    public sealed class InMemoryCache : IDisposable
    {
        public InMemoryCache()
        {
            mCache = new MemoryCache("InMemoryCache");
        }

        private readonly MemoryCache mCache;

        /// <summary>
        ///     Remove item from cache
        /// </summary>
        /// <param name="key">Name of cached item</param>
        public void Clear(string key)
        {
            mCache.Remove(key);
        }

        /// <summary>
        ///     Remove all cache item.
        /// </summary>
        public void Clear()
        {
            mCache.Trim(100);
        }

        /// <summary>
        ///     Check for item in cache
        /// </summary>
        /// <param name="key">Name of cached item</param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            return mCache.Get(key) != null;
        }

        /// <summary>
        ///     Retrieve cached item
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Name of cached item</param>
        /// <returns>Cached item as type</returns>
        public T Get<T>(string key) where T : IExtensible
        {
            try
            {
                return (T) mCache[key];
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        ///     Gets all cached items as a list by their key.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAll()
        {
            return mCache.Select(keyValuePair => keyValuePair.Key).ToList();
        }

        /// <summary>
        ///     Insert value into the cache using
        ///     appropriate name/value pairs
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="objectToCache">Item to be cached</param>
        /// <param name="key">Name of item</param>
        public void Set<T>(T objectToCache, string key) where T : IExtensible
        {
            mCache.Set(key, objectToCache, DateTime.Now.AddHours(1));
        }

        /// <summary>
        ///     Insert value into the cache using
        ///     appropriate name/value pairs
        /// </summary>
        /// <param name="objectToCache">Item to be cached</param>
        /// <param name="key">Name of item</param>
        public void Set(object objectToCache, string key)
        {
            mCache.Set(key, objectToCache, DateTime.Now.AddHours(1));
        }

        public void Dispose()
        {
            mCache.Dispose();
        }
    }
}