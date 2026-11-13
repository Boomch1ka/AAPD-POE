using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.Data
{
    /// <summary>
    /// A generic priority queue built on a binary min-heap stored in a List.
    /// .NET Framework 4.8 has no built-in PriorityQueue, so it is implemented here.
    /// The item with the LOWEST priority number is dequeued first.
    /// Enqueue and Dequeue are O(log n); Peek is O(1). Items with equal
    /// priority leave in the order they were added (FIFO) via a sequence number.
    /// </summary>
    public class MinPriorityQueue<T>
    {
        private struct Node
        {
            public int Priority;
            public long Sequence;
            public T Item;
        }

        private readonly List<Node> _heap = new List<Node>();
        private long _sequence;

        public int Count { get { return _heap.Count; } }

        public void Enqueue(T item, int priority)
        {
            _heap.Add(new Node { Priority = priority, Sequence = _sequence++, Item = item });
            SiftUp(_heap.Count - 1);
        }

        public T Peek()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("The priority queue is empty.");
            return _heap[0].Item;
        }

        public T Dequeue()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("The priority queue is empty.");
            T result = _heap[0].Item;
            int last = _heap.Count - 1;
            _heap[0] = _heap[last];
            _heap.RemoveAt(last);
            if (_heap.Count > 0) SiftDown(0);
            return result;
        }

        private bool Less(Node a, Node b)
        {
            if (a.Priority != b.Priority) return a.Priority < b.Priority;
            return a.Sequence < b.Sequence;
        }

        private void SiftUp(int i)
        {
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (!Less(_heap[i], _heap[parent])) break;
                Swap(i, parent);
                i = parent;
            }
        }

        private void SiftDown(int i)
        {
            int n = _heap.Count;
            while (true)
            {
                int left = 2 * i + 1, right = left + 1, smallest = i;
                if (left < n && Less(_heap[left], _heap[smallest])) smallest = left;
                if (right < n && Less(_heap[right], _heap[smallest])) smallest = right;
                if (smallest == i) break;
                Swap(i, smallest);
                i = smallest;
            }
        }

        private void Swap(int a, int b)
        {
            Node tmp = _heap[a];
            _heap[a] = _heap[b];
            _heap[b] = tmp;
        }
    }
}
