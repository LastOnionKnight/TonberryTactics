# Tonberry Tactics — Web Package (v1.4.0 · Phase 2 visual overhaul)

The TT optimizer web surface. Plain HTML/CSS/JS — no framework, no build step —
so it ports cleanly to the Blazor WASM app. Open `index.html` directly.

## Files

| File | Purpose |
|---|---|
| `index.html` | The optimizer app — masthead, hero demo, all panels, sample loader. |
| `styles.css` | All styles + design tokens (`:root`). Dark theme. |
| `landing-marketing.html` | Earlier standalone marketing landing page (kept for reference). |
| `assets/masthead.png` | Cinematic gold-lantern masthead banner (Canva). |
| `assets/og-banner.png` | Wide social/OG banner. |
| `assets/onion-walker.png` | Walking Onion Knight sprite (footer). |
| `assets/adventurer.png` | Adventurer-card portrait placeholder. |
| `assets/logo-circle.png` | Round TLF crest. |
| `assets/tt-lantern-mark.svg` · `tt-icon.svg` | Lantern logo (vector) + square app icon. |
| `assets/favicon-16/32.png` · `apple-touch-icon-180.png` | Favicons. |

## Phase 2 — what's new in this pass

**1. The hero — live "close-the-gap" demo** (`.hero-demo`)
On load, two cap-gauges animate current→cap (gold fill → cobalt near-cap glow),
settle short of the cap tick, then a beat later a **green projected ghost fill**
glows ahead ("after optimization"). The headline lands as the fill settles.
`↺ Replay` re-runs it. This is the 3-second value-prop demo.

**2. Gauges became living instruments** (`renderStats`)
- **Projected overlay** (`.gauge-proj`) — ghosted green fill ahead of the solid
  fill when the optimizer proposes a meld for that stat. One glance = the delta.
- **Overcap bleed** (`.gauge-bleed`) — overcapped stats render the wasted portion
  *past* the cap tick, pulsing restrained red (see Skill Speed in the sample).
- **Near-cap cobalt glow** — fill shifts gold→cobalt as it crosses 90%.

**3. Cinematic masthead** replaces the old text header; slim sticky sub-bar
carries tagline · version · plugin status · patch pill.

## Motion discipline (carried from the brief)

- **Tier 0 — stillness:** stat numbers, audit rows, meld lists never animate
  while being read. They appear, then hold still.
- **Tier 1 — functional motion:** the gauge fills + projected overlay (they
  *teach* the cap relationship). The majority of the motion budget.
- **Tier 2 — flourish:** footer walk-cycle, hover lifts. Chrome only.
- All motion animates `width`/`opacity`/`transform` only and respects
  `prefers-reduced-motion` (everything snaps to end-state).

## Blazor / port notes

- **Tokens** live in `:root` in `styles.css` — lift them into the app's global
  stylesheet verbatim.
- **Gauge math** is in `index.html` → `gState()` / `projPctOf()`. The web
  **renders, never recomputes** — real builds feed it plugin-computed
  `TotalStat.Value` + `.Cap`. Replace `SAMPLE` / `HERO` / `EMPTY` with live data.
- **Hero data source** is still open (brief §7.1): currently a fixed
  representative sample (`HERO`). Swap for last-pasted gear or a rotating set.
- **Fonts:** Cinzel · EB Garamond · JetBrains Mono · Press Start 2P (pixel =
  tiny accents only), loaded from Google Fonts in `<head>`.
- **No `scrollIntoView`-dependent logic**, no sticky inside panels — safe to host
  inside the app shell.

## Sample data

`Load sample export` / `▶ Step Forward` populates the whole page with the GNB
i735 "Refia Rakkiri" sample so you can see the populated state (gauges with
projected overlays, overcap bleed on Skill Speed, advisor recs, audit severities).
