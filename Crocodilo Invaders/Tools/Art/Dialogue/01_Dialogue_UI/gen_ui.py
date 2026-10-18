# Crocodilo Invaders — interface de diálogo "rádio de piloto". Assets nativos 1x (exibir em 2x na referência 1200x720).
import os, json
import numpy as np
from PIL import Image

OUT_DIR = '/home/claude/proj/dialogue/Dialogue_UI'
os.makedirs(f'{OUT_DIR}/sprites', exist_ok=True)

# paleta (mesma família do HUD do player)
K = (14, 16, 20)                                                       # contorno
S = [(18, 22, 28), (28, 34, 42), (40, 46, 54), (62, 70, 80), (98, 108, 118)]   # ardósia/aço
O = [(44, 50, 30), (66, 74, 44), (92, 100, 62), (128, 136, 84)]        # oliva
B = [(110, 72, 30), (178, 122, 54), (226, 170, 80), (255, 214, 130)]   # bronze
A = [(160, 90, 20), (236, 150, 40), (255, 206, 90), (255, 240, 180)]   # âmbar
G = [(20, 60, 40), (40, 140, 70), (110, 230, 90), (210, 255, 170)]     # LED verde
CREAM = (246, 236, 204)
T = None                                                               # transparente


def canvas(w, h):
    return np.zeros((h, w, 4), np.uint8)


def px(im, x, y, c):
    if c is None: im[y, x] = 0; return
    im[y, x, :3] = c; im[y, x, 3] = 255


def rect(im, x0, y0, x1, y1, c):          # inclusivo
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1): px(im, x, y, c)


def save(im, name):
    Image.fromarray(im).save(f'{OUT_DIR}/sprites/{name}.png')
    return im


# ======================================================================================
# PAINEL (9-slice). Fonte 64x44. Bordas: L=R=14, T=B=12. Espessura do quadro = 7 px.
PW, PH = 64, 44
BORDER = dict(left=14, right=14, top=12, bottom=12)
FR = 7                     # espessura da moldura
CH_T, CH_B = 3, 5          # chanfro dos cantos (cima, baixo)


def inside_shape(x, y, w, h, inset=0):
    """silhueta chanfrada (cantos de cima 3 px, de baixo 5 px)"""
    if x < inset or y < inset or x > w - 1 - inset or y > h - 1 - inset: return False
    ct, cb = CH_T, CH_B
    if y - inset < ct and (x - inset < ct - (y - inset) or (w - 1 - inset - x) < ct - (y - inset)): return False
    yb = h - 1 - inset - y
    if yb < cb and (x - inset < cb - yb or (w - 1 - inset - x) < cb - yb): return False
    return True


def shape_mask(w, h, inset=0):
    m = np.zeros((h, w), bool)
    for y in range(h):
        for x in range(w): m[y, x] = inside_shape(x, y, w, h, inset)
    return m


