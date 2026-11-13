using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.Models
{
    /// <summary>
    /// Represents a single issue reported by a citizen through the
    /// Report Issues screen.
    /// </summary>
    public class Issue
    {
        public int Id { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public DateTime DateReported { get; set; }

        /// <summary>
        /// Full file paths of any images/documents the citizen attached
        /// to this report via the OpenFileDialog.
        /// </summary>
        public List<string> AttachmentPaths { get; set; }

        public Issue()
        {
            AttachmentPaths = new List<string>();
            DateReported = DateTime.Now;
        }
    }
}
