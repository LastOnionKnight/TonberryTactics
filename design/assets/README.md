# Tonberry Tactics — Logo Assets (Lantern-lead, v1.3.0)

Direction **A · Lantern-lead**, locked. Brand palette: lantern gold `#C9A227`
→ cobalt `#2D6CDF` core, gold cage `#E5C24E`/`#8A7320`, deep navy ground
`#1B2A3A`/`#111c27`.

## Files

| File | Use |
|---|---|
| `tt-lantern-mark.svg` | Full-color bare mark, transparent bg. Headers, lockups, anywhere on dark. |
| `tt-icon.svg` | Square app icon — navy rounded-square bg + lantern. Source for all favicons. |
| `tt-lantern-gold.svg` | One-color gold. Stamps, single-color print, low-ink contexts. |
| `tt-lantern-knockout.svg` | Navy knockout — the mark on light/gold grounds. |
| `tt-lantern-mark-512.png` | Transparent raster of the bare mark (410×512). |
| `favicon-16/32/48/64.png` | Browser favicons. |
| `icon-128.png` | General small app icon. |
| `apple-touch-icon-180.png` | iOS home-screen / Apple touch icon. |
| `icon-512.png` | PWA manifest / store / high-res. |

## Web favicon snippet

```html
<link rel="icon" type="image/png" sizes="32x32" href="/assets/favicon-32.png">
<link rel="icon" type="image/png" sizes="16x16" href="/assets/favicon-16.png">
<link rel="apple-touch-icon" sizes="180x180" href="/assets/apple-touch-icon-180.png">
<link rel="icon" type="image/svg+xml" href="/assets/tt-icon.svg">
```

## Plugin (Dalamud) icon

Use `icon-512.png` for the plugin manifest `IconUrl`, or down-res to the
repo's expected size. The in-window titlebar can render `tt-lantern-mark.svg`
inline at ~22px tall in place of the placeholder `◆` glyph.

## Notes

- At 16px the lantern silhouette + glow still reads; if you ever need an even
  smaller or higher-contrast tile, the **TT monogram** (direction D) is the
  designed fallback — say the word and I'll cut that asset set too.
- All SVGs are hand-authored, no external fonts, no filters — they rasterize
  identically everywhere.
- Wordmark lockups (Cinzel) live in `../Logo Mockups.html`. Cinzel isn't
  outlined in these files; vectorize the wordmark in Canva/Illustrator if you
  need a font-independent lockup.
