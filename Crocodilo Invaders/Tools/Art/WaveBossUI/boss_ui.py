# Boss UI — estrutura genérica (faixa, PERIGO!, impacto, linhas) + identidade GOOSINI (título, pena longa, cachecol, bico, penas)
# Pixel art nativa (exibida 2x em 1200x720). Mesma família da Wave UI: inclinação 0.30, contorno navy, extrusão em bloco.
import numpy as np
from scipy import ndimage
from wave_core import poly_mask, NAVY, WHITE, N4, over

SLANT = 0.30
# paleta (amostrada do retrato aprovado do Gusini F1)
P = [(37, 24, 61), (60, 36, 92), (88, 54, 124), (118, 72, 152), (150, 96, 174), (186, 140, 214)]    # roxo
R = [(84, 10, 24), (130, 16, 30), (172, 26, 34), (206, 40, 38), (240, 90, 56)]                        # vermelho cachecol
FW = [(255, 255, 255), (240, 238, 236), (212, 208, 222), (180, 172, 196), (145, 134, 163)]            # penas
BK = [(204, 98, 30), (236, 136, 36), (250, 174, 50), (250, 212, 108)]                                  # bico / âmbar
LN = [(30, 106, 190), (66, 196, 242), (196, 240, 255)]                                                 # lente dos óculos
CREAM = (255, 244, 222)
K = (13, 13, 21)


def slanted(polys, H, dx=0, dy=0, k=1.0):
    return [[(x * k + (H - y * k) * SLANT + dx, y * k + dy) for x, y in p] for p in polys]


def outline(o, m, col=NAVY, it=1):
    ring = ndimage.binary_dilation(m, structure=N4, iterations=it) & ~m
    o[ring & (o[..., 3] == 0)] = (*col, 255)


def paint(o, m, c): o[m] = (*c, 255)


# ====================================================================== GENÉRICO: faixa do boss
BAND_H = 44


def band(length=236, pal=P):
    """faixa angular: ponta afiada na frente (esquerda), fim rasgado em dentes (direita), 3 dentes na borda de baixo.
    Cores por índice de paleta → trocar 'pal' (6 tons) cria a variação de outro boss."""
    H = BAND_H; w = length + int(H * SLANT) + 20; h = H + 12
    L = length
    shape = [(0, 24), (18, 0), (L - 16, 0), (L - 4, 7), (L - 12, 12), (L + 2, 20), (L - 10, 27), (L - 2, 35), (L - 16, H),
             (L - 40, H), (L - 46, H + 7), (L - 52, H), (L - 70, H), (L - 76, H + 9), (L - 82, H), (L - 100, H), (L - 106, H + 6),
             (L - 112, H), (20, H)]
    m = poly_mask(w, h, slanted([shape], H, 3, 1))
    o = np.zeros((h, w, 4), np.uint8); yy, xx = np.mgrid[0:h, 0:w]
    paint(o, m, pal[2])
    tops = np.argmax(m, 0); tops[~m.any(0)] = h
    paint(o, m & (yy < tops[None] + 5), pal[3]); paint(o, m & (yy < tops[None] + 1), pal[4])
    paint(o, m & (yy > 1 + H - 6), pal[1]); paint(o, m & (yy > 1 + H), pal[0])          # base e dentes mais escuros
    # dois cortes de brilho diagonais (mesmo ângulo da tipografia) perto da frente
    for x0, wd in ((34, 3), (42, 1)):
        stripe = m & (np.abs(xx - (x0 + (h - yy) * SLANT)) < wd / 2 + 0.01)
        paint(o, stripe & (yy < tops[None] + 30), pal[5])
    outline(o, m)
    return o


# ====================================================================== GENÉRICO: tag PERIGO!
SH = 12
SMALL = {
    'P': (10, [[(0, 0), (8, 0), (10, 2), (10, 6), (8, 8), (3, 8), (3, SH), (0, SH)]], [[(3, 3), (7, 3), (7, 5), (3, 5)]]),
    'E': (10, [[(0, 0), (10, 0), (10, 3), (3, 3), (3, 4.5), (8, 4.5), (8, 7.5), (3, 7.5), (3, 9), (10, 9), (10, SH), (0, SH)]], []),
    'R': (10, [[(0, 0), (8, 0), (10, 2), (10, 6), (8, 8), (10.5, SH), (7, SH), (5, 8), (3, 8), (3, SH), (0, SH)]], [[(3, 3), (7, 3), (7, 5), (3, 5)]]),
    'I': (3, [[(0, 0), (3, 0), (3, SH), (0, SH)]], []),
    'G': (10, [[(0, 2), (2, 0), (10, 0), (10, 3), (3, 3), (3, 9), (7, 9), (7, 7.5), (5.5, 7.5), (5.5, 5), (10, 5), (10, SH), (2, SH), (0, 10)]], []),
    'O': (10, [[(0, 2), (2, 0), (8, 0), (10, 2), (10, 10), (8, SH), (2, SH), (0, 10)]], [[(3, 3), (7, 3), (7, 9), (3, 9)]]),
    '!': (4, [[(0, 0), (4, 0), (3.2, 8), (0.8, 8)], [(0.6, 9.5), (3.4, 9.5), (3.4, SH), (0.6, SH)]], []),
}


