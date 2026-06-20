using System;
using System.Collections.Generic;
using System.Linq;
using GearGoblin.Core;
using GearGoblin.Core.Materia;
using TonberryTactics.Models;

namespace TonberryTactics.Services;

public static class MeldOptimizerAdapter
{
    public static OptimizerResult Optimize(ExportPayloadV2 payload)
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
                    if (Enum.TryParse<Substat>(kvp.Key, out var s))
                        baseSubstats[s] = kvp.Value;
                }
            }

            var melds = new List<MeldSlot>();
            var stats = new Dictionary<Substat, int>();

            foreach (var m in p.Materia)
            {
                if (!Enum.TryParse<Substat>(m.StatName, out var s))
                    s = Substat.None;

                var spec = MateriaCatalog.FromGrade(m.StatName, m.Grade, m.StatValue);
                melds.Add(new MeldSlot
                {
                    SlotIndex = m.SlotIndex,
                    IsGuaranteed = m.SlotIndex < p.MateriaSlotCount,
                    Current = spec,
                    SuccessRate = SuccessRateForSlot(m.SlotIndex)
                });

                if (s != Substat.None)
                {
                    stats.TryGetValue(s, out var existing);
                    stats[s] = existing + m.StatValue;
                }
            }

            int total = p.IsOvermeldAllowed ? 5 : p.MateriaSlotCount;
            var existingIndices = new HashSet<int>(melds.Select(x => x.SlotIndex));

            for (int i = 0; i < total; i++)
            {
                if (!existingIndices.Contains(i))
                {
                    melds.Add(new MeldSlot
                    {
                        SlotIndex = i,
                        IsGuaranteed = i < p.MateriaSlotCount,
                        Current = null,
                        SuccessRate = SuccessRateForSlot(i)
                    });
                }
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
                CurrentMeldStats = stats,
                BaseSubstats = baseSubstats,
                SubstatCap = (int)p.SubstatCap
            });
        }

        var profile = JobProfiles.GetOrDefault(payload.Character.Job);
        
        // For the web app, we don't have current live stats from the game.
        // We just pass an empty stat snapshot and default LevelMod.
        var statsSnapshot = new StatSnapshot();
        var mod = LevelTable.Get(payload.Character.Level);

        return MeldOptimizer.Optimize(pieces, statsSnapshot, mod, profile, WeightMode.BalancePreset);
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




