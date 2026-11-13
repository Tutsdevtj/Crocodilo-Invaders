// Crocs skin v02: jet wings, tail fin and engine on the v01 clog, plus a Crocs-style helmet
// with war jibbitz plugged into the holes. Drawn on the native 160x120 grid.
// Base: v01 hull (SourceArt/CrocsSkin_v01/shoe-native.png) and the production pilot cels.
// Replaces the v01 output in Assets/Art/Player/Crocs (same files, GUIDs and pivots).
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
let sharp;
try { sharp = require('sharp'); }
catch { sharp = require(path.join(process.env.USERPROFILE, '.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp')); }
const root = path.resolve(__dirname, '../..');
const v01 = path.join(root, 'SourceArt/CrocsSkin_v01');
const source = path.join(root, 'SourceArt/CrocsSkin_v02');
const preview = path.join(root, 'Docs/ArtPreviews/CrocsSkin_v02');
const W = 160, H = 120;
const hex = h => [0, 2, 4].map(i => parseInt(h.slice(i, i + 2), 16));
const C = {
  ink: hex('101018'), d1: hex('30391e'), d2: hex('47572a'), g1: hex('5d6e32'), g2: hex('738642'), g3: hex('8b9e48'),
  g4: hex('a5b653'), g5: hex('c3d66c'), g6: hex('e6e6a5'), cream: hex('fff0ce'), ivory: hex('ebd8a6'),
  m0: hex('2a2d33'), m1: hex('42464c'), m2: hex('6d7276'), m3: hex('9a9e98'), m4: hex('b5b9ae'),
  gold: hex('ffd34a'), goldD: hex('c98a1a'), red: hex('c8382c'), redD: hex('8a2020'), white: hex('f4f1e6'),
  fireO: hex('ff7a1c'), fireY: hex('ffd34a'),
};

// v01 masks: which pixels of the production cel belong to the pilot.
const pilotMask = (x, y) => x <= 130 && ((x >= (y < 51 ? 44 : 47) && y < 59) ||
  (x >= 63 && y < 62) || (x >= 68 && y < 65) || (x >= 75 && y < 69) || (x >= 80 && x <= 123 && y < 71));

const newLayer = () => new Array(W * H).fill(null);
const put = (L, x, y, c) => { if (c && x >= 0 && y >= 0 && x < W && y < H) L[y * W + x] = c; };
const get = (L, x, y) => (x >= 0 && y >= 0 && x < W && y < H ? L[y * W + x] : null);
function shift(L, dx, dy) { const out = newLayer(); for (let y = 0; y < H; y++) for (let x = 0; x < W; x++) if (L[y * W + x]) put(out, x + dx, y + dy, L[y * W + x]); return out; }
function over(base, top) { for (let i = 0; i < W * H; i++) if (top[i]) base[i] = top[i]; return base; }
function under(base, back) { for (let i = 0; i < W * H; i++) if (!base[i] && back[i]) base[i] = back[i]; return base; }
function inPoly(px, py, pts) {
  let inside = false;
  for (let i = 0, j = pts.length - 1; i < pts.length; j = i++) {
    const [xi, yi] = pts[i], [xj, yj] = pts[j];
    if ((yi > py) !== (yj > py) && px < (xj - xi) * (py - yi) / (yj - yi) + xi) inside = !inside;
  }
  return inside;
}
// Fill a shape given by a predicate, with a one-pixel ink outline and a shading callback.
function shape(L, test, shade) {
  const inside = (x, y) => x >= 0 && y >= 0 && x < W && y < H && test(x + .5, y + .5);
  for (let y = 0; y < H; y++) for (let x = 0; x < W; x++) {
    if (!inside(x, y)) continue;
    const edge = !inside(x - 1, y) || !inside(x + 1, y) || !inside(x, y - 1) || !inside(x, y + 1);
    put(L, x, y, edge ? C.ink : shade(x, y));
  }
}
// Small sprites: '.' transparent, letters map to colors.
function stamp(L, cx, cy, rows, map) {
  const ox = cx - Math.floor(rows[0].length / 2), oy = cy - Math.floor(rows.length / 2);
  rows.forEach((row, y) => [...row].forEach((ch, x) => { if (ch !== '.') put(L, ox + x, oy + y, map[ch]); }));
}

