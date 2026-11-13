using System.Collections.Generic;

namespace MunicipalServicesApp.Data.Graphs
{
    /// <summary>
    /// Disjoint Set Union (DSU / Union-Find) data structure with path compression
    /// and union by rank. Used by Kruskal's algorithm to detect cycles in near-O(1) time.
    /// </summary>
    public class DisjointSet
    {
        private readonly Dictionary<string, string> _parent = new Dictionary<string, string>();
        private readonly Dictionary<string, int> _rank = new Dictionary<string, int>();

        public void MakeSet(string item)
        {
            if (!_parent.ContainsKey(item))
            {
                _parent[item] = item;
                _rank[item] = 0;
            }
        }

        public string Find(string item)
        {
            if (!_parent.ContainsKey(item))
            {
                MakeSet(item);
                return item;
            }

            // Path compression
            if (_parent[item] != item)
            {
                _parent[item] = Find(_parent[item]);
            }

            return _parent[item];
        }

        public bool Union(string a, string b)
        {
            string rootA = Find(a);
            string rootB = Find(b);

            if (rootA == rootB)
            {
                return false; // Cycle detected: already in the same set
            }

            // Union by rank
            if (_rank[rootA] < _rank[rootB])
            {
                _parent[rootA] = rootB;
            }
            else if (_rank[rootA] > _rank[rootB])
            {
                _parent[rootB] = rootA;
            }
            else
            {
                _parent[rootB] = rootA;
                _rank[rootA]++;
            }

            return true;
        }
    }
}
