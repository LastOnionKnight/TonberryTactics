// onion-knight.jsx
// Pixel-art Onion Knight sprite, rendered as SVG rects.
// Two variants: <OnionKnight size="hero" /> (big portrait) and size="mini" (rail avatar).

const OK_PALETTE = {
  '.': null,                 // transparent
  'K': '#1a1330',            // outline / darkest
  'k': '#2e2148',            // softer outline
  'S': '#5a4480',            // topknot mid
  's': '#3a2c5e',            // topknot stem dark
  'h': '#b6c4d0',            // helmet silver mid
  'H': '#ecf2f6',            // helmet bright highlight
  'D': '#6a7888',            // helmet shadow
  'd': '#4a5868',            // helmet deep shadow
  'v': '#100820',            // visor slit
  'f': '#f0c896',            // face skin
  'F': '#c08858',            // face shadow
  'g': '#c89018',            // tunic gold mid
  'G': '#f6d660',            // tunic gold highlight
  'Y': '#7a4e10',            // tunic gold dark edge
  'b': '#5a3818',            // brown belt mid
  'B': '#2e1a08',            // brown dark / boot
  'p': '#3a8040',            // pants green mid
  'P': '#1e4a22',            // pants green dark
  'n': '#62a85a',            // pants green light
  'o': '#cad6e2',            // sword blade
  'O': '#f4faff',            // sword blade highlight
  'w': '#d8a830',            // sword guard/pommel gold
  'W': '#8a5a14',            // pommel shadow
  'r': '#a8702a',            // shield rim
  'R': '#c8902a',            // shield rim highlight
  'c': '#2a4ca8',            // shield blue
  'C': '#5278d8',            // shield blue highlight
};

// ── HERO PORTRAIT — 22 × 32 ────────────────────────────────────────────────
const OK_HERO = [
  '..........SS..........',  // 0
  '..........SS..........',  // 1
  '.........SSSS.........',  // 2
  '.........SSSS.........',  // 3
  '........KsssK.........',  // 4
  '.......KhHHHhK........',  // 5
  '......KhHHHHHhK.......',  // 6
  '.....KhHHHHHHHhK......',  // 7
  '....KhhHHHHHHHhhDK....',  // 8
  '...KhhHHHHHHHHHhhhDK..',  // 9
  '..KhhhHHHHHHHHhhhhDDK.',  // 10
  '..KhhhhhHHHHHhhhhhDDK.',  // 11
  '..KhhhhhhhhhhhhhhhDDK.',  // 12
  '..Khhhhvvhhhhhvvhhhddk',  // 13
  '..Khhhhvvhhhhhvvhhhddk',  // 14
  '..Khhhhhhhhhhhhhhhhddk',  // 15
  '...KhhhhhhhhhhhhhhhdK.',  // 16
  '....KKhhfffffffhhKK...',  // 17
  '......KffFFFFFFffK....',  // 18
  '......KfffFFFFfffK....',  // 19
  '.....YggggggggggggY...',  // 20
  '....YgGGGGGGGGGGGGgY..',  // 21
  '...rYgGGGGGGGGGGGGgY..',  // 22
  '..rRrYgGGGGGGGGGGggY..',  // 23
  '.rRCcYbbbbbbbbbbbbY..o',  // 24  belt + sword blade start
  '.rRCCrYgGGGGGGGGgY.wWw',  // 25  gold skirt + cross-guard
  '.rRCCcrYpppppppgY..oO.',  // 26
  '..rRCcr.YppPPPppY..oO.',  // 27
  '...rRrr..pPPnnPpp..oO.',  // 28
  '..........pPPnnPpp.oO.',  // 29
  '..........BBB.BBB..oO.',  // 30
  '..........KKK.KKK..ww.',  // 31  pommel
];

// ── MINI AVATAR — 14 × 16 ──────────────────────────────────────────────────
const OK_MINI = [
  '......SS......',
  '.....SsssS....',
  '....KhHHHhK...',
  '...KhHHHHhDK..',
  '..KhhHHHHhhDK.',
  '..KhhhhhhhhDK.',
  '..Khvhhhhhvhdk',
  '..Khvhhhhhvhdk',
  '...Khhfffhhdk.',
  '....KfFFFfK...',
  '...YggGGGggY..',
  '..YgGGGGGGgY..',
  '..YbbbbbbbbY..',
  '..YgGGGGGGgY..',
  '..YppppPpppY..',
  '...BBB.BBB....',
];

function PixelSprite({ pixels, cell = 6, style = {}, ariaLabel }) {
  const rows = pixels.length;
  const cols = pixels[0].length;
  const rects = [];
  for (let y = 0; y < rows; y++) {
    for (let x = 0; x < cols; x++) {
      const ch = pixels[y][x];
      const color = OK_PALETTE[ch];
      if (color) {
        rects.push(
          <rect
            key={`${x},${y}`}
            x={x}
            y={y}
            width="1.02"
            height="1.02"
            fill={color}
          />
        );
      }
    }
  }
  return (
    <svg
      role={ariaLabel ? 'img' : undefined}
      aria-label={ariaLabel}
      viewBox={`0 0 ${cols} ${rows}`}
      width={cols * cell}
      height={rows * cell}
      shapeRendering="crispEdges"
      style={{ display: 'block', imageRendering: 'pixelated', ...style }}
    >
      {rects}
    </svg>
  );
}

function OnionKnight({ variant = 'hero', cell, className, style }) {
  const pixels = variant === 'mini' ? OK_MINI : OK_HERO;
  const defaultCell = variant === 'mini' ? 2 : 7;
  return (
    <span className={className} style={style}>
      <PixelSprite
        pixels={pixels}
        cell={cell ?? defaultCell}
        ariaLabel="Onion Knight"
      />
    </span>
  );
}

// ── Pixel slot icons (sword / ring / armor) — 8×8 grids ────────────────────
const SLOT_ICONS = {
  earring: [
    '..KKKK..',
    '.KGGGGK.',
    '.KGggGK.',
    '.KGGGGK.',
    '..KKKK..',
    '...KK...',
    '...rr...',
    '...KK...',
  ],
  necklace: [
    '.K....K.',
    '.K....K.',
    '..K..K..',
    '..K..K..',
    '...KK...',
    '..KGGK..',
    '..KGGK..',
    '...KK...',
  ],
  bracelet: [
    '..KKKK..',
    '.KGGGGK.',
    'KGRRRGK.',
    'KGRRRGK.',
    '.KGGGGK.',
    '..KKKK..',
    '........',
    '........',
  ],
  ring: [
    '..KKKK..',
    '.KooooK.',
    'KoOOOoK.',
    'KoOROoK.',
    'KoOOOoK.',
    '.KooooK.',
    '..KKKK..',
    '........',
  ],
  weapon: [
    '......KK',
    '.....KOK',
    '....KOK.',
    '...KOK..',
    '..KOK...',
    '.KwwK...',
    'KKwK....',
    '.KK.....',
  ],
};

function SlotIcon({ slot, cell = 2 }) {
  const grid = SLOT_ICONS[slot] || SLOT_ICONS.ring;
  return <PixelSprite pixels={grid} cell={cell} />;
}

Object.assign(window, { OnionKnight, PixelSprite, SlotIcon, OK_PALETTE });
