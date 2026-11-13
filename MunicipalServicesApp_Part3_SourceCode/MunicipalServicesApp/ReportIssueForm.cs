using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using MunicipalServicesApp.Data;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp
{
    /// <summary>
    /// Lets a citizen report a municipal issue: location, category,
    /// description and optional media attachments. Includes a gamified
    /// engagement feature (progress bar + encouraging label) that fills
    /// in as the citizen completes the form, and is the user engagement
    /// strategy chosen and justified in the Task 1 research report.
    /// </summary>
    public partial class ReportIssueForm : Form
    {
        // Full paths of files the citizen has attached to this report.
        private readonly List<string> _attachedFiles = new List<string>();

        public ReportIssueForm()
        {
            InitializeComponent();
            PopulateCategories();
            UpdateEngagementProgress();
        }

        private void PopulateCategories()
        {
            cmbCategory.Items.AddRange(new object[]
            {
                "Sanitation",
                "Roads",
                "Water & Utilities",
                "Electricity",
                "Waste Management",
                "Public Safety",
                "Parks & Recreation",
                "Other"
            });
        }

        private void txtLocation_TextChanged(object sender, EventArgs e)
        {
            UpdateEngagementProgress();
        }

        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateEngagementProgress();
        }

        private void rtbDescription_TextChanged(object sender, EventArgs e)
        {
            UpdateEngagementProgress();
        }

        private void btnAttach_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Attach images or documents related to the issue";
                dialog.Filter = "Supported files (*.jpg;*.jpeg;*.png;*.pdf;*.docx;*.txt)|" +
                                 "*.jpg;*.jpeg;*.png;*.pdf;*.docx;*.txt|All files (*.*)|*.*";
                dialog.Multiselect = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var fileName in dialog.FileNames)
                    {
                        _attachedFiles.Add(fileName);
                        lstAttachments.Items.Add(Path.GetFileName(fileName));
                    }

                    UpdateEngagementProgress();
                }
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLocation.Text))
            {
                MessageBox.Show("Please enter the location of the issue.", "Missing information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLocation.Focus();
                return;
            }

            if (cmbCategory.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a category for the issue.", "Missing information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCategory.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(rtbDescription.Text))
            {
                MessageBox.Show("Please provide a description of the issue.", "Missing information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                rtbDescription.Focus();
                return;
            }

            var issue = new Issue
            {
                Id = IssueRepository.GetNextId(),
                Location = txtLocation.Text.Trim(),
                Category = cmbCategory.SelectedItem.ToString(),
                Description = rtbDescription.Text.Trim()
            };
            issue.AttachmentPaths.AddRange(_attachedFiles);

            IssueRepository.ReportedIssues.Add(issue);
            ServiceRequestRepository.Instance.AddFromReportedIssue(issue);

            MessageBox.Show(
                "Thank you! Your issue (Reference #" + issue.Id + ") has been submitted successfully." +
                Environment.NewLine + "A municipal official will review it shortly.",
                "Report submitted",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ClearForm();
        }

        private void ClearForm()
        {
            txtLocation.Clear();
            cmbCategory.SelectedIndex = -1;
            rtbDescription.Clear();
            lstAttachments.Items.Clear();
            _attachedFiles.Clear();
            UpdateEngagementProgress();
        }

        /// <summary>
        /// Gamified engagement feature: updates the progress bar and an
        /// encouraging message as the citizen completes each of the four
        /// report fields (location, category, description, attachment).
        /// </summary>
        private void UpdateEngagementProgress()
        {
            int completed = 0;
            const int totalSteps = 4;

            if (!string.IsNullOrWhiteSpace(txtLocation.Text)) completed++;
            if (cmbCategory.SelectedIndex != -1) completed++;
            if (!string.IsNullOrWhiteSpace(rtbDescription.Text)) completed++;
            if (_attachedFiles.Count > 0) completed++;

            int percentage = (completed * 100) / totalSteps;
            progressEngagement.Value = percentage;

            switch (completed)
            {
                case 0:
                    lblEngagement.Text = "Let's get started - tell us where the issue is.";
                    break;
                case 1:
                    lblEngagement.Text = "Great start! Keep going, every detail helps.";
                    break;
                case 2:
                    lblEngagement.Text = "Halfway there! Your report is taking shape.";
                    break;
                case 3:
                    lblEngagement.Text = "Almost done! Attach a photo if you can.";
                    break;
                case 4:
                    lblEngagement.Text = "All set! Thank you for helping improve your community.";
                    break;
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
