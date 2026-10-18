# Previews da interface de diálogo (mockups — os portraits e textos NÃO fazem parte dos sprites)
import os, sys
import numpy as np
from PIL import Image, ImageDraw, ImageFont
D = '/home/claude/proj/dialogue/Dialogue_UI'; SP = f'{D}/sprites'; PV = f'{D}/previews'
os.makedirs(PV, exist_ok=True)
FONT = '/home/claude/proj/dialogue/fontsrc/PixelifySans-wght.ttf'
PS2P = '/mnt/user-data/uploads/Crocodilo Invaders/Assets/Fonts/PressStart2P/PressStart2P-Regular.ttf'
L = lambda n: Image.open(f'{SP}/{n}.png').convert('RGBA')
CREAM = (246, 236, 204); AMBER = (255, 206, 90)


def font(size, weight='Regular'):
    f = ImageFont.truetype(FONT, size)
    try: f.set_variation_by_name(weight)
    except Exception: pass
    f._w = weight
    return f


def nine(img, w, h, l, r, t, b):
    """9-slice nativo: cantos fixos, bordas e centro esticados por repetição de nearest (= Unity Sliced)"""
    W0, H0 = img.size
    out = Image.new('RGBA', (w, h))
    xs = [(0, l, 0, l), (l, W0 - r, l, w - r), (W0 - r, W0, w - r, w)]
    ys = [(0, t, 0, t), (t, H0 - b, t, h - b), (H0 - b, H0, h - b, h)]
    for (sx0, sx1, dx0, dx1) in xs:
        for (sy0, sy1, dy0, dy1) in ys:
            if dx1 <= dx0 or dy1 <= dy0: continue
            piece = img.crop((sx0, sy0, sx1, sy1)).resize((dx1 - dx0, dy1 - dy0), Image.NEAREST)
            out.alpha_composite(piece, (dx0, dy0))
    return out


def panel(w, h, highlight=False):
    p = nine(L('dialogue_panel_backplate'), w, h, 14, 14, 12, 12)
    p.alpha_composite(nine(L('dialogue_panel_frame'), w, h, 14, 14, 12, 12))
    if highlight: p.alpha_composite(nine(L('dialogue_highlight'), w, h, 14, 14, 12, 12))
    return p


def tint(img, color):
    a = np.array(img).astype(float); a[..., :3] = a[..., :3] * np.array(color) / 255.0
    return Image.fromarray(a.clip(0, 255).astype(np.uint8))


def nameplate(text, accent, f):
    tw = int(f.getlength(text))
    w = max(40, 10 + tw + 3 + 8)
    npl = nine(L('dialogue_nameplate'), w, 15, 10, 7, 0, 0)
    npl.alpha_composite(tint(nine(L('dialogue_nameplate_accent'), w, 15, 10, 7, 0, 0), accent))
    return npl, w


GX, GY = 3.2, 6.6


