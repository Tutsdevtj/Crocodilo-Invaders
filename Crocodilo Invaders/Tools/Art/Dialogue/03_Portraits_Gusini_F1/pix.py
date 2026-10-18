# Ferramentas de pixel art para os portraits: máscaras supersampled (sem AA), sombreamento por deslocamento,
# contornos. Tudo 0/255 de alpha.
import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage

SS = 4
OUTL = (13, 13, 21)


class Canvas:
    def __init__(self, w, h):
        self.w, self.h = w, h
        self.rgb = np.zeros((h, w, 3), np.uint8)
        self.a = np.zeros((h, w), bool)
        self.lab = np.zeros((h, w), np.int16)   # id de região (para contornos internos)

    def paint(self, m, c):
        self.rgb[m] = c; self.a |= m

    def image(self):
        o = np.zeros((self.h, self.w, 4), np.uint8); o[..., :3] = self.rgb; o[..., 3] = self.a * 255
        return Image.fromarray(o)


def _blank(w, h):
    return Image.new('L', (w * SS, h * SS), 0)


def _down(im, w, h, thr=0.5):
    a = np.array(im, float).reshape(h, SS, w, SS).mean((1, 3)) / 255
    return a >= thr


def poly(w, h, pts, thr=0.5):
    im = _blank(w, h); ImageDraw.Draw(im).polygon([(x * SS, y * SS) for x, y in pts], fill=255)
    return _down(im, w, h, thr)


def ellipse(w, h, cx, cy, rx, ry, thr=0.5, rot=0.0):
    if rot == 0:
        im = _blank(w, h); ImageDraw.Draw(im).ellipse([(cx - rx) * SS, (cy - ry) * SS, (cx + rx) * SS, (cy + ry) * SS], fill=255)
        return _down(im, w, h, thr)
    t = np.linspace(0, 2 * np.pi, 80); c, s = np.cos(rot), np.sin(rot)
    pts = [(cx + rx * np.cos(a) * c - ry * np.sin(a) * s, cy + rx * np.cos(a) * s + ry * np.sin(a) * c) for a in t]
    return poly(w, h, pts, thr)


def bez(pts, n=40):
    """curva de bezier por pontos de controle (qualquer grau)"""
    pts = np.array(pts, float); out = []
    for t in np.linspace(0, 1, n):
        p = pts.copy()
        while len(p) > 1: p = p[:-1] * (1 - t) + p[1:] * t
        out.append(tuple(p[0]))
    return out


def path(*segs):
    """concatena segmentos: cada um é lista de pontos (bezier se for tupla de >=3 pontos marcada com 'b')"""
    out = []
    for s in segs:
        if isinstance(s, tuple) and s and s[0] == 'b': out += bez(s[1:])
        else: out += list(s)
    return out


def line_mask(w, h, pts, width=1):
    im = Image.new('L', (w, h), 0); ImageDraw.Draw(im).line([tuple(p) for p in pts], fill=255, width=width)
    return np.array(im) > 0


def shift(m, dx, dy):
    o = np.zeros_like(m); H, W = m.shape
    ys, ye = max(0, dy), min(H, H + dy); xs, xe = max(0, dx), min(W, W + dx)
    o[ys:ye, xs:xe] = m[ys - dy:ye - dy, xs - dx:xe - dx]
    return o


def near_edge(m, dx, dy, k):
    """pixels de m cujo vizinho a k passos na direção (dx,dy) sai de m"""
    r = np.zeros_like(m)
    for i in range(1, k + 1): r |= m & ~shift(m, -dx * i, -dy * i)
    return r


def shade(cv, m, ramp, hl=(-1, -1, 2), sh=(1, 1, 4), deep=(1, 1, 2), base=1, extra_hl=None):
    """ramp: [mais escuro ... mais claro]. base = índice da cor base.
    hl: borda clara (direção para fora, k). sh: sombra larga. deep: sombra funda."""
    cv.paint(m, ramp[base])
    if sh: cv.paint(near_edge(m, *sh), ramp[max(0, base - 1)])
    if deep and base >= 2: cv.paint(near_edge(m, *deep), ramp[base - 2])
    if hl and base + 1 < len(ramp): cv.paint(near_edge(m, *hl) & ~near_edge(m, *sh), ramp[base + 1])
    if extra_hl is not None and base + 2 < len(ramp): cv.paint(m & extra_hl, ramp[base + 2])


def outline_outer(cv, color=OUTL):
    m = cv.a
    o = ndimage.binary_dilation(m, structure=[[0, 1, 0], [1, 1, 1], [0, 1, 0]]) & ~m
    cv.rgb[o] = color; cv.a |= o


def inner_line(cv, m_front, color, below=None):
    """linha de 1px na borda de m_front, desenhada por dentro do que está atrás (pixels fora de m_front vizinhos)"""
    ring = ndimage.binary_dilation(m_front, structure=[[0, 1, 0], [1, 1, 1], [0, 1, 0]]) & ~m_front & cv.a
    if below is not None: ring &= below
    cv.rgb[ring] = color
    return ring


def edge_in(m):
    """borda interna de 1 px"""
    return m & ~ndimage.binary_erosion(m, structure=[[0, 1, 0], [1, 1, 1], [0, 1, 0]])


def cleanup(cv, protect=None):
    """remove pixels isolados (cor diferente dos 4 vizinhos iguais)"""
    rgb = cv.rgb.astype(int); key = rgb[..., 0] * 65536 + rgb[..., 1] * 256 + rgb[..., 2]
    up, dn, lf, rt = np.roll(key, 1, 0), np.roll(key, -1, 0), np.roll(key, 1, 1), np.roll(key, -1, 1)
    iso = cv.a & (up == dn) & (dn == lf) & (lf == rt) & (key != up)
    if protect is not None: iso &= ~protect
    cv.rgb[iso] = np.roll(cv.rgb, 1, 0)[iso]


def upscale(img, s, bg=None):
    im = img
    if bg is not None:
        b = Image.new('RGBA', img.size, bg); b.alpha_composite(img); im = b
    return im.resize((img.width * s, img.height * s), Image.NEAREST)
