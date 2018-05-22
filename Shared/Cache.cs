#region using

using System.Collections.Generic;

#endregion

namespace Shared
{
    public class Cache<TKey, TValue>
    {
        public Cache(int size)
        {
            Size = size;
        }

        private readonly Dictionary<TKey, DataItem> mDictionary = new Dictionary<TKey, DataItem>();
        private readonly LinkedList<DataItem> mLinkedList = new LinkedList<DataItem>();
        public int Size { get; set; }

        public void Add(TKey key, TValue value)
        {
            DataItem data;
            if (mDictionary.TryGetValue(key, out data))
            {
                mLinkedList.Remove(data.Node);
                mLinkedList.AddFirst(data.Node);
            }
            else
            {
                data = new DataItem();
                data.Value = value;
                data.Node = mLinkedList.AddFirst(data);
                data.Key = key;

                mDictionary.Add(key, data);

                if (mDictionary.Count > Size)
                {
                    var n = mLinkedList.Last;
                    mDictionary.Remove(n.Value.Key);
                    mLinkedList.RemoveLast();
                }
            }
        }

        public TValue Get(TKey key)
        {
            DataItem data;
            if (mDictionary.TryGetValue(key, out data))
            {
                mLinkedList.Remove(data.Node);
                mLinkedList.AddFirst(data.Node);
                return data.Value;
            }

            return default(TValue);
        }

        private class DataItem
        {
            public TKey Key;
            public LinkedListNode<DataItem> Node;
            public TValue Value;
        }
    }
}