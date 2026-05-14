// GearGoblin.Core/StatNames.cs
//
// v0.6.3 — Centralized BaseParam name canonicalization. Both halves of
// Tonberry Tactics see stat names from different sources:
//
//   - The plugin's GearsetExporter writes the in-game BaseParam display
//     names verbatim ("Critical Hit", "Direct Hit Rate", "Determination").
//   - The web's StatProfile grid keys on abbreviations ("CRT", "DH",
//     "DET").
//   - Older v0.4.x plan strings sometimes used mixed forms.
//
// This helper accepts any form (full name, abbreviation, common
// synonyms, whitespace and case variations) and returns the canonical
// three-letter key the optimizer and UI consume.
//
// History:
//   - v0.6.0 web: had separate uppercase-match against abbreviations
//     only. Plugin-emitted "Critical Hit" missed → all stats showed +0.
//   - v0.6.1 web: introduced inline CanonicalStatKey helper, matched
//     both full names and abbreviations. Worked.
//   - v0.6.2 web: ditto, plus role-aware Speed slot picking.
//   - v0.6.3 (this): the helper moves into Core so the plugin can
//     share it and the two sides can't drift on the matching rules.

using System;
using System.Linq;

namespace GearGoblin.Core;

/// <summary>
/// Canonicalize FFXIV substat names to a three-letter key. Accepts the
/// abbreviated form ("CRT", "DH", "DET", "SKS", "SPS", "TEN", "PIE"),
/// the in-game BaseParam display name ("Critical Hit", "Direct Hit Rate",
/// "Determination", "Skill Speed", "Spell Speed", "Tenacity", "Piety"),
/// and common variants (case-insensitive, whitespace-stripped, with
/// or without "Rate" suffix on Direct Hit).
/// </summary>
public static class StatNames
{
    // ── Three-letter canonical keys ───────────────────────────────────

    public const string Crit       = "CRT";
    public const string DirectHit  = "DH";
    public const string Det        = "DET";
    public const string SkillSpeed = "SKS";
    public const string SpellSpeed = "SPS";
    public const string Tenacity   = "TEN";
    public const string Piety      = "PIE";

    /// <summary>
    /// Convert any plugin-emitted or hand-typed substat name into its
    /// canonical three-letter form. Returns an empty string for
    /// null/whitespace input and the uppercased input verbatim for
    /// anything we don't recognise — callers can then decide whether
    /// an unrecognised stat name is a parse error or just an unfamiliar
    /// future addition.
    ///
    /// <para>Examples:</para>
    /// <list type="bullet">
    /// <item><c>Canonical("Critical Hit")</c> → <c>"CRT"</c></item>
    /// <item><c>Canonical("crt")</c> → <c>"CRT"</c></item>
    /// <item><c>Canonical("Direct Hit Rate")</c> → <c>"DH"</c></item>
    /// <item><c>Canonical("DirectHit")</c> → <c>"DH"</c></item>
    /// <item><c>Canonical("Spell Speed")</c> → <c>"SPS"</c></item>
    /// <item><c>Canonical(null)</c> → <c>""</c></item>
    /// </list>
    /// </summary>
    public static string Canonical(string? statName)
    {
        if (string.IsNullOrWhiteSpace(statName)) return string.Empty;

        // Strip whitespace and uppercase. Faster than a regex; this
        // lives in a hot path called once per materia per render.
        var s = new string(statName.Where(c => !char.IsWhiteSpace(c)).ToArray())
                  .ToUpperInvariant();

        return s switch
        {
            "CRT" or "CRIT" or "CRITICAL" or "CRITICALHIT"
                => Crit,
            "DH"  or "DIRECT" or "DIRECTHIT" or "DIRECTHITRATE"
                => DirectHit,
            "DET" or "DETERMINATION"
                => Det,
            "SKS" or "SS" or "SKILLSPEED"
                => SkillSpeed,
            "SPS" or "SPELLSPEED"
                => SpellSpeed,
            "TEN" or "TNC" or "TENACITY"
                => Tenacity,
            "PIE" or "PTY" or "PIETY"
                => Piety,

            _ => s,
        };
    }

    /// <summary>
    /// Reverse of <see cref="Canonical"/> — go from canonical key to
    /// the in-game BaseParam display name, which is what gets written
    /// into <c>ExportMateriaV1.StatName</c> on the wire and into the
    /// in-game character panel rows.
    /// </summary>
    public static string DisplayName(string canonicalKey) => canonicalKey switch
    {
        Crit       => "Critical Hit",
        DirectHit  => "Direct Hit Rate",
        Det        => "Determination",
        SkillSpeed => "Skill Speed",
        SpellSpeed => "Spell Speed",
        Tenacity   => "Tenacity",
        Piety      => "Piety",
        _          => canonicalKey ?? string.Empty,
    };

    /// <summary>
    /// Whether <paramref name="canonicalKey"/> is one of the seven
    /// known substat keys. Useful for filtering wire payloads.
    /// </summary>
    public static bool IsKnown(string? canonicalKey) =>
        canonicalKey == Crit       || canonicalKey == DirectHit
     || canonicalKey == Det        || canonicalKey == SkillSpeed
     || canonicalKey == SpellSpeed || canonicalKey == Tenacity
     || canonicalKey == Piety;

    /// <summary>
    /// Is this stat a "Speed" stat (Skill Speed or Spell Speed)?
    /// Used by the role-aware Stat Profile grid to pick which one to
    /// show in the speed slot.
    /// </summary>
    public static bool IsSpeed(string? canonicalKey) =>
        canonicalKey == SkillSpeed || canonicalKey == SpellSpeed;
}
