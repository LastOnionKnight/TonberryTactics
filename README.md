# Tonberry Tactics

**Current version: 1.6.1**

Tonberry Tactics is the web companion to the GearGoblin Dalamud plugin. Together with `GearGoblin.Core`, the three repositories form one lockstep gearing and materia-planning system for Final Fantasy XIV.

**Live site:** https://tonberrytactics.pages.dev

## Ecosystem

```text
GearGoblin plugin     — in-game character/gear reader, planner, importer/exporter
GearGoblin.Core       — shared optimizer, formulas, job profiles, schema types
TonberryTactics web   — browser-side audit/optimization and plan export
```

All three are intended to ship at the same product version. Current lockstep release: **1.6.1**.

## Current workflow

1. In FFXIV, run:

```text
/ttexport
```

2. GearGoblin copies a versioned gear export to the clipboard. The current producer emits:

```text
GG-EXPORT:v2:<base64-json>
```

3. Paste that string into the Tonberry Tactics web app.
4. The web app parses the character and equipped gear, displays the current stat/gear state, and runs the shared `GearGoblin.Core` meld optimizer.
5. Copy the generated plan:

```text
GG-PLAN:v1:<base64-json>
```

6. Back in FFXIV, run:

```text
/ttimport
```

GearGoblin imports and persists the plan and surfaces it in the plugin UI.

The web parser remains backward-compatible with `GG-EXPORT:v1:` as well as the current v2 export.

## What the web app does today

### Gear import

`Services/GearsetParser.cs` accepts both export schema generations currently in circulation:

- `GG-EXPORT:v1:`
- `GG-EXPORT:v2:`

v1 payloads are adapted into the current v2 model so the rest of the application can operate on one normalized shape.

### Shared optimizer

The old hardcoded GNB-only `PureMathOptimizer` is retired.

Current optimization routes through `GearGoblin.Core.Materia.MeldOptimizer` using the same shared logic as the in-game plugin. This prevents the web and plugin from recommending different materia for the same gearset.

Current shared optimizer capabilities include:

- all 21 standard combat jobs
- job-aware relevant stats and weighting
- per-piece substat caps
- empty-slot recommendations
- overcap / zero-value / replacement auditing
- current endgame materia tiers
- Pure Math and Balance-weight infrastructure
- DoH/DoL identification and display-only handling

### Character and stat display

Current v2 exports include total stat data and per-piece cap/base-substat context. The web UI uses that to display the imported character, equipped pieces, materia state, and stat-profile information.

### Plan serialization

`Services/PlanSerializer.cs` serializes optimizer output to `GG-PLAN:v1:` for `/ttimport` in the plugin.

The current plan schema carries meld recommendations. Future schema revisions are expected as Raider-specific plan data such as consumables is added.

## Tech stack

- Blazor WebAssembly
- .NET 10
- entirely client-side application
- Cloudflare Pages deployment
- shared `GearGoblin.Core` git submodule

No backend is required for the core export → optimize → plan workflow.

## Repository layout

```text
TonberryTactics/
├─ Models/
│  └─ ExportSchema.cs            web-facing schema/adaptation types
├─ Services/
│  ├─ GearsetParser.cs           GG-EXPORT v1/v2 parser
│  ├─ MeldOptimizerAdapter.cs    bridge into GearGoblin.Core
│  └─ PlanSerializer.cs          GG-PLAN:v1 producer
├─ Shared/
│  └─ CapGauge.razor
├─ Pages/
│  └─ Index.razor                main application UI/state
├─ Resources/
├─ Design-Reference/
├─ docs/
├─ external/GearGoblin.Core/     shared Core git submodule
├─ TonberryTactics.csproj
├─ build.sh
├─ CHANGELOG.md
└─ README.md
```

## Core submodule

The web app consumes Core from:

```text
external/GearGoblin.Core/
```

Fresh clone setup:

```powershell
git submodule update --init --recursive
```

The project reference is:

```xml
<ProjectReference Include="external\GearGoblin.Core\GearGoblin.Core.csproj" />
```

## Build locally

```powershell
git submodule update --init --recursive
dotnet restore
dotnet build -c Release
```

For local development:

```powershell
dotnet run
```

For Cloudflare/static deployment output:

```powershell
dotnet publish -c Release -o output
```

Cloudflare Pages serves `output/wwwroot/`.

## Versioning and releases

Tonberry Tactics follows **trinity lockstep** with the plugin and Core:

```text
GearGoblin          1.6.1
GearGoblin.Core     1.6.1
TonberryTactics     1.6.1
```

The repository retains `release.ps1` and the Cloudflare Pages build flow. Version changes should be coordinated across all three repositories unless a divergence is explicitly intentional.

## Current known debt

- Some user-facing in-page copy still carries older version-era wording and should be normalized as UI polish continues.
- `GG-PLAN:v1` only carries meld recommendations today.
- Plan serialization must remain version-accurate rather than advertising an old emitter version.
- DoH/DoL optimization remains display-only until crafting/gathering-specific formulas are implemented.
- Raider mode needs first-class support across the system rather than remaining primarily a plugin plan concept.

## Next planned feature: Raider consumables

The next major planning feature is a Raider food/potion advisor shared with the in-game plugin.

The intended model is:

- plugin enumerates current FFXIV food/medicine through live Lumina data
- real player stats determine actual capped HQ food gains
- Core owns reusable consumable scoring rules where possible
- the correct offensive main-stat potion is selected for the current job
- loaded BiS data can override the calculated recommendation when the source explicitly specifies consumables
- a future plan schema revision carries consumable recommendations between web and plugin

This is deliberately data-driven so new food and potion tiers do not require maintaining a hardcoded per-job item-name table.

## Companion repositories

- Plugin: https://github.com/LastOnionKnight/GearGoblin
- Core: https://github.com/LastOnionKnight/GearGoblin-Core
- Live site: https://tonberrytactics.pages.dev

## Credits

Tonberry Tactics is part of the LastOnionKnight / Refia Rakkiri project.

Shared combat-stat/materia behavior is implemented in GearGoblin.Core; see the plugin/Core repositories for additional formula and third-party attribution notes.

## License

See the repository and component license files for the current licensing state. Third-party code and assets retain their original license terms.
