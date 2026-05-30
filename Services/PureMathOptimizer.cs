using System.Collections.Generic;
using GearGoblin.Core;
using TonberryTactics.Models;

namespace TonberryTactics.Services;

/// <summary>
/// Pure-Math optimizer for Tonberry Tactics web. v0.6.4 routes
/// recommendations through the vendored <see cref="JobPriorities"/>
/// table in <c>Services/Core/</c> — same data the plugin's
/// <c>GearGoblin.Core</c> ProjectReference resolves, just baked into
/// this repo so Cloudflare Pages CI doesn't need a sibling checkout.
///
/// <para>History at a glance:</para>
/// <list type="bullet">
///   <item>
///     <b>v0.5.1 – v0.6.0:</b> hardcoded GNB tank priority
///     (Crit → DH → DET) for every job. The Materia Advisor surfaced
///     an "AST FALLBACK (GNB PRIORITY)" badge to be honest about it.
///     Users who play not-Gunbreaker saw not-relevant recommendations.
///   </item>
///   <item>
///     <b>v0.6.3:</b> rewrote this class to consume <c>Core.JobPriorities</c>
///     via <c>&lt;ProjectReference&gt;</c> to the sibling Core repo. Worked
///     locally; broke Cloudflare CI (Core repo not in build context).
///     Reverted as <c>86beba2</c>.
///   </item>
///   <item>
///     <b>v0.6.4 (this):</b> same Core types, vendored into
///     <c>Services/Core/</c>. Web builds self-contained in CI; plugin
///     keeps real ProjectReference. Two halves agree on namespaces and
///     signatures; bytes are kept in sync by hand for now (submodule
///     migration tracked for v0.7.x).
///   </item>
/// </list>
///
/// <para>Still TODO past v0.6.4 (tracked in CHANGELOG):</para>
/// <list type="bullet">
///   <item>No stat-cap awareness — over-recommends if a slot would
///   overcap. The plugin handles this via its in-game BaseParam read;
///   the web doesn't yet have BaseParam data in-browser.</item>
///   <item>No audit of existing melds (wrong-stat or overcap swaps).
///   Current logic only fills <em>guaranteed</em> empty slots.</item>
///   <item>No overmeld recommendations — those need success-probability
///   math (slot 2 = 35% on most pieces, etc.). Out of scope for
///   Pure-Math; lives on the Balance preset roadmap.</item>
/// </list>
/// </summary>
public static class PureMathOptimizer
{
    public sealed record Recommendation(
        string Piece,
        string PieceName,
        int    SlotIndex,
        string MateriaName,
        string StatName,
        int    StatValue);

    public sealed record Result(
        List<Recommendation> Recommendations,
        int                  TotalEmptySlots,
        string               OptimizationMode);

    public static Result Optimize(ExportPayloadV1 payload)
    {
        // v0.6.4 — resolve the active job's stat-priority list via Core.
        // For tabled jobs (all 21 combat jobs plus BLU), this returns the
        // real per-job rotation. For an unknown job (future patch
        // additions we haven't yet tabled), Core falls back to the GNB
        // tank baseline — same behaviour as v0.6.0 web, but now scoped
        // to the actual edge case rather than the everyone-not-GNB
        // common case.
        var jobAbbr  = payload.Character.JobAbbreviation;
        var priority = JobPriorities.For(jobAbbr);
        var isHit    = JobPriorities.IsTableHit(jobAbbr);
        var modeLabel = isHit
            ? $"{jobAbbr} priority (via Core)"
            : $"{jobAbbr} fallback (no Core table)";

        // Materia tier for new recommendations — current patch cap.
        // Core's MateriaTiers tracks the latest tier per patch so both
        // halves recommend the same grade.
        const int recTier  = MateriaTiers.CurrentCapTier;
        var       recValue = MateriaTiers.SubstatValue(recTier);

        var recs         = new List<Recommendation>();
        int statRotation = 0;
        int totalEmpty   = 0;

        foreach (var piece in payload.Equipped)
        {
            int filledSlots          = piece.Materia?.Count ?? 0;
            int totalGuaranteedSlots = piece.MateriaSlotCount;
            int emptySlots           = totalGuaranteedSlots - filledSlots;
            if (emptySlots <= 0) continue;

            totalEmpty += emptySlots;

            var pieceMeldTotals = new Dictionary<string, int>();
            if (piece.Materia != null)
            {
                foreach (var m in piece.Materia)
                {
                    pieceMeldTotals.TryGetValue(m.StatName, out var v);
                    pieceMeldTotals[m.StatName] = v + m.StatValue;
                }
            }

            for (int i = 0; i < emptySlots; i++)
            {
                int slotIndex = filledSlots + i;

                string? chosenStat = null;
                for (int attempts = 0; attempts < priority.Count; attempts++)
                {
                    var stat = priority[statRotation % priority.Count];
                    statRotation++;

                    pieceMeldTotals.TryGetValue(stat, out var currentOnPiece);
                    var baseOnPiece = 0;
                    if (piece.BaseSubstats != null)
                        piece.BaseSubstats.TryGetValue(stat, out baseOnPiece);
                    
                    var roomForStat = Math.Max(0, (int)piece.SubstatCap - (currentOnPiece + baseOnPiece));
                    var actualGain  = Math.Min(recValue, roomForStat);
                    
                    // If no cap is provided (SubstatCap == 0) from an old V1 export, blindly allow.
                    if (piece.SubstatCap == 0 || actualGain > 0)
                    {
                        chosenStat = stat;
                        pieceMeldTotals[stat] = currentOnPiece + recValue;
                        break;
                    }
                }

                if (chosenStat == null)
                    continue; // All priority stats are capped.

                recs.Add(new Recommendation(
                    Piece:       piece.Slot,
                    PieceName:   piece.Name,
                    SlotIndex:   slotIndex,
                    MateriaName: MateriaTiers.NameOf(chosenStat, recTier),
                    StatName:    chosenStat,
                    StatValue:   recValue));
            }
        }

        return new Result(recs, totalEmpty, modeLabel);
    }
}
