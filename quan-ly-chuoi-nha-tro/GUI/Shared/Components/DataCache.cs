using System;
using System.Collections.Generic;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Simple in-memory cache with TTL support
    /// For caching frequently accessed data like lookup tables
    /// </summary>
    public static class DataCache
    {
        private static readonly Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();
        private static readonly object _lock = new object();

        /// <summary>
        /// Get item from cache
        /// </summary>
        public static T Get<T>(string key)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    if (entry.Expiry > DateTime.Now)
                    {
                        return (T)entry.Data;
                    }
                    // Expired, remove
                    _cache.Remove(key);
                }
                return default;
            }
        }

        /// <summary>
        /// Try get item from cache
        /// </summary>
        public static bool TryGet<T>(string key, out T value)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    if (entry.Expiry > DateTime.Now)
                    {
                        value = (T)entry.Data;
                        return true;
                    }
                    _cache.Remove(key);
                }
                value = default;
                return false;
            }
        }

        /// <summary>
        /// Set item in cache with TTL
        /// </summary>
        public static void Set<T>(string key, T data, TimeSpan ttl)
        {
            lock (_lock)
            {
                _cache[key] = new CacheEntry
                {
                    Data = data,
                    Expiry = DateTime.Now.Add(ttl)
                };
            }
        }

        /// <summary>
        /// Set item in cache (default 5 minutes TTL)
        /// </summary>
        public static void Set<T>(string key, T data)
        {
            Set(key, data, TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// Get or create cached item
        /// </summary>
        public static T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? ttl = null)
        {
            if (TryGet<T>(key, out var cached))
                return cached;

            var data = factory();
            Set(key, data, ttl ?? TimeSpan.FromMinutes(5));
            return data;
        }

        /// <summary>
        /// Get or create cached item async
        /// </summary>
        public static async System.Threading.Tasks.Task<T> GetOrCreateAsync<T>(
            string key, 
            Func<System.Threading.Tasks.Task<T>> factory, 
            TimeSpan? ttl = null)
        {
            if (TryGet<T>(key, out var cached))
                return cached;

            var data = await factory();
            Set(key, data, ttl ?? TimeSpan.FromMinutes(5));
            return data;
        }

        /// <summary>
        /// Remove item from cache
        /// </summary>
        public static void Remove(string key)
        {
            lock (_lock)
            {
                _cache.Remove(key);
            }
        }

        /// <summary>
        /// Remove items matching prefix
        /// </summary>
        public static void RemoveByPrefix(string prefix)
        {
            lock (_lock)
            {
                var keysToRemove = new List<string>();
                foreach (var key in _cache.Keys)
                {
                    if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        keysToRemove.Add(key);
                }
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                }
            }
        }

        /// <summary>
        /// Clear all cached items
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _cache.Clear();
            }
        }

        /// <summary>
        /// Clean up expired entries
        /// </summary>
        public static void CleanExpired()
        {
            lock (_lock)
            {
                var now = DateTime.Now;
                var keysToRemove = new List<string>();
                foreach (var kvp in _cache)
                {
                    if (kvp.Value.Expiry <= now)
                        keysToRemove.Add(kvp.Key);
                }
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                }
            }
        }

        /// <summary>
        /// Get cache statistics
        /// </summary>
        public static (int Count, int ExpiredCount) GetStats()
        {
            lock (_lock)
            {
                var now = DateTime.Now;
                int total = _cache.Count;
                int expired = 0;
                foreach (var kvp in _cache)
                {
                    if (kvp.Value.Expiry <= now)
                        expired++;
                }
                return (total, expired);
            }
        }

        private class CacheEntry
        {
            public object Data { get; set; }
            public DateTime Expiry { get; set; }
        }
    }

    /// <summary>
    /// Common cache keys for the application
    /// </summary>
    public static class CacheKeys
    {
        public const string RoomStatuses = "lookup_room_statuses";
        public const string RoomTypes = "lookup_room_types";
        public const string Branches = "lookup_branches";
        public const string UtilityTypes = "lookup_utility_types";
        public const string SystemSettings = "system_settings";

        public static string RoomsByBranch(int branchId) => $"rooms_branch_{branchId}";
        public static string TenantsByBranch(int branchId) => $"tenants_branch_{branchId}";
        public static string ContractsByBranch(int branchId) => $"contracts_branch_{branchId}";
    }
}
