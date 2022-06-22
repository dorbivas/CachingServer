/// <summary>
/// fully associative cache implimented with LRU system.  
/// </summary>
namespace CachingServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class LruCache
    {
        private int size = 0;
        private int capacity = 0;
        private LinkedList<DataNode> list = new LinkedList<DataNode>();
        private Dictionary<string, LinkedListNode<DataNode>> cache = new Dictionary<string, LinkedListNode<DataNode>>();

        public LruCache(int capacity)
        {
            this.capacity = capacity;
        }

        public bool ContainsKey(string key) => cache.ContainsKey(key);

        public DataNode GetKey(string key)
        {
            if (!ContainsKey(key))
                throw new("MISSING");

            var node = cache[key];
            updateLastUsed(node);
            return node.Value;
        }

        public void Add(string key, DataNode data)
        {
            int addedSize;
            if (ContainsKey(key))
                addedSize = data.Size - cache.GetValueOrDefault(key).Value.Size;
            else
                addedSize = data.Size;
            
            while (size + addedSize > capacity)
            {
                DataNode last = list.Last();
                Remove(last.Key);
            }

            if (cache.ContainsKey(key))
            {
                list.Remove(cache[key]);
            }

            list.AddFirst(data);
            cache[key] = list.First;
            size += data.Size;
        }

        public void Remove(string key)
        {
            var node = cache[key];
            cache.Remove(key);
            list.Remove(node);
            size -= node.Value.Size;
        }

        public DataNode this[string key]
        {
            get => GetKey(key);
            set => Add(key, value);
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();

            foreach (var key in cache.Keys)
            {
                s.Append(key + " ");
            }

            return s.ToString();
        }

        private void updateLastUsed(LinkedListNode<DataNode> node)
        {
            list.Remove(node);
            list.AddFirst(node);
        }
    }
}