// ---- War jibbitz (each one plugged into a Crocs hole) ----
const K = { k: C.ink };
const JIBBITZ = {
  star: { rows: ['...k...', '..kyk..', 'kkyYykk', 'kyyyyyk', '.kyyyk.', '.kykyk.', '.kk.kk.'], map: { ...K, y: C.gold, Y: C.cream } },
  bomb: { rows: ['....f.', '...k..', '.kkkk.', 'kbbbbk', 'kbWbbk', 'kbbbbk', '.kkkk.'], map: { ...K, b: C.m0, W: C.m2, f: C.fireO } },
  grenade: { rows: ['.kkk..', 'kmmmk.', '.kgk..', 'kgGgk.', 'kGgGk.', 'kgGgk.', '.kkk..'], map: { ...K, m: C.m3, g: C.d2, G: C.g2 } },
  dogtag: { rows: ['.kkk.', 'ksksk', 'kssSk', 'kssSk', 'kSSSk', '.kkk.'], map: { ...K, s: C.m4, S: C.m3 } },
  medal: { rows: ['kkkkk', 'krwrk', 'krwrk', '.kkk.', 'kyyyk', 'kyYyk', '.kkk.'], map: { ...K, r: C.red, w: C.white, y: C.gold, Y: C.cream } },
  bullet: { rows: ['.k.', 'kck', 'kck', 'kyk', 'kyk', 'kYk', 'kkk'], map: { ...K, c: C.goldD, y: C.gold, Y: C.cream } },
};

// ---- Crocs helmet (pilot coordinates of idle_0; moved with the head per frame) ----
const HELMET = { cx: 74, cy: 34, rx: 25.5, ry: 22.5 };
const HELMET_HOLES = [[64, 18], [74, 17], [84, 19], [58, 25], [68, 25], [79, 25], [90, 26]];
const HELMET_JIBBITZ = [['star', 74, 17], ['dogtag', 58, 25], ['grenade', 79, 25], ['bomb', 90, 25]];
function drawHelmet() {
  const L = newLayer();
  const dome = (x, y) => y <= 31.5 && ((x - HELMET.cx) / HELMET.rx) ** 2 + ((y - HELMET.cy) / HELMET.ry) ** 2 <= 1;
  // Brim: thick rubber rim, longer at the front (right), with a rear skirt over the neck.
  const brim = (x, y) => inPoly(x, y, [[48, 29], [101, 29], [104, 31], [103, 34], [57, 34], [56, 37.5], [47, 37.5]]);
  shape(L, (x, y) => dome(x, y) || brim(x, y), (x, y) => {
    if (brim(x, y) && y > 30.5) return y < 32.5 ? C.g3 : C.g1;
    const nx = (x - HELMET.cx) / HELMET.rx, ny = (y - HELMET.cy) / HELMET.ry;
    const light = -ny * .75 + nx * .35;
    return light > .8 ? C.g5 : light > .55 ? C.g4 : light > .2 ? C.g3 : C.g2;
  });
  // Rim highlight line and a few specular pixels.
  for (let x = 50; x <= 100; x++) if (get(L, x, 30) && get(L, x, 30) !== C.ink) put(L, x, 30, C.g4);
  for (const [x, y] of [[80, 14], [81, 14], [84, 15], [86, 16], [88, 17]]) if (get(L, x, y) && get(L, x, y) !== C.ink) put(L, x, y, C.g6);
  // Ventilation holes (Crocs signature).
  for (const [x, y] of HELMET_HOLES) stamp(L, x, y, ['.kk.', 'kkkk', '.kk.', '.dd.'], { k: C.ink, d: C.g1 });
  // Heel-strap rivet on the side, with the strap running back over the skirt.
  for (let i = 0; i < 6; i++) { put(L, 49 + i, 33 + (i < 3 ? 1 : 0), C.g2); put(L, 49 + i, 35, C.d2); }
  stamp(L, 54, 31, ['.kkk.', 'kcCck', 'kCCck', 'kcckk', '.kkk.'], { k: C.ink, c: C.ivory, C: C.cream });
  for (const [name, x, y] of HELMET_JIBBITZ) stamp(L, x, y, JIBBITZ[name].rows, JIBBITZ[name].map);
  return L;
}

