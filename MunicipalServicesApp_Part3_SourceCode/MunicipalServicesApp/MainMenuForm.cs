using System;
using System.Windows.Forms;

namespace MunicipalServicesApp
{
    /// <summary>
    /// The application's landing screen. Presents the three top-level
    /// municipal services tasks. "Report Issues" (Part 1) and "Local Events and
    /// Announcements" (Part 2) are active; "Service Request Status" stays disabled until the final PoE.
    /// </summary>
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();
        }

        private void btnReportIssues_Click(object sender, EventArgs e)
        {
            this.Hide();

            using (var reportForm = new ReportIssueForm())
            {
                reportForm.ShowDialog();
            }

            this.Show();
        }

        private void btnLocalEvents_Click(object sender, EventArgs e)
        {
            this.Hide();

            using (var eventsForm = new LocalEventsForm())
            {
                eventsForm.ShowDialog();
            }

            this.Show();
        }

        private void btnServiceStatus_Click(object sender, EventArgs e)
        {
            this.Hide();

            using (var statusForm = new ServiceRequestStatusForm())
            {
                statusForm.ShowDialog();
            }

            this.Show();
        }
    }
}
