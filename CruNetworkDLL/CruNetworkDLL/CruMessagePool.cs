using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CruNetworkDLL
{
    /// <summary>
    /// 클라이언트마다 가지고 있는 것.
    /// Send와 Receive가 공유메모리를 가져갈 수 있다.
    /// </summary>
    class CruMessagePool
    {
        public int Count { get { return _pool.Count; } }

        private Stack<CruMessage> _pool;
        private int _capacity;

        public CruMessagePool(int capacity)
        {
            _capacity = capacity;
            _pool = new Stack<CruMessage>(_capacity);
        }      
      
        public void Push(CruMessage item)
        {
            if (item == null)
                throw new ArgumentNullException("item is null");

            if (Count == _capacity)
                throw new IndexOutOfRangeException("pool is full");

            item.Initialize(0, null);
            lock (_pool)
            {
                _pool.Push(item);
            }
        }

        public CruMessage Pop()
        {
            if (Count == 0)
                Push(new CruMessage());

            lock (_pool)
            {
                return _pool.Pop();
            }
        }

        public void SetEmptyMessage(int cnt)
        {
            if ((Count + cnt) > _capacity)
                throw new IndexOutOfRangeException("Over SIze");

            for (int i = 0; i < cnt; ++i)
                Push(new CruMessage());
        }
    }   
}
