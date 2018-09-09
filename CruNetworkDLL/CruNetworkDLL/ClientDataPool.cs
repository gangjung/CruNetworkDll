using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace CruNetworkDLL
{
    class ClientDataPool
    {
        public int Count { get { return _pool.Count; } }

        private int _capacity;  // 솔직히 이거 이상으로 해줘도 상관없음.
        private Stack<ClientData> _pool;

        public ClientDataPool(int capacity = 0)
        {
            _capacity = capacity;

            _pool = new Stack<ClientData>(_capacity);
        }

        public void Push(ClientData item)
        {
            if (item == null)
                throw new ArgumentException("Item is null");

            if (Count == _capacity)
                throw new IndexOutOfRangeException("pool is full");

            //item.Initialize(null, null, null);
            item.Initialize(null, 0);
            lock (_pool)
            {
                _pool.Push(item);
            }
        }

        public ClientData Pop()
        {
            if (Count == 0)
                Push(new ClientData(null, -1));
            //Push(new ClientData(null, null, null));

            lock (_pool)
            {
                return _pool.Pop();
            }
        }

        public void SetEmptyData(int cnt)
        {
            if ((Count + cnt) > _capacity)
                throw new IndexOutOfRangeException("too many data");

            for(int i = 0; i<cnt; ++i)
            {
                // Push(new ClientData(null, new SocketAsyncEventArgs(), new SocketAsyncEventArgs()));
                Push(new ClientData(null, -1));
            }
        }
    }
}
