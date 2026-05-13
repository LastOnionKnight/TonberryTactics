# Design v0.6.0 — Port Plan to Blazor

Status: **STAGED, NOT YET WIRED**. Assets and stylesheet are in the repo and
will deploy via Cloudflare Pages on the next push, but the live site won't
change visually until `Pages/Index.razor` is refactored to use the new layout.
That refactor is the v0.6.0 release scope.

This document is the porting checklist. Each row maps a JSX component in
`Design-Reference/` to the Blazor work needed to recreate it.

---

## What's already in the repo as of this staging

| Path | Purpose |
|---|---|
| `wwwroot/assets/circle-logo.png` | Brandmark for the title bar (`gg-brandmark`) |
| `wwwroot/assets/rags-portrait.png` | Hero portrait inside `gg-hero-knight` |
| `wwwroot/assets/rags-mini.png` | Rail-corner avatar inside `gg-mini-knight` |
| `wwwroot/assets/rags-action.png` | Speaker portrait inside `gg-knight-callout` |
| `wwwroot/css/design-v060.css` | Full v0.6.0 stylesheet — palette tokens, window chrome, rail, content, sections, audit table, footer actions |
| `Design-Reference/demo.html` | Open this locally to see the rendered design target |
| `Design-Reference/materia-advisor.jsx` | Canonical implementation of the hero screen |
| `Design-Reference/onion-knight.jsx` | Pixel-sprite Onion Knight + slot-icon SVG helpers |
| `Design-Reference/tweaks-panel.jsx` | Dev panel for palette swap testing (ff3-blue / ff6-brown / ff14-indigo) |

To preview the target locally before porting: open `Design-Reference/demo.html`
in a browser. The relative paths point at `../wwwroot/assets/*` after staging,
so the portraits and logo will load correctly.

---

## What `design-v060.css` introduces

### Three-palette system
- `[data-palette="ff3-blue"]` (default) — deep cobalt, gold borders, cool white text
- `[data-palette="ff6-brown"]` — warm brown, vellum, FF6 menu vibe
- `[data-palette="ff14-indigo"]` — light vellum bg, indigo title bar, FFXIV print-style

Set on `<body>` via attribute. Persists via local-storage in the JSX prototype;
Blazor port should do the same (probably via JS interop on the `<body>` element).

### Atmospheric layers
- CRT scanlines: `[data-scanlines="on"]` adds repeating linear-gradient overlay
- Mascot toggle: `[data-show-knight="off"]` hides the hero portrait, collapses callout

### Window chrome (`gg-window`)
- Four corner ornaments (`gg-corner.tl/tr/bl/br`) — small gold pixel squares
- Title bar (`gg-titlebar`) with brandmark + wordmark + version + three pixel buttons
- Body split: rail left, content right (CSS Grid, 240px / 1fr)

### Hero screen head (`gg-screen-head`)
- Three-column grid: titles / hero knight / status chips
- Eyebrow (`▼ CHAPTER IV · MATERIA ADVISOR`)
- H1 with ornament (`◆ The Knight Reports`)
- Archaic subtitle in Refia's voice

### Knight callout (`gg-knight-callout`)
- Action portrait left, speech bubble right
- Speaker line (`▸ REFIA, TACTICIAN`)
- Speech in TLF voice with `<span class="accent">` highlights

### Section headers (`gg-section-head`)
- H2 with pixel-bracket glyph
- Meta text right-aligned

### Recommendation rows (`gg-rec`)
- 5-column grid: rank / slot / swap / gain / apply
- Rank in Roman numerals (I / II / III)
- Slot icon (`SlotIcon` from `onion-knight.jsx`, draws an 8×8 pixel sprite as SVG rects)
- Swap shows `from` → arrow → `to` with materia name + stat detail
- Gain score in big numerals with label below
- "APPLY ▶" pixel button

### Audit rows (`gg-audit-row`)
- Severity-coded glyphs: `!` crit (red), `?` warn (yellow), `✓` ok (green)
- 5-column grid: severity / slot / reason / gain / verdict
- Verdict pills: SWAP / REROLL / UPGRADE / KEEP

