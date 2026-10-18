# Gusini F1 — expressões derivadas do master aprovado. Só o rosto muda (olho, pálpebra, sobrancelha, sombra roxa,
# vinco do sorriso e mandíbula inferior do bico). Tudo o resto é o mesmo pixel do master.
import sys; sys.path.insert(0, '/home/claude/proj/portraits'); sys.path.insert(0, '/home/claude/proj/portraits4')
import numpy as np
from PIL import Image
from scipy import ndimage
from pix import poly, ellipse, bez, line_mask, shift
from gus_base import PAL, OUT, W, H

WHT, PUR, BEK, MTH, RED = PAL['WHT'], PAL['PUR'], PAL['BEK'], PAL['MTH'], PAL['RED']
yy, xx = np.mgrid[0:H, 0:W]
N4 = [[0, 1, 0], [1, 1, 1], [0, 1, 0]]

E = (71.0, 51.5); ERX = 8.4; ERY = 5.2               # olho (congelado)
HINGE = (64, 71)                                     # dobradiça do bico inferior
ML = [(79, 65), (68, 69), (58, 72), (48, 74.5), (38, 77.5), (28, 80.5), (19, 83)]

EXPR = {
    # lid: 0 aberto..1 fechado; lt: inclinação da pálpebra (+ = desce para o lado do bico = duro/bravo; - = desce para fora = triste)
    # brow: (y dentro, y fora) relativo; shadow: tamanho da sombra roxa; pup: (dx, dy, r); cheek: vinco; beak: 'open0' (como o master), 'closed', 'open+'
    'gusini_p1_smug':       dict(lid=0.46, lt=-0.10, low=0.10, brow=(0.5, -1.5), shadow=1.0, pup=(1.4, 0.8, 1.6), cheek='smirk', crease=1, beak='open0'),
    'gusini_p1_resentful':  dict(lid=0.52, lt=0.12, low=0.20, brow=(1.4, 0.4), shadow=1.15, pup=(-0.6, 1.0, 1.5), cheek='flat', beak='closed'),
    'gusini_p1_power':      dict(lid=0.14, lt=0.06, low=0.10, brow=(-1.2, -2.6), shadow=0.6, pup=(0.6, 0.2, 1.05), cheek='grin', crease=1, beak='open+', twitch=1),
    'gusini_p1_vulnerable': dict(lid=0.20, lt=-0.34, low=0.0, brow=(-2.4, 0.6), shadow=0.55, pup=(-0.4, 0.8, 1.9), cheek='soft', beak='closed'),
    'gusini_p1_hostile':    dict(lid=0.40, lt=0.34, low=0.18, brow=(2.6, -0.4), shadow=1.1, pup=(-1.4, 0.4, 1.15), cheek='tense', beak='open1', furrow=1),
}


def _rect(x0, y0, w, h, skip=()):
    return [(x, y) for x in range(x0, x0 + w) for y in range(y0, y0 + h) if (x, y) not in skip]


EYES = {
    # L: linha da pálpebra por coluna; Lo: linha de baixo; S: topo da sombra roxa; pupil: pixels; brow: traços escuros
    'gusini_p1_smug': dict(
        L=[53, 52, 52, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 52, 53],
        Lo=[54, 55, 55, 56, 56, 56, 56, 56, 56, 56, 56, 56, 56, 56, 55, 55, 55, 54, 54, 53],
        S=[50, 48, 47, 46, 46, 45, 45, 45, 45, 45, 45, 45, 45, 46, 46, 47, 48, 49, 50, 51], thick=1,
        pupil=_rect(71, 52, 3, 2) + [(72, 54)],
        brow=[([(70, 45), (75, 44), (80, 46)], 1)]),
    'gusini_p1_resentful': dict(
        L=[53, 53, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 53, 53],
        Lo=[54, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 54, 54, 54, 54, 53],
        S=[49, 48, 47, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 47, 47, 48, 49, 50, 51], thick=1,
        pupil=_rect(66, 53, 3, 2),
        brow=[([(61, 49), (66, 46), (72, 45)], 1)]),
    'gusini_p1_power': dict(
        L=[53, 52, 51, 50, 49, 49, 48, 48, 48, 48, 48, 48, 48, 49, 49, 50, 50, 51, 52, 53],
        Lo=[55, 56, 56, 57, 57, 57, 57, 57, 57, 57, 57, 57, 57, 57, 56, 56, 56, 55, 55, 54],
        S=[51, 50, 49, 48, 47, 47, 46, 46, 46, 46, 46, 46, 46, 47, 47, 48, 48, 49, 50, 51],
        pupil=_rect(70, 51, 2, 3),
        brow=[([(62, 47), (68, 44), (76, 44), (80, 46)], 1)]),
    'gusini_p1_vulnerable': dict(
        L=[53, 52, 51, 50, 49, 49, 48, 48, 48, 48, 49, 49, 49, 50, 50, 51, 51, 52, 52, 53],
        Lo=[54, 55, 55, 56, 56, 56, 56, 56, 56, 56, 56, 56, 56, 56, 55, 55, 55, 54, 54, 53],
        S=[49, 48, 47, 47, 47, 47, 47, 47, 47, 47, 48, 48, 48, 49, 49, 50, 50, 51, 51, 52],
        pupil=_rect(68, 50, 4, 5, skip={(68, 50), (71, 50), (68, 54), (71, 54)}), hl=[(69, 51)],
        brow=[([(62, 46), (66, 44), (72, 45), (79, 48)], 1)]),
    'gusini_p1_hostile': dict(
        L=[56, 55, 54, 53, 53, 52, 52, 51, 51, 51, 51, 50, 50, 50, 50, 50, 50, 51, 52, 53],
        Lo=[54, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 54, 54, 54, 54, 53],
        S=[50, 49, 48, 48, 47, 47, 46, 46, 46, 46, 46, 46, 46, 46, 46, 47, 47, 48, 49, 50], thick=1,
        pupil=_rect(68, 52, 3, 2) + [(69, 54)],
        brow=[([(61, 50), (68, 47), (76, 44), (80, 44)], 2)]),
}


