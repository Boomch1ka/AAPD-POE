using System;

namespace MunicipalServicesApp.Models
{
    /// <summary>
    /// A local event or municipal announcement shown on the
    /// Local Events and Announcements page.
    /// </summary>
    public class EventItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }

        /// <summary>1 = urgent, 2 = important, 3 = normal. Lower number = higher priority.</summary>
        public int Priority { get; set; }

        /// <summary>True for municipal notices (water outages etc.), false for community events.</summary>
        public bool IsAnnouncement { get; set; }

        public string PriorityText
        {
            get
            {
                switch (Priority)
                {
                    case 1: return "Urgent";
                    case 2: return "Important";
                    default: return "Normal";
                }
            }
        }

        public override string ToString()
        {
            return Date.ToString("dd MMM yyyy") + " - " + Title;
        }
    }
}
