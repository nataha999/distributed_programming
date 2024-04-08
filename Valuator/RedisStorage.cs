using StackExchange.Redis;

namespace Valuator.Redis
{
    public class RedisStorage : IRedisStorage
    {
        private readonly IConnectionMultiplexer _connection;
        private readonly IConfiguration _сonfiguration;

        public RedisStorage(IConfiguration configuration)
        {
            _сonfiguration = configuration;
            var host = _сonfiguration["RedisValues:HOST_NAME"];
            _connection = ConnectionMultiplexer.Connect(host);
        }

        public void Set(string key, string value)
        {
            var db = _connection.GetDatabase();

            db.StringSet(key, value);
        }

        public string Get(string key)
        {
            var db = _connection.GetDatabase();

            return db.StringGet(key);
        }

        public List<string> GetKeys()
        {
            var host = _сonfiguration["RedisValues:HOST_NAME"];
            var port = Convert.ToInt32(_сonfiguration["RedisValues:HOST_PORT"]);
            var keys = _connection.GetServer(host, port).Keys();

            return keys.Select(item => item.ToString()).ToList();
        }
    }
}