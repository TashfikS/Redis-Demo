﻿
using StackExchange.Redis;
using System.Text.Json;

namespace RadisDemo.Services
{
    public class CacheService : ICacheService
    {
        private IDatabase _cacheDb;

        public CacheService(IConnectionMultiplexer redis)
        {
            /*var redis = ConnectionMultiplexer.Connect(config.GetConnectionString("Redis"));*/
            _cacheDb = redis.GetDatabase();
        }
        public T GetData<T>(string key)
        {
            var value = _cacheDb.StringGet(key);

            if (!string.IsNullOrEmpty(value))
                return JsonSerializer.Deserialize<T>(value);

            return default;
        }

        public object RemoveData(string key)
        {
            var _exist = _cacheDb.KeyExists(key);

            if (_exist)
                return _cacheDb.KeyDelete(key);

            return false;
        }

        public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            var expiryTime = expirationTime.DateTime.Subtract(DateTime.Now);
            return _cacheDb.StringSet(key, JsonSerializer.Serialize(value), expiryTime);
        }
    }
}