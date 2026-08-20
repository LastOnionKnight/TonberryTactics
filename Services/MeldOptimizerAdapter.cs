using System;
using System.Collections.Generic;
using System.Linq;
using GearGoblin.Core;
using GearGoblin.Core.Materia;
using TonberryTactics.Models;

namespace TonberryTactics.Services;

/// <summary>
/// Adapts a GG-EXPORT:v2 payload into GearGoblin.Core's optimizer model.
/// The web and plugin therefore run the same meld engine against the same
/// current-stat baseline and selected weight mode.
/// </summary>
public static class MeldOptimizerAdapter
{
    public static OptimizerResult Optimize(ExportPayloadV2 payload) =>
        Optimize(payload, WeightMode.BalancePreset);

    public static OptimizerResult Optimize(
        ExportPayloadV2 payload,
        WeightMode weightMode,
        uint? jobOverride = null)
    {
        var pieces = new List<MeldablePiece>();

        foreach (var p in payload.Equipped)
        {
            if (!Enum.TryParse<EquipSlot>(p.Slot, out var slot))
                slot = EquipSlot.Unknown;

            var baseSubstats = new Dictionary<Substat, int>();
            if (p.BaseSubstats != null)
            {
                foreach (var kvp in p.BaseSubstats)
                {
                    if (Enum.TryParse<Substat>(kvp.Key, out var stat))
                        baseSubstats[stat] = kvp.Value;
                }
            }

            var melds = new List<MeldSlot>();
            var currentMeldStats = new Dictionary<Substat, int>();

            foreach (var m in p.Materia)
            {
                var spec = MateriaCatalog.FromGrade(m.StatName, m.Grade, m.StatValue);
                melds.Add(new MeldSlot
                {
                    SlotIndex = m.SlotIndex,
                    IsGuaranteed = m.SlotIndex < p.MateriaSlotCount,
                    Current = spec,
                    SuccessRate = SuccessRateForSlot(m.SlotIndex)
                });

                if (spec.Stat != Substat.None)
                {
                    currentMeldStats.TryGetValue(spec.Stat, out var existing);
                    currentMeldStats[spec.Stat] = existing + m.StatValue;
                }
            }

            int totalSlots = p.IsOvermeldAllowed ? 5 : p.MateriaSlotCount;
            var existingIndices = new HashSet<int>(melds.Select(x => x.SlotIndex));

            for (int i = 0; i < totalSlots; i++)
            {
                if (existingIndices.Contains(i))
                    continue;

                melds.Add(new MeldSlot
                {
                    SlotIndex = i,
                    IsGuaranteed = i < p.MateriaSlotCount,
                    Current = null,
                    SuccessRate = SuccessRateForSlot(i)
                });
            }

            melds.Sort((a, b) => a.SlotIndex.CompareTo(b.SlotIndex));

            pieces.Add(new MeldablePiece
            {
                Slot = slot,
                Name = p.Name,
                ItemId = p.ItemId,
                ItemLevel = p.ItemLevel,
                IsHighQuality = p.IsHighQuality,
                Slots = melds,
                CurrentMeldStats = currentMeldStats,
                BaseSubstats = baseSubstats,
                SubstatCap = (int)p.SubstatCap
            });
        }

        uint effectiveJobId = jobOverride.GetValueOrDefault(payload.Character.Job);
        if (effectiveJobId == 0)
            effectiveJobId = payload.Character.Job;

        var profile = JobProfiles.GetOrDefault(effectiveJobId);
        var stats = BuildStatSnapshot(payload.Character, effectiveJobId);
        var mod = LevelTable.Get(payload.Character.Level);

        return MeldOptimizer.Optimize(pieces, stats, mod, profile, weightMode);
    }

    private static StatSnapshot BuildStatSnapshot(ExportCharacterV2 character, uint effectiveJobId)
    {
        var totals = character.TotalStats
            .GroupBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Last().Value, StringComparer.OrdinalIgnoreCase);

        int Get(string name) => totals.TryGetValue(name, out var value) ? value : 0;

        return new StatSnapshot(
            Crit: Get("Critical Hit"),
            Det: Get("Determination"),
            DH: Get("Direct Hit"),
            SkS: Get("Skill Speed"),
            SpS: Get("Spell Speed"),
            Ten: Get("Tenacity"),
            Pie: Get("Piety"),
            Level: character.Level,
            JobId: effectiveJobId,
            Craftsmanship: Get("Craftsmanship"),
            Control: Get("Control"),
            CP: Get("CP"),
            Gathering: Get("Gathering"),
            Perception: Get("Perception"),
            GP: Get("GP")
        );
    }

    private static double SuccessRateForSlot(int slotIndex) => slotIndex switch
    {
        0 => 1.00,
        1 => 1.00,
        2 => 0.17,
        3 => 0.10,
        4 => 0.07,
        _ => 0.00,
    };
}
