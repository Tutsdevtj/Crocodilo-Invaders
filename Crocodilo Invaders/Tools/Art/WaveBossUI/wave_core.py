# CONCEITO — Wave UI "Arcade Swoosh" (só ONDA 7). Pixel art nativa (exibida 2x em 1200x720).
import os, numpy as np
from PIL import Image, ImageDraw, ImageFont
from scipy import ndimage
OUT = '/home/claude/proj/wave2/Concept'; os.makedirs(OUT, exist_ok=True)
N4 = np.array([[0, 1, 0], [1, 1, 1], [0, 1, 0]])
# paleta viva
NAVY = (16, 20, 48); DEEP = (30, 46, 120)
CREAM = (255, 244, 222); PEACH = (255, 204, 150); WHITE = (255, 255, 255)
Y0, Y1, Y2, Y3 = (255, 236, 120), (255, 208, 50), (255, 150, 30), (226, 92, 24)
CY0, CY1, CY2, CY3 = (190, 242, 255), (90, 210, 255), (40, 150, 235), (28, 92, 190)
SS = 4


def poly_mask(w, h, polys, holes=()):
    im = Image.new('L', (w * SS, h * SS), 0); d = ImageDraw.Draw(im)
    for p in polys: d.polygon([(x * SS, y * SS) for x, y in p], fill=255)
    for p in holes: d.polygon([(x * SS, y * SS) for x, y in p], fill=0)
    return np.array(im, float).reshape(h, SS, w, SS).mean((1, 3)) > 127


def rr(x0, y0, x1, y1, c=2):
    """retângulo com cantos chanfrados (c px)"""
    return [(x0 + c, y0), (x1 - c, y0), (x1, y0 + c), (x1, y1 - c), (x1 - c, y1), (x0 + c, y1), (x0, y1 - c), (x0, y0 + c)]


# ---------------- letras "ONDA" (altura 17, traço 5) ----------------
LH = 17


def glyph(ch):
    if ch == 'O':
        w = 15; return w, [rr(0, 0, w, LH, 4)], [rr(5, 5, w - 5, LH - 5, 1)]
    if ch == 'N':
        w = 15; return w, [[(0, 0), (5, 0), (10, 9), (10, 0), (15, 0), (15, LH), (10, LH), (5, 8), (5, LH), (0, LH)]], []
    if ch == 'D':
        w = 15; return w, [[(0, 0), (9, 0), (15, 5), (15, LH - 5), (9, LH), (0, LH)]], [[(5, 5), (8, 5), (10, 7), (10, LH - 7), (8, LH - 5), (5, LH - 5)]]
    if ch == 'A':
        w = 16; return w, [[(5, 0), (11, 0), (16, LH), (11, LH), (10, 13), (6, 13), (5, LH), (0, LH)]], [[(7, 6), (9, 6), (9.6, 9), (6.4, 9)]]
    if ch == 'V':
        w = 15; return w, [[(0, 0), (5, 0), (7.5, 10), (10, 0), (15, 0), (10, LH), (5, LH)]], []
    raise KeyError(ch)


def word_mask(s, gap=2, slant=0.30):
    w = sum(glyph(c)[0] + gap for c in s) + int(LH * slant) + 4
    m = np.zeros((LH + 2, w), bool); x = 0
    for c in s:
        gw, P, Hs = glyph(c)
        P2 = [[(px + x + (LH - py) * slant, py + 1) for px, py in p] for p in P]
        H2 = [[(px + x + (LH - py) * slant, py + 1) for px, py in p] for p in Hs]
        m |= poly_mask(w, LH + 2, P2, H2); x += gw + gap
    return m


# ---------------- número "7" grande (altura 46) ----------------
NH = 46