// ---- Jet parts (shoe coordinates, moved with the shoe per frame) ----
function metal(x, y, x0, x1) { const t = (x - x0) / Math.max(1, x1 - x0); return t > .7 ? C.m4 : t > .35 ? C.m2 : C.m1; }
function drawNearWing() {
  // v02.1: swept jet wing bolted to the side of the clog (root fairing over the lower side),
  // sweeping back and down past the heel.
  const L = newLayer();
  const pts = [[82, 84], [46, 84], [4, 109], [26, 111]];
  const leadX = y => 82 + (y - 84) * (26 - 82) / 27;
  shape(L, (x, y) => inPoly(x, y, pts), (x, y) => {
    const d = leadX(y) - x;
    return y <= 86 ? C.m3 : d < 3 ? C.m4 : d < 12 ? C.m2 : C.m1;
  });
  // Root fairing: darker seam under the mount strip, with bolts into the rubber.
  for (let x = 47; x <= 81; x++) { if (get(L, x, 87) && get(L, x, 87) !== C.ink) put(L, x, 87, C.m0); }
  for (let x = 51; x <= 79; x += 7) put(L, x, 85, C.ink);
  // Panel line parallel to the leading edge and a white star roundel.
  for (let y = 90; y <= 108; y++) { const x = Math.round(leadX(y) - 14); if (get(L, x, y) && get(L, x, y) !== C.ink) put(L, x, y, C.m0); }
  stamp(L, 34, 102, ['..kk..', '.kddk.', 'kdwwdk', 'kdwwdk', '.kddk.', '..kk..'], { k: C.ink, d: C.d2, w: C.white });
  return L;
}
function drawBackParts() {
  const L = newLayer();
  // Far wing, swept back above the heel, in shadow.
  shape(L, (x, y) => inPoly(x, y, [[66, 79], [44, 79], [4, 68], [17, 66]]), (x, y) => (y < 70 ? C.m2 : C.m1));
  // Vertical tail fin rising from the heel.
  shape(L, (x, y) => inPoly(x, y, [[58, 74], [37, 74], [19, 31], [35, 31]]), (x, y) => {
    const lead = 35 + (y - 31) * (58 - 35) / 43; // leading edge x at this row
    return x > lead - 3 ? C.m4 : y < 40 ? C.d2 : C.m2;
  });
  // Olive band near the fin tip.
  for (let y = 35; y <= 37; y++) for (let x = 0; x < W; x++) { const c = get(L, x, y); if (c && c !== C.ink) put(L, x, y, C.g2); }
  return L;
}
const FLAMES = { Idle: [6, 7, 6, 8], Fly: [10, 13, 11, 14, 12, 10], Shoot: [9, 10, 9, 11, 10], Special: [10, 12, 13, 14, 12, 11, 10], Hurt: [4, 7, 3, 6] };
function drawEngine(flame, phase) {
  const L = newLayer();
  // Nozzle at the heel (replaces the tiny v01 exhaust).
  shape(L, (x, y) => inPoly(x, y, [[34, 79], [34, 91], [20, 92], [20, 78]]), (x, y) => (y < 82 ? C.m4 : y < 87 ? C.m2 : C.m1));
  for (let y = 80; y <= 90; y++) put(L, 21, y, C.m0);
  // Flame: outer orange, inner yellow, cream core; length varies per frame.
  for (let y = 80; y <= 90; y++) {
    const half = 5 - Math.abs(y - 85), len = Math.max(0, Math.round(flame * (half + 1) / 6) - ((y + phase) % 2));
    for (let i = 1; i <= len; i++) {
      const t = i / Math.max(1, len);
      put(L, 20 - i, y, half >= 3 && t < .45 ? C.cream : half >= 2 && t < .75 ? C.fireY : C.fireO);
    }
  }
  return L;
}
function shoeHoles(shoe) {
  // Connected dark clusters on the shoe top (the v01 ventilation holes).
  const seen = new Set(), holes = [];
  const dark = (x, y) => { const c = get(shoe, x, y); return c && c[0] < 40 && c[1] < 40 && c[2] < 40; };
  for (let y = 72; y <= 86; y++) for (let x = 70; x <= 120; x++) {
    if (!dark(x, y) || seen.has(y * W + x)) continue;
    const stack = [[x, y]], pts = [];
    while (stack.length) { const [a, b] = stack.pop(); if (seen.has(b * W + a) || !dark(a, b) || b < 72 || b > 86) continue; seen.add(b * W + a); pts.push([a, b]); stack.push([a + 1, b], [a - 1, b], [a, b + 1], [a, b - 1]); }
    if (pts.length >= 5 && pts.length <= 24) holes.push([Math.round(pts.reduce((s, p) => s + p[0], 0) / pts.length), Math.round(pts.reduce((s, p) => s + p[1], 0) / pts.length)]);
  }
  return holes.sort((a, b) => a[0] - b[0]);
}

