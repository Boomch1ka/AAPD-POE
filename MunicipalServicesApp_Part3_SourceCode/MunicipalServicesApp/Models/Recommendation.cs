namespace MunicipalServicesApp.Models
{
    /// <summary>A suggested event together with its relevance score and a human-readable reason.</summary>
    public class Recommendation
    {
        public EventItem Event { get; set; }
        public int Score { get; set; }
        public string Reason { get; set; }
    }
}
