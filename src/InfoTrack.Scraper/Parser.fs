module InfoTrack.Scraper.Parser

open System
open System.Text.RegularExpressions
open Types

// Block extraction
// Each result-item div has nested divs, so a simple greedy-to-first-</div> pattern
// would close too early.  The last child is always a <ul class="list-item">, so
// we anchor the end of each block on </ul> which is unambiguous.
let private extractBlocks (resultContainerClass: string) (html: string) : string list =
    let pattern = sprintf "<div[^>]+class=\"[^\"]*%s[^\"]*\"[^>]*>(.*?</ul>)" (Regex.Escape(resultContainerClass))
    Regex.Matches(html, pattern, RegexOptions.IgnoreCase ||| RegexOptions.Singleline)
    |> Seq.cast<Match>
    |> Seq.map (fun m -> m.Groups.[1].Value)
    |> Seq.toList

// Field extractors

// Firm name — first text node inside span.<nameClass>, before the .greentick inner div
let private extractName (nameClass: string) (block: string) : string option =
    let pattern = sprintf """<span[^>]+class="[^"]*%s[^"]*"[^>]*>([^<]+)""" (Regex.Escape(nameClass))
    let m = Regex.Match(block, pattern, RegexOptions.IgnoreCase)
    if m.Success then
        let text = m.Groups.[1].Value.Trim()
        if String.IsNullOrWhiteSpace(text) then None else Some text
    else None

// Phone — text content of the tel: anchor inside div.phone-block
let private extractPhone (block: string) : string option =
    let m = Regex.Match(block, "href=\"tel:[^\"]*\"[^>]*>(.*?)</a>", RegexOptions.IgnoreCase ||| RegexOptions.Singleline)
    if m.Success then
        let text = Regex.Replace(m.Groups.[1].Value, "<[^>]+>", "").Trim()
        if String.IsNullOrWhiteSpace(text) then None else Some text
    else None

// Address — text content of <address> element
let private extractAddress (block: string) : string option =
    let m = Regex.Match(block, """<address>(.*?)</address>""", RegexOptions.IgnoreCase ||| RegexOptions.Singleline)
    if m.Success then
        let text = Regex.Replace(m.Groups.[1].Value, "<[^>]+>", "").Trim()
        if String.IsNullOrWhiteSpace(text) then None else Some text
    else None

// Rating — count star-full (1.0 each) and star-half (0.5 each) within the rating container
let private extractRating (ratingContainerClass: string) (block: string) : float option =
    let pattern = sprintf """<span[^>]+class="[^"]*%s[^"]*"[^>]*>(.*?)</span>""" (Regex.Escape(ratingContainerClass))
    let revMatch = Regex.Match(block, pattern, RegexOptions.IgnoreCase ||| RegexOptions.Singleline)
    if revMatch.Success then
        let revHtml = revMatch.Groups.[1].Value
        let fullStars = Regex.Matches(revHtml, "star-full").Count
        let halfStars = Regex.Matches(revHtml, "star-half").Count
        let rating = float fullStars + float halfStars * 0.5
        if rating > 0.0 then Some rating else None
    else None

// ReviewCount — the "(N)" text node that follows the star divs inside the rating container
let private extractReviewCount (ratingContainerClass: string) (block: string) : int option =
    let pattern = sprintf """<span[^>]+class="[^"]*%s[^"]*"[^>]*>(.*?)</span>""" (Regex.Escape(ratingContainerClass))
    let revMatch = Regex.Match(block, pattern, RegexOptions.IgnoreCase ||| RegexOptions.Singleline)
    if revMatch.Success then
        let m = Regex.Match(revMatch.Groups.[1].Value, """\((\d[\d,]*)\)""")
        if m.Success then
            match Int32.TryParse(m.Groups.[1].Value.Replace(",", "")) with
            | true, v -> Some v
            | _ -> None
        else None
    else None

// Website — href of the nofollow external link (the globe-icon list item)
let private extractWebsite (block: string) : string option =
    let pattern = "href=\"(https?://[^\"]+)\"[^>]*rel=\"nofollow\""
    let m = Regex.Match(block, pattern, RegexOptions.IgnoreCase)
    if m.Success then Some m.Groups.[1].Value
    else None

//Record assembly

let private parseSolicitor (selectors: SiteSelectors) (location: string) (block: string) : Solicitor option =
    match extractName selectors.NameClass block with
    | None -> None  // name is required
    | Some name ->
        Some {
            Name = name
            Location = location
            Phone = extractPhone block
            Email = None  // solicitors.com uses an enquiry form — no direct email exposed
            Address = extractAddress block
            Rating = extractRating selectors.RatingContainer block
            ReviewCount = extractReviewCount selectors.RatingContainer block
            Website = extractWebsite block
        }

//Main entry point
let parseHtml (selectors: SiteSelectors) (location: string) (html: string) : ParseHealth =
    let blocks = extractBlocks selectors.ResultContainer html
    let solicitors =
        blocks
        |> List.choose (parseSolicitor selectors location)

    let warnings = ResizeArray<string>()

    if blocks.IsEmpty then
        Empty location
    elif solicitors.IsEmpty then
        StructureChanged $"Found {blocks.Length} result containers for {location} but could not extract any names — selector '{selectors.NameClass}' may be stale"
    elif solicitors.Length < 3 then
        warnings.Add $"Only {solicitors.Length} solicitors extracted — unusually low for {location}"
        Degraded (solicitors, warnings |> Seq.toList)
    else
        Healthy solicitors