# TonberryTactics web v0.6.5.2 dropin — "Release Hardening"

**Mostly release-infra.** Web is the component that needed this work
the most — its `release.ps1` was the only one of the three without a
build gate, which is why the v0.6.5.1 first attempt managed to ship a
broken csproj to GitHub before being force-pushed back. Also one small
UX polish item: the EVERCOLD wordmark in the header now links to the
official FFXIV Evercold expansion page.

## What's in this dropin

```
release.ps1                       overwrite — fetch + rebase preamble + build gate
Pages/Index.razor                  overwrite — EVERCOLD <a>, all v0.6.5.2 version strings
wwwroot/css/design-v060.css        overwrite — .expansion-link hover/focus styles
TonberryTactics.csproj             overwrite — version 0.6.5.1 → 0.6.5.2
CHANGELOG.md                       overwrite — v0.6.5.2 entry on top
```

## Build & deploy

```
cd D:\TonberryTactics-workspace\TonberryTactics
Move-Item $env:USERPROFILE\Downloads\TonberryTactics-v0.6.5.2-dropin.zip ..\ -Force
Expand-Archive -Path ..\TonberryTactics-v0.6.5.2-dropin.zip -DestinationPath . -Force
Unblock-File .\release.ps1
.\release.ps1 -DryRun
.\release.ps1
```

Two new lines in the release output relative to v0.6.5.1:

1. **"Syncing with origin/main (fetch + rebase + autostash)…"** near
   the top — protects against the non-fast-forward push rejection
   that hit on the v0.6.5.1 recovery.
2. **"Running build gate: dotnet build --configuration Release…"**
   between the status display and the commit-message preview — same
   pattern Core and Plugin have had since v0.4.6. Aborts the release
   with a clear message on non-zero exit. Bypass with `-SkipBuild`
   for rare fast-iteration cases.

## Verify after Cloudflare Pages deploys

1. Hard-refresh `tonberrytactics.pages.dev` (Ctrl+F5).
2. Header pill reads **v0.6.5.2**. Footer copy says
   `TONBERRY TACTICS · v0.6.5.2 · CLOUDFLARE PAGES · …`.
3. **Hover the EVERCOLD wordmark** in the header — the ice-cyan glow
   should intensify subtly, cursor changes to a pointer.
4. **Click the EVERCOLD wordmark** — opens
   `https://na.finalfantasyxiv.com/evercold/` in a new tab.
5. **Tab to the wordmark with keyboard** — a 2px ice-cyan focus ring
   surfaces, then Enter activates the link.
6. **Audit panel** still works correctly (v0.6.5.1 off-by-one fix
   preserved — no regression).

## Notes on the build gate (key learning from v0.6.5.1)

The v0.6.5.1 web dropin shipped `<AssemblyVersion>0.6.5.1.0</AssemblyVersion>`
(5 components, invalid per CS7034). Core and Plugin's build gates caught
it locally and aborted. Web's gateless `release.ps1` committed, tagged
`v0.6.5.1`, and pushed to GitHub anyway — Cloudflare Pages then failed
its build on the same error, and recovery required:

- `git tag -d v0.6.5.1` + `git push --delete origin v0.6.5.1`
- `git reset --hard HEAD~1`
- re-ship corrected dropin
- `git push --force-with-lease origin main`

All of that goes away in v0.6.5.2. The build gate now catches the same
class of error locally and aborts before any git operation happens.

## Pairing

- **GearGoblin.Core v0.6.5.2** — same release.ps1 sync step,
  lockstep version bump.
- **GearGoblin plugin v0.6.5.2** — same sync step; particularly
  important on the plugin side because of the `repo.json`
  github-actions[bot] that pushes after every tag.

## Out of scope (deferred to v0.6.6+)

- Lodestone integration (architecture choice pending).
- Character-panel advisor row offset (plugin-side, not web).
- Balance preset, stat-cap math, Akhmorning breakpoint formulas.
