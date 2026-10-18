# Exporta os sprites nativos + atlas + manifesto JSON do pacote Wave_Boss_UI.
import os, json, shutil, numpy as np
from PIL import Image
import wave_ui as WU
import boss_ui as BU
import anim_all as AN
from digits import digit_sprite, DIG

ROOT = '/home/claude/proj/wbui/Wave_Boss_UI'
D = {k: f'{ROOT}/{v}' for k, v in dict(wave='Wave_UI/sprites', dig='Wave_UI/digits', bossc='Boss_UI_Common', goo='Boss_Goosini',
                                         part='Particles_Feathers', tools='tools').items()}
for p in D.values(): os.makedirs(p, exist_ok=True)
MAN = {}


def save(key, name, arr, **meta):
    arr = arr.copy(); arr[..., 3] = np.where(arr[..., 3] > 0, 255, 0)
    path = f'{D[key]}/{name}.png'; Image.fromarray(arr).save(path)
    rel = os.path.relpath(path, ROOT)
    MAN[name] = dict(file=rel, w=int(arr.shape[1]), h=int(arr.shape[0]), pivot='top_left', unity_pivot=[0, 1], **meta)
    return arr


def longest_uniform(a, lo, hi):
    best = (0, 0); s = lo
    for c in range(lo + 1, hi + 1):
        if c == hi or not (a[:, c] == a[:, s]).all():
            if c - s > best[1] - best[0]: best = (s, c)
            s = c
    return best


# ============================================================ WAVE
head, mid, end = WU.bar_parts()
save('wave', 'wave_bar_head', head, note='frente da faixa (corte inclinado + cunha). Fixa.')
save('wave', 'wave_bar_mid', mid, note='1 coluna. Repetir/esticar horizontalmente (Image Type Tiled ou escala X inteira).')
save('wave', 'wave_bar_end', end, note='fim irregular + cerdas + blocos. Seu x local 0 = L-12 (L = comprimento da faixa).')
save('wave', 'wave_word_onda', WU.ONDA)
save('wave', 'wave_burst', WU.BURST, note='estalo de impacto no canto superior direito do ÚLTIMO algarismo')
for sz, L in WU.SPEED_LEN.items():
    for ph in (0, 1):
        save('wave', f'wave_speedlines_{sz}_{ph}', WU.speedlines(L, ph), note='alternar _0/_1 a cada 33 ms')

