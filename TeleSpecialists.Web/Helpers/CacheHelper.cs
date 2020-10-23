//using Microsoft.Analytics.Interfaces;
//using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TeleSpecialists.Web.Helpers
{
    public class CacheHelper
    {
        public static void Insert(string key, object value, string prefix = "")
        {
            string cacheKey = string.Format("{0}_{1}", prefix, key.Replace("-", ""));
            new System.Web.Caching.Cache().Insert(cacheKey, value);
        }

        public static void Remove(string key, string prefix = "")
        {
            string cacheKey = string.Format("{0}_{1}", prefix, key.Replace("-", ""));
            new System.Web.Caching.Cache().Remove(cacheKey);
        }

        public static T InsertAndGet<T>(string key, Lazy<T> func, string prefix = "", int absExpireMinutes = 0)
        {
            string cacheKey = string.Format("{0}_{1}", prefix, key.Replace("-", ""));
            var cache = new System.Web.Caching.Cache();

            if (cache.Get(cacheKey) != null)
            {
                return (T)cache.Get(cacheKey);
            }
            else
            {
                T extractValue = func.Value;

                // if no expiration is mentioned, exec this code
                if (absExpireMinutes == 0) cache.Insert(cacheKey, extractValue);

                // if an expiration time is mentioned, use this code | Added to refresh it for MD Staff Last Updated Date
                else cache.Insert(cacheKey, extractValue, null, DateTime.Now.AddMinutes(absExpireMinutes), System.Web.Caching.Cache.NoSlidingExpiration);

                return extractValue;
            }
        }
    }
}