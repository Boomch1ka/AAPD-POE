using System;
using System.Collections.Generic;
using System.Linq;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data
{
    /// <summary>
    /// Central store for local events and announcements. The same events are
    /// indexed in several structures, each chosen for a specific job:
    ///
    ///  - SortedDictionary&lt;DateTime, List&lt;EventItem&gt;&gt; : events kept in date order,
    ///    so the page always lists them chronologically without re-sorting.
    ///  - Dictionary&lt;string, List&lt;EventItem&gt;&gt; (hash table) : O(1) lookup of
    ///    all events in a category.
    ///  - Dictionary&lt;int, EventItem&gt; (hash table) : O(1) lookup by event Id.
    ///  - SortedSet&lt;string&gt; / SortedSet&lt;DateTime&gt; (sets) : unique, sorted
    ///    categories and event dates (used to fill the filter controls).
    ///  - MinPriorityQueue : orders announcements so urgent ones come first.
    /// </summary>
    public class EventRepository
    {
        private readonly SortedDictionary<DateTime, List<EventItem>> _byDate =
            new SortedDictionary<DateTime, List<EventItem>>();

        private readonly Dictionary<string, List<EventItem>> _byCategory =
            new Dictionary<string, List<EventItem>>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<int, EventItem> _byId = new Dictionary<int, EventItem>();

        private readonly SortedSet<string> _categories = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly SortedSet<DateTime> _dates = new SortedSet<DateTime>();

        private int _nextId = 1;

        public EventRepository()
        {
            SeedSampleData();
        }

        public int Count { get { return _byId.Count; } }

        /// <summary>Unique categories (a set never holds duplicates), alphabetical.</summary>
        public IEnumerable<string> Categories { get { return _categories; } }

        /// <summary>Unique event dates, earliest first.</summary>
        public IEnumerable<DateTime> EventDates { get { return _dates; } }

        public void AddEvent(EventItem item)
        {
            item.Id = _nextId++;
            item.Date = item.Date.Date;

            List<EventItem> dayList;
            if (!_byDate.TryGetValue(item.Date, out dayList))
            {
                dayList = new List<EventItem>();
                _byDate[item.Date] = dayList;
            }
            dayList.Add(item);

            List<EventItem> catList;
            if (!_byCategory.TryGetValue(item.Category, out catList))
            {
                catList = new List<EventItem>();
                _byCategory[item.Category] = catList;
            }
            catList.Add(item);

            _byId[item.Id] = item;
            _categories.Add(item.Category);
            _dates.Add(item.Date);
        }

        public EventItem GetById(int id)
        {
            EventItem item;
            return _byId.TryGetValue(id, out item) ? item : null;
        }

        /// <summary>All events in chronological order (SortedDictionary iterates by key).</summary>
        public List<EventItem> GetAllSorted()
        {
            var result = new List<EventItem>();
            foreach (var pair in _byDate)
                result.AddRange(pair.Value.OrderBy(e => e.Priority).ThenBy(e => e.Title));
            return result;
        }

        /// <summary>
        /// Searches by optional keyword, category and date range. If a category is
        /// given the hash table narrows the candidates first; otherwise the sorted
        /// dictionary is walked in date order.
        /// </summary>
        public List<EventItem> Search(string keyword, string category, DateTime? from, DateTime? to)
        {
            IEnumerable<EventItem> candidates;

            if (!string.IsNullOrWhiteSpace(category))
            {
                List<EventItem> catList;
                candidates = _byCategory.TryGetValue(category, out catList)
                    ? (IEnumerable<EventItem>)catList
                    : new List<EventItem>();
            }
            else
            {
                candidates = GetAllSorted();
            }

            string kw = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim().ToLowerInvariant();

            var results = new List<EventItem>();
            foreach (var e in candidates)
            {
                if (from.HasValue && e.Date < from.Value.Date) continue;
                if (to.HasValue && e.Date > to.Value.Date) continue;
                if (kw != null &&
                    e.Title.ToLowerInvariant().IndexOf(kw) < 0 &&
                    e.Description.ToLowerInvariant().IndexOf(kw) < 0 &&
                    e.Location.ToLowerInvariant().IndexOf(kw) < 0 &&
                    e.Category.ToLowerInvariant().IndexOf(kw) < 0)
                    continue;
                results.Add(e);
            }
            return results.OrderBy(e => e.Date).ThenBy(e => e.Priority).ThenBy(e => e.Title).ToList();
        }

        /// <summary>
        /// Builds a priority queue of announcements so the most urgent come out first;
        /// within the same priority the earliest date comes first.
        /// </summary>
        public MinPriorityQueue<EventItem> BuildAnnouncementQueue()
        {
            var pq = new MinPriorityQueue<EventItem>();
            // Walking the sorted dictionary feeds equal-priority items in date order.
            foreach (var pair in _byDate)
                foreach (var e in pair.Value)
                    if (e.IsAnnouncement)
                        pq.Enqueue(e, e.Priority);
            return pq;
        }

        private void SeedSampleData()
        {
            DateTime t = DateTime.Today;
            Add("Planned Water Interruption", "Water & Utilities", "Ward 12, Mitchells Plain", t.AddDays(2), 1, true,
                "Water will be shut off from 08:00 to 16:00 for pipe maintenance. Please store water in advance.");
            Add("Load Shedding Schedule Update", "Electricity", "All wards", t.AddDays(1), 2, true,
                "The municipal electricity department has published the updated schedule. Check your zone online.");
            Add("Road Closure: Main Road Resurfacing", "Roads", "Main Road, Claremont", t.AddDays(4), 2, true,
                "Main Road will be closed between 20:00 and 05:00 for resurfacing. Use Protea Road as an alternative.");
            Add("Refuse Collection Day Change", "Waste Management", "Ward 5, Soweto", t.AddDays(6), 3, true,
                "Public holiday collections move to the following day. Place bins out by 06:00.");
            Add("Public Participation: Draft Budget", "Community Meeting", "City Hall, Council Chamber", t.AddDays(9), 2, true,
                "Residents are invited to comment on the draft municipal budget before it is tabled.");
            Add("Community Clean-up Day", "Community Meeting", "Mamelodi Community Park", t.AddDays(5), 3, false,
                "Join your neighbours to clean the park. Gloves and bags are provided. Bring water and a hat.");
            Add("Youth Soccer Tournament", "Sports", "Athlone Stadium", t.AddDays(8), 3, false,
                "Under-17 community teams compete for the Mayor's Cup. Free entry for spectators.");
            Add("Fun Run 5km", "Sports", "Sea Point Promenade", t.AddDays(15), 3, false,
                "A family-friendly 5km fun run raising funds for local clinics.");
            Add("Heritage Day Celebration", "Culture", "Union Buildings Gardens", t.AddDays(12), 3, false,
                "Food, music and dance celebrating South Africa's diverse cultures.");
            Add("Local Craft & Food Market", "Culture", "Greenmarket Square", t.AddDays(3), 3, false,
                "Local artisans and street-food vendors. Live music from 11:00.");
            Add("Free Health Screening", "Health", "Gugulethu Clinic", t.AddDays(7), 2, false,
                "Free blood pressure, glucose and HIV screening. No booking required.");
            Add("Child Immunisation Drive", "Health", "Khayelitsha Community Hall", t.AddDays(10), 2, false,
                "Bring your child's Road to Health card. Open 09:00 to 15:00.");
            Add("Small Business Workshop", "Education", "Municipal Library, Pretoria", t.AddDays(11), 3, false,
                "Learn how to register a business, tender for municipal work and access SMME funding.");
            Add("Library Holiday Reading Programme", "Education", "Johannesburg Central Library", t.AddDays(14), 3, false,
                "Storytelling and reading sessions for children aged 6 to 12.");
            Add("Neighbourhood Watch Meeting", "Public Safety", "Durbanville Police Station", t.AddDays(6), 2, false,
                "Meet your SAPS sector commander and learn how to report crime safely.");
            Add("Tree Planting Day", "Parks & Recreation", "Rondebosch Common", t.AddDays(16), 3, false,
                "Help plant 200 indigenous trees. Families welcome.");
            Add("Gas Leak Safety Notice", "Public Safety", "Ward 8, Durban North", t.AddDays(1), 1, true,
                "Residents in affected streets must not use open flames until the all-clear is given.");
            Add("Jazz in the Park", "Culture", "Bo-Kaap Park", t.AddDays(20), 3, false,
                "An open-air evening of local jazz artists. Bring a picnic blanket.");
        }

        private void Add(string title, string category, string location, DateTime date,
                         int priority, bool isAnnouncement, string description)
        {
            AddEvent(new EventItem
            {
                Title = title,
                Category = category,
                Location = location,
                Date = date,
                Priority = priority,
                IsAnnouncement = isAnnouncement,
                Description = description
            });
        }
    }
}
