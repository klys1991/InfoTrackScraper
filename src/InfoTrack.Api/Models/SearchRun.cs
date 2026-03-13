namespace InfoTrack.Api.Models;

public class SearchRun
{
    public int Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string[] Locations { get; set; } = [];
    public string Status { get; set; } = "pending"; // pending | running | completed | failed
    public ICollection<SolicitorRecord> Results { get; set; } = [];
}