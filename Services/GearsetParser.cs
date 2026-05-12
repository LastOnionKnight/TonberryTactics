using System;
using System.Text;
using System.Text.Json;
using TonberryTactics.Models;

namespace TonberryTactics.Services;

/// <summary>
/// Parses <c>GG-EXPORT:v1:&lt;base64&gt;</c> strings produced by GearGoblin's
/// <c>/goblinexport</c> command. Returns a typed payload or a diagnostic
/// error string suitable for showing the user. v0.5.1.
/// </summary>
public static class GearsetParser
{
    private const string Prefix         = "GG-EXPORT:v1:";
    private const int    SchemaVersion  = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public sealed record ParseResult(bool Success, ExportPayloadV1? Payload, string? Error);

    public static ParseResult TryParse(string? clipboardText)
    {
        if (string.IsNullOrWhiteSpace(clipboardText))
        {
            return new ParseResult(false, null, "Clipboard is empty. Run /goblinexport in-game first.");
        }

        var trimmed = clipboardText.Trim();

        if (!trimmed.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return new ParseResult(false, null,
                $"Expected prefix '{Prefix}' but got '{trimmed[..Math.Min(20, trimmed.Length)]}...'. " +
                "Did you copy the right string from /goblinexport?");
        }

        var base64 = trimmed[Prefix.Length..];
        byte[] jsonBytes;
        try
        {
            jsonBytes = Convert.FromBase64String(base64);
        }
        catch (FormatException ex)
        {
            return new ParseResult(false, null, $"Payload is not valid base64: {ex.Message}");
        }

        ExportPayloadV1? payload;
        try
        {
            var json = Encoding.UTF8.GetString(jsonBytes);
            payload = JsonSerializer.Deserialize<ExportPayloadV1>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            return new ParseResult(false, null, $"JSON parse error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new ParseResult(false, null, $"Unexpected parse error: {ex.Message}");
        }

        if (payload is null)
        {
            return new ParseResult(false, null, "Decoded payload was null.");
        }

        if (payload.V != SchemaVersion)
        {
            return new ParseResult(false, null,
                $"Schema version mismatch: expected v{SchemaVersion}, got v{payload.V}. " +
                "Update Tonberry Tactics or downgrade GearGoblin.");
        }

        if (payload.Character is null || payload.Equipped is null || payload.Equipped.Count == 0)
        {
            return new ParseResult(false, null, "Payload is structurally valid but contains no character or gear.");
        }

        return new ParseResult(true, payload, null);
    }
}
