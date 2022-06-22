namespace CachingServer
{
    class Program
    {
        static void Main()
        {
            Server cacheServer = new Server();
            cacheServer.Run();
        }
    }
}
