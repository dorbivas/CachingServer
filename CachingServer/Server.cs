namespace CachingServer
{
    using System;
    using System.Text;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using System.IO;
    using System.Threading;

    public class Server
    {
        private readonly TcpListener m_Server;
        private readonly IPAddress m_IpAddress;
        private readonly Int32 r_Port;
        private readonly object block = new object();
        private const int r_MaxStorage = 128 * (1024 * 1024); 
        private LruCache m_DataCache = new LruCache(r_MaxStorage);

        public Server(int i_Port = 10011, string i_IpAddress = "127.0.0.1")
        {
            IPAddress ipAddress = IPAddress.Parse(i_IpAddress);
            m_Server = new TcpListener(ipAddress, i_Port);
            r_Port = i_Port;
            m_IpAddress = ipAddress;
        }
        /// <summary>
        /// for testing allow to see the synchronization
        /// Console.Out.Flush();
        /// Console.WriteLine($" {Thread.CurrentThread.ManagedThreadId} LOCKED ");
        /// Console.WriteLine($" {Thread.CurrentThread.ManagedThreadId}: get: ");
        /// Console.WriteLine($" {Thread.CurrentThread.ManagedThreadId} UNLOCKED ");
        public DataNode GetCacheKey(string i_Key)
        {
            lock (block)
            {
                return m_DataCache.GetKey(i_Key);
            }
        }

        /// <summary>
        /// this test doco allow you to see the cache state at current time.
        /// Console.Out.Flush();
        /// Console.WriteLine($" {Thread.CurrentThread.ManagedThreadId} LOCKED ");   
        /// Console.WriteLine($" {Thread.CurrentThread.ManagedThreadId}: set: ");
        /// m_DataCache.Add(data.Key, data);
        /// Console.WriteLine("cache: " + m_DataCache.ToString()); 
        /// Console.WriteLine("data: " + data.ToString());  
        /// Console.WriteLine($" {Thread.CurrentThread.ManagedThreadId} UNLOCKED "); 
        public void SetCacheKey(DataNode data)
        {
            lock (block)
            {
                m_DataCache.Add(data.Key, data);
            }
        }

        private static bool validateSize(int i_Size, string i_Data)
        {
            int sizeInBytes = UTF8Encoding.UTF8.GetByteCount(i_Data);

            if (i_Size != sizeInBytes)
            {
                throw new FormatException("non matching size to actual data");
            }

            return true;
        }

        private static DataNode parseData(string i_Data, out string i_CommandType)
        {
            int size;
            DataNode dataParsed = new DataNode();

            string[] words = i_Data.Split(' ');
            i_CommandType = words[0];
            dataParsed.Key = words[1];
            dataParsed.Data = getData(words);

            switch (i_CommandType)
            {
                case "get":
                    {
                        if (words.Length != 2)
                            throw new("Invalid get command");
                    }
                    break;
                case "set":
                    {
                        bool isNumeric = int.TryParse(words[2], out size);

                        if (isNumeric)
                            dataParsed.Size = size;
                        else
                            throw new("Invalid size");

                        if (!validateSize(dataParsed.Size, dataParsed.Data))
                            throw new("Invalid set command");
                    }
                    break;
                default:
                    throw new("Invalid command");
            }

            return dataParsed;
        }

        private static string getData(string[] words)
        {
            StringBuilder data = new StringBuilder();

            for (int i = 3; i < words.Length; i++)
            {
                if (i == words.Length - 1)
                    data.Append($"{words[i]}");
                else
                    data.Append($"{words[i]} ");
            }

            return data.ToString();
        }

        public void Run()
        {
            m_Server.Start();

            while (true)
            {
                TcpClient client = m_Server.AcceptTcpClient();
                Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
                //Task.Run(() => handleClient(client));

                Task task = handleClient(client);
            }
        }

        private async Task handleClient(TcpClient client)
        {
            //await Task.Delay(millisecondsDelay: 500);
            NetworkStream stream = client.GetStream();
            StreamReader sr = new StreamReader(stream);
            StreamWriter sw = new StreamWriter(stream);
            string line = "", commandType;
            
            sw.AutoFlush = true;
            sw.WriteLine("Welcome to the server");
            sw.WriteLine("Yout wish is my command");
            sw.WriteLine($"The server is running on {m_IpAddress}:{r_Port}");
            
            do
            {
                try
                {
                    line = sr.ReadLine();
                    DataNode data = parseData(line, out commandType);

                    if (commandType.CompareTo("get") == 0)
                    {
                        data = GetCacheKey(data.Key);
                        sw.WriteLine($"OK {data.Size}{Environment.NewLine}{data.Data}");
                        Console.WriteLine($" {Thread.CurrentThread.ManagedThreadId}: get: {data.Key}");

                    }
                    else if (commandType.CompareTo("set") == 0)
                    {
                        SetCacheKey(data);
                        sw.WriteLine($"OK");
                        Console.WriteLine($" {Thread.CurrentThread.ManagedThreadId}: set: {data.Key}");
                    }

                }
                catch (Exception e)
                {
                    sw.WriteLine(e.Message); //display spacific error
                    sw.WriteLine("MISSING");
                }
            } while (line != null);
        }
    }
}


