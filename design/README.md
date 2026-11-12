# Tonberry Tactics — Graphics & Design Workbench

This folder is a standalone, browser-runnable design environment for updating Tonberry Tactics website graphics and layout.

- **Open `index.html` directly in any web browser** — no Node, no Blazor, no build step required.
- Everything runs on plain HTML5, CSS3, and vanilla JS.
- All styles are in `styles.css`.
- All images, icons, and fonts are in `assets/`.

---

## 🎨 Graphic Surfaces to Update

| Surface | File Path | Current Asset & Role |
| :--- | :--- | :--- |
| **Masthead Banner** | `assets/masthead.png` | Cinematic gold-lantern wide header banner |
| **Brand Mark / Seal** | `assets/logo-circle.png` / `circle-logo.png` | Circular TLF Onion Knight crest in sub-bar |
| **Adventurer Banner** | `assets/refia-dungeon.jpg` | Wide background behind the Adventurer Plate |
| **Adventurer Hero Portrait** | `assets/rags-portrait.png` / `rags-mini.png` | Refia Rakkiri portrait (Miqo'te Sun Seeker, Onion Knight helm, blue/white/leather palette) |
| **TLF Flame Logo** | `assets/tlf-flame-logo-768.png` | Tonberry Liberation Front flame emblem in PSA section |
| **Refia Action Sprite** | `assets/rags-action.png` | Sprite on video thumbnail |
| **Tonberry Mascot** | `assets/tonberry-mascot.png` | Tonberry lantern mascot |
| **Lantern Marks (Vector)** | `assets/tt-lantern-mark.svg` | Theme toggle & footer decorative mark |
| **Jobstone Crystals** | `assets/jobs/[JOB].png` | 80x80 soul crystal icons (includes new `BAS.png`, `ZEP.png`, `BST.png`) |
| **Social / OG Banner** | `assets/og-banner.png` | 1200x630 OpenGraph card |
| **App Icons & Favicons** | `assets/favicon-16.png`, `favicon-32.png`, `apple-touch-icon-180.png`, `tt-icon.svg` | Browser tab and mobile icons |

---

## 🔁 Hand-off Instructions

1. Edit `index.html`, `styles.css`, or swap files inside `assets/`.
2. Double-check your visual changes by opening `index.html` in your browser.
3. Test dark and light mode using the lantern toggle in the subbar.
4. When finished, inform the assistant: we will merge the updated HTML, CSS, and assets directly into `TonberryTactics/Pages/Index.razor` and `TonberryTactics/wwwroot/`.
