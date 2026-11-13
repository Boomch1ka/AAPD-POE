# Updates Based on Lecturer Feedback

**Module:** Programming 3B (PROG7312) / Advanced Application Development (AAPD7112/w)  
**Student Name:** Aaron Adams  
**Student Number:** ST10376245  
**Assessment:** Portfolio of Evidence (PoE) - Part 3 Final Submission  
**Date:** 2026-11-13  

---

## 1. Summary of Feedback and Improvements

Across Part 1 (Report Issues) and Part 2 (Local Events and Announcements), feedback pointed to five main areas to address before submitting Part 3:

1. Activating the **Service Request Status** tile on the main menu and linking it to newly reported Part 1 issues.
2. Moving beyond a basic Binary Search Tree to self-balancing trees (AVL and Red-Black Trees) with clear comparisons.
3. Grounding graph algorithms (BFS, DFS, MST, and Topological Sort) in practical municipal road and repair workflows.
4. Handling emergency dispatch order using a custom Min-Heap priority queue instead of a flat list.
5. Keeping the UI layout consistent, responsive, and easy to inspect during live marking.

---

## 2. Feedback and Implementation Matrix

| Item | Feedback Focus | Changes Implemented in Part 3 | Files |
|---|---|---|---|
| **1** | **Activate Service Request Status & link Part 1 reports** | Enabled `btnServiceStatus` on `MainMenuForm` and wired `ReportIssueForm` directly into `ServiceRequestRepository.Instance.AddFromReportedIssue(issue)` so new faults appear immediately in Tab 1, all three search trees, and the Min-Heap. Added milestone tracking (`Submitted` -> `Under Review` -> `Dispatched` -> `In Progress` -> `Inspection` -> `Resolved`). | `MainMenuForm.cs`<br>`ReportIssueForm.cs`<br>`ServiceRequestRepository.cs` |
| **2** | **Add self-balancing trees (AVL & Red-Black)** | Implemented `AvlTree<TKey, TValue>` (with LL, RR, LR, RL rotations) and `RedBlackTree<TKey, TValue>` (with node colouring, `NIL` sentinels, and `VerifyProperties()` invariant checking). Added Tab 3 Tree Explorer with ASCII visualization and comparison counts. | `Data/Trees/AvlTree.cs`<br>`Data/Trees/RedBlackTree.cs`<br>`Data/Trees/BinarySearchTree.cs` |
| **3** | **Connect graphs & MSTs to real municipal tasks** | Built `ServiceNetworkGraph` (5 depots, 10 incidents, 19 weighted road corridors) with Kruskal's MST (using `DisjointSet` Union-Find), Prim's MST, BFS, and DFS, plus `DependencyGraph` with Kahn's Topological Sort. Added numbered orange visit-order badges for BFS/DFS and clickable node inspection on the GDI+ map. | `Data/Graphs/ServiceNetworkGraph.cs`<br>`Data/Graphs/DisjointSet.cs`<br>`Data/Graphs/DependencyGraph.cs`<br>`ServiceRequestStatusForm.cs` |
| **4** | **Prioritize urgent emergencies cleanly** | Built `ServiceRequestMinHeap` ordering requests by `Priority` (`Critical`, `High`, `Medium`, `Low`) and `DateReported`. Tab 4 supports O(1) `Peek`, O(log n) `Dequeue` dispatch, and rapid test incident injection. | `Data/ServiceRequestMinHeap.cs`<br>`ServiceRequestStatusForm.cs` (Tab 4) |
| **5** | **UI consistency, reporting & transparency** | Standardized the blue/navy palette and `TableLayoutPanel` docking across all forms. Added a live KPI summary strip (`Showing X of Y | Critical | Active | Resolved`) and an **Export CSV** button on Tab 1 so filtered requests can be saved cleanly. | `ServiceRequestStatusForm.cs`<br>`MainMenuForm.cs`<br>`LocalEventsForm.cs` |

---

## 3. Notes on the Iterative Build

Each part of the PoE built on top of the previous layer:
- **Part 1** handled citizen issue intake, validation, file attachments, and basic list storage (`List<Issue>`).
- **Part 2** added event filtering, `SortedDictionary`, `HashSet`, `Stack` viewing history, and search recommendations.
- **Part 3** tied the reported issues into self-balancing trees, a Min-Heap dispatch queue, and a weighted road/dependency graph with interactive visualization and CSV export.
