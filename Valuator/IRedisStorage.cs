using StackExchange.Redis;
namespace Valuator.Redis;

public interface IRedisStorage
{
    void Set(string key, string value);
    string Get(string key);
    List<string> GetKeys();
}