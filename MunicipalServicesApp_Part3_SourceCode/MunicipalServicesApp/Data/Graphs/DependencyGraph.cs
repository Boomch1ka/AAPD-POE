using System;
using System.Collections.Generic;
using System.Linq;

namespace MunicipalServicesApp.Data.Graphs
{
    /// <summary>
    /// Represents municipal work order dependencies as a Directed Acyclic Graph (DAG).
    /// Vertex: Service Request ID.
    /// Directed Edge (u -> v): "Request u must be completed BEFORE Request v can begin".
    /// Employs Topological Sort (Kahn's Algorithm) to determine the valid resolution order
    /// and detects cyclic dependency deadlocks.
    /// </summary>
    public class DependencyGraph
    {
        private readonly HashSet<int> _allRequests = new HashSet<int>();
        private readonly Dictionary<int, List<int>> _adjacency = new Dictionary<int, List<int>>();
        private readonly Dictionary<int, int> _inDegree = new Dictionary<int, int>();

        public void AddRequest(int requestId)
        {
            _allRequests.Add(requestId);
            if (!_adjacency.ContainsKey(requestId))
            {
                _adjacency[requestId] = new List<int>();
                _inDegree[requestId] = 0;
            }
        }

        public void AddDependency(int prerequisiteId, int dependentId)
        {
            AddRequest(prerequisiteId);
            AddRequest(dependentId);

            if (!_adjacency[prerequisiteId].Contains(dependentId))
            {
                _adjacency[prerequisiteId].Add(dependentId);
                _inDegree[dependentId]++;
            }
        }

        public List<int> GetPrerequisites(int requestId)
        {
            var prereqs = new List<int>();
            foreach (var kvp in _adjacency)
            {
                if (kvp.Value.Contains(requestId))
                {
                    prereqs.Add(kvp.Key);
                }
            }
            return prereqs;
        }

        public List<int> GetDependents(int requestId)
        {
            return _adjacency.ContainsKey(requestId) ? new List<int>(_adjacency[requestId]) : new List<int>();
        }

        /// <summary>
        /// Computes the valid sequential resolution order using Kahn's Algorithm (Topological Sort).
        /// If a cycle exists, outputs the cycle warning.
        /// </summary>
        public bool GetTopologicalOrder(out List<int> order, out string message)
        {
            order = new List<int>();
            var inDegreeCopy = new Dictionary<int, int>(_inDegree);
            var queue = new Queue<int>();

            // Enqueue all tasks with in-degree 0 (no prerequisites)
            foreach (var id in _allRequests)
            {
                if (inDegreeCopy.ContainsKey(id) && inDegreeCopy[id] == 0)
                {
                    queue.Enqueue(id);
                }
            }

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                order.Add(current);

                if (_adjacency.ContainsKey(current))
                {
                    foreach (var neighbor in _adjacency[current])
                    {
                        inDegreeCopy[neighbor]--;
                        if (inDegreeCopy[neighbor] == 0)
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            if (order.Count != _allRequests.Count)
            {
                message = "Cycle detected in service dependencies! A circular dependency exists that would deadlock municipal crews.";
                return false;
            }

            message = $"Valid resolution sequence established for all {order.Count} tasks without conflicts.";
            return true;
        }
    }
}
