using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace InfoTrack.Api.Services;

/// <summary>
/// Thin helpers for bridging C# and F# type boundaries.
/// All conversions are kept in this file — orchestrator calls these rather than
/// scattering FSharp.Core API calls throughout business logic.
/// </summary>
internal static class FSharpInterop
{
    // ── Option → nullable ────────────────────────────────────────────────────

    public static T? ValueOrDefault<T>(this FSharpOption<T>? opt) where T : class =>
        opt is not null && FSharpOption<T>.get_IsSome(opt) ? opt.Value : null;

    public static double? ValueOrDefault(this FSharpOption<double>? opt) =>
        opt is not null && FSharpOption<double>.get_IsSome(opt) ? opt.Value : (double?)null;

    public static int? ValueOrDefault(this FSharpOption<int>? opt) =>
        opt is not null && FSharpOption<int>.get_IsSome(opt) ? opt.Value : (int?)null;

    // ── Nullable → option ────────────────────────────────────────────────────

    public static FSharpOption<T> ToFSharpOption<T>(this T? value) where T : class =>
        value is not null ? FSharpOption<T>.Some(value) : FSharpOption<T>.None;

    public static FSharpOption<double> ToFSharpOption(this double? value) =>
        value.HasValue ? FSharpOption<double>.Some(value.Value) : FSharpOption<double>.None;

    public static FSharpOption<int> ToFSharpOption(this int? value) =>
        value.HasValue ? FSharpOption<int>.Some(value.Value) : FSharpOption<int>.None;

    // ── IEnumerable → FSharpList ──────────────────────────────────────────────

    public static FSharpList<T> ToFSharpList<T>(this IEnumerable<T> source) =>
        ListModule.OfSeq(source);
}