// materia-advisor.jsx
// The "hero" screen for GearGoblin: Materia Advisor.
// Renders inside the FF3 dialog-box window chrome.

const { useState } = React;

const TABS = [
  { id: 'gear',    label: 'Current Gear',    badge: '13'   },
  { id: 'materia', label: 'Materia Advisor', badge: '3'    },
  { id: 'stats',   label: 'Stat Sheet',      badge: null   },
  { id: 'plan',    label: 'BiS Plan',        badge: 'Etro' },
  { id: 'about',   label: 'About',           badge: null   },
];

// Mock data — shaped like what MeldOptimizer would emit from the plugin
const RECS = [
  {
    rank: 'I',
    slot: 'Earrings',
    slotIcon: 'earring',
    from: { name: '— empty slot —', empty: true },
    to:   { name: 'Savage Aim XII', stat: 'Crit +36' },
    gain: '+18.4',
    gainLabel: 'score',
  },
  {
    rank: 'II',
    slot: 'Necklace',
    slotIcon: 'necklace',
    from: { name: "Heavens' Eye XI",  stat: 'DH +12' },
    to:   { name: "Heavens' Eye XII", stat: 'DH +14' },
    gain: '+6.2',
    gainLabel: 'score',
  },
  {
    rank: 'III',
    slot: 'Bracelet',
    slotIcon: 'bracelet',
    from: { name: 'Piety XII', stat: 'Pie +36' },
    to:   { name: 'Savage Might XII', stat: 'Det +36' },
    gain: '+11.7',
    gainLabel: 'score',
  },
];

const AUDITS = [
  {
    severity: 'crit', glyph: '!',
    slot: 'Bracelet #1',
    reason: <>Wrong stat for <em>BLM Pure-Math</em> — Piety has zero weight</>,
    gain: '+11.7',
    verdict: 'SWAP',
  },
  {
    severity: 'warn', glyph: '?',
    slot: 'Ring #2',
    reason: <>Overcap by <em>+24 Det</em> — last 24 points are dead</>,
    gain: '+4.1',
    verdict: 'REROLL',
  },
  {
    severity: 'warn', glyph: '?',
    slot: 'Earrings #2',
    reason: <>Tier XI in a Tier XII socket — <em>+2 stat</em> available</>,
    gain: '+2.0',
    verdict: 'UPGRADE',
  },
  {
    severity: 'ok', glyph: '✓',
    slot: 'Head, Body, Hands, Legs, Feet',
    reason: <>Guaranteed slots filled · no upgrades suggested</>,
    gain: '—',
    verdict: 'KEEP',
  },
];

function StatusChip({ tone = 'ok', label, value }) {
  return (
    <span className={`gg-chip ${tone}`}>
      <span>{label}</span>
      <span className="num">{value}</span>
    </span>
  );
}

