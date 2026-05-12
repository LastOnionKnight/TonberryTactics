using System.Collections.Generic;
using TonberryTactics.Models;

namespace TonberryTactics.Services;

/// <summary>
/// Pure-Math optimizer for Tonberry Tactics v0.5.1. Currently hardcoded to
/// Gunbreaker stat priorities (Crit &gt; Direct Hit &gt; Determination).
///
/// <para>
/// Limitations vs the plugin's full MeldOptimizer:
/// </para>
/// <list type="bullet">
///   <item>Doesn't audit existing melds (wrong-stat or overcap swaps).</item>
///   <item>Only fills <em>guaranteed</em> empty slots — doesn't recommend
///   overmelds, which need success-probability math.</item>
///   <item>Single job profile (GNB). Other jobs surface as "use Pure Math
///   defaults" with the same rotation, which is wrong but acceptable as a
///   placeholder until v0.5.2 wires multi-job profiles.</item>
///   <item>No stat-cap awareness — over-recommends if a slot would
///   overcap. The plugin handles this; we don't (yet) have the BaseParam
///   data in-browser.</item>
/// </list>
///
/// <para>
/// v0.5.2 plan: extract <c>GearGoblin.Core.dll</c> as a shared assembly
/// compiling for both .NET and Wasm so this whole class can be retired in
/// favor of the plugin's real MeldOptimizer.
/// </para>
/// </summary>
public static class PureMathOptimizer
{
    // GNB rotation. Loops over the priority list once per empty slot
    // encountered to spread melds across stats rather than dumping every
    // slot into Crit. Realistic GNB BiS usually does dump everything into
    // Crit + a few Det/DH; the rotation here is a reasonable v0.5.1
    // placeholder until we model stat caps.
    private static readonly string[] GnbPriority =
    {
        "Critical Hit",
        "Direct Hit Rate",
        "Determination",
    };

    public sealed record Recommendation(
        string Piece,
        string PieceName,
        int SlotIndex,
        string MateriaName,
        string StatName,
        int StatValue);

    public sealed record Result(
        List<Recommendation> Recommendations,
        int TotalEmptySlots,
        string OptimizationMode);

    public static Result Optimize(ExportPayloadV1 payload)
    {
        var recs = new List<Recommendation>();
        int statRotation = 0;
        int totalEmpty = 0;

        foreach (var piece in payload.Equipped)
        {
            int filledSlots = piece.Materia?.Count ?? 0;
            int totalGuaranteedSlots = piece.MateriaSlotCount;
            int emptySlots = totalGuaranteedSlots - filledSlots;
            if (emptySlots <= 0) continue;

            totalEmpty += emptySlots;

            for (int i = 0; i < emptySlots; i++)
            {
                int slotIndex = filledSlots + i;
                var stat = GnbPriority[statRotation % GnbPriority.Length];
                statRotation++;

                // Tier XII materia values per Akhmorning Allagan Studies.
                // Substat materia (Crit/DH/Det/SkS/SpS/Ten) Tier XII = +36.
                // Primary stat materia (Str/Dex/Int/Mnd/Pie) differ; not
                // used in the GNB damage rotation.
                recs.Add(new Recommendation(
                    Piece:        piece.Slot,
                    PieceName:    piece.Name,
                    SlotIndex:    slotIndex,
                    MateriaName:  $"{stat} XII",
                    StatName:     stat,
                    StatValue:    36));
            }
        }

        // Use a job-aware mode label so the UI can show what we did.
        var mode = payload.Character.JobAbbreviation == "GNB"
            ? "GNB Pure Math (hardcoded)"
            : $"{payload.Character.JobAbbreviation} fallback (GNB priority)";

        return new Result(recs, totalEmpty, mode);
    }
}
