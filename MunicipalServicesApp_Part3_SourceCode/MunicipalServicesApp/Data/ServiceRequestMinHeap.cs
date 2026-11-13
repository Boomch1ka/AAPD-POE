using System;
using System.Collections.Generic;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data
{
    /// <summary>
    /// Binary Min-Heap prioritizing Municipal Service Requests by urgency level.
    /// Priority 1 (Critical Emergency) items are serviced before Priority 2, 3, and 4.
    /// Tie-breaking is done chronologically (FIFO based on submission timestamp).
    /// Enqueue and Dequeue operations run in O(log n) time; Peek runs in O(1).
    /// </summary>
    public class ServiceRequestMinHeap
    {
        private readonly List<ServiceRequest> _elements = new List<ServiceRequest>();

        public int Count => _elements.Count;

        public void Enqueue(ServiceRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            _elements.Add(request);
            SiftUp(_elements.Count - 1);
        }

        public ServiceRequest Peek()
        {
            if (_elements.Count == 0)
                throw new InvalidOperationException("The priority dispatch queue is currently empty.");

            return _elements[0];
        }

        public ServiceRequest Dequeue()
        {
            if (_elements.Count == 0)
                throw new InvalidOperationException("The priority dispatch queue is currently empty.");

            var top = _elements[0];
            int lastIndex = _elements.Count - 1;
            _elements[0] = _elements[lastIndex];
            _elements.RemoveAt(lastIndex);

            if (_elements.Count > 0)
            {
                SiftDown(0);
            }

            return top;
        }

        public List<ServiceRequest> ToList()
        {
            // Return sorted copy by repeatedly peeking/dequeuing
            var copy = new List<ServiceRequest>(_elements);
            copy.Sort();
            return copy;
        }

        public void Remove(int requestId)
        {
            int index = _elements.FindIndex(r => r.Id == requestId);
            if (index >= 0)
            {
                int lastIndex = _elements.Count - 1;
                _elements[index] = _elements[lastIndex];
                _elements.RemoveAt(lastIndex);
                if (index < _elements.Count)
                {
                    SiftDown(index);
                    SiftUp(index);
                }
            }
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (_elements[index].CompareTo(_elements[parentIndex]) >= 0)
                    break;

                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void SiftDown(int index)
        {
            int n = _elements.Count;
            while (true)
            {
                int left = 2 * index + 1;
                int right = 2 * index + 2;
                int smallest = index;

                if (left < n && _elements[left].CompareTo(_elements[smallest]) < 0)
                    smallest = left;

                if (right < n && _elements[right].CompareTo(_elements[smallest]) < 0)
                    smallest = right;

                if (smallest == index)
                    break;

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int i, int j)
        {
            var temp = _elements[i];
            _elements[i] = _elements[j];
            _elements[j] = temp;
        }
    }
}
