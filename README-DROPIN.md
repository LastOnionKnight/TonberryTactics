# Tonberry Tactics v0.5.4 dropin

**Headline:** "Feedback Loop." Closes the beta-reporting loop with
the in-game plugin. GearGoblin v0.4.7 ships a Feedback tab; this
release adds the matching panel to the web side.

## What's in this dropin

- `TonberryTactics.csproj` — version 0.5.3 → 0.5.4, Description
  reframed for Feedback Loop + v0.4.7 compatibility.
- `Pages/Index.razor` — three additions:
  1. New `▶ FEEDBACK` `menu-box` in the right-aside column, between
     PLUGIN UPDATE and the closing `</aside>`. Category radio,
     multiline message, "Include diagnostic info" checkbox (default
     on), two action buttons.
  2. New `.fb-*` CSS classes inside the existing `.ff-theme-wrapper`
     scoped `<style>` block. Reuses Press Start 2P pixel labels and
     lantern-gold `accent-color`.
  3. New code-behind state and handlers in `@code { ... }`:
     `FeedbackCategories`, `FeedbackLabels`, `FeedbackCategory`,
     `FeedbackText`, `FeedbackIncludeDiag`, `FeedbackLastAction`,
     `OpenGitHubIssue()`, `CopyFeedbackPayload()`,
     `BuildGitHubIssueUrlAsync()`, `BuildFeedbackPayloadAsync()`.
- `CHANGELOG.md` — new top entry for v0.5.4 plus the captured
  v0.6.x roadmap note for the TLF Gear Division redesign.

## Build & deploy

```bash
cd /path/to/TonberryTactics
dotnet build -c Release
# Cloudflare Pages auto-deploys on git push. If you want to verify
# locally before pushing:
dotnet run
# then open http://localhost:5000 (or whatever Kestrel picks)
```

## Smoke test

1. Open `https://tonberrytactics.pages.dev` (or local).
2. Right-aside column now shows three cards top-to-bottom:
   `▶ EXPORT TO GAME` → `▶ PLUGIN UPDATE` → `▶ FEEDBACK`.
3. In the Feedback panel:
   - Type a test message. Both buttons enable.
   - Click **COPY FOR DISCORD / DM**. Paste into a chat. Confirm
     the payload has `### Category`, `### Message`, and (if the
     checkbox is on) a fenced `### Diagnostic info` block with
     TT version, user-agent, export state, and timestamp.
   - Click **OPEN GITHUB ISSUE**. A new tab opens at
     `github.com/LastOnionKnight/TonberryTactics/issues/new?title=...`
     pre-populated. (If the repo doesn't exist yet, create it or
     change the `FeedbackRepoUrl` constant — see Notes below.)
4. With the checkbox **off**, copy again — confirm the diagnostic
   block is omitted.

## Notes

- **Repo URL.** Code-behind uses
  `https://github.com/LastOnionKnight/TonberryTactics`. If you have
  a different repo (or want feedback to land in the GearGoblin repo
  instead), change `FeedbackRepoUrl` in the `@code` block — single
  constant.
- **TT version.** Hardcoded as `0.5.4` in `TtVersion`. Mirror this
  with whatever `TonberryTactics.csproj` says when you cut subsequent
  releases.
- **No backend.** Same posture as the plugin: GitHub auth + Discord
  / DM fallback. Nothing leaves the browser without an explicit
  button press. If volume eventually justifies it, v0.6.x can add a
  Cloudflare Worker proxy that mirrors anonymized submissions
  server-side with rate-limiting.

## What's next

- **v0.6.x — TLF Gear Division redesign.** Direction captured at
  `v060-tt-design-reference/` in the GearGoblin sandbox. Component
  port map in `CHANGELOG.md`. Includes design tokens, manifesto
  copy, walking-Tonberry sprite, Tweaks panel for in-design accent
  swap, optional CRT scanlines.
