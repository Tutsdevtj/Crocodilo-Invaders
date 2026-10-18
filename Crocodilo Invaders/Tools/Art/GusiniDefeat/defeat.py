# GUSINI — derrota final: o parasita é destruído e o Gusini volta à Fase 1 (exausto, vivo).
# Mesmo canvas/pivô de tudo do boss: 256x196, pivô (120,118). Recompõe as peças aprovadas da F1 e F2.
import os, json, sys
sys.path.insert(0, '/home/claude/proj/transform')
import numpy as np
from PIL import Image
from scipy import ndimage
import transform as T
from transform import W, H, A0, A1, A2, A3, WHITE, OUTL, MODULE_C, EXH

OUT = '/home/claude/proj/transform/Gusini_Defeat_F2_F1'
for d in ('frames', 'layers/sprite', 'layers/fx', 'fx', 'final', 'previews'):
    os.makedirs(f'{OUT}/{d}', exist_ok=True)
CHT = [(28, 26, 34), (48, 46, 58), (74, 72, 86)]          # carapaça do parasita (detritos)
SOOT = [(40, 38, 48), (62, 60, 72)]
yy, xx = np.mgrid[0:H, 0:W]
F1 = {'*': 1}; F2 = {'*': 2}

# ---------- variantes derivadas das peças aprovadas ----------
def tired_head():
    h = T.P[1]['head'].copy()
    rgb = h[..., :3].astype(int); a = h[..., 3] > 0
    eye = (xx >= 80) & (xx <= 94) & (yy >= 35) & (yy <= 45) & a
    white = eye & (rgb.sum(-1) > 600)
    # pálpebra pesada: tudo do olho acima de y=40 vira pálpebra cinza
    lid = white & (yy < 40)
    h[lid, :3] = (190, 194, 210)
    h[lid & (yy == 36), :3] = (232, 234, 240)
    # linha da pálpebra (escura) logo acima da parte visível
    line = eye & (yy == 40) & (xx >= 82) & (xx <= 93)
    h[line, :3] = OUTL
    # pupila caída (olhar para baixo)
    for (x, y) in ((86, 41), (87, 41), (86, 42), (87, 42)):
        h[y, x, :3] = OUTL
    # gota de suor
    for (x, y, c) in ((97, 31, (150, 210, 250)), (97, 32, (150, 210, 250)), (96, 32, (220, 245, 255)), (97, 33, (90, 160, 220))):
        h[y, x, :3] = c; h[y, x, 3] = 255
    return h


N4 = np.array([[0, 1, 0], [1, 1, 1], [0, 1, 0]])


def _reoutline(part, removed):
    """depois de arrancar pixels (lascas), a nova borda vira contorno escuro"""
    a = part[..., 3] > 0
    part[removed] = 0; a &= ~removed
    edge = a & ndimage.binary_dilation(removed, structure=N4)
    part[edge, :3] = OUTL
    lab, n = ndimage.label(part[..., 3] > 0, structure=np.ones((3, 3)))
    if n > 1:
        sizes = ndimage.sum(part[..., 3] > 0, lab, range(1, n + 1))
        for i, sz in enumerate(sizes):
            if sz <= 6: part[lab == i + 1] = 0


def _line(part, pts, col, only=None):
    a = part[..., 3] > 0
    for (x0, y0), (x1, y1) in zip(pts[:-1], pts[1:]):
        n = int(max(abs(x1 - x0), abs(y1 - y0))) + 1
        for t in np.linspace(0, 1, n):
            x, y = int(round(x0 + (x1 - x0) * t)), int(round(y0 + (y1 - y0) * t))
            if a[y, x] and not np.all(part[y, x, :3] == OUTL): part[y, x, :3] = col


def _crack(part, pts):
    _line(part, pts, OUTL)
    _line(part, [(x + 1, y) for x, y in pts], (186, 150, 220))       # aresta clara da rachadura


def _scorch(part, cx, cy, rx, ry):
    a = (part[..., 3] > 0) & ~np.all(part[..., :3] == OUTL, -1)
    d = ((xx - cx) / rx) ** 2 + ((yy - cy) / ry) ** 2
    part[a & (d < 1) & ((xx + yy) % 2 == 0), :3] = SOOT[1]
    part[a & (d < 0.45), :3] = SOOT[1]; part[a & (d < 0.45) & ((xx + yy) % 2 == 0), :3] = SOOT[0]


