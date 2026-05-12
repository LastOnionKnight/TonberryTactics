using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using TonberryTactics.Models;

namespace TonberryTactics.Services;

/// <summary>
/// Serializes an optimization result to a <c>GG-PLAN:v1:&lt;base64&gt;</c>
/// string suitable for pasting back into GearGoblin's planned
/// <c>/goblinimport</c> command. v0.5.1.
///
/// <para>
/// Wire-format symmetric with the parser side. Schema versioned in the
/// prefix so the plugin can refuse incompatible payloads cleanly.
/// </para>
/// </summary>
public static class PlanSerializer
{
    private const string Prefix         = "GG-PLAN:v1:";
    private const int    SchemaVersion  = 1;
    private const string EmitterName    = "TonberryTactics";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented        = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(
        PureMathOptimizer.Result optimization,
        ExportPayloadV1 sourcePayload,
        string emitterVersion)
    {
        var melds = optimization.Recommendations.Select(r => new PlanMeldV1(
            Piece:        r.Piece,
            PieceName:    r.PieceName,
            SlotIndex:    r.SlotIndex,
            MateriaName:  r.MateriaName,
            StatName:     r.StatName,
            StatValue:    r.StatValue
        )).ToList();

        var plan = new PlanPayloadV1(
            V:                SchemaVersion,
            Plugin:           EmitterName,
            Version:          emitterVersion,
            GeneratedAt:      DateTime.UtcNow.ToString("o"),
            SourceCharacter:  sourcePayload.Character,
            Melds:            melds);

        var json    = JsonSerializer.Serialize(plan, JsonOptions);
        var bytes   = Encoding.UTF8.GetBytes(json);
        var encoded = Prefix + Convert.ToBase64String(bytes);
        return encoded;
    }
}
