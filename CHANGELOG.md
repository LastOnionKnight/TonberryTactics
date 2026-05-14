# Changelog

All notable changes to Tonberry Tactics are documented here. Format based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), versioning loosely
follows [Semantic Versioning](https://semver.org/).

Tonberry Tactics is the web companion to the in-game plugin (formerly
"GearGoblin", now also "Tonberry Tactics"). From v0.5.5 onward both halves
ship at the same version number.

## [0.6.4] — 2026-05-14  "Vendored Lockstep"

**Headline:** The Materia Advisor finally produces real per-job
recommendations, _and the build actually deploys this time_. v0.6.3
shipped the right code with the wrong delivery mechanism (a
`<ProjectReference>` to a sibling Core repo that doesn't exist in
Cloudflare Pages CI); that deployment failed and was reverted.
v0.6.4 vendors the three Core files (`StatNames`, `MateriaTiers`,
`JobPriorities`) into `Services/Core/` with their original
`GearGoblin.Core` namespace intact. Web builds self-contained;
plugin keeps its real ProjectReference unchanged; both halves agree
on namespaces and signatures.

This restores three-way lockstep (Core / web / plugin all at v0.6.4)
without taking the architectural shortcut that broke CI in v0.6.3.

### Added

- **`Services/Core/StatNames.cs`** — vendored mirror of
  `GearGoblin.Core/StatNames.cs` v0.6.3. Canonicalize FFXIV substat
  names ("Critical Hit" / "CRT" / "Critical" → `CRT`). Used by the
  Materia Advisor for stat-dot styling and stat-totals bucketing.
- **`Services/Core/MateriaTiers.cs`** — vendored mirror of
  `GearGoblin.Core/MateriaTiers.cs` v0.6.3. Tier-to-stat-value lookup
  (Tier XII → +54) and materia-name composition (`Savage Aim Materia XII`).
  Inherits the known Skill Speed prefix stub (`"Piety"` instead of
  `"Quickarm"`) from Core; cosmetic-only, tracked for next sync.
- **`Services/Core/JobPriorities.cs`** — vendored mirror of
  `GearGoblin.Core/JobPriorities.cs` v0.6.3. Per-job materia priority
  tables for all 21 combat jobs (plus BLU). Tank / Healer / Melee DPS
  / Phys Ranged / Mage baselines sourced from public guides at
  thebalanceffxiv.com (patch 7.x Dawntrail tier).
- **`Services/Core/VENDORED.md`** — sync-workflow doc. When Core ships
  a new version, copy these three files over and bump web's version
  in lockstep.

### Changed

- **`Services/PureMathOptimizer.cs` — rewrite.** Now reads its priority
  list from `GearGoblin.Core.JobPriorities.For(jobAbbr)` rather than
  the hardcoded `GnbPriority` array. Materia name + stat value resolve
  via `Core.MateriaTiers.NameOf()` and `Core.MateriaTiers.SubstatValue()`
  so both halves agree on what "Tier XII" means. Mode label now reads
  `"AST priority (via Core)"` on table hit, `"<JOB> fallback (no Core
  table)"` on miss — instead of always-GNB.
- **`Pages/Index.razor` — Materia Advisor mode badge.** The
  apologetic "falling back to GNB priorities (per-job optimizer ships
  v0.6.1 Core)" text is gone. Tabled jobs see "real <JOB> priority
  from thebalanceffxiv.com"; un-tabled jobs see "falling back to GNB
  tank baseline (Core has no table for XYZ)" — honest in the rare
  case rather than the common case.
- **`Pages/Index.razor` — stat name matching.** `MateriaStatClass`
  and `BuildStatCells` now route through a new private
  `CanonicalStatKey` forwarder to `GearGoblin.Core.StatNames.Canonical`,
  so plugin-emitted "Critical Hit" / "Direct Hit Rate" / etc. match
  into the right stat bucket. Pre-v0.6.4 these matched on uppercased
  three-letter codes only, which is why all stat totals showed `+0`
  for any payload that came from the plugin emitter.
- **`Pages/Index.razor` — Meld Audit panel.** The "Per-job priority
  detection ships v0.6.1" placeholder updated to "Per-job priority
  via GearGoblin.Core (v0.6.4)" now that the feature actually exists.
- **`EmitterVersion` and `TtVersion` constants** bumped `0.6.0 → 0.6.4`.
  Version pill and footer copy bumped accordingly.
