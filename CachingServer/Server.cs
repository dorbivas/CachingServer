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
        private const int r_MaxStorage = 60;//* (1024 * 1024); // 128MB
        private LruCache m_DataCache = new LruCache(r_MaxStorage);

        public Server(int i_Port = 10011, string i_IpAddress = "127.0.0.1")
        {
            IPAddress ipAddress = IPAddress.Parse(i_IpAddress);
            m_Server = new TcpListener(ipAddress, i_Port);
            r_Port = i_Port;
            m_IpAddress = ipAddress;
        }

        public dataNode GetCacheKey(string i_Key)
        {
            lock (block)
            {
                Console.Out.Flush();

                Console.WriteLine($" {Thread.CurrentThread.ManagedThreadId} LOCKED ");

                Console.WriteLine($" {Thread.CurrentThread.ManagedThreadId}: get: ");

                Console.WriteLine($" {Thread.CurrentThread.ManagedThreadId} UNLOCKED ");

                return m_DataCache.GetKey(i_Key);

            }
        }

        public void SetCacheKey(dataNode data)
        {
            lock (block)
            {
                Console.Out.Flush();

                Console.WriteLine($" {Thread.CurrentThread.ManagedThreadId} LOCKED ");

                Console.WriteLine($" {Thread.CurrentThread.ManagedThreadId}: set: ");
                m_DataCache.Add(data.Key, data);
                Console.WriteLine("cache: " + m_DataCache.ToString());
                Console.WriteLine("data: " + data.ToString());

                Console.WriteLine($" {Thread.CurrentThread.ManagedThreadId} UNLOCKED ");

                Console.Out.Flush();
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

        private static dataNode parseData(string i_Data, out string i_CommandType)
        {
            StringBuilder data = new StringBuilder();
            dataNode dataParsed = new dataNode();
            string[] words = i_Data.Split(' '); // get key data \\ set key size data

            i_CommandType = words[0];
            dataParsed.Key = words[1];
            for (int i = 3; i < words.Length; i++)
            {
                if (i == words.Length - 1)
                    data.Append($"{words[i]}");
                else
                    data.Append($"{words[i]} ");
            }
            dataParsed.Data = data.ToString();

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
                        dataParsed = handleSet(words);
                    }
                    break;

                default:
                    throw new("Invalid command");
            }

            return dataParsed;
        }

        private static dataNode handleSet(string[] words)
        {
            int size;
            bool isNumeric = int.TryParse(words[2], out size);
            dataNode dataParsed = new dataNode();

            if (isNumeric)
                dataParsed.Size = size;
            else
                throw new("Invalid size");

            if (!validateSize(dataParsed.Size, dataParsed.Data))
                throw new("Invalid set command");
            return dataParsed;
        }

        public void Run()
        {
            m_Server.Start();

            while (true)
            {
                TcpClient client = m_Server.AcceptTcpClient();
                Task task = handleClient(client);
            }
        }

        private async Task handleClient(TcpClient client)
        {
            await Task.Delay(millisecondsDelay: 500);
            NetworkStream stream = client.GetStream();
            StreamReader sr = new StreamReader(stream);
            StreamWriter sw = new StreamWriter(stream);
            string line = "", commandType;

            sw.AutoFlush = true;

            do
            {
                try
                {
                    line = sr.ReadLine();
                    dataNode data = parseData(line, out commandType);
                    if (commandType.CompareTo("get") == 0)
                    {
                        data = GetCacheKey(data.Key);
                        Console.WriteLine(data.Key.ToString()); // todo
                        sw.WriteLine($"OK {data.Key}{Environment.NewLine}{data.Data}");
                    }
                    else if (commandType.CompareTo("set") == 0)
                    {
                        SetCacheKey(data);
                        sw.WriteLine($"OK");
                    }

                }
                catch (Exception e)
                {
                    sw.WriteLine("MISSING");
                }
            } while (line != null);
        }
    }
}
