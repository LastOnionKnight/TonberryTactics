# Tonberry Tactics — Web Refresh (Design Package)

**DTG:** 20260618
**Direction (locked with operator):** lighter & cleaner ground · subtle pixel accents · glowing **gold→cobalt** cap-gauges (the signature element).
**Purpose:** brand-align the website to the TLF/Onion identity and land the Schema v2 real-cap bars in the same pass.
**Status:** design reference + drop-in component. NOT a re-skin of the live `Index.razor` — that mapping is Antigravity/Claude Code's job.

---

## What's in here
- `styles.css` — the token system (`:root`). Light/Frost palette, gold/cobalt accents, Cinzel/EB Garamond/Press Start 2P/JetBrains Mono stack, gauge glows.
- `TonberryTactics.html` — full reference page: hero (the thesis = a live cap-gauge), the 3-step optimizer flow, the stat-profile panel with cap-bars in context, footer. Open in a browser to see the whole language working.
- `components/CapGauge.razor` — **the signature element as a real Blazor component.** Drop-in. Renders one substat as a gold→cobalt cap gauge with over-cap + near-cap states.

---

## The brand mapping (why these choices)
Everything derives from the existing `TlfTheme`, not invented:

| Brand token (TlfTheme) | Web token | Role |
|---|---|---|
| Lantern gold | `--lantern-gold #C9A227` | signature accent |
| GoldBright / GoldDim | `--gold-bright / --gold-dim` | hover / low-emphasis |
| InkPanel | `--ink-panel #1B2A3A` | text + fine chrome (pulled back from full-bg) |
| Frost Soft/Dim/Faint | `--frost-soft/dim/faint` | the light ground + panels |
| (new cool counter-tone) | `--cobalt #2D6CDF` | "near cap" — the one new color, justified by the gauge logic |
| Cinzel | `--font-display` | headers |
| EB Garamond | `--font-body` | body |
| Press Start 2P | `--font-pixel` | **tiny accents only** — eyebrow + step numbers, never body |
| JetBrains Mono | `--font-data` | stat numbers |

**Restraint note (per design discipline):** boldness is spent in ONE place — the cap-gauge glow. Everything else stays quiet: light surfaces, hairline borders, elegant serif type. The retro-pixel flavor is deliberately subtle (two corner ticks + the eyebrow/step labels), never a Game-Boy texture. If anything reads as too much, the first thing to cut is the `body::before` ambient glow.

---

## The signature: cap-gauge gold→cobalt logic
The fill gradient runs gold→cobalt left-to-right, so **the fuller the bar, the more cobalt shows** — the color itself reads the data. Three states (in `styles.css`, driven by `CapGauge.razor`):
- `.is-mid` — building, gold glow.
- `.is-near` — ≥90% of cap, cobalt glow (the "you're almost there" signal).
- `.is-over` — past cap, red gradient + red glow (wasted stat).
- `.cap-tick` — the cap line at 100%.

No-cap stats (Determination) fill relative to a soft reference and show no tick — they can't be "capped."

---

## Schema v2 binding (the reason this refresh is now)
The cap-gauges are the front-end for the **Schema v2 / total-substats** work order (`WO-ANTIGRAVITY_schema-v2-total-substats`). The web couldn't show real caps before because the export carried no totals. Once the plugin emits `GG-EXPORT:v2` with `TotalStats`, `CapGauge` binds straight to those values:

```razor
@foreach (var s in character.TotalStats)
{
    <CapGauge Label="@s.DisplayName" Value="@s.Value" Cap="@CapFor(s, character.ItemLevel)" />
}
```

`CapFor(...)` lives in **GearGoblin.Core** (single source of truth — don't duplicate cap math in the web). So this design and the Schema v2 work order ship together as **v1.3.0**, or the refresh ships first with placeholder/relative values and the bars go live when v2 lands. Operator's call on sequencing.

---

## Port notes for Antigravity / Claude Code
1. Drop `styles.css` `:root` block into the site's global stylesheet (merge tokens; don't fight existing class names — check selector specificity, especially section padding).
2. Lift `CapGauge.razor` into the Blazor components folder as-is. It only needs the gauge classes from `styles.css`.
3. Re-skin existing panels to the token system incrementally — the HTML reference shows target treatments for hero, steps, and the stat panel. Layout/flow is unchanged; this is a skin, not a restructure.
4. Fonts: Cinzel / EB Garamond / JetBrains Mono / Press Start 2P via Google Fonts `<link>` (already in the reference `<head>`). These are the **same faces as the plugin** — site and in-game tool now share a type identity.
5. Respect `prefers-reduced-motion` (already handled in tokens — gauge fill animation zeroes out).
6. Quality floor: responsive to mobile (reference collapses at 820px), visible focus, hairline-on-light contrast checked.

---

## Copy note
The reference uses real Refia voice in two sane places (footer: *"for the lads still chasing the cap"*) but keeps the tool's working copy plain and active per UX-writing discipline — "Paste your gear," "Import the plan." The brand voice flavors the chrome; it never gets in the way of someone trying to read a stat number. Keep that split.
