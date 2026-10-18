# Wave UI definitiva — peças modulares + regras de montagem (derivadas do conceito aprovado "ONDA 7").
# Coordenadas "banner" = px nativos (1x). Na tela de referência 1200x720 tudo é exibido 2x.
import numpy as np
from scipy import ndimage
from wave_core import (word_mask, ink, burst, over, N4, NAVY, WHITE, CREAM, PEACH,
                       CY0, CY1, CY2, CY3, NH)
from digits import digit_sprite, DIG

SLANT = 0.30
# ---------------------------------------------------------------- faixa azul (3 peças)
BAR_H = 42; BAR_TOP, BAR_BOT = 4, 37          # linhas com corpo dentro da peça
BAR_X, BAR_Y = 14, 24                         # posição da faixa no banner (como no conceito)
HEAD_W = 138                                  # cabeça: borda de ataque + cunha em degraus (largura fixa)
END_BACK, END_FWD = 12, 64                    # peça final: de L-12 até L+64 (borda irregular + cerdas + blocos)
L_MIN = HEAD_W + END_BACK                     # = 150 (comprimento do conceito aprovado)


def _draws():
    """mesma sequência aleatória do conceito aprovado (seed 7) — irregularidade idêntica"""
    rng = np.random.default_rng(7)
    rag = [int(rng.integers(0, 7)) for _ in range(BAR_TOP, BAR_BOT)]
    bri = [int(rng.integers(8, 30)) for _ in range(BAR_TOP + 1, BAR_BOT - 1, 3)]
    blk = []
    for k in range(7):
        bx = int(rng.integers(0, 42)); by = int(rng.integers(BAR_TOP + 2, BAR_BOT - 4)); s = int(rng.integers(2, 5))
        blk.append((bx, by, 2 if k % 3 == 2 else s, k))
    return rag, bri, blk


RAG, BRI, BLK = _draws()


def bar_full(L):
    """faixa inteira com comprimento de corpo L (L >= 150). Largura total L + 64."""
    w = L + END_FWD; h = BAR_H
    o = np.zeros((h, w, 4), np.uint8); yy = np.mgrid[0:h, 0:w][0]
    body = np.zeros((h, w), bool); top, bot = BAR_TOP, BAR_BOT
    for i, y in enumerate(range(top, bot)):
        xl = 3 + int((bot - y) * SLANT)
        xr = L + int((bot - y) * SLANT) - RAG[i] - (y % 4 == 1) * 5
        body[y, xl:xr] = True
    for j, y in enumerate(range(top + 1, bot - 1, 3)):
        xs = np.where(body[y])[0]; body[y, xs.max():min(w - 12, xs.max() + BRI[j])] = True
    rise = 6; x0 = 3 + int((bot - top) * SLANT); step = 21           # cunha termina em x = 138 (cabeça fixa)
    for k in range(rise):
        body[top:top + (rise - k), x0 - 12 + k * step: x0 + (k + 1) * step] = False
    o[body] = (*CY2, 255)
    tops = np.argmax(body, 0); tops[~body.any(0)] = h
    o[body & (yy < tops[None, :] + 3)] = (*CY1, 255)
    o[body & (yy < tops[None, :] + 1)] = (*CY0, 255)
    o[body & (yy < top + 3)] = (*CY0, 255)
    o[body & (yy > bot - 4)] = (*CY3, 255)
    lead = body & ~np.roll(body, 1, 1)
    o[lead & (yy < bot - 3) & (np.mgrid[0:h, 0:w][1] < 40)] = (*WHITE, 255)
    o[ndimage.binary_dilation(body, structure=N4) & ~body] = (*NAVY, 255)
    for bx, by, s, k in BLK:
        X = L + 14 + bx
        o[by - 1:by + s + 1, X - 1:X + s + 1] = (*NAVY, 255)
        o[by:by + s, X:X + s] = (*(CY1 if k % 2 else CY0), 255)
    return o


