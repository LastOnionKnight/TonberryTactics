# NOTE TO ANTIGRAVITY — Unblock Web Refresh + Fix CapFor Stub
**DTG:** 20260620_0025z
**Re:** v1.3.0 work order, Part B block + a correction to your Core surgery

---

## 1. THE BLOCK IS RESOLVED — package attached

`tt-web-refresh` was never on the D: drive or in Downloads — it lived only in the chat. Nothing was hiding; it was a handoff gap. The package is now delivered as **`tt-web-refresh.zip`**. Contents:
- `styles.css` — `:root` design tokens
- `TonberryTactics.html` — full reference page
- `components/CapGauge.razor` — the drop-in gauge component
- `README.md` — port notes

**Placement (per workspace discipline — not Downloads):** unzip and commit into the web repo at
`D:\TonberryTactics-workspace\TonberryTactics\docs\design\web-refresh\`
Downloads is transit only. The design package is a reference asset — it belongs in the repo.

You can now execute Part B.

---

## 2. CORRECTION — your `CapFor` is a placeholder, do NOT ship it

Your `ExportSchema.cs` Core surgery is otherwise correct: `ExportPayloadV2` / `ExportCharacterV2` as new additive records, V1 untouched, `TotalStats` added. Good. **But `Caps.CapFor` is a stub:**

```csharp
public static int? CapFor(TotalStat stat, int itemLevel)
{
    if (stat.DisplayName == "Determination") return null;
    return 2000;   // placeholder — WRONG for every stat
}
```

Returning a flat `2000` for every capped stat defeats the entire purpose of Schema v2. The whole reason for this feature is **real** caps — the hardcoded-cap confusion (the tester's 3,140 problem) is what we're fixing. A flat 2000 is the same bug wearing a different number, and worse: the gauges will render against a fake cap while *looking* authoritative.

**The real math already exists in the repo.** Your own log shows you read it:
- `D:\GearGoblin-v0.1\GearGoblin\...\StatCaps.cs`
- `D:\GearGoblin-v0.1\GearGoblin\...\Formulas.cs`
- `LevelTable.cs`

**Required fix:** port the actual cap formula from `StatCaps.cs` / `Formulas.cs` into `Caps.CapFor` in Core, so Core becomes the *true* single source of truth for cap math (this is also the Architecture Debt #8 resolution — the math lives in Core, plugin and web both call it, nothing duplicated). Do not leave the 2000 placeholder.

If the formula needs item-level inputs you don't yet have wired, surface that as a specific question rather than shipping a constant. The `CapFor(stat, itemLevel)` signature is right — it just needs the real body.

---

## 3. GATE REMINDER

Per the v1.3.0 work order, Gate 1 (Core) is not complete until `CapFor` returns real values and builds clean. The cap-gauges (Gate 3/4) can't be verified against a placeholder — the operator's in-game end-to-end check compares the web's cap numbers to what the game shows, and 2000-for-everything will visibly fail that check.

Order holds: Core (real CapFor) → plugin emits V2 → web binds + refresh (LIGHT brief) → trinity v1.3.0.

---

## 4. WHILE YOU'RE THERE — design direction is LOCKED

Part B must follow the **LIGHT** design brief (`LOCKED-Web-Design-Brief_20260619`). Light ground, gold→cobalt gauges, red only as the tiny over-cap state. If any earlier canvas/mockup showed a dark hero or red bar-chart, ignore it — it drifted and is superseded.
