# Passo 1: reconstrução da base do B. Crocodilo a partir da imagem aprovada (160x160).
# - regiões de material traçadas à mão sobre a imagem reduzida
# - remapeamento para rampas de paleta controlada (Lab, cor mais próxima dentro da rampa permitida)
# - limpeza de clusters
import sys; sys.path.insert(0, '/home/claude/proj/portraits')
import numpy as np
from PIL import Image, ImageDraw
from skimage.color import rgb2lab
from scipy import ndimage
from pix import poly, ellipse, bez, path, line_mask, shift

W = H = 160
OUT = (13, 13, 21)
PAL = {
    'SKIN':  [(26, 42, 30), (40, 66, 38), (60, 96, 46), (90, 128, 56), (134, 162, 72)],
    'HELM':  [(36, 38, 26), (64, 66, 38), (96, 98, 50), (134, 138, 64), (190, 186, 88)],
    'JACK':  [(34, 34, 26), (58, 56, 38), (88, 82, 50), (122, 114, 64), (164, 154, 82)],
    'CREAM': [(116, 78, 46), (166, 120, 64), (208, 160, 88), (238, 200, 122), (250, 228, 168)],
    'SHIRT': [(118, 104, 80), (170, 154, 116), (214, 202, 164), (236, 228, 196)],
    'GREY':  [(44, 44, 50), (74, 74, 80), (112, 112, 114), (162, 160, 156), (212, 210, 202)],
    'EYE':   [(150, 96, 20), (228, 168, 30), (252, 214, 72), (255, 244, 170)],
    'TEETH': [(194, 178, 140), (246, 238, 210)],
}
yy, xx = np.mgrid[0:H, 0:W]


def regions():
    helm = poly(W, H, [(34, 64), (36, 50), (44, 36), (58, 25), (76, 19), (96, 19), (110, 25), (119, 35), (124, 46), (127, 52),
                      (118, 54), (104, 52), (90, 53), (76, 55), (62, 60), (48, 66)])
    head = poly(W, H, [(36, 64), (48, 66), (62, 60), (76, 55), (90, 53), (104, 52), (118, 54), (122, 60), (130, 68), (146, 68),
                      (154, 72), (157, 82), (155, 94), (152, 104), (142, 113), (120, 114), (100, 112), (86, 110), (74, 108),
                      (64, 104), (58, 98), (52, 96), (40, 92), (36, 80)])
    return helm, head