def build_panel(w=PW, h=PH):
    outer = shape_mask(w, h, 0)
    ring1 = outer & ~shape_mask(w, h, 1)              # contorno
    metal = shape_mask(w, h, 1) & ~shape_mask(w, h, 5)
    lipK = shape_mask(w, h, 5) & ~shape_mask(w, h, 6)  # linha escura interna
    lip = shape_mask(w, h, 6) & ~shape_mask(w, h, 7)   # lábio de aço
    screen = shape_mask(w, h, 7)
    frame = canvas(w, h); back = canvas(w, h)
    yy, xx = np.mgrid[0:h, 0:w]
    # --- metal oliva com bisel (luz de cima/esquerda) ---
    for y, x in zip(*np.where(metal)):
        d_top, d_left, d_bot, d_right = y - 1, x - 1, h - 2 - y, w - 2 - x
        dmin = min(d_top, d_left, d_bot, d_right)
        if d_top == dmin or d_left == dmin:
            c = O[3] if dmin == 0 else O[2]
        else:
            c = O[0] if dmin == 0 else O[1]
        if dmin == 3: c = O[1] if (d_top == dmin or d_left == dmin) else O[0]
        px(frame, x, y, c)
    # faixa central do metal (sulco) — 1 px mais escuro no meio da moldura, dá cara de chapa dobrada
    for y, x in zip(*np.where(metal)):
        d = min(y - 1, x - 1, h - 2 - y, w - 2 - x)
        if d == 2: px(frame, x, y, O[1] if (y - 1 == d or x - 1 == d) else O[0])
    for y, x in zip(*np.where(ring1)): px(frame, x, y, K)
    for y, x in zip(*np.where(lipK)):
        top_left = (y == 5 or x == 5) and not (y == h - 6 or x == w - 6)
        px(frame, x, y, B[2] if top_left else B[0])
    for y, x in zip(*np.where(lip)): px(frame, x, y, K)
    # --- cantoneiras de bronze (saem 1 px para fora: silhueta própria) ---
    def bracket(cx, cy, sx, sy):
        # L de bronze 9x9 com rebite; (cx,cy) = canto externo; sx,sy = direção para dentro
        pts = []
        for i in range(9):
            for t in range(3):
                pts.append((cx + sx * i, cy + sy * t)); pts.append((cx + sx * t, cy + sy * i))
        pts = set(pts)
        # contorno
        for (x, y) in pts:
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                q = (x + dx, y + dy)
                if q not in pts and 0 <= q[0] < w and 0 <= q[1] < h: px(frame, q[0], q[1], K)
        for (x, y) in pts:
            if not (0 <= x < w and 0 <= y < h): continue
            i = abs(x - cx); t = abs(y - cy)
            edge_light = (sy > 0 and y == min(cy, cy + sy * 2)) or (sx > 0 and x == min(cx, cx + sx * 2))
            c = B[2] if edge_light else B[1]
            if (sy < 0 and y == max(cy, cy + sy * 2)) or (sx < 0 and x == max(cx, cx + sx * 2)): c = B[0]
            px(frame, x, y, c)
        # rebite
        rx, ry = cx + sx * 1, cy + sy * 1
        px(frame, rx, ry, B[3]); px(frame, rx + sx, ry, B[1]); px(frame, rx, ry + sy, B[1]); px(frame, rx + sx, ry + sy, B[0])
    bracket(1, 1, 1, 1); bracket(w - 2, 1, -1, 1); bracket(1, h - 2, 1, -1); bracket(w - 2, h - 2, -1, -1)
    # recorta pixels da cantoneira que caíram fora da silhueta chanfrada => a cantoneira "tapa" o chanfro
    # --- marcas de instrumento (ticks) no topo, perto dos cantos (ficam dentro da borda fixa) ---
    for x in (11, 13): px(frame, x, 3, B[2])
    for x in (w - 12, w - 14): px(frame, x, 3, B[2])
    # --- fundo (tela) ---
    for y, x in zip(*np.where(screen | lip)):
        c = S[1]
        if y <= 8: c = S[2]
        if y >= h - 9: c = S[0]
        px(back, x, y, c)
    # linhas de varredura MUITO sutis só na faixa de cima (fica na borda fixa, não briga com o texto)
    for y, x in zip(*np.where(screen)):
        if y == 9: px(back, x, y, S[2] if x % 2 == 0 else S[1])
    frame[screen & ~lip] = 0
    # --- realce de troca de falante (mesmo canvas/bordas) ---
    hl = canvas(w, h)
    for y, x in zip(*np.where(lip)):
        if y == 6: px(hl, x, y, A[2])
        elif x == 6 or x == w - 7: px(hl, x, y, A[1] if y < h // 2 else None)
    for y, x in zip(*np.where(screen)):
        if y == 7: px(hl, x, y, A[1])
    return back, frame, hl


back, frame, hl = build_panel()
save(back, 'dialogue_panel_backplate'); save(frame, 'dialogue_panel_frame'); save(hl, 'dialogue_highlight')

# ======================================================================================
# PLACA DE NOME (3-slice horizontal). 40x15. Bordas: L=10, R=7.
NW, NH = 40, 15
NB = dict(left=10, right=7, top=0, bottom=0)


def build_nameplate(w=NW, h=NH):
    im = canvas(w, h); acc = canvas(w, h)
    rect(im, 0, 0, w - 1, h - 1, K)
    rect(im, 1, 1, w - 2, h - 2, O[2]); rect(im, 1, 1, w - 2, 1, O[3]); rect(im, 1, h - 2, w - 2, h - 2, O[0])
    rect(im, 1, 1, 1, h - 2, O[3]); rect(im, w - 2, 1, w - 2, h - 2, O[0])
    # etiqueta escura (onde vai o nome)
    rect(im, 8, 2, w - 6, h - 3, K)
    rect(im, 9, 3, w - 7, h - 4, S[0]); rect(im, 9, 3, w - 7, 3, S[2])
    # cantos arredondados de 1 px
    for (x, y) in ((0, 0), (w - 1, 0), (0, h - 1), (w - 1, h - 1)): px(im, x, y, None)
    # tarja de cor (acento) na tampa esquerda: 3 px, vai em camada separada para tingir na Unity
    for y in range(2, h - 2):
        for x in (3, 4, 5):
            c = (255, 255, 255) if x == 3 else ((214, 214, 214) if x == 4 else (164, 164, 164))
            if y == 2: c = (255, 255, 255)
            if y == h - 3: c = (128, 128, 128)
            acc[y, x, :3] = c; acc[y, x, 3] = 255
            px(im, x, y, B[1] if x != 3 else B[2])
    rect(im, 2, 2, 2, h - 3, K); rect(im, 6, 2, 6, h - 3, K)
    # rebite da direita
    px(im, w - 4, 6, B[3]); px(im, w - 3, 6, B[1]); px(im, w - 4, 7, B[1]); px(im, w - 3, 7, B[0])
    return im, acc


npl, nacc = build_nameplate()
save(npl, 'dialogue_nameplate'); save(nacc, 'dialogue_nameplate_accent')

# ======================================================================================
# MÓDULO DE RÁDIO 34x17 (LED grande + 3 barras de sinal + grelha + antena) — senta na borda de cima do painel
RW, RH = 34, 17


def build_radio(state):
    im = canvas(RW, RH)
    # antena
    for y in range(1, 5): px(im, 29, y, S[4] if y > 1 else K)
    for (x, y) in ((28, 0), (29, 0), (30, 0), (28, 1), (30, 1)): px(im, x, y, K)
    px(im, 29, 1, A[2] if state == 2 else (A[1] if state >= 0 else S[3]))
    # caixa (aço) com bisel
    rect(im, 0, 4, RW - 1, RH - 1, K)
    rect(im, 1, 5, RW - 2, RH - 2, S[2]); rect(im, 1, 5, RW - 2, 5, S[4]); rect(im, 1, 5, 1, RH - 2, S[3])
    rect(im, 1, RH - 2, RW - 2, RH - 2, S[1]); rect(im, RW - 2, 5, RW - 2, RH - 2, S[1])
    for (x, y) in ((0, 4), (RW - 1, 4), (0, RH - 1), (RW - 1, RH - 1)): px(im, x, y, None)
    # aro de bronze do LED 9x9 (redondo) com lente 5x5
    cx, cy = 7, 10
    for y in range(cy - 4, cy + 5):
        for x in range(cx - 4, cx + 5):
            d2 = (x - cx) ** 2 + (y - cy) ** 2
            if d2 <= 18:
                c = B[1]
                if y < cy - 1 and d2 > 8: c = B[2]
                if y > cy + 1 and d2 > 8: c = B[0]
                px(im, x, y, c)
            if 18 < d2 <= 24: px(im, x, y, K)
    for y in range(cy - 2, cy + 3):
        for x in range(cx - 2, cx + 3):
            d2 = (x - cx) ** 2 + (y - cy) ** 2
            if d2 <= 5:
                if state < 0: c = G[0]
                else: c = [G[1], G[2], G[2]][state]
                if state >= 1 and d2 <= 2: c = G[3] if state == 2 else G[2]
                px(im, x, y, c)
            elif d2 <= 8: px(im, x, y, K)
    px(im, cx - 1, cy - 1, (255, 255, 255) if state == 2 else (G[3] if state >= 0 else (44, 90, 64)))
    if state == 2:                                   # pulso: brilho no aro
        for (x, y) in ((cx, cy - 4), (cx - 4, cy), (cx + 4, cy), (cx, cy + 4)): px(im, x, y, G[2])
    # barras de sinal (2 px), alturas 3/5/7 sobre base y=14
    bars = [(14, 3), (17, 5), (20, 7)]
    lit = {-1: 0, 0: 1, 1: 2, 2: 3}[state]
    for i, (bx, bh) in enumerate(bars):
        rect(im, bx - 1, 14 - bh - 1, bx + 2, 14, K)
        for y in range(14 - bh, 14):
            for x in (bx, bx + 1):
                on = i < lit
                c = (A[2] if x == bx else A[1]) if on else (S[1] if x == bx else S[0])
                if on and y == 14 - bh: c = A[3]
                px(im, x, y, c)
    # grelha do alto-falante
    rect(im, 24, 8, 31, 14, K)
    for y in (9, 11, 13):
        for x in range(25, 31): px(im, x, y, S[3] if x % 2 else S[2])
    for y in (10, 12):
        for x in range(25, 31): px(im, x, y, S[0])
    return im


save(build_radio(-1), 'dialogue_radio_idle')
for k in range(3): save(build_radio(k), f'dialogue_radio_active_{k}')

# ======================================================================================
# INDICADOR DE CONTINUAR 11x10 (chevron âmbar que desce e pulsa)
CW, CHH = 11, 10


def build_continue(f):
    im = canvas(CW, CHH)
    dy = [0, 1, 2][f]
    shape = [(x, y) for y in range(4) for x in range(1 + y, 10 - y)]       # triângulo 9->3
    shape2 = [(x, y + 4) for (x, y) in [(4, 0), (5, 0), (6, 0)]] + [(5, 5)] if False else []
    pts = set((x, y + 2 + dy) for (x, y) in shape) | {(5, 6 + dy)}
    pts = {p for p in pts if p[1] < CHH}
    for (x, y) in pts:
        for ddx, ddy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            q = (x + ddx, y + ddy)
            if q not in pts and 0 <= q[0] < CW and 0 <= q[1] < CHH: px(im, q[0], q[1], K)
    for (x, y) in pts:
        top = (x, y - 1) not in pts
        c = [A[1], A[2], A[2]][f]
        if top: c = [A[2], A[3], A[3]][f]
        if (x + 1, y) not in pts and not top: c = A[0] if f == 0 else A[1]
        px(im, x, y, c)
    return im


for f in range(3): save(build_continue(f), f'dialogue_continue_{f}')

meta = {
    'native_scale_note': 'arte nativa 1x; na referência 1200x720 exibir em 2x (PPU 50 para 1 unidade = 100 px de tela, ou Canvas com Reference Pixels Per Unit 100 e sprites PPU 50).',
    'palette': {'outline': K, 'slate': S, 'olive': O, 'bronze': B, 'amber': A, 'led_green': G, 'text_cream': CREAM},
    'sprites': {
        'dialogue_panel_backplate': {'size': [PW, PH], 'nine_slice_border_LRTB': [14, 14, 12, 12], 'pivot': [0.5, 0.0]},
        'dialogue_panel_frame': {'size': [PW, PH], 'nine_slice_border_LRTB': [14, 14, 12, 12], 'pivot': [0.5, 0.0], 'frame_thickness': FR},
        'dialogue_highlight': {'size': [PW, PH], 'nine_slice_border_LRTB': [14, 14, 12, 12], 'pivot': [0.5, 0.0]},
        'dialogue_nameplate': {'size': [NW, NH], 'nine_slice_border_LRTB': [10, 7, 0, 0], 'pivot': [0.0, 0.5], 'text_rect_in_source': {'x': 9, 'y_top': 3, 'height': 9, 'pad_right': 7}},
        'dialogue_nameplate_accent': {'size': [NW, NH], 'nine_slice_border_LRTB': [10, 7, 0, 0], 'pivot': [0.0, 0.5], 'tint': 'cor por falante (multiplicativa)'},
        'dialogue_radio_idle': {'size': [RW, RH], 'pivot': [1.0, 0.0]},
        'dialogue_radio_active_0..2': {'size': [RW, RH], 'pivot': [1.0, 0.0]},
        'dialogue_continue_0..2': {'size': [CW, CHH], 'pivot': [0.5, 0.5]},
    }}
json.dump(meta, open(f'{OUT_DIR}/dialogue_ui.json', 'w'), indent=1, ensure_ascii=False)
print('ok')