- **`TonberryTactics.csproj`** — version `0.6.0 → 0.6.4`, Description
  rewritten for "Vendored Lockstep", **no ProjectReference**
  (vendored Core is in-tree).

### Pairing

Ships in lockstep with:

- **GearGoblin.Core v0.6.4** — content unchanged from v0.6.3 in
  practice (these three files are byte-identical to Core's v0.6.3
  vendored copies); version bump exists to keep lockstep with web
  and plugin.
- **GearGoblin plugin v0.6.4** — keeps its real `<ProjectReference>`
  to Core. Plugin's existing `MeldOptimizer` (already job-aware via
  `JobProfile`) continues to work unchanged; consuming Core's priority
  tables from the plugin side is an incremental migration tracked for
  v0.7.x alongside CPR-replacement work.

### Verify after deploy

1. Open https://tonberrytactics.pages.dev. Hard refresh (Ctrl+Shift+R)
   to bust any stale cached chrome.
2. Version pill in the top-right of the layout reads **v 0.6.4**.
   Footer reads `TONBERRY TACTICS · v0.6.4 · CLOUDFLARE PAGES`.
3. Paste your AST export. Materia Advisor badge should read
   "real AST priority from thebalanceffxiv.com" (cyan), not
   "falling back to GNB priorities" (amber).
4. If your AST has empty meld slots, the recommended fills should
   reflect healer priority (Crit → DH → DET → SPS → PIE) rather than
   tank (Crit → DH → DET → SKS → TEN).
5. Stat Profile panel should show non-zero numbers in Crit / DH / DET
   / etc. — previously these were all `+0` for plugin payloads because
   the matching was three-letter-codes-only.

### Notes

- v0.6.3 reverted as commit `86beba2`. v0.6.4 starts from the post-revert
  state and re-applies the v0.6.3 feature surface via the vendoring
  approach.
- v0.7.x will switch from manual vendoring to a git submodule of
  `LastOnionKnight/GearGoblin-Core`. That's the architecturally proper
  fix; this approach is the pragmatic v0.6.x bridge.

---

## [0.6.0] — 2026-05-13  "Gear Division"

**Headline:** Full design port from Claude Design's React prototype.
The site no longer looks like a Blazor app dressed in FF3 paint —
it _is_ the TLF Gear Division landing page, lockstep with the
deployed prototype. Header crest, onion-shield TLF stamp, Evercold
expansion banner, FF dialog-box import section, three-column layout,
gear grid with materia dots, stat profile with cap bars, Materia
Advisor with circled-number rec rows, Meld Audit panel, 21-job
picker, CSS-animated walking Tonberry footer sprite.

Pairs with plugin v0.6.0+. Same version number on both halves from
this release forward (per the v0.5.5 alignment commitment).

### Added

- **`wwwroot/css/design-v060.css`** (34KB) — full TLF design system
  ported verbatim from the prototype's `styles.css`. Multi-palette
  switching scaffold, FF3-style menu boxes, doubled-gold-bevel
  borders, advisor row chrome, stat-cell capacity bars, severity
  pips, Tonberry trail animation. `@font-face` for `Eorzea.ttf` with
  the path adjusted from `assets/Eorzea.ttf` to `../assets/Eorzea.ttf`
  to resolve correctly from Blazor's `wwwroot/css/` layout.

- **New design assets in `wwwroot/assets/`:**
  - `onion-helm.png` (533KB) — adventurer-card portrait
  - `onion-shield.png` (3.9MB) — TLF emblem in the header
  - `onion-knight-ninja.png` (1.5MB) — reserved (not yet rendered;
    earmarked for OC card / Easter egg / settings panel)
  - `rags-pixel.png` (27KB) — walking pixel sprite (footer trail)
  - `rags-pixel-back.png` (40KB) — back-view sprite (reserved)
  - `evercold-logo.png` + `evercold-logo-cropped.png` (1.4MB) —
    expansion logo for the masthead patch-tag
  - `Eorzea.ttf` (27KB) — decorative Eorzean font for the brand block

- **`<HeadContent>` block in Index.razor** — Google Fonts +
  `design-v060.css` link injected into the document head from the
  Razor page, no `wwwroot/index.html` edit required.

### Changed

