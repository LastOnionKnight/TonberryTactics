# TonberryTactics web v0.6.5.1 dropin — "Audit reads right"

**Hotfix.** Fixes the off-by-one Tier display in the v0.6.5 audit
panel. Two-line root cause: wire format's `Grade` field is 0-indexed
(Tier XII materia arrives as Grade=11), and the audit was comparing
that directly against the 1-indexed `MateriaTiers.CurrentCapTier`
(=12) and rendering with `RomanGrade(11) = "XI"`. Every Tier XII
meld flagged as under-tier with a phantom upgrade recommendation.

## What's in this dropin

```
Pages/Index.razor       overwrite — add +1 to Grade at the consumer
TonberryTactics.csproj  overwrite — version 0.6.5 → 0.6.5.1
CHANGELOG.md            overwrite — v0.6.5.1 entry on top
```

`Services/Core/MateriaTiers.cs` is **not** in this dropin —
unchanged from v0.6.5.

## Build & deploy

```
cd D:\TonberryTactics-workspace\TonberryTactics
Move-Item $env:USERPROFILE\Downloads\TonberryTactics-v0.6.5.1-dropin.zip ..\ -Force
Expand-Archive -Path ..\TonberryTactics-v0.6.5.1-dropin.zip -DestinationPath . -Force
Unblock-File .\release.ps1
git status
dotnet build -c Release
.\release.ps1 -DryRun
.\release.ps1
```

## Verify after Cloudflare Pages deploys

1. Hard-refresh tonberrytactics.pages.dev (Ctrl+F5).
2. Header pill should read **v0.6.5.1**. Footer copy should say
   `TONBERRY TACTICS · v0.6.5.1 · CLOUDFLARE PAGES · ...`.
3. Paste the same VPR/PLD export you used to test v0.6.5.
4. **Open the Meld Audit panel.** Pre-v0.6.5.1:
   - Under-tier: 20 (all melds flagged)
   - Sell / replace: 20
   - Per-materia breakdown: every row said "Direct Hit Rate XI (+54) →
     Tier XI below cap (XII) → Upgrade to Tier XII (+54)"
5. Post-v0.6.5.1 on the **same export**:
   - Under-tier: 0 (or very few, only if real Tier XI melds exist)
   - Sell / replace: just the wrong-stat findings
   - Per-materia breakdown: melds correctly render "Det XII (+54)",
     "Crit XII (+54)", etc.
6. **If you have any actual Tier XI melds** in the gearset (uncommon
   but possible), those should still flag correctly with "Tier XI
   below cap (XII)" and a recommendation to upgrade to Tier XII.

## Pairing

- **GearGoblin.Core v0.6.5.1** — lockstep version bump only.
- **GearGoblin plugin v0.6.5.1** — "Quiet Info". `/ttinfo` crash fix
  + About-tab What's New trim. Ship plugin before web so the export
  format is right.

## Note on the wire-format choice

The fix is at the consumer (web), not the emitter (plugin). This is
deliberate:

- Existing GG-EXPORT:v1: strings on Discord, in chat logs, in users'
  clipboards continue to work — no schema churn.
- When v0.7.x lifts the plugin's `MeldOptimizer` into Core as a
  shared service, that code already handles raw 0-indexed grades
  correctly through its `MateriaCatalog` lookups. The web's audit
  will eventually consume that same Core path, at which point the
  `displayGrade = m.Grade + 1` shim here gets removed.

## Out of scope (v0.6.6+)

- Balance preset (toggle exists; both modes fall through to
  Pure-Math).
- In-game `GG-PLAN:v1:` paste UI (plugin v0.6.6).
- Stat-cap math + Akhmorning breakpoint formulas (v0.8.x).
