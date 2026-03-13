module InfoTrack.Scraper.Sites.SolicitorsCom

open System
open InfoTrack.Scraper.Types
open InfoTrack.Scraper.Parser

// Selectors for solicitors.com conveyancing listing pages.
// Verified against https://www.solicitors.com/conveyancing+london.html on 2026-03-12
let private selectors : SiteSelectors = {
    ResultContainer = "result-item"   // outer card div for each solicitor listing
    NameClass       = "h2"            // span.h2 — firm name is the first text node before .greentick
    RatingContainer = "rev-results"   // span.rev-results — star divs + "(N)" review count
}

let config : SiteConfig = {
    SiteId = "solicitors-com"
    BaseUrl = "https://www.solicitors.com"
    BuildPageUrl = fun location page ->
        let encoded = Uri.EscapeDataString(location.ToLowerInvariant())
        let base' = $"https://www.solicitors.com/conveyancing+{encoded}.html"
        if page <= 1 then base' else $"{base'}?page={page}"
    ParseSolicitors = fun location html -> parseHtml selectors location html
    MaxPages = 10   // site loops back after last real page, so this is a safety cap
}