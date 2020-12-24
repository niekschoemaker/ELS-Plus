using Shared;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ELS_Server
{
    public class Cache
    {
        public Dictionary<int, IDictionary<string, object>> CachedData { get; private set; } = new Dictionary<int, IDictionary<string, object>>();

        public IDictionary<string, object> Get(int networkID)
        {
            if (!CachedData.TryGetValue(networkID, out var cachedData))
            {
                cachedData = new Dictionary<string, object>();
                CachedData.Add(networkID, cachedData);
            }

            return cachedData;
        }

        public void Set(int networkid, IDictionary<string, object> newData)
        {
            var data = Get(networkid);
            foreach (var a in newData)
            {
                data[a.Key] = a.Value;
            }
        }

        public IDictionary<string, object> GetKey(int networkId, string key)
        {
            var cache = Get(networkId);
            IDictionary<string, object> data;
            if (!cache.TryGetValue(key, out var obj))
            {
                data = new Dictionary<string, object>();
                cache[key] = data;
            }

            data = obj as IDictionary<string, object>;

            return data;
        }

        public void SetKey(int networkId, string key, IDictionary<string, object> newData)
        {
            var data = GetKey(networkId, key);
            foreach(var a in newData)
            {
                data[a.Key] = a.Value;
            }
        }
    }
}
