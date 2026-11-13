using System;
using System.Collections.Generic;
using System.Linq;
using MunicipalServicesApp.Data;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Services
{
    /// <summary>
    /// Learns the citizen's interests from what they search for and open, then
    /// ranks upcoming events by relevance.
    ///
    /// How it works:
    ///  1. Every search adds weight to the chosen category (+2) and to each
    ///     keyword typed (+1). Opening an event adds +1 to its category.
    ///     These counters are Dictionary&lt;string,int&gt; (hash tables).
    ///  2. Each event is scored: 3 x category weight, plus 2 x weight of every
    ///     keyword found in its text, plus a small bonus for events happening soon.
    ///  3. Scored events go into a MinPriorityQueue (priority = -score) and the
    ///     top N are dequeued as the recommendations.
    ///  4. A Stack of recent search terms is kept so the UI can show history.
    /// </summary>
    public class RecommendationEngine
    {
        private readonly Dictionary<string, int> _categoryWeights =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, int> _keywordWeights =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private readonly Stack<string> _searchHistory = new Stack<string>();

        public bool HasHistory
        {
            get { return _categoryWeights.Count > 0 || _keywordWeights.Count > 0; }
        }

        public IEnumerable<string> RecentSearches { get { return _searchHistory; } }

        public void RecordSearch(string keyword, string category)
        {
            if (!string.IsNullOrWhiteSpace(category))
            {
                Bump(_categoryWeights, category.Trim(), 2);
                _searchHistory.Push("Category: " + category.Trim());
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                _searchHistory.Push("\"" + keyword.Trim() + "\"");
                foreach (string token in Tokenise(keyword))
                    Bump(_keywordWeights, token, 1);
            }
        }

        public void RecordView(EventItem item)
        {
            if (item != null) Bump(_categoryWeights, item.Category, 1);
        }

        /// <summary>Top categories the user has shown interest in.</summary>
        public List<KeyValuePair<string, int>> TopInterests(int count)
        {
            return _categoryWeights.OrderByDescending(p => p.Value).ThenBy(p => p.Key).Take(count).ToList();
        }

        public List<Recommendation> Recommend(IEnumerable<EventItem> candidates, int max)
        {
            var results = new List<Recommendation>();
            if (!HasHistory) return results;

            var queue = new MinPriorityQueue<Recommendation>();
            foreach (var e in candidates)
            {
                Recommendation r = Score(e);
                if (r != null) queue.Enqueue(r, -r.Score); // highest score = lowest number
            }

            while (queue.Count > 0 && results.Count < max)
                results.Add(queue.Dequeue());
            return results;
        }

        private Recommendation Score(EventItem e)
        {
            int catWeight;
            _categoryWeights.TryGetValue(e.Category, out catWeight);

            int keywordScore = 0;
            string firstKeyword = null;
            string text = (e.Title + " " + e.Description + " " + e.Category + " " + e.Location).ToLowerInvariant();
            foreach (var pair in _keywordWeights)
            {
                if (text.Contains(pair.Key.ToLowerInvariant()))
                {
                    keywordScore += pair.Value * 2;
                    if (firstKeyword == null) firstKeyword = pair.Key;
                }
            }

            int relevance = catWeight * 3 + keywordScore;
            if (relevance == 0) return null; // nothing in common with the user's interests

            int days = (e.Date - DateTime.Today).Days;
            int soonBonus = days <= 7 ? 2 : (days <= 30 ? 1 : 0);

            string reason = catWeight > 0
                ? "You showed interest in " + e.Category
                : "Matches your search for \"" + firstKeyword + "\"";

            return new Recommendation { Event = e, Score = relevance + soonBonus, Reason = reason };
        }

        private static IEnumerable<string> Tokenise(string text)
        {
            return text.ToLowerInvariant()
                       .Split(new[] { ' ', ',', ';', '.', '-' }, StringSplitOptions.RemoveEmptyEntries)
                       .Where(t => t.Length >= 3)
                       .Distinct();
        }

        private static void Bump(Dictionary<string, int> dict, string key, int amount)
        {
            int current;
            dict.TryGetValue(key, out current);
            dict[key] = current + amount;
        }
    }
}
