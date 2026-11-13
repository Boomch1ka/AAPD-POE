using System;

namespace MunicipalServicesApp.Models
{
    /// <summary>
    /// Lifecycle statuses for a municipal service request.
    /// </summary>
    public enum RequestStatus
    {
        Submitted,
        UnderReview,
        Dispatched,
        InProgress,
        QualityInspection,
        Resolved
    }

    /// <summary>
    /// Urgency / priority levels for municipal service requests.
    /// Lower numerical value indicates higher dispatch priority (1 = most critical).
    /// </summary>
    public enum RequestPriority
    {
        Critical = 1, // e.g., Burst water main, downed live wire
        High = 2,     // e.g., Major traffic light failure, massive sinkhole
        Medium = 3,   // e.g., Standard pothole, sewer blockage
        Low = 4       // e.g., Broken park bench, scheduled tree trimming
    }
}
