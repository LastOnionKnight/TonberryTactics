# Tonberry Tactics

Web companion to [GearGoblin](https://github.com/LastOnionKnight/GearGoblin), the Dalamud BiS planner for Final Fantasy XIV.

Tonberry Tactics is a Blazor WebAssembly app that consumes the `GG-EXPORT:v1:` strings produced by GearGoblin's `/goblinexport` command, runs an in-browser materia optimizer over the parsed gearset, and emits a `GG-PLAN:v1:` round-trip string that the plugin's planned `/goblinimport` will consume back into a native checklist inside the game's Character window.

**Live site:** https://tonberrytactics.pages.dev

> **Status:** v0.5.1 — first real round-trip release. Mock data is gone; an actual GG-EXPORT parser + hardcoded GNB Pure-Math optimizer + GG-PLAN serializer is wired through end-to-end. Multi-job profiles, stat-cap awareness, and the Balance preset toggle are v0.5.2+ work.

## What it does (v0.5.1)

1. You run `/goblinexport` in FFXIV (requires GearGoblin v0.4.1+). A `GG-EXPORT:v1:<base64>` string lands on your clipboard.
2. You paste that string into the IMPORT FIELD DATA box on tonberrytactics.pages.dev.
3. Tonberry Tactics:
   - Parses the string (`Services/GearsetParser.cs`).
   - Displays your real character data — job, level, average item level, equipped piece count — in the ADVENTURER sidebar card.
   - Runs the hardcoded GNB Pure-Math optimizer (`Services/PureMathOptimizer.cs`) across every equipped piece, fills each empty meld slot with a Tier XII materia by rotating through the priority list `[Critical Hit, Direct Hit Rate, Determination]`.
   - Renders the recommendations under MATERIA REQUIRED with the optimization mode label visible so you know what was applied.
   - Serializes the plan to `GG-PLAN:v1:<base64>` (`Services/PlanSerializer.cs`) and stages it in the EXPORT TO GAME card with a one-click COPY button (real `navigator.clipboard.writeText` via IJSRuntime).
4. The portrait icon in the sidebar switches state with the pipeline: **AWAITING** (subdued red ring) → **ENGAGED** (lightning, post-optimize) → **WARNING** (vivid red ring, parse failed).

## Wire format

Both directions are versioned base64-encoded JSON with a literal-string prefix:

```
GG-EXPORT:v1:<base64(JSON)>     // plugin → web
GG-PLAN:v1:<base64(JSON)>       // web → plugin
```

The DTOs in `Models/ExportSchema.cs` match GearGoblin's `Services/GearsetExporter.cs` records verbatim. Schema versions bump in the prefix (`v1:` → `v2:`) so consumers can refuse incompatible payloads cleanly without trying to decode them.

`ExportPayloadV1` carries: `Plugin` name, plugin `Version`, ISO `ExportedAt`, an `ExportCharacterV1` (job ID + abbreviation, level, average item level), and a list of `ExportPieceV1` (slot name, item ID + name + level, HQ flag, guaranteed materia slot count, overmeld permission, list of `ExportMateriaV1` melded). `PlanPayloadV1` mirrors that shape on the way back, with a list of `PlanMeldV1` recommendations.

## Tech stack

- **Blazor WebAssembly** on **.NET 10** — entirely client-side, no backend.
- **Cloudflare Pages** deploy at https://tonberrytactics.pages.dev.
- `build.sh` installs the .NET 10 SDK during Pages' build step, then runs `dotnet publish -c Release -o output`. Pages serves `output/wwwroot/`.
- **VT323** and **Press Start 2P** Google Fonts for the retro FF SNES aesthetic.
- No external JS libraries. Knife cursor in the materia list is inline SVG. Portrait icons are static JPGs in `wwwroot/portraits/`.

## Project layout

```
TonberryTactics/
├─ Models/
│  └─ ExportSchema.cs           Wire-format DTOs (mirror of plugin)
├─ Services/
│  ├─ GearsetParser.cs          GG-EXPORT:v1: → ExportPayloadV1
│  ├─ PureMathOptimizer.cs      Hardcoded GNB Pure-Math
│  └─ PlanSerializer.cs         OptimizationResult → GG-PLAN:v1:
├─ Pages/
│  └─ Index.razor               Single-page app, all UI + state
├─ wwwroot/
│  └─ portraits/                Refia portrait icons (state-switched)
│     ├─ portrait_danger.jpg
│     ├─ portrait_danger_alt1.jpg
│     ├─ portrait_combat.jpg
│     └─ portrait_combat_alt1.jpg
├─ TonberryTactics.csproj
├─ CHANGELOG.md
├─ README.md                    (this file)
└─ release.ps1                  Unified push script (same as GearGoblin)
```

## Build locally

```
dotnet restore
dotnet run                      # serves on http://localhost:5000 by default
```

Or to produce the artifacts that Cloudflare Pages serves:

```
dotnet publish -c Release -o output
# output/wwwroot/ is the static site
```

## Release

`release.ps1` is identical to the one in the GearGoblin repo. From the project root:

```powershell
.\release.ps1 -DryRun           # preview the commit message
.\release.ps1                   # commit, tag vX.Y.Z, push to origin/main
```

The script auto-detects the project from the `.csproj` file in the CWD, reads `<Version>` from it, generates the commit message from the matching CHANGELOG entry, tags `vX.Y.Z`, and pushes with `--follow-tags`. Cloudflare Pages picks up the push and rebuilds automatically.

Flags: `-DryRun`, `-Message "override"`, `-SkipPush`.

## Roadmap

**v0.5.2 (planned):**
- Multi-job stat profiles for all 21 combat jobs, replacing the hardcoded GNB priority.
- Stat-cap awareness — refuse to recommend a materia that would push a stat past its cap. Currently the optimizer is greedy and may over-recommend.
- Balance preset weighting alongside Pure Math, with a UI toggle.
- Audit pass: surface wrong-stat / overcap / outdated-tier existing melds, not just empty slots.

**v0.6.x (longer-term):**
- Extract `GearGoblin.Core.dll` as a shared assembly compiling for both .NET and WebAssembly. Retires the duplicated `PureMathOptimizer` here in favor of the plugin's real `MeldOptimizer`. The web app's parser/serializer/UI layers stay; only the math core is replaced.
- Overmeld recommendations with success-probability math.
- Shareable plan URLs (encode the export+plan in the URL fragment so plans can be linked in Discord).

## Why offload to a web app?

- Optimization runs on phones and tablets without launching the game.
- Decouples the combinatorial search from FFXIV's frame budget.
- Plans become shareable when the URL-encoded variant ships.
- Browser-side updates ship instantly without a plugin reinstall.

## Companion plugin

Get GearGoblin at https://github.com/LastOnionKnight/GearGoblin. Tonberry Tactics requires GearGoblin **v0.4.1 or later** for the `/goblinexport` command. GearGoblin v0.5.0+ will add `/goblinimport` to consume the `GG-PLAN:v1:` strings this app produces.

## Credits

- **GearGoblin** and the underlying materia / stat-formula work — see the [GearGoblin README](https://github.com/LastOnionKnight/GearGoblin/blob/main/README.md) for the full credits stack (CharacterPanelRefined, Akhmorning Allagan Studies, The Balance Discord).
- Portrait artwork: original character designs of Refia Rakkiri, by the project author.
- TLF aesthetic: Tonberry Liberation Front. *No gear. No hope. No pants. Just onions.*

## License

TBD — currently unreleased software, mirroring GearGoblin's license stance. To be added prior to v1.0.0.
