// GearGoblin.Core/JobPriorities.cs
//
// v0.6.3 — Initial release. Per-job materia stat-priority tables for the
// Pure-Math optimizer. Both the web's PureMathOptimizer and (eventually)
// the plugin's MeldOptimizer consume these so the two halves of Tonberry
// Tactics agree on what to recommend.
//
// ════════════════════════════════════════════════════════════════════════
//  PRIORITY MODEL
// ════════════════════════════════════════════════════════════════════════
//
// Each job maps to an ORDERED list of stat names. The optimizer rotates
// through this list once per empty materia slot it encounters, dumping
// melds into stats in priority order. The TOP of the list is what the
// job benefits from most per stat-point at typical iLvl ranges.
//
// The list does NOT model:
//   - GCD breakpoints (Skill/Spell Speed caps to hit 2.50/2.40/etc. GCD)
//   - Overall stat caps (hitting Crit-rate plateau)
//   - Fight-specific tuning (some content rewards Det over DH)
//   - HQ vs NQ materia tiers (we recommend Tier XII baseline)
//
// These are simplifications. The expectation is that the plugin's
// MeldOptimizer (which has more in-game context) will eventually consume
// this same table plus its own breakpoint awareness. For now this gets
// the web out of "GNB fallback for everyone" and into "actually relevant
// recommendations per job."
//
// ════════════════════════════════════════════════════════════════════════
//  SOURCES
// ════════════════════════════════════════════════════════════════════════
//
// Priorities derived from public BiS/guide pages on thebalanceffxiv.com
// for the patch 7.x (Dawntrail) tier. Where a job has multiple competing
// schools of thought, the priority used here reflects the most-
// recommended option in the guide. Casters and healers prioritize their
// Speed stat slot differently per job; that nuance is flattened to "your
// Speed first if your guide caps it, otherwise Crit first" — most jobs
// in practice meld Crit > DH > DET > Speed regardless.
//
// Patch-tier-specific accuracy is not a stable target — these tables
// will need refreshing each major patch (typically every 6 months) and
// can also be overridden in-app once the v0.7.x Settings tab supports
// custom priority editing.
//
// ════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;

namespace GearGoblin.Core;

/// <summary>
/// Per-job materia stat-priority tables. Look up via
/// <see cref="For(string)"/> using a job's three-letter abbreviation
/// (PLD, WAR, GNB, WHM, ...). Falls back to a role-derived default for
/// unrecognized abbreviations (e.g. a future job we haven't tabled yet).
/// </summary>
public static class JobPriorities
{
    // ── Canonical priority lists per job ──────────────────────────────
    //
    // Stat names match the in-game BaseParam display strings verbatim
    // ("Critical Hit", "Direct Hit Rate", "Determination", "Skill Speed",
    // "Spell Speed", "Tenacity", "Piety") so the optimizer can compare
    // against the plugin-emitted StatName field in ExportMateriaV1
    // without additional canonicalization in the hot path. Callers that
    // need to compare against abbreviated forms should route through
    // GearGoblin.Core.StatNames.Canonical first.

    private static readonly string[] TankBaseline =
    {
        "Critical Hit",      // tank primary — universal scaling
        "Direct Hit Rate",   // secondary damage
        "Determination",     // tertiary damage
        "Skill Speed",       // GCD speed (cap to job-preferred GCD then fall off)
        "Tenacity",          // role stat — lowest priority but real
    };

    private static readonly string[] HealerBaseline =
    {
        "Critical Hit",      // crit damage scales healing AND damage
        "Direct Hit Rate",   // direct-hit chance lifts damage but not heals (caster scales DH too)
        "Determination",     // flat multiplier on both damage and healing
        "Spell Speed",       // caster GCD
        "Piety",             // MP regen — comfort stat, slot of last resort
    };

    private static readonly string[] MeleeDpsBaseline =
    {
        "Critical Hit",
        "Direct Hit Rate",
        "Determination",
        "Skill Speed",       // melee GCD; specific job guides may cap-then-stop
    };

    private static readonly string[] PhysRangedBaseline =
    {
        "Critical Hit",
        "Direct Hit Rate",
        "Determination",
        "Skill Speed",
    };

    private static readonly string[] MageBaseline =
    {
        "Critical Hit",
        "Direct Hit Rate",
        "Determination",
        "Spell Speed",
    };

