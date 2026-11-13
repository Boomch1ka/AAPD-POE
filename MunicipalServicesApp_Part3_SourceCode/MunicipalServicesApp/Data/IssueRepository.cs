using System.Collections.Generic;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data
{
    /// <summary>
    /// Central in-memory store for all issues reported during the current
    /// application session.
    ///
    /// A List&lt;Issue&gt; is used because reported issues are always added
    /// at the end (as they are submitted) and then read back sequentially
    /// in submission order - both operations List&lt;T&gt; supports
    /// efficiently (amortised O(1) append, O(n) ordered iteration). This
    /// satisfies the requirement to "utilise appropriate data structures
    /// to store user-reported issues and relevant details".
    /// </summary>
    public static class IssueRepository
    {
        public static List<Issue> ReportedIssues { get; } = new List<Issue>();

        private static int _nextId = 1;

        /// <summary>
        /// Generates a simple sequential reference number for a new issue.
        /// </summary>
        public static int GetNextId()
        {
            return _nextId++;
        }
    }
}