// v02.2: like the original airplane, the croc body ends on its own ink outline and the
// vehicle rim starts right below. Enclosed gaps under the body become the clog opening rim.
const isFuselage = c => { const mx = Math.max(...c), mn = Math.min(...c); return (mx - mn < 20 && mx > 50) || (mx - mn < 14 && mx >= 38) || (c[0] > 195 && c[1] > 185 && c[2] > 140 && mx - mn < 70); };
function fillNeckHole(L) {
  const outside = new Uint8Array(W * H), stack = [];
  for (let x = 0; x < W; x++) stack.push(x, (H - 1) * W + x);
  for (let y = 0; y < H; y++) stack.push(y * W, y * W + W - 1);
  while (stack.length) { const i = stack.pop(); if (outside[i] || L[i]) continue; outside[i] = 1; const x = i % W, y = (i / W) | 0; if (x > 0) stack.push(i - 1); if (x < W - 1) stack.push(i + 1); if (y > 0) stack.push(i - W); if (y < H - 1) stack.push(i + W); }
  const hole = [];
  for (let i = 0; i < W * H; i++) if (!L[i] && !outside[i]) hole.push(i);
  const set = new Set(hole);
  for (const i of hole) {
    let k = 0; while (set.has(i - (k + 1) * W)) k++;
    const lastRow = !set.has(i + W);
    L[i] = k === 0 ? C.ink : k === 1 ? C.g5 : k === 2 ? C.g4 : lastRow ? C.g2 : C.g3;
  }
  return hole.length;
}

async function readLayer(file) {
  const { data } = await sharp(file).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
  const L = newLayer();
  for (let i = 0; i < W * H; i++) if (data[i * 4 + 3] > 0) L[i] = [data[i * 4], data[i * 4 + 1], data[i * 4 + 2]];
  return L;
}
const toPng = L => { const b = Buffer.alloc(W * H * 4); L.forEach((c, i) => { if (c) { b[i * 4] = c[0]; b[i * 4 + 1] = c[1]; b[i * 4 + 2] = c[2]; b[i * 4 + 3] = 255; } }); return sharp(b, { raw: { width: W, height: H, channels: 4 } }).png().toBuffer(); };
const hash = b => crypto.createHash('sha256').update(b).digest('hex');

