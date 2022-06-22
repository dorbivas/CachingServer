namespace CachingServer
{
    public interface ICacheManager
    {
        object Get(string key);
        void Set(string key, object data, int cacheTime);

    }
}