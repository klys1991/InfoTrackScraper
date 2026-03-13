module InfoTrack.Scraper.Pipeline

open System
open System.Collections.Generic
open System.Net.Http
open System.Threading
open System.Threading.Tasks
open Types

// ─── HTTP fetch ───────────────────────────────────────────────────────────────

let private fetchHtml (httpClient: HttpClient) (url: string) (ct: CancellationToken)
    : Task<Result<string, ScrapeError>> =
    task {
        try
            let! response = httpClient.GetAsync(url, ct)
            match response.StatusCode with
            | Net.HttpStatusCode.TooManyRequests ->
                let retryAfter =
                    response.Headers.RetryAfter
                    |> Option.ofObj
                    |> Option.bind (fun h -> Option.ofNullable h.Delta)
                return Error (RateLimited retryAfter)
            | Net.HttpStatusCode.NotFound ->
                return Error (LocationNotFound url)
            | s when int s >= 400 ->
                return Error (NetworkError (url, Exception $"HTTP {int s}"))
            | _ ->
                let! html = response.Content.ReadAsStringAsync(ct)
                return Ok html
        with
        | :? OperationCanceledException as ex -> return raise ex
        | ex -> return Error (NetworkError (url, ex))
    }

let private extractSolicitorList (health: ParseHealth) : Solicitor list =
    match health with
    | Healthy s -> s
    | Degraded (s, _) -> s
    | _ -> []

// ─── Single location — paginated ─────────────────────────────────────────────
// Fetches pages 1..MaxPages, stops early when a page returns only names we have
// already seen (the site repeats the last real page rather than returning 404).

[<CompiledName("ScrapeLocationAsync")>]
let scrapeLocationAsync
    (httpClient: HttpClient)
    (config: SiteConfig)
    (location: string)
    (ct: CancellationToken)
    : Task<LocationResult> =
    task {
        let accumulated = ResizeArray<Solicitor>()
        let seenNames   = HashSet<string>()
        let mutable page       = 1
        let mutable keepGoing  = true
        let mutable lastHealth = Empty location

        while keepGoing && page <= config.MaxPages do
            let url = config.BuildPageUrl location page
            let! result = fetchHtml httpClient url ct

            match result with
            | Error (NetworkError (_, ex)) ->
                lastHealth <- StructureChanged $"Network error for {location}: {ex.Message}"
                keepGoing  <- false
            | Error (RateLimited _) ->
                lastHealth <- StructureChanged $"Rate limited for {location}"
                keepGoing  <- false
            | Error (LocationNotFound l) ->
                lastHealth <- Empty l
                keepGoing  <- false
            | Error (ParseFailure (_, reason)) ->
                lastHealth <- StructureChanged reason
                keepGoing  <- false
            | Ok html ->
                let pageHealth     = config.ParseSolicitors location html
                let pageSolicitors = extractSolicitorList pageHealth

                // Stop if every name on this page was already accumulated
                // (the site loops back to last page instead of 404-ing)
                let newOnes = pageSolicitors |> List.filter (fun s -> seenNames.Add(s.Name))

                if newOnes.IsEmpty && page > 1 then
                    keepGoing <- false
                else
                    accumulated.AddRange(newOnes)
                    lastHealth <- pageHealth
                    page       <- page + 1

        let allSolicitors = accumulated |> Seq.toList

        let finalHealth =
            if allSolicitors.Length >= 3 then
                Healthy allSolicitors
            elif allSolicitors.Length > 0 then
                Degraded (allSolicitors, [$"Only {allSolicitors.Length} solicitors across all pages"])
            else
                lastHealth

        return {
            Location  = location
            SiteId    = config.SiteId
            Health    = finalHealth
            ScrapedAt = DateTime.UtcNow
        }
    }

// ─── All locations ────────────────────────────────────────────────────────────

[<CompiledName("ScrapeAllLocationsAsync")>]
let scrapeAllLocationsAsync
    (httpClient: HttpClient)
    (config: SiteConfig)
    (locations: string list)
    (ct: CancellationToken)
    : Task<LocationResult list> =
    task {
        let! results =
            locations
            |> List.map (fun location -> scrapeLocationAsync httpClient config location ct)
            |> Task.WhenAll
        return results |> Array.toList
    }

// ─── New solicitor detection ──────────────────────────────────────────────────

[<CompiledName("DiffSolicitors")>]
let diffSolicitors (previous: Solicitor list) (current: Solicitor list) : SolicitorDiff =
    let previousNames = previous |> List.map (fun s -> s.Name) |> Set.ofList
    let currentNames  = current  |> List.map (fun s -> s.Name) |> Set.ofList
    {
        NewSolicitors     = current  |> List.filter (fun s -> not (Set.contains s.Name previousNames))
        RemovedSolicitors = previous |> List.filter (fun s -> not (Set.contains s.Name currentNames))
        Unchanged         = current  |> List.filter (fun s -> Set.contains s.Name previousNames) |> List.length
    }