def bar_parts():
    """head [0,138) | mid (1 coluna, repetir/esticar) | end [L-12, L+64) — reproduz bar_full(L) exatamente"""
    full = bar_full(L_MIN + 40)
    head = full[:, :HEAD_W].copy(); mid = full[:, HEAD_W:HEAD_W + 1].copy()
    L = L_MIN + 40; end = full[:, L - END_BACK:L + END_FWD].copy()
    # verificação: todas as colunas do meio são iguais
    assert all((full[:, c] == mid[:, 0]).all() for c in range(HEAD_W, L - END_BACK))
    return head, mid, end


def bar_from_parts(L):
    head, mid, end = bar_parts()
    o = np.zeros((BAR_H, L + END_FWD, 4), np.uint8)
    o[:, :HEAD_W] = head
    for c in range(HEAD_W, L - END_BACK): o[:, c] = mid[:, 0]
    o[:, L - END_BACK:] = end
    return o


# ---------------------------------------------------------------- texto ONDA, explosão, linhas de velocidade
ONDA = ink(word_mask('ONDA'), [(0, CREAM), (0.62, PEACH)], extrude=3)
ONDA_X, ONDA_Y = 40, 32
BURST = burst(36, 30, 14, 22)
DIG_Y = 14                                   # topo do sprite do algarismo (tamanho base) no banner
DIG_BASE = DIG_Y + digit_sprite('7').shape[0]  # base do sprite (todos os algarismos têm a mesma altura de sprite)

SPEED_ROWS = [(30, 2, CREAM, 1.0), (37, 1, CY0, 0.7), (44, 3, WHITE, 0.9), (52, 2, CY1, 0.8), (58, 1, CY0, 0.55)]
SPEED_LEN = {'s': 40, 'm': 80, 'l': 130}


def speedlines(L, phase):
    w = 140; o = np.zeros((76, w, 4), np.uint8)
    for i, (y, th, c, k) in enumerate(SPEED_ROWS):
        ln = int(L * k * (0.85 + 0.15 * ((phase + i) % 2))); x0 = 2 + (i * 5) % 9
        for j in range(ln):
            if j > ln * 0.7 and j % 2: continue
            o[y:y + (th if j < ln * 0.5 else 1), x0 + j] = (*c, 255)
    m = o[..., 3] > 0
    ring = ndimage.binary_dilation(m, structure=N4) & ~m & (np.roll(m, 1, 0) | np.roll(m, -1, 0))
    o[ring] = (*NAVY, 255)
    return o


# ---------------------------------------------------------------- métricas dos algarismos (medidas na arte)
def _body(s):
    """máscara do corpo (sem contorno navy/extrusão)"""
    a = s[..., 3] > 0
    return a & ~np.all(s[..., :3] == NAVY, -1) & ~np.all(s[..., :3] == (30, 46, 120), -1)


def _rows_span(mask):
    L = np.full(mask.shape[0], 10 ** 6); R = np.full(mask.shape[0], -10 ** 6)
    for y in range(mask.shape[0]):
        xs = np.where(mask[y])[0]
        if len(xs): L[y], R[y] = xs.min(), xs.max()
    return L, R


DS = {d: digit_sprite(d) for d in '0123456789'}
DB = {d: _body(DS[d]) for d in DS}
SPR_W = {d: DS[d].shape[1] for d in DS}; SPR_H = DS['7'].shape[0]

# ONDA -> 1º algarismo: mesma distância mínima (corpo a corpo) que o 7 aprovado tem do ONDA (7 no x=120)
_onda_body = np.zeros((80, 400), bool); _ob = _body(ONDA); _onda_body[ONDA_Y:ONDA_Y + _ob.shape[0], ONDA_X:ONDA_X + _ob.shape[1]] = _ob


def _min_gap(left_mask, right_mask, rx):
    """distância horizontal mínima entre a máscara esquerda (em coords banner) e a direita colocada em x=rx"""
    best = 10 ** 6
    for y in range(right_mask.shape[0]):
        yb = DIG_Y + y
        if yb >= left_mask.shape[0]: continue
        xs_r = np.where(right_mask[y])[0]; xs_l = np.where(left_mask[yb])[0]
        if len(xs_r) and len(xs_l): best = min(best, rx + xs_r.min() - xs_l.max() - 1)
    return best