def _dent(part, cx, cy, r):
    a = (part[..., 3] > 0) & ~np.all(part[..., :3] == OUTL, -1)
    d = np.hypot(xx - cx, (yy - cy) * 1.3)
    arc = a & (d < r) & (d >= r - 1.2) & (yy < cy - 1) & (np.abs(xx - cx) < r * 0.8); part[arc, :3] = (60, 36, 92)       # sombra da amassada


GREEN_RES = [A3, (30, 120, 70), A2]          # resíduo do alien morto (escuro, não brilha)


def _splat(part, x, y, big=False):
    a = (part[..., 3] > 0) & ~np.all(part[..., :3] == OUTL, -1)
    pts = [(x, y), (x + 1, y)] + ([(x, y + 1), (x - 1, y), (x + 1, y + 1)] if big else [])
    for (X, Y) in pts:
        if a[Y, X]: part[Y, X, :3] = GREEN_RES[0] if (X + Y) % 2 else GREEN_RES[1]


def damaged_parts():
    P = {k: v.copy() for k, v in T.P[1].items()}
    b = P['body']
    # --- cratera onde o parasita estourou (base do leme): mordida na silhueta + pétalas de metal para fora ---
    rng = np.random.default_rng(7)
    jag = 1.0 + 0.25 * np.sin(np.arctan2(yy - 96, xx - 169) * 7)
    bite = ((((xx - 169) / 10.5) ** 2 + ((yy - 95) / 5.5) ** 2) < jag) & (yy < 100)
    _reoutline(b, bite & (b[..., 3] > 0))
    ab = (b[..., 3] > 0)
    cr = ab & ((((xx - 169) / 11.0) ** 2 + ((yy - 102) / 5.5) ** 2) < jag) & ~np.all(b[..., :3] == OUTL, -1)
    b[cr, :3] = SOOT[0]
    rim = ab & ((((xx - 169) / 15.0) ** 2 + ((yy - 104) / 8.0) ** 2) < 1) & ~cr & ~np.all(b[..., :3] == OUTL, -1)
    b[rim & ((xx + yy) % 2 == 0), :3] = SOOT[1]; b[rim & ((xx + yy) % 2 == 1), :3] = (92, 60, 120)
    # pétalas de metal rasgado na borda da mordida (lado claro = pra fora)
    for (px_, py_, ddx, ddy) in ((158, 98, -3, -2), (163, 96, -1, -3), (175, 96, 1, -3), (180, 99, 3, -2)):
        for k in range(3):
            X, Y = px_ + int(round(ddx * k / 2)), py_ + int(round(ddy * k / 2))
            b[Y, X, :3] = (186, 150, 220) if k < 2 else OUTL; b[Y, X, 3] = 255
            b[Y + 1, X, :3] = OUTL; b[Y + 1, X, 3] = 255
    pool = ab & ((((xx - 169) / 7.0) ** 2 + ((yy - 103) / 2.8) ** 2) < 1)
    b[pool, :3] = A3; b[pool & (yy == 103) & (np.abs(xx - 169) < 4), :3] = (30, 120, 70)
    b[103, 167, :3] = A2                                            # único brilho residual (fraco)
    for (dx_, ln) in ((-6, 4), (-1, 7), (3, 4), (7, 6)):            # escorridos verdes descendo da cratera
        for k in range(ln):
            X, Y = 169 + dx_, 106 + k
            if ab[Y, X] and not np.all(b[Y, X, :3] == OUTL): b[Y, X, :3] = A3 if k < ln - 1 else (30, 120, 70)
    for ang in np.linspace(-0.4, np.pi + 0.4, 9):                    # borrifo radial (o estouro espirrou)
        r0 = rng.uniform(13, 16); ln = rng.integers(2, 5)
        for k in range(ln):
            X = int(round(169 + np.cos(ang) * (r0 + k * 1.6))); Y = int(round(103 + np.sin(ang) * (r0 + k * 1.6) * 0.55 + 4))
            if 0 <= Y < H and ab[Y, X] and not np.all(b[Y, X, :3] == OUTL): b[Y, X, :3] = A3 if k else (30, 120, 70)
    for (x, y, big) in ((150, 101, True), (186, 109, False), (156, 113, False), (190, 95, True), (143, 107, False), (179, 117, True), (197, 87, False), (201, 104, False)):
        _splat(b, x, y, big)
    # rastro de fuligem para trás da cratera
    for k in range(10):
        X = 183 + k; Y0 = 100 + k // 3
        for Y in (Y0, Y0 + 1):
            if ab[Y, X] and not np.all(b[Y, X, :3] == OUTL) and (X + Y) % 2 == 0: b[Y, X, :3] = SOOT[1]
    # painel arrancado (fiação aparecendo)
    pan = ab & (xx >= 138) & (xx <= 147) & (yy >= 103) & (yy <= 108) & ~np.all(b[..., :3] == OUTL, -1)
    b[pan, :3] = SOOT[0]; b[pan & ((xx == 138) | (yy == 103)), :3] = OUTL
    b[pan & (yy == 108), :3] = (92, 60, 120)
    for (X, Y, c) in ((141, 105, (226, 70, 60)), (142, 106, (226, 70, 60)), (144, 105, (252, 214, 72)), (145, 106, (252, 214, 72))): b[Y, X, :3] = c
    # --- danos estruturais ---
    _crack(b, [(118, 104), (121, 108), (119, 111), (124, 116), (122, 120)])
    _crack(b, [(84, 112), (88, 116), (86, 120)])
    _crack(b, [(196, 70), (199, 76), (197, 80), (200, 86)])
    _line(b, [(96, 103), (106, 106)], (186, 150, 220)); _line(b, [(98, 105), (104, 107)], (186, 150, 220))     # arranhões
    _line(b, [(138, 120), (148, 124)], (186, 150, 220)); _line(b, [(62, 108), (70, 111)], (186, 150, 220))
    _dent(b, 100, 116, 4.5); _dent(b, 150, 116, 3.5)
    _scorch(b, 128, 112, 5, 4); _scorch(b, 78, 124, 4, 3); _scorch(b, 192, 112, 5, 6)
    # leme lascado (canto de cima) e entalhe na traseira
    chip = (xx >= 199) & (yy <= 74) & (xx + (yy - 62) * 0.9 >= 207) & ~(((xx - 205) % 4 == 0) & (yy == 70))
    _reoutline(b, chip & (b[..., 3] > 0))
    notch = (((xx - 212) / 3.0) ** 2 + ((yy - 100) / 4.0) ** 2 < 1)
    _reoutline(b, notch & (b[..., 3] > 0))
    # --- asa de trás: ponta rasgada + queimado + respingo ---
    wf = P['wing_far']
    tear = (xx <= 147) & (yy <= 79) & ((147 - xx) + (79 - yy) > 6)
    _reoutline(wf, tear & (wf[..., 3] > 0))
    _scorch(wf, 160, 86, 5, 4); _splat(wf, 170, 90, True); _crack(wf, [(176, 80), (172, 86), (175, 92)])
    # --- asa da frente: ponta entortada 2 px para baixo (assimetria), furo, borda lascada ---
    wn = P['wing_near']
    tip = (xx >= 186) & (wn[..., 3] > 0)
    moved = np.zeros_like(wn); moved[2:][tip[:-2]] = wn[:-2][tip[:-2]]
    wn[tip] = 0; T.over(wn, moved)
    hole = (((xx - 160) / 2.2) ** 2 + ((yy - 147) / 1.6) ** 2 < 1) & (wn[..., 3] > 0)
    wn[hole, :3] = OUTL; wn[ndimage.binary_dilation(hole, structure=N4) & ~hole & (wn[..., 3] > 0) & (yy < 147), :3] = SOOT[1]
    lip = (((xx - 132) / 4.0) ** 2 + ((yy - 125) / 2.0) ** 2 < 1)
    _reoutline(wn, lip & (wn[..., 3] > 0))
    torn = (xx >= 196) & (yy >= 150) & ((xx - 196) + (yy - 150) * 0.8 > 6) & ~(((xx + yy) % 3 == 0) & (xx < 203))
    _reoutline(wn, torn & (wn[..., 3] > 0))
    _crack(wn, [(176, 136), (180, 141), (178, 146)]); _splat(wn, 150, 132, True); _splat(wn, 186, 146, False)
    _scorch(wn, 196, 150, 5, 4)
    # --- motor: bocal chamuscado e rachado ---
    en = P['engine']
    _scorch(en, 222, 120, 7, 12); _crack(en, [(208, 108), (211, 114), (209, 120)])
    # --- canhão: cano chamuscado, amassado e 1 px caído ---
    cn = P['cannon']
    _scorch(cn, 30, 142, 7, 6); _dent(cn, 50, 140, 3.5)
    drop = (xx < 40) & (cn[..., 3] > 0)
    mv = np.zeros_like(cn); mv[1:][drop[:-1]] = cn[:-1][drop[:-1]]
    cn[drop] = 0; T.over(cn, mv)
    return P


