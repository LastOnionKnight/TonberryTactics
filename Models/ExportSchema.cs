using System.Collections.Generic;

namespace TonberryTactics.Models;

// =============================================================================
// GG-EXPORT:v1: wire-format DTOs. Match the records emitted by
// GearGoblin/Services/GearsetExporter.cs verbatim. The plugin and web app
// share this contract; bumping the wire version (v1 -> v2) requires
// adding new record types here, not mutating these. v0.5.1.
// =============================================================================

public sealed record ExportPayloadV1(
    int V,
    string Plugin,
    string Version,
    string ExportedAt,
    ExportCharacterV1 Character,
    List<ExportPieceV1> Equipped);

public sealed record ExportCharacterV1(
    uint Job,
    string JobAbbreviation,
    int Level,
    int AverageItemLevel);

public sealed record ExportPieceV1(
    string Slot,
    uint ItemId,
    string Name,
    uint ItemLevel,
    bool IsHighQuality,
    byte MateriaSlotCount,
    bool IsOvermeldAllowed,
    List<ExportMateriaV1> Materia,
    uint SubstatCap = 0,
    Dictionary<string, int>? BaseSubstats = null);

public sealed record ExportMateriaV1(
    int SlotIndex,
    ushort MateriaId,
    byte Grade,
    string StatName,
    int StatValue);

// =============================================================================
// GG-PLAN:v1: wire-format DTOs. Emitted by Tonberry Tactics' optimizer,
// consumed by GearGoblin's planned /goblinimport command. v0.5.1.
// =============================================================================

public sealed record PlanPayloadV1(
    int V,
    string Plugin,
    string Version,
    string GeneratedAt,
    ExportCharacterV1 SourceCharacter,
    List<PlanMeldV1> Melds);

public sealed record PlanMeldV1(
    string Piece,
    string PieceName,
    int SlotIndex,
    string MateriaName,
    string StatName,
    int StatValue);