def seven_mask(slant=0.30, nh=NH):
    k = nh / NH; w = int(44 * k)
    P = [[(1 * k, 0), (36 * k, 0), (38 * k, 2 * k), (38 * k, 11 * k), (24 * k, nh), (11 * k, nh), (24 * k, 12 * k), (3 * k, 12 * k), (1 * k, 10 * k)]]
    P2 = [[(px + (nh - py) * slant + 2, py + 2) for px, py in p] for p in P]
    return poly_mask(w + int(nh * slant) + 4, nh + 4, P2)


def ink(mask, fills, extrude=3, hl=True):
    """pinta tipografia: preenchimento em faixas verticais, brilho, contorno navy grosso e extrusão 3D para baixo-direita"""
    h, w = mask.shape
    pad = extrude + 3
    m = np.zeros((h + pad * 2, w + pad * 2), bool); m[pad:pad + h, pad:pad + w] = mask
    H, W = m.shape
    o = np.zeros((H, W, 4), np.uint8)
    # extrusão (bloco 3D)
    ext = np.zeros_like(m)
    for k in range(1, extrude + 1):
        ext |= np.roll(np.roll(m, k, 0), k, 1)
    ext &= ~m
    out_all = ndimage.binary_dilation(m | ext, structure=N4, iterations=2) & ~(m | ext)
    o[out_all] = (*NAVY, 255)
    o[ext] = (*DEEP, 255)
    o[ext & ~np.roll(np.roll(ext, -1, 0), -1, 1)] = (*NAVY, 255)
    rows = np.where(m.any(1))[0]; top, bot = rows.min(), rows.max(); hh = bot - top + 1
    yy = np.mgrid[0:H, 0:W][0]
    f = (yy - top) / hh
    for thr, c in fills:
        o[m & (f >= thr)] = (*c, 255)
    # contorno interno de 1 px navy entre letra e extrusão já existe; brilho no canto de cima-esquerda
    if hl:
        edge = m & ~np.roll(m, 1, 0)
        o[edge] = (*WHITE, 255)
        e2 = m & ~np.roll(m, 1, 1) & (f < 0.5)
        o[e2] = (*WHITE, 255)
    return o


