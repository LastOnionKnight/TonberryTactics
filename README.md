# Tonberry Tactics

**Current released version: 1.6.1**  
**Current `main`: unreleased 1.6.2 stabilization work**

Tonberry Tactics is the browser-side planning companion to the GearGoblin Dalamud plugin. Together with `GearGoblin.Core`, the three repositories form one FFXIV character optimization platform.

**Live site:** https://tonberrytactics.pages.dev

## Product direction

The long-term target is an **Ask Mr. Robot-style optimizer for FFXIV**: ingest real character/gear state, evaluate candidate sets, recommend melds/food/potions/upgrades, and eventually produce a constrained acquisition plan rather than only a static BiS comparison.

```text
GearGoblin plugin     — live game-state reader + in-game advisor
GearGoblin.Core       — shared formulas / optimizer / schemas
TonberryTactics web   — browser planner / audit / plan export
```

## Current workflow

1. Run `/ttexport` in FFXIV.
2. GearGoblin emits:

```text
GG-EXPORT:v2:<base64-json>
```

3. Paste the payload into the web app.
4. The web normalizes the character/gear data and runs the shared Core optimizer.
5. Copy the generated:

```text
GG-PLAN:v1:<base64-json>
```

6. Run `/ttimport` in FFXIV.

The parser also accepts `GG-EXPORT:v1` for backward compatibility.

## Current optimizer behavior

The retired GNB-only web optimizer is no longer used. `Services/MeldOptimizerAdapter.cs` bridges into `GearGoblin.Core.Materia.MeldOptimizer`.

Current web-side behavior includes:

- all 21 standard combat jobs
- real exported v2 total-stat ingestion into `StatSnapshot`
- per-piece cap context
- empty-slot recommendations
- meld audit / overcap / replacement logic
- real Pure Math vs Balance mode wiring
- job override that affects optimizer profile, audit profile, and emitted plan identity
- shared Tier XII combat projection from Core
- DoH/DoL recognition with battle optimization intentionally disabled

The current `main` no longer feeds a zeroed stat snapshot when real exported totals are available.

## Plan serialization

`PlanSerializer` emits `GG-PLAN:v1` using the supplied/current emitter version. The old hardcoded `1.1.4` emitter value is retired.

When the user runs the optimizer under a job override, the plan's source-job identity is updated consistently so `/ttimport` stores the plan under the selected job rather than the originally exported job.

## Tech stack

- Blazor WebAssembly
- .NET 10
- client-side execution
- Cloudflare Pages deployment
- shared `GearGoblin.Core` submodule at `external/GearGoblin.Core`

No backend is required for the core export → optimize → plan workflow.

## Repository layout

```text
TonberryTactics/
├─ Models/
│  └─ ExportSchema.cs
├─ Services/
│  ├─ GearsetParser.cs
│  ├─ MeldOptimizerAdapter.cs
│  └─ PlanSerializer.cs
├─ Shared/
│  └─ CapGauge.razor
├─ Pages/
│  └─ Index.razor
├─ docs/
├─ external/GearGoblin.Core/
├─ TonberryTactics.csproj
├─ build.sh
├─ CHANGELOG.md
└─ README.md
```

## Build

```powershell
git submodule update --init --recursive
dotnet restore
dotnet build -c Release
```

For local development:

```powershell
dotnet run
```

For static deployment output:

```powershell
dotnet publish -c Release -o output
```

## Versioning

GearGoblin, GearGoblin.Core, and TonberryTactics use **trinity lockstep** for released versions.

Current tagged release:

```text
GearGoblin          1.6.1
GearGoblin.Core     1.6.1
TonberryTactics     1.6.1
```

Current `main` contains stabilization work intended for the next lockstep release and therefore may be ahead of the tagged 1.6.1 state.

## Current known debt

- `GG-PLAN:v1` still carries meld recommendations only.
- the shared optimizer is still a meld/advisor engine rather than a full normalized gearset expected-output solver
- Raider food/potion solving is not implemented yet
- Best-in-Bags / candidate gear enumeration is not implemented yet
- acquisition/currency/weekly-lockout planning is not implemented yet
- DoH/DoL optimization remains display-only
- external target schemas must continue to be validated as Etro/XIVGear evolve

## Next milestone

**v1.7 Solver Foundation**: move from stat-priority/advisor behavior toward a true gearset objective model. First visible feature: Raider Consumables, solved with gear/meld context rather than a hardcoded job-to-item table.

## Companion repositories

- Plugin: https://github.com/LastOnionKnight/GearGoblin
- Core: https://github.com/LastOnionKnight/GearGoblin-Core
- Live site: https://tonberrytactics.pages.dev

## License

See repository/component license files for current terms. Third-party code and assets retain their original licenses.