function MateriaAdvisor() {
  const [selectedTab, setSelectedTab] = useState('materia');

  return (
    <>
      <header className="gg-screen-head">
        <div className="gg-titles">
          <div className="eyebrow">▼ &nbsp; CHAPTER IV · MATERIA ADVISOR</div>
          <h1>
            <span className="ornament">◆</span>
            The Knight Reports
          </h1>
          <div className="subtitle">
            Three suggestions to mend the gaps in thy melding, in order of consequence.
          </div>
        </div>

        <div className="gg-hero-knight" aria-hidden="true">
          <img src="assets/rags-portrait.png" alt="Refia" className="gg-mascot gg-mascot--hero" />
          <div className="label">— <em>REFIA</em>, TACTICIAN —</div>
        </div>

        <div className="gg-status-chips">
          <StatusChip tone="err"  label="CRIT" value="1" />
          <StatusChip tone="warn" label="WARN" value="2" />
          <StatusChip tone="ok"   label="OK"   value="9" />
        </div>
      </header>

      {/* Knight callout — dialog box with the speaker */}
      <section className="gg-knight-callout">
        <div style={{ display: 'grid', placeItems: 'center' }}>
          <img src="assets/rags-action.png" alt="" className="gg-mascot gg-mascot--callout" />
        </div>
        <div>
          <span className="speaker">▸ REFIA, TACTICIAN</span>
          <div className="speech">
            "Thy gear is sound, friend — but three slots yet protest. Mind the <span className="accent">Piety</span> upon
            thy bracelet; thou art no healer, and the realm doth not reward such waste."
          </div>
        </div>
      </section>

      {/* Recommendations section */}
      <div className="gg-section-head">
        <h2><span className="glyph">▶</span>RECOMMENDED MELDS</h2>
        <span className="meta">sorted by score gain · pure-math weights</span>
      </div>

      <div className="gg-recs">
        {RECS.map((r) => (
          <div className="gg-rec" key={r.rank}>
            <div className="rank">{r.rank}</div>

            <div className="slot">
              <span className="slot-icon"><SlotIcon slot={r.slotIcon} cell={2} /></span>
              <span className="slot-label">{r.slot}</span>
            </div>

            <div className="swap">
              <span className={`from ${r.from.empty ? 'empty' : ''}`}>
                <span className="name">{r.from.name}</span>
                {!r.from.empty && <><br/><span style={{fontSize:13, color:'var(--fg-3)'}}>{r.from.stat}</span></>}
              </span>
              <span className="arrow">►</span>
              <span className="to">
                <span className="name">{r.to.name}</span><br/>
                <span style={{fontSize:13, color:'var(--fg-cursor)'}}>{r.to.stat}</span>
              </span>
            </div>

            <div className="gain">
              <span className="num">{r.gain}</span>
              <span className="lbl">{r.gainLabel}</span>
            </div>

            <div className="apply">APPLY ▶</div>
          </div>
        ))}
      </div>

      {/* Audit section */}
      <div className="gg-section-head">
        <h2><span className="glyph">▶</span>MELD AUDIT</h2>
        <span className="meta">existing melds checked against BLM Pure-Math</span>
      </div>

      <div className="gg-audit">
        {AUDITS.map((a, i) => (
          <div className={`gg-audit-row sev-${a.severity}`} key={i}>
            <div className="severity">{a.glyph}</div>
            <div className="slot-cell">{a.slot}</div>
            <div className="reason">{a.reason}</div>
            <div className="gain">{a.gain}</div>
            <div className="verdict">{a.verdict}</div>
          </div>
        ))}
      </div>

      {/* Footer actions */}
      <div className="gg-actions">
        <div className="gg-btn">
          <span className="glyph">◆</span> COPY PLAN
        </div>
        <div className="gg-btn">
          <span className="glyph">↺</span> RE-SCAN INVENTORY
        </div>
        <div className="gg-btn is-primary">
          <span className="glyph">▶</span> EXPORT TO TONBERRY TACTICS
        </div>
      </div>
    </>
  );
}

function GearGoblinWindow({ palette, scanlines, onPaletteChange }) {
  const [selectedTab, setSelectedTab] = useState('materia');

  return (
    <div className="gg-window" data-screen-label="01 Materia Advisor">
      <span className="gg-corner tl" /><span className="gg-corner tr" />
      <span className="gg-corner bl" /><span className="gg-corner br" />

      {/* Title bar */}
      <div className="gg-titlebar">
        <div className="gg-title">
          <img src="assets/circle-logo.png" alt="" className="gg-brandmark" />
          <span>TONBERRY TACTICS</span>
          <span className="ver">v0.4.5 · REFIA · LV 100 BLM</span>
        </div>
        <div className="gg-winbtns">
          <div className="gg-winbtn" title="Pin">▣</div>
          <div className="gg-winbtn" title="Settings">⚙</div>
          <div className="gg-winbtn" title="Close">×</div>
        </div>
      </div>

      <div className="gg-body">
        {/* Left rail — classic FF menu with ► cursor */}
        <aside className="gg-rail">
          <div className="gg-rail-header">
            <div className="gg-mini-knight">
              <img src="assets/rags-mini.png" alt="" className="gg-mascot gg-mascot--rail" />
            </div>
            <div className="gg-rail-meta">
              <span className="name">REFIA</span>
              <span className="job">⚷ Black Mage · 100</span>
            </div>
          </div>

          <nav>
            {TABS.map((tab) => (
              <button
                key={tab.id}
                className={`gg-rail-item ${selectedTab === tab.id ? 'is-selected' : ''}`}
                onClick={() => setSelectedTab(tab.id)}
              >
                <span className="cursor">►</span>
                <span>{tab.label}</span>
                {tab.badge && <span className="badge">{tab.badge}</span>}
              </button>
            ))}
          </nav>

          <div className="gg-rail-spacer" />

          <div className="gg-rail-foot">
            <div>Use <span className="key">↑</span><span className="key">↓</span> to choose</div>
            <div style={{marginTop: 4}}><span className="key">⏎</span> to confirm</div>
          </div>
        </aside>

        {/* Right content — Materia Advisor */}
        <main className="gg-content">
          <MateriaAdvisor />
        </main>
      </div>
    </div>
  );
}

Object.assign(window, { GearGoblinWindow });
