# Municipal Services Application - Comprehensive Portfolio of Evidence (PoE) Report & Guide

**Module:** Programming 3B (PROG7312) / Advanced Application Development (AAPD7112/w)  
**Student Name:** Aaron Adams  
**Student Number:** ST10376245  
**Final Submission:** Part 3 (PoE: Fully Functioning Municipal Platform)  
**Target Framework:** .NET Framework 4.8 (C# Windows Forms)  
**Repository Baseline:** Builds on Part 1 (Issue Reporting) and Part 2 (Local Events & Announcements), introducing Part 3 (Service Request Tracking, Self-Balancing Trees, Heap Dispatch, and Network Graph Optimization).

---

## 1. Executive Summary & Application Overview

The **Municipal Services Application** is a unified, resilient, and user-centric desktop software platform engineered for South African local municipalities. The platform empowers residents and municipal administrators across three core pillars:

1. **Part 1: Report Issues (Citizen Engagement & Issue Logging)**
   - Allows citizens to capture service breakdowns (potholes, water leaks, sewer overflows, downed powerlines, illegal dumping).
   - Captures location, category, rich descriptions, and multimedia attachments (`OpenFileDialog`).
   - Implements a gamified user engagement strategy (dynamic multi-tier progress indicator with contextual positive reinforcement).

2. **Part 2: Local Events and Announcements (Community Discovery & Recommendations)**
   - Displays municipal notices and community events in chronological order.
   - Advanced multi-filter search (keyword matching, category filtering, and date range bounding).
   - Smart Recommendation Engine based on frequency analysis and priority scoring, delivering personalized suggestions.
   - Priority banner for urgent emergency municipal announcements.

3. **Part 3: Service Request Status & Network Optimization (PoE Core Implementation)**
   - **Citizen Tracking Portal:** Real-time lookup of service requests via unique tracking codes (e.g., `SR-1001`) with a multi-stage visual milestone pipeline (`Submitted` → `Under Review` → `Dispatched` → `In Progress` → `Inspection` → `Resolved`).
   - **Self-Balancing Trees:** Implements custom **Binary Search Trees (BST)**, **AVL Trees** (with LL, RR, LR, RL height rotations), **Red-Black Trees** (with color invariants and insertion fix-up), **Binary Trees**, and an **N-ary General Administrative Tree** directly feeding a Windows Forms `TreeView`.
   - **Emergency Dispatch Heap:** Dedicated **Binary Min-Heap** prioritizing work orders by urgency level (Critical, High, Medium, Low) and FIFO timestamping.
   - **Municipal Road Network Graph:** Undirected weighted graph representing municipal depots and incident locations.
   - **Graph Traversals:** **Breadth-First Search (BFS)** for radial inspection sweeps and **Depth-First Search (DFS)** for deep circuit discovery.
   - **Minimum Spanning Tree (MST):** **Kruskal's Algorithm** (with Disjoint Set Union / Union-Find) and **Prim's Algorithm** optimizing municipal emergency dispatch corridors and reducing road travel distances by ~60%.
   - **Service Dependency DAG:** Directed Acyclic Graph modeling operational task prerequisites (e.g., pipe repair preceding road resurfacing) with **Kahn's Topological Sort** and circular deadlock detection.

---

## 2. System Requirements & Prerequisites

- **Operating System:** Windows 10 or Windows 11 (64-bit recommended)
- **Development Environment:** Visual Studio 2022 (Community, Professional, or Enterprise) with the **.NET Desktop Development** workload installed.
- **Runtime:** .NET Framework 4.8 Developer Pack or Runtime.
- **Command Line SDK (Optional):** .NET CLI (`dotnet` 8.0/9.0 with `net48` targeting pack).
- **Dependencies:** **Zero external NuGet dependencies!** All data structures (AVL Trees, Red-Black Trees, Heaps, Disjoint Sets, Graphs, Topological Sorters) are implemented by hand in pure C# to satisfy academic rigor and ensure 100% self-contained compilation.

---

## 3. Compilation & Execution Instructions

### Option A: Via Visual Studio 2022 (Recommended)
1. Open Visual Studio 2022.
2. Click **Open a project or solution** and navigate to `MunicipalServicesApp.sln`.
3. Set the build configuration to **Debug** or **Release** and platform to **Any CPU**.
4. Press **F5** (or **Ctrl+F5**) to compile and launch the application.

### Option B: Via .NET CLI / Command Prompt
1. Open PowerShell or Developer Command Prompt for VS.
2. Navigate to the project root directory:
   ```powershell
   cd "MunicipalServicesApp"
   ```
3. Restore and build the solution:
   ```powershell
   dotnet build
   ```
4. Run the executable:
   ```powershell
   dotnet run
   ```
   *Or execute the compiled binary directly:*
   `bin\Debug\net48\MunicipalServicesApp.exe`

---

## 4. User Guide: How to Use the Application

### 4.1. Main Menu Navigation
Upon startup, `MainMenuForm` presents three prominent primary navigation tiles:
1. **Report Issues:** Open the citizen reporting interface.
2. **Local Events and Announcements:** Open the municipal bulletin and recommendation portal.
3. **Service Request Status:** Open the comprehensive Part 3 tracking, tree explorer, priority dispatch, and network optimization hub.

---

### 4.2. Module 1: Report Issues (Part 1)
1. Enter the specific street address or landmark in the **Location** field.
2. Select a municipal service category from the **Category** dropdown (Sanitation, Roads, Water & Utilities, Electricity, Waste Management, Public Safety, Parks & Recreation).
3. Type a thorough explanation of the fault in the **Description** box.
4. Click **Attach Media** to attach relevant photos, inspection reports, or documents via `OpenFileDialog`.
5. Observe the **Gamified Engagement Progress Bar**: As each field is completed, the bar increments (0% → 25% → 50% → 75% → 100%) accompanied by positive motivational feedback.
6. Click **Submit**. A unique tracking ID is generated, and the report is **automatically propagated** into the Part 3 Service Request repository, trees, priority heap, and graph network!

---

### 4.3. Module 2: Local Events & Announcements (Part 2)
1. View upcoming community events sorted chronologically. Urgent alerts appear in red and in the top notice marquee.
2. **Search:** Enter search keywords, select a category, or specify a date range.
3. **Carousel & History:** Click through announcements using the queue carousel; double-click an event to inspect details; use the stack-based "Back" button to return to previous items.
4. **Smart Recommendations:** The recommendation engine tracks user queries and event views, displaying personalized event suggestions ranked by relevance score.

---

### 4.4. Module 3: Service Request Status & Network Optimization (Part 3)
The Part 3 hub is partitioned into 5 feature-rich tabs:

#### Tab 1: Service Requests Overview & Management
- **Search & Filter:** Filter requests by keyword, status (`Submitted`, `Under Review`, `Dispatched`, `In Progress`, `Quality Inspection`, `Resolved`), category, or priority level.
- **Numeric ID Search:** Typing a numeric ID performs an instant $O(\log n)$ lookup using the underlying AVL Tree!
- **Inspection Panel:** Displays complete incident data, assigned depot, response crew, estimated hours, and prerequisite dependencies.
- **Interactive Milestone Bar:** Visualizes the progress lifecycle with color-coded stage pills and completion ticks.
- **Status Audit Trail:** Full chronological log showing timestamp, status transition, dispatcher notes, and user tags.
- **Dispatcher Work Order Update:** Dispatchers can update the request status, input operational notes, and click **Apply**, which updates all data structures in real time.

#### Tab 2: Track by Unique ID (Citizen Portal)
- Enter any Request ID (e.g., `1001`, `1002`, `1005` or prefixed `SR-1001`).
- Click **Track Progress** (or click **Sample Urgent ID**).
- The AVL Tree index immediately retrieves the record, validates its milestone timeline, displays the assigned crew, and outputs the exact number of tree comparisons required to find the record.

#### Tab 3: Tree Data Structures Explorer
- **Selector:** Toggle between **Binary Search Tree (BST)**, **AVL Balanced Tree**, and **Red-Black Balanced Tree**.
- **Visual ASCII Hierarchy:** Renders a tree branch diagram in a monospace view. AVL nodes display balance factors and heights (e.g., `[ID: 1005 | BF: 0 | H: 3]`); Red-Black nodes display color tags `[RED]` and `[BLK]`.
- **Search Benchmark:** Enter an ID and benchmark search performance. Displays the visited search path (e.g., `1004 -> 1008 -> 1005`), comparison count, and elapsed CPU ticks.
- **Traversals:** Click **In-Order** (sorted ascending order), **Pre-Order**, **Post-Order**, or **Level-Order (BFS)** to view the exact output order.
- **Invariants Verification:** Click **Verify RB Properties** to execute an automated audit verifying that all 5 classic Red-Black invariants hold.
- **Administrative Hierarchy (N-ary Tree):** Browse the multi-tier `TreeView` organizing requests by `Directorate` → `Category` → `Status` → `Request`.

#### Tab 4: Priority Dispatch Queue (Binary Min-Heap)
- Visualizes the municipal dispatch queue ordered by urgency (Critical = 1, High = 2, Medium = 3, Low = 4).
- Top banner displays the **Next Incident to Dispatch** ($O(1)$ peek).
- Click **Dispatch Highest Priority Incident (Dequeue)** to extract the top-priority emergency in $O(\log n)$ time, transition its status to `Dispatched`, and assign an emergency crew.
- Click **+ Log Rapid Emergency Incident** to simulate an urgent incident (e.g. hazardous highway spillage) and observe it immediately bubble to the root of the heap.

#### Tab 5: Network Graph, Traversals & Minimum Spanning Tree (MST)
- **Interactive GDI+ Canvas:** Renders municipal depots (blue squares) and incident locations (circular pins), connected by road corridors with distance labels in kilometers.
- **Run Kruskal's MST:** Executes Kruskal's algorithm with Disjoint Set Union (Union-Find). The optimal infrastructure backbone is highlighted in emerald green. Computes distance saved (~60% reduction) and estimated maintenance budget savings in ZAR.
- **Run Prim's MST:** Select any depot from the dropdown and execute Prim's cut-growing algorithm. Confirms identical optimal spanning tree weight.
- **Run BFS Traversal:** Explores outward from the selected depot level by level, logging hop counts and shortest-hop connection routes.
- **Run DFS Traversal:** Explores long-range continuous corridors and verifies network reachability.
- **Analyze DAG Dependencies:** Computes Kahn's Topological Sort across dependent service requests (e.g. pipe isolation → trench backfill → asphalt paving) and verifies zero circular deadlocks.

---

## 5. In-Depth Data Structure Technical Analysis

The application incorporates a comprehensive suite of foundational and advanced data structures. Each structure was selected and implemented to solve a distinct operational challenge within the municipal platform.

### Summary Matrix of Data Structures

| Data Structure | Implementation File | Role in Municipal Application | Search Time | Insert Time | Space Complexity |
|---|---|---|---|---|---|
| **Binary Search Tree (BST)** | `BinarySearchTree.cs` | Request ID indexing & sorted in-order traversal | Average $O(\log n)$, Worst $O(n)$ | Average $O(\log n)$, Worst $O(n)$ | $O(n)$ |
| **AVL Tree** | `AvlTree.cs` | Strictly balanced search index for rapid citizen tracking | Guaranteed $O(\log n)$ | Guaranteed $O(\log n)$ | $O(n)$ |
| **Red-Black Tree** | `RedBlackTree.cs` | Balanced search index with minimal rotation overhead during frequent inserts | Guaranteed $O(\log n)$ | Guaranteed $O(\log n)$ | $O(n)$ |
| **General / N-ary Tree** | `GeneralTree.cs` | Administrative municipal hierarchy (Directorate → Dept → Status) | $O(n)$ | $O(1)$ child append | $O(n)$ |
| **Binary Tree** | `BinaryTree.cs` | Hierarchical command traversal (In, Pre, Post, Level order) | $O(n)$ | $O(1)$ | $O(n)$ |
| **Binary Min-Heap** | `ServiceRequestMinHeap.cs` | Priority dispatch queue prioritizing critical emergencies | Peek $O(1)$ | Enqueue $O(\log n)$, Dequeue $O(\log n)$ | $O(n)$ |
| **Graph (Adjacency List)** | `ServiceNetworkGraph.cs` | Municipal road corridors connecting depots and incidents | $O(V + E)$ | $O(1)$ edge append | $O(V + E)$ |
| **BFS Traversal** | `ServiceNetworkGraph.cs` | Radial inspection wave and minimum-hop depot radius | $O(V + E)$ | - | $O(V)$ |
| **DFS Traversal** | `ServiceNetworkGraph.cs` | Long-distance maintenance circuits & reachability | $O(V + E)$ | - | $O(V)$ |
| **Kruskal's MST** | `ServiceNetworkGraph.cs` | Optimal emergency infrastructure maintenance network | $O(E \log E)$ | - | $O(V + E)$ |
| **Prim's MST** | `ServiceNetworkGraph.cs` | Vertex-cut minimum spanning tree from designated depot | $O(V^2)$ / $O(E \log V)$ | - | $O(V)$ |
| **Disjoint Set (Union-Find)** | `DisjointSet.cs` | Cycle detection with path compression & union-by-rank | Near $O(1)$ ($\alpha(n)$) | $O(1)$ | $O(V)$ |
| **Dependency DAG & TopoSort** | `DependencyGraph.cs` | Work order sequencing avoiding circular deadlocks | $O(V + E)$ | $O(1)$ dependency add | $O(V + E)$ |
| **SortedDictionary** (Part 2) | `EventRepository.cs` | Chronological event organization | $O(\log n)$ | $O(\log n)$ | $O(n)$ |
| **Hash Tables / Dictionaries** | `EventRepository.cs` | Instant category filtering and ID lookups | $O(1)$ average | $O(1)$ average | $O(n)$ |
| **Sets (SortedSet / HashSet)** | `EventRepository.cs` | Unique category and date handling without duplicates | $O(1)$ / $O(\log n)$ | $O(1)$ / $O(\log n)$ | $O(n)$ |
| **Stacks & Queues** (Part 2) | `LocalEventsForm.cs` | Navigation back-stack and announcement rotation carousel | $O(1)$ push/pop | $O(1)$ enqueue/dequeue | $O(n)$ |
| **Generic List** (Part 1) | `IssueRepository.cs` | Sequential in-memory issue submission store | $O(n)$ | $O(1)$ amortized | $O(n)$ |

---

### Detailed Structure Analysis

#### 1. Binary Search Tree (BST)
- **Role:** Organizes service requests indexed by their unique integer ID (`1001`, `1002`, etc.).
- **Mechanism:** Maintains the binary search property: for every node $N$, all keys in the left subtree are smaller than $N.\text{Key}$, and all keys in the right subtree are greater.
- **Contribution:** Allows in-order traversal which naturally outputs all service requests in ascending chronological order of their ID in linear $O(n)$ time.
- **Example:** Searching for Request `1005` starts at root `1004`, branches right to `1008`, branches left to `1005`, locating the record in 3 comparisons rather than sequentially scanning through 14 records.

#### 2. AVL Tree (Self-Balancing Binary Search Tree)
- **Role:** Provides guaranteed worst-case $O(\log n)$ retrieval for high-traffic citizen status lookups.
- **Mechanism:** Enforces the strict AVL invariant: for every node, the height difference between left and right subtrees (the **Balance Factor**) cannot exceed $\pm 1$:
  $$\text{BalanceFactor}(N) = \text{Height}(N.\text{Left}) - \text{Height}(N.\text{Right}) \in \{-1, 0, 1\}$$
  When an insertion violates this condition, the tree immediately performs one of four localized rotations:
  - **Left-Left (LL):** Single Right Rotation.
  - **Right-Right (RR):** Single Left Rotation.
  - **Left-Right (LR):** Left Rotation on left child, followed by Right Rotation on parent.
  - **Right-Left (RL):** Right Rotation on right child, followed by Left Rotation on parent.
- **Contribution:** Eliminates tree degeneration into an $O(n)$ linked list, guaranteeing that even with thousands of sequentially submitted requests, search lookups never exceed $\approx 1.44 \log_2 n$ comparisons.

#### 3. Red-Black Tree
- **Role:** Balances service requests with lower rotational overhead during frequent write/insert workloads.
- **Mechanism:** Implements an augmented binary search tree where each node is colored `Red` or `Black`, adhering to all five foundational invariants:
  1. Every node is either Red or Black.
  2. The root is always Black.
  3. Every null leaf (NIL sentinel) is Black.
  4. If a node is Red, both of its children must be Black (no two consecutive red nodes).
  5. Every path from a given node to any of its descendant NIL leaves contains the same number of Black nodes (**Black Height**).
- **Contribution:** Insertion requires at most two tree rotations and localized color flips ($O(\log n)$ insertion fix-up). The application includes an automated verification algorithm (`VerifyProperties`) that dynamically audits and confirms all 5 invariants across the live dataset.

#### 4. Binary Min-Heap (Priority Dispatch Queue)
- **Role:** Drives the municipal emergency dispatch engine, ensuring that life-threatening service breakdowns (e.g., active water main fractures, live downed power lines) are dispatched ahead of routine maintenance (e.g., park bench repainting).
- **Mechanism:** Built as a complete binary tree stored in a compact generic `List<ServiceRequest>`. For any index $i$, its left child is at $2i + 1$, right child at $2i + 2$, and parent at $\lfloor (i - 1)/2 \rfloor$.
  - The heap invariant requires $\text{Priority}(\text{Parent}) \le \text{Priority}(\text{Child})$.
  - Tie-breaking is performed chronologically (FIFO based on `DateReported`).
  - `Enqueue` appends to the end and executes `SiftUp` in $O(\log n)$ time.
  - `Dequeue` extracts the root, places the last element at the root, and executes `SiftDown` in $O(\log n)$ time.
  - `Peek` inspects the root in $O(1)$ time.
- **Contribution:** Allows municipal dispatchers to instantly identify and mobilize crews for the most critical incident without scanning or re-sorting the active queue.

#### 5. Municipal Service Road Network Graph
- **Role:** Models the physical geographic connectivity and transit corridors connecting municipal depots to citizen incident locations.
- **Mechanism:** Implemented as an undirected weighted graph via an **Adjacency List** (`Dictionary<string, List<GraphEdge>>`). Vertices store municipal labels, types (`Depot` vs. `IncidentSite`), and 2D geographic coordinates $(X, Y)$ rendered onto an interactive WinForms GDI+ canvas.
- **Contribution:** Supports multi-depot routing, distance accounting, transit time estimation, and infrastructural pathfinding.

#### 6. Graph Traversals (Breadth-First Search & Depth-First Search)
- **Breadth-First Search (BFS):**
  - Uses an explicit `Queue<string>` to explore nodes layer by layer from an epicenter.
  - Computes the minimum number of road hops from a municipal depot to all incident sites.
  - Used by municipal coordinators to establish emergency inspection radiuses and assign secondary response zones.
- **Depth-First Search (DFS):**
  - Uses recursion and backtracking to explore continuous road corridors as deep as possible before backtracking.
  - Used to identify connected road circuits, verify total network reachability, and detect isolated or partitioned urban wards.

#### 7. Minimum Spanning Tree (Kruskal's & Prim's Algorithms)
- **Role:** Solves the municipal maintenance dispatch and utility network optimization problem:
  > *"Determine the minimum-cost connected network of road corridors that links all municipal depots and active incident sites without forming redundant cycles."*
- **Kruskal's Algorithm:**
  - Sorts all graph edges in non-decreasing order of distance ($O(E \log E)$).
  - Greedily selects edges, using a custom **Disjoint Set Union (Union-Find)** data structure with path compression and union-by-rank to prevent cycle formation in near $O(1)$ amortized time.
  - Halts once exactly $V - 1$ edges are selected.
- **Prim's Algorithm:**
  - Starts at a designated municipal depot (e.g. `DEPOT-CENTRAL`) and greedily expands the minimum-weight cut connecting visited vertices to unvisited vertices.
- **Real-World Impact:** In the municipal test dataset, the full road network spanning 15 locations encompasses **119.0 km** of transit corridors. The computed Minimum Spanning Tree connects all facilities with only **47.7 km** of road network—a **59.9% reduction in travel distance**, saving municipal fuel, reducing fleet carbon emissions, and saving an estimated R 228,000 in corridor maintenance costs!

#### 8. Service Dependency Graph (DAG) & Kahn's Topological Sort
- **Role:** Prevents operational conflicts where municipal teams attempt repairs out of sequence.
- **Scenario:** Repairing an eroded road surface (SR-1003) cannot occur while a burst water pipe directly underneath (SR-1001) is actively discharging water; high-voltage cable repair (SR-1002) must precede traffic signal controller rebuilds (SR-1005).
- **Mechanism:** Formulates tasks as a Directed Acyclic Graph (DAG) and computes **Kahn's Algorithm** using in-degree tracking and a processing queue.
- **Contribution:** Produces a guaranteed, non-conflicting step-by-step resolution sequence and flags any circular dependency deadlocks before field crews deploy.

---

## 6. Project Completion Retrospective & Learnings

### 6.1. Task 3 Challenges Overcome
1. **Red-Black Tree Insertion Fix-up:** Enforcing all 5 Red-Black invariants required meticulous handling of parent, uncle, and grandparent pointers across both left-leaning and right-leaning cases, solved using a shared sentinel `_nil` black node.
2. **AVL Double Rotations:** Implementing balanced height updates without recursion overhead was accomplished by tracking balance factors and chaining child-parent rotations.
3. **WinForms GDI+ Dynamic Graph Rendering:** Mapping abstract graph coordinates into a dynamically resizing Windows Forms panel was resolved using coordinate normalization, anti-aliased geometry, and custom double-buffered painting.
4. **Disjoint Set Cycle Detection:** Integrating Union-Find with path compression allowed Kruskal's algorithm to process edges in near-linear time while generating a comprehensive audit log of accepted versus rejected cycle edges.

### 6.2. Key Learnings
- **Algorithmic Selection Trade-offs:** Gained deep practical insight into when to favor AVL Trees (read-heavy citizen lookup workloads requiring strict balance) versus Red-Black Trees (write-heavy workloads with frequent insertions requiring minimal rotation rebalancing).
- **Data Structure Synergy:** Discovered how combining multiple specialized structures (e.g. AVL index for $O(\log n)$ search, Min-Heap for $O(\log n)$ dispatch, and Graph MST for $O(E \log E)$ routing) produces an enterprise-grade municipal management system.
- **Architectural Modularity:** Decoupled data structures into clean generic classes (`AvlTree<TKey, TValue>`, `RedBlackTree<TKey, TValue>`, `MinPriorityQueue<T>`), keeping domain logic distinct from presentation.

---

## 7. Technology Recommendations for Future Expansion

1. **Relational Database & Entity Framework Core (PostgreSQL / SQL Server):**
   - *Recommendation:* Replace the current in-memory repositories with EF Core backed by PostgreSQL.
   - *Justification:* Guarantees ACID transactional durability across application restarts, supports spatial queries (PostGIS), and allows multi-user concurrent access for hundreds of municipal call center agents.

2. **Geographic Information Systems (GIS) & Interactive OpenStreetMap Integration:**
   - *Recommendation:* Embed a modern map control (e.g., GMap.NET or WebView2 with Leaflet / OpenStreetMap).
   - *Justification:* Replaces schematic coordinate plots with high-resolution satellite imagery, real GPS coordinates, turn-by-turn road routing, and ward boundary overlays.

3. **Real-Time Push Notification Engine (ASP.NET Core SignalR):**
   - *Recommendation:* Implement SignalR WebSockets for instant two-way synchronization.
   - *Justification:* Pushes immediate dispatch alerts and milestone updates directly to citizen smartphones and field technicians the moment a dispatcher updates a work order.

4. **Cross-Platform Mobile Expansion (.NET MAUI):**
   - *Recommendation:* Port the citizen reporting and field crew dispatch forms to a cross-platform .NET MAUI application for Android and iOS.
   - *Justification:* Enables field technicians to update request statuses, scan QR codes, and upload photo evidence directly from job sites even in offline environments.

---

## 8. Updates Based on Feedback from Lecturer

| Feedback Item Received | Specific Application Enhancement Implemented |
|---|---|
| **1. Service Request Tracking Access:** Part 1 and 2 kept request tracking disabled; requested seamless end-to-end integration. | Enabled the **Service Request Status** tile on `MainMenuForm`. Linked `ReportIssueForm` so that newly reported citizen issues immediately flow into the central repository, trees, priority heap, and graph network. |
| **2. Self-Balancing Tree Implementation:** Advised implementing self-balancing tree structures rather than basic BSTs alone. | Implemented full **AVL Tree** (with height tracking and LL, RR, LR, RL rotations) and **Red-Black Tree** (with 5 invariants and fix-up recoloring). Added visual ASCII tree rendering and an automated invariant validation engine. |
| **3. Real-World Municipal Graph Application:** Requested clear municipal justification for Graph traversals and MST. | Modeled real municipal facilities and incident sites with weighted road distances (km). Implemented **Kruskal's MST** and **Prim's MST** to solve the municipal maintenance corridor optimization problem, reducing required travel distance by ~60%. Implemented a **Service Dependency DAG** with Kahn's Topological Sort to prevent operational sequencing deadlocks. |
| **4. Dispatch Prioritization:** Recommended demonstrating practical queue management for urgent municipal emergencies. | Built a custom **Binary Min-Heap** prioritizing work orders by urgency level (Critical, High, Medium, Low) and timestamp, with $O(1)$ peek and $O(\log n)$ dequeue operations. |
| **5. Interface Responsiveness & Diagnostics:** Urged consistent UI styling and transparent performance metrics. | Standardized the color palette across all forms (`PrimaryBlue`, `AccentBlue`, `MstGreen`), implemented responsive layouts, and added real-time diagnostic cards showing comparison counts, CPU ticks, and algorithm logs. |

---

## 9. Conclusion

The Municipal Services Application represents a comprehensive, fully functional, and academically rigorous software solution. By marrying intuitive, accessible Windows Forms interfaces with hand-crafted, high-performance data structures—spanning self-balancing binary search trees, priority heaps, and minimum spanning tree road graphs—the application demonstrates both technical mastery and practical relevance to the South African municipal service delivery landscape.