def glyph_px(ch, size, weight):
    F = font(size * 10, weight)
    m = Image.new('L', (size * 30, size * 20), 0); ImageDraw.Draw(m).text((GX, GY), ch, font=F, fill=255)
    arr = np.array(m, float); h_, w_ = arr.shape
    return arr.reshape(h_ // 10, 10, w_ // 10, 10).mean((1, 3)) > 127


_GC = {}


def draw_text_px(img, xy, text, f, fill):
    """cada caractere renderizado a 10x e reduzido por maioria, avanço arredondado para pixel inteiro"""
    size = f.size; weight = getattr(f, '_w', 'Regular')
    x = 0.0
    for ch in text:
        key = (ch, size, weight)
        if key not in _GC: _GC[key] = glyph_px(ch, size, weight)
        g = _GC[key]
        lay = np.zeros(g.shape + (4,), np.uint8); lay[g] = (*fill, 255)
        img.alpha_composite(Image.fromarray(lay), (int(xy[0] + round(x)), int(xy[1])))
        x += max(4, round(f.getlength(ch))) if ch == ' ' else round(f.getlength(ch))


def text_w(t, f):
    return sum((max(4, round(f.getlength(ch))) if ch == ' ' else round(f.getlength(ch))) for ch in t)


def wrap(text, f, maxw):
    words = text.split(' '); lines = []; cur = ''
    for w_ in words:
        t = (cur + ' ' + w_).strip()
        if text_w(t, f) <= maxw: cur = t
        else: lines.append(cur); cur = w_
    lines.append(cur); return lines


ACCENT = {'B. Crocodilo': (150, 190, 90), 'Gusini': (226, 70, 60), 'Gusini Corrompido': (124, 236, 72)}


def dialogue_native(name, text, radio_state=1, cont=0, w=360, h=70, highlight=False):
    """monta a interface em resolução NATIVA (1x). Retorna imagem w x (h+8) com a placa sobressaindo em cima."""
    top = 10
    im = Image.new('RGBA', (w, h + top))
    im.alpha_composite(panel(w, h, highlight), (0, top))
    fN = ImageFont.truetype(PS2P, 8); fT = font(11, 'Medium')
    npl, nw = nameplate(name, ACCENT.get(name, (200, 200, 200)), fN)
    im.alpha_composite(npl, (12, 2))
    dd = ImageDraw.Draw(im); dd.fontmode = '1'; dd.text((12 + 11, 2 + 4), name, font=fN, fill=CREAM)
    rad = L('dialogue_radio_idle' if radio_state < 0 else f'dialogue_radio_active_{radio_state}')
    im.alpha_composite(rad, (w - 12 - rad.width, top + 4 - rad.height + 4))
    lines = wrap(text, fT, w - 2 * 15)
    for i, ln in enumerate(lines[:3]):
        draw_text_px(im, (15, top + 13 + i * 15 - 3), ln, fT, CREAM)
    if cont is not None:
        c = L(f'dialogue_continue_{cont}'); im.alpha_composite(c, (w - 11 - c.width, top + h - 9 - c.height))
    return im, len(lines)


def x2(im): return im.resize((im.width * 2, im.height * 2), Image.NEAREST)


LINES = [('B. Crocodilo', 'Gusini...? Que porra tu tá fazendo? Aqueles eram dos nossos!'),
         ('Gusini', 'Eu sei. O esquadrão inteiro nunca me deixou esquecer.'),
         ('Gusini', '...Croc. Vai embora.'),
         ('B. Crocodilo', 'Nem fodendo. Não vou te deixar aqui.')]
PORT = {'B. Crocodilo': '/home/claude/proj/portraits3/Portraits_BCrocodilo/portraits/crocodilo_01_surpreso_indignado.png',
        'Gusini': '/home/claude/proj/portraits4/Portraits_Gusini_F1/portraits/gusini_p1_resentful.png'}
PORT2 = {3: '/home/claude/proj/portraits3/Portraits_BCrocodilo/portraits/crocodilo_05_determinado.png',
         2: '/home/claude/proj/portraits4/Portraits_Gusini_F1/portraits/gusini_p1_vulnerable.png'}

if __name__ == '__main__':
    # 1) painel sozinho (nativo e x4), incluindo tamanhos diferentes (teste de 9-slice)
    sz = Image.new('RGBA', (360 + 200 + 140 + 40, 90), (44, 52, 62, 255))
    x = 10
    for (w, h) in ((360, 70), (200, 52), (140, 40)):
        sz.alpha_composite(panel(w, h), (x, 10)); x += w + 10
    x2(x2(sz)).save(f'{PV}/01_painel_sozinho_9slice_x4.png'); sz.save(f'{PV}/01_painel_sozinho_x1.png')
    # 2) com nome e fala (nativo x2 = tamanho real em 1200x720) e x4
    for i, (n, t) in enumerate(LINES):
        im, nl = dialogue_native(n, t)
        b = Image.new('RGBA', im.size, (44, 52, 62, 255)); b.alpha_composite(im)
        if i == 0:
            x2(b).save(f'{PV}/02_com_nome_e_fala_tamanho_real.png'); x2(x2(b)).save(f'{PV}/04_ampliado_x4.png')
    # 3) mockups sobre a tela do jogo (1200x720): cena de diálogo pré-boss
    BG = Image.open('/home/claude/proj/bg/Background_CeuDeBatalha/previews/01_composicao_final_x2.png').convert('RGBA')
    bomb = Image.open('/home/claude/proj/package/Gusini_Boss/bombardiro/bombardiro_idle/bombardiro_idle_0.png').convert('RGBA')
    gus = Image.open('/home/claude/proj/package/Gusini_Boss/boss_gusini/gusini_phase1/gusini_phase1_assembled.png').convert('RGBA')
    HUD = Image.open('/home/claude/proj/hud/HUD_Player_v2/hud_v2_x2.png').convert('RGBA')
    shots = []
    for i, (n, t) in enumerate(LINES):
        s = BG.copy()
        s.alpha_composite(bomb.resize((bomb.width * 2 // 2 * 1, bomb.height * 2 // 2 * 1), Image.NEAREST), (180, 250))
        g2 = gus.resize((int(gus.width * 1.25), int(gus.height * 1.25)), Image.NEAREST); s.alpha_composite(g2, (1200 - g2.width - 40, 170))
        s.alpha_composite(HUD, (16, 16))
        im, _ = dialogue_native(n, t, radio_state=[1, 2, 0, 2][i], cont=[0, 1, 2, 1][i], highlight=(i in (1, 3)))
        big = x2(im)
        bx = (1200 - big.width) // 2; by = 720 - 16 - big.height
        # portraits (contexto) — falante aceso, ouvinte escurecido
        pc = Image.open(PORT2.get(i, PORT['B. Crocodilo'] if n == 'B. Crocodilo' else PORT['Gusini'])).convert('RGBA')
        pcb = x2(pc)
        other = Image.open(PORT['Gusini'] if n == 'B. Crocodilo' else PORT['B. Crocodilo']).convert('RGBA')
        ob = np.array(x2(other)).astype(float); ob[..., :3] *= 0.45; ob = Image.fromarray(ob.astype(np.uint8))
        if n == 'B. Crocodilo':
            s.alpha_composite(pcb, (bx - 250, 720 - pcb.height)); s.alpha_composite(ob, (bx + big.width - 70, 720 - ob.height))
        else:
            s.alpha_composite(ob, (bx - 250, 720 - ob.height)); s.alpha_composite(pcb, (bx + big.width - 70, 720 - pcb.height))
        s.alpha_composite(big, (bx, by))
        s.save(f'{PV}/03_mockup_jogo_{i + 1}.png'); shots.append(s)
    # 3b) painel sem portrait
    s = BG.copy(); s.alpha_composite(HUD, (16, 16))
    im, _ = dialogue_native('Gusini Corrompido', 'Então vai ter que me derrubar.', radio_state=2, cont=None)
    big = x2(im); s.alpha_composite(big, ((1200 - big.width) // 2, 720 - 16 - big.height)); s.save(f'{PV}/03b_sem_portrait.png')
    # 5) estados animados (gif x4)
    fr = []
    seq = [0, 1, 2, 1]
    for k in range(8):
        c = Image.new('RGBA', (70, 26), (40, 46, 54, 255))
        c.alpha_composite(L(f'dialogue_radio_active_{seq[k % 4]}'), (4, 5)); c.alpha_composite(L(f'dialogue_continue_{seq[k % 4]}'), (50, 9))
        fr.append(c.resize((280, 104), Image.NEAREST).convert('RGB'))
    fr[0].save(f'{PV}/05_estados_animados_x4.gif', save_all=True, append_images=fr[1:], duration=130, loop=0)
    st = Image.new('RGBA', (4 * 40 + 3 * 16 + 8, 26), (40, 46, 54, 255)); d = ImageDraw.Draw(st)
    for k, n in enumerate(['dialogue_radio_idle', 'dialogue_radio_active_0', 'dialogue_radio_active_1', 'dialogue_radio_active_2']):
        st.alpha_composite(L(n), (4 + k * 40, 4))
    for k in range(3): st.alpha_composite(L(f'dialogue_continue_{k}'), (4 + 4 * 40 + k * 16, 8))
    x2(x2(st)).save(f'{PV}/05_estados_x4.png')
    # 6) região clara x escura
    cl = Image.new('RGBA', (1200, 360))
    regions = [BG.crop((0, 300, 600, 480)), BG.crop((600, 20, 1200, 200))]
    for j, rg in enumerate(regions):
        r = rg.resize((600, 360), Image.NEAREST)
        im, _ = dialogue_native('B. Crocodilo', 'Nem fodendo. Não vou te deixar aqui.', radio_state=1, cont=0, w=280, h=62)
        r.alpha_composite(x2(im), ((600 - im.width * 2) // 2, (360 - im.height * 2) // 2)); cl.paste(r, (j * 600, 0))
    cl.save(f'{PV}/06_regiao_clara_vs_escura.png')
    print('ok')
