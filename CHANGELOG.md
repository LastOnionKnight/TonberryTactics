# Changelog

All notable changes to Tonberry Tactics are documented here. Format based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), versioning loosely
follows [Semantic Versioning](https://semver.org/).

Tonberry Tactics is the web companion to GearGoblin
(https://github.com/LastOnionKnight/GearGoblin). Both projects ship together
when wire-format changes cross the boundary.

## [0.5.3] — 2026-05-12

Hotfix release. v0.5.2 deployed to Cloudflare Pages but the live site hung
on the Blazor "Loading" spinner because `dotnet publish` was injecting
unresolved fingerprint templates (`#[.{fingerprint}]`) into the script
tags in `index.html`. .NET 10's Blazor SDK enables static-asset
fingerprinting by default, and resolving the templates requires the
`wasm-tools` workload — which Cloudflare's build environment doesn't
pre-install. The build log itself flagged this:

> Publishing without optimizations. Although it's optional for Blazor,
> we strongly recommend using `wasm-tools` workload!

v0.5.1 worked because Cloudflare's build env was running an earlier .NET
SDK that didn't apply fingerprinting; an SDK upgrade between v0.5.1 and
v0.5.2 changed the behavior.

### Fixed

- **Blazor WASM startup**: site now loads past the spinner. Two
  complementary fixes:
  - `TonberryTactics.csproj` — added
    `<WasmFingerprintAssets>false</WasmFingerprintAssets>`. Tells the
    .NET 10 Blazor SDK to skip fingerprint template injection at publish
    time. Source `index.html` tags pass through unchanged.
  - `build.sh` — added `dotnet workload install wasm-tools` before
    `dotnet publish`. Belt-and-suspenders backup: if the csproj property
    doesn't fully suppress templates for any reason, wasm-tools resolves
    them to real hashes so the site still ships.

### Notes

- **What we lose**: automatic cache-busting on framework asset hash
  changes. For a personal beta with one user this is theoretical; we can
  re-enable fingerprinting in v0.6.x when the site has actual users to
  worry about cache invalidation for.
- **What we keep**: everything else. No content changes, no optimizer
  changes, no wire-format changes. Wallclock from v0.5.2 → v0.5.3 is
  ~30 minutes plus one Cloudflare rebuild.

### v0.4.6 retro additions

- ".NET 10 SDK enables WasmFingerprintAssets by default, incompatible with Cloudflare Pages build env without wasm-tools workload."
- "Cloudflare's build environment auto-upgrades .NET SDK versions over time — a build that worked yesterday may fail today on the same source if Microsoft published a new SDK channel patch in between."

[0.5.3]: https://github.com/LastOnionKnight/TonberryTactics/releases/tag/v0.5.3

## [0.5.2] — 2026-05-12

Docs/version alignment release for **GearGoblin v0.4.5**, which became a
full CharacterPanelRefined replacement. No optimizer, parser, or
wire-format changes — `GG-EXPORT:v1:` is still consumed unchanged, the
PureMathOptimizer is bit-for-bit identical to v0.5.1. Site content
updated to reflect the plugin's expanded role.

### Added

- **Plugin Update sidebar callout** on the landing page (right column,
  below "Export to Game"). Single amber-bordered box announcing GG
  v0.4.5: full Character Panel takeover, compact derived stats per
  substat, breakpoint hints, real GCD, role-gated Tenacity / Piety,
  Materia Advisor, CPR coexistence. Frames Tonberry Tactics as the
  web-side optimizer that pairs with the plugin's panel takeover.
- **Compat tag on the header version pill**: `TLF GEAR DIVISION · v0.5.2
  · for GearGoblin v0.4.5+`. Lets visitors immediately see plugin
  compatibility at a glance.

### Changed

- **`TonberryTactics.csproj` description** updated to mention GG v0.4.5
  compatibility and the CPR-replacement positioning.
- **`README.md`** rewritten in the "What this does" section to lead with
  GearGoblin v0.4.5's panel takeover and TT's role as its web-side
  companion. Compat notes added. Feature roadmap unchanged.

### Notes

- **Wire format unchanged.** GG-EXPORT:v1: produced by GearGoblin v0.4.5
  is byte-for-byte the same as v0.4.2's — schema versioning earns its
  keep here. No code path in `Services/GearsetParser.cs` or
  `Services/PureMathOptimizer.cs` was touched.
- **Inline `(v0.5.1)` code comments preserved.** Those annotate when
  individual code sections were written. They're accurate as-is; only
  the file-header banner and the visible version pill were bumped.
- **Next code-level update** is queued for v0.5.3 if the Tier XIII
  materia drop changes recommendation rankings, or v0.6.0 when shared
  `GearGoblin.Core.dll` lands.

[0.5.2]: https://github.com/LastOnionKnight/TonberryTactics/releases/tag/v0.5.2

## [0.5.1] — 2026-05-11

First real round-trip release. The v0.5.0 mock optimizer is gone, replaced
by an actual `GG-EXPORT:v1:` parser, a hardcoded Gunbreaker Pure-Math
optimizer, and a `GG-PLAN:v1:` serializer that produces a string consumable
by GearGoblin's future `/goblinimport` command.

### Added

- **`Models/ExportSchema.cs`.** Wire-format DTO records matching the schema
  emitted by GearGoblin v0.4.1+'s `/goblinexport` command verbatim
  (`ExportPayloadV1`, `ExportCharacterV1`, `ExportPieceV1`,
  `ExportMateriaV1`). Plus the matching `PlanPayloadV1` and `PlanMeldV1`
  records for the round-trip back into the plugin.
- **`Services/GearsetParser.cs`.** Strips the `GG-EXPORT:v1:` prefix,
  base64-decodes the payload, deserializes JSON with `JsonNamingPolicy.CamelCase`
  to match the plugin's emit format, and returns a `ParseResult` record
  carrying either the typed payload or a diagnostic error string. Distinct
  error messages for empty input, wrong prefix, malformed base64, malformed
  JSON, version mismatch, and structurally-empty payload — so the UI can
  show the user exactly what's wrong with their paste.
- **`Services/PureMathOptimizer.cs`.** Hardcoded Gunbreaker stat-priority
  optimizer. Iterates each piece's empty meld slots and assigns Tier XII
  materia by rotating through the GNB priority list (Crit, Direct Hit,
  Determination). Returns a `Result` carrying the list of `Recommendation`
  records plus a job-aware mode label so the UI can show what was applied.
  Non-GNB jobs get a "fallback" label since v0.5.1 doesn't have multi-job
  profiles yet. Doesn't audit existing melds, doesn't handle overmelds,
  doesn't enforce stat caps — that's all v0.5.2+ work.
- **`Services/PlanSerializer.cs`.** Symmetric counterpart to the parser.
  Builds a `PlanPayloadV1` from an optimization result plus the source
  character snapshot, JSON-serializes with camelCase, base64-encodes,
  prefixes `GG-PLAN:v1:`. Output is a single round-trip string suitable
  for clipboard.
- **Adventurer portrait.** A 400×400 portrait icon (Refia Rakkiri side
  profile in Onion Knight helm) now lives at the top of the ADVENTURER
  sidebar card. State machine switches between three variants based on
  pipeline state: `portrait_danger.jpg` (READY/awaiting input — subdued
  red ring), `portrait_combat.jpg` (post-optimize — lightning, engaged),
  `portrait_danger_alt1.jpg` (parse error — vivid red, alert). State tag
  label inside the frame reads "AWAITING" / "ENGAGED" / "WARNING".
- **Real character data in the Adventurer card.** JOB, LEVEL, and ILVL
  rows now display values pulled from the parsed payload instead of the
  hardcoded "WAR" placeholder. MELDS row shows the actual recommendation
  count. STATUS row reflects the real pipeline state.
- **Parse error banner.** Bright red-on-dark error box renders above the
  content grid whenever `GearsetParser` returns a failure, carrying the
  diagnostic message verbatim. Auto-clears on next OPTIMIZE attempt or
  manual CLEAR.
- **Real clipboard copy.** The COPY button on the EXPORT TO GAME card now
  invokes `navigator.clipboard.writeText` via `IJSRuntime`. v0.5.0's
  no-op stub is gone. Browsers that block the clipboard API silently
  no-op; the readonly textarea is still selectable for manual copy as a
  fallback.

### Changed

- **`Pages/Index.razor`.** The mock `MockPlan` / `MockRec` / `MockMateria`
  classes and the hardcoded sample-data optimizer are removed. The page
  now imports `TonberryTactics.Models` and `TonberryTactics.Services`,
  holds `ExportPayloadV1?`, `PureMathOptimizer.Result?`, and `string?
  ParseError` as state, and threads the real parser → optimizer →
  serializer pipeline through `RunOptimization`. Markup uses
  `Optimization.Recommendations` and `Optimization.OptimizationMode`
  instead of the mock shape.
- **Materia Required panel.** Above the list, a muted info line now shows
  the optimization mode label and the number of empty slots filled. Helps
  users understand whether they're getting GNB Pure-Math output or the
  multi-job-fallback placeholder.
- **Hero instructions.** The hint now says `/goblinexport` (the real
  command added in GearGoblin v0.4.1) instead of `/goblin export`.
- **Footer version label.** v0.5.0 → v0.5.1.

### Fixed

- N/A — first real round-trip, no regressions to fix yet.

### Known limitations

- Single job profile (Gunbreaker). Other jobs surface in the UI but get
  GNB priorities applied; the result is wrong for any non-tank.
  Multi-job profiles arrive in v0.5.2.
- No stat-cap awareness. The optimizer happily recommends materia that
  would push a stat past its cap. The plugin's MeldOptimizer does this
  right; we'll inherit that logic once `GearGoblin.Core.dll` is extracted
  as a shared assembly.
- No Balance-vs-Pure-Math toggle yet. Hardcoded to Pure Math. The
  Balance preset weighting goes in alongside multi-job profiles in v0.5.2.
- Overmeld recommendations are not produced. We only fill guaranteed
  slots. Overmelds need probability-of-success math that's not yet
  ported to the web side.

## [0.5.0] — 2026-05-11

### Added

- Initial public deploy at https://tonberrytactics.pages.dev via
  Cloudflare Pages. Blazor WebAssembly app, .NET 10 SDK, builds via
  `build.sh` on each push to `main`.
- Retro Final Fantasy SNES/PS1 UI: VT323 and Press Start 2P fonts, navy
  gradient background (#060a28 → #14184c), gold/green TLF accents,
  Materia recommendations rendered as a list with an animated knife
  cursor (inline SVG, no external image asset required).
- Mock optimizer producing a hardcoded WAR/Maiming gear plan to validate
  the round-trip clipboard flow end-to-end before real optimization
  logic shipped.
- TLF branding throughout: tagline, footer mark, color palette.

[0.5.1]: https://github.com/LastOnionKnight/TonberryTactics/releases/tag/v0.5.1
[0.5.0]: https://github.com/LastOnionKnight/TonberryTactics/releases/tag/v0.5.0
