# Tonberry Tactics v0.6.4 dropin — "Vendored Lockstep"

**Headline:** Restores the per-job materia priority feature that
v0.6.3 tried and reverted, without using a ProjectReference that
Cloudflare Pages CI can't resolve. Three Core files vendored into
`Services/Core/` so the web builds self-contained in CI.

## What's in this dropin

```
Services/Core/StatNames.cs        new — vendored from Core v0.6.3
Services/Core/MateriaTiers.cs     new — vendored from Core v0.6.3
Services/Core/JobPriorities.cs    new — vendored from Core v0.6.3
Services/Core/VENDORED.md         new — sync-workflow doc
Services/PureMathOptimizer.cs     overwrite — consumes Core types
Pages/Index.razor                 overwrite — canonical stat matching + advisor badge text + v0.6.4 chrome
TonberryTactics.csproj            overwrite — v0.6.4, NO ProjectReference
CHANGELOG.md                      overwrite — v0.6.4 entry on top
```

## Why this approach

v0.6.3 attempted a `<ProjectReference>` to a sibling `GearGoblin.Core`
repo. Cloudflare Pages' build environment only has the TonberryTactics
repo's contents available — it can't see a sibling directory outside
the repo. v0.6.3 was reverted (`86beba2`) and the web has been at
v0.6.0-equivalent ever since.

v0.6.4 vendors the three Core files into `Services/Core/`. The
namespace stays `GearGoblin.Core` so all the `using GearGoblin.Core;`
and `GearGoblin.Core.X.Y` references compile identically whether
the types come from a ProjectReference or from this vendored copy.

Plugin's own `<ProjectReference>` to Core is **unchanged**. The plugin
still consumes Core via the normal sibling-repo path. The two halves
agree on the namespace + signatures; the bytes are mirrored by hand
until the v0.7.x submodule migration.

## Build & deploy

```
cd D:\TonberryTactics-workspace\TonberryTactics
dotnet build -c Release
.\release.ps1 -DryRun
.\release.ps1
```

Cloudflare Pages auto-deploys on push to main. The build should
succeed this time because the project no longer references anything
outside the repo.

## Smoke test (after Cloudflare deploys)

1. Open `https://tonberrytactics.pages.dev`. Hard-refresh `Ctrl+Shift+R`.
2. Version pill in the top-right reads **v 0.6.4**. Footer reads
   `TONBERRY TACTICS · v0.6.4 · CLOUDFLARE PAGES`.
3. Paste an AST plugin export into the STEP FORWARD box.
4. Materia Advisor mode badge reads
   `· real AST priority from thebalanceffxiv.com` (cyan), NOT
   `· falling back to GNB priorities` (amber).
5. Recommended fills for empty slots follow healer priority
   (Crit → DH → DET → SPS → PIE), not tank.
6. Stat Profile shows non-zero numbers for stats your character has
   melded. Pre-v0.6.4 these were all `+0` for plugin payloads.
7. Try a non-tabled job (none exist yet — every job has a Core entry —
   but if you fake a payload with `"jobAbbreviation": "XYZ"`, the
   badge should read "falling back to GNB tank baseline (Core has
   no table for XYZ)").

## Lockstep

This ships alongside:

- **GearGoblin.Core v0.6.4** — content unchanged from v0.6.3 in
  practice; version bump is for lockstep alignment.
- **GearGoblin plugin v0.6.4** — keeps real ProjectReference to Core.

All three projects align on v0.6.4 after this release.

## v0.7.x roadmap

Switch from manual vendoring to a git submodule of
`LastOnionKnight/GearGoblin-Core`. Cloudflare clones submodules
during build, so the ProjectReference can resolve through the
submodule path. This folder (`Services/Core/`) goes away when that
lands.