def small_text(s, gap=2, fill=CREAM, bang=BK[2], shadow=R[0]):
    W = sum(SMALL[c][0] + gap for c in s) + 10; H = SH + 6
    m = np.zeros((H, W), bool); mb = np.zeros((H, W), bool); x = 2
    for c in s:
        w, Ps, Hs = SMALL[c]
        mm = poly_mask(W, H, slanted(Ps, SH, x, 2), slanted(Hs, SH, x, 2))
        (mb if c == '!' else m).__ior__(mm); x += w + gap
    o = np.zeros((H, W, 4), np.uint8); al = m | mb
    sh = np.roll(np.roll(al, 1, 0), 1, 1) & ~al
    paint(o, sh, shadow); paint(o, m, fill); paint(o, mb, bang)
    outline(o, al | sh, K)
    return o


def alert_tag(state='red'):
    """state: 'red' (normal), 'amber' (piscada), 'flash' (quadro de impacto)"""
    txt = small_text('PERIGO!', fill=(K if state == 'flash' else CREAM), bang=(R[3] if state != 'red' else BK[2]),
                     shadow=(R[0] if state == 'red' else BK[0]))
    tw = txt.shape[1]; H = 20; L = tw + 10
    shape = [(4, 0), (L, 0), (L - 5, H / 2), (L, H), (0, H)]          # fim em "rabo de fita" (corte em V), como o cachecol
    w = L + int(H * SLANT) + 6
    m = poly_mask(w, H + 4, slanted([shape], H, 2, 2))
    o = np.zeros((H + 4, w, 4), np.uint8); yy = np.mgrid[0:H + 4, 0:w][0]
    base = {'red': (R[3], R[4], R[1]), 'amber': (BK[2], BK[3], BK[0]), 'flash': (WHITE, WHITE, FW[2])}[state]
    paint(o, m, base[0]); paint(o, m & (yy < 5), base[1]); paint(o, m & (yy > H - 1), base[2])
    lead = m & ~np.roll(m, 1, 1); paint(o, lead, BK[3] if state == 'red' else WHITE)   # borda de ataque acesa
    outline(o, m)
    over(o, txt, 6, (H + 4 - txt.shape[0]) // 2 + 1)
    return o


def impact_burst(cols=(BK[3], WHITE)):
    """estalo de impacto genérico (mesmo desenho da Wave UI, cores de alerta)"""
    w, h, cx, cy = 40, 34, 16, 24
    polys = []
    for ang, r0, r1, half in ((-80, 5, 17, 2.0), (-40, 5, 20, 2.4), (0, 6, 15, 1.8), (-120, 6, 13, 1.6)):
        a = np.radians(ang); d = np.array([np.cos(a), np.sin(a)]); n = np.array([-d[1], d[0]])
        p0 = np.array([cx, cy]) + d * r0; p1 = np.array([cx, cy]) + d * r1
        polys.append([tuple(p0 + n * half * 0.5), tuple(p1 + n * half), tuple(p1 + d * 2.5), tuple(p1 - n * half), tuple(p0 - n * half * 0.5)])
    m = poly_mask(w, h, polys); o = np.zeros((h, w, 4), np.uint8)
    paint(o, m, cols[0]); paint(o, m & ndimage.binary_erosion(m, structure=N4), cols[1]); outline(o, m)
    return o


def boss_speedlines(L, phase, cols=(FW[0], R[4], FW[1], P[4], R[3])):
    rows = [(14, 2, cols[0], 1.0), (22, 1, cols[1], 0.6), (30, 3, cols[2], 0.9), (40, 2, cols[3], 0.75), (50, 1, cols[4], 0.55), (58, 2, cols[0], 0.7)]
    w = 150; o = np.zeros((70, w, 4), np.uint8)
    for i, (y, th, c, k) in enumerate(rows):
        ln = int(L * k * (0.85 + 0.15 * ((phase + i) % 2))); x0 = 2 + (i * 7) % 11
        for j in range(min(ln, w - x0 - 1)):
            if j > ln * 0.7 and j % 2: continue
            o[y:y + (th if j < ln * 0.5 else 1), x0 + j] = (*c, 255)
    m = o[..., 3] > 0
    ring = ndimage.binary_dilation(m, structure=N4) & ~m & (np.roll(m, 1, 0) | np.roll(m, -1, 0))
    o[ring] = (*NAVY, 255)
    return o


# ====================================================================== GOOSINI: título
TH = 28   # grade de desenho (renderizado com k = 32/28 → 32 px de altura)
BIG = {
    'G': (24, [[(0, 6), (6, 0), (22, 0), (24, 2), (24, 9), (16, 9), (15, 8), (9, 8), (8, 9), (8, 19), (9, 20), (15, 20), (16, 19),
                (16, 17), (12, 17), (12, 12), (24, 12), (24, 22), (18, TH), (6, TH), (0, 22)]], []),
    'O': (24, [[(0, 6), (6, 0), (18, 0), (24, 6), (24, 22), (18, TH), (6, TH), (0, 22)]], [[(8, 10), (10, 8), (14, 8), (16, 10), (16, 18), (14, 20), (10, 20), (8, 18)]]),
    'S': (24, [[(0, 6), (6, 0), (22, 0), (24, 2), (24, 8), (10, 8), (9, 9), (9, 10), (10, 11), (18, 11), (24, 16), (24, 22), (18, TH),
                (2, TH), (0, 26), (0, 20), (14, 20), (15, 19), (15, 18), (14, 17), (6, 17), (0, 12)]], []),
    'I': (9, [[(0, 0), (9, 0), (9, TH), (0, TH)]], []),
    'N': (24, [[(0, 0), (8, 0), (16, 13), (16, 0), (24, 0), (24, TH), (16, TH), (8, 15), (8, TH), (0, TH)]], []),
}
TK = 32 / 28
TFILL = [(0, FW[0]), (0.30, FW[1]), (0.62, FW[2]), (0.86, FW[3])]


def title(word='GOOSINI', goggles=True, flash=False, gap=3):
    """lettering do Goosini: branco-pena com sombra lilás, extrusão roxa; os dois O viram os óculos azuis do personagem."""
    k = TK; Hh = int(round(TH * k)); ext = 4; pad = ext + 3
    adv = [int(round((BIG[c][0] + gap) * k)) for c in word]
    W = sum(adv) + int(Hh * SLANT) + pad * 2 + 4; Hc = Hh + pad * 2 + 2
    m = np.zeros((Hc, W), bool); lens = np.zeros((Hc, W), bool); ocent = []
    x = pad
    for c, a in zip(word, adv):
        w, Ps, Hs = BIG[c]
        m |= poly_mask(W, Hc, slanted(Ps, TH, x, pad, k), slanted(Hs, TH, x, pad, k))
        if c == 'O' and goggles:
            lens |= poly_mask(W, Hc, slanted(Hs, TH, x, pad, k))
            ocent.append(x)
        x += a
    o = np.zeros((Hc, W, 4), np.uint8); yy = np.mgrid[0:Hc, 0:W][0]
    exm = np.zeros_like(m)
    for e in range(1, ext + 1): exm |= np.roll(np.roll(m, e, 0), e, 1)
    exm &= ~m & ~lens
    ring = ndimage.binary_dilation(m | exm, structure=N4, iterations=2) & ~(m | exm | lens)
    paint(o, ring, NAVY); paint(o, exm, P[2]); paint(o, exm & ~np.roll(np.roll(exm, -1, 0), -1, 1), P[0])
    rows = np.where(m.any(1))[0]; f = (yy - rows.min()) / (rows.max() - rows.min() + 1)
    for thr, c in TFILL: paint(o, m & (f >= thr), WHITE if flash else c)
    if not flash:
        paint(o, m & ~np.roll(m, 1, 0), WHITE)
        # "penas": 3 pequenas fendas diagonais na base de cada letra larga (como barbas de pena), sem desmontar a letra
        notch = np.zeros_like(m)
        for xo in [x0 for x0 in range(pad, W, 1)]:
            pass
    if goggles and len(ocent) == 2:
        # lentes: azul com reflexo, e uma alça roxa ligando os dois O (o "óculos de aviador")
        paint(o, lens, LN[0]); paint(o, lens & (yy < np.where(lens.any(1))[0].min() + 6), LN[1])
        ys, xs = np.where(lens)
        for cx in ocent:
            sel = (xs > cx) & (xs < cx + 24 * k + 12)
            ty = ys[sel].min(); tx = xs[sel & (ys == ty)].min()
            o[ty + 1:ty + 3, tx + 1:tx + 2] = (*LN[2], 255); o[ty + 1, tx + 2] = (*WHITE, 255)
            o[ty + 4, tx] = (*LN[2], 255)
        lo = ndimage.binary_dilation(lens, structure=N4) & ~lens & m
        paint(o, lo, NAVY)
    return o


# ====================================================================== GOOSINI: pena longa (desenhada no ângulo final, sem girar bitmap)
def feather(length=176, width=16, ang=-24, bend=9, notches=((0.50, -1), (0.62, 1), (0.71, -1), (0.80, 1), (0.40, 1)),
            pal=FW, quill=(240, 232, 210), barbs=True):
    """pena de voo: haste curva, palheta assimétrica (frente estreita, trás larga), ponta arredondada,
    fendas entre barbas inclinadas para a ponta, penugem na base. Desenhada direto no ângulo final."""
    a = np.radians(ang); d = np.array([np.cos(a), np.sin(a)]); n = np.array([-d[1], d[0]])
    span = np.abs(d) * length + np.abs(n) * width * 2.4 + 10
    w, h = int(span[0]) + 8, int(span[1]) + 8
    yy, xx = np.mgrid[0:h, 0:w].astype(float)
    start = np.array([4 + (0 if d[0] >= 0 else -d[0] * length), 4 + (0 if d[1] >= 0 else -d[1] * length)]) + np.abs(n) * width * 1.2
    rel = np.stack([xx - start[0], yy - start[1]], -1)
    U = rel @ d; V = rel @ n; t = U / length
    v = V - bend * np.sin(np.pi * np.clip(t, 0, 1)) * (1 if ang <= 0 else -1)
    u0 = 0.16
    tt = np.clip(t, 0, 1)
    prof = np.where(tt < u0, 0, np.where(tt < 0.32, np.sin(np.pi / 2 * np.clip((tt - u0) / (0.32 - u0), 0, 1)) ** 0.7,
                    np.where(tt < 0.80, 1.0, np.sqrt(np.clip(1 - ((tt - 0.80) / 0.20) ** 2, 0, 1)))))
    inside_t = (t >= 0) & (t <= 1)
    wu, wl = width * 0.62 * prof, width * prof
    up = (v < 0) & (-v < wu) & inside_t
    lo = (v >= 0) & (v < wl) & inside_t
    for nt, side in notches:                                          # fendas
        dep = (wu if side < 0 else wl)
        sl = np.abs(t - nt - np.abs(v) / length * 1.4) < 1.0 / length
        cut = sl & (np.abs(v) > dep * 0.30) & ((v < 0) if side < 0 else (v > 0))
        up &= ~cut; lo &= ~cut
    shaft = (np.abs(v) < (1.0 if t.max() and True else 0.8)) & (t >= -0.03) & (t <= 0.96)
    m = up | lo | shaft
    o = np.zeros((h, w, 4), np.uint8)
    paint(o, up, pal[0]); paint(o, up & (-v > wu * 0.6), pal[1])
    paint(o, lo, pal[1]); paint(o, lo & (v > wl * 0.45), pal[2]); paint(o, lo & (v > wl * 0.8), pal[3])
    if barbs:                                                         # poucas linhas de barba (textura) na palheta de trás
        for bt in (0.36, 0.46, 0.56, 0.66, 0.75):
            ln = (np.abs(t - bt - np.abs(v) / length * 1.4) < 0.45 / length) & lo & (v > 2) & (v < wl * 0.75)
            paint(o, ln, pal[3] if bt < 0.6 else pal[2])
    paint(o, shaft & (t > u0), pal[3]); paint(o, shaft & (t > 0.55), pal[2]); paint(o, shaft & (t <= u0), quill)
    # penugem: fiapos curtos na base da palheta
    for k, (ft, fv) in enumerate(((u0 + 0.01, -4), (u0 + 0.03, 5), (u0 + 0.0, 3), (u0 + 0.05, -5))):
        fl = (np.abs(t - ft - np.abs(v) / length * 2.2) < 0.5 / length) & (np.abs(v) < abs(fv) + 1) & (np.sign(v) == np.sign(fv)) & (np.abs(v) > 1)
        paint(o, fl & ~m, pal[1]); m |= fl
    outline(o, m)
    cols = np.where(o[..., 3].any(0))[0]; rows = np.where(o[..., 3].any(1))[0]
    return o[rows.min():rows.max() + 1, cols.min():cols.max() + 1]


# ====================================================================== GOOSINI: cachecol (4 quadros de tremular)
def scarf(frame, nf=4):
    w, h = 64, 40; o = np.zeros((h, w, 4), np.uint8)
    yy, xx = np.mgrid[0:h, 0:w].astype(float)
    ph = frame / nf * 2 * np.pi
    m_all = np.zeros((h, w), bool)
    layers = []
    for (y0, amp, th0, th1, ln, off) in ((10, 3.0, 11, 7, 60, 0.0), (19, 3.6, 10, 6, 50, 1.7)):
        u = xx / ln
        cy = y0 + amp * u * np.sin(2 * np.pi * u * 1.3 - ph + off) + 4 * u
        th = th0 + (th1 - th0) * u
        band_ = (np.abs(yy - cy) < th / 2) & (xx < ln) & (xx >= 0)
        # ponta em V (rabo de fita)
        band_ &= ~((xx > ln - 6) & (np.abs(yy - cy) < (xx - (ln - 6)) * 0.7))
        layers.append((band_, cy))
    for i, (b, cy) in enumerate(layers[::-1]):          # a ponta de trás primeiro
        dark = i == 0
        paint(o, b, R[2] if dark else R[3])
        paint(o, b & (yy < cy - 1.2), R[3] if dark else R[4])
        paint(o, b & (yy > cy + 1.5), R[1] if dark else R[2])
        ring = ndimage.binary_dilation(b, structure=N4) & ~b
        o[ring] = (*NAVY, 255)
        m_all |= b
    # nó (preso na faixa)
    knot = ((xx - 3) ** 2 / 20 + (yy - 15) ** 2 / 49) <= 1
    paint(o, knot, R[3]); paint(o, knot & (yy < 12), R[4]); paint(o, knot & (yy > 19), R[1])
    ring = ndimage.binary_dilation(knot, structure=N4) & ~knot; o[ring & (xx < 7)] = (*NAVY, 255)
    return o


# ====================================================================== GOOSINI: ponta-bico (sobreposta à ponta afiada da faixa)
def beak_tip():
    H = BAND_H
    shape = [(0, 24), (13, 10.5), (17, 14), (17, 33), (13, 36)]
    w, h = 40, H + 4
    m = poly_mask(w, h, slanted([shape], H, 3, 1))
    o = np.zeros((h, w, 4), np.uint8); yy = np.mgrid[0:h, 0:w][0]
    paint(o, m, BK[2]); paint(o, m & (yy < 22), BK[3]); paint(o, m & (yy > 28), BK[1]); paint(o, m & (yy > 33), BK[0])
    outline(o, m)
    # linha da boca (separa as duas metades do bico) + narina — é isso que faz a ponta ler como bico
    ys, xs = np.where(m)
    for k in range(11):
        y = 26 + k // 4
        row = xs[ys == y]
        if len(row): o[y, row.min() + 1 + k] = (*NAVY, 255)
    row = xs[ys == 19]
    if len(row): o[19, row.min() + 9:row.min() + 11] = (*BK[0], 255)
    return o


# ====================================================================== GOOSINI: partículas de pena
def feather_particle(i):
    angs = [-20, 15, 50, 85]
    return feather(length=22, width=5.0, ang=angs[i % 4], bend=1.6, notches=(), barbs=False)


def feather_dissolve(sprite, step):
    """some em pixel (sem alpha parcial): step 1 = remove 50% em xadrez, step 2 = remove 75%"""
    o = sprite.copy(); h, w = o.shape[:2]; yy, xx = np.mgrid[0:h, 0:w]
    if step >= 1: o[(xx + yy) % 2 == 0] = 0
    if step >= 2: o[(xx % 2 == 1) & (yy % 2 == 1)] = 0
    return o


# ====================================================================== composição do banner do boss
BOSS_W, BOSS_H = 340, 120
POS = dict(band=(30, 40), beak=(30, 40), feather=(150, 0), title=(66, 34), scarf=(268, 52), tag=(56, 14), burst=(40, 0))


def compose(title_flash=False, tag='red', show_tag=True, scarf_f=0, show_burst=False, band_len=236):
    o = np.zeros((BOSS_H, BOSS_W, 4), np.uint8)
    b = band(band_len)
    over(o, scarf(scarf_f), *POS['scarf'])
    over(o, b, *POS['band'])
    over(o, beak_tip(), *POS['beak'])
    over(o, FEATHER_LONG, *POS['feather'])
    over(o, title(flash=title_flash), *POS['title'])
    if show_tag: over(o, alert_tag(tag), *POS['tag'])
    if show_burst: over(o, impact_burst(), *POS['burst'])
    return o


FEATHER_LONG = feather()
