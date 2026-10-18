# Gusini F1 — reconstrução da base (160x160) a partir do busto aprovado: paleta controlada, clusters limpos, contorno 1 px.
import numpy as np
from PIL import Image
from scipy import ndimage
from skimage.color import rgb2lab

W = H = 160
OUT = (13, 13, 21)
PAL = {
    'WHT': [(96, 86, 120), (145, 134, 163), (180, 172, 196), (212, 208, 222), (240, 238, 236)],
    'PUR': [(37, 24, 61), (60, 36, 92), (88, 54, 124), (118, 72, 152), (150, 96, 174)],
    'RED': [(84, 10, 24), (130, 16, 30), (172, 26, 34), (206, 40, 38), (240, 90, 56)],
    'BEK': [(150, 66, 28), (204, 98, 30), (236, 136, 36), (250, 174, 50), (250, 212, 108)],
    'GLD': [(110, 70, 36), (160, 112, 44), (204, 150, 48), (238, 190, 62), (248, 222, 120)],
    'BLU': [(20, 48, 110), (30, 106, 190), (66, 196, 242), (196, 240, 255)],
    'LTH': [(40, 26, 30), (70, 40, 36), (104, 64, 46)],
    'MET': [(52, 50, 56), (90, 86, 92), (137, 130, 135), (196, 192, 196)],
    'MTH': [(70, 18, 34), (139, 80, 80)],
}
N4 = [[0, 1, 0], [1, 1, 1], [0, 1, 0]]


def build():
    a = np.array(Image.open('/home/claude/proj/portraits4/out/down160.png')).astype(int)
    m0 = a[..., 3] > 0
    m = ndimage.binary_fill_holes(m0)
    a[m & ~m0, :3] = OUT
    lab = rgb2lab(a[..., :3][None].astype(float) / 255.)[0]
    names, cols = [], []
    for k, v in PAL.items():
        for i, c in enumerate(v): names.append((k, i)); cols.append(c)
    cl = rgb2lab(np.array(cols)[None] / 255.)[0]
    out = np.zeros((H, W, 3), np.uint8); mat = np.full((H, W), '', object); lvl = np.zeros((H, W), int)
    ys, xs = np.where(m)
    for y, x in zip(ys, xs):
        if lab[y, x, 0] < 14: out[y, x] = OUT; mat[y, x] = 'OUT'; continue
        d = ((cl - lab[y, x]) ** 2) * np.array([1.0, 1.4, 1.4])
        j = int(np.argmin(d.sum(1))); k, i = names[j]
        mat[y, x] = k; lvl[y, x] = i; out[y, x] = PAL[k][i]
    # bico: nada de dourado dentro do bico (laranja e dourado são vizinhos na paleta)
    yy_, xx_ = np.mgrid[0:H, 0:W]
    beakz = (xx_ < 80) & (yy_ > 52) & (yy_ < 94) & (xx_ > 4)
    fix = beakz & (mat == 'GLD')
    for y, x in zip(*np.where(fix)):
        i = min(4, lvl[y, x]); mat[y, x] = 'BEK'; lvl[y, x] = i; out[y, x] = PAL['BEK'][i]
    # simplifica clusters (mediana do nível por material)
    for k in PAL:
        mk = mat == k
        if not mk.any(): continue
        lv = np.where(mk, lvl, -1)
        sm = lvl.copy()
        for y, x in zip(*np.where(mk)):
            w_ = lv[max(0, y - 1):y + 2, max(0, x - 1):x + 2]; w_ = w_[w_ >= 0]
            sm[y, x] = int(np.round(np.median(w_)))
        lvl[mk] = sm[mk]
        for y, x in zip(*np.where(mk)): out[y, x] = PAL[k][lvl[y, x]]
    # pixels de material isolado (1 px sozinho) viram o material vizinho mais comum
    key = np.zeros((H, W), int)
    mats = list(PAL) + ['OUT']
    for i, k in enumerate(mats): key[mat == k] = i + 1
    for _ in range(2):
        for y, x in zip(*np.where(m)):
            if y in (0, H - 1) or x in (0, W - 1): continue
            win = key[y - 1:y + 2, x - 1:x + 2].ravel()
            if (win == key[y, x]).sum() <= 1:
                vals, cnt = np.unique(win[win > 0], return_counts=True)
                v = vals[np.argmax(cnt)]
                if v != key[y, x]:
                    # copia a cor do vizinho com esse material
                    for dy, dx in ((0, 1), (0, -1), (1, 0), (-1, 0), (1, 1), (-1, -1), (1, -1), (-1, 1)):
                        if key[y + dy, x + dx] == v:
                            out[y, x] = out[y + dy, x + dx]; key[y, x] = v; mat[y, x] = mats[v - 1]; break
    # contorno externo 1 px (fora da silhueta)
    o = ndimage.binary_dilation(m, structure=N4) & ~m
    out[o] = OUT; m = m | o; mat[o] = 'OUT'
    return out, m, mat


if __name__ == '__main__':
    out, m, mat = build()
    im = np.zeros((H, W, 4), np.uint8); im[..., :3] = out; im[..., 3] = m * 255
    Image.fromarray(im).save('/home/claude/proj/portraits4/out/base.png')
    np.save('/home/claude/proj/portraits4/out/base_mat.npy', mat.astype('U3'))
    b = Image.new('RGBA', (W, H), (60, 80, 90, 255)); b.alpha_composite(Image.fromarray(im))
    b.resize((800, 800), Image.NEAREST).save('/home/claude/proj/portraits4/out/base_x5.png')
    print('ok')