def build():
    a = np.array(Image.open('/home/claude/proj/portraits3/out/down160.png')).astype(int)
    m = a[..., 3] > 0
    helm, head = regions()
    body = m & ~helm & ~head
    lab = rgb2lab(a[..., :3][None] / 255.)[0]
    L = lab[..., 0]; A_ = lab[..., 1]; B_ = lab[..., 2]
    out = np.zeros((H, W, 3), np.uint8); mat = np.full((H, W), '', object); lvl = np.zeros((H, W), int)
    dark = (L < 17)
    # classificação de material por pixel
    sat = np.hypot(A_, B_)
    greenish = (A_ < -6)
    grey = (sat < 10) & (L > 25)
    yellow_eye = (B_ > 55) & (A_ > -12) & (L > 70) & (A_ < 15) & (yy < 72) & (xx > 78) & head
    creamish = (A_ > -4) & (B_ > 18) & ~greenish & (L > 45)
    for y in range(H):
        for x in range(W):
            if not m[y, x]: continue
            if dark[y, x]: mat[y, x] = 'OUT'; continue
            if helm[y, x]: k = 'GREY' if grey[y, x] else 'HELM'
            elif head[y, x]:
                if yellow_eye[y, x]: k = 'EYE'
                elif grey[y, x]: k = 'GREY'
                elif creamish[y, x]: k = 'CREAM'
                else: k = 'SKIN'
            else:
                if grey[y, x]: k = 'GREY'
                elif creamish[y, x] and L[y, x] > 55: k = 'CREAM' if (y < 112 + 9 * np.exp(-((x - 84) / 12.0) ** 2) and 56 < x < 106) else 'SHIRT'
                else: k = 'JACK'
            mat[y, x] = k
    # cor mais próxima dentro da rampa
    labpal = {k: rgb2lab(np.array(v)[None] / 255.)[0] for k, v in PAL.items()}
    for y in range(H):
        for x in range(W):
            k = mat[y, x]
            if k == '' : continue
            if k == 'OUT': out[y, x] = OUT; continue
            d = ((labpal[k] - lab[y, x]) ** 2)
            d[:, 0] *= 1.6   # prioriza luminância (mantém o volume da referência)
            i = int(np.argmin(d.sum(1))); lvl[y, x] = i; out[y, x] = PAL[k][i]
    # simplifica clusters: mediana do nível dentro de cada material
    from scipy.ndimage import median_filter
    for k in ('HELM', 'SKIN', 'JACK', 'CREAM', 'SHIRT', 'GREY'):
        mk = (mat == k)
        if not mk.any(): continue
        lv = np.where(mk, lvl, -1).astype(float)
        sm = lvl.copy()
        for y, x in zip(*np.where(mk)):
            win = lv[max(0, y - 1):y + 2, max(0, x - 1):x + 2]; win = win[win >= 0]
            sm[y, x] = int(np.round(np.median(win)))
        lvl[mk] = sm[mk]
        for y, x in zip(*np.where(mk)): out[y, x] = PAL[k][lvl[y, x]]
    return out, m, mat, lvl


def majority(out, m, mat, iters=2):
    """limpeza: pixel cuja cor aparece só nele no 3x3 vira a cor mais comum dos vizinhos (mesmo material)"""
    for _ in range(iters):
        key = out[..., 0].astype(int) * 65536 + out[..., 1].astype(int) * 256 + out[..., 2]
        new = out.copy()
        for y in range(1, H - 1):
            for x in range(1, W - 1):
                if not m[y, x] or mat[y, x] == 'OUT': continue
                win = key[y - 1:y + 2, x - 1:x + 2].ravel(); mw = m[y - 1:y + 2, x - 1:x + 2].ravel()
                same = (win == key[y, x]).sum()
                if same <= 1:
                    vals, cnt = np.unique(win[mw & (win != key[y, x])], return_counts=True)
                    if len(vals):
                        v = vals[np.argmax(cnt)]
                        new[y, x] = (v // 65536, (v // 256) % 256, v % 256)
        out = new
    return out


if __name__ == '__main__':
    out, m, mat, lvl = build()
    out = majority(out, m, mat)
    o = np.zeros((H, W, 4), np.uint8); o[..., :3] = out; o[..., 3] = m * 255
    Image.fromarray(o).save('/home/claude/proj/portraits3/out/step1.png')
    np.save('/home/claude/proj/portraits3/out/step1_mat.npy', mat.astype('U5'))
    b = Image.new('RGBA', (W, H), (60, 80, 90, 255)); b.alpha_composite(Image.fromarray(o))
    big = b.resize((W * 5, H * 5), Image.NEAREST)
    # overlay das regiões
    helm, head = regions(); ov = big.copy(); d = ImageDraw.Draw(ov)
    for msk, c in ((helm, (255, 0, 255)), (head, (0, 255, 255))):
        e = msk & ~ndimage.binary_erosion(msk)
        for y, x in zip(*np.where(e)): d.rectangle([x * 5 + 1, y * 5 + 1, x * 5 + 3, y * 5 + 3], fill=c)
    cmp_ = Image.new('RGBA', (W * 10, H * 5)); cmp_.paste(big, (0, 0)); cmp_.paste(ov, (W * 5, 0)); cmp_.save('/home/claude/proj/portraits3/out/step1_x5.png')
    print('ok')
