using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MunicipalServicesApp.Data;
using MunicipalServicesApp.Models;
using MunicipalServicesApp.Services;

namespace MunicipalServicesApp
{
    /// <summary>
    /// Local Events and Announcements page. Lets citizens browse and search
    /// upcoming events, read announcements, and see personalised recommendations.
    /// The UI is built in code with docked/table layouts so it resizes with the window.
    /// </summary>
    public class LocalEventsForm : Form
    {
        private static readonly Color Blue = Color.FromArgb(0, 90, 158);
        private static readonly Color LightBlue = Color.FromArgb(0, 120, 200);
        private static readonly Color Green = Color.FromArgb(0, 150, 80);

        private readonly EventRepository _repo = new EventRepository();
        private readonly RecommendationEngine _engine = new RecommendationEngine();

        // Stack: events the citizen has opened, most recent on top (supports "Back").
        private readonly Stack<EventItem> _viewStack = new Stack<EventItem>();
        // Queue: announcements shown one at a time, rotating first-in-first-out.
        private readonly Queue<EventItem> _announcementQueue = new Queue<EventItem>();

        private List<Recommendation> _currentRecs = new List<Recommendation>();
        private bool _suppressPush;

        // Controls
        private TextBox txtSearch;
        private ComboBox cmbCategory;
        private CheckBox chkDate;
        private DateTimePicker dtpFrom, dtpTo;
        private ListView lvEvents;
        private RichTextBox rtbDetails;
        private Label lblStatus, lblInterests, lblAnnouncement, lblAnnouncementCount, lblBanner;
        private ListBox lstRecommendations, lstRecent;
        private Button btnNextAnnouncement, btnBack;

        public LocalEventsForm()
        {
            BuildUi();
            LoadFilters();
            BuildAnnouncementQueue();
            ShowAnnouncement();
            ShowResults(_repo.GetAllSorted(), "Showing all upcoming events and announcements.");
            RefreshRecommendations();
        }

        // ------------------------------------------------------------ UI
        private void BuildUi()
        {
            Text = "Municipal Services Application - Local Events and Announcements";
            ClientSize = new Size(1150, 720);
            MinimumSize = new Size(950, 620);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9.5F);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(10) };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            Controls.Add(root);

