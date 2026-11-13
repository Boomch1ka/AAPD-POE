using System;
using System.Collections.Generic;
using System.Linq;
using MunicipalServicesApp.Data.Graphs;
using MunicipalServicesApp.Data.Trees;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data
{
    /// <summary>
    /// Singleton repository managing all municipal service requests and the
    /// associated advanced data structures:
    /// - Binary Search Tree (BST)
    /// - AVL Self-Balancing Tree
    /// - Red-Black Self-Balancing Tree
    /// - General N-ary Hierarchical Tree
    /// - Binary Tree
    /// - Binary Min-Heap (Priority Queue)
    /// - Municipal Service Road Network Graph (BFS, DFS, Kruskal & Prim MST)
    /// - Dependency Graph (DAG with Topological Sort)
    /// </summary>
    public class ServiceRequestRepository
    {
        private static ServiceRequestRepository _instance;
        public static ServiceRequestRepository Instance => _instance ?? (_instance = new ServiceRequestRepository());

        public List<ServiceRequest> AllRequests { get; } = new List<ServiceRequest>();
        public Dictionary<int, ServiceRequest> RequestMap { get; } = new Dictionary<int, ServiceRequest>();

        // Tree structures
        public BinarySearchTree<int, ServiceRequest> Bst { get; private set; } = new BinarySearchTree<int, ServiceRequest>();
        public AvlTree<int, ServiceRequest> Avl { get; private set; } = new AvlTree<int, ServiceRequest>();
        public RedBlackTree<int, ServiceRequest> RedBlack { get; private set; } = new RedBlackTree<int, ServiceRequest>();
        public GeneralTree<ServiceRequest> AdministrativeTree { get; private set; }
        public BinaryTree<string> SampleBinaryTree { get; private set; }

        // Heaps & Graphs
        public ServiceRequestMinHeap PriorityHeap { get; private set; } = new ServiceRequestMinHeap();
        public ServiceNetworkGraph NetworkGraph { get; private set; } = new ServiceNetworkGraph();
        public DependencyGraph Dependencies { get; private set; } = new DependencyGraph();

        private int _nextId = 1015;

        public ServiceRequestRepository()
        {
            SeedData();
            RebuildTrees();
            BuildNetworkGraph();
            BuildDependencyGraph();
        }

        private void SeedData()
        {
            var seedItems = new List<ServiceRequest>
            {
                new ServiceRequest
                {
                    Id = 1001,
                    Title = "Burst Water Main on Sandton Drive",
                    Category = "Water & Utilities",
                    Location = "Sandton Drive & William Nicol",
                    Ward = "Ward 4",
                    Description = "High pressure clean water main fractured, flooding the intersection and eroding road shoulder.",
                    DateReported = DateTime.Now.AddHours(-36),
                    Priority = RequestPriority.Critical,
                    Status = RequestStatus.InProgress,
                    AssignedDepot = "Water Works Maintenance Station",
                    AssignedCrew = "Rapid Pipe Crew A",
                    EstimatedHours = 8
                },
                new ServiceRequest
                {
                    Id = 1002,
                    Title = "Downed Live Cable on Rivonia Road",
                    Category = "Electricity",
                    Location = "Rivonia Road near 7th Ave",
                    Ward = "Ward 4",
                    Description = "Overhead 11kV electrical cable severed by falling tree branch; sparking onto pedestrian pavement.",
                    DateReported = DateTime.Now.AddHours(-18),
                    Priority = RequestPriority.Critical,
                    Status = RequestStatus.Dispatched,
                    AssignedDepot = "Electricity Substation Alpha",
                    AssignedCrew = "High Voltage Emergency Team",
                    EstimatedHours = 4
                },
                new ServiceRequest
                {
                    Id = 1003,
                    Title = "Deep Pothole Cluster on Oxford Road",
                    Category = "Roads",
                    Location = "Oxford Road, Rosebank",
                    Ward = "Ward 8",
                    Description = "Three consecutive deep potholes exceeding 15cm depth, posing severe risk of vehicle tyre blowouts.",
                    DateReported = DateTime.Now.AddHours(-48),
                    Priority = RequestPriority.High,
                    Status = RequestStatus.UnderReview,
                    AssignedDepot = "North Roads & Stormwater Depot",
                    AssignedCrew = "Road Patching Unit 2",
                    EstimatedHours = 6
                },
                new ServiceRequest
                {
                    Id = 1004,
                    Title = "Sewer Overflow Near Primary School",
                    Category = "Sanitation",
                    Location = "Corner of 5th Ave & School Lane",
                    Ward = "Ward 12",
                    Description = "Manhole cover displaced by severe raw sewage backup flowing towards school playing grounds.",
                    DateReported = DateTime.Now.AddHours(-24),
                    Priority = RequestPriority.Critical,
                    Status = RequestStatus.InProgress,
                    AssignedDepot = "Central Operations Depot",
                    AssignedCrew = "Sanitation Jetting Team 1",
                    EstimatedHours = 5
                },
                new ServiceRequest
                {
                    Id = 1005,
                    Title = "Traffic Signal Outage at Katherine & Grayston",
                    Category = "Electricity",
                    Location = "Katherine St & Grayston Dr",
                    Ward = "Ward 4",
                    Description = "Complete traffic light controller failure causing multi-kilometer vehicular gridlock.",
                    DateReported = DateTime.Now.AddHours(-10),
                    Priority = RequestPriority.High,
                    Status = RequestStatus.Dispatched,
                    AssignedDepot = "Electricity Substation Alpha",
                    AssignedCrew = "Traffic Signal Technicians",
                    EstimatedHours = 3
                },
                new ServiceRequest
                {
                    Id = 1006,
                    Title = "Illegal Dumping of Builder's Rubble",
                    Category = "Waste Management",
                    Location = "Vacant lot on 11th Street, Marlboro",
                    Ward = "Ward 10",
                    Description = "Truck dumped concrete blocks, drywall, and asbestos sheets onto municipal servitude.",
                    DateReported = DateTime.Now.AddHours(-72),
                    Priority = RequestPriority.Medium,
                    Status = RequestStatus.Submitted,
                    AssignedDepot = "Southern Waste & Parks Depot",
                    AssignedCrew = "Heavy Haulage Removal Crew",
                    EstimatedHours = 12
                },
                new ServiceRequest
                {
                    Id = 1007,
                    Title = "Blocked Stormwater Culvert Flooding Roadway",
                    Category = "Water & Utilities",
                    Location = "Jan Smuts Ave & 8th St",
                    Ward = "Ward 8",
                    Description = "Stormwater intake grate clogged with urban debris; water ponding across both south-bound lanes.",
                    DateReported = DateTime.Now.AddHours(-30),
                    Priority = RequestPriority.High,
                    Status = RequestStatus.UnderReview,
                    AssignedDepot = "North Roads & Stormwater Depot",
                    AssignedCrew = "Stormwater Clearing Crew B",
                    EstimatedHours = 4
                },
                new ServiceRequest
                {
                    Id = 1008,
                    Title = "Damaged Pedestrian Guardrail & Kerb",
                    Category = "Roads",
                    Location = "Jan Smuts Ave outside Civic Centre",
                    Ward = "Ward 8",
                    Description = "Steel guardrail mangled following collision; exposes pedestrians to fast-moving arterial traffic.",
                    DateReported = DateTime.Now.AddHours(-60),
                    Priority = RequestPriority.Medium,
                    Status = RequestStatus.InProgress,
                    AssignedDepot = "North Roads & Stormwater Depot",
                    AssignedCrew = "Civil Infrastructure Maintenance",
                    EstimatedHours = 8
                },
                new ServiceRequest
                {
                    Id = 1009,
                    Title = "Leaning Streetlight Pole Over Walkway",
                    Category = "Electricity",
                    Location = "Bram Fischer Drive, Randburg",
                    Ward = "Ward 14",
                    Description = "Concrete lighting mast damaged at base, tilting at 25-degree angle towards pedestrian sidewalk.",
                    DateReported = DateTime.Now.AddHours(-40),
                    Priority = RequestPriority.Medium,
                    Status = RequestStatus.UnderReview,
                    AssignedDepot = "Electricity Substation Alpha",
                    AssignedCrew = "Streetlight Maintenance Unit",
                    EstimatedHours = 5
                },
                new ServiceRequest
                {
                    Id = 1010,
                    Title = "Vandalized Children's Playground Swings",
                    Category = "Parks & Recreation",
                    Location = "Community Park, Delta Central",
                    Ward = "Ward 12",
                    Description = "Playground swings chains snapped and supporting steel framework unbolted by vandals.",
                    DateReported = DateTime.Now.AddHours(-90),
                    Priority = RequestPriority.Low,
                    Status = RequestStatus.Submitted,
                    AssignedDepot = "Southern Waste & Parks Depot",
                    AssignedCrew = "Parks Maintenance Crew",
                    EstimatedHours = 6
                },
                new ServiceRequest
                {
                    Id = 1011,
                    Title = "Public Water Standpipe Gasket Failure",
                    Category = "Water & Utilities",
                    Location = "Ward 7 Informal Settlement Hub",
                    Ward = "Ward 7",
                    Description = "Continuous valve leak wasting potable water supply; replaced brass washer and reseated tap.",
                    DateReported = DateTime.Now.AddDays(-5),
                    Priority = RequestPriority.Medium,
                    Status = RequestStatus.Resolved,
                    AssignedDepot = "Water Works Maintenance Station",
                    AssignedCrew = "Plumbing Maintenance Unit 3",
                    EstimatedHours = 2
                },
                new ServiceRequest
                {
                    Id = 1012,
                    Title = "Tree Branches Obscuring Regulatory Stop Sign",
                    Category = "Roads",
                    Location = "Corner 3rd Ave & 14th St",
                    Ward = "Ward 10",
                    Description = "Low-hanging acacia branches fully blocking motorist sightlines of stop sign; trimmed and cleared.",
                    DateReported = DateTime.Now.AddDays(-4),
                    Priority = RequestPriority.Medium,
                    Status = RequestStatus.Resolved,
                    AssignedDepot = "North Roads & Stormwater Depot",
                    AssignedCrew = "Horticulture Clearing Team",
                    EstimatedHours = 3
                },
                new ServiceRequest
                {
                    Id = 1013,
                    Title = "Illegal Chemical Waste Dump Near Stream",
                    Category = "Waste Management",
                    Location = "Braamfontein Spruit Greenbelt",
                    Ward = "Ward 14",
                    Description = "Multiple drums containing automotive coolant discarded near natural watercourse.",
                    DateReported = DateTime.Now.AddHours(-15),
                    Priority = RequestPriority.High,
                    Status = RequestStatus.Dispatched,
                    AssignedDepot = "Southern Waste & Parks Depot",
                    AssignedCrew = "Hazmat Remediation Unit",
                    EstimatedHours = 10
                },
                new ServiceRequest
                {
                    Id = 1014,
                    Title = "Perimeter Palisade Fence Stolen from Sports Ground",
                    Category = "Parks & Recreation",
                    Location = "Electoral Ward 7 Sports Complex",
                    Ward = "Ward 7",
                    Description = "Four sections of galvanized palisade fencing cut and stolen; repaired with welded reinforcement.",
                    DateReported = DateTime.Now.AddDays(-6),
                    Priority = RequestPriority.Low,
                    Status = RequestStatus.Resolved,
                    AssignedDepot = "Southern Waste & Parks Depot",
                    AssignedCrew = "Facilities Repair Team",
                    EstimatedHours = 14
                }
            };

            foreach (var req in seedItems)
            {
                // Add initial audit logs
                req.StatusHistory.Add(new StatusAuditEntry(RequestStatus.Submitted, "Service request submitted by citizen via mobile portal."));
                if (req.Status != RequestStatus.Submitted)
                {
                    req.StatusHistory.Add(new StatusAuditEntry(RequestStatus.UnderReview, "Incident evaluated by municipal dispatch dispatcher."));
                }
                if (req.Status == RequestStatus.Dispatched || req.Status == RequestStatus.InProgress || req.Status == RequestStatus.Resolved)
                {
                    req.StatusHistory.Add(new StatusAuditEntry(RequestStatus.Dispatched, $"Work order assigned to {req.AssignedCrew} from {req.AssignedDepot}."));
                }
                if (req.Status == RequestStatus.InProgress || req.Status == RequestStatus.Resolved)
                {
                    req.StatusHistory.Add(new StatusAuditEntry(RequestStatus.InProgress, "Field team on site. Repair operations actively underway."));
                }
                if (req.Status == RequestStatus.Resolved)
                {
                    req.StatusHistory.Add(new StatusAuditEntry(RequestStatus.QualityInspection, "Post-repair municipal inspection verified standards compliance."));
                    req.StatusHistory.Add(new StatusAuditEntry(RequestStatus.Resolved, "Repairs completed. Incident closed and citizen notified."));
                }

                AllRequests.Add(req);
                RequestMap[req.Id] = req;
            }
        }

        public void RebuildTrees()
        {
            Bst = new BinarySearchTree<int, ServiceRequest>();
            Avl = new AvlTree<int, ServiceRequest>();
            RedBlack = new RedBlackTree<int, ServiceRequest>();
            PriorityHeap = new ServiceRequestMinHeap();

            // Insert each request into trees and heap
            foreach (var req in AllRequests)
            {
                Bst.Insert(req.Id, req);
                Avl.Insert(req.Id, req);
                RedBlack.Insert(req.Id, req);

                if (req.Status != RequestStatus.Resolved)
                {
                    PriorityHeap.Enqueue(req);
                }
            }

            BuildAdministrativeHierarchyTree();
            BuildSampleBinaryTree();
        }

        private void BuildAdministrativeHierarchyTree()
        {
            AdministrativeTree = new GeneralTree<ServiceRequest>("Metropolitan Municipality Services");
            var root = AdministrativeTree.Root;

            var categories = AllRequests.Select(r => r.Category).Distinct().OrderBy(c => c);

            foreach (var category in categories)
            {
                var catNode = root.AddChild($"Directorate: {category}");
                var inCategory = AllRequests.Where(r => r.Category == category).ToList();

                var statuses = inCategory.Select(r => r.Status).Distinct().OrderBy(s => s);

                foreach (var status in statuses)
                {
                    var statusNode = catNode.AddChild($"Status: {status}");
                    var inStatus = inCategory.Where(r => r.Status == status).OrderBy(r => r.Id);

                    foreach (var req in inStatus)
                    {
                        statusNode.AddChild($"[{req.TrackingCode}] ({req.Priority}) {req.Title}", req);
                    }
                }
            }
        }

        private void BuildSampleBinaryTree()
        {
            var root = new BinaryTreeNode<string>("Municipal Emergency Command");
            root.Left = new BinaryTreeNode<string>("Infrastructure Operations");
            root.Right = new BinaryTreeNode<string>("Community & Social Services");

            root.Left.Left = new BinaryTreeNode<string>("Water & Roads Division");
            root.Left.Right = new BinaryTreeNode<string>("Power & Energy Division");

            root.Right.Left = new BinaryTreeNode<string>("Health & Sanitation");
            root.Right.Right = new BinaryTreeNode<string>("Parks & Public Spaces");

            SampleBinaryTree = new BinaryTree<string>(root);
        }

        private void BuildNetworkGraph()
        {
            NetworkGraph = new ServiceNetworkGraph();

            // Depots
            var dCentral = new GraphNode("DEPOT-CENTRAL", "Central Operations Depot", GraphNodeType.Depot, 400, 240);
            var dNorth = new GraphNode("DEPOT-NORTH", "North Roads Depot", GraphNodeType.Depot, 230, 110);
            var dWater = new GraphNode("DEPOT-WATER", "Water Works Station", GraphNodeType.Depot, 570, 110);
            var dElect = new GraphNode("DEPOT-ELECT", "Electrical Substation Alpha", GraphNodeType.Depot, 580, 360);
            var dSouth = new GraphNode("DEPOT-SOUTH", "Southern Waste & Parks Depot", GraphNodeType.Depot, 210, 360);

            NetworkGraph.AddNode(dCentral);
            NetworkGraph.AddNode(dNorth);
            NetworkGraph.AddNode(dWater);
            NetworkGraph.AddNode(dElect);
            NetworkGraph.AddNode(dSouth);

            // Incident Nodes mapped to Service Requests
            var loc1001 = new GraphNode("LOC-1001", "SR-1001: Sandton Dr", GraphNodeType.IncidentSite, 480, 160, 1001);
            var loc1002 = new GraphNode("LOC-1002", "SR-1002: Rivonia Rd", GraphNodeType.IncidentSite, 530, 260, 1002);
            var loc1003 = new GraphNode("LOC-1003", "SR-1003: Oxford Rd", GraphNodeType.IncidentSite, 310, 190, 1003);
            var loc1004 = new GraphNode("LOC-1004", "SR-1004: 5th Ave", GraphNodeType.IncidentSite, 350, 310, 1004);
            var loc1005 = new GraphNode("LOC-1005", "SR-1005: Katherine St", GraphNodeType.IncidentSite, 620, 220, 1005);
            var loc1006 = new GraphNode("LOC-1006", "SR-1006: 11th St", GraphNodeType.IncidentSite, 260, 280, 1006);
            var loc1007 = new GraphNode("LOC-1007", "SR-1007: Jan Smuts Culvert", GraphNodeType.IncidentSite, 160, 190, 1007);
            var loc1008 = new GraphNode("LOC-1008", "SR-1008: Jan Smuts Guardrail", GraphNodeType.IncidentSite, 140, 270, 1008);
            var loc1009 = new GraphNode("LOC-1009", "SR-1009: Bram Fischer", GraphNodeType.IncidentSite, 420, 390, 1009);
            var loc1010 = new GraphNode("LOC-1010", "SR-1010: Delta Central", GraphNodeType.IncidentSite, 490, 420, 1010);

            NetworkGraph.AddNode(loc1001);
            NetworkGraph.AddNode(loc1002);
            NetworkGraph.AddNode(loc1003);
            NetworkGraph.AddNode(loc1004);
            NetworkGraph.AddNode(loc1005);
            NetworkGraph.AddNode(loc1006);
            NetworkGraph.AddNode(loc1007);
            NetworkGraph.AddNode(loc1008);
            NetworkGraph.AddNode(loc1009);
            NetworkGraph.AddNode(loc1010);

            // Corridors / Road Edges (with distances in km)
            // Inter-depot backbone
            NetworkGraph.AddEdge("DEPOT-CENTRAL", "DEPOT-NORTH", 6.8);
            NetworkGraph.AddEdge("DEPOT-CENTRAL", "DEPOT-WATER", 7.2);
            NetworkGraph.AddEdge("DEPOT-CENTRAL", "DEPOT-ELECT", 5.9);
            NetworkGraph.AddEdge("DEPOT-CENTRAL", "DEPOT-SOUTH", 6.4);
            NetworkGraph.AddEdge("DEPOT-NORTH", "DEPOT-WATER", 9.5);
            NetworkGraph.AddEdge("DEPOT-ELECT", "DEPOT-SOUTH", 11.2);

            // Depots to Incidents
            NetworkGraph.AddEdge("DEPOT-WATER", "LOC-1001", 3.2);
            NetworkGraph.AddEdge("DEPOT-CENTRAL", "LOC-1001", 4.1);
            NetworkGraph.AddEdge("DEPOT-CENTRAL", "LOC-1002", 3.6);
            NetworkGraph.AddEdge("DEPOT-ELECT", "LOC-1002", 4.0);
            NetworkGraph.AddEdge("DEPOT-NORTH", "LOC-1003", 3.8);
            NetworkGraph.AddEdge("DEPOT-CENTRAL", "LOC-1003", 4.5);
            NetworkGraph.AddEdge("DEPOT-CENTRAL", "LOC-1004", 2.9);
            NetworkGraph.AddEdge("DEPOT-SOUTH", "LOC-1004", 4.7);
            NetworkGraph.AddEdge("DEPOT-ELECT", "LOC-1005", 3.4);
            NetworkGraph.AddEdge("LOC-1002", "LOC-1005", 2.6);
            NetworkGraph.AddEdge("DEPOT-SOUTH", "LOC-1006", 3.1);
            NetworkGraph.AddEdge("LOC-1004", "LOC-1006", 3.5);
            NetworkGraph.AddEdge("DEPOT-NORTH", "LOC-1007", 4.2);
            NetworkGraph.AddEdge("LOC-1007", "LOC-1008", 2.1);
            NetworkGraph.AddEdge("DEPOT-SOUTH", "LOC-1008", 4.0);
            NetworkGraph.AddEdge("DEPOT-CENTRAL", "LOC-1009", 4.8);
            NetworkGraph.AddEdge("DEPOT-ELECT", "LOC-1009", 5.1);
            NetworkGraph.AddEdge("LOC-1009", "LOC-1010", 2.4);
            NetworkGraph.AddEdge("DEPOT-CENTRAL", "LOC-1010", 6.0);
        }

        private void BuildDependencyGraph()
        {
            Dependencies = new DependencyGraph();

            foreach (var req in AllRequests)
            {
                Dependencies.AddRequest(req.Id);
            }

            // Real-world municipal prerequisites:
            // 1. Water main repair (1001) must complete before road pothole/trench repair (1003) in same sector
            Dependencies.AddDependency(1001, 1003);
            reqWithId(1003).DependsOnIds.Add(1001);

            // 2. High voltage cable isolation (1002) must complete before traffic signal controller rebuild (1005)
            Dependencies.AddDependency(1002, 1005);
            reqWithId(1005).DependsOnIds.Add(1002);

            // 3. Stormwater culvert clearing (1007) must complete before kerb/guardrail civil reconstruction (1008)
            Dependencies.AddDependency(1007, 1008);
            reqWithId(1008).DependsOnIds.Add(1007);
        }

        private ServiceRequest reqWithId(int id)
        {
            return RequestMap.ContainsKey(id) ? RequestMap[id] : null;
        }

        public void AddFromReportedIssue(Issue issue)
        {
            if (issue == null) return;

            // Determine priority based on category
            var priority = RequestPriority.Medium;
            if (issue.Category.IndexOf("Electricity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                issue.Category.IndexOf("Water", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                priority = RequestPriority.High;
            }
            else if (issue.Category.IndexOf("Safety", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                priority = RequestPriority.Critical;
            }

            var newReq = new ServiceRequest
            {
                Id = issue.Id >= 1000 ? issue.Id : _nextId++,
                Title = $"{issue.Category} report at {issue.Location}",
                Category = issue.Category,
                Location = issue.Location,
                Ward = "Citizen Ward",
                Description = issue.Description,
                DateReported = issue.DateReported,
                Priority = priority,
                Status = RequestStatus.Submitted,
                AssignedDepot = "Central Operations Depot",
                AssignedCrew = "Pending Dispatch",
                EstimatedHours = 4,
                AttachmentPaths = new List<string>(issue.AttachmentPaths)
            };

            newReq.StatusHistory.Add(new StatusAuditEntry(RequestStatus.Submitted, "Report submitted via Part 1 citizen issue reporting form."));

            AllRequests.Add(newReq);
            RequestMap[newReq.Id] = newReq;

            // Update all structures
            Bst.Insert(newReq.Id, newReq);
            Avl.Insert(newReq.Id, newReq);
            RedBlack.Insert(newReq.Id, newReq);
            PriorityHeap.Enqueue(newReq);
            Dependencies.AddRequest(newReq.Id);
            BuildAdministrativeHierarchyTree();
        }

        public void UpdateRequestStatus(int id, RequestStatus newStatus, string note)
        {
            if (!RequestMap.ContainsKey(id)) return;

            var req = RequestMap[id];
            req.UpdateStatus(newStatus, note);

            if (newStatus == RequestStatus.Resolved)
            {
                PriorityHeap.Remove(id);
            }

            BuildAdministrativeHierarchyTree();
        }

        public ServiceRequest DispatchNextUrgentRequest()
        {
            if (PriorityHeap.Count == 0) return null;

            var top = PriorityHeap.Dequeue();
            top.UpdateStatus(RequestStatus.Dispatched, "Dispatched to first available emergency crew via Min-Heap priority scheduler.");
            top.AssignedCrew = "Emergency Response Unit 1";
            BuildAdministrativeHierarchyTree();
            return top;
        }
    }
}