P1X = damaged_parts(); P1X['head'] = tired_head()


def pose(form, dx=0, dy=0, tilt=0.0, head=(0, 0), jitter=None, droop=0, skip=(), tired=False):
    img = np.zeros((H, W, 4), np.uint8)
    for k in T.ORDER:
        if k in skip or k.startswith('fx_'): continue
        ph = form.get(k, form.get('*', 1))
        src = (P1X if (tired and ph == 1) else T.P[ph])
        if k not in src: continue
        cx = T.CX[(ph, k)]; ty = int(round(tilt * (130 - cx) / 80))
        ox, oy = dx, dy + ty
        if k in T.HEADG: ox += head[0]; oy += head[1]
        if k == 'scarf': oy += droop
        if jitter and k in jitter: ox += jitter[k][0]; oy += jitter[k][1]
        T.over(img, T.shift(src[k], ox, oy))
    return img


def alien_mask(img):
    m = np.zeros((H, W), bool)
    for c in (A0, A1, A2, A3): m |= np.all(img[..., :3] == c, -1)
    return m & (img[..., 3] > 0)


def glow(img, mode):
    """mode 'fail': verde apaga (A0/A1 -> A2/A3); 'surge': verde estoura (A2/A3 -> A1/A0); 'dead': tudo A3/cinza"""
    o = img.copy(); rgb = img[..., :3]
    m = {c: np.all(rgb == c, -1) & (img[..., 3] > 0) for c in (A0, A1, A2, A3)}
    if mode == 'fail':
        o[m[A0], :3] = A2; o[m[A1], :3] = A3; o[m[A2], :3] = A3
    elif mode == 'surge':
        o[m[A2], :3] = A1; o[m[A3], :3] = A2; o[m[A1], :3] = A0
    elif mode == 'dead':
        o[m[A0] | m[A1], :3] = A3; o[m[A2] | m[A3], :3] = SOOT[0]
    return o