### Footer actions (`gg-actions`)
- Three pixel buttons, primary highlighted gold
- COPY PLAN / RE-SCAN INVENTORY / EXPORT TO TONBERRY TACTICS

---

## Blazor port checklist

Roughly six discrete steps. Estimated total: **6–8 hours**, well-suited to one
focused session or two paired short ones.

### Step 1 — Wire the stylesheet (~15 min)

Add to `Pages/Index.razor` head section:

```razor
<link rel="stylesheet" href="css/design-v060.css" />
```

Decide whether to keep the existing scoped `<style>` block (for backward-compat
during transition) or remove it entirely. Recommended: **remove entirely** at
v0.6.0 time and commit to the new design; partial transition is its own bug surface.

### Step 2 — Restructure top-level layout (~1 hr)

Replace the current `.ff-theme-wrapper` → `.tt-header` → `.content-grid` shape
with the design's nested structure:

```html
<body data-palette="ff3-blue" data-scanlines="on" data-show-knight="on">
  <div class="gg-window">
    <span class="gg-corner tl"></span><span class="gg-corner tr"></span>
    <span class="gg-corner bl"></span><span class="gg-corner br"></span>
    <div class="gg-titlebar">...</div>
    <div class="gg-body">
      <aside class="gg-rail">...</aside>
      <main class="gg-content">...</main>
    </div>
  </div>
</body>
```

The `<body>` data-attributes can be set via JS interop on initial render.
Cleanest: a single `OnAfterRenderAsync` call that reads from local storage and
applies stored palette/scanlines/showKnight settings.

### Step 3 — Title bar + rail (~1 hr)

- **Title bar**: brandmark `<img src="assets/circle-logo.png">` + wordmark + version string + three pixel buttons (pin / settings / close — render as static for now, wire later)
- **Rail header**: mini portrait `<img src="assets/rags-mini.png">` + REFIA name + job line
- **Rail nav**: foreach over a `TabModel[]` collection with `►` cursor, label, optional badge
- **Rail footer**: keyboard hint block (`↑↓ to choose`, `⏎ to confirm`)

The rail's tab selection state is local Blazor state. Currently the live site
doesn't have a tab system at all — adding one is a meaningful structural shift.
For v0.6.0 it's fine to ship with only "Materia Advisor" tab functional; other
tabs render placeholder ("Coming in v0.6.1") with the same chrome.

### Step 4 — Hero screen head (~45 min)

Three-column grid with:
- Titles column: eyebrow + h1-with-ornament + subtitle
- Hero portrait column: `<img src="assets/rags-portrait.png">` + label
- Status chips column: three `StatusChip` components showing CRIT/WARN/OK counts

The chip counts are derived from the existing audit results. For Blazor:

```csharp
var critCount = auditResults.Count(a => a.Severity == AuditSeverity.Critical);
var warnCount = auditResults.Count(a => a.Severity == AuditSeverity.Warning);
var okCount   = auditResults.Count(a => a.Severity == AuditSeverity.Ok);
```

If `AuditSeverity` doesn't exist yet in the model, add it. Currently the
optimizer emits a flat list; we need severity classification for the chips +
audit rows below.

### Step 5 — Knight callout + Recommended Melds + Audit (~2-3 hrs)

This is the bulk of the visible content port:

- **Knight callout**: action portrait + speaker line + speech. Speech text can
  be static templated ("Thy gear is sound, friend — but {N} slots yet protest...")
  with `{N}` filled from rec count.