- **`Pages/Index.razor` — full rewrite.** Replaced 455 lines of inline
  `<style>` and 336 lines of v0.5.x markup with new structure mirroring
  the prototype's component tree:
  - `<header class="masthead">` — crest, brand text, TLF stamp, version
    pill, expansion logo (5 children, grid layout from design-v060.css)
  - `<div class="dlg import-dlg">` — FF dialog-box import section with
    "ADVENTURER" speaker tab and `STEP FORWARD` primary button
  - `<div class="cols">` — three-column responsive layout:
    - Left aside: Adventurer card (portrait, name, stats list, credo)
    - Center main: Gearset, Stat Profile, Materia Advisor, Meld Audit
    - Right aside: Optimizer controls, Export card, Plugin callout,
      Feedback panel (restyled), TLF Manifesto
  - `<div class="tonberry-trail">` — CSS-only walking sprite animation
  - `<div class="foot">` — standing-ready footer

  C# code-behind logic is unchanged from v0.5.5 — `RunOptimization()`,
  `ClearPlan()`, `CopyImportString()`, all v0.5.4 Feedback panel
  methods carry through identically. The optimizer pipeline
  (GearsetParser → PureMathOptimizer → PlanSerializer) is untouched.

- **`EmitterVersion` and `TtVersion` constants** both bumped from
  `"0.5.4"`/`"0.5.1"` to `"0.6.0"`. The emitted `GG-PLAN:v1:` plan
  strings now carry the v0.6.0 generator stamp. Wire format itself
  (the `v1:` prefix) is unchanged.

### Helpers added to `@code`

- **`BuildStatCells()`** — sums per-stat materia totals from
  `ParsedPayload.Equipped[].Materia[]` into 6 `StatCell` records
  (CRT/DH/DET/TEN/SKS/PIE) for the stat profile grid. Cap bars
  render against a 3140 ceiling. Derived rows (chance %, damage %,
  DI per point) are stubbed `—` until v0.6.1 lands the Akhmorning
  formula tables.

- **`SlotGlyph(slot)`** — maps wire-format `Slot` strings
  (MainHand/Head/Body/etc) to FF-style glyph icons (⚔ ⛑ ☗ ✋ ⌷ ⌒
  ◊ ⌑ ◯) for the gear-grid slot icons.

- **`MateriaStatClass(statName)`** — maps `CRT`/`DH`/`DET`/`SKS`/
  `SPS`/`TEN`/`PIE` to the design-v060 CSS classes that color the
  materia dots per stat.

- **`AllJobs[]`** — 21-tuple list of FFXIV combat jobs by abbr +
  role, drives the right-rail job-picker grid. `ActiveJob()` resolves
  to user override → parsed payload → `GNB` default.

- **`JobFullName(abbr)`** — maps job abbreviation to display name
  (`BRD` → `Bard`, etc.) for the Adventurer card.

- **`CircledNum(n)`** — returns `①`–`⑩` Unicode glyphs for advisor
  rank badges.

- **`PipelineTagLabel()` / `PipelineTagClass()`** — translate the
  existing `Portrait` state machine (Ready/Combat/Danger) to the
  design's pipeline state tags (`AWAITING`/`ENGAGED`/`WARNING`).

- **`AuditEmptyCount()` / `AuditPipClass(n)`** — drive the Meld
  Audit panel pip severity colors (`ok`/`warn`/`error`) from
  `Optimization.TotalEmptySlots`.

### Deferred (deliberately, called out so they don't read as broken)

- **Stat profile derived rows** show `—` placeholders. The 6 visual
  frames are there with materia totals + cap bars, but `CHANCE %`,
  `DMG %`, and `DI/pt` per substat need the Akhmorning formula
  tables in C#. Lands v0.6.1.

- **Audit "wrong stat" / "under-tier" / "overcap"** rows show `—`.
  Detection logic lives in the plugin's Audit tab today; the v0.5.0
  Core refactor (planned as `GearGoblin.Core` netstandard library)
  brings full audit parity to the web. Until then, only "empty
  slots" is derived (from `Optimization.TotalEmptySlots`).

- **21-job picker** is clickable and visually responsive (tile
  highlights), but the optimizer is still GNB-only. Picking BRD
  doesn't change recommendations — they remain GNB-priority. Per-job
  priorities also ship with the Core refactor.

- **Balance preset toggle** is selectable in the optimizer mode
  segment; the underlying logic is queued for v0.6.1+. Pure-Math is
  the only active mode.