def module_white(img, dx, dy, tilt):
    cx = T.CX[(2, 'module')]; ty = int(round(tilt * (130 - cx) / 80))
    part = T.shift(T.P[2]['module'], dx, dy + ty); m = part[..., 3] > 0
    o = img.copy(); outl = np.all(part[..., :3] == OUTL, -1)
    o[m & ~outl, :3] = WHITE; o[m & ~outl & ~ndimage.binary_erosion(m & ~outl), :3] = A0
    return o


def debris(o, cx, cy, t, n=9, seed=5):
    """pedaços escuros da carapaça voando (o parasita se desfaz)"""
    rng = np.random.default_rng(seed)
    for i in range(n):
        a = rng.uniform(-np.pi * 0.95, -np.pi * 0.05) if i % 3 else rng.uniform(0, 2 * np.pi)
        v = rng.uniform(16, 30)
        x = cx + np.cos(a) * v * t; y = cy + np.sin(a) * v * t + 22 * t * t
        s = 2 if i % 2 else 1
        for ddx in range(s + 1):
            for ddy in range(s):
                X, Y = int(x) + ddx, int(y) + ddy
                if 0 <= X < W and 0 <= Y < H: o[Y, X, :3] = CHT[1] if (ddx + ddy) else CHT[2]; o[Y, X, 3] = 255
        X, Y = int(x) - 1, int(y)
        if 0 <= X < W and 0 <= Y < H and i % 2: o[Y, X, :3] = A1; o[Y, X, 3] = 255     # brasa verde


