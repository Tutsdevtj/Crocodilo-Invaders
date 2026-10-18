# Animações e previews: Wave UI (qualquer número), banner do boss GOOSINI e sequência ONDA 10 → GOOSINI.
import os, numpy as np
from PIL import Image, ImageDraw, ImageFont
import wave_ui as WU
import boss_ui as BU
from wave_core import over

OUT = '/home/claude/proj/wbui/Wave_Boss_UI/previews'; os.makedirs(OUT, exist_ok=True)
PK = '/home/claude/proj/package/Gusini_Boss'
BG = Image.open('/home/claude/proj/bg/Background_CeuDeBatalha/previews/01_composicao_final_x2.png').convert('RGBA')
SHIP = Image.open(f'{PK}/bombardiro/bombardiro_idle/bombardiro_idle_0.png').convert('RGBA')
HUD = Image.open('/home/claude/proj/hud/HUD_Player_v2/hud_v2_x2.png').convert('RGBA')
GAN = Image.open(f'{PK}/boss_gusini/gusini_phase1_anim/gansinho/gansinho_0.png').convert('RGBA')
BUL = Image.open(f'{PK}/boss_gusini/gusini_phase1_anim/gusini_p1_projectile/gusini_p1_bullet_0.png').convert('RGBA')
F8 = ImageFont.truetype('/mnt/user-data/uploads/Crocodilo Invaders/Assets/Fonts/PressStart2P/PressStart2P-Regular.ttf', 8)


def game(enemies=True):
    g = BG.copy(); g.alpha_composite(SHIP, (180, 330)); g.alpha_composite(HUD, (16, 16))
    if enemies:
        for (x, y) in ((900, 120), (980, 470), (1050, 300)):
            g.alpha_composite(GAN.resize((GAN.width * 2, GAN.height * 2), Image.NEAREST), (x, y))
        for (x, y) in ((760, 500), (700, 515), (840, 140)):
            g.alpha_composite(BUL.resize((BUL.width * 2, BUL.height * 2), Image.NEAREST), (x, y))
    return g


GAME, GAME_CLEAR = game(True), game(False)
x2 = lambda a: Image.fromarray(a).resize((a.shape[1] * 2, a.shape[0] * 2), Image.NEAREST)
EO = lambda u: 1 - (1 - u) ** 3
EI = lambda u: u ** 3

# =============================================================== WAVE (tempos aprovados — inalterados)
WAVE_T = dict(enter=180, settle_a=50, settle_b=50, read=520, anticip=40, exit=210)   # total 1050 ms
WAVE_TOTAL = 1050
WAVE_CY = 236


def wave_motion(t):
    if t < 180:  return 760 + (24 - 760) * EO(t / 180), 'ENTRADA'
    if t < 230:  return 24 + (-8 - 24) * EO((t - 180) / 50), 'ASSENTAMENTO'
    if t < 280:  return -8 + 8 * EO((t - 230) / 50), 'ASSENTAMENTO'
    if t < 800:  return -16 * (t - 280) / 520, 'LEITURA'
    if t < 840:  return -16 + 8 * EO((t - 800) / 40), 'SAIDA'
    return -8 + (-1000 + 8) * EI(min(1, (t - 840) / 210)), 'SAIDA'


def wave_pose(t):
    if 180 <= t < 213: return dict(pop=2, flash=True, show_burst=True)
    if 213 <= t < 246: return dict(pop=1, flash=False, show_burst=True)
    return dict(pop=0, flash=False, show_burst=t >= 180)


def wave_bbox(num):
    items, info = WU.layout(num)
    x0 = min(x for _, _, sp, x, y in items); x1 = max(x + sp.shape[1] for _, _, sp, x, y in items)
    return x0, x1, info


