namespace InfoTrack.Api.Models;

public class LocationBaseline
{
    public int Id { get; set; }
    public string Location { get; set; } = "";
    public string SiteId { get; set; } = "";
    public int LastKnownCount { get; set; }
    public string LastHealth { get; set; } = "empty";
    public DateTime LastVerified { get; set; }
}