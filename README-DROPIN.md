# TonberryTactics web v0.6.5 dropin — "Audit lit up"

**Headline:** Wires the Meld Audit panel rows (Wrong stat /
Under-tier / Overcap) to real logic, adds a Sell / replace verdict
row with per-materia expandable breakdown, and syncs the vendored
MateriaTiers.cs to match Core's Quickarm fix.

## What's in this dropin

```
Services/Core/MateriaTiers.cs    overwrite — Skill Speed prefix sync (Piety → Quickarm)
Pages/Index.razor                 overwrite — real audit logic + Sell/replace row + version bump
TonberryTactics.csproj            overwrite — version 0.6.4 → 0.6.5
CHANGELOG.md                      overwrite — v0.6.5 entry on top
```

## Build & deploy

```
cd D:\TonberryTactics-workspace\TonberryTactics
Move-Item $env:USERPROFILE\Downloads\TonberryTactics-v0.6.5-dropin.zip ..\ -Force
Expand-Archive -Path ..\TonberryTactics-v0.6.5-dropin.zip -DestinationPath . -Force
Unblock-File .\release.ps1
git status
dotnet build -c Release
.\release.ps1 -DryRun
.\release.ps1
```

## Verify after Cloudflare Pages deploys

1. Hard-refresh tonberrytactics.pages.dev (Ctrl+F5).
2. Header pill should read **v0.6.5**. Footer copy should say
   `TONBERRY TACTICS · v0.6.5 · CLOUDFLARE PAGES · ...`.
3. Paste a real `GG-EXPORT:v1:` string from the plugin (v0.6.5
   plugin recommended — that's the one with the HQ fix).
4. **Open the Meld Audit panel.** Pre-v0.6.5 the Wrong stat /
   Under-tier / Overcap rows showed `—` with italic "ships v0.6.1"
   captions. Now they should show real counts:
   - **Wrong stat:** count of melds outside the active job's
     priority list.
   - **Under-tier:** count of melds below Tier XII.
   - **Overcap:** count of pieces with 3+ Tier XII on one stat.
   - **Sell / replace:** total of materia recommended to replace,
     with `(show breakdown)` toggle.
5. Click "show breakdown". You should see a list of `Piece · slot N
   · Current materia · Headline · Detail` lines, matching the
   plugin's in-game Materia → Audit tab format.
6. Verify Skill Speed materia (if any) renders as "Quickarm
   Materia XII" — not "Piety Materia XII".

## Pairing

- **GearGoblin.Core v0.6.5** — lockstep version bump only, no
  source changes. Ships separately.
- **GearGoblin plugin v0.6.5** — "Crafted Visible". Critical HQ
  fix. Web's new audit logic relies on receiving the full 13-piece
  gearset from the plugin — without the plugin v0.6.5 HQ-offset
  fix, the web continues to receive 3-of-13 piece exports and has
  almost nothing to audit. Ship plugin v0.6.5 before/with this
  web release.

## Note on the existing audit panel CSS

The new Sell/replace row reuses the existing `.audit-cell` /
`.pip` / `.audit-text` styles defined inline. The expandable
breakdown sits inside the `.audit` grid (which is set to
`grid-column: 1 / -1` to span all columns) and uses inline styles
matching the existing TLF lantern theme (`var(--bg-2)`,
`var(--lantern)`, `var(--lantern-dim)`, `var(--frost)`,
`var(--frost-dim)`, `var(--ship-bright)`, `var(--warning)`). No
CSS file changes — pure Razor.

## Out of scope (v0.6.6+)

- Balance preset (toggle exists; both modes fall through to
  Pure-Math).
- In-game `GG-PLAN:v1:` paste UI (plugin v0.6.6 Plan tab work).
- Stat-cap math, Akhmorning breakpoint formulas (v0.8.x).
