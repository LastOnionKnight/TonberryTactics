# Tonberry Tactics v0.5.2 — Dropin Apply Instructions

**Target:** `D:\TonberryTactics-workspace\TonberryTactics`

This is a **docs/content-only** release that aligns Tonberry Tactics with
GearGoblin v0.4.5's full CharacterPanelRefined replacement positioning.
The optimizer, parser, serializer, and wire format are bit-for-bit
identical to v0.5.1 — only landing copy, version strings, README, and
CHANGELOG were touched.

---

## What changed

| File | Change |
|---|---|
| `Pages/Index.razor` | Header version pill bumped to `v0.5.2 · for GearGoblin v0.4.5+`; new amber-bordered "Plugin Update" sidebar callout describing v0.4.5; file-header comment updated; new `.menu-box-accent` CSS for the callout |
| `README.md` | Front section rewritten to lead with GG v0.4.5 framing; new "Why pair them" section; status line bumped |
| `CHANGELOG.md` | v0.5.2 entry prepended |
| `TonberryTactics.csproj` | Version 0.5.1 → 0.5.2; description mentions GG v0.4.5 compat |

**Untouched** (no diff vs v0.5.1):
- `Models/ExportSchema.cs` (wire-format DTOs)
- `Services/GearsetParser.cs`
- `Services/PureMathOptimizer.cs`
- `Services/PlanSerializer.cs`
- `wwwroot/*` (portraits, fonts, static assets)
- `build.sh`, `_redirects`, `_headers`
- `release.ps1`

## 1. Apply the dropin

Extract over your working tree, overwriting only these four files:

```
TonberryTactics/
├── TonberryTactics.csproj   ← version bump 0.5.1 → 0.5.2
├── CHANGELOG.md             ← v0.5.2 entry prepended
├── README.md                ← front section rewritten
└── Pages/
    └── Index.razor          ← version pill, file header, Plugin Update callout, CSS
```

## 2. Local sanity check

```powershell
cd D:\TonberryTactics-workspace\TonberryTactics
dotnet build -c Release
```

Should build clean. No code changed so no new compile errors are
possible from this dropin — if you hit one, it'll be a pre-existing
v0.5.1 issue surfaced by a clean build.

Optional — local dev server preview before push:

```powershell
dotnet run -c Release
```

Open the URL it prints, paste a GG-EXPORT string, confirm:
- Header pill reads `TLF GEAR DIVISION · v0.5.2 · for GearGoblin v0.4.5+`
- Right sidebar shows the new amber-bordered "Plugin Update" box below
  "Export to Game"
- Existing optimizer behavior is unchanged (same recommendations, same
  GG-PLAN output)

## 3. Push to deploy

Cloudflare Pages auto-rebuilds on push to `main`:

```powershell
.\release.ps1
```

Same as yesterday's v0.5.1 ship. Auto-detects 0.5.2 from csproj,
generates commit message from CHANGELOG v0.5.2 entry, tags `v0.5.2`,
pushes with `--follow-tags`. If you hit the rejected-push pattern again
(some unknown commits landed on origin/main between deploys):

```powershell
git fetch origin
git pull --rebase origin main
git tag -f v0.5.2
git push origin main
git push origin v0.5.2 --force
```

Cloudflare picks up the push and rebuilds within ~2-3 minutes. The live
site at https://tonberrytactics.pages.dev will reflect v0.5.2 once the
Pages deploy finishes (you'll see the build status in the Cloudflare
dashboard's Pages > tonberry-tactics > Deployments tab).

---

## What this dropin does NOT do

- No new optimizer modes (Balance preset still v0.5.3+)
- No multi-job awareness (still GNB hardcoded)
- No stat-cap respecting (still naive Tier XII fills)
- No code consolidation with the plugin (shared `GearGoblin.Core.dll`
  remains v0.6.0+ target)
- No new pages, no routing changes
- No new dependencies

This is purely "the website acknowledges the plugin grew up."

---

*"No gear. No hope. No pants. Just onions." — TLF*
*Stab once, stab true.*