- **Tonberry Trail animation** is CSS-only (`design-v060.css`
  drives it). The dev-mode Tweaks panel from the prototype is
  intentionally NOT ported — it's a design tool, not a production
  feature.

### Files touched

- `Pages/Index.razor` — full rewrite (1032 lines → ~640 lines, CSS
  externalized)
- `TonberryTactics.csproj` — version 0.5.5 → 0.6.0 + new Description
- `CHANGELOG.md` — this entry
- `wwwroot/css/design-v060.css` — new (34KB)
- `wwwroot/assets/onion-helm.png` — new (533KB)
- `wwwroot/assets/onion-shield.png` — new (3.9MB)
- `wwwroot/assets/onion-knight-ninja.png` — new (1.5MB)
- `wwwroot/assets/rags-pixel.png` — new (27KB)
- `wwwroot/assets/rags-pixel-back.png` — new (40KB)
- `wwwroot/assets/evercold-logo.png` — new (1MB)
- `wwwroot/assets/evercold-logo-cropped.png` — new (1.4MB)
- `wwwroot/assets/Eorzea.ttf` — new (27KB)

### Not changed

- `Models/ExportSchema.cs` — wire-format DTOs untouched. v1 still v1.
- `Services/GearsetParser.cs` — parsing logic preserved verbatim.
- `Services/PureMathOptimizer.cs` — optimizer logic preserved
  verbatim. Still GNB-only; per-job priorities pending v0.5.0 Core.
- `Services/PlanSerializer.cs` — plan emission preserved verbatim,
  now stamps `0.6.0` as the emitter version instead of `0.5.1`.
- `release.ps1` — unchanged.
- `wwwroot/portraits/*.jpg` — kept around even though Index.razor
  no longer references them (the design uses `onion-helm.png` for
  the portrait). Could be cleaned up in a later release if confirmed
  unused.

---

## [0.5.5] — 2026-05-13  "Brand Convergence"

**Headline:** First round of brand assets from Claude Design lands on the
live site. The text-only `▶ TONBERRY TACTICS` header is retired in favor
of the new circle-logo wordmark lockup. Plugin (formerly "GearGoblin")
and web are now branded as one product — same name on both halves of
the round-trip.

The full visual redesign — three-palette FF3 dialog-box theme system,
Refia speech callout, severity-styled audit rows, action bar at bottom
— remains scoped to v0.6.0 and tracked separately. This release is a
brand-only landing pad so the rebrand becomes user-visible without
gating on the full port.

### Added

- **Brand assets** in `wwwroot/assets/`:
  - `circle-logo.png` — new circular wordmark, wired into the header
  - `rags-portrait.png` — full hero portrait of Refia (not yet wired;
    reserved for v0.6.0 hero card)
  - `rags-action.png` — small action portrait (reserved for the
    Knight callout in v0.6.0)
  - `rags-mini.png` — compact avatar (reserved for the left-rail
    sidebar in v0.6.0)

### Changed

- **Header layout** — `.tt-header` now uses a flex row pairing the
  circle-logo (96×96) with the wordmark + tagline stack. CSS
  `image-rendering: pixelated` preserves the pixel-art edge crispness;
  the `drop-shadow` filter pair gives the logo the same gold-on-brown
  glow the wordmark already had.
- **Header version copy** — dropped the stale
  `"v0.5.2 · for GearGoblin v0.4.5+"` line. The plugin is now also
  called Tonberry Tactics, so the cross-product coupling reads as
  circular. Header now reads simply
  `TLF GEAR DIVISION  ·  v0.5.5`.

### Not changed (still scoped for v0.6.0)

- Full CSS palette swap (three-theme system: ff3-blue, ff6-brown,
  ff14-indigo) — staged in `/v060-tt-design-reference/` for reference
- FF3 dialog-box window chrome with doubled-gold-bevel borders
- Left-rail menu structure with `►` cursor selection indicator
- Knight callout (Refia speech bubble with action portrait)
- Severity-styled audit rows (`.sev-crit`, `.sev-warn`, `.sev-ok`)
- Action bar at bottom (Copy Plan / Re-scan Inventory / Export to Plugin)

### Known limitation

- `circle-logo.png` ships at 704KB. Functionally fine; visually fine.
  Production should run it through `pngquant` or similar to reduce by
  60-80%. Defer to v0.6.0 polish pass.

### Wire format

- No change. `GG-EXPORT:v1:` and `GG-PLAN:v1:` prefixes stay — they're
  versioned identifiers, not brand names. Breaking them would break
  round-trip with any beta plugin still in the wild.