def swoosh(w, h, slant=0.30, body_end=None):
    """pincelada de velocidade ciano: frente reta inclinada (esquerda), cauda em cerdas e blocos (direita)"""
    o = np.zeros((h, w, 4), np.uint8)
    yy, xx = np.mgrid[0:h, 0:w]
    rng = np.random.default_rng(7)
    body = np.zeros((h, w), bool)
    top, bot = 4, h - 5
    be = body_end or int(w * 0.72)
    for y in range(top, bot):
        xl = 3 + int((bot - y) * slant)                        # frente: corte limpo, inclinado como a tipografia
        xr = be + int((bot - y) * slant) - int(rng.integers(0, 7)) - (y % 4 == 1) * 5
        body[y, xl:xr] = True
    for y in range(top + 1, bot - 1, 3):                       # cerdas: riscos que escapam para trás
        xs = np.where(body[y])[0]
        if len(xs): body[y, xs.max():min(w - 12, xs.max() + int(rng.integers(8, 30)))] = True
    # cunha: a borda de cima sobe em degraus regulares em direção ao número (energia apontando para o 7)
    rise = 6; x0 = 3 + int((bot - top) * slant); step = max(1, (be - x0) // rise)
    for k in range(rise):
        body[top:top + (rise - k), x0 - 12 + k * step: x0 + (k + 1) * step] = False
    o[body] = (*CY2, 255)
    tops = np.argmax(body, 0); tops[~body.any(0)] = h
    o[body & (yy < tops[None, :] + 3)] = (*CY1, 255)
    o[body & (yy < tops[None, :] + 1)] = (*CY0, 255)
    o[body & (yy < top + (bot - top) * 0.40) & False] = (*CY1, 255)
    o[body & (yy < top + 3)] = (*CY0, 255)
    o[body & (yy > bot - 4)] = (*CY3, 255)
    lead = body & ~np.roll(body, 1, 1)                          # brilho na borda de ataque
    o[lead & (yy < bot - 3)] = (*WHITE, 255)
    outl = ndimage.binary_dilation(body, structure=N4) & ~body
    o[outl] = (*NAVY, 255)
    for k in range(7):                                           # blocos que se desprendem atrás
        bx = be + 14 + int(rng.integers(0, w - be - 22)); by = int(rng.integers(top + 2, bot - 4)); s_ = int(rng.integers(2, 5))
        if k % 3 == 2: s_ = 2
        o[by - 1:by + s_ + 1, bx - 1:bx + s_ + 1] = (*NAVY, 255)
        o[by:by + s_, bx:bx + s_] = (*(CY1 if k % 2 else CY0), 255)
    return o


def burst(w, h, cx, cy):
    """marca de impacto de quadrinho no canto do número: 3 raios em cunha, limpos"""
    o = np.zeros((h, w, 4), np.uint8)
    polys = []
    for ang, r0, r1, half in ((-75, 5, 15, 1.8), (-35, 5, 18, 2.2), (5, 6, 14, 1.6)):
        a = np.radians(ang); d = np.array([np.cos(a), np.sin(a)]); n = np.array([-d[1], d[0]])
        p0 = np.array([cx, cy]) + d * r0; p1 = np.array([cx, cy]) + d * r1
        polys.append([tuple(p0 + n * half * 0.5), tuple(p1 + n * half), tuple(p1 + d * 2.5), tuple(p1 - n * half), tuple(p0 - n * half * 0.5)])
    m = poly_mask(w, h, polys)
    outl = ndimage.binary_dilation(m, structure=N4) & ~m
    o[outl] = (*NAVY, 255); o[m] = (*Y0, 255)
    o[m & ndimage.binary_erosion(m, structure=N4)] = (*WHITE, 255)
    return o


def over(dst, src, x, y):
    h, w = src.shape[:2]
    a = src[..., 3] > 0
    sub = dst[y:y + h, x:x + w]; sub[a[:sub.shape[0], :sub.shape[1]]] = src[:sub.shape[0], :sub.shape[1]][a[:sub.shape[0], :sub.shape[1]]]


W_, H_ = 236, 76
SEVEN_X, SEVEN_BASE = 120, 66      # canto esq. e linha de base do número (o número cresce para cima)


def compose(flash=False, nh=NH, show_burst=True):
    o = np.zeros((H_, W_, 4), np.uint8)
    over(o, swoosh(214, 42, body_end=150), 14, 24)
    wm = ink(word_mask('ONDA'), [(0, CREAM), (0.62, PEACH)], extrude=3)
    over(o, wm, 40, 32)
    sm = ink(seven_mask(nh=nh), [(0, Y0), (0.18, Y1), (0.55, Y2), (0.86, Y3)], extrude=4)
    if flash:
        a = sm[..., 3] > 0; inner = a & ~np.all(sm[..., :3] == NAVY, -1) & ~np.all(sm[..., :3] == DEEP, -1)
        sm[inner] = (*WHITE, 255)
    dx = (sm.shape[1] - ink(seven_mask(), [(0, Y0)], 4).shape[1]) // 2
    over(o, sm, SEVEN_X - dx, SEVEN_BASE - sm.shape[0] + 12)
    if show_burst:
        by = SEVEN_BASE - sm.shape[0] + 12
        over(o, burst(36, 30, 14, 22), 172 + dx, max(0, by - 6))
    return o


def x2(a): return Image.fromarray(a).resize((a.shape[1] * 2, a.shape[0] * 2), Image.NEAREST)


if __name__ == '__main__':
    c = compose()
    Image.fromarray(c).save(f'{OUT}/onda7_conceito_x1.png')
    bg = Image.new('RGBA', (c.shape[1] + 8, c.shape[0] + 8), (66, 132, 200, 255)); bg.alpha_composite(Image.fromarray(c), (4, 4))
    bg.resize((bg.width * 4, bg.height * 4), Image.NEAREST).save(f'{OUT}/onda7_conceito_x4.png')
    print('ok', c.shape)