def load():
    im = np.array(Image.open('/home/claude/proj/portraits4/out/base.png'))
    mat = np.load('/home/claude/proj/portraits4/out/base_mat.npy')
    return im[..., :3].copy(), im[..., 3] > 0, mat


def draw(e):
    X = EXPR[e]
    rgb, a, mat = load()
    beak = (mat == 'BEK') | (mat == 'MTH') | (((mat == 'RED') | (mat == 'OUT')) & (xx < 62) & (yy > 60) & (yy < 96))
    beak = ndimage.binary_closing(beak, iterations=1) & a & (xx < 82) & (yy < 96) & (yy > 40)
    mly = np.interp(xx, [p[0] for p in ML[::-1]], [p[1] for p in ML[::-1]])
    # ================= BICO INFERIOR =================
    lower = beak & (yy > mly + 0.5) & (xx < 62)
    mouth_old = lower & ((mat == 'MTH') | (mat == 'RED') | ((rgb.astype(int).sum(-1) < 200) & ~(mat == 'OUT')))
    if X['beak'] in ('closed', 'open1', 'open+'):
        # fecha: interior da boca vira bico inferior (luz de cima), linha da boca escura
        fill = lower & ndimage.binary_dilation(mouth_old, iterations=1) & ~(mat == 'OUT') | mouth_old
        rgb[fill] = BEK[1]; rgb[fill & (yy > mly + 3)] = BEK[2]
        cv = line_mask(W, H, [(p[0], p[1] + 0.6) for p in ML]) & beak & (xx < 79)
        rgb[cv] = OUT
    if X['beak'] in ('open1', 'open+'):
        rgb0, a0, _ = load()                                     # fonte: master (boca levemente aberta)
        ang = 2.5 if X['beak'] == 'open1' else 6.0               # graus; ponta desce
        LOWPOLY = [(p[0], p[1] + 0.8) for p in ML] + [(13, 85), (15, 90), (22, 93), (34, 92), (46, 88), (56, 84), (64, 78), (70, 72)]
        lowz = ndimage.binary_dilation(poly(W, H, LOWPOLY), iterations=1) & (a0 | ndimage.binary_dilation(a0, structure=N4))
        lowz &= (yy > mly - 0.2)
        phi = np.radians(ang)                                    # para a esquerda da dobradiça, +phi abaixa a ponta
        def fwd(x, y):
            dx_, dy_ = x - HINGE[0], y - HINGE[1]
            return HINGE[0] + dx_ * np.cos(phi) + dy_ * np.sin(phi), HINGE[1] - dx_ * np.sin(phi) + dy_ * np.cos(phi)
        def inv(x, y):
            dx_, dy_ = x - HINGE[0], y - HINGE[1]
            return HINGE[0] + dx_ * np.cos(phi) - dy_ * np.sin(phi), HINGE[1] + dx_ * np.sin(phi) + dy_ * np.cos(phi)
        new = np.zeros((H, W), bool); newc = np.zeros((H, W, 3), np.uint8)
        for y in range(50, 110):
            for x in range(0, 74):
                ox, oy = inv(x, y); ix, iy = int(round(ox)), int(round(oy))
                if 0 <= ix < W and 0 <= iy < H and lowz[iy, ix] and a0[iy, ix]:
                    new[y, x] = True; newc[y, x] = rgb0[iy, ix]
        lip = np.array([fwd(*p) for p in ML]); lipy = np.interp(xx, lip[::-1, 0], lip[::-1, 1])
        vac = lowz & ~new
        rgb[vac] = rgb0[vac]; a[vac] = False
        mo = (xx < 66) & (xx > lip[:, 0].min()) & (yy > mly) & (yy <= lipy + 1)
        rgb[mo] = MTH[0]; a |= mo
        tg = mo & (yy > lipy - 2) & (xx > 22) & (xx < 52); rgb[tg] = MTH[1]
        rgb[new] = newc[new]; a |= new
        ring = ndimage.binary_dilation(a, structure=N4) & ~a & (xx < 72) & (yy > 60) & (yy < 100); rgb[ring] = OUT; a |= ring
    # ================= ROSTO: patch do olho + bochecha =================
    face = a & ((mat == 'WHT') | (mat == 'PUR') | (mat == 'OUT') | (mat == 'MET') | (mat == 'BLU') | (mat == 'LTH')) & ~beak
    zone = face & (ellipse(W, H, 71, 51, 11.5, 7.5) | ellipse(W, H, 80, 60.5, 5.5, 4)) & (yy >= 45) & (xx > 58)
    zone &= ~((xx > 83) & (yy < 50))
    bright = (rgb.astype(int).sum(-1) > 560) & face
    erase = zone & ~bright
    _, (iy, ix) = ndimage.distance_transform_edt(~(bright & ~erase), return_indices=True)
    rgb[erase] = rgb[iy[erase], ix[erase]]
    patch = zone
    # ================= OLHO (desenhado por coluna, x = 61..80) =================
    X0 = 61
    T = [53, 52, 51, 50, 49, 49, 48, 48, 48, 48, 48, 48, 48, 49, 49, 50, 50, 51, 52, 53]   # topo da amêndoa
    B = [54, 55, 55, 56, 56, 56, 56, 56, 56, 56, 56, 56, 56, 56, 55, 55, 55, 54, 54, 53]   # base da amêndoa
    G = EYES[e]
    L = [max(t, l) for t, l in zip(T, G['L'])]
    Lo = [min(b + 1, lo) for b, lo in zip(B, G['Lo'])]
    S = G['S']                                        # topo da sombra roxa (pálpebra/arcada)
    for c in range(20):
        x = X0 + c
        # sombra roxa: de S até a linha da pálpebra
        for y in range(S[c], L[c]):
            if patch[y, x] or y >= 45:
                rgb[y, x] = PUR[4] if y == S[c] else (PUR[2] if y == L[c] - 1 else PUR[3]); a[y, x] = True
        # linha da pálpebra (grossa no meio)
        rgb[L[c], x] = OUT
        if G.get('thick') and 2 <= c <= 16: rgb[L[c] - 1, x] = OUT
        # esclera
        for y in range(L[c] + 1, Lo[c]):
            rgb[y, x] = WHT[4] if y < Lo[c] - 1 else WHT[2]
        # linha de baixo
        if Lo[c] <= B[c] + 1 and c not in (0, 19): rgb[Lo[c], x] = WHT[1]
    for (x, y) in G['pupil']: rgb[y, x] = OUT
    for (x, y) in G.get('hl', []): rgb[y, x] = (250, 250, 250)
    for seg in G.get('brow', []): rgb[line_mask(W, H, seg[0], seg[1]) & a] = OUT
    for (x, y) in G.get('lash', [(81, 52), (82, 51), (83, 50)]): rgb[y, x] = OUT
    if X.get('furrow'):     # ruga de raiva acima do bico
        rgb[line_mask(W, H, [(61, 50), (63, 53)]) & patch] = WHT[1]; rgb[line_mask(W, H, [(63, 48), (65, 51)]) & patch] = WHT[1]
    # vinco do sorriso na bochecha (continua a linha da boca)
    ck = {'smirk': [(76, 63.5), (80, 63), (83, 60.5), (84, 57.5)], 'grin': [(76, 63.5), (82, 62), (85, 57)], 'flat': [(77, 63), (81, 63.5)],
          'soft': [(77, 63), (80, 63.5)], 'tense': [(77, 63), (81, 63), (82, 64.5)]}[X['cheek']]
    rgb[line_mask(W, H, bez(ck, 12)) & a] = OUT
    if X.get('crease'):
        rgb[line_mask(W, H, [(65, 58), (69, 58.5)]) & patch] = WHT[1]
        if X['cheek'] == 'grin': rgb[line_mask(W, H, [(84, 55), (85, 53)]) & patch] = WHT[1]
    o = np.zeros((H, W, 4), np.uint8); o[..., :3] = rgb; o[..., 3] = a * 255
    return Image.fromarray(o)


if __name__ == '__main__':
    import os
    os.makedirs('/home/claude/proj/portraits4/out/exp', exist_ok=True)
    from PIL import ImageDraw
    ims = {e: draw(e) for e in EXPR}
    for e, im in ims.items(): im.save(f'/home/claude/proj/portraits4/out/exp/{e}.png')
    c = Image.new('RGBA', (3 * 450, 2 * 330), (30, 34, 44, 255)); d = ImageDraw.Draw(c)
    ref = Image.open('/home/claude/proj/portraits4/out/base.png')
    allims = [('REF (base)', ref)] + list(ims.items())
    for i, (n, im) in enumerate(allims):
        b = Image.new('RGBA', (160, 160), (60, 80, 90, 255)); b.alpha_composite(im)
        c.paste(b.crop((0, 36, 100, 102)).resize((450, 297), Image.NEAREST), ((i % 3) * 450, (i // 3) * 330 + 30)); d.text(((i % 3) * 450 + 4, (i // 3) * 330 + 10), n, fill=(255, 255, 0))
    c.save('/home/claude/proj/portraits4/out/faces.png')
    print('ok')