### Files touched

- `Pages/Index.razor` — header markup + `.tt-brand*` CSS
- `wwwroot/assets/` — new directory, four PNG assets
- `TonberryTactics.csproj` — `<Version>0.5.5</Version>`, Description
  rewrite

---

## [0.5.4] — 2026-05-13  "Feedback Loop"

**Headline:** Closing the beta-reporting loop. The in-game plugin
(GearGoblin v0.4.7) ships a Feedback tab; this release adds the
matching panel to the web side. Same approach on both halves of the
round-trip: pre-filled GitHub issue URL with diagnostic block
auto-attached, plus a clipboard fallback for users without GitHub.

No backend. No webhooks. No analytics. No telemetry. Nothing leaves
the browser unless the user clicks a button. Aligns naturally with
the "offline · WASM · no backend" principle being explored in the
v0.6.x redesign direction.

### Added

- **Feedback panel** in the right-aside column, sitting below
  EXPORT TO GAME and PLUGIN UPDATE. New `▶ FEEDBACK` menu-box with:
  - Category radio (Bug / glitch, Optimizer disagreement, Confusion
    / unclear, Just saying hi). Drives the GitHub label
    (`bug`/`optimizer`/`ux`/`feedback`) and the issue title prefix.
  - Multiline message field (4000-char cap), bound to
    `FeedbackText` with `oninput` for responsive disable state.
  - "Include diagnostic info" checkbox (on by default). When on,
    auto-attaches a fenced block with: TT version, browser
    user-agent (via `navigator.userAgent` JS eval), whether an
    export is loaded, active job from `ParsedPayload.Character`,
    whether the optimizer has run, whether a plan was emitted,
    and a UTC timestamp.
  - Two action buttons: **OPEN GITHUB ISSUE** constructs a
    `/issues/new?title=...&body=...&labels=...` URL on
    `github.com/LastOnionKnight/TonberryTactics` and opens it in a
    new tab via `window.open`; **COPY FOR DISCORD / DM** puts the
    same markdown payload on the clipboard via the same JS interop
    pattern as `CopyImportString` (`navigator.clipboard.writeText`).
  - Both buttons disabled while the message field is empty.
  - Status line ("✓ Opened …" or "✓ Copied …") appears in a
    ship-green tinted box after a successful action, with an
    HH:mm:ss timestamp.
- **Scoped `.fb-*` CSS classes** inside the existing `.ff-theme-wrapper`
  style block. Reuses the page's Press Start 2P pixel labels and
  the lantern (#f5b95d) accent for radio/checkbox `accent-color`.

### Changed

- `TonberryTactics.csproj` — version bumped 0.5.3 → 0.5.4,
  Description reframed for "Feedback Loop" + GearGoblin v0.4.7
  round-trip compatibility.

### Roadmap captured

- **v0.6.x "TLF Gear Division" redesign** — direction set via a
  Claude Design prototype. Hybrid SNES menu chrome + FFXIV serif
  + Tonberry lantern accents, TLF crest, manifesto block, walking
  Tonberry footer sprite, palette swap (lantern / frost / blood /
  void), optional CRT scanlines. Component boundaries preserved
  from current Blazor structure for clean port:

    | Existing Blazor (Index.razor)  | Target redesign component |
    |--------------------------------|---------------------------|
    | Character header / portrait    | `AdventurerCard`          |
    | 13-slot gear grid              | `Gearset` + `SlotIcon`    |
    | Stat profile                   | `StatProfile`             |
    | Materia advisor                | `MateriaAdvisor`          |
    | Audit counts (v0.5.3+ scan)    | `AuditPanel`              |
    | EXPORT TO GAME menu-box        | `ExportCard`              |
    | PLUGIN UPDATE menu-box         | `PluginCallout`           |
    | Pure-Math / Balance toggles    | `OptimizerControls`       |
    | NEW v0.5.4 FEEDBACK menu-box   | `FeedbackPanel`           |

  Design reference: prototype HTML/CSS/JSX assets archived at
  `v060-tt-design-reference/` (alongside the plugin sandbox).
  Includes TLF design tokens (palette, type, lantern glow, knife
  cursor), the full component sketch, the manifesto/credo copy,
  and a Tweaks panel for in-design palette and intensity controls.
  Desktop-first; tablet/mobile pass deferred.

---

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
