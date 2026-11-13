# Updates Based on Lecturer Feedback

**Module:** Programming 3B (PROG7312) / Advanced Application Development (AAPD7112/w)  
**Student Name:** Aaron Adams  
**Student Number:** ST10376245  
**Assessment:** Portfolio of Evidence (PoE) — Part 3 Final Submission  
**Date:** 2026-10-20  

---

## 1. Executive Summary of Feedback & Iterative Improvement

Throughout the iterative lifecycle of the **Municipal Services Application for South Africa**, constructive feedback was gathered across Part 1 (Report Issues) and Part 2 (Local Events and Announcements). This document details how every piece of feedback provided by the lecturer and academic mentors was systematically analyzed, addressed, and integrated into the final Part 3 (PoE) application and documentation.

The feedback received highlighted five key areas for enhancement:
1. End-to-end integration and activation of the **Service Request Status** module.
2. Elevating data structure complexity from basic trees to self-balancing tree structures (AVL and Red-Black Trees).
3. Real-world grounding and practical justification for graph algorithms (BFS, DFS, and Minimum Spanning Trees).
4. Priority management for critical municipal emergencies using specialized heap data structures.
5. User interface consistency, responsiveness, and algorithmic diagnostic transparency.

---

## 2. Detailed Feedback & Enhancement Matrix

| Item # | Specific Lecturer Feedback Received | Architectural & Code Enhancement Implemented in Part 3 | Verification & Evidence in Codebase |
|---|---|---|---|
| **1** | *"In Part 1 and 2, the 'Service Request Status' tile was disabled. Ensure that for the final PoE, this feature is fully activated, user-friendly, and directly linked to newly submitted citizen reports so the system feels like a cohesive, end-to-end platform."* | **Full Activation & Repository Synchronization:**<br>1. Enabled the `btnServiceStatus` button on `MainMenuForm` and styled it consistently (`#0078C8`).<br>2. Linked `ReportIssueForm` directly to `ServiceRequestRepository.Instance.AddFromReportedIssue(issue)`, ensuring that any issue reported by a citizen in Part 1 is immediately populated into the Part 3 service request tracker, tree indexes, priority heap, and graph network.<br>3. Built `ServiceRequestStatusForm` with real-time milestone tracking (`Submitted` → `Under Review` → `Dispatched` → `In Progress` → `Inspection` → `Resolved`). | `MainMenuForm.cs`<br>`MainMenuForm.Designer.cs`<br>`ReportIssueForm.cs`<br>`ServiceRequestRepository.cs` |
| **2** | *"Basic Binary Search Trees risk degenerating into O(n) linked lists when keys are inserted sequentially. Implement self-balancing tree variants such as AVL Trees or Red-Black Trees to demonstrate worst-case O(log n) guarantees, and explain their trade-offs."* | **Implementation of AVL & Red-Black Trees:**<br>1. Developed a custom generic `AvlTree<TKey, TValue>` supporting height calculation, balance factor audits, and all four rotation cases (Left-Left, Right-Right, Left-Right, Right-Left).<br>2. Developed a custom generic `RedBlackTree<TKey, TValue>` maintaining all 5 Red-Black invariants using node coloring (`Red`/`Black`), parent pointers, sentinel `NIL` leaves, and insertion fix-up rotations.<br>3. Added an automated validation method (`VerifyProperties`) to audit the 5 Red-Black invariants on the live dataset.<br>4. Built an interactive ASCII tree diagram visualizer and benchmark panel comparing search comparisons across BST, AVL, and Red-Black trees. | `Data/Trees/AvlTree.cs`<br>`Data/Trees/RedBlackTree.cs`<br>`Data/Trees/BinarySearchTree.cs`<br>`ServiceRequestStatusForm.cs` (Tab 3) |
| **3** | *"Ensure graph algorithms are not presented as abstract theoretical exercises. Clearly connect Breadth-First Search, Depth-First Search, and Minimum Spanning Trees to municipal service delivery and resource optimization in South Africa."* | **Municipal Road Network & Maintenance Optimization:**<br>1. Modeled municipal depots (Central Depot, North Roads Depot, Water Works, Substation Alpha, Southern Depot) and incident sites as vertices with road distances (km) and travel times (mins).<br>2. Implemented **Kruskal's Algorithm** (with Disjoint Set Union / Union-Find) and **Prim's Algorithm** to compute the Minimum Spanning Tree (MST), showing a **59.9% reduction in road travel distance** (from 119.0 km down to 47.7 km) and saving an estimated R 228,000 in corridor maintenance costs.<br>3. Implemented a **Service Dependency DAG** with **Kahn's Topological Sort** to model prerequisite tasks (e.g. water pipe repairs must precede road trench resurfacing) and detect circular deadlock conditions.<br>4. Rendered the entire network on an interactive custom GDI+ graphical canvas. | `Data/Graphs/ServiceNetworkGraph.cs`<br>`Data/Graphs/DisjointSet.cs`<br>`Data/Graphs/DependencyGraph.cs`<br>`ServiceRequestStatusForm.cs` (Tab 5) |
| **4** | *"Municipal call centers face severe backlogs during storm events and infrastructure failures. Show how the application manages emergency priority dispatch so life-threatening faults are resolved first."* | **Binary Min-Heap Priority Dispatch Queue:**<br>1. Hand-crafted a custom `ServiceRequestMinHeap` prioritizing requests by urgency level: Critical (1), High (2), Medium (3), Low (4), with chronological FIFO tie-breaking.<br>2. Provided an $O(1)$ `Peek` to display the "Next Incident to Dispatch" and an $O(\log n)$ `Dequeue` that transitions the top emergency into active dispatch and assigns emergency response crews.<br>3. Added a simulated emergency logger (`+ Log Rapid Emergency Incident`) allowing evaluators to inject high-severity faults and watch them bubble to the top of the heap. | `Data/ServiceRequestMinHeap.cs`<br>`ServiceRequestStatusForm.cs` (Tab 4) |
| **5** | *"Improve visual consistency, responsiveness across screen resolutions, and transparent user feedback when data structures perform operations."* | **UI Modernization & Diagnostic Transparency:**<br>1. Standardized typography (Segoe UI) and color palette (`#005A9E` Primary Blue, `#0078C8` Accent Blue, `#009650` MST Green, `#C0392B` Critical Red) across all forms.<br>2. Refactored layout containers to dynamic WinForms `TableLayoutPanel` and `FlowLayoutPanel` docking to support responsive window resizing without clipping.<br>3. Added real-time diagnostic reporting showing comparison counts, CPU execution ticks, and step-by-step algorithm logs for every tree search, traversal, and MST computation. | `ServiceRequestStatusForm.cs`<br>`MainMenuForm.cs`<br>`LocalEventsForm.cs` |

---

## 3. Reflection on the Iterative Development Process

The progression from Part 1 to Part 3 highlights the benefits of iterative software engineering:
- **Part 1** established citizen engagement fundamentals, user input validation, media attachment handling, and basic linear storage (`List<Issue>`).
- **Part 2** introduced algorithmic filtering, multi-criteria querying, chronological sorting (`SortedDictionary`), priority queues (`MinPriorityQueue`), and associative lookups (`Dictionary<TKey, TValue>`).
- **Part 3** culminated in an integrated municipal enterprise suite combining self-balancing trees, priority heaps, graph traversals, topological task scheduling, and minimum spanning tree route optimization.

By listening actively to lecturer recommendations and applying rigorous algorithmic principles, the final platform successfully transitions from a prototype into a high-performance, fully functioning software system.
