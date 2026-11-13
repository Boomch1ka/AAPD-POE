using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data.Graphs
{
    /// <summary>
    /// Represents the municipal service and road infrastructure network as an undirected,
    /// weighted graph. Vertices represent Municipal Depots and Incident Locations.
    /// Edges represent road corridors with real-world distances (km) and travel times (mins).
    /// </summary>
    public class ServiceNetworkGraph
    {
        public Dictionary<string, GraphNode> Nodes { get; } = new Dictionary<string, GraphNode>();
        public List<GraphEdge> Edges { get; } = new List<GraphEdge>();
        private readonly Dictionary<string, List<GraphEdge>> _adjacency = new Dictionary<string, List<GraphEdge>>();

        public void AddNode(GraphNode node)
        {
            if (!Nodes.ContainsKey(node.Id))
            {
                Nodes[node.Id] = node;
                _adjacency[node.Id] = new List<GraphEdge>();
            }
        }

        public void AddEdge(string fromId, string toId, double distanceKm, int transitMinutes = 0)
        {
            if (!Nodes.ContainsKey(fromId) || !Nodes.ContainsKey(toId)) return;

            var edge = new GraphEdge(fromId, toId, distanceKm, transitMinutes);
            Edges.Add(edge);
            _adjacency[fromId].Add(edge);
            _adjacency[toId].Add(edge);
        }

        public List<GraphEdge> GetIncidentEdges(string nodeId)
        {
            return _adjacency.ContainsKey(nodeId) ? _adjacency[nodeId] : new List<GraphEdge>();
        }

        public void ClearMstFlags()
        {
            foreach (var edge in Edges)
            {
                edge.IsMstEdge = false;
            }
        }

        /// <summary>
        /// Breadth-First Search (BFS) graph traversal. Explores the municipal network
        /// layer-by-layer radiating outward from a designated starting depot or incident.
        /// Useful for emergency dispatch zones and finding minimum hop routes.
        /// </summary>
        public TraversalResult BreadthFirstSearch(string startNodeId)
        {
            var sw = Stopwatch.StartNew();
            var result = new TraversalResult
            {
                TraversalType = "BFS",
                StartNodeId = startNodeId
            };

            if (!Nodes.ContainsKey(startNodeId)) return result;

            var visited = new HashSet<string>();
            var queue = new Queue<string>();

            visited.Add(startNodeId);
            queue.Enqueue(startNodeId);
            result.HopDistances[startNodeId] = 0;

            result.LogSteps.Add($"[BFS Start] Commenced Breadth-First traversal at {Nodes[startNodeId].Label} (Depth 0).");

            while (queue.Count > 0)
            {
                string currentId = queue.Dequeue();
                var currentNode = Nodes[currentId];
                result.VisitedNodes.Add(currentNode);
                int currentHop = result.HopDistances[currentId];

                foreach (var edge in GetIncidentEdges(currentId))
                {
                    string neighborId = edge.GetOtherNode(currentId);
                    if (neighborId != null && !visited.Contains(neighborId))
                    {
                        visited.Add(neighborId);
                        result.HopDistances[neighborId] = currentHop + 1;
                        queue.Enqueue(neighborId);

                        result.LogSteps.Add($"  -> Discovered {Nodes[neighborId].Label} (Hop {currentHop + 1}) via corridor {edge.DistanceKm:F1} km.");
                    }
                }
            }

            sw.Stop();
            result.ExecutionTimeTicks = sw.ElapsedTicks;
            result.LogSteps.Add($"[BFS Complete] Visited {result.VisitedNodes.Count} locations in {sw.Elapsed.TotalMilliseconds:F3} ms.");
            return result;
        }

        /// <summary>
        /// Depth-First Search (DFS) graph traversal. Explores deep routes across the municipal network
        /// to identify complete circular routes, network reachability, and connected components.
        /// </summary>
        public TraversalResult DepthFirstSearch(string startNodeId)
        {
            var sw = Stopwatch.StartNew();
            var result = new TraversalResult
            {
                TraversalType = "DFS",
                StartNodeId = startNodeId
            };

            if (!Nodes.ContainsKey(startNodeId)) return result;

            var visited = new HashSet<string>();
            result.LogSteps.Add($"[DFS Start] Commenced Depth-First traversal at {Nodes[startNodeId].Label}.");

            DfsRecursive(startNodeId, visited, result, 0);

            sw.Stop();
            result.ExecutionTimeTicks = sw.ElapsedTicks;
            result.LogSteps.Add($"[DFS Complete] Visited {result.VisitedNodes.Count} locations in {sw.Elapsed.TotalMilliseconds:F3} ms.");
            return result;
        }

        private void DfsRecursive(string currentId, HashSet<string> visited, TraversalResult result, int depth)
        {
            visited.Add(currentId);
            var currentNode = Nodes[currentId];
            result.VisitedNodes.Add(currentNode);
            result.HopDistances[currentId] = depth;

            string indent = new string(' ', (depth + 1) * 2);

            foreach (var edge in GetIncidentEdges(currentId).OrderBy(e => e.DistanceKm))
            {
                string neighborId = edge.GetOtherNode(currentId);
                if (neighborId != null && !visited.Contains(neighborId))
                {
                    result.LogSteps.Add($"{indent}-> Branching to {Nodes[neighborId].Label} ({edge.DistanceKm:F1} km, depth {depth + 1})");
                    DfsRecursive(neighborId, visited, result, depth + 1);
                }
            }
        }

        /// <summary>
        /// Computes the Minimum Spanning Tree (MST) using Kruskal's Algorithm
        /// with Disjoint Set Union (Union-Find) and path compression.
        /// Optimizes municipal crew dispatch and road maintenance by finding the minimum total distance
        /// that connects all municipal facilities and incident sites with no cycles.
        /// </summary>
        public MstResult ComputeKruskalMst()
        {
            var sw = Stopwatch.StartNew();
            ClearMstFlags();

            var result = new MstResult
            {
                AlgorithmName = "Kruskal's Algorithm (Greedy Edge Selection with Union-Find)",
                OriginalTotalDistanceKm = Edges.Sum(e => e.DistanceKm)
            };

            // 1. Sort all edges in non-decreasing order of distance
            var sortedEdges = Edges.OrderBy(e => e.DistanceKm).ToList();
            var dsu = new DisjointSet();

            foreach (var nodeKey in Nodes.Keys)
            {
                dsu.MakeSet(nodeKey);
            }

            result.ExecutionLog.Add($"[Kruskal Init] {Nodes.Count} vertices, {Edges.Count} total corridors. Sorting edges by distance...");

            int edgesTarget = Nodes.Count - 1;

            foreach (var edge in sortedEdges)
            {
                string u = edge.FromId;
                string v = edge.ToId;

                if (dsu.Union(u, v))
                {
                    // No cycle: add to MST
                    edge.IsMstEdge = true;
                    result.Edges.Add(edge);
                    result.TotalMstDistanceKm += edge.DistanceKm;

                    result.ExecutionLog.Add($"  [ACCEPTED] {Nodes[u].Label} <--> {Nodes[v].Label} ({edge.DistanceKm:F1} km) - MST Edge #{result.Edges.Count}");

                    if (result.Edges.Count == edgesTarget)
                    {
                        result.ExecutionLog.Add($"  -> Target of {edgesTarget} edges reached. Minimum Spanning Tree is fully connected!");
                        break;
                    }
                }
                else
                {
                    result.ExecutionLog.Add($"  [REJECTED - CYCLE] {Nodes[u].Label} <--> {Nodes[v].Label} ({edge.DistanceKm:F1} km) already connected.");
                }
            }

            sw.Stop();
            result.ExecutionTimeTicks = sw.ElapsedTicks;
            result.ExecutionLog.Add($"[Kruskal Complete] MST Total: {result.TotalMstDistanceKm:F1} km vs Original Network: {result.OriginalTotalDistanceKm:F1} km.");
            result.ExecutionLog.Add($"Savings: {result.KilometersSaved:F1} km ({result.SavingsPercentage:F1}% optimization) in {sw.Elapsed.TotalMilliseconds:F3} ms.");

            return result;
        }

        /// <summary>
        /// Computes the Minimum Spanning Tree (MST) using Prim's Algorithm.
        /// Starts from a chosen root depot and greedily grows the tree by adding
        /// the minimum-weight cut edge.
        /// </summary>
        public MstResult ComputePrimMst(string startNodeId = null)
        {
            var sw = Stopwatch.StartNew();
            ClearMstFlags();

            if (string.IsNullOrEmpty(startNodeId) || !Nodes.ContainsKey(startNodeId))
            {
                startNodeId = Nodes.Keys.First();
            }

            var result = new MstResult
            {
                AlgorithmName = "Prim's Algorithm (Greedy Vertex-Cut Growing)",
                OriginalTotalDistanceKm = Edges.Sum(e => e.DistanceKm)
            };

            var visited = new HashSet<string>();
            visited.Add(startNodeId);

            result.ExecutionLog.Add($"[Prim Init] Starting MST cut expansion from {Nodes[startNodeId].Label}...");

            while (visited.Count < Nodes.Count)
            {
                GraphEdge minEdge = null;
                double minDistance = double.MaxValue;
                string nextNodeId = null;

                foreach (var u in visited)
                {
                    foreach (var edge in GetIncidentEdges(u))
                    {
                        string v = edge.GetOtherNode(u);
                        if (!visited.Contains(v) && edge.DistanceKm < minDistance)
                        {
                            minDistance = edge.DistanceKm;
                            minEdge = edge;
                            nextNodeId = v;
                        }
                    }
                }

                if (minEdge == null)
                {
                    result.ExecutionLog.Add("  -> Warning: Graph has disconnected components; Prim spanned the current component.");
                    break;
                }

                visited.Add(nextNodeId);
                minEdge.IsMstEdge = true;
                result.Edges.Add(minEdge);
                result.TotalMstDistanceKm += minEdge.DistanceKm;

                result.ExecutionLog.Add($"  [ACCEPTED] {Nodes[minEdge.FromId].Label} <--> {Nodes[minEdge.ToId].Label} ({minEdge.DistanceKm:F1} km) - Visited {visited.Count}/{Nodes.Count}");
            }

            sw.Stop();
            result.ExecutionTimeTicks = sw.ElapsedTicks;
            result.ExecutionLog.Add($"[Prim Complete] MST Total: {result.TotalMstDistanceKm:F1} km (Savings: {result.SavingsPercentage:F1}%) in {sw.Elapsed.TotalMilliseconds:F3} ms.");
            return result;
        }
    }
}
