using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.Models
{
    public enum GraphNodeType
    {
        Depot,
        IncidentSite,
        Substation
    }

    /// <summary>
    /// Represents a vertex in the Municipal Service Network Graph.
    /// Can be a Municipal Depot / Workshop or a Service Request incident location.
    /// </summary>
    public class GraphNode
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public GraphNodeType NodeType { get; set; }
        public int? ServiceRequestId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public GraphNode(string id, string label, GraphNodeType nodeType, float x, float y, int? serviceRequestId = null)
        {
            Id = id;
            Label = label;
            NodeType = nodeType;
            X = x;
            Y = y;
            ServiceRequestId = serviceRequestId;
        }

        public override string ToString()
        {
            return $"{Label} ({NodeType})";
        }
    }

    /// <summary>
    /// Represents a weighted undirected edge connecting two municipal locations
    /// via a road / transit corridor.
    /// </summary>
    public class GraphEdge : IComparable<GraphEdge>
    {
        public string FromId { get; set; }
        public string ToId { get; set; }
        public double DistanceKm { get; set; }
        public int TransitMinutes { get; set; }
        public bool IsMstEdge { get; set; }

        public GraphEdge(string fromId, string toId, double distanceKm, int transitMinutes = 0)
        {
            FromId = fromId;
            ToId = toId;
            DistanceKm = distanceKm;
            TransitMinutes = transitMinutes == 0 ? (int)Math.Max(3, Math.Round(distanceKm * 2.2)) : transitMinutes;
            IsMstEdge = false;
        }

        public bool Connects(string a, string b)
        {
            return (FromId == a && ToId == b) || (FromId == b && ToId == a);
        }

        public string GetOtherNode(string currentId)
        {
            if (FromId == currentId) return ToId;
            if (ToId == currentId) return FromId;
            return null;
        }

        public int CompareTo(GraphEdge other)
        {
            if (other == null) return 1;
            return DistanceKm.CompareTo(other.DistanceKm);
        }

        public override string ToString()
        {
            return $"{FromId} <--> {ToId} ({DistanceKm:F1} km, ~{TransitMinutes} min)";
        }
    }

    /// <summary>
    /// Encapsulates the output of Kruskal's or Prim's Minimum Spanning Tree algorithm.
    /// </summary>
    public class MstResult
    {
        public string AlgorithmName { get; set; }
        public List<GraphEdge> Edges { get; set; } = new List<GraphEdge>();
        public double TotalMstDistanceKm { get; set; }
        public double OriginalTotalDistanceKm { get; set; }
        public double KilometersSaved => Math.Max(0, OriginalTotalDistanceKm - TotalMstDistanceKm);
        public double SavingsPercentage => OriginalTotalDistanceKm > 0 ? (KilometersSaved / OriginalTotalDistanceKm) * 100.0 : 0.0;
        public double EstimatedCostZar => TotalMstDistanceKm * 3200.0; // Estimated R3,200 per km road corridor maintenance
        public List<string> ExecutionLog { get; set; } = new List<string>();
        public long ExecutionTimeTicks { get; set; }
    }

    /// <summary>
    /// Encapsulates the results of Breadth-First or Depth-First graph traversals.
    /// </summary>
    public class TraversalResult
    {
        public string TraversalType { get; set; } // "BFS" or "DFS"
        public string StartNodeId { get; set; }
        public List<GraphNode> VisitedNodes { get; set; } = new List<GraphNode>();
        public List<string> LogSteps { get; set; } = new List<string>();
        public Dictionary<string, int> HopDistances { get; set; } = new Dictionary<string, int>();
        public long ExecutionTimeTicks { get; set; }
    }
}
