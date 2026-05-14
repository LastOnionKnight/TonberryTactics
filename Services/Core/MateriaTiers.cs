// GearGoblin.Core/MateriaTiers.cs
//
// v0.6.3 — Materia tier → stat value tables. Used by both halves to
// decide what stat value to assume when recommending a new meld.
//
// Materia tiers in FFXIV are written with roman numerals (I through XII
// as of patch 7.x Dawntrail). Each tier has a fixed stat value that
// depends on which BaseParam the materia targets — substats (Crit, DH,
// Det, SkS, SpS, Ten) and the role stat Piety scale on a different
// curve from primary attributes (Str, Dex, Int, Mnd, Vit).
//
// Tonberry Tactics only optimizes substat materia (and Piety for
// healers), so this table only encodes those. Primary-attribute melds
// aren't part of the workflow.
//
// Source: cross-checked between Akhmorning's Allagan Studies and the
// in-game datamining communities. Values for Tier XII are the patch 7.x
// release values; if Square updates them in a sub-patch we'd refresh.

using System;
using System.Collections.Generic;

namespace GearGoblin.Core;

/// <summary>
/// Materia grade → stat value lookup. Grades are 1-indexed (Grade 1 =
/// Tier I, Grade 12 = Tier XII). Stat values shown here are for
/// substat materia (Crit, DH, Det, SkS, SpS, Ten, Pie) — the only
/// materia kinds the optimizer recommends.
/// </summary>
public static class MateriaTiers
{
    // ── Substat stat-value per tier ───────────────────────────────────
    //
    // Tier I:   +1    (Materia)
    // Tier II:  +2    (Materia II)
    // Tier III: +4    (Materia III)
    // Tier IV:  +6    (Materia IV)
    // Tier V:   +9    (Materia V)
    // Tier VI:  +12   (Materia VI)
    // Tier VII: +20   (Materia VII)
    // Tier VIII:+24   (Materia VIII)
    // Tier IX:  +36   (Materia IX)
    // Tier X:   +40   (Materia X)   — Endwalker introduction
    // Tier XI:  +48   (Materia XI)  — Endwalker high
    // Tier XII: +54   (Materia XII) — Dawntrail current
    //
    // The optimizer defaults to recommending Tier XII (current cap) for
    // new melds, but callers can request lower tiers via Value().

    private static readonly Dictionary<int, int> SubstatValues = new()
    {
        [1]  = 1,
        [2]  = 2,
        [3]  = 4,
        [4]  = 6,
        [5]  = 9,
        [6]  = 12,
        [7]  = 20,
        [8]  = 24,
        [9]  = 36,
        [10] = 40,
        [11] = 48,
        [12] = 54,
    };

    /// <summary>
    /// Current cap tier — the highest tier worth recommending in patch
    /// 7.x. Updated each major patch.
    /// </summary>
    public const int CurrentCapTier = 12;

    /// <summary>
    /// Substat stat-value at a given tier. Returns the Tier XII value
    /// for out-of-range tiers so callers don't crash on bad input.
    /// </summary>
    public static int SubstatValue(int tier) =>
        SubstatValues.TryGetValue(tier, out var v)
            ? v
            : SubstatValues[CurrentCapTier];

    /// <summary>
    /// Convert a tier number to its roman-numeral name suffix, e.g.
    /// <c>RomanNumeral(12) == "XII"</c>. Used by the optimizer to
    /// build the materia name string ("Savage Aim Materia XII").
    /// </summary>
    public static string RomanNumeral(int tier) => tier switch
    {
        1  => "I",   2  => "II",  3  => "III", 4  => "IV",
        5  => "V",   6  => "VI",  7  => "VII", 8  => "VIII",
        9  => "IX",  10 => "X",   11 => "XI",  12 => "XII",
        _  => tier.ToString(),
    };

    /// <summary>
    /// Build the in-game materia name for a stat at a tier, e.g.
    /// <c>NameOf("Critical Hit", 12) == "Savage Aim Materia XII"</c>.
    /// Names match the in-game item display strings.
    /// </summary>
    public static string NameOf(string statName, int tier)
    {
        var roman = RomanNumeral(tier);
        var prefix = MateriaPrefix(statName);
        return $"{prefix} Materia {roman}";
    }

    /// <summary>
    /// Materia prefix per stat — the words that go in front of "Materia
    /// XII" to form the in-game item name. Casters and tanks share the
    /// same materia for any given substat (a Critical Hit materia is a
    /// Critical Hit materia regardless of who melds it), so this is a
    /// pure stat-to-prefix lookup.
    ///
    /// <para>v0.6.4 — Skill Speed prefix corrected from "Piety" to
    /// "Quickarm" (the actual in-game item name). The v0.6.3 stub used
    /// "Piety" as a placeholder during overnight Core ship; this is
    /// the proper value. Piety has its own materia ("Piety") and Skill
    /// Speed has "Quickarm"; the two were transposed.</para>
    /// </summary>
    public static string MateriaPrefix(string statName) =>
        statName?.Trim() switch
        {
            "Critical Hit"     => "Savage Aim",
            "Direct Hit Rate"  => "Heavens' Eye",
            "Direct Hit"       => "Heavens' Eye",
            "Determination"    => "Savage Might",
            "Skill Speed"      => "Quickarm",
            "Spell Speed"      => "Quicktongue",
            "Tenacity"         => "Battledance",
            "Piety"            => "Piety",
            _                   => statName ?? "Generic",
        };

    // v0.6.4: Skill Speed prefix now returns the correct in-game item
    // name ("Quickarm Materia XII") rather than the v0.6.3 placeholder
    // ("Piety Materia XII"). Web's vendored copy of MateriaTiers.cs is
    // one Core-content-version behind for the duration of web v0.6.4;
    // sync happens on web's next release.

    /// <summary>
    /// All tiers we have stat values for. Iteration order is 1 → 12.
    /// </summary>
    public static IEnumerable<int> AllTiers()
    {
        for (var t = 1; t <= CurrentCapTier; t++) yield return t;
    }
}