def alien_smoke(o, cx, cy, age, n=4, seed=0):
    """fumaça verde-escura do parasita se dissipando (sobe e some em pontilhado)"""
    rng = np.random.default_rng(seed)
    for i in range(n):
        a = min(1.0, age + i * 0.08)
        px = cx + rng.uniform(-10, 10) + 6 * a; py = cy - 4 - 22 * a + rng.uniform(-3, 3)
        r = 3 + 6 * a
        m = T.disk(px, py, r)
        if a > 0.55: m &= (xx + yy) % 2 == 0
        if a > 0.85: m &= (xx % 2 == 0)
        T.paint(o, m, A3 if a < 0.6 else (40, 64, 56))
        T.paint(o, T.disk(px - r * 0.3, py - r * 0.3, r * 0.35) & m & (a < 0.45), A2)


def pop(o, cx, cy, ph):
    """estouro do parasita: núcleo branco, anel verde, halo pontilhado e 8 raios (ph 0 = pico, 1 = se partindo)"""
    d = np.sqrt((xx - cx) ** 2 + ((yy - cy) / 0.85) ** 2)
    ang = np.arctan2(yy - cy, xx - cx)
    if ph == 0:
        T.paint(o, (d < 21) & (d >= 14) & ((xx + yy) % 2 == 0), A1)
        T.paint(o, (d < 14) & (d >= 9), A0)
        T.paint(o, d < 9, WHITE)
        for k in range(8):
            a = k * np.pi / 4 + 0.2
            for r in range(10, 28 if k % 2 == 0 else 20):
                X, Y = int(round(cx + np.cos(a) * r)), int(round(cy + np.sin(a) * r * 0.85))
                if 0 <= X < W and 0 <= Y < H:
                    o[Y, X, :3] = WHITE if r < 16 else A0; o[Y, X, 3] = 255
                    if r < 18 and 0 <= X + 1 < W: o[Y, X + 1, :3] = A0; o[Y, X + 1, 3] = 255
    else:
        T.paint(o, (d < 25) & (d >= 21), A1); T.paint(o, (d < 21) & (d >= 19) & ((xx + yy) % 2 == 0), A2)
        T.paint(o, d < 5, A0); T.paint(o, d < 2.5, WHITE)
        for k in range(8):
            a = k * np.pi / 4 + 0.2
            for r in range(26, 33):
                if r % 3 == 0: continue
                X, Y = int(round(cx + np.cos(a) * r)), int(round(cy + np.sin(a) * r * 0.85))
                if 0 <= X < W and 0 <= Y < H: o[Y, X, :3] = A1; o[Y, X, 3] = 255


# ---------- ROTEIRO ----------
S = []


def add(dur, kind, kw, fx=None, tag=None, form=None, skip=(), mode=None, tired=False, cor=None):
    S.append(dict(dur=dur, kind=kind, kw=kw, fx=fx or [], tag=tag, form=form, skip=skip, mode=mode, tired=tired, cor=cor))


