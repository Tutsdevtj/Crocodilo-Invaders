# Passo 2: master do B. Crocodilo — refinamentos + camada de rosto paramétrica (expressões derivam daqui).
import sys; sys.path.insert(0, '/home/claude/proj/portraits'); sys.path.insert(0, '/home/claude/proj/portraits3')
import numpy as np
from PIL import Image
from scipy import ndimage
from pix import poly, ellipse, bez, line_mask, shift
from croc_base import PAL, OUT, regions, W, H

SK, HM, CR, EY, TT, GR = PAL['SKIN'], PAL['HELM'], PAL['CREAM'], PAL['EYE'], PAL['TEETH'], PAL['GREY']
MOUTH = [(52, 16, 22), (96, 30, 38)]
TONG = [(150, 58, 64), (196, 96, 96)]
yy, xx = np.mgrid[0:H, 0:W]
N4 = [[0, 1, 0], [1, 1, 1], [0, 1, 0]]

# --------------------------------------------------------------------------------------------
# geometria congelada do master (todas as expressões usam estes números)
HINGE = (76, 84)                                     # dobradiça da mandíbula
ML = [(76, 80), (88, 87), (100, 92), (112, 96), (124, 99), (136, 101), (148, 101), (153, 99)]   # linha da boca (borda do maxilar superior)
EYE_N = dict(c=(86.5, 65.0), rx=7.0, ry=5.4)         # olho próximo
EYE_F = dict(c=(113.5, 67.5), rx=3.2, ry=3.4)        # olho distante (parcialmente escondido pela ponte do focinho)
UPPER_TEETH = [(0.05, 3.0, 2.0), (0.16, 4.2, 2.4), (0.29, 8.5, 3.4), (0.43, 4.4, 2.4), (0.59, 9.5, 3.6), (0.74, 4.6, 2.4), (0.86, 5.0, 2.4), (0.96, 3.4, 2.0)]
LOWER_TEETH = [(0.915, 5.0, 2.2)]


def ml_curve(n=200):
    return np.array(bez(ML, n)) if False else np.array(_catmull(ML, n))