async function main() {
  fs.mkdirSync(source, { recursive: true }); fs.mkdirSync(preview, { recursive: true });
  // Shoe: v01 hull minus the tiny heel exhaust (the new nozzle replaces it), plus war jibbitz.
  const shoe = await readLayer(path.join(v01, 'shoe-native.png'));
  for (let y = 78; y <= 92; y++) for (let x = 0; x < 33; x++) shoe[y * W + x] = null;
  // v02.6: under the croc body the clog side wall rises to the opening rim (row 65, where the
  // airplane's cream cockpit rim was). One highlight row, then the same rubber color as the side
  // below, with no separating black line, so it reads as the shoe and not as a separate bar.
  for (let x = 49; x <= 75; x++) {
    for (let y = 65; y <= 73; y++) shoe[y * W + x] = y === 65 ? C.g5 : C.g4;
  }
  const holes = shoeHoles(shoe);
  if (holes.length < 4) throw new Error('Expected the v01 ventilation holes, found ' + holes.length);
  const shoeJibbitz = ['medal', 'bullet', 'star'];
  [1, 3, holes.length - 1].forEach((h, n) => stamp(shoe, holes[h][0], holes[h][1], JIBBITZ[shoeJibbitz[n]].rows, JIBBITZ[shoeJibbitz[n]].map));
  const helmet = drawHelmet(), nearWing = drawNearWing(), back = drawBackParts();
  fs.writeFileSync(path.join(source, 'helmet-native.png'), await toPng(helmet));
  fs.writeFileSync(path.join(source, 'jet-shoe-native.png'), await toPng(under(over(over(newLayer(), shoe), nearWing), back)));

  const base = await readLayer(path.join(root, 'Assets/Art/Player/Bombardiro/Idle/bombardiro_idle_0.png'));
  const manifest = { version: 2, mode: 'native-grid pixel art drawn by code over the v01 hull and production pilot cels', canvas: [W, H], ppu: 100, pivot: [.45, 1 / 3], shoeHoles: holes, frames: [] };
  const sheet = [];
  for (const state of ['Idle', 'Fly', 'Shoot', 'Special', 'Hurt']) {
    const originalDir = path.join(root, 'Assets/Art/Player/Bombardiro', state), outputDir = path.join(root, 'Assets/Art/Player/Crocs', state);
    fs.mkdirSync(outputDir, { recursive: true });
    const names = fs.readdirSync(originalDir).filter(n => n.endsWith('.png')).sort();
    for (let f = 0; f < names.length; f++) {
      const name = names[f], input = fs.readFileSync(path.join(originalDir, name));
      const original = await readLayer(path.join(originalDir, name));
      // Vertical hull offset (v01 rule) and exact head offset (helmet silhouette match).
      let top = H;
      for (let y = 0; y < 40; y++) for (let x = 44; x < 130; x++) if (original[y * W + x]) top = Math.min(top, y);
      const dy = top - 12;
      let best = [-1e9, 0, 0];
      for (let hy = -5; hy <= 5; hy++) for (let hx = -6; hx <= 6; hx++) {
        let s = 0;
        for (let y = 8; y < 36; y++) for (let x = 44; x < 104; x++) s += !!get(base, x, y) === !!get(original, x + hx, y + hy) ? 1 : -1;
        if (s > best[0]) best = [s, hx, hy];
      }
      const [, hx, hy] = best;
      // Pilot pixels without the old helmet (which the new one replaces).
      // v02.5: the junction is the ORIGINAL airplane's. In each column, every original pixel above
      // the first fuselage pixel is kept (body, jaw, its black shadow), exactly as drawn; only the
      // fuselage and below is replaced by the clog, which sits at almost the same height.
      const fusTop = new Array(W).fill(-1);
      for (let x = 40; x <= 132; x++) for (let y = 0; y < H; y++) {
        const sy = y - dy, c = original[y * W + x];
        if (sy >= (x < 62 ? 55 : 62) && sy < 90 && c && isFuselage(c)) { fusTop[x] = y; break; }
      }
      const shoeLayer = shift(shoe, 0, dy), shoeTop = new Array(W).fill(-1);
      for (let x = 0; x < W; x++) for (let y = 0; y < H; y++) if (shoeLayer[y * W + x]) { shoeTop[x] = y; break; }
      const pilot = newLayer();
      for (let y = 0; y < H; y++) for (let x = 0; x < W; x++) {
        const c = original[y * W + x];
        if (!c) continue;
        if (c[2] > c[0] + 40 && c[2] >= c[1]) continue; // stray blue/cyan charge sparks from the airplane special
        const ux = x - hx, uy = y - hy;
        if (uy <= 33 && ux >= 44 && ux <= 104) continue; // old helmet, replaced by the Crocs helmet
        const ft = fusTop[x];
        const aboveFuselage = ft >= 0 ? y < ft : true;
        const inJunction = x >= 40 && x <= 132 && ft >= 0 && y - dy >= 45;
        if (!aboveFuselage || !(pilotMask(x, y - dy) || inJunction)) continue;
        if (y - dy >= 57 && isFuselage(c) && x <= 80) continue;
        pilot[y * W + x] = c;
      }
      // Where no clog sits under the head (snout tip past the toe), keep only the jaw's own outline,
      // so the airplane's shadow does not hang in the air.
      for (let x = 60; x <= 132; x++) {
        if (shoeTop[x] >= 0 && fusTop[x] >= 0 && shoeTop[x] - fusTop[x] <= 3) continue;
        let jb = -1;
        for (let y = 0; y < H; y++) { const sy = y - dy, c = pilot[y * W + x]; if (sy >= 40 && sy <= 74 && c && Math.max(...c) >= 40) jb = y; }
        if (jb >= 0) for (let y = jb + 2; y < H; y++) if (y - dy <= 78) pilot[y * W + x] = null;
      }
      // Muzzle-flash remnants in front of the snout are small groups not attached to the head.
      { const seen = new Uint8Array(W * H);
        for (let i = 0; i < W * H; i++) {
          if (!pilot[i] || seen[i] || (i % W) < 110) continue;
          const group = [], stack = [i]; let attached = false; seen[i] = 1;
          while (stack.length) { const j = stack.pop(); group.push(j); const jx = j % W;
            if (jx < 110) attached = true;
            for (const d of [-1, 1, -W, W, -W - 1, -W + 1, W - 1, W + 1]) { const k = j + d; if (k < 0 || k >= W * H || seen[k] || !pilot[k]) continue; if (Math.abs((k % W) - jx) > 1) continue; seen[k] = 1; stack.push(k); } }
          if (!attached && group.length < 40) for (const j of group) pilot[j] = null;
        } }
      const frame = over(shoeLayer, pilot);
      // 1-3 row slits between the original shadow and the clog top continue the shadow, as on the airplane.
      for (let x = 40; x <= 132; x++) {
        if (fusTop[x] < 0 || shoeTop[x] < 0 || shoeTop[x] <= fusTop[x] || shoeTop[x] - fusTop[x] > 3) continue;
        for (let y = fusTop[x]; y < shoeTop[x]; y++) if (!frame[y * W + x] && frame[(y - 1) * W + x]) frame[y * W + x] = C.ink;
      }
      const filled = fillNeckHole(frame);
      over(frame, shift(helmet, hx, hy));
            over(frame, shift(nearWing, 0, dy));
      over(frame, shift(drawEngine(FLAMES[state][f % FLAMES[state].length], f), 0, dy));
            under(frame, shift(back, 0, dy));
      const png = await toPng(frame);
      fs.writeFileSync(path.join(outputDir, name), png);
      const n = manifest.frames.length;
      sheet.push({ input: await sharp(png).resize(480, 360, { kernel: 'nearest' }).toBuffer(), left: (n % 6) * 480, top: Math.floor(n / 6) * 360 });
      manifest.frames.push({ state, name, originalSha256: hash(input), sha256: hash(png), offsetY: dy, head: [hx, hy], filledNeckPixels: filled });
      if (state === 'Idle' && f === 0) await sharp(png).resize(960, 720, { kernel: 'nearest' }).toFile(path.join(preview, 'crocs-jet-idle-x6.png'));
    }
  }
  if (manifest.frames.length !== 26) throw new Error('Expected 26 production frames');
  fs.writeFileSync(path.join(source, 'manifest.json'), JSON.stringify(manifest, null, 2) + '\n');
  await sharp({ create: { width: 2880, height: 1800, channels: 4, background: '#253044' } }).composite(sheet).png().toFile(path.join(preview, 'crocs-jet-frames.png'));
  console.log('CROCS_JET_V02_OK: 26 frames, ' + holes.length + ' shoe holes, helmet head offsets matched.');
}
main().catch(e => { console.error(e); process.exitCode = 1; });
