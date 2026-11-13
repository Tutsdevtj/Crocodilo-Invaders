// Draws the "Hangar" menu wordmark on a pixel grid, matching the cream/navy chunky labels
// of Assets/Art/MainMenu (Jogar, Opções...). Deterministic: rerun to regenerate the PNG.
const fs = require('node:fs'), path = require('node:path'), crypto = require('node:crypto');
let sharp;
try { sharp = require('sharp'); }
catch { sharp = require(path.join(process.env.USERPROFILE, '.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp')); }
const root = path.resolve(__dirname, '../..');
const output = path.join(root, 'Assets/Art/MainMenu/Hangar.png');
const preview = path.join(root, 'Docs/ArtPreviews/Wardrobe_v01');

// Each glyph: rows of '#'. top = first row (0 = cap line, 3 = x-height line).
const glyphs = {
  H: { top: 0, rows: ['###...###', '###...###', '###...###', '###...###', '###...###', '#########', '#########', '###...###', '###...###', '###...###', '###...###', '###...###'] },
  a: { top: 3, rows: ['#######.', '########', '.....###', '.#######', '########', '###..###', '###..###', '########', '.#######'] },
  n: { top: 3, rows: ['#######.', '########', '###..###', '###..###', '###..###', '###..###', '###..###', '###..###', '###..###'] },
  g: { top: 3, rows: ['.#######', '########', '###..###', '###..###', '###..###', '###..###', '########', '.#######', '.....###', '.....###', '.....###', '########', '#######.'] },
  r: { top: 3, rows: ['###.###', '#######', '####...', '###....', '###....', '###....', '###....', '###....', '###....'] },
};
const word = 'Hangar', gap = 2, cell = 10, pad = 3;
const cream = [248, 240, 218], shadow = [11, 76, 124], navy = [5, 36, 56];

let width = 0;
for (const ch of word) width += glyphs[ch].rows[0].length + gap;
width -= gap;
const height = 16;
const W = width + pad * 2, H = height + pad * 2;
const fill = new Uint8Array(W * H);
let x0 = pad;
for (const ch of word) {
  const g = glyphs[ch];
  g.rows.forEach((row, y) => [...row].forEach((c, x) => { if (c === '#') fill[(pad + g.top + y) * W + x0 + x] = 1; }));
  x0 += g.rows[0].length + gap;
}
// Layer order: navy outline around (fill + drop shadow), shadow one cell down, cream fill on top.
const at = (a, x, y) => x >= 0 && y >= 0 && x < W && y < H && a[y * W + x];
const body = new Uint8Array(W * H);
for (let y = 0; y < H; y++) for (let x = 0; x < W; x++) body[y * W + x] = at(fill, x, y) || at(fill, x, y - 1) ? 1 : 0;
const grid = new Array(W * H).fill(null);
for (let y = 0; y < H; y++) for (let x = 0; x < W; x++) {
  let near = false;
  for (let dy = -1; dy <= 1; dy++) for (let dx = -1; dx <= 1; dx++) near = near || at(body, x + dx, y + dy);
  if (near) grid[y * W + x] = navy;
  if (at(fill, x, y - 1)) grid[y * W + x] = shadow;
  if (at(fill, x, y)) grid[y * W + x] = cream;
}
const raw = Buffer.alloc(W * cell * H * cell * 4);
for (let y = 0; y < H * cell; y++) for (let x = 0; x < W * cell; x++) {
  const c = grid[Math.floor(y / cell) * W + Math.floor(x / cell)], i = (y * W * cell + x) * 4;
  if (c) { raw[i] = c[0]; raw[i + 1] = c[1]; raw[i + 2] = c[2]; raw[i + 3] = 255; }
}
(async () => {
  const png = await sharp(raw, { raw: { width: W * cell, height: H * cell, channels: 4 } }).png({ compressionLevel: 9 }).toBuffer();
  fs.writeFileSync(output, png);
  fs.mkdirSync(preview, { recursive: true });
  fs.writeFileSync(path.join(preview, 'hangar-wordmark.png'), png);
  console.log(`HANGAR_WORDMARK_OK ${W * cell}x${H * cell} sha256=${crypto.createHash('sha256').update(png).digest('hex')}`);
})();
