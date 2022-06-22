using System;

namespace CachingServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server s = new Server();
            s.Run();

        }
    }
}
