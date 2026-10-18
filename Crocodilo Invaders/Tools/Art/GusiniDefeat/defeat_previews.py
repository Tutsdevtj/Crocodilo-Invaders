import json, numpy as np, sys, os
sys.path.insert(0, '/home/claude/proj/transform')
from PIL import Image, ImageDraw, ImageFont
import defeat as D, transform as T
O = D.OUT
m = json.load(open(f'{O}/_tmp.json'))
F8 = ImageFont.truetype('/mnt/user-data/uploads/Crocodilo Invaders/Assets/Fonts/PressStart2P/PressStart2P-Regular.ttf', 8)
fr = [Image.open(f'{O}/' + f['file']) for f in m['frames']]; du = [f['duration_ms'] for f in m['frames']]
loop = [Image.open(f'{O}/final/gusini_p1_defeated_idle_{k}.png') for k in range(4)]
sh = Image.new('RGBA', (256 * len(fr), 196))
for i, im in enumerate(fr): sh.alpha_composite(im, (i * 256, 0))
sh.save(f'{O}/gusini_defeat_sheet.png'); sh.resize((sh.width * 2, sh.height * 2), Image.NEAREST).save(f'{O}/gusini_defeat_sheet_x2.png')
ls = Image.new('RGBA', (256 * 4, 196))
for i, im in enumerate(loop): ls.alpha_composite(im, (i * 256, 0))
ls.save(f'{O}/final/gusini_p1_defeated_idle_sheet.png')
SKY = (56, 122, 190, 255)
labels = {f['index']: f['tag'] for f in m['frames']}
seq = [(im, d, labels.get(i) or '') for i, (im, d) in enumerate(zip(fr, du))] + [(loop[k % 4], 120, 'defeated_idle (loop)') for k in range(12)]
an, ad, last = [], [], ''
for im, d, lab in seq:
    last = lab or last
    c = Image.new('RGBA', (768, 612), (30, 34, 46, 255)); b = Image.new('RGBA', (256, 196), SKY); b.alpha_composite(im)
    c.paste(b.resize((768, 588), Image.NEAREST), (0, 24)); ImageDraw.Draw(c).text((6, 8), last, font=F8, fill=(214, 255, 140))
    an.append(c.convert('RGB').quantize(colors=200, dither=Image.Dither.NONE)); ad.append(d)
an[0].save(f'{O}/previews/gusini_derrota_x3.gif', save_all=True, append_images=an[1:], duration=ad, loop=0)
BG = Image.open('/home/claude/proj/bg/Background_CeuDeBatalha/previews/01_composicao_final_x2.png').convert('RGBA')
bomb = Image.open('/home/claude/proj/package/Gusini_Boss/bombardiro/bombardiro_idle/bombardiro_idle_0.png').convert('RGBA')
BB = Image.open('/home/claude/proj/hud/BossBar_Gusini_F2/bossbar_f2_x2.png').convert('RGBA')
an, ad = [], []
for k, (im, d, lab) in enumerate(seq):
    s = BG.copy(); s.alpha_composite(bomb, (200, 280)); s.alpha_composite(im.resize((320, 245), Image.NEAREST), (820, 190))
    if k < 2: s.alpha_composite(BB, (1200 - 16 - BB.width, 720 - 14 - BB.height))
    an.append(s.resize((600, 360), Image.NEAREST).convert('RGB').quantize(colors=220, dither=Image.Dither.NONE)); ad.append(d)
an[0].save(f'{O}/previews/gusini_derrota_em_tela.gif', save_all=True, append_images=an[1:], duration=ad, loop=0)
keys = [0, 2, 6, 7, 8, 11, 14, 17]
kk = Image.new('RGBA', (4 * 512, 2 * 412), (30, 34, 46, 255)); d = ImageDraw.Draw(kk)
for j, i in enumerate(keys):
    b = Image.new('RGBA', (256, 196), SKY); b.alpha_composite(fr[i]); x, y = (j % 4) * 512, (j // 4) * 412
    kk.paste(b.resize((512, 392), Image.NEAREST), (x, y + 20)); d.text((x + 6, y + 6), f"F{i} {m['frames'][i]['start_ms']}ms {m['frames'][i]['tag'] or ''}", font=F8, fill=(214, 255, 140))
kk.save(f'{O}/previews/quadros_chave_x2.png')
# O QUE MUDOU: v1 x v2 (loop final) + mapa de danos sobre a F1 limpa
v1 = Image.open('/home/claude/proj/transform/Gusini_Defeat_F2_F1_v1/final/gusini_p1_defeated_idle_0.png')
c = Image.new('RGBA', (1590, 470), (30, 34, 46, 255)); d = ImageDraw.Draw(c)
clean = T.pose({'*': 1}, with_fx=False); dmg = D.pose({'*': 1}, tired=True)
diff = (np.abs(clean.astype(int) - dmg.astype(int)).sum(-1) > 0)
hl = np.array(Image.fromarray(dmg)).astype(int); g = hl.copy(); g[..., :3] = (g[..., :3] * 0.35 + 30).astype(int)
g[diff] = (255, 70, 60, 255); g[hl[..., 3] == 0] = 0
for j, (im, lab) in enumerate([(v1, 'ANTES (v1): nave limpa demais'), (loop[0], 'AGORA (v2): nave avariada + resto do alien'), (Image.fromarray(np.uint8(g)), 'MAPA: tudo que foi danificado (vermelho)')]):
    b = Image.new('RGBA', (256, 196), SKY); b.alpha_composite(im); c.paste(b.resize((512, 392), Image.NEAREST), (10 + j * 528, 60))
    d.text((10 + j * 528, 36), lab, font=F8, fill=(214, 255, 140) if j else (255, 160, 140))
c.save(f'{O}/previews/o_que_mudou_v1_v2.png')
# zoom na nave final x4
b = Image.new('RGBA', (256, 196), SKY); b.alpha_composite(loop[0]); b.crop((20, 20, 256, 176)).resize((944, 624), Image.NEAREST).save(f'{O}/previews/gusini_recuperado_final_x4.png')
cmp_ = Image.new('RGBA', (1060, 460), (30, 34, 46, 255)); d = ImageDraw.Draw(cmp_)
for j, (im, lab) in enumerate([(Image.fromarray(clean), 'FASE 1 (inicio da luta)'), (loop[0], 'GUSINI RECUPERADO (pos-batalha)')]):
    b = Image.new('RGBA', (256, 196), SKY); b.alpha_composite(im); cmp_.paste(b.resize((512, 392), Image.NEAREST), (10 + j * 530, 50)); d.text((10 + j * 530, 24), lab, font=F8, fill=(214, 255, 140))
cmp_.save(f'{O}/previews/fase1_vs_recuperado_x2.png')
print('ok')
