using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MunicipalServicesApp.Data;
using MunicipalServicesApp.Data.Graphs;
using MunicipalServicesApp.Data.Trees;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp
{
    /// <summary>
    /// Service Request Status Page for Part 3 (PoE).
    /// Provides comprehensive tracking of municipal service requests,
    /// integrating all required advanced data structures:
    /// - Binary Search Trees, AVL Trees, Red-Black Trees, Binary Trees, General Hierarchical Trees
    /// - Binary Min-Heap Priority Queue for Dispatch
    /// - Municipal Road Network Graph with BFS, DFS, Kruskal's & Prim's MST
    /// - Dependency Graph (DAG) with Kahn's Topological Sorting
    /// </summary>
    public class ServiceRequestStatusForm : Form
    {
        // Color Palette
        private static readonly Color PrimaryBlue = Color.FromArgb(0, 90, 158);
        private static readonly Color AccentBlue = Color.FromArgb(0, 120, 200);
        private static readonly Color DarkHeader = Color.FromArgb(24, 43, 73);
        private static readonly Color MstGreen = Color.FromArgb(0, 150, 80);
        private static readonly Color CriticalRed = Color.FromArgb(192, 57, 43);
        private static readonly Color HighOrange = Color.FromArgb(211, 84, 0);
        private static readonly Color MediumYellow = Color.FromArgb(180, 130, 0);
        private static readonly Color LowGray = Color.FromArgb(108, 122, 137);

        private readonly ServiceRequestRepository _repo = ServiceRequestRepository.Instance;

        // Form Controls
        private TabControl tabControl;

        // Tab 1: Overview Controls
        private TextBox txtSearchOverview;
        private ComboBox cmbStatusFilter;
        private ComboBox cmbCategoryFilter;
        private ComboBox cmbPriorityFilter;
        private ListView lvRequests;
        private Label lblOverviewSummary;
        private Label lblSelectedTitle;
        private Label lblSelectedTracking;
        private Label lblSelectedPriority;
        private Label lblSelectedStatus;
        private Label lblSelectedDepot;
        private Label lblSelectedCrew;
        private Label lblSelectedPrereqs;
        private TextBox txtSelectedDescription;
        private ListBox lstAuditHistory;
        private ComboBox cmbUpdateStatus;
        private TextBox txtStatusNote;
        private Button btnApplyStatusUpdate;
        private FlowLayoutPanel pnlStageTracker;

        // Tab 2: Track by ID Controls
        private TextBox txtTrackId;
        private Label lblTrackStatusBadge;
        private Label lblTrackTitle;
        private Label lblTrackDetails;
        private ListBox lstTrackAudit;
        private FlowLayoutPanel pnlTrackStages;

        // Tab 3: Tree Explorer Controls
        private RadioButton rbBst;
        private RadioButton rbAvl;
        private RadioButton rbRedBlack;
        private TextBox txtTreeSearchId;
        private Label lblTreeSearchPath;
        private Label lblTreeMetrics;
        private RichTextBox rtbTreeAscii;
        private TreeView tvAdministrative;
        private ListBox lstTraversalOutput;

        // Tab 4: Priority Heap Controls
        private ListView lvHeap;
        private Label lblNextUrgent;
        private Label lblHeapCount;

        // Tab 5: Graph & MST Controls
        private Panel pnlGraphCanvas;
        private RichTextBox rtbGraphLog;
        private Label lblMstStats;
        private ComboBox cmbStartDepot;
        private MstResult _lastMstResult;
        private TraversalResult _lastTraversalResult;
        private string _selectedGraphNodeId;

        public ServiceRequestStatusForm()
        {
            InitializeComponentLayout();
            LoadDataIntoOverview();
            RefreshHeapView();
            RefreshTreeExplorer();
            PopulateAdministrativeTreeView();
            PopulateDepotDropdown();
        }

        private void InitializeComponentLayout()
        {
            Text = "Municipal Services Application - Service Request Status & Network Optimization";
            ClientSize = new Size(1180, 760);
            MinimumSize = new Size(1020, 680);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9.5F);

            var rootTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(12)
            };
            rootTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            rootTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            rootTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            Controls.Add(rootTable);

            // 1. Header Bar
            var pnlHeader = new Panel { Dock = DockStyle.Fill, BackColor = PrimaryBlue };
            var lblHeaderTitle = new Label
            {
                Text = "Service Request Status & Infrastructure Operations Hub",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(16, 8),
                AutoSize = true
            };
            var lblHeaderSub = new Label
            {
                Text = "Real-time Tracking • Self-Balancing Trees • Priority Queue Dispatch • Minimum Spanning Tree Routing",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(220, 235, 252),
                Location = new Point(18, 34),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(lblHeaderSub);
            rootTable.Controls.Add(pnlHeader, 0, 0);

            // 2. Tab Control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
            };
            tabControl.TabPages.Add(BuildOverviewTab());
            tabControl.TabPages.Add(BuildTrackByIdTab());
            tabControl.TabPages.Add(BuildTreeExplorerTab());
            tabControl.TabPages.Add(BuildPriorityHeapTab());
            tabControl.TabPages.Add(BuildGraphMstTab());
            rootTable.Controls.Add(tabControl, 0, 1);

            // 3. Footer Bar
            var pnlFooter = new Panel { Dock = DockStyle.Fill };
            var btnBack = new Button
            {
                Text = "Back to Main Menu",
                BackColor = PrimaryBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Size = new Size(160, 36),
                Dock = DockStyle.Right
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => Close();

            var lblFooterInfo = new Label
            {
                Text = "Powered by Custom Data Structures: AVL Tree, Red-Black Tree, Binary Min-Heap, Kruskal/Prim MST, and DAG Topological Sort.",
                ForeColor = Color.FromArgb(100, 110, 125),
                Font = new Font("Segoe UI", 8.5F),
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true
            };

            pnlFooter.Controls.Add(lblFooterInfo);
            pnlFooter.Controls.Add(btnBack);
            rootTable.Controls.Add(pnlFooter, 0, 2);
        }

        // ============================================================
        // TAB 1: OVERVIEW & MANAGEMENT
        // ============================================================
        private TabPage BuildOverviewTab()
        {
            var page = new TabPage("Service Requests Overview") { BackColor = Color.White, Padding = new Padding(8) };

            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            split.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Top Filter Bar
            var filterPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(2) };
            filterPanel.Controls.Add(new Label { Text = "Search:", AutoSize = true, Margin = new Padding(0, 6, 2, 0) });
            txtSearchOverview = new TextBox { Width = 130 };
            txtSearchOverview.TextChanged += (s, e) => ApplyOverviewFilter();
            filterPanel.Controls.Add(txtSearchOverview);

            filterPanel.Controls.Add(new Label { Text = "Status:", AutoSize = true, Margin = new Padding(8, 6, 2, 0) });
            cmbStatusFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
            cmbStatusFilter.Items.AddRange(new object[] { "All", "Submitted", "UnderReview", "Dispatched", "InProgress", "QualityInspection", "Resolved" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => ApplyOverviewFilter();
            filterPanel.Controls.Add(cmbStatusFilter);

            filterPanel.Controls.Add(new Label { Text = "Category:", AutoSize = true, Margin = new Padding(8, 6, 2, 0) });
            cmbCategoryFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
            cmbCategoryFilter.Items.AddRange(new object[] { "All", "Water & Utilities", "Electricity", "Roads", "Sanitation", "Waste Management", "Parks & Recreation" });
            cmbCategoryFilter.SelectedIndex = 0;
            cmbCategoryFilter.SelectedIndexChanged += (s, e) => ApplyOverviewFilter();
            filterPanel.Controls.Add(cmbCategoryFilter);

            filterPanel.Controls.Add(new Label { Text = "Priority:", AutoSize = true, Margin = new Padding(8, 6, 2, 0) });
            cmbPriorityFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
            cmbPriorityFilter.Items.AddRange(new object[] { "All", "Critical", "High", "Medium", "Low" });
            cmbPriorityFilter.SelectedIndex = 0;
            cmbPriorityFilter.SelectedIndexChanged += (s, e) => ApplyOverviewFilter();
            filterPanel.Controls.Add(cmbPriorityFilter);

            var btnReset = new Button { Text = "Reset", Width = 65, Height = 26 };
            btnReset.Click += (s, e) =>
            {
                txtSearchOverview.Clear();
                cmbStatusFilter.SelectedIndex = 0;
                cmbCategoryFilter.SelectedIndex = 0;
                cmbPriorityFilter.SelectedIndex = 0;
            };
            filterPanel.Controls.Add(btnReset);

            var btnExportCsv = new Button
            {
                Text = "Export CSV",
                Width = 85,
                Height = 26,
                BackColor = PrimaryBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(6, 2, 0, 0)
            };
            btnExportCsv.Click += (s, e) => ExportOverviewCsv();
            filterPanel.Controls.Add(btnExportCsv);

            split.Controls.Add(filterPanel, 0, 0);
            split.SetColumnSpan(filterPanel, 2);

            // Left: KPI Summary Bar + List View
            var pnlLeftOverview = new Panel { Dock = DockStyle.Fill };
            lblOverviewSummary = new Label
            {
                Dock = DockStyle.Top,
                Height = 26,
                BackColor = Color.FromArgb(240, 245, 252),
                ForeColor = DarkHeader,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 8, 0),
                Text = "Total: 0 | Critical: 0 | Active: 0 | Resolved: 0"
            };

            lvRequests = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };
            lvRequests.Columns.Add("ID", 60);
            lvRequests.Columns.Add("Priority", 75);
            lvRequests.Columns.Add("Category", 125);
            lvRequests.Columns.Add("Location", 155);
            lvRequests.Columns.Add("Status", 110);
            lvRequests.Columns.Add("Date Reported", 110);
            lvRequests.SelectedIndexChanged += (s, e) => ShowSelectedRequestDetails();
            pnlLeftOverview.Controls.Add(lvRequests);
            pnlLeftOverview.Controls.Add(lblOverviewSummary);
            split.Controls.Add(pnlLeftOverview, 0, 1);

            // Right: Detail Panel
            var pnlDetail = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(8) };
            pnlDetail.BackColor = Color.FromArgb(250, 252, 255);
            pnlDetail.BorderStyle = BorderStyle.FixedSingle;

            lblSelectedTracking = new Label { Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = PrimaryBlue, Location = new Point(10, 10), AutoSize = true };
            lblSelectedStatus = new Label { Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Location = new Point(140, 12), AutoSize = true };
            lblSelectedPriority = new Label { Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Location = new Point(270, 12), AutoSize = true };
            lblSelectedTitle = new Label { Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = DarkHeader, Location = new Point(10, 38), Size = new Size(380, 40) };

            var lblDescTag = new Label { Text = "Description:", Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(10, 80), AutoSize = true };
            txtSelectedDescription = new TextBox { Location = new Point(10, 100), Size = new Size(380, 55), Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = Color.White };

            lblSelectedDepot = new Label { Location = new Point(10, 162), Size = new Size(380, 20), Text = "Depot: -" };
            lblSelectedCrew = new Label { Location = new Point(10, 184), Size = new Size(380, 20), Text = "Assigned Crew: -" };
            lblSelectedPrereqs = new Label { Location = new Point(10, 206), Size = new Size(380, 20), Text = "Dependencies: None", ForeColor = PrimaryBlue };

            // Stage Tracker Flow Panel
            var lblStageTag = new Label { Text = "Progress Milestones:", Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(10, 230), AutoSize = true };
            pnlStageTracker = new FlowLayoutPanel { Location = new Point(10, 250), Size = new Size(380, 48), WrapContents = false };

            // Audit History
            var lblAuditTag = new Label { Text = "Status Audit Trail:", Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(10, 305), AutoSize = true };
            lstAuditHistory = new ListBox { Location = new Point(10, 325), Size = new Size(380, 110), Font = new Font("Consolas", 8.5F) };

            // Update Status GroupBox
            var grpUpdate = new GroupBox { Text = "Update Status & Work Order", Location = new Point(10, 445), Size = new Size(380, 115) };
            grpUpdate.Controls.Add(new Label { Text = "Next Status:", Location = new Point(12, 24), AutoSize = true });
            cmbUpdateStatus = new ComboBox { Location = new Point(95, 20), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUpdateStatus.Items.AddRange(new object[] { "UnderReview", "Dispatched", "InProgress", "QualityInspection", "Resolved" });
            cmbUpdateStatus.SelectedIndex = 0;
            grpUpdate.Controls.Add(cmbUpdateStatus);

            grpUpdate.Controls.Add(new Label { Text = "Dispatcher Note:", Location = new Point(12, 54), AutoSize = true });
            txtStatusNote = new TextBox { Location = new Point(12, 74), Width = 250, Text = "Crew on site conducting repairs." };
            grpUpdate.Controls.Add(txtStatusNote);

            btnApplyStatusUpdate = new Button
            {
                Text = "Apply",
                Location = new Point(275, 71),
                Size = new Size(95, 28),
                BackColor = AccentBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnApplyStatusUpdate.Click += (s, e) => ApplySelectedStatusUpdate();
            grpUpdate.Controls.Add(btnApplyStatusUpdate);

            pnlDetail.Controls.Add(lblSelectedTracking);
            pnlDetail.Controls.Add(lblSelectedStatus);
            pnlDetail.Controls.Add(lblSelectedPriority);
            pnlDetail.Controls.Add(lblSelectedTitle);
            pnlDetail.Controls.Add(lblDescTag);
            pnlDetail.Controls.Add(txtSelectedDescription);
            pnlDetail.Controls.Add(lblSelectedDepot);
            pnlDetail.Controls.Add(lblSelectedCrew);
            pnlDetail.Controls.Add(lblSelectedPrereqs);
            pnlDetail.Controls.Add(lblStageTag);
            pnlDetail.Controls.Add(pnlStageTracker);
            pnlDetail.Controls.Add(lblAuditTag);
            pnlDetail.Controls.Add(lstAuditHistory);
            pnlDetail.Controls.Add(grpUpdate);

            split.Controls.Add(pnlDetail, 1, 1);
            page.Controls.Add(split);
            return page;
        }

        private void LoadDataIntoOverview()
        {
            ApplyOverviewFilter();
            if (lvRequests.Items.Count > 0)
            {
                lvRequests.Items[0].Selected = true;
            }
        }

        private void ApplyOverviewFilter()
        {
            lvRequests.BeginUpdate();
            lvRequests.Items.Clear();

            string keyword = txtSearchOverview?.Text?.Trim()?.ToLower() ?? "";
            string statusFilter = cmbStatusFilter?.SelectedItem?.ToString() ?? "All";
            string categoryFilter = cmbCategoryFilter?.SelectedItem?.ToString() ?? "All";
            string priorityFilter = cmbPriorityFilter?.SelectedItem?.ToString() ?? "All";

            // If user typed a numeric ID, search using BST / AVL Tree in O(log n)!
            if (int.TryParse(keyword, out int searchId) && _repo.Bst.Search(searchId, out var foundReq, out _, out _))
            {
                AddRequestToListView(foundReq);
                lvRequests.EndUpdate();
                UpdateOverviewKpiSummary(new[] { foundReq });
                return;
            }

            var query = _repo.AllRequests.AsEnumerable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r => r.TrackingCode.ToLower().Contains(keyword) ||
                                         r.Title.ToLower().Contains(keyword) ||
                                         r.Location.ToLower().Contains(keyword) ||
                                         r.Description.ToLower().Contains(keyword));
            }

            if (statusFilter != "All")
            {
                query = query.Where(r => r.Status.ToString() == statusFilter);
            }

            if (categoryFilter != "All")
            {
                query = query.Where(r => r.Category == categoryFilter);
            }

            if (priorityFilter != "All")
            {
                query = query.Where(r => r.Priority.ToString() == priorityFilter);
            }

            var filteredList = query.OrderBy(r => (int)r.Priority).ThenBy(r => r.Id).ToList();
            foreach (var req in filteredList)
            {
                AddRequestToListView(req);
            }

            lvRequests.EndUpdate();
            UpdateOverviewKpiSummary(filteredList);
        }

        private void UpdateOverviewKpiSummary(IEnumerable<ServiceRequest> visibleRequests)
        {
            if (lblOverviewSummary == null) return;
            var list = visibleRequests.ToList();
            int totalAll = _repo.AllRequests.Count;
            int critical = list.Count(r => r.Priority == RequestPriority.Critical);
            int resolved = list.Count(r => r.Status == RequestStatus.Resolved);
            int active = list.Count - resolved;
            lblOverviewSummary.Text = $"Showing {list.Count} of {totalAll} Requests   |   Critical: {critical}   |   Active: {active}   |   Resolved: {resolved}";
        }

        private void ExportOverviewCsv()
        {
            if (lvRequests.Items.Count == 0)
            {
                MessageBox.Show("There are no visible requests to export.", "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog
            {
                Title = "Export Service Requests to CSV",
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"ServiceRequests_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                var sb = new StringBuilder();
                sb.AppendLine("ID,TrackingCode,Priority,Category,Location,Status,DateReported,Title,AssignedDepot,AssignedCrew");
                foreach (ListViewItem item in lvRequests.Items)
                {
                    if (item.Tag is ServiceRequest r)
                    {
                        sb.AppendLine(string.Join(",",
                            r.Id,
                            EscapeCsv(r.TrackingCode),
                            EscapeCsv(r.Priority.ToString()),
                            EscapeCsv(r.Category),
                            EscapeCsv(r.Location),
                            EscapeCsv(r.Status.ToString()),
                            r.DateReported.ToString("yyyy-MM-dd HH:mm"),
                            EscapeCsv(r.Title),
                            EscapeCsv(r.AssignedDepot),
                            EscapeCsv(r.AssignedCrew)));
                    }
                }

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"Exported {lvRequests.Items.Count} service requests to:\n{sfd.FileName}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "\"\"";
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        private void AddRequestToListView(ServiceRequest req)
        {
            var item = new ListViewItem(req.Id.ToString()) { Tag = req };
            item.SubItems.Add(req.Priority.ToString());
            item.SubItems.Add(req.Category);
            item.SubItems.Add(req.Location);
            item.SubItems.Add(req.Status.ToString());
            item.SubItems.Add(req.DateReported.ToString("yyyy-MM-dd HH:mm"));

            switch (req.Priority)
            {
                case RequestPriority.Critical:
                    item.ForeColor = CriticalRed;
                    item.Font = new Font(lvRequests.Font, FontStyle.Bold);
                    break;
                case RequestPriority.High:
                    item.ForeColor = HighOrange;
                    break;
                case RequestPriority.Medium:
                    item.ForeColor = PrimaryBlue;
                    break;
                default:
                    item.ForeColor = LowGray;
                    break;
            }

            lvRequests.Items.Add(item);
        }

        private void ShowSelectedRequestDetails()
        {
            if (lvRequests.SelectedItems.Count == 0) return;

            var req = lvRequests.SelectedItems[0].Tag as ServiceRequest;
            if (req == null) return;

            lblSelectedTracking.Text = req.TrackingCode;
            lblSelectedTitle.Text = req.Title;
            lblSelectedStatus.Text = $"Status: {req.Status}";
            lblSelectedPriority.Text = $"Priority: {req.Priority}";
            lblSelectedPriority.ForeColor = req.Priority == RequestPriority.Critical ? CriticalRed : PrimaryBlue;

            txtSelectedDescription.Text = req.Description;
            lblSelectedDepot.Text = $"Assigned Depot: {req.AssignedDepot}";
            lblSelectedCrew.Text = $"Crew: {req.AssignedCrew} (Est. {req.EstimatedHours}h)";

            if (req.DependsOnIds != null && req.DependsOnIds.Count > 0)
            {
                string prereqDetails = string.Join(", ", req.DependsOnIds.Select(id =>
                {
                    var p = _repo.RequestMap.ContainsKey(id) ? _repo.RequestMap[id] : null;
                    return p != null ? $"{p.TrackingCode} ({p.Status})" : $"SR-{id}";
                }));
                lblSelectedPrereqs.Text = $"Prerequisites: {prereqDetails}";
                lblSelectedPrereqs.ForeColor = CriticalRed;
            }
            else
            {
                lblSelectedPrereqs.Text = "Dependencies: None (Can be dispatched immediately)";
                lblSelectedPrereqs.ForeColor = MstGreen;
            }

            // Milestone Tracker
            RenderStageTracker(pnlStageTracker, req.Status);

            // Audit history
            lstAuditHistory.Items.Clear();
            foreach (var audit in req.StatusHistory)
            {
                lstAuditHistory.Items.Add(audit.ToString());
            }
        }

        private void RenderStageTracker(FlowLayoutPanel pnl, RequestStatus currentStatus)
        {
            pnl.Controls.Clear();
            string[] stages = { "Submitted", "Under Review", "Dispatched", "In Progress", "Inspection", "Resolved" };
            RequestStatus[] statusEnums = {
                RequestStatus.Submitted,
                RequestStatus.UnderReview,
                RequestStatus.Dispatched,
                RequestStatus.InProgress,
                RequestStatus.QualityInspection,
                RequestStatus.Resolved
            };

            int currentIndex = Array.IndexOf(statusEnums, currentStatus);

            for (int i = 0; i < stages.Length; i++)
            {
                var lblStage = new Label
                {
                    Text = stages[i],
                    Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                    Size = new Size(58, 40),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new Padding(2)
                };

                if (i < currentIndex)
                {
                    lblStage.BackColor = Color.FromArgb(200, 240, 215);
                    lblStage.ForeColor = MstGreen;
                    lblStage.Text += "\n✓";
                }
                else if (i == currentIndex)
                {
                    lblStage.BackColor = AccentBlue;
                    lblStage.ForeColor = Color.White;
                    lblStage.Text += "\n●";
                }
                else
                {
                    lblStage.BackColor = Color.FromArgb(235, 238, 242);
                    lblStage.ForeColor = Color.Gray;
                }

                pnl.Controls.Add(lblStage);
            }
        }

        private void ApplySelectedStatusUpdate()
        {
            if (lvRequests.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a service request from the list to update.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var req = lvRequests.SelectedItems[0].Tag as ServiceRequest;
            if (req == null) return;

            string statusStr = cmbUpdateStatus.SelectedItem.ToString();
            var newStatus = (RequestStatus)Enum.Parse(typeof(RequestStatus), statusStr);
            string note = string.IsNullOrWhiteSpace(txtStatusNote.Text) ? "Status updated by dispatcher." : txtStatusNote.Text.Trim();

            _repo.UpdateRequestStatus(req.Id, newStatus, note);

            MessageBox.Show($"Request {req.TrackingCode} updated to '{newStatus}' successfully.", "Status Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);

            ApplyOverviewFilter();
            ShowSelectedRequestDetails();
            RefreshHeapView();
            PopulateAdministrativeTreeView();
        }

        // ============================================================
        // TAB 2: TRACK BY UNIQUE IDENTIFIER (CITIZEN PORTAL)
        // ============================================================
        private TabPage BuildTrackByIdTab()
        {
            var page = new TabPage("Track by Unique ID") { BackColor = Color.White, Padding = new Padding(16) };

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            // Search Header Box
            var searchBox = new GroupBox { Text = "Citizen Service Request Tracker", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            var searchFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8), WrapContents = false };

            searchFlow.Controls.Add(new Label { Text = "Enter Unique Identifier (e.g., 1001 or SR-1001):", Font = new Font("Segoe UI", 9.5F), AutoSize = true, Margin = new Padding(0, 6, 8, 0) });
            txtTrackId = new TextBox { Width = 150, Font = new Font("Segoe UI", 10.5F), Text = "1001" };
            txtTrackId.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ExecuteTrackById(); } };
            searchFlow.Controls.Add(txtTrackId);

            var btnTrack = new Button { Text = "Track Progress", BackColor = PrimaryBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Height = 28, Width = 120 };
            btnTrack.Click += (s, e) => ExecuteTrackById();
            searchFlow.Controls.Add(btnTrack);

            var btnRandom = new Button { Text = "Sample Urgent ID", Height = 28, Width = 130 };
            btnRandom.Click += (s, e) =>
            {
                txtTrackId.Text = "1002";
                ExecuteTrackById();
            };
            searchFlow.Controls.Add(btnRandom);

            searchBox.Controls.Add(searchFlow);
            root.Controls.Add(searchBox, 0, 0);

            // Milestone Banner
            pnlTrackStages = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, BackColor = Color.FromArgb(245, 248, 253), Padding = new Padding(12, 8, 8, 8) };
            root.Controls.Add(pnlTrackStages, 0, 1);

            // Result Details Container
            var pnlResult = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(252, 253, 255), BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(12) };
            lblTrackStatusBadge = new Label { Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = PrimaryBlue, Location = new Point(14, 12), AutoSize = true, Text = "Ready to Track" };
            lblTrackTitle = new Label { Font = new Font("Segoe UI", 11.5F, FontStyle.Bold), ForeColor = DarkHeader, Location = new Point(14, 42), Size = new Size(800, 28) };
            lblTrackDetails = new Label { Font = new Font("Segoe UI", 9.5F), Location = new Point(14, 75), Size = new Size(800, 110) };

            var lblTrackAuditTag = new Label { Text = "Public Tracking Log & Municipal Milestones:", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Location = new Point(14, 195), AutoSize = true };
            lstTrackAudit = new ListBox { Location = new Point(14, 220), Size = new Size(950, 160), Font = new Font("Consolas", 9F) };

            pnlResult.Controls.Add(lblTrackStatusBadge);
            pnlResult.Controls.Add(lblTrackTitle);
            pnlResult.Controls.Add(lblTrackDetails);
            pnlResult.Controls.Add(lblTrackAuditTag);
            pnlResult.Controls.Add(lstTrackAudit);

            root.Controls.Add(pnlResult, 0, 2);

            page.Controls.Add(root);
            return page;
        }

        private void ExecuteTrackById()
        {
            string raw = txtTrackId.Text.Trim();
            if (raw.StartsWith("SR-", StringComparison.OrdinalIgnoreCase))
            {
                raw = raw.Substring(3);
            }

            if (!int.TryParse(raw, out int id))
            {
                MessageBox.Show("Please enter a valid numeric Service Request ID (e.g. 1001).", "Invalid ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // O(log n) search using AVL Tree
            bool found = _repo.Avl.Search(id, out var req, out var path, out int comparisons);

            if (!found)
            {
                lblTrackStatusBadge.Text = "Request Not Found";
                lblTrackStatusBadge.ForeColor = CriticalRed;
                lblTrackTitle.Text = $"No service request matching reference number #{id} exists in the municipal register.";
                lblTrackDetails.Text = "Please double check your tracking code or submit a new report via the Report Issues form.";
                lstTrackAudit.Items.Clear();
                pnlTrackStages.Controls.Clear();
                return;
            }

            lblTrackStatusBadge.Text = $"{req.TrackingCode} - Current Status: {req.Status.ToString().ToUpper()}";
            lblTrackStatusBadge.ForeColor = req.Status == RequestStatus.Resolved ? MstGreen : PrimaryBlue;
            lblTrackTitle.Text = req.Title;

            lblTrackDetails.Text = $"Category: {req.Category}    |    Ward / Location: {req.Ward} ({req.Location})\n" +
                                   $"Priority Level: {req.Priority}    |    Reported On: {req.DateReported:yyyy-MM-dd HH:mm}\n" +
                                   $"Assigned Depot: {req.AssignedDepot}    |    Field Crew: {req.AssignedCrew}\n" +
                                   $"Estimated Resolution: {req.EstimatedHours} hours\n" +
                                   $"Description: {req.Description}\n" +
                                   $"(Retrieved in {comparisons} tree comparisons via AVL Self-Balancing Tree index)";

            RenderStageTracker(pnlTrackStages, req.Status);

            lstTrackAudit.Items.Clear();
            foreach (var entry in req.StatusHistory)
            {
                lstTrackAudit.Items.Add(entry.ToString());
            }
        }

        // ============================================================
        // TAB 3: TREE DATA STRUCTURES EXPLORER (BST, AVL & RED-BLACK)
        // ============================================================
        private TabPage BuildTreeExplorerTab()
        {
            var page = new TabPage("Tree Structures Explorer") { BackColor = Color.White, Padding = new Padding(8) };

            var rootTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 };
            rootTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            rootTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            rootTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));
            rootTable.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            rootTable.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

            // Top Control Bar
            var pnlTop = new GroupBox { Text = "Data Structure Selector & Search Benchmark", Dock = DockStyle.Fill };
            var flowTop = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(4), WrapContents = true };

            rbBst = new RadioButton { Text = "Binary Search Tree (BST)", Checked = false, AutoSize = true, Margin = new Padding(4, 6, 8, 0) };
            rbAvl = new RadioButton { Text = "AVL Tree (Balanced)", Checked = true, AutoSize = true, Margin = new Padding(4, 6, 8, 0) };
            rbRedBlack = new RadioButton { Text = "Red-Black Tree (Balanced)", Checked = false, AutoSize = true, Margin = new Padding(4, 6, 16, 0) };

            rbBst.CheckedChanged += (s, e) => RefreshTreeExplorer();
            rbAvl.CheckedChanged += (s, e) => RefreshTreeExplorer();
            rbRedBlack.CheckedChanged += (s, e) => RefreshTreeExplorer();

            flowTop.Controls.Add(rbBst);
            flowTop.Controls.Add(rbAvl);
            flowTop.Controls.Add(rbRedBlack);

            flowTop.Controls.Add(new Label { Text = "Request ID:", AutoSize = true, Margin = new Padding(8, 6, 2, 0) });
            txtTreeSearchId = new TextBox { Width = 70, Text = "1005" };
            flowTop.Controls.Add(txtTreeSearchId);

            var btnSearch = new Button { Text = "Benchmark Search", BackColor = PrimaryBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Height = 26, Width = 130 };
            btnSearch.Click += (s, e) => BenchmarkTreeSearch();
            flowTop.Controls.Add(btnSearch);

            var btnVerifyRb = new Button { Text = "Verify RB Properties", Height = 26, Width = 140 };
            btnVerifyRb.Click += (s, e) =>
            {
                if (_repo.RedBlack.VerifyProperties(out string report))
                {
                    MessageBox.Show(report, "Red-Black Properties Validated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(report, "Red-Black Violation Detected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            flowTop.Controls.Add(btnVerifyRb);

            pnlTop.Controls.Add(flowTop);
            rootTable.Controls.Add(pnlTop, 0, 0);
            rootTable.SetColumnSpan(pnlTop, 2);

            // Left Middle: Visual ASCII Tree Box
            var grpAscii = new GroupBox { Text = "Tree Structure Hierarchy (Visual ASCII Representation)", Dock = DockStyle.Fill };
            rtbTreeAscii = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9F),
                ReadOnly = true,
                BackColor = Color.FromArgb(248, 250, 253),
                WordWrap = false
            };
            grpAscii.Controls.Add(rtbTreeAscii);
            rootTable.Controls.Add(grpAscii, 0, 1);

            // Right Middle: Administrative N-ary TreeView
            var grpNary = new GroupBox { Text = "Administrative Directorate Hierarchy (N-ary General Tree)", Dock = DockStyle.Fill };
            tvAdministrative = new TreeView { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F) };
            tvAdministrative.AfterSelect += (s, e) =>
            {
                if (e.Node.Tag is ServiceRequest req)
                {
                    MessageBox.Show($"[{req.TrackingCode}] {req.Title}\nCategory: {req.Category}\nStatus: {req.Status}\nPriority: {req.Priority}", "Administrative Tree Record", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            grpNary.Controls.Add(tvAdministrative);
            rootTable.Controls.Add(grpNary, 1, 1);

            // Left Bottom: Diagnostic Metrics & Search Path
            var grpMetrics = new GroupBox { Text = "Tree Balancing & Retrieval Metrics", Dock = DockStyle.Fill };
            var pnlMetrics = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(6) };
            lblTreeMetrics = new Label { Font = new Font("Consolas", 9F), Dock = DockStyle.Top, Height = 95, Text = "Tree metrics loading..." };
            lblTreeSearchPath = new Label { Font = new Font("Consolas", 8.5F), Dock = DockStyle.Fill, ForeColor = PrimaryBlue, Text = "Search path: (Enter an ID above and click Benchmark Search)" };
            pnlMetrics.Controls.Add(lblTreeSearchPath);
            pnlMetrics.Controls.Add(lblTreeMetrics);
            grpMetrics.Controls.Add(pnlMetrics);
            rootTable.Controls.Add(grpMetrics, 0, 2);

            // Right Bottom: Traversals Panel
            var grpTraversals = new GroupBox { Text = "Tree Traversals (In-Order, Pre-Order, Post-Order, Level-Order)", Dock = DockStyle.Fill };
            var pnlTrav = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };

            var flowTravButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 35 };
            var btnInOrder = new Button { Text = "In-Order", Width = 80, Height = 26 };
            btnInOrder.Click += (s, e) => RunTraversal("In-Order");
            var btnPreOrder = new Button { Text = "Pre-Order", Width = 80, Height = 26 };
            btnPreOrder.Click += (s, e) => RunTraversal("Pre-Order");
            var btnPostOrder = new Button { Text = "Post-Order", Width = 85, Height = 26 };
            btnPostOrder.Click += (s, e) => RunTraversal("Post-Order");
            var btnLevelOrder = new Button { Text = "Level-Order (BFS)", Width = 115, Height = 26 };
            btnLevelOrder.Click += (s, e) => RunTraversal("Level-Order");

            flowTravButtons.Controls.Add(btnInOrder);
            flowTravButtons.Controls.Add(btnPreOrder);
            flowTravButtons.Controls.Add(btnPostOrder);
            flowTravButtons.Controls.Add(btnLevelOrder);

            lstTraversalOutput = new ListBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 8.5F) };

            pnlTrav.Controls.Add(lstTraversalOutput);
            pnlTrav.Controls.Add(flowTravButtons);
            grpTraversals.Controls.Add(pnlTrav);
            rootTable.Controls.Add(grpTraversals, 1, 2);

            page.Controls.Add(rootTable);
            return page;
        }

        private void RefreshTreeExplorer()
        {
            if (rbAvl.Checked)
            {
                rtbTreeAscii.Text = _repo.Avl.GenerateAsciiTree();
                lblTreeMetrics.Text = $"Data Structure: AVL Self-Balancing Binary Search Tree\n" +
                                      $"Total Nodes: {_repo.Avl.Count}   |   Tree Height: {_repo.Avl.GetTreeHeight()} (Optimal: ~{(int)Math.Ceiling(Math.Log(_repo.Avl.Count + 1, 2))})\n" +
                                      $"Root Key: {_repo.Avl.Root?.Key} (Balance Factor: {_repo.Avl.GetBalanceFactor(_repo.Avl.Root):+0;-0;0})\n" +
                                      $"Rebalancing Rotations: Total={_repo.Avl.TotalRotations} (LL={_repo.Avl.LlRotations}, RR={_repo.Avl.RrRotations}, LR={_repo.Avl.LrRotations}, RL={_repo.Avl.RlRotations})\n" +
                                      $"Guaranteed Search Time: O(log n)";
            }
            else if (rbRedBlack.Checked)
            {
                rtbTreeAscii.Text = _repo.RedBlack.GenerateAsciiTree();
                lblTreeMetrics.Text = $"Data Structure: Red-Black Self-Balancing Tree\n" +
                                      $"Total Nodes: {_repo.RedBlack.Count}   |   Tree Height: {_repo.RedBlack.GetTreeHeight()}   |   Black Height: {_repo.RedBlack.GetBlackHeight()}\n" +
                                      $"Root Key: {_repo.RedBlack.Root?.Key} (Color: {_repo.RedBlack.Root?.Color})\n" +
                                      $"Rotations: {_repo.RedBlack.TotalRotations}   |   Color Flips: {_repo.RedBlack.ColorFlips}\n" +
                                      $"Guaranteed Search Time: O(log n) with fewer rotation overheads on insertion.";
            }
            else
            {
                rtbTreeAscii.Text = _repo.Bst.GenerateAsciiTree();
                lblTreeMetrics.Text = $"Data Structure: Standard Binary Search Tree (BST)\n" +
                                      $"Total Nodes: {_repo.Bst.Count}   |   Tree Height: {_repo.Bst.GetHeight()}\n" +
                                      $"Root Key: {_repo.Bst.Root?.Key}\n" +
                                      $"Search Complexity: Average O(log n), Worst-case O(n) if unbalanced.";
            }
        }

        private void BenchmarkTreeSearch()
        {
            if (!int.TryParse(txtTreeSearchId.Text.Trim(), out int id))
            {
                MessageBox.Show("Please enter a valid numeric Request ID to benchmark.", "Invalid ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var sw = Stopwatch.StartNew();
            bool found;
            List<int> path;
            int comparisons;
            string treeName;

            if (rbAvl.Checked)
            {
                treeName = "AVL Tree";
                found = _repo.Avl.Search(id, out _, out path, out comparisons);
            }
            else if (rbRedBlack.Checked)
            {
                treeName = "Red-Black Tree";
                found = _repo.RedBlack.Search(id, out _, out path, out comparisons);
            }
            else
            {
                treeName = "Binary Search Tree";
                found = _repo.Bst.Search(id, out _, out path, out comparisons);
            }
            sw.Stop();

            string status = found ? "FOUND" : "NOT FOUND";
            string pathStr = string.Join(" -> ", path.Select(k => $"[ID: {k}]"));

            lblTreeSearchPath.Text = $"[{treeName} Benchmark Result: {status}]\n" +
                                     $"Search Path Visited: {pathStr}\n" +
                                     $"Comparisons Required: {comparisons} node lookups\n" +
                                     $"Search Time Elapsed: {sw.Elapsed.TotalMilliseconds:F4} ms ({sw.ElapsedTicks} CPU ticks)\n" +
                                     $"Theoretical Complexity: O(log2 { _repo.AllRequests.Count }) ≈ 4 comparisons.";
        }

        private void RunTraversal(string type)
        {
            lstTraversalOutput.Items.Clear();
            List<ServiceRequest> results;

            if (rbAvl.Checked)
            {
                switch (type)
                {
                    case "Pre-Order": results = _repo.Avl.PreOrderTraversal(); break;
                    case "Post-Order": results = _repo.Avl.PostOrderTraversal(); break;
                    case "Level-Order": results = _repo.Avl.LevelOrderTraversal(); break;
                    default: results = _repo.Avl.InOrderTraversal(); break;
                }
            }
            else if (rbRedBlack.Checked)
            {
                switch (type)
                {
                    case "Pre-Order": results = _repo.RedBlack.PreOrderTraversal(); break;
                    case "Post-Order": results = _repo.RedBlack.PostOrderTraversal(); break;
                    case "Level-Order": results = _repo.RedBlack.LevelOrderTraversal(); break;
                    default: results = _repo.RedBlack.InOrderTraversal(); break;
                }
            }
            else
            {
                switch (type)
                {
                    case "Pre-Order": results = _repo.Bst.PreOrderTraversal(); break;
                    case "Post-Order": results = _repo.Bst.PostOrderTraversal(); break;
                    case "Level-Order": results = _repo.Bst.LevelOrderTraversal(); break;
                    default: results = _repo.Bst.InOrderTraversal(); break;
                }
            }

            lstTraversalOutput.Items.Add($"--- {type} Traversal Output ({results.Count} items) ---");
            foreach (var req in results)
            {
                lstTraversalOutput.Items.Add($"[{req.TrackingCode}] ({req.Priority}) {req.Category} - {req.Title}");
            }
        }

        private void PopulateAdministrativeTreeView()
        {
            tvAdministrative.BeginUpdate();
            tvAdministrative.Nodes.Clear();
            if (_repo.AdministrativeTree != null)
            {
                var winNode = _repo.AdministrativeTree.ToWinFormsTreeNode();
                tvAdministrative.Nodes.Add(winNode);
                winNode.Expand();
                foreach (TreeNode child in winNode.Nodes)
                {
                    child.Expand();
                }
            }
            tvAdministrative.EndUpdate();
        }

        // ============================================================
        // TAB 4: PRIORITY DISPATCH QUEUE (BINARY MIN-HEAP)
        // ============================================================
        private TabPage BuildPriorityHeapTab()
        {
            var page = new TabPage("Priority Queue & Dispatch (Heap)") { BackColor = Color.White, Padding = new Padding(12) };

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 95));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            // Banner Panel
            var pnlBanner = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 246, 255), BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };
            lblNextUrgent = new Label { Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = CriticalRed, Location = new Point(12, 10), AutoSize = true, Text = "Next Incident to Dispatch: Loading..." };
            lblHeapCount = new Label { Font = new Font("Segoe UI", 9.5F), Location = new Point(14, 38), AutoSize = true, Text = "Heap items: 0" };
            var lblHeapDesc = new Label
            {
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(80, 90, 110),
                Location = new Point(14, 62),
                AutoSize = true,
                Text = "Implemented via custom Binary Min-Heap in List<T>. Enqueue/Dequeue in O(log n); Peek in O(1)."
            };
            pnlBanner.Controls.Add(lblNextUrgent);
            pnlBanner.Controls.Add(lblHeapCount);
            pnlBanner.Controls.Add(lblHeapDesc);
            root.Controls.Add(pnlBanner, 0, 0);

            // Heap List View
            lvHeap = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lvHeap.Columns.Add("Priority Level", 110);
            lvHeap.Columns.Add("Reference ID", 100);
            lvHeap.Columns.Add("Category", 140);
            lvHeap.Columns.Add("Ward / Location", 180);
            lvHeap.Columns.Add("Incident Title", 250);
            lvHeap.Columns.Add("Date Reported", 140);
            root.Controls.Add(lvHeap, 0, 1);

            // Action Bar
            var pnlAction = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(4) };
            var btnDispatchNext = new Button
            {
                Text = "Dispatch Highest Priority Incident (Dequeue)",
                BackColor = CriticalRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Height = 38,
                Width = 320
            };
            btnDispatchNext.Click += (s, e) =>
            {
                if (_repo.PriorityHeap.Count == 0)
                {
                    MessageBox.Show("All emergency incidents have already been dispatched or resolved!", "Queue Empty", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var dispatched = _repo.DispatchNextUrgentRequest();
                MessageBox.Show($"DISPATCHED: [{dispatched.TrackingCode}] {dispatched.Title}\nAssigned To: {dispatched.AssignedCrew}\nStatus set to In Progress.", "Incident Dispatched", MessageBoxButtons.OK, MessageBoxIcon.Information);

                RefreshHeapView();
                ApplyOverviewFilter();
                PopulateAdministrativeTreeView();
            };

            var btnSimulateEmergency = new Button
            {
                Text = "+ Log Rapid Emergency Incident (Priority 1)",
                BackColor = AccentBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 38,
                Width = 280
            };
            btnSimulateEmergency.Click += (s, e) =>
            {
                var simulated = new Issue
                {
                    Id = _repo.AllRequests.Count + 1001,
                    Location = "N1 Highway Buccleuch Interchange",
                    Category = "Public Safety",
                    Description = "Major multi-vehicle fuel spillage obstructing 3 lanes of highway traffic.",
                    DateReported = DateTime.Now
                };
                _repo.AddFromReportedIssue(simulated);
                MessageBox.Show($"New Priority 1 Emergency logged (SR-{simulated.Id}). Enqueued into binary min-heap at root!", "Emergency Logged", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                RefreshHeapView();
                ApplyOverviewFilter();
                PopulateAdministrativeTreeView();
            };

            pnlAction.Controls.Add(btnDispatchNext);
            pnlAction.Controls.Add(btnSimulateEmergency);
            root.Controls.Add(pnlAction, 0, 2);

            page.Controls.Add(root);
            return page;
        }

        private void RefreshHeapView()
        {
            lvHeap.BeginUpdate();
            lvHeap.Items.Clear();

            var queuedList = _repo.PriorityHeap.ToList();
            lblHeapCount.Text = $"Total Active Requests Awaiting Dispatch: {queuedList.Count} incidents in priority heap";

            if (queuedList.Count > 0)
            {
                var top = _repo.PriorityHeap.Peek();
                lblNextUrgent.Text = $"Next Incident to Dispatch: [{top.TrackingCode}] ({top.Priority}) {top.Title}";
            }
            else
            {
                lblNextUrgent.Text = "No incidents pending dispatch. Heap is clear!";
            }

            foreach (var req in queuedList)
            {
                var item = new ListViewItem(req.Priority.ToString());
                item.SubItems.Add(req.TrackingCode);
                item.SubItems.Add(req.Category);
                item.SubItems.Add(req.Location);
                item.SubItems.Add(req.Title);
                item.SubItems.Add(req.DateReported.ToString("yyyy-MM-dd HH:mm"));

                if (req.Priority == RequestPriority.Critical)
                {
                    item.ForeColor = CriticalRed;
                    item.Font = new Font(lvHeap.Font, FontStyle.Bold);
                }
                else if (req.Priority == RequestPriority.High)
                {
                    item.ForeColor = HighOrange;
                }
                else
                {
                    item.ForeColor = PrimaryBlue;
                }

                lvHeap.Items.Add(item);
            }

            lvHeap.EndUpdate();
        }

        // ============================================================
        // TAB 5: MUNICIPAL NETWORK GRAPH, TRAVERSALS & MST
        // ============================================================
        private TabPage BuildGraphMstTab()
        {
            var page = new TabPage("Network Graph, Traversals & MST") { BackColor = Color.White, Padding = new Padding(8) };

            var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            split.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Top Operations Toolbar
            var pnlToolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, Padding = new Padding(2) };

            var btnKruskal = new Button { Text = "Run Kruskal's MST", BackColor = MstGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Height = 28, Width = 145 };
            btnKruskal.Click += (s, e) => ExecuteKruskalMst();
            pnlToolbar.Controls.Add(btnKruskal);

            var btnPrim = new Button { Text = "Run Prim's MST", BackColor = AccentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Height = 28, Width = 130 };
            btnPrim.Click += (s, e) => ExecutePrimMst();
            pnlToolbar.Controls.Add(btnPrim);

            pnlToolbar.Controls.Add(new Label { Text = "Start Depot:", AutoSize = true, Margin = new Padding(8, 6, 2, 0) });
            cmbStartDepot = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
            pnlToolbar.Controls.Add(cmbStartDepot);

            var btnBfs = new Button { Text = "Run BFS Traversal", Height = 28, Width = 130 };
            btnBfs.Click += (s, e) => ExecuteBfs();
            pnlToolbar.Controls.Add(btnBfs);

            var btnDfs = new Button { Text = "Run DFS Traversal", Height = 28, Width = 130 };
            btnDfs.Click += (s, e) => ExecuteDfs();
            pnlToolbar.Controls.Add(btnDfs);

            var btnDag = new Button { Text = "Analyze DAG Dependencies", Height = 28, Width = 185 };
            btnDag.Click += (s, e) => ExecuteDagAnalysis();
            pnlToolbar.Controls.Add(btnDag);

            split.Controls.Add(pnlToolbar, 0, 0);
            split.SetColumnSpan(pnlToolbar, 2);

            // Left: Custom GDI+ Graph Canvas
            pnlGraphCanvas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 254),
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };
            pnlGraphCanvas.Paint += RenderGraphOnCanvas;
            pnlGraphCanvas.MouseClick += PnlGraphCanvas_MouseClick;
            split.Controls.Add(pnlGraphCanvas, 0, 1);

            // Right: Graph Statistics & Algorithm Execution Log
            var pnlRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };
            var grpStats = new GroupBox { Text = "MST Road Optimization Metrics", Dock = DockStyle.Top, Height = 135 };
            lblMstStats = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 8.5F),
                Padding = new Padding(6),
                Text = "Click 'Run Kruskal's MST' or 'Run Prim's MST' to optimize municipal emergency response corridors."
            };
            grpStats.Controls.Add(lblMstStats);

            var grpLog = new GroupBox { Text = "Algorithm Step-by-Step Execution Log", Dock = DockStyle.Fill };
            rtbGraphLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 8.5F),
                ReadOnly = true,
                BackColor = Color.White
            };
            grpLog.Controls.Add(rtbGraphLog);

            pnlRight.Controls.Add(grpLog);
            pnlRight.Controls.Add(grpStats);
            split.Controls.Add(pnlRight, 1, 1);

            page.Controls.Add(split);
            return page;
        }

        private void PopulateDepotDropdown()
        {
            cmbStartDepot.Items.Clear();
            foreach (var node in _repo.NetworkGraph.Nodes.Values.Where(n => n.NodeType == GraphNodeType.Depot))
            {
                cmbStartDepot.Items.Add(node.Id);
            }
            if (cmbStartDepot.Items.Count > 0) cmbStartDepot.SelectedIndex = 0;
        }

        private void ExecuteKruskalMst()
        {
            _lastTraversalResult = null;
            _lastMstResult = _repo.NetworkGraph.ComputeKruskalMst();

            lblMstStats.Text = $"Algorithm: {_lastMstResult.AlgorithmName}\n" +
                               $"Original Road Corridors: {_lastMstResult.OriginalTotalDistanceKm:F1} km ({_repo.NetworkGraph.Edges.Count} routes)\n" +
                               $"Optimized MST Backbone: {_lastMstResult.TotalMstDistanceKm:F1} km ({_lastMstResult.Edges.Count} routes)\n" +
                               $"Distance Saved: {_lastMstResult.KilometersSaved:F1} km ({_lastMstResult.SavingsPercentage:F1}% optimization)\n" +
                               $"Estimated Maintenance Cost: R {_lastMstResult.EstimatedCostZar:N0}\n" +
                               $"Runtime: {_lastMstResult.ExecutionTimeTicks} CPU ticks";

            rtbGraphLog.Clear();
            foreach (var line in _lastMstResult.ExecutionLog)
            {
                rtbGraphLog.AppendText(line + Environment.NewLine);
            }

            pnlGraphCanvas.Invalidate();
        }

        private void ExecutePrimMst()
        {
            string startDepot = cmbStartDepot.SelectedItem?.ToString() ?? "DEPOT-CENTRAL";
            _lastTraversalResult = null;
            _lastMstResult = _repo.NetworkGraph.ComputePrimMst(startDepot);

            lblMstStats.Text = $"Algorithm: {_lastMstResult.AlgorithmName}\n" +
                               $"Starting Depot: {startDepot}\n" +
                               $"Original Road Corridors: {_lastMstResult.OriginalTotalDistanceKm:F1} km\n" +
                               $"Optimized MST Backbone: {_lastMstResult.TotalMstDistanceKm:F1} km\n" +
                               $"Distance Saved: {_lastMstResult.KilometersSaved:F1} km ({_lastMstResult.SavingsPercentage:F1}% optimization)\n" +
                               $"Estimated Maintenance Cost: R {_lastMstResult.EstimatedCostZar:N0}";

            rtbGraphLog.Clear();
            foreach (var line in _lastMstResult.ExecutionLog)
            {
                rtbGraphLog.AppendText(line + Environment.NewLine);
            }

            pnlGraphCanvas.Invalidate();
        }

        private void ExecuteBfs()
        {
            string startDepot = cmbStartDepot.SelectedItem?.ToString() ?? "DEPOT-CENTRAL";
            _lastMstResult = null;
            _lastTraversalResult = _repo.NetworkGraph.BreadthFirstSearch(startDepot);

            lblMstStats.Text = $"Traversal: Breadth-First Search (BFS)\n" +
                               $"Origin: {startDepot} ({_repo.NetworkGraph.Nodes[startDepot].Label})\n" +
                               $"Total Locations Reached: {_lastTraversalResult.VisitedNodes.Count} / {_repo.NetworkGraph.Nodes.Count}\n" +
                               $"Discovers minimum hop radii from municipal operations center.";

            rtbGraphLog.Clear();
            foreach (var line in _lastTraversalResult.LogSteps)
            {
                rtbGraphLog.AppendText(line + Environment.NewLine);
            }

            pnlGraphCanvas.Invalidate();
        }

        private void ExecuteDfs()
        {
            string startDepot = cmbStartDepot.SelectedItem?.ToString() ?? "DEPOT-CENTRAL";
            _lastMstResult = null;
            _lastTraversalResult = _repo.NetworkGraph.DepthFirstSearch(startDepot);

            lblMstStats.Text = $"Traversal: Depth-First Search (DFS)\n" +
                               $"Origin: {startDepot} ({_repo.NetworkGraph.Nodes[startDepot].Label})\n" +
                               $"Total Locations Reached: {_lastTraversalResult.VisitedNodes.Count} / {_repo.NetworkGraph.Nodes.Count}\n" +
                               $"Explores long-distance connected routes and network partitions.";

            rtbGraphLog.Clear();
            foreach (var line in _lastTraversalResult.LogSteps)
            {
                rtbGraphLog.AppendText(line + Environment.NewLine);
            }

            pnlGraphCanvas.Invalidate();
        }

        private void ExecuteDagAnalysis()
        {
            bool ok = _repo.Dependencies.GetTopologicalOrder(out var order, out string msg);

            rtbGraphLog.Clear();
            rtbGraphLog.AppendText("====================================================" + Environment.NewLine);
            rtbGraphLog.AppendText("MUNICIPAL WORK ORDER DEPENDENCY ANALYSIS (DAG)" + Environment.NewLine);
            rtbGraphLog.AppendText("====================================================" + Environment.NewLine);
            rtbGraphLog.AppendText(msg + Environment.NewLine + Environment.NewLine);

            if (ok)
            {
                rtbGraphLog.AppendText("Optimal Topological Sequence for Field Crew Dispatch:" + Environment.NewLine);
                int step = 1;
                foreach (var reqId in order)
                {
                    var r = _repo.RequestMap.ContainsKey(reqId) ? _repo.RequestMap[reqId] : null;
                    string title = r != null ? r.Title : $"Request #{reqId}";
                    string prereqs = r != null && r.DependsOnIds.Count > 0 ? $" (Requires: {string.Join(", ", r.DependsOnIds.Select(id => $"SR-{id}"))})" : " (No prerequisites)";
                    rtbGraphLog.AppendText($"  Step {step++}: [{r?.TrackingCode ?? reqId.ToString()}] {title}{prereqs}" + Environment.NewLine);
                }
            }
        }

        private PointF GetScaledGraphPoint(GraphNode n, float minX, float maxX, float minY, float maxY)
        {
            const float padX = 50f, padY = 40f;
            float drawWidth = Math.Max(1f, pnlGraphCanvas.Width - (padX * 2));
            float drawHeight = Math.Max(1f, pnlGraphCanvas.Height - (padY * 2));
            float spanX = Math.Max(1f, maxX - minX);
            float spanY = Math.Max(1f, maxY - minY);
            float x = padX + ((n.X - minX) / spanX) * drawWidth;
            float y = padY + ((n.Y - minY) / spanY) * drawHeight;
            return new PointF(x, y);
        }

        private void PnlGraphCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            var graph = _repo.NetworkGraph;
            if (graph == null || graph.Nodes.Count == 0) return;

            float minX = graph.Nodes.Values.Min(n => n.X);
            float maxX = graph.Nodes.Values.Max(n => n.X);
            float minY = graph.Nodes.Values.Min(n => n.Y);
            float maxY = graph.Nodes.Values.Max(n => n.Y);

            GraphNode hit = null;
            float bestDist = 20f;
            foreach (var node in graph.Nodes.Values)
            {
                var pt = GetScaledGraphPoint(node, minX, maxX, minY, maxY);
                float dx = e.X - pt.X;
                float dy = e.Y - pt.Y;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                if (dist <= bestDist)
                {
                    bestDist = dist;
                    hit = node;
                }
            }

            if (hit == null) return;

            _selectedGraphNodeId = hit.Id;
            var incidentEdges = graph.GetIncidentEdges(hit.Id);
            string edgesText = incidentEdges.Count == 0
                ? "None"
                : string.Join(", ", incidentEdges.Select(edge =>
                {
                    string neighborId = edge.FromId == hit.Id ? edge.ToId : edge.FromId;
                    return $"{neighborId} ({edge.DistanceKm:F1}km)";
                }));

            string requestInfo = "Depot Hub";
            if (hit.ServiceRequestId.HasValue && _repo.RequestMap.TryGetValue(hit.ServiceRequestId.Value, out var req))
            {
                requestInfo = $"#{req.Id} [{req.Priority}] {req.Title} ({req.Status})";
            }

            lblMstStats.Text = $"Selected Node: {hit.Label} ({hit.Id}) - {hit.NodeType}\n" +
                               $"Linked Item: {requestInfo}\n" +
                               $"Connected Corridors ({incidentEdges.Count}): {edgesText}";

            pnlGraphCanvas.Invalidate();
        }

        private void RenderGraphOnCanvas(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var graph = _repo.NetworkGraph;
            if (graph == null || graph.Nodes.Count == 0) return;

            // Coordinate Scaling
            float minX = graph.Nodes.Values.Min(n => n.X);
            float maxX = graph.Nodes.Values.Max(n => n.X);
            float minY = graph.Nodes.Values.Min(n => n.Y);
            float maxY = graph.Nodes.Values.Max(n => n.Y);

            // Map visited node IDs to their 1-based traversal order if BFS/DFS is active
            var visitOrder = new Dictionary<string, int>();
            if (_lastTraversalResult != null && _lastTraversalResult.VisitedNodes != null)
            {
                for (int i = 0; i < _lastTraversalResult.VisitedNodes.Count; i++)
                {
                    string id = _lastTraversalResult.VisitedNodes[i].Id;
                    if (!visitOrder.ContainsKey(id))
                    {
                        visitOrder[id] = i + 1;
                    }
                }
            }

            // 1. Draw Edges
            using (var penStandard = new Pen(Color.FromArgb(195, 205, 220), 1.5f))
            using (var penMst = new Pen(MstGreen, 3.5f))
            using (var penTraversal = new Pen(HighOrange, 2.5f))
            using (var fontEdge = new Font("Segoe UI", 7.5F))
            using (var brushEdge = new SolidBrush(Color.FromArgb(100, 110, 125)))
            using (var brushMstText = new SolidBrush(MstGreen))
            {
                foreach (var edge in graph.Edges)
                {
                    if (!graph.Nodes.ContainsKey(edge.FromId) || !graph.Nodes.ContainsKey(edge.ToId)) continue;

                    var p1 = GetScaledGraphPoint(graph.Nodes[edge.FromId], minX, maxX, minY, maxY);
                    var p2 = GetScaledGraphPoint(graph.Nodes[edge.ToId], minX, maxX, minY, maxY);

                    if (edge.IsMstEdge)
                    {
                        g.DrawLine(penMst, p1, p2);
                    }
                    else if (visitOrder.ContainsKey(edge.FromId) && visitOrder.ContainsKey(edge.ToId))
                    {
                        g.DrawLine(penTraversal, p1, p2);
                    }
                    else
                    {
                        penStandard.DashStyle = DashStyle.Dash;
                        g.DrawLine(penStandard, p1, p2);
                    }

                    // Midpoint label
                    float midX = (p1.X + p2.X) / 2f;
                    float midY = (p1.Y + p2.Y) / 2f;
                    string distLabel = $"{edge.DistanceKm:F1}km";
                    g.DrawString(distLabel, fontEdge, edge.IsMstEdge ? brushMstText : brushEdge, midX - 10, midY - 7);
                }
            }

            // 2. Draw Nodes
            using (var brushDepot = new SolidBrush(PrimaryBlue))
            using (var brushIncident = new SolidBrush(AccentBlue))
            using (var brushCritical = new SolidBrush(CriticalRed))
            using (var brushBadge = new SolidBrush(HighOrange))
            using (var penBorder = new Pen(Color.White, 2f))
            using (var penVisitedRing = new Pen(HighOrange, 2.5f))
            using (var penSelectedRing = new Pen(DarkHeader, 2.5f))
            using (var fontLabel = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var fontBadge = new Font("Segoe UI", 7F, FontStyle.Bold))
            using (var brushText = new SolidBrush(DarkHeader))
            {
                foreach (var node in graph.Nodes.Values)
                {
                    var pt = GetScaledGraphPoint(node, minX, maxX, minY, maxY);

                    if (_selectedGraphNodeId == node.Id)
                    {
                        g.DrawEllipse(penSelectedRing, pt.X - 18, pt.Y - 18, 36, 36);
                    }

                    if (visitOrder.ContainsKey(node.Id))
                    {
                        g.DrawEllipse(penVisitedRing, pt.X - 15, pt.Y - 15, 30, 30);
                    }

                    if (node.NodeType == GraphNodeType.Depot)
                    {
                        // Depots as squares
                        const float size = 26f;
                        var rect = new RectangleF(pt.X - size / 2, pt.Y - size / 2, size, size);
                        g.FillRectangle(brushDepot, rect);
                        g.DrawRectangle(penBorder, rect.X, rect.Y, rect.Width, rect.Height);

                        // Icon label
                        g.DrawString("DEPOT", fontLabel, Brushes.White, pt.X - 18, pt.Y - 6);
                        g.DrawString(node.Label, fontLabel, brushText, pt.X - 40, pt.Y + 16);
                    }
                    else
                    {
                        // Incidents as circles
                        const float size = 18f;
                        var rect = new RectangleF(pt.X - size / 2, pt.Y - size / 2, size, size);

                        var fillBrush = brushIncident;
                        if (node.ServiceRequestId.HasValue && _repo.RequestMap.ContainsKey(node.ServiceRequestId.Value))
                        {
                            var r = _repo.RequestMap[node.ServiceRequestId.Value];
                            if (r.Priority == RequestPriority.Critical) fillBrush = brushCritical;
                        }

                        g.FillEllipse(fillBrush, rect);
                        g.DrawEllipse(penBorder, rect);

                        g.DrawString(node.Label, fontLabel, brushText, pt.X - 35, pt.Y + 11);
                    }

                    if (visitOrder.TryGetValue(node.Id, out int orderNum))
                    {
                        var badgeRect = new RectangleF(pt.X + 8, pt.Y - 18, 16, 16);
                        g.FillEllipse(brushBadge, badgeRect);
                        g.DrawEllipse(penBorder, badgeRect);
                        string orderText = orderNum.ToString();
                        g.DrawString(orderText, fontBadge, Brushes.White, orderNum >= 10 ? pt.X + 9 : pt.X + 12, pt.Y - 16);
                    }
                }
            }

            // Legend
            g.FillRectangle(new SolidBrush(Color.FromArgb(230, 255, 255, 255)), 10, pnlGraphCanvas.Height - 65, 260, 55);
            g.DrawRectangle(Pens.LightGray, 10, pnlGraphCanvas.Height - 65, 260, 55);
            using (var fontLegend = new Font("Segoe UI", 7.5F))
            {
                g.FillRectangle(new SolidBrush(PrimaryBlue), 18, pnlGraphCanvas.Height - 55, 12, 12);
                g.DrawString("Municipal Service Depot", fontLegend, Brushes.Black, 35, pnlGraphCanvas.Height - 56);

                g.FillEllipse(new SolidBrush(CriticalRed), 18, pnlGraphCanvas.Height - 38, 12, 12);
                g.DrawString("Critical Incident Site", fontLegend, Brushes.Black, 35, pnlGraphCanvas.Height - 39);

                using (var mstPen = new Pen(MstGreen, 3f))
                {
                    g.DrawLine(mstPen, 150, pnlGraphCanvas.Height - 48, 180, pnlGraphCanvas.Height - 48);
                    g.DrawString("MST Optimized Backbone", fontLegend, Brushes.Black, 185, pnlGraphCanvas.Height - 54);
                }
            }
        }
    }
}
