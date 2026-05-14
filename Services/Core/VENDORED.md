# Vendored from GearGoblin.Core

These files are **byte-identical mirrors** of files in the
`LastOnionKnight/GearGoblin-Core` repository, copied here so the web
builds self-contained in Cloudflare Pages CI without needing a sibling
checkout of Core.

## Files

| File | Source | Synced from Core |
|---|---|---|
| `StatNames.cs`     | `GearGoblin.Core/StatNames.cs`     | v0.6.3 |
| `MateriaTiers.cs`  | `GearGoblin.Core/MateriaTiers.cs`  | v0.6.3 |
| `JobPriorities.cs` | `GearGoblin.Core/JobPriorities.cs` | v0.6.3 |

The namespace declared in these files is `GearGoblin.Core` — same as the
real library. Web code refers to `GearGoblin.Core.JobPriorities` etc.
identically whether resolved from a ProjectReference or from these
vendored copies.

## Why vendored, not ProjectReference

v0.6.3 web attempted a `<ProjectReference>` to Core via a filesystem path
(`..\..\GearGoblin-Core-v0.1\GearGoblin.Core\GearGoblin.Core.csproj`).
Cloudflare Pages CI only has the TonberryTactics repo's contents
available during build — it cannot see a sibling directory outside the
repo, so the ProjectReference failed to resolve and the deployment was
abandoned. The revert (`86beba2`) restored a working build at the cost
of losing the per-job priority feature.

v0.6.4 vendors the three small files into this folder. Web builds
self-contained in CI; plugin keeps a real ProjectReference to Core
unchanged. Two halves can't drift on the *meaning* of the types
(namespace + signatures are identical) but the *bytes* need manual
sync if Core changes.

## Sync workflow

When Core ships a new version:

1. `git -C ..\..\GearGoblin-Core-v0.1\GearGoblin.Core pull`
2. Copy these three .cs files from there over the files in this folder.
3. Diff (`git diff Services/Core`) to confirm the changes match Core's
   tagged release notes.
4. Bump web's version in lockstep with Core/plugin and ship.

## v0.7.x plan

Switch to a git submodule of `LastOnionKnight/GearGoblin-Core` at
`external/GearGoblin-Core/` and restore the ProjectReference pointing
into the submodule. Cloudflare clones submodules during build, so CI
resolves correctly. That's the architecturally proper fix; this folder
goes away when that lands.
