module InfoTrack.Scraper.Types

open System

type SiteSelectors = {
    ResultContainer: string   // CSS class of the outer card div per listing
    NameClass: string         // CSS class of the element containing the firm name
    RatingContainer: string   // CSS class of the element containing star ratings and review count
}

type Solicitor = {
    Name: string
    Location: string
    Phone: string option
    Email: string option
    Address: string option
    Rating: float option
    ReviewCount: int option
    Website: string option
}

type ScrapeError =
    | NetworkError of url: string * exn: exn
    | ParseFailure of location: string * reason: string
    | LocationNotFound of location: string
    | RateLimited of retryAfter: TimeSpan option

type ParseHealth =
    | Healthy of Solicitor list
    | Degraded of Solicitor list * warnings: string list
    | StructureChanged of evidence: string
    | Empty of location: string

// location -> page (1-based) -> url
// location -> html -> ParseHealth
type SiteConfig = {
    SiteId: string
    BaseUrl: string
    BuildPageUrl: string -> int -> string
    ParseSolicitors: string -> string -> ParseHealth
    MaxPages: int
}

type LocationResult = {
    Location: string
    SiteId: string
    Health: ParseHealth
    ScrapedAt: DateTime
}

type SolicitorDiff = {
    NewSolicitors: Solicitor list
    RemovedSolicitors: Solicitor list
    Unchanged: int
}

// C# interop helper — create a Solicitor record from individual fields
module SolicitorModule =
    let Create
        (name: string)
        (location: string)
        (phone: string option)
        (email: string option)
        (address: string option)
        (rating: float option)
        (reviewCount: int option)
        (website: string option) : Solicitor =
        { Name = name
          Location = location
          Phone = phone
          Email = email
          Address = address
          Rating = rating
          ReviewCount = reviewCount
          Website = website }