def _catmull(pts, n):
    pts = [np.array(p, float) for p in pts]; pts = [pts[0]] + pts + [pts[-1]]
    out = []
    for i in range(1, len(pts) - 2):
        p0, p1, p2, p3 = pts[i - 1], pts[i], pts[i + 1], pts[i + 2]
        for t in np.linspace(0, 1, n // (len(pts) - 3), endpoint=False):
            out.append(0.5 * ((2 * p1) + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t + (-p0 + 3 * p1 - 3 * p2 + p3) * t ** 3))
    out.append(pts[-2]); return out


def rot(pts, c, deg):
    a = np.radians(deg); co, si = np.cos(a), np.sin(a)
    return [(c[0] + (x - c[0]) * co - (y - c[1]) * si, c[1] + (x - c[0]) * si + (y - c[1]) * co) for x, y in pts]


def load_base():
    im = np.array(Image.open('/home/claude/proj/portraits3/out/step1.png'))
    rgb = im[..., :3].copy(); a = im[..., 3] > 0
    mat = np.load('/home/claude/proj/portraits3/out/step1_mat.npy')
    return rgb, a, mat


def nearest_fill(rgb, mask, src_ok):
    """preenche mask com a cor do pixel válido mais próximo (src_ok)"""
    _, (iy, ix) = ndimage.distance_transform_edt(~src_ok, return_indices=True)
    rgb[mask] = rgb[iy[mask], ix[mask]]


def build_master_base():
    rgb, a, mat = load_base()
    helm_poly, head_poly = regions()
    # ---------- 1. capacete menos dominante: reduz 12% na altura e 6% na largura, ancorado na aba ----------
    hz = (helm_poly | (ndimage.binary_dilation(helm_poly, iterations=2) & (mat == 'OUT') & (yy < 60))) & a
    layer = np.zeros((H, W, 4), np.uint8); layer[..., :3] = rgb; layer[..., 3] = hz * 255
    rgb2 = rgb.copy(); a2 = a & ~hz
    ax, ay, sx, sy = 82, 62, 0.94, 0.86
    for y in range(H):
        for x in range(W):
            ox = ax + (x - ax) / sx; oy = ay + (y - ay) / sy
            ix, iy = int(round(ox)), int(round(oy))
            if 0 <= ix < W and 0 <= iy < H and layer[iy, ix, 3]:
                rgb2[y, x] = layer[iy, ix, :3]; a2[y, x] = True
    # buracos entre aba e cabeça viram sombra escura
    filled = ndimage.binary_fill_holes(ndimage.binary_closing(a2, iterations=1))
    gap = filled & ~a2; rgb2[gap] = OUT; a2 = filled
    rgb, a = rgb2, a2
    # ---------- 2. contorno externo consistente de 1 px ----------
    outer = a & ~ndimage.binary_erosion(a, structure=N4)
    dark = (rgb.astype(int).sum(-1) < 60)
    ring2 = a & ~outer & ndimage.binary_dilation(outer, structure=N4) & dark
    ring2 &= ~ndimage.binary_erosion(dark, structure=np.ones((3, 3)))       # preserva massas escuras internas
    nearest_fill(rgb, ring2, a & ~outer & ~dark)
    rgb[outer] = OUT
    # ---------- 3. limpa o rosto: apaga olhos e dentes antigos (vão ser redesenhados) ----------
    eye_erase = ellipse(W, H, 86.5, 65, 8.5, 7) | ellipse(W, H, 113, 68, 4, 4)
    nearest_fill(rgb, eye_erase, a & ~eye_erase & ~dark & (np.abs(rgb[..., 1].astype(int) - rgb[..., 0]) > 15))
    cur = np.array(_catmull(ML, 200))
    mly = np.interp(xx, cur[:, 0], cur[:, 1])
    band = a & (xx >= HINGE[0]) & (xx <= 154) & (yy > mly - 6) & (yy < mly + 7)
    up = band & (yy <= mly); dn = band & (yy > mly)
    green = a & (rgb[..., 1].astype(int) - rgb[..., 0] > 18) & ~band
    cream = a & (rgb[..., 0] > 150) & (rgb[..., 1].astype(int) < rgb[..., 0]) & ~band
    nearest_fill(rgb, up & ~ndimage.binary_erosion(a, iterations=1) == False, green) if False else None
    nearest_fill(rgb, up & ndimage.binary_erosion(a, structure=N4), green)
    nearest_fill(rgb, dn & ndimage.binary_erosion(a, structure=N4), cream)
    # sombra do maxilar superior sobre a mandíbula (1 px) e luz na borda inferior do focinho
    sh_ = a & (yy > mly) & (yy <= mly + 1.5) & (xx > HINGE[0]) & (xx < 153) & ndimage.binary_erosion(a, structure=N4)
    rgb[sh_] = CR[1]
    # resíduos claros isolados no focinho verde
    gmask = a & (rgb[..., 1].astype(int) - rgb[..., 0] > 18)
    lightres = a & (rgb[..., 0] > 140) & (yy < mly - 1) & (xx > 90) & (ndimage.uniform_filter(gmask.astype(float), 5) > 0.55)
    nearest_fill(rgb, ndimage.binary_dilation(lightres), gmask & ~ndimage.binary_dilation(lightres))
    # linha da gola da camiseta
    neck = a & (np.abs(yy - (112 + 9 * np.exp(-((xx - 84) / 12.0) ** 2))) < 0.6) & (xx > 66) & (xx < 102) & (rgb[..., 0] > 120)
    rgb[neck] = PAL['SHIRT'][0]
    return rgb, a


BASE = None


def get_base():
    global BASE
    if BASE is None: BASE = build_master_base()
    return BASE[0].copy(), BASE[1].copy()


# --------------------------------------------------------------------------------------------
# camada de rosto
EXPR = {
    'master_neutral':             dict(jaw=0,   lid=0.30, lt=0.10,  low=0.08, brow=(1.0, -0.5), pup=(0.9, 0.3, 1.0)),
    'CROC_01_surpreso_indignado': dict(jaw=5,   lid=0.00, lt=0.00,  low=0.00, brow=(1.5, -3.0), pup=(0.5, 0.0, 0.9), wide=1.0),
    'CROC_02_serio_autoritario':  dict(jaw=0,   lid=0.36, lt=0.20,  low=0.10, brow=(2.4, 0.2),  pup=(1.3, 0.5, 1.1)),
    'CROC_03_confuso_preocupado': dict(jaw=3,   lid=0.12, lt=-0.26, low=0.04, brow=(-3.0, 1.0), pup=(-0.2, 0.4, 1.2), tilt_brow=1),
    'CROC_04_sincero_magoado':    dict(jaw=1.5, lid=0.40, lt=-0.30, low=0.12, brow=(-2.4, 1.8), pup=(-0.4, 1.3, 1.3), wet=1),
    'CROC_05_determinado':        dict(jaw=0,   lid=0.30, lt=0.22,  low=0.20, brow=(3.0, -1.0), pup=(1.4, 0.3, 1.1), grit=1),
}


def draw(e):
    X = EXPR[e]
    rgb, a = get_base()
    cur = np.array(_catmull(ML, 200))
    mly = np.interp(xx, cur[:, 0], cur[:, 1])
    # ---------------- mandíbula (abre girando na dobradiça) ----------------
    if X['jaw']:
        jp = [HINGE] + [tuple(p) for p in cur[::10]] + [(154, 100), (152, 106), (143, 113), (120, 115), (100, 113), (86, 111), (74, 104)]
        jaw_full = poly(W, H, jp) & a
        src_rgb = rgb.copy()
        ang = X['jaw']; c, s_ = np.cos(np.radians(-ang)), np.sin(np.radians(-ang))
        newj = np.zeros((H, W), bool); newc = np.zeros((H, W, 3), np.uint8)
        for y in range(H):
            for x in range(W):
                ox = HINGE[0] + (x - HINGE[0]) * c - (y - HINGE[1]) * s_; oy = HINGE[1] + (x - HINGE[0]) * s_ + (y - HINGE[1]) * c
                ix, iy = int(round(ox)), int(round(oy))
                if 0 <= ix < W and 0 <= iy < H and jaw_full[iy, ix]: newj[y, x] = True; newc[y, x] = src_rgb[iy, ix]
        lip = np.array(rot([tuple(p) for p in cur], HINGE, ang))
        lipy = np.interp(xx, lip[:, 0], lip[:, 1])
        mouth = a & (xx > HINGE[0] + 1) & (yy > mly) & (yy <= lipy + 0.5) | (jaw_full & ~newj & (yy > mly))
        rgb[mouth] = MOUTH[0]; a |= mouth
        tg = mouth & (yy > mly + 1.5) & (xx < 134) & (xx > 96)
        rgb[tg] = TONG[0]; rgb[tg & ~shift(tg, 0, 1)] = TONG[1]
        keep = newj & ~(yy <= lipy)
        rgb[keep] = newc[keep]; a |= keep
        # contorno inferior novo
        o2 = ndimage.binary_dilation(a, structure=N4) & ~a; rgb[o2] = OUT; a |= o2
        lower_shift = lambda p: rot([p], HINGE, ang)[0]
    else:
        lower_shift = lambda p: p
    # ---------------- dentes (ritmo irregular de propósito: 2 presas grandes) ----------------
    L = len(cur) - 1
    def tooth(x, y, h, w, down):
        tip = (x + (0.4 if down else -0.4), y + h if down else y - h)
        t = poly(W, H, [(x - w, y), tip, (x + w, y)], 0.45)
        sh = t & ~shift(t, -1, 0)          # lado direito/sombra
        rgb[t] = TT[1]; rgb[sh] = TT[0]; a[t] = True
        o = ndimage.binary_dilation(t, structure=N4) & ~t & a
        o &= (yy > y) if down else (yy < y)
        rgb[o & ~(rgb.astype(int).sum(-1) < 60)] = OUT
    for (t, h, w) in LOWER_TEETH:
        x, y = cur[int(t * L)]; x2, y2 = lower_shift((x, y + 2.2))
        tooth(x2, y2, h + (0.6 if X.get('grit') else 0), w, False)
    for (t, h, w) in UPPER_TEETH:
        x, y = cur[int(t * L)]
        tooth(x, y - 0.6, h, w, True)
    # linha da boca escura (só onde fechada)
    if not X['jaw']:
        for x in range(HINGE[0] + 2, 154):
            y = int(round(mly[0, x]))
            if a[y, x] and not (rgb[y, x].astype(int).sum() > 600): rgb[y, x] = OUT
    corner = {'master_neutral': 'up', 'CROC_02_serio_autoritario': 'flat', 'CROC_05_determinado': 'flat', 'CROC_03_confuso_preocupado': 'down', 'CROC_04_sincero_magoado': 'down'}.get(e)
    cx0, cy0 = ML[0]
    if corner == 'up':
        rgb[line_mask(W, H, [(cx0 + 1, cy0), (cx0 - 3, cy0 - 2), (cx0 - 5, cy0 - 5)]) & a] = OUT
    elif corner == 'down':
        rgb[line_mask(W, H, [(cx0 + 1, cy0), (cx0 - 3, cy0 + 2), (cx0 - 5, cy0 + 3)]) & a] = OUT
    elif corner == 'flat':
        rgb[line_mask(W, H, [(cx0 + 1, cy0), (cx0 - 4, cy0)]) & a] = OUT
    if X.get('grit'):     # dentes cerrados: músculo da bochecha marcado
        rgb[line_mask(W, H, [(70, 78), (74, 84)]) & a] = CR[1]
    # ---------------- olhos + sobrancelhas ----------------
    bi, bo = X['brow']
    def eye(E, near):
        ex, ey = E['c']; rx, ry = E['rx'], E['ry']
        sgn = 1 if near else -1                    # lado "de dentro" (entre os olhos): direita no próximo, esquerda no distante
        k = 1.0 if near else 0.55
        # patch limpo de pele em volta do olho
        patch = ellipse(W, H, ex, ey - 1, rx + 3.2, ry + 2.6) & a
        rgb[patch] = SK[2]
        rgb[patch & (yy < ey - ry * 0.2)] = SK[1]
        if X.get('wide'): rx += 0.5; ry += 0.9
        em = ellipse(W, H, ex, ey, rx, ry)
        rgb[ndimage.binary_dilation(em, structure=N4) & ~em & a] = SK[0]
        rgb[em] = EY[2]
        rgb[em & ~shift(em, 0, 2)] = EY[1]; rgb[em & ~shift(em, 0, 1) & ~shift(em, sgn, 0)] = EY[0]
        rgb[em & ~shift(em, 0, -2) & (xx < ex)] = EY[3]
        dx, dy, pw = X['pup']
        if not near: pw = max(0.6, pw * 0.65); dx *= 0.5
        ph = ry * (0.55 if X.get('wide') else 0.72)
        pu = ellipse(W, H, ex + dx * k, ey + dy * k, pw, ph) & em
        rgb[pu] = OUT
        if near:
            rgb[em & ~pu & ellipse(W, H, ex - rx * 0.5, ey - ry * 0.4, 1.1, 0.8)] = (255, 255, 240)
        # pálpebra superior
        lidy = (ey - ry) + X['lid'] * 2 * ry + X['lt'] * sgn * (xx - ex)
        lid = em & (yy < lidy)
        rgb[lid] = SK[2]; rgb[lid & ~shift(lid, 0, -1)] = SK[3]
        rgb[em & ~lid & shift(lid, 0, 1)] = OUT
        if X['low'] > 0:
            lo = em & (yy > (ey + ry) - X['low'] * 2 * ry); rgb[lo] = SK[2]; rgb[em & ~lo & shift(lo, 0, -1)] = SK[1]
        if X.get('wet') and near:
            rgb[em & ~lid & ~pu & (yy == int(ey + ry * 0.5)) & (xx > ex - rx + 1) & (xx < ex + rx - 1)] = (214, 240, 255)
        # sobrancelha: faixa logo acima do olho; ângulo = emoção
        yin = ey - ry - 0.8 + bi * k * 0.6; yout = ey - ry - 0.8 + bo * k * 0.6
        x_in, x_out = ex + sgn * (rx + 2), ex - sgn * (rx + 2)
        th = 3.6 * k + 0.6
        bp = [(x_out, yout), (ex, (yin + yout) / 2 + 0.6), (x_in, yin), (x_in, yin - th), (ex, (yin + yout) / 2 - th - 0.8), (x_out, yout - th + 0.6)]
        br = poly(W, H, bp) & a
        rgb[br] = SK[3]; rgb[br & ~shift(br, 0, 1)] = SK[1]; rgb[br & ~shift(br, 0, -1)] = SK[4] if near else SK[3]
        rgb[ndimage.binary_dilation(br, structure=N4) & ~br & a & shift(br, 0, 1)] = SK[0]
    eye(EYE_F, False); eye(EYE_N, True)
    if X.get('tilt_brow'):   # preocupação: ruga entre os olhos
        rgb[line_mask(W, H, [(98, 58), (100, 62)]) & a] = SK[0]
    o = np.zeros((H, W, 4), np.uint8); o[..., :3] = rgb; o[..., 3] = a * 255
    return Image.fromarray(o)


if __name__ == '__main__':
    import os
    os.makedirs('/home/claude/proj/portraits3/out/croc', exist_ok=True)
    ims = []
    for e in EXPR:
        im = draw(e); im.save(f'/home/claude/proj/portraits3/out/croc/{e}.png'); ims.append(im)
    sh = Image.new('RGBA', (3 * 164, 2 * 164), (60, 80, 90, 255))
    for i, im in enumerate(ims): sh.alpha_composite(im, ((i % 3) * 164 + 2, (i // 3) * 164 + 2))
    sh.resize((sh.width * 3, sh.height * 3), Image.NEAREST).save('/home/claude/proj/portraits3/out/croc_sheet_x3.png')
    print('ok')