J = lambda s: {k: (int(v[0]), int(v[1])) for k, v in zip(['head', 'body', 'cannon', 'wing_near', 'engine', 'module', 'scarf'], np.random.default_rng(s).integers(-1, 2, (7, 2)))}
MC = MODULE_C
# A) HP zera: golpe, cambaleio, brilho falha, núcleo pulsa descontrolado
add(60, 'p2', dict(dx=2), mode='white', fx=[('hit', 7)], tag='defeat_hit')
add(90, 'p2', dict(dx=3, tilt=-2, head=(1, -1), jitter=J(1)), mode='fail', fx=[('hit', 4), ('sputter',)], tag='stagger_start')
add(80, 'p2', dict(dx=1, dy=2, tilt=2, jitter=J(2)), mode='surge', fx=[('aura_mod', 2), ('bolts', 3, 10)], tag='core_unstable')
add(70, 'p2', dict(dx=0, dy=3, tilt=3, jitter=J(3)), mode='fail', fx=[('sputter',)])
add(70, 'p2', dict(dx=1, dy=3, tilt=2, jitter=J(4)), mode='surge', fx=[('aura_mod', 3), ('bolts', 5, 14), ('ring_mod', 9)])
add(60, 'p2', dict(dx=0, dy=4, tilt=3, jitter=J(5)), mode='dead', fx=[('bolts', 2, 6)])
add(60, 'p2', dict(dx=0, dy=4, tilt=3, jitter=J(6)), mode='core_white', fx=[('aura_mod', 3), ('bolts', 6, 16)], tag='core_critical')
# B) o parasita explode (efeito localizado no módulo) e é expelido
add(60, 'p2', dict(dy=4, tilt=3), skip=('module',), mode='dead', fx=[('pop', 0)], tag='parasite_burst')
add(70, 'p2', dict(dy=4, tilt=3), skip=('module',), mode='dead', fx=[('pop', 1), ('debris', 0.25), ('asmoke', 0.0)])
add(60, 'p2', dict(dy=4, tilt=3), skip=('module',), mode='dead', fx=[('ring_mod', 30, True), ('debris', 0.4), ('asmoke', 0.12)])
# C) a corrupção se dissipa: primeiro o rosto, depois o corpo
add(80, 'mid2', dict(dy=4, tilt=3), skip=('module',), mode='dead', fx=[('debris', 0.5), ('asmoke', 0.25), ('motes', 0.1)], tag='face_recovered')
add(40, 'mid2', dict(dy=4, tilt=3), skip=('module',), mode='white')
add(90, 'p1', dict(dy=5, tilt=4, droop=1, head=(0, 1)), cor=24, fx=[('debris', 0.7), ('asmoke', 0.5), ('motes', 0.3)], tag='corruption_fading', tired=True)
add(100, 'p1', dict(dy=5, tilt=4, droop=2, head=(0, 2)), cor=10, fx=[('asmoke', 0.75), ('motes', 0.6), ('smoke', 0.1, 2)], tired=True)
add(110, 'p1', dict(dy=6, tilt=5, droop=2, head=(0, 2)), fx=[('asmoke', 0.95), ('smoke', 0.35, 3), ('sputter',)], tag='gusini_recovered', tired=True)
# D) pós-batalha: exausto, cai um pouco e fica
add(140, 'p1', dict(dy=7, tilt=6, droop=3, head=(0, 3)), fx=[('smoke', 0.6, 3)], tired=True)
add(160, 'p1', dict(dy=8, tilt=6, droop=3, head=(0, 3)), fx=[('smoke', 0.85, 3), ('sputter',)], tired=True)
add(500, 'p1', dict(dy=7, tilt=5, droop=3, head=(0, 3)), fx=[('smoke', 0.1, 3)], tag='defeated_hold', tired=True)

MID2 = {'*': 2, 'head': 1, 'beak_lower': 1, 'beak_upper': 1, 'scarf': 1}


def render(i, s):
    kw = dict(s['kw'])
    form = {'p2': F2, 'p1': F1, 'mid2': MID2}[s['kind']]
    spr = pose(form, skip=s['skip'], tired=s['tired'], **kw)
    dx, dy, tilt = kw.get('dx', 0), kw.get('dy', 0), kw.get('tilt', 0)
    cor = np.zeros((H, W, 4), np.uint8)
    if s['cor']:
        p2 = pose(F2, dx, dy, tilt, skip=('module',))
        cor = T.corruption_overlay(spr, glow(p2, 'fail'), s['cor'])
        cor[..., :3] = np.where(np.all(cor[..., :3] == A3, -1)[..., None], np.array(SOOT[0]), cor[..., :3])
    m = s['mode']
    if m in ('fail', 'surge', 'dead'): spr = glow(spr, m)
    if m == 'core_white': spr = module_white(glow(spr, 'surge'), dx, dy, tilt)
    if m == 'white': full = spr.copy(); T.over(full, cor); spr = T.silhouette(full, WHITE); cor[:] = 0
    T.over(spr, cor)
    body = T.mask(spr)
    mc = (MC[0] + dx, MC[1] + dy + int(round(tilt * (130 - T.CX[(2, 'module')]) / 80)))
    fx = np.zeros((H, W, 4), np.uint8)
    for e in s['fx']:
        if e[0] == 'aura_mod':
            mm = np.zeros((H, W), bool)
            part = T.shift(T.P[2]['module'], dx, mc[1] - MC[1]); mm = part[..., 3] > 0
            T.over(fx, T.aura(mm, e[1], i))
    for e in s['fx']:
        if e[0] == 'hit': T.hit_star(fx, T.HIT[0] + 40 + dx, T.HIT[1] + dy, e[1])
        if e[0] == 'sputter': T.sputter(fx, EXH[0] + dx, EXH[1] + dy, i)
        if e[0] == 'bolts': T.bolts(fx, mc[0], mc[1], e[1], e[2], i + 3)
        if e[0] == 'ring_mod': T.ring(fx, mc[0], mc[1], e[1], 2, dither=len(e) > 2)
        if e[0] == 'burst': T.burst(fx, mc[0], mc[1], e[1])
        if e[0] == 'pop': pop(fx, mc[0], mc[1], e[1])
        if e[0] == 'shards': T.shards(fx, mc[0], mc[1], e[1], n=8, seed=i)
        if e[0] == 'debris': debris(fx, mc[0], mc[1], e[1])
        if e[0] == 'asmoke': alien_smoke(fx, mc[0], mc[1], e[1], seed=11)
        if e[0] == 'motes': T.motes(fx, e[1], body & (np.abs(xx - mc[0]) < 40), n=8, seed=i)
        if e[0] == 'smoke':
            T.smoke_puffs(fx, EXH[0] + dx - 4, EXH[1] + dy - 2, e[1], e[2] + 1, seed=i)
            if s['tired']: T.smoke_puffs(fx, 168 + dx, 96 + dy, (e[1] + 0.4) % 1, 2, seed=i + 5, drift=(6, -14))
    final = np.zeros((H, W, 4), np.uint8)
    behind = fx.copy(); behind[body] = 0
    T.over(final, behind); T.over(final, spr)
    front_kinds = {'hit', 'burst', 'pop', 'ring_mod', 'shards', 'debris', 'bolts', 'asmoke', 'motes', 'sputter'}
    if any(e[0] in front_kinds for e in s['fx']):
        front = fx.copy(); front[~body] = 0
        # só o que é efeito "na frente": tudo menos a aura e a fumaça cinza do motor
        if any(e[0] == 'aura_mod' for e in s['fx']):
            au = np.zeros((H, W), bool)
            for e in s['fx']:
                if e[0] == 'aura_mod': au |= T.mask(T.aura(T.mask(T.shift(T.P[2]['module'], dx, mc[1] - MC[1])), e[1], i))
            front[au & ~T.mask(np.zeros_like(fx))] = front[au & ~T.mask(np.zeros_like(fx))]
        T.over(final, front)
    return final, spr, fx