def draw_wave(s, num, t):
    """desenha a Wave UI no tempo t sobre a imagem de tela s (1200x720). Retorna fase."""
    if t < 0 or t >= WAVE_TOTAL: return None
    x, ph = wave_motion(t); v = wave_motion(t + 1)[0] - x
    x0, x1, info = wave_bbox(num)
    items, _ = WU.layout(num, **wave_pose(t))
    cx_local = (x0 + x1) / 2                              # centro do conjunto (px nativos)
    X = int(round(600 - cx_local * 2 + x)); X -= X % 2
    Y = WAVE_CY - 76
    L = min(130, abs(v) * 10)
    if L >= 6:
        sl = WU.speedlines(L, int(t // 33))
        s.alpha_composite(x2(sl), (X + (info['L_banner'] + 50) * 2, Y))
    for _, _, sp, ix, iy in items:
        s.alpha_composite(x2(sp), (X + ix * 2, Y + iy * 2))
    return ph


# =============================================================== BOSS
BOSS_TOTAL = 1300
BOSS_CY = 228


def boss_motion(t):
    if t < 200:   return 820 + (28 - 820) * EO(t / 200), 'ENTRADA'
    if t < 260:   return 28 + (-10 - 28) * EO((t - 200) / 60), 'IMPACTO'
    if t < 320:   return -10 + 10 * EO((t - 260) / 60), 'IMPACTO'
    if t < 1050:  return -20 * (t - 320) / 730, 'LEITURA'
    if t < 1090:  return -20 + 8 * EO((t - 1050) / 40), 'SAIDA'
    return -12 + (-1100 + 12) * EI(min(1, (t - 1090) / 210)), 'SAIDA'


def boss_pose(t):
    tag = None; flash = False; burst = False
    if t >= 200:
        tag = 'red'
        if t < 233: tag = 'flash'; flash = True
        elif t < 266: tag = 'amber'
        elif 420 <= t < 480 or 540 <= t < 600: tag = 'amber'          # 2 piscadas e para
        burst = t < 300
    return dict(title_flash=flash, tag=tag or 'red', show_tag=tag is not None, scarf_f=int(t // 90) % 4, show_burst=burst)


PARTS = [BU.feather_particle(i) for i in range(4)]


def particle_img(i, age, life=260):
    f = PARTS[(i + age // 66) % 4]
    if age > life * 0.62: f = BU.feather_dissolve(f, 1)
    if age > life * 0.82: f = BU.feather_dissolve(f, 2)
    return f


def boss_particles(t, Xof):
    """penas que ficam para trás: nascem no banner (entrada e saída) e ficam paradas no MUNDO, derivando devagar.
    Xof(t) = posição X da tela do banner no tempo t."""
    out = []
    spawns = []
    for k, ts in enumerate(range(0, 200, 33)):                 # entrada
        spawns.append((ts, k, 300 + (k * 37) % 120, 40 + (k * 29) % 70))
    for k, ts in enumerate(range(1090, 1300, 30)):              # saída: as penas se soltam
        spawns.append((ts, k + 10, 140 + (k * 53) % 420, 20 + (k * 31) % 80))
    for k in range(4):                                           # puff de saída no início da aceleração
        spawns.append((1090, k + 30, 380 + k * 70, 30 + (k * 23) % 60))
    for ts, k, lx, ly in spawns:
        age = t - ts
        if 0 <= age < 260:
            sx = Xof(ts) + lx + age * 0.18; sy = BOSS_CY - 120 + ly * 2 + age * 0.06 + (k % 3) * 6
            out.append((particle_img(k, int(age)), int(sx) // 2 * 2, int(sy) // 2 * 2))
    return out


def boss_X(t):
    x, _ = boss_motion(t)
    X = int(round(600 - BU.BOSS_W + x)); return X - X % 2


def draw_boss(s, t):
    if t < 0 or t >= BOSS_TOTAL: return None
    x, ph = boss_motion(t); v = boss_motion(t + 1)[0] - x
    X = boss_X(t); Y = BOSS_CY - BU.BOSS_H
    L = min(140, abs(v) * 10)
    if L >= 6:
        s.alpha_composite(x2(BU.boss_speedlines(L, int(t // 33))), (X + 300 * 2, Y + 20 * 2))
    for p, px, py in boss_particles(t, boss_X):
        if px < X + 60: continue
        s.alpha_composite(x2(p), (px, py))
    s.alpha_composite(x2(BU.compose(**boss_pose(t))), (X, Y))
    return ph


# =============================================================== TRANSIÇÃO ONDA → BOSS
TRANS_T0, TRANS_LEN = 1050, 150       # entre a saída da onda e a entrada do boss
SEQ_BOSS_T0 = TRANS_T0 + TRANS_LEN    # 1200


def draw_transition(s, tt):
    """3 penas atravessam a tela da direita para a esquerda (o 'vento' do Goosini anunciando a chegada)"""
    if not (0 <= tt < TRANS_LEN + 80): return None
    for k, (y, delay) in enumerate(((190, 0), (262, 30), (150, 55), (310, 75))):
        u = (tt - delay) / 140
        if 0 <= u <= 1:
            X = int(1260 - 1400 * EI(u) - 120 * u); f = PARTS[(k + int(tt // 40)) % 4]
            s.alpha_composite(x2(f), (X // 2 * 2, y))
    return 'TRANSICAO'


def label(s, txt):
    d = ImageDraw.Draw(s); d.fontmode = '1'
    d.rectangle([0, 0, s.width - 1, 14], fill=(16, 20, 48)); d.text((6, 4), txt, font=F8, fill=(255, 236, 120))


def gif(frames, path, durs):
    q = [f.convert('RGB').quantize(colors=220, dither=Image.Dither.NONE) for f in frames]
    q[0].save(path, save_all=True, append_images=q[1:], duration=durs, loop=0, optimize=False)


def band_crop(img, y0, y1):
    g = Image.new('RGBA', (1200, y1 - y0 + 15), (16, 20, 48, 255)); g.alpha_composite(img.crop((0, 0, 1200, 15)), (0, 0))
    g.alpha_composite(img.crop((0, y0, 1200, y1)), (0, 15)); return g


if __name__ == '__main__':
    # ---------------- previews estáticas Wave UI
    def wave_still(num, bg=GAME, t=500):
        s = bg.copy(); draw_wave(s, num, t); return s
    for n in (1, 10, 15, 99):
        wave_still(n).convert('RGB').save(f'{OUT}/wave_onda_{n}_no_jogo.png')
    # folhas ONDA 1–10 e 15/99 (recorte real 1:1 do jogo, centro da tela)
    def sheet(nums, cols, name):
        tiles = []
        for n in nums:
            s = GAME_CLEAR.copy(); draw_wave(s, n, 500); tiles.append((n, s.crop((260, 140, 940, 330))))
        w, h = tiles[0][1].size; rows = (len(tiles) + cols - 1) // cols
        sh = Image.new('RGB', (w * cols, (h + 16) * rows), (16, 20, 48)); d = ImageDraw.Draw(sh); d.fontmode = '1'
        for i, (n, im) in enumerate(tiles):
            cx, cy = (i % cols) * w, (i // cols) * (h + 16)
            sh.paste(im.convert('RGB'), (cx, cy + 16)); d.text((cx + 6, cy + 5), f'ONDA {n}', font=F8, fill=(255, 236, 120))
        sh.save(f'{OUT}/{name}.png')
    sheet(range(1, 11), 2, 'wave_onda_1_a_10')
    sheet([11, 15, 99], 1, 'wave_onda_11_15_99')
    # GIF da onda (ONDA 10) e quadros-chave aprovados
    frames = []; ts = list(range(0, WAVE_TOTAL + 33, 33))
    for t in ts:
        s = GAME.copy(); ph = draw_wave(s, 10, t) or 'GAMEPLAY'; label(s, f'{t:5d} ms  ONDA 10  {ph}'); frames.append(band_crop(s, 96, 456))
    gif(frames, f'{OUT}/wave_onda_10_movimento.gif', [400] + [33] * (len(frames) - 2) + [600])
    keys = [60, 120, 180, 260, 500, 820, 980]
    tiles = []
    for t in keys:
        s = GAME.copy(); ph = draw_wave(s, 10, t); tiles.append((t, ph, s.crop((150, 130, 1050, 340))))
    w, h = tiles[0][2].size; sh = Image.new('RGB', (w, (h + 16) * len(tiles)), (16, 20, 48)); d = ImageDraw.Draw(sh); d.fontmode = '1'
    for i, (t, ph, im) in enumerate(tiles):
        sh.paste(im.convert('RGB'), (0, i * (h + 16) + 16)); d.text((6, i * (h + 16) + 5), f'{t} ms  {ph}', font=F8, fill=(255, 236, 120))
    sh.save(f'{OUT}/wave_onda_10_quadros_chave.png')

    # ---------------- boss: conceito, com/sem PERIGO!, no jogo
    def on_sky(a, pad=10, sc=3):
        o = Image.new('RGBA', (a.shape[1] + pad * 2, a.shape[0] + pad * 2), (98, 160, 220, 255)); o.alpha_composite(Image.fromarray(a), (pad, pad))
        return o.resize((o.width * sc, o.height * sc), Image.NEAREST)
    on_sky(BU.compose(show_tag=False)).save(f'{OUT}/boss_goosini_conceito_x3.png')
    on_sky(BU.compose(show_tag=True)).save(f'{OUT}/boss_goosini_com_perigo_x3.png')
    s = GAME_CLEAR.copy(); draw_boss(s, 600); s.convert('RGB').save(f'{OUT}/boss_goosini_no_jogo.png')
    # GIF do boss
    frames = []; ts = list(range(0, BOSS_TOTAL + 33, 33))
    for t in ts:
        s = GAME_CLEAR.copy(); ph = draw_boss(s, t) or 'CENA DO BOSS'; label(s, f'{t:5d} ms  PERIGO! GOOSINI  {ph}'); frames.append(band_crop(s, 60, 420))
    gif(frames, f'{OUT}/boss_goosini_animacao.gif', [400] + [33] * (len(frames) - 2) + [700])
    keys = [(66, 'entrada'), (150, 'entrada'), (200, 'impacto'), (240, 'impacto'), (450, 'leitura (piscada)'), (800, 'leitura'),
            (1080, 'saida (antecipacao)'), (1160, 'saida'), (1230, 'saida (penas)')]
    tiles = []
    for t, nm in keys:
        s = GAME_CLEAR.copy(); draw_boss(s, t); tiles.append((t, nm, s.crop((60, 80, 1140, 360))))
    w, h = tiles[0][2].size; sh = Image.new('RGB', (w, (h + 16) * len(tiles)), (16, 20, 48)); d = ImageDraw.Draw(sh); d.fontmode = '1'
    for i, (t, nm, im) in enumerate(tiles):
        sh.paste(im.convert('RGB'), (0, i * (h + 16) + 16)); d.text((6, i * (h + 16) + 5), f'{t} ms  {nm}', font=F8, fill=(255, 236, 120))
    sh.save(f'{OUT}/boss_goosini_quadros_chave.png')

    # ---------------- sequência completa ONDA 10 → transição → PERIGO! GOOSINI
    SEQ = SEQ_BOSS_T0 + BOSS_TOTAL       # 2500 ms
    frames = []; ts = list(range(0, SEQ + 33, 33))
    for t in ts:
        s = GAME_CLEAR.copy()
        ph = draw_wave(s, 10, t)
        tp = draw_transition(s, t - TRANS_T0)
        pb = draw_boss(s, t - SEQ_BOSS_T0)
        lab = ph or pb or tp or ('CENA DO BOSS (dialogo existente)' if t >= SEQ else '')
        label(s, f'{t:5d} ms  {"ONDA 10" if ph else ("GOOSINI" if pb else "")}  {lab}')
        frames.append(band_crop(s, 60, 440))
    gif(frames, f'{OUT}/sequencia_onda10_goosini.gif', [500] + [33] * (len(frames) - 2) + [900])
    # tira da sequência
    keys = [120, 500, 950, 1110, 1170, 1300, 1450, 1900, 2350]
    tiles = []
    for t in keys:
        s = GAME_CLEAR.copy(); a = draw_wave(s, 10, t); b = draw_transition(s, t - TRANS_T0); c = draw_boss(s, t - SEQ_BOSS_T0)
        tiles.append((t, a and f'ONDA 10 {a}' or c and f'GOOSINI {c}' or b or '', s.crop((0, 70, 1200, 380)).resize((600, 155), Image.NEAREST)))
    w, h = tiles[0][2].size; cols = 3; rows = 3
    sh = Image.new('RGB', (w * cols, (h + 16) * rows), (16, 20, 48)); d = ImageDraw.Draw(sh); d.fontmode = '1'
    for i, (t, nm, im) in enumerate(tiles):
        cx, cy = (i % cols) * w, (i // cols) * (h + 16)
        sh.paste(im.convert('RGB'), (cx, cy + 16)); d.text((cx + 6, cy + 5), f'{t} ms  {nm}', font=F8, fill=(255, 236, 120))
    sh.save(f'{OUT}/sequencia_onda10_goosini_quadros.png')
    print('ok')