GAP_ONDA = _min_gap(_onda_body, DB['7'], 120)
FIRST_X = {}
_left_any = lambda d: int(np.where(DB[d].any(0))[0].min())
for d in DS:   # média entre "mesma distância mínima" e "mesmo recuo do ponto mais à esquerda" (equilíbrio ótico)
    x = 60
    while _min_gap(_onda_body, DB[d], x) < GAP_ONDA: x += 1
    FIRST_X[d] = int(round((x + 120 + _left_any('7') - _left_any(d)) / 2))

# kerning entre algarismos: distância mínima corpo-a-corpo fixa (tabela de pares)
DIGIT_GAP = 5


def _pair_adv(a, b):
    La, Ra = _rows_span(DB[a]); Lb, Rb = _rows_span(DB[b])
    ok = (Ra > -10 ** 5) & (Lb < 10 ** 5)
    return int(np.max(Ra[ok] - Lb[ok]) + 1 + DIGIT_GAP)


ADV = {a + b: _pair_adv(a, b) for a in DS for b in DS}

# fim da faixa e explosão relativos ao ÚLTIMO algarismo (calibrados no 7 aprovado)
MID_ROW = (BAR_Y + 20) - DIG_Y          # linha do sprite no meio da faixa


def _right_at(d, rows=range(MID_ROW - 6, MID_ROW + 7)):
    return int(np.median([_rows_span(DB[d])[1][r] for r in rows]))


def _top_right(d):
    L, R = _rows_span(DB[d]); ys = np.where(R > -10 ** 5)[0]
    return int(R[ys.min():ys.min() + 3].max())


BAR_END_OFF = {d: (BAR_X + L_MIN - 120) + (_right_at(d) - _right_at('7')) for d in DS}   # L_banner = x_ultimo + off
BURST_OFF = {d: (52 + _top_right(d) - _top_right('7'), -6) for d in DS}


# ---------------------------------------------------------------- montagem
def layout(num, pop=0, flash=False, show_burst=True):
    """devolve lista de (camada, nome, sprite, x, y) em coords banner, de trás para frente.
    pop: 0 = tamanho base, 1 = +3 px, 2 = +6 px (ênfase do assentamento)."""
    s = str(num); xs = []; x = FIRST_X[s[0]]
    for i, d in enumerate(s):
        xs.append(x)
        if i + 1 < len(s): x += ADV[d + s[i + 1]]
    last = s[-1]; xl = xs[-1]
    L_banner = max(BAR_X + L_MIN, xl + BAR_END_OFF[last]); L = L_banner - BAR_X
    items = [('bar', 'wave_bar', bar_from_parts(L), BAR_X, BAR_Y), ('word', 'wave_word_onda', ONDA, ONDA_X, ONDA_Y)]
    nh = NH + (0, 3, 6)[pop]
    for d, x in zip(s, xs):
        sp = digit_sprite(d, nh, flash) if (pop or flash) else DS[d]
        dx = (sp.shape[1] - SPR_W[d]) // 2
        items.append(('digit', f'wave_digit_{d}', sp, x - dx, DIG_BASE - sp.shape[0]))
    if show_burst:
        dx = (digit_sprite(last, nh).shape[1] - SPR_W[last]) // 2 if pop else 0
        top = DIG_BASE - (SPR_H + (0, 3, 6)[pop])
        items.append(('burst', 'wave_burst', BURST, xl + BURST_OFF[last][0] + dx, max(0, top + BURST_OFF[last][1])))
    return items, dict(L=L, L_banner=L_banner, digit_x=xs)


def compose(num, **kw):
    items, info = layout(num, **kw)
    W = max(x + sp.shape[1] for _, _, sp, x, y in items) + 2
    o = np.zeros((80, W, 4), np.uint8)
    for _, _, sp, x, y in items: over(o, sp, x, y)
    return o, info