def defeated_loop():
    """loop pós-batalha: flutua 1 px, fumaça do motor, faísca ocasional"""
    fr = []
    for k, (dy, age, sp) in enumerate([(7, 0.0, False), (8, 0.25, False), (8, 0.5, True), (7, 0.75, False)]):
        spr = pose(F1, dy=dy, tilt=5, droop=3, head=(0, 3), tired=True)
        fx = np.zeros((H, W, 4), np.uint8)
        T.smoke_puffs(fx, EXH[0] - 4, EXH[1] + dy - 2, age, 4, seed=k)
        T.smoke_puffs(fx, 168, 96 + dy, (age + 0.5) % 1, 2, seed=k + 9, drift=(6, -14))
        if sp: T.sputter(fx, EXH[0], EXH[1] + dy, 40 + k)
        final = np.zeros((H, W, 4), np.uint8); b = fx.copy(); b[T.mask(spr)] = 0
        T.over(final, b); T.over(final, spr)
        fr.append((final, spr, fx))
    return fr


if __name__ == '__main__':
    meta = []; t = 0
    for i, s in enumerate(S):
        final, spr, fx = render(i, s)
        n = f'{i:02d}'
        Image.fromarray(final).save(f'{OUT}/frames/gusini_defeat_{n}.png')
        Image.fromarray(spr).save(f'{OUT}/layers/sprite/gusini_defeat_sprite_{n}.png')
        Image.fromarray(fx).save(f'{OUT}/layers/fx/gusini_defeat_fx_{n}.png')
        meta.append(dict(index=i, file=f'frames/gusini_defeat_{n}.png', duration_ms=s['dur'], start_ms=t, tag=s['tag']))
        t += s['dur']
    loop = defeated_loop()
    for k, (final, spr, fx) in enumerate(loop):
        Image.fromarray(final).save(f'{OUT}/final/gusini_p1_defeated_idle_{k}.png')
        Image.fromarray(spr).save(f'{OUT}/final/gusini_p1_defeated_sprite_{k}.png')
        Image.fromarray(fx).save(f'{OUT}/final/gusini_p1_defeated_fx_{k}.png')
    json.dump(dict(frames=meta, total_ms=t), open(f'{OUT}/_tmp.json', 'w'), indent=1)
    print('frames', len(S), 'total_ms', t)