- **Recommended Melds section**: foreach over top-3 recommendations from
  `PureMathOptimizer.Recommend(...)`. Render each as a `gg-rec` row with rank
  (Roman numerals via a helper), `SlotIcon` (inline SVG matching the JSX
  prototype's pixel grids), swap, gain, APPLY button.
- **Meld Audit section**: foreach over the same recommendations grouped by
  severity. Currently the optimizer doesn't distinguish "minor issue" vs
  "wrong stat" vs "overcap" — that classification logic needs to be added.
  This dovetails cleanly with the plugin's **overmeld-cap detection** work
  scoped for v0.4.8 — same severity model on both halves.

`SlotIcon` as inline SVG: take the pixel grids from `onion-knight.jsx` (earring,
necklace, bracelet, ring, weapon) and emit `<rect>` elements at scaled
coordinates. Blazor equivalent of the React `PixelSprite` component is a
simple `@code` block + Razor foreach.

### Step 6 — Footer actions (~30 min)

Three pixel buttons:
- **COPY PLAN** → calls the existing `BuildGgPlan(...)` and writes to clipboard
- **RE-SCAN INVENTORY** → currently no equivalent in the live site; can be
  wired to clear local state and prompt for re-paste, or shipped as a stub
  pending plugin-side handshake feature
- **EXPORT TO TONBERRY TACTICS** → mislabeled in the JSX prototype (it's the
  reverse direction); should be **"EXPORT TO GAME"** in the Blazor port. The
  paste-back-into-game flow.

---

## Things deliberately NOT scoped in v0.6.0

- **Tweaks panel** (palette swap UI): the prototype includes `tweaks-panel.jsx`
  for dev-time palette/atmosphere/mascot toggles. **Defer to v0.6.1 or later**
  as a user-facing setting, or drop entirely. The three palettes can be
  hard-coded to ff3-blue at v0.6.0 ship; user theme toggle is its own feature.
- **CRT scanlines toggle**: same — default on, no UI to toggle yet.
- **Title bar window buttons** (pin / settings / close): visual only at v0.6.0;
  wire interactions in v0.6.x.
- **Multi-tab content**: only the Materia Advisor tab fully implemented at
  v0.6.0. Other tabs (Current Gear, Stat Sheet, BiS Plan, About) render
  placeholders.
- **Pixel-SVG OnionKnight sprite**: the prototype includes a vector-pixel
  Onion Knight as a fallback for when the PNG portraits aren't loaded. The
  PNG portraits are the primary art; the SVG sprite is a backup. Skip the
  SVG sprite at v0.6.0 — if a portrait fails to load, show a gold rect placeholder.

---

## Coordination with plugin v0.4.8

The Audit section's severity classification (crit / warn / ok) is the same
model the plugin's overmeld-cap fix needs. Build the severity enum in the
shared model space first:

```csharp
public enum AuditSeverity { Critical, Warning, Ok }

public record AuditRow(
    AuditSeverity Severity,
    string Slot,
    string Reason,
    decimal ExpectedGain,
    string Verdict  // SWAP | REROLL | UPGRADE | KEEP
);
```

If this lands in `GearGoblin.Core` at v0.5.0, both the plugin's overmeld-cap
audit and the web's Audit section can render from the same data. Strongly
recommend ordering as:

1. **v0.4.8 plugin**: overmeld-cap fix → introduces severity model
2. **v0.5.0 Core**: severity model moves to `GearGoblin.Core`
3. **v0.6.0 web**: Audit section consumes severity model from Core

That way the v0.6.0 design ships against the right architectural backbone, not
against a temporary shape that gets refactored later.

---

## Recommended next-session flow when porting

1. Open `Design-Reference/demo.html` in a browser. Have it visible alongside.
2. Backup `Pages/Index.razor` → `Pages/Index.razor.v054-backup` so you can diff.
3. Step 1 → wire the stylesheet, verify scoped styles still don't conflict.
4. Step 2 → restructure the layout shell. Site will look broken at this point. That's expected.
5. Step 3 → title bar + rail. Site starts to look like the design.
6. Step 4 → hero screen head. Site looks like the design above the fold.
7. Step 5 → callout + recs + audit. The meat. Most time spent here.
8. Step 6 → footer actions.
9. Test paste flow end-to-end with Refia's VPR export. Verify recs render.
10. Stage as v0.6.0 dropin. Push.

Estimated total: 6–8 hours.
