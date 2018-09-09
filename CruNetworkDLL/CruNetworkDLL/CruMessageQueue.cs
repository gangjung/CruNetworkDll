using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruNetworkDLL
{
    class CruMessageQueue
    {
        public int Count { get { return _queue.Count; } }

        private Queue<CruMessage> _queue;
        private int _capacity;

        public CruMessageQueue(int capacity)
        {
            _capacity = capacity;
            _queue = new Queue<CruMessage>(capacity);
        }

        // Single Item
        public void Enqueue(CruMessage item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item is null");
            }

            if (Count == _capacity)
            {
                throw new IndexOutOfRangeException("queue is full");
            }

            lock (_queue)
            {
                _queue.Enqueue(item);
            }
        }
        // Items 
        public void Enqueue(CruMessage[] items)
        {
            int size = items.Length;

            if (items == null)
            {
                throw new ArgumentNullException("item is null");
            }

            if (Count == _capacity)
            {
                throw new IndexOutOfRangeException("queue is full");
            }

            if((Count + size) > _capacity)
            {
                throw new IndexOutOfRangeException("queue is full");
            }

            lock (_queue)
            {
                for(int i = 0; i<size; ++i)
                    _queue.Enqueue(items[i]);
            }
        }

        // 지금까진 쌓인 데이터 배열로 반환.
        public CruMessage[] Dequeue()
        {
            CruMessage[] result;
            
            lock (_queue)
            {
                if (_queue.Count == 0)
                    return null;

                result = _queue.ToArray();

                _queue.Clear();
            }

            return result;
        }

        public void Clear()
        {
            _queue.Clear();
        }
    }
}
