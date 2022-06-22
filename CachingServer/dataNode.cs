namespace CachingServer
{
    public struct dataNode
    {
        public string Key { get; set; }
        public string Data { get; set; }
        public int Size { get; set; }
 
        public dataNode(string i_Key, string i_Data, int i_Size)
        {
            Key = i_Key;
            Data = i_Data;
            Size = i_Size;
        }

        public override string ToString()
        {
            return ($"key: {Key} size: {Size} data:{Data}");
        }
    }
}