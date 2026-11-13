using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.Models
{
    /// <summary>
    /// Represents an entry in the timestamped status audit trail for a service request.
    /// </summary>
    public class StatusAuditEntry
    {
        public DateTime Timestamp { get; set; }
        public RequestStatus Status { get; set; }
        public string Note { get; set; }
        public string UpdatedBy { get; set; }

        public StatusAuditEntry() { }

        public StatusAuditEntry(RequestStatus status, string note, string updatedBy = "Municipal Dispatch")
        {
            Timestamp = DateTime.Now;
            Status = status;
            Note = note;
            UpdatedBy = updatedBy;
        }

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm}] {Status} - {Note} (by {UpdatedBy})";
        }
    }

    /// <summary>
    /// Core domain entity representing a municipal service request.
    /// Indexed by Request ID in Binary Search Trees, AVL Trees, and Red-Black Trees;
    /// prioritized by Urgency in Heaps; and mapped to locations in the Municipal Network Graph.
    /// </summary>
    public class ServiceRequest : IComparable<ServiceRequest>
    {
        public int Id { get; set; }
        public string TrackingCode => $"SR-{Id:D4}";
        public string Title { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public string Ward { get; set; }
        public string Description { get; set; }
        public DateTime DateReported { get; set; }
        public RequestPriority Priority { get; set; }
        public RequestStatus Status { get; set; }
        public string AssignedDepot { get; set; }
        public string AssignedCrew { get; set; }
        public int EstimatedHours { get; set; }
        public List<string> AttachmentPaths { get; set; }

        /// <summary>
        /// Service Request IDs of prerequisite tasks that must be resolved
        /// before this request can be completed (Service Dependency DAG).
        /// </summary>
        public List<int> DependsOnIds { get; set; }

        /// <summary>
        /// Historical timeline of status changes for transparent citizen tracking.
        /// </summary>
        public List<StatusAuditEntry> StatusHistory { get; set; }

        public ServiceRequest()
        {
            DateReported = DateTime.Now;
            Priority = RequestPriority.Medium;
            Status = RequestStatus.Submitted;
            AttachmentPaths = new List<string>();
            DependsOnIds = new List<int>();
            StatusHistory = new List<StatusAuditEntry>();
        }

        public void UpdateStatus(RequestStatus newStatus, string note, string updatedBy = "Municipal Dispatch")
        {
            Status = newStatus;
            StatusHistory.Add(new StatusAuditEntry(newStatus, note, updatedBy));
        }

        /// <summary>
        /// Compares requests primarily by Priority urgency (lower number = higher priority),
        /// then by DateReported (FIFO tie-breaker) for heap ordering.
        /// </summary>
        public int CompareTo(ServiceRequest other)
        {
            if (other == null) return 1;

            int priorityComparison = ((int)Priority).CompareTo((int)other.Priority);
            if (priorityComparison != 0) return priorityComparison;

            return DateReported.CompareTo(other.DateReported);
        }

        public override string ToString()
        {
            return $"[{TrackingCode}] ({Priority}) {Category} - {Location}: {Status}";
        }
    }
}
