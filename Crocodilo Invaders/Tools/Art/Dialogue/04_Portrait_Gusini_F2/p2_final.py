# Gusini F2 (corrompido) — acabamento final sobre a base reconstruída: limpeza de borda, veias verdes, olho furioso redesenhado.
import sys; sys.path.insert(0, '/home/claude/proj/portraits'); sys.path.insert(0, '/home/claude/proj/portraits5')
import numpy as np
from PIL import Image
from scipy import ndimage
from pix import ellipse, line_mask
from p2_base import PAL, OUT, W, H

WHT, PUR, GRN = PAL['WHT'], PAL['PUR'], PAL['GRN']
yy, xx = np.mgrid[0:H, 0:W]
N4 = np.array([[0, 1, 0], [1, 1, 1], [0, 1, 0]])


def build():
    im = np.array(Image.open('/home/claude/proj/portraits5/out/base.png'))
    rgb = im[..., :3].copy(); a = im[..., 3] > 0
    mat = np.load('/home/claude/proj/portraits5/out/base_mat.npy')
    # ---------- 1. silhueta: remove espetos de 1 px (halo da referência) ----------
    body = a & ~((mat == 'OUT') & ~ndimage.binary_erosion(a, structure=N4))   # sem o contorno externo
    for _ in range(2):
        nb = ndimage.convolve(body.astype(int), N4, mode='constant') - body
        body &= ~(body & (nb <= 1))
    body = ndimage.binary_fill_holes(body)
    # pixels de borda com material diferente do interior vizinho (franja colorida) viram contorno
    edge = body & ~ndimage.binary_erosion(body, structure=N4)
    inner = body & ~edge
    for y, x in zip(*np.where(edge)):
        win_m = mat[max(0, y - 1):y + 2, max(0, x - 1):x + 2]; win_i = inner[max(0, y - 1):y + 2, max(0, x - 1):x + 2]
        vals = win_m[win_i]
        if len(vals) and mat[y, x] != 'OUT':
            u, c = np.unique(vals, return_counts=True)
            if mat[y, x] not in u: rgb[y, x] = OUT
    a = body.copy(); rgb[~a] = 0
    # ---------- 2. veias verdes: remove pontinhos soltos, mantém linhas ----------
    green = (mat == 'GRN') & a
    lab, n = ndimage.label(green, structure=np.ones((3, 3)))
    sizes = ndimage.sum(green, lab, range(1, n + 1))
    small = np.isin(lab, np.where(sizes <= 2)[0] + 1)
    ok = a & ~green & (rgb.astype(int).sum(-1) > 60)
    _, (iy, ix) = ndimage.distance_transform_edt(~ok, return_indices=True)
    rgb[small] = rgb[iy[small], ix[small]]
    # ---------- 3. olho furioso (mesma grade por coluna da Fase 1: x = 61..80) ----------
    zone = a & (ellipse(W, H, 70.5, 52, 10.5, 7.2) | (ellipse(W, H, 56, 51, 5.5, 6.5) & (xx >= 52))) & (yy >= 45) & ~((mat == 'BEK') | (mat == 'MTH'))
    bright = a & (rgb.astype(int).sum(-1) > 560) & ~zone
    _, (iy, ix) = ndimage.distance_transform_edt(~bright, return_indices=True)
    rgb[zone] = rgb[iy[zone], ix[zone]]
    X0 = 61
    T = [53, 52, 51, 50, 49, 49, 48, 48, 48, 48, 48, 48, 48, 49, 49, 50, 50, 51, 52, 53]
    B = [55, 56, 56, 57, 57, 57, 57, 57, 57, 57, 57, 57, 57, 56, 56, 55, 55, 54, 54, 53]
    L = [58, 57, 57, 56, 55, 55, 54, 54, 53, 53, 52, 52, 51, 51, 51, 51, 51, 51, 52, 53]   # pálpebra despenca para o lado do bico
    Lo = [57, 57, 58, 58, 58, 58, 58, 58, 58, 58, 58, 58, 58, 57, 57, 56, 56, 55, 54, 53]  # pálpebra de baixo tensa
    S = [53, 52, 51, 51, 50, 50, 49, 49, 48, 48, 47, 47, 46, 46, 46, 46, 46, 47, 48, 50]
    for c in range(20):
        x = X0 + c; l = max(T[c], L[c]); lo = min(B[c] + 1, Lo[c])
        for y in range(S[c], l):
            rgb[y, x] = PUR[4] if y == S[c] else (PUR[2] if y == l - 1 else PUR[3])
        rgb[l, x] = OUT
        if 2 <= c <= 16: rgb[l - 1, x] = OUT
        for y in range(l + 1, lo):                    # íris verde alienígena ocupa o olho todo
            rgb[y, x] = GRN[3] if y == l + 1 else GRN[2]
            if y == lo - 1: rgb[y, x] = GRN[1]
        if lo <= B[c] + 1 and c not in (0, 19): rgb[lo, x] = OUT if c < 6 else WHT[1]
    # pupila em fenda + brilho
    for (x, y) in [(73, 53), (73, 54), (73, 55), (74, 54)]: rgb[y, x] = OUT
    rgb[53, 71] = (240, 255, 220)
    # brilho verde fraco ao redor (1 px, só embaixo/fora — não domina)
    for (x, y) in [(66, 57), (67, 57), (75, 57), (76, 56), (79, 55)]:
        if a[y, x]: rgb[y, x] = GRN[1]
    # sobrancelha franzida (traço grosso em V, desce para o bico)
    for seg, w in [([(58, 54), (64, 50.5), (71, 47), (77, 45), (81, 45)], 2)]:
        rgb[line_mask(W, H, seg, w) & a] = OUT
    # rugas de raiva: entre olho e bico, embaixo do olho
    for seg in [[(52, 46), (55, 49)], [(51, 50), (55, 52)], [(53, 54), (57, 56)]]:
        rgb[line_mask(W, H, seg) & a] = WHT[0]
    for seg in [[(65, 60), (69, 60.5), (73, 60)], [(77, 59), (80, 58)]]:
        rgb[line_mask(W, H, seg) & a & (rgb.astype(int).sum(-1) > 450)] = WHT[1]
    rgb[(54, 55, 55), (57, 57, 58)] = rgb[(54, 55, 55), (57, 57, 58)]
    # cílio de fora
    for (x, y) in [(81, 52), (82, 51), (83, 50)]: rgb[y, x] = OUT
    # ---------- 4. contorno externo 1 px (mesmo padrão da Fase 1) ----------
    o = ndimage.binary_dilation(a, structure=N4) & ~a
    rgb[o] = OUT; a |= o
    out = np.zeros((H, W, 4), np.uint8); out[..., :3] = rgb; out[..., 3] = a * 255
    return Image.fromarray(out)


if __name__ == '__main__':
    im = build(); im.save('/home/claude/proj/portraits5/out/gusini_p2_corrupted_furious.png')
    b = Image.new('RGBA', (W, H), (60, 80, 90, 255)); b.alpha_composite(im); b.resize((800, 800), Image.NEAREST).save('/home/claude/proj/portraits5/out/final_x5.png')
    print('ok')