    /// <summary>
    /// Job abbreviation → priority list. Twenty-one combat jobs (Endwalker
    /// roster + Dawntrail additions VPR + PCT). Each list is ordered from
    /// highest priority to lowest; the optimizer rotates through it once
    /// per empty materia slot. Stat names use FFXIV in-game BaseParam
    /// display strings (with "Direct Hit Rate" rather than "Direct Hit")
    /// so callers don't need to re-canonicalize.
    /// </summary>
    private static readonly Dictionary<string, string[]> JobTable =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // ── Tanks ────────────────────────────────────────────────
            ["PLD"] = TankBaseline,
            ["WAR"] = TankBaseline,
            ["DRK"] = TankBaseline,
            ["GNB"] = TankBaseline,

            // ── Healers ──────────────────────────────────────────────
            ["WHM"] = HealerBaseline,
            ["SCH"] = HealerBaseline,
            ["AST"] = HealerBaseline,
            ["SGE"] = HealerBaseline,

            // ── Melee DPS ────────────────────────────────────────────
            ["MNK"] = MeleeDpsBaseline,
            ["DRG"] = MeleeDpsBaseline,
            ["NIN"] = MeleeDpsBaseline,
            ["SAM"] = MeleeDpsBaseline,
            ["RPR"] = MeleeDpsBaseline,
            ["VPR"] = MeleeDpsBaseline,

            // ── Physical Ranged DPS ─────────────────────────────────
            ["BRD"] = PhysRangedBaseline,
            ["MCH"] = PhysRangedBaseline,
            ["DNC"] = PhysRangedBaseline,

            // ── Magical Ranged DPS / Casters ────────────────────────
            ["BLM"] = MageBaseline,
            ["SMN"] = MageBaseline,
            ["RDM"] = MageBaseline,
            ["PCT"] = MageBaseline,

            // BLU lives in its own category (Limited Job, can't raid in
            // current content beyond Masked Carnivale). Treated as mage
            // baseline for completeness — gets fallback behaviour and
            // priority recs that won't be very useful in practice.
            ["BLU"] = MageBaseline,
        };

    /// <summary>
    /// Resolve the stat-priority list for the given job abbreviation.
    /// Returns a defensive copy so callers can iterate without holding a
    /// reference to the internal table. If the job isn't tabled (e.g. a
    /// new job from a future patch we haven't seen yet), falls back to
    /// the GNB tank priority — the same fallback the v0.6.0/0.6.1/0.6.2
    /// web shipped, so unrecognised jobs keep working the way they used
    /// to instead of erroring.
    /// </summary>
    public static IReadOnlyList<string> For(string? jobAbbreviation)
    {
        if (!string.IsNullOrWhiteSpace(jobAbbreviation)
            && JobTable.TryGetValue(jobAbbreviation!, out var list))
        {
            return list;
        }

        // Fallback — preserves v0.6.0 behaviour for unrecognised jobs.
        // Callers checking IsTableHit() can surface a "FALLBACK" badge
        // in the UI for honesty about what's going on.
        return TankBaseline;
    }

    /// <summary>
    /// Whether the given job has a real entry in the priority table.
    /// False means callers got the fallback (GNB tank baseline) from
    /// <see cref="For"/>. UI surfaces should display a clear "fallback"
    /// indicator so users know recommendations aren't job-tuned.
    /// </summary>
    public static bool IsTableHit(string? jobAbbreviation) =>
        !string.IsNullOrWhiteSpace(jobAbbreviation)
        && JobTable.ContainsKey(jobAbbreviation!);

    /// <summary>
    /// Human-readable description of the priority source. Surfaced in
    /// the web's Materia Advisor badge ("AST priority", "GNB FALLBACK
    /// (no table for XYZ)", etc.) and in the plugin's About tab.
    /// </summary>
    public static string SourceLabel(string? jobAbbreviation)
    {
        if (string.IsNullOrWhiteSpace(jobAbbreviation))
            return "GNB fallback (no job)";

        return JobTable.ContainsKey(jobAbbreviation!)
            ? $"{jobAbbreviation!.ToUpperInvariant()} priority"
            : $"GNB fallback (no table for {jobAbbreviation})";
    }

    /// <summary>
    /// All job abbreviations with table entries. Useful for diagnostics
    /// and for the Settings tab when it eventually lets users browse
    /// and override priorities.
    /// </summary>
    public static IEnumerable<string> AllTabledJobs() => JobTable.Keys;
}