            // Header with the urgent-notice banner (top of the priority queue)
            var header = new Panel { Dock = DockStyle.Fill };
            var title = new Label
            {
                Text = "Local Events and Announcements",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Blue,
                Dock = DockStyle.Top,
                Height = 34
            };
            lblBanner = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(192, 57, 43),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Padding = new Padding(8, 0, 0, 0)
            };
            header.Controls.Add(lblBanner);
            header.Controls.Add(title);
            root.Controls.Add(header, 0, 0);

            root.Controls.Add(BuildSearchPanel(), 0, 1);

            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            content.Controls.Add(BuildLeftPanel(), 0, 0);
            content.Controls.Add(BuildRightPanel(), 1, 0);
            root.Controls.Add(content, 0, 2);

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var btnMenu = MakeButton("Back to Main Menu", Blue, 170);
            btnMenu.Click += (s, e) => Close();
            bottom.Controls.Add(btnMenu);
            root.Controls.Add(bottom, 0, 3);
        }

        private Control BuildSearchPanel()
        {
            var box = new GroupBox { Text = "Search events", Dock = DockStyle.Fill };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, Padding = new Padding(4, 2, 4, 2) };

            flow.Controls.Add(new Label { Text = "Keyword:", AutoSize = true, Margin = new Padding(3, 8, 3, 0) });
            txtSearch = new TextBox { Width = 170 };
            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; PerformSearch(); }
            };
            flow.Controls.Add(txtSearch);

            flow.Controls.Add(new Label { Text = "Category:", AutoSize = true, Margin = new Padding(10, 8, 3, 0) });
            cmbCategory = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            flow.Controls.Add(cmbCategory);

            chkDate = new CheckBox { Text = "Date range:", AutoSize = true, Margin = new Padding(10, 6, 3, 0) };
            chkDate.CheckedChanged += (s, e) => { dtpFrom.Enabled = chkDate.Checked; dtpTo.Enabled = chkDate.Checked; };
            flow.Controls.Add(chkDate);

            dtpFrom = new DateTimePicker { Width = 110, Format = DateTimePickerFormat.Short, Enabled = false, Value = DateTime.Today };
            dtpTo = new DateTimePicker { Width = 110, Format = DateTimePickerFormat.Short, Enabled = false, Value = DateTime.Today.AddDays(30) };
            flow.Controls.Add(dtpFrom);
            flow.Controls.Add(new Label { Text = "to", AutoSize = true, Margin = new Padding(3, 8, 3, 0) });
            flow.Controls.Add(dtpTo);

            var btnSearch = MakeButton("Search", Green, 90);
            btnSearch.Click += (s, e) => PerformSearch();
            flow.Controls.Add(btnSearch);

            var btnClear = MakeButton("Clear", Color.Gray, 80);
            btnClear.Click += (s, e) => ClearSearch();
            flow.Controls.Add(btnClear);

            box.Controls.Add(flow);
            return box;
        }

        private Control BuildLeftPanel()
        {
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 38));

            lblStatus = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Blue, Font = new Font("Segoe UI", 9.5F, FontStyle.Italic) };
            table.Controls.Add(lblStatus, 0, 0);

            lvEvents = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                HideSelection = false
            };
            lvEvents.Columns.Add("Date", 95);
            lvEvents.Columns.Add("Title", 230);
            lvEvents.Columns.Add("Category", 120);
            lvEvents.Columns.Add("Location", 170);
            lvEvents.Columns.Add("Type", 90);
            lvEvents.SelectedIndexChanged += lvEvents_SelectedIndexChanged;
            table.Controls.Add(lvEvents, 0, 1);

            var detailsBox = new GroupBox { Text = "Details", Dock = DockStyle.Fill };
            rtbDetails = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = Color.White };
            detailsBox.Controls.Add(rtbDetails);
            table.Controls.Add(detailsBox, 0, 2);
            return table;
        }

        private Control BuildRightPanel()
        {
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8, 0, 0, 0) };
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 28));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

            // Recommendations
            var recBox = new GroupBox { Text = "Recommended for you", Dock = DockStyle.Fill };
            lstRecommendations = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false, DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 38 };
            lstRecommendations.DrawItem += lstRecommendations_DrawItem;
            lstRecommendations.DoubleClick += lstRecommendations_DoubleClick;
            lblInterests = new Label { Dock = DockStyle.Top, Height = 34, ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8.5F, FontStyle.Italic) };
            recBox.Controls.Add(lstRecommendations);   // Fill first,
            recBox.Controls.Add(lblInterests);         // then Top, so Top docks above
            table.Controls.Add(recBox, 0, 0);

            // Announcements (queue)
            var annBox = new GroupBox { Text = "Announcements", Dock = DockStyle.Fill };
            var annPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            annPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            annPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            annPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            lblAnnouncementCount = new Label { Dock = DockStyle.Fill, ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8.5F) };
            lblAnnouncement = new Label { Dock = DockStyle.Fill };
            btnNextAnnouncement = MakeButton("Next announcement", LightBlue, 160);
            btnNextAnnouncement.Click += (s, e) => { RotateAnnouncement(); ShowAnnouncement(); };
            annPanel.Controls.Add(lblAnnouncementCount, 0, 0);
            annPanel.Controls.Add(lblAnnouncement, 0, 1);
            annPanel.Controls.Add(btnNextAnnouncement, 0, 2);
            annBox.Controls.Add(annPanel);
            table.Controls.Add(annBox, 0, 1);

            // Recently viewed (stack)
            var recentBox = new GroupBox { Text = "Recently viewed", Dock = DockStyle.Fill };
            lstRecent = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
            btnBack = MakeButton("Back to previous event", LightBlue, 170);
            btnBack.Dock = DockStyle.Bottom;
            btnBack.Click += btnBack_Click;
            recentBox.Controls.Add(lstRecent);
            recentBox.Controls.Add(btnBack);
            table.Controls.Add(recentBox, 0, 2);
            return table;
        }

        private static Button MakeButton(string text, Color back, int width)
        {
            var b = new Button
            {
                Text = text,
                Width = width,
                Height = 30,
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        // ------------------------------------------------------------ Data -> UI
        private void LoadFilters()
        {
            cmbCategory.Items.Add("All Categories");
            foreach (string c in _repo.Categories) cmbCategory.Items.Add(c); // SortedSet: unique + alphabetical
            cmbCategory.SelectedIndex = 0;

            // Date pickers are limited to the span covered by the set of event dates.
            var dates = _repo.EventDates.ToList();
            if (dates.Count > 0)
            {
                dtpFrom.Value = dates.First();
                dtpTo.Value = dates.Last();
            }
        }

        private void ShowResults(List<EventItem> items, string status)
        {
            lvEvents.BeginUpdate();
            lvEvents.Items.Clear();
            foreach (var e in items)
            {
                var row = new ListViewItem(e.Date.ToString("dd MMM yyyy"));
                row.SubItems.Add(e.Title);
                row.SubItems.Add(e.Category);
                row.SubItems.Add(e.Location);
                row.SubItems.Add(e.IsAnnouncement ? "Announcement" : "Event");
                row.Tag = e;
                if (e.Priority == 1) { row.ForeColor = Color.DarkRed; row.Font = new Font(lvEvents.Font, FontStyle.Bold); }
                lvEvents.Items.Add(row);
            }
            lvEvents.EndUpdate();
            lblStatus.Text = status;
            rtbDetails.Clear();
            if (items.Count == 0)
                rtbDetails.Text = "No events match your search. Try a different keyword, category or date range.";
        }

        private void ShowDetails(EventItem e)
        {
            rtbDetails.Clear();
            rtbDetails.SelectionFont = new Font("Segoe UI", 12F, FontStyle.Bold);
            rtbDetails.SelectionColor = Blue;
            rtbDetails.AppendText(e.Title + "\r\n");
            rtbDetails.SelectionFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            rtbDetails.SelectionColor = Color.Black;
            rtbDetails.AppendText(e.Date.ToString("dddd, dd MMMM yyyy") + "  |  " + e.Location + "\r\n");
            rtbDetails.AppendText(e.Category + "  |  " + (e.IsAnnouncement ? "Announcement" : "Event") + "  |  Priority: " + e.PriorityText + "\r\n\r\n");
            rtbDetails.SelectionFont = new Font("Segoe UI", 10F);
            rtbDetails.AppendText(e.Description);
        }

        // ------------------------------------------------------------ Events
        private void PerformSearch()
        {
            string keyword = txtSearch.Text.Trim();
            string category = cmbCategory.SelectedIndex > 0 ? cmbCategory.SelectedItem.ToString() : null;
            DateTime? from = null, to = null;

            if (chkDate.Checked)
            {
                if (dtpFrom.Value.Date > dtpTo.Value.Date)
                {
                    MessageBox.Show("The 'from' date must be on or before the 'to' date.",
                        "Invalid date range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                from = dtpFrom.Value.Date;
                to = dtpTo.Value.Date;
            }

            if (keyword.Length == 0 && category == null && !chkDate.Checked)
            {
                MessageBox.Show("Enter a keyword, choose a category or tick the date range to search.",
                    "Nothing to search for", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var results = _repo.Search(keyword, category, from, to);
            _engine.RecordSearch(keyword, category); // learn from this search
            ShowResults(results, results.Count + (results.Count == 1 ? " event found." : " events found."));
            RefreshRecommendations();
        }

        private void ClearSearch()
        {
            txtSearch.Clear();
            cmbCategory.SelectedIndex = 0;
            chkDate.Checked = false;
            ShowResults(_repo.GetAllSorted(), "Showing all upcoming events and announcements.");
        }

        private void lvEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvEvents.SelectedItems.Count == 0) return;
            var item = (EventItem)lvEvents.SelectedItems[0].Tag;
            OpenEvent(item);
        }

        private void OpenEvent(EventItem item)
        {
            ShowDetails(item);
            if (_suppressPush) return;

            if (_viewStack.Count == 0 || _viewStack.Peek().Id != item.Id)
            {
                _viewStack.Push(item);
                _engine.RecordView(item);
                RefreshRecentList();
                RefreshRecommendations();
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (_viewStack.Count < 2)
            {
                MessageBox.Show("There is no earlier event to go back to.", "Back",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _viewStack.Pop();                 // discard the current one
            EventItem previous = _viewStack.Peek();
            _suppressPush = true;
            try
            {
                SelectInList(previous);
                ShowDetails(previous);
            }
            finally { _suppressPush = false; }
            RefreshRecentList();
        }

        private void SelectInList(EventItem item)
        {
            foreach (ListViewItem row in lvEvents.Items)
            {
                if (((EventItem)row.Tag).Id == item.Id)
                {
                    row.Selected = true;
                    row.EnsureVisible();
                    return;
                }
            }
            lvEvents.SelectedItems.Clear();
        }

        private void RefreshRecentList()
        {
            lstRecent.Items.Clear();
            foreach (var e in _viewStack) lstRecent.Items.Add(e.ToString()); // top of stack first
        }

        // ------------------------------------------------------------ Announcements
        private void BuildAnnouncementQueue()
        {
            // Priority queue sorts by urgency; draining it into a Queue gives a FIFO carousel.
            var pq = _repo.BuildAnnouncementQueue();
            while (pq.Count > 0) _announcementQueue.Enqueue(pq.Dequeue());

            if (_announcementQueue.Count > 0)
            {
                EventItem top = _announcementQueue.Peek();
                lblBanner.Text = "  URGENT: " + top.Title + " (" + top.Date.ToString("dd MMM") + ", " + top.Location + ")";
            }
            else
            {
                lblBanner.Text = "  No urgent notices at the moment.";
                lblBanner.BackColor = Green;
            }
        }

        private void RotateAnnouncement()
        {
            if (_announcementQueue.Count > 1)
                _announcementQueue.Enqueue(_announcementQueue.Dequeue());
        }

        private void ShowAnnouncement()
        {
            if (_announcementQueue.Count == 0)
            {
                lblAnnouncement.Text = "There are no announcements.";
                lblAnnouncementCount.Text = "";
                btnNextAnnouncement.Enabled = false;
                return;
            }
            EventItem a = _announcementQueue.Peek();
            lblAnnouncementCount.Text = _announcementQueue.Count + " announcement(s)  -  " + a.PriorityText;
            lblAnnouncement.Text = a.Title + "\r\n" + a.Date.ToString("dd MMM yyyy") + " - " + a.Location + "\r\n\r\n" + a.Description;
        }

        // ------------------------------------------------------------ Recommendations
        private void RefreshRecommendations()
        {
            _currentRecs = _engine.Recommend(_repo.GetAllSorted(), 5);
            lstRecommendations.Items.Clear();

            if (!_engine.HasHistory)
            {
                lblInterests.Text = "Search for events or open one and we'll suggest others you may like.";
                return;
            }

            var interests = _engine.TopInterests(3);
            lblInterests.Text = interests.Count > 0
                ? "Your interests: " + string.Join(", ", interests.Select(i => i.Key))
                : "Based on your recent searches.";

            foreach (var r in _currentRecs) lstRecommendations.Items.Add(r);
            if (_currentRecs.Count == 0) lblInterests.Text += "\r\nNo matching events right now.";
        }

        private void lstRecommendations_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0 || e.Index >= _currentRecs.Count) return;
            var r = _currentRecs[e.Index];
            Brush text = (e.State & DrawItemState.Selected) != 0 ? Brushes.White : Brushes.Black;
            Brush sub = (e.State & DrawItemState.Selected) != 0 ? Brushes.White : Brushes.DimGray;
            using (var bold = new Font("Segoe UI", 9.5F, FontStyle.Bold))
            using (var small = new Font("Segoe UI", 8.5F))
            {
                e.Graphics.DrawString(r.Event.Title, bold, text, e.Bounds.X + 4, e.Bounds.Y + 2);
                e.Graphics.DrawString(r.Event.Date.ToString("dd MMM") + " - " + r.Reason, small, sub, e.Bounds.X + 4, e.Bounds.Y + 20);
            }
            e.DrawFocusRectangle();
        }

        private void lstRecommendations_DoubleClick(object sender, EventArgs e)
        {
            int i = lstRecommendations.SelectedIndex;
            if (i < 0 || i >= _currentRecs.Count) return;
            EventItem item = _currentRecs[i].Event;
            SelectInList(item);   // selecting triggers OpenEvent when the event is in the list
            if (lvEvents.SelectedItems.Count == 0) OpenEvent(item); // filtered out of the list: still show it
        }
    }
}
