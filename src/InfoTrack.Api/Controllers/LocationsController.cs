using Microsoft.AspNetCore.Mvc;

namespace InfoTrack.Api.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private static readonly List<string> DefaultLocations =
    [
        "London", "Birmingham", "Leeds", "Manchester",
        "Sheffield", "Bradford", "Liverpool", "Bristol"
    ];

    [HttpGet]
    public IActionResult GetDefaults() => Ok(DefaultLocations);
}