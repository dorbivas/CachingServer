namespace CachingServer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using System.IO;

    public class Server
    {
        private readonly TcpListener m_Server;
        private readonly IPAddress m_IpAddress;

        private readonly Int32 r_Port;
        private byte m_SizeInBytes = new byte();
        private const char k_EscapeKey = '^';
        private const int r_MaxStorage = 60;//* (1024 * 1024); // 128MB
        private long m_CurrentStorageSize;

        /*todo*/
        private List<TcpClient> m_connectedClients = new List<TcpClient>();

        private LruCache m_DataCache = new LruCache(r_MaxStorage);

        /**/
        public Server(int i_Port = 10011, string i_IpAddress = "127.0.0.1")
        {
            IPAddress ipAddress = IPAddress.Parse(i_IpAddress);
            m_Server = new TcpListener(ipAddress, i_Port);
            r_Port = i_Port;
            m_IpAddress = ipAddress;
            CurrentStorageSize = 0;
        }

        public IPAddress IpAddress { get => m_IpAddress; }
        public byte SizeInBytes { get => m_SizeInBytes; set => m_SizeInBytes = value; }
        public static long MaxStorage => r_MaxStorage;
        public static char EscapeKey => k_EscapeKey;
        public long CurrentStorageSize { get => m_CurrentStorageSize; set => m_CurrentStorageSize = value; }

        public dataNode GetCacheKey(string i_Key)
        {
            return m_DataCache.GetKey(i_Key);
        }

        public void SetCacheKey(dataNode data)
        {
            m_DataCache.Add(data.Key, data);
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
            int size;
            StringBuilder data = new StringBuilder();
            dataNode dataParsed = new dataNode();
            string[] words = i_Data.Split(' '); // get key data \\ set key size data
            i_CommandType = words[0];
            dataParsed.Key = words[1];

            switch (i_CommandType)
            {
                case "get":
                    {
                        if (words.Length != 2)
                            throw new("Missing");// todo
                    }
                    break;
                case "set":
                    {
                        size = int.Parse(words[2]);

                        dataParsed.Size = size;

                        for (int i = 3; i < words.Length; i++)
                        {
                            if (i == words.Length-1)
                                data.Append($"{words[i]}");
                            else
                                data.Append($"{words[i]} ");
                        }

                        dataParsed.Data = data.ToString();

                        if (!checkValidSet(words[0], dataParsed.Data, size))
                            throw new("Missing");// todo

                    }
                    break;

                default:
                    break;
            }

            return dataParsed;
        }

        private static bool checkValidSet(string i_Command, string i_Data, int i_Size)
        {
            return validateSize(i_Size, i_Data) &&
                  (i_Command.CompareTo("set") == 0 || i_Command.CompareTo("get") == 0);
        }

        public void Run()
        {
            m_Server.Start();

            while (true)
            {
                TcpClient client = m_Server.AcceptTcpClient();
                Task task = handleClient(client);
                //todo stop ?
            }
        }

        private async Task handleClient(TcpClient client)
        {

            //Task a = new Task(null);
            await Task.Delay(millisecondsDelay: 500);

            NetworkStream stream = client.GetStream();

            StreamReader sr = new StreamReader(stream);
            StreamWriter sw = new StreamWriter(stream);
            sw.AutoFlush = true;
            sw.WriteLine($"Hallo welcome to my server young client! ");

            string line;
            string commandType;
            do
            {
                line = sr.ReadLine();
                dataNode data = parseData(line, out commandType);
                if (commandType.CompareTo("get") == 0)
                {
                    sw.WriteLine($"OK {data.Key}");
                    Console.WriteLine(GetCacheKey(data.Key).ToString()); // todo
                }
                else if (commandType.CompareTo("set") == 0)
                {
                    SetCacheKey(data);
                    

                    sw.WriteLine($"OK");
                }

                Console.WriteLine("cache: " + m_DataCache.ToString());
                Console.WriteLine("data: " + data.ToString());

            } while (line != null); 
        }
    }
}