dig_meta = {}
atlas_items = []
for d in '0123456789':
    base = WU.DS[d]
    save('dig', f'wave_digit_{d}', base)
    v = {'pop1': digit_sprite(d, WU.NH + 3), 'pop2': digit_sprite(d, WU.NH + 6), 'flash': digit_sprite(d, WU.NH + 6, flash=True)}
    offs = {}
    for k, sp in v.items():
        save('dig', f'wave_digit_{d}_{k}', sp)
        offs[k] = [-((sp.shape[1] - base.shape[1]) // 2), base.shape[0] - sp.shape[0]]
    dig_meta[d] = dict(w=int(base.shape[1]), h=int(base.shape[0]), first_x=WU.FIRST_X[d], bar_end_offset=WU.BAR_END_OFF[d],
                       burst_offset=list(WU.BURST_OFF[d]), variant_offset=offs)
    for nm in ['', '_pop1', '_pop2', '_flash']:
        atlas_items.append(f'wave_digit_{d}{nm}')
# atlas (linhas por variante)
rows = [[f'wave_digit_{d}{nm}' for d in '0123456789'] for nm in ['', '_pop1', '_pop2', '_flash']]
pad = 2; rects = {}; y = pad
aw = max(sum(MAN[n]['w'] + pad for n in r) + pad for r in rows); ah = sum(max(MAN[n]['h'] for n in r) + pad for r in rows) + pad
atlas = np.zeros((ah, aw, 4), np.uint8)
for r in rows:
    x = pad; rh = max(MAN[n]['h'] for n in r)
    for n in r:
        im = np.array(Image.open(f"{ROOT}/{MAN[n]['file']}")); atlas[y:y + im.shape[0], x:x + im.shape[1]] = im
        rects[n] = dict(x=x, y=y, w=im.shape[1], h=im.shape[0], unity_y=ah - y - im.shape[0]); x += im.shape[1] + pad
    y += rh + pad
Image.fromarray(atlas).save(f"{D['dig']}/wave_digits_atlas.png")
json.dump(dict(image='wave_digits_atlas.png', size=[aw, ah], note='y = topo (origem em cima); unity_y = origem embaixo (Sprite Editor)', rects=rects),
          open(f"{D['dig']}/wave_digits_atlas.json", 'w'), indent=1)

# ============================================================ BOSS — genérico
band = BU.band(236)
s0, s1 = longest_uniform(band, 0, band.shape[1])
hb, me, eb = band[:, :s0 + 1], band[:, s0 + 1:s0 + 2], band[:, s1 - 1:]
assert all((band[:, c] == me[:, 0]).all() for c in range(s0 + 1, s1))
BAND_HEAD_W = hb.shape[1]; BAND_END_W = eb.shape[1]
save('bossc', 'boss_band_head', hb, note='ponta afiada + cortes de brilho')
save('bossc', 'boss_band_mid', me, note='1 coluna, esticar')
save('bossc', 'boss_band_end', eb, note='fim rasgado + 3 dentes')
save('bossc', 'boss_band_full_236', band, note='faixa já montada no comprimento do GOOSINI (236)')
for st in ('red', 'amber', 'flash'):
    save('bossc', f'boss_alert_perigo_{st}', BU.alert_tag(st))
save('bossc', 'boss_impact_burst', BU.impact_burst())
for sz, L in dict(s=45, m=90, l=140).items():
    for ph in (0, 1):
        save('bossc', f'boss_speedlines_{sz}_{ph}', BU.boss_speedlines(L, ph))
pal = np.zeros((8, 8 * 6, 4), np.uint8)
for i, c in enumerate(BU.P): pal[:, i * 8:(i + 1) * 8] = (*c, 255)
Image.fromarray(pal).save(f"{D['bossc']}/boss_band_palette_goosini.png")

# ============================================================ BOSS — Goosini
save('goo', 'goosini_title', BU.title(), note='lettering GOOSINI; os dois O são os óculos azuis')
save('goo', 'goosini_title_flash', BU.title(flash=True), note='1 quadro (33 ms) no impacto')
save('goo', 'goosini_feather_long', BU.FEATHER_LONG, note='pena longa (penacho) — atrás do título, na frente da faixa')
for f in range(4): save('goo', f'goosini_scarf_{f}', BU.scarf(f), note='loop 4 quadros, 90 ms cada')
save('goo', 'goosini_beak_tip', BU.beak_tip(), note='sobre a ponta da faixa (mesma posição da faixa)')

# ============================================================ partículas
for i in range(4):
    p = BU.feather_particle(i)
    save('part', f'feather_particle_{i}', p, note='gira trocando de quadro a cada 66 ms (0→1→2→3)')
    save('part', f'feather_particle_{i}_dissolve_1', BU.feather_dissolve(p, 1))
    save('part', f'feather_particle_{i}_dissolve_2', BU.feather_dissolve(p, 2))

# ============================================================ layouts de validação
examples = {}
for n in [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 15, 99]:
    items, info = WU.layout(n)
    x0, x1, _ = AN.wave_bbox(n)
    examples[str(n)] = dict(bar_length_L=info['L'], bar_mid_columns=info['L'] - 12 - WU.HEAD_W, digit_x=info['digit_x'],
                            burst=[items[-1][3], items[-1][4]], bbox_x=[x0, x1], center_x=(x0 + x1) / 2,
                            speedlines_x=info['L_banner'] + 50)

BOSS_POS = BU.POS
manifest = {
    'meta': dict(project='Crocodilo Invaders', package='Wave_Boss_UI', version='1.0',
                 reference_resolution=[1200, 720], pixel_scale=2,
                 import_settings=dict(filter='Point (no filter)', compression='None', mipmaps=False, ppu=50,
                                      canvas_reference_ppu=100, note='todos os sprites são nativos (1x); na tela aparecem 2x'),
                 coords='px nativos, origem no canto superior esquerdo do banner, y para baixo; todos os sprites com pivô top-left (Unity pivot 0,1)',
                 alpha='somente 0/255', aa=False),
    'sprites': MAN,
    'wave': dict(
        banner_height=76,
        pieces=dict(bar=dict(x=WU.BAR_X, y=WU.BAR_Y, head_w=WU.HEAD_W, end_back=WU.END_BACK, end_fwd=WU.END_FWD, L_min=WU.L_MIN),
                    word_onda=dict(x=WU.ONDA_X, y=WU.ONDA_Y), digits_top_y=WU.DIG_Y, digits_base_y=WU.DIG_BASE),
        digits=dig_meta,
        kerning_advance=WU.ADV,
        rules=dict(
            first_digit_x='digits[d0].first_x',
            next_digit_x='x_prev + kerning_advance[d_prev + d_next]',
            bar_length_L='max(150, x_last + digits[d_last].bar_end_offset - 14)',
            bar_assembly='head em x=14; mid repetido de x=14+138 até x=14+L-12; end em x=14+L-12 (todos y=24)',
            burst='x_last + digits[d_last].burst_offset[0], y = 14 + burst_offset[1] = 8',
            speedlines='x = 14 + L + 50, y = 0; atrás de tudo; só durante entrada/saída',
            centering='centro horizontal = (bbox_x0 + bbox_x1)/2 (ver examples). Na tela: X_banner = 600 - center_x*2 + offset_motion',
            vertical='Y_banner (tela) = 236 - 76 = 160 (ajustável)',
            render_order=['speedlines', 'wave_bar (head/mid/end)', 'wave_word_onda', 'dígitos (esquerda→direita)', 'wave_burst'],
            pop='no assentamento troca o sprite do algarismo: 180–213 ms _flash, 213–246 ms _pop1, depois base. Aplicar variant_offset ao x/y do dígito; o burst desloca +(-variant_offset[0]) em x e usa topo do sprite+offset'),
        examples=examples,
        timing_ms=dict(total=1050, keyframes=[
            dict(t=0, x=760, ease='out_cubic', to_t=180, to_x=24, label='entrada (speedlines on)'),
            dict(t=180, x=24, ease='out_cubic', to_t=230, to_x=-8, label='assentamento / overshoot (burst aparece)'),
            dict(t=230, x=-8, ease='out_cubic', to_t=280, to_x=0, label='assentamento'),
            dict(t=280, x=0, ease='linear', to_t=800, to_x=-16, label='leitura (deriva lenta)'),
            dict(t=800, x=-16, ease='out_cubic', to_t=840, to_x=-8, label='micro-antecipação'),
            dict(t=840, x=-8, ease='in_cubic', to_t=1050, to_x=-1000, label='saída (speedlines on)')],
            x_unit='px de TELA (1200x720), relativo ao centro; arredondar para par',
            speedlines='comprimento = min(130, |vel px/ms| * 10) nativo → usar s(<60) / m(<105) / l; alternar _0/_1 a cada 33 ms; esconder se < 6',
            easing=dict(out_cubic='1-(1-u)^3', in_cubic='u^3'))),
    'boss': dict(
        canvas=[BU.BOSS_W, BU.BOSS_H],
        generic=dict(band=dict(x=BOSS_POS['band'][0], y=BOSS_POS['band'][1], length_goosini=236, head_w=BAND_HEAD_W, end_w=BAND_END_W,
                               length_rule='length = largura do título do boss + 26', palette='boss_band_palette_goosini.png (6 tons, escuro→claro) + contorno navy (16,20,48)'),
                     alert=dict(x=BOSS_POS['tag'][0], y=BOSS_POS['tag'][1], states=['red', 'amber', 'flash']),
                     impact_burst=dict(x=BOSS_POS['burst'][0], y=BOSS_POS['burst'][1]),
                     speedlines=dict(x=300, y=20, rule='mesma regra da onda, comprimento = min(140, |vel|*10)')),
        goosini=dict(title=dict(x=BOSS_POS['title'][0], y=BOSS_POS['title'][1]), beak_tip=dict(x=BOSS_POS['beak'][0], y=BOSS_POS['beak'][1]),
                     feather_long=dict(x=BOSS_POS['feather'][0], y=BOSS_POS['feather'][1]),
                     scarf=dict(x=BOSS_POS['scarf'][0], y=BOSS_POS['scarf'][1], frames=4, frame_ms=90, loop=True,
                                anchor='x = band.x + length + 2, y = band.y + 12')),
        render_order=['boss_speedlines', 'goosini_scarf', 'boss_band', 'goosini_beak_tip', 'goosini_feather_long', 'goosini_title', 'boss_alert_perigo', 'boss_impact_burst', 'feather_particles (camada própria, espaço de tela)'],
        timing_ms=dict(total=1300, screen_center_y=228, keyframes=[
            dict(t=0, x=820, ease='out_cubic', to_t=200, to_x=28, label='entrada (penas ficam para trás)'),
            dict(t=200, x=28, ease='out_cubic', to_t=260, to_x=-10, label='IMPACTO: title_flash 200–233, tag flash→amber→red, burst 200–300'),
            dict(t=260, x=-10, ease='out_cubic', to_t=320, to_x=0, label='assenta'),
            dict(t=320, x=0, ease='linear', to_t=1050, to_x=-20, label='leitura (cachecol tremula; PERIGO! pisca 2x)'),
            dict(t=1050, x=-20, ease='out_cubic', to_t=1090, to_x=-12, label='antecipação'),
            dict(t=1090, x=-12, ease='in_cubic', to_t=1300, to_x=-1100, label='saída (penas se soltam e se dissolvem)')],
            x_unit='px de TELA relativo a: X_banner = 600 - 340 + x',
            alert_timeline=[dict(t=[0, 200], state='hidden'), dict(t=[200, 233], state='flash'), dict(t=[233, 266], state='amber'),
                            dict(t=[266, 420], state='red'), dict(t=[420, 480], state='amber'), dict(t=[480, 540], state='red'),
                            dict(t=[540, 600], state='amber'), dict(t=[600, 1300], state='red')],
            particles=dict(life_ms=260, frame_ms=66, dissolve_1_at=0.62, dissolve_2_at=0.82,
                           drift_px_per_ms=[0.18, 0.06], world_space=True,
                           spawn_entry='a cada 33 ms de 0 a 200, no lado direito do banner (lx 300–420, ly 40–110 nativos)',
                           spawn_exit='a cada 30 ms de 1090 a 1300 + 4 de uma vez em 1090, espalhadas ao longo do banner'))),
    'sequence_wave10_to_goosini': dict(
        total_ms=2500,
        steps=[dict(t=0, what='Wave UI "ONDA 10" (1050 ms, timing normal)'),
               dict(t=1050, what='transição 150 ms: 4 feather_particle atravessam a tela da direita para a esquerda (ease-in), y 150–310 de tela'),
               dict(t=1200, what='banner PERIGO! GOOSINI (1300 ms)'),
               dict(t=2500, what='liberar a cena de entrada/diálogo existente do boss')],
        rule='nunca sobrepor: o banner do boss só começa quando a onda terminou de sair (t >= 1050) + transição'),
    'skip_cancel': dict(
        wave='Se o jogador pausar: congelar tudo. Pular (botão/skip): se t<800 saltar para t=800 (inicia a saída); nunca cortar sem saída. Se uma nova onda começar antes do fim, terminar a saída em 120 ms (acelerar o tempo restante).',
        boss='Pular durante a apresentação: se t<1050, saltar para t=1050; a saída de 250 ms sempre toca. Pular a sequência inteira: pular a onda (regra acima) e começar o boss já em t=1050.',
        gameplay='Inimigos/projéteis não devem nascer durante os 2500 ms da sequência ONDA 10 → GOOSINI (é o fim do capítulo).'),
}
json.dump(manifest, open(f'{ROOT}/wave_boss_ui.json', 'w'), indent=1, ensure_ascii=False)

for f in ('wave_core.py', 'digits.py', 'wave_ui.py', 'boss_ui.py', 'anim_all.py', 'export.py'):
    shutil.copy(f'/home/claude/proj/wbui/{f}', f"{D['tools']}/{f}")
print('sprites:', len(MAN), 'band mid', s0, s1)
