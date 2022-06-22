using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachingServer
{
    public class LruCache
    {
        private int size = 0;
        private int capacity = 0;
        private LinkedList<dataNode> lst = new LinkedList<dataNode>();
        private Dictionary<string, LinkedListNode<dataNode>> dic = new Dictionary<string, LinkedListNode<dataNode>>();

        public LruCache(int capacity)
        {
            this.capacity = capacity;
        }

        public bool ContainsKey(string key) => dic.ContainsKey(key);

        public dataNode GetKey(string key)
        {
            if (!ContainsKey(key))
                throw new("MISSING");

            var node = dic[key];
            updateUsed(node);
            return node.Value;
        }

        public void Add(string key, dataNode data)
        {
            int addedSize;
            if (ContainsKey(key))
                addedSize = data.Size - dic.GetValueOrDefault(key).Value.Size;
            else
                addedSize = data.Size;
            
            while (size + addedSize > capacity)
            {
                dataNode last = lst.Last();
                Remove(last.Key);
            }

            if (dic.ContainsKey(key))
            {
                lst.Remove(dic[key]);
            }

            lst.AddFirst(data);
            dic[key] = lst.First;
            size += data.Size;
        }

        public void Remove(string key)
        {
            var node = dic[key];
            dic.Remove(key);
            lst.Remove(node);
            size -= node.Value.Size;
        }

        public dataNode this[string key]
        {
            get => GetKey(key);
            set => Add(key, value);
        }

        public override string ToString() // todo
        {
            StringBuilder s = new StringBuilder();

            foreach (var key in dic.Keys)
            {
                s.Append(key + " ");
            }

            return s.ToString();
        }

        private void updateUsed(LinkedListNode<dataNode> node)
        {
            lst.Remove(node);
            lst.AddFirst(node);
        }
    }
}