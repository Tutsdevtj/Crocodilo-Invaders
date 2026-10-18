# Família de algarismos da Wave UI, derivada do 7 aprovado.
# Mesma construção do 7: polígonos numa grade de 46 px de altura, inclinação 0.30, traço ~12 px,
# cantos chanfrados, preenchimento em faixas Y0/Y1/Y2/Y3, brilho branco, contorno navy de 2 px e extrusão de 4 px.
import numpy as np
from wave_core import poly_mask, ink, NH, Y0, Y1, Y2, Y3, WHITE, NAVY, DEEP

SLANT = 0.30
FILLS = [(0, Y0), (0.18, Y1), (0.55, Y2), (0.86, Y3)]
EXTRUDE = 4

# (largura de desenho, polígonos externos, buracos) — coordenadas na grade de 46 px (y=0 topo, y=46 base)
H = 46
DIG = {
    '7': (44, [[(1, 0), (36, 0), (38, 2), (38, 11), (24, H), (11, H), (24, 12), (3, 12), (1, 10)]], []),   # APROVADO (inalterado)
    '0': (40, [[(0, 9), (9, 0), (29, 0), (38, 9), (38, 37), (29, H), (9, H), (0, 37)]],
              [[(12, 14), (14, 12), (24, 12), (26, 14), (26, 32), (24, 34), (14, 34), (12, 32)]]),
    '1': (26, [[(9, 0), (21, 0), (21, H), (9, H), (9, 14), (2, 14), (2, 9), (7, 0)]], []),
    '2': (40, [[(0, 9), (9, 0), (29, 0), (38, 9), (38, 21), (33, 27), (21, 34), (38, 34), (38, H), (0, H), (0, 35),
                (22, 21), (26, 17), (26, 14), (24, 12), (14, 12), (12, 14), (12, 17), (0, 17)]], []),
    '3': (40, [[(0, 9), (9, 0), (29, 0), (38, 9), (38, 18), (34, 23), (38, 28), (38, 37), (29, H), (9, H), (0, 37), (0, 34),
                (24, 34), (26, 32), (26, 30), (24, 28), (12, 28), (10, 26), (10, 20), (12, 18), (24, 18), (26, 16),
                (26, 14), (24, 12), (0, 12)]], []),
    '4': (42, [[(3, 0), (15, 0), (14, 22), (23, 22), (23, 0), (35, 0), (35, 22), (39, 22), (39, 33), (35, 33), (35, H),
                (23, H), (23, 33), (1, 33), (1, 23)]], []),
    '5': (40, [[(2, 0), (37, 0), (37, 12), (14, 12), (14, 16), (29, 16), (38, 25), (38, 37), (29, H), (9, H), (0, 37), (0, 34),
                (24, 34), (26, 32), (26, 30), (24, 28), (2, 28)]], []),
    '6': (40, [[(0, 9), (9, 0), (29, 0), (38, 9), (38, 15), (26, 15), (26, 14), (24, 12), (14, 12), (12, 14), (12, 17),
                (29, 17), (38, 26), (38, 37), (29, H), (9, H), (0, 37)]],
              [[(12, 29), (14, 27), (24, 27), (26, 29), (26, 32), (24, 34), (14, 34), (12, 32)]]),
    '8': (40, [[(2, 8), (10, 0), (28, 0), (36, 8), (36, 17), (32, 22), (38, 27), (38, 37), (29, H), (9, H), (0, 37), (0, 27),
                (6, 22), (2, 17)]],
              [[(13, 13), (15, 11), (23, 11), (25, 13), (25, 16), (23, 18), (15, 18), (13, 16)],
               [(12, 30), (14, 28), (24, 28), (26, 30), (26, 33), (24, 35), (14, 35), (12, 33)]]),
}


def _rot(polys):  # 6 girado 180° = 9 (garante parentesco perfeito entre 6 e 9)
    return [[(38 - x, H - y) for x, y in p] for p in polys]


DIG['9'] = (40, _rot(DIG['6'][1]), _rot(DIG['6'][2]))


def digit_mask(d, nh=NH):
    k = nh / NH; w, P, Hs = DIG[d]
    tr = lambda p: [(px * k + (nh - py * k) * SLANT + 2, py * k + 2) for px, py in p]
    return poly_mask(int(w * k) + int(nh * SLANT) + 4, nh + 4, [tr(p) for p in P], [tr(p) for p in Hs])


def digit_sprite(d, nh=NH, flash=False):
    s = ink(digit_mask(d, nh), FILLS, extrude=EXTRUDE)
    if flash:
        a = s[..., 3] > 0
        inner = a & ~np.all(s[..., :3] == NAVY, -1) & ~np.all(s[..., :3] == DEEP, -1)
        s[inner] = (*WHITE, 255)
    return s


def trim_x(s):
    """recorta colunas vazias, devolve (sprite, deslocamento esquerdo)"""
    cols = np.where(s[..., 3].any(0))[0]
    return s[:, cols.min():cols.max() + 1], int(cols.min())
