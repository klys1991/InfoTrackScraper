namespace InfoTrack.Api.Models;

public class SolicitorRecord
{
    public int Id { get; set; }
    public int SearchRunId { get; set; }
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    public string SiteId { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public double? Rating { get; set; }
    public int? ReviewCount { get; set; }
    public string? Website { get; set; }
    public bool IsNew { get; set; }
    public string ParseHealth { get; set; } = "healthy";
    public SearchRun SearchRun { get; set; } = null!;
}