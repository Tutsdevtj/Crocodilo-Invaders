import numpy as np
from PIL import Image, ImageDraw, ImageFont
import rigtest as RT
F8 = RT.F8; OUT = RT.OUT; CW, SH = 140, 22
ARM = [('estendido', 0, 0), ('parcial', 0, 35), ('mais flexionado', 0, 80), ('ombro p/ tras + cotovelo', -35, 50), ('ombro p/ frente + cotovelo', 35, 90)]
LEG = [('estendida', 0, 0), ('joelho parcial', 0, -35), ('joelho mais dobrado', 0, -75), ('coxa a frente', 35, -40), ('coxa atras', -30, -60)]

def arm_pose(side, sh, el, wr=0):
    S = 'Front' if side == 'front' else 'Back'
    return {f'Arm{S}Upper': (sh, 0, 0), f'Arm{S}Lower': (el, 0, 0), f'Hand{S}': (wr, 0, 0)}

def leg_pose(side, hp, kn, an=0):
    S = 'Front' if side == 'front' else 'Back'
    return {f'Leg{S}Upper': (hp, 0, 0), f'Leg{S}Lower': (kn, 0, 0), f'Boot{S}': (an, 0, 0)}

def sheet(name, cases, posefn, scale=3):
    rows = []
    for side in ('front', 'back'):
        row = []
        for lab, a, b in cases:
            o = RT.render(posefn(side, a, b), canvas=(CW, 184), shift=(SH, 0))
            row.append((f'{side}: {lab}', o))
        rows.append(row)
    cw, ch = CW * scale, 184 * scale
    img = Image.new('RGBA', (cw * len(cases), (ch + 16) * 2 + (184 + 16) * 2), (20, 20, 30, 255)); d = ImageDraw.Draw(img); d.fontmode = '1'
    for r, row in enumerate(rows):
        for c, (lab, o) in enumerate(row):
            img.alpha_composite(RT.on(o).resize((cw, ch), Image.NEAREST), (c * cw, r * (ch + 16) + 16)); d.text((c * cw + 4, r * (ch + 16) + 4), lab, font=F8, fill=(255, 236, 120))
    # linha em tamanho real
    y0 = (ch + 16) * 2
    for r, row in enumerate(rows):
        for c, (lab, o) in enumerate(row):
            img.alpha_composite(RT.on(o), (c * cw + 4, y0 + r * (184 + 16) + 12))
    d.text((4, y0 + 2), 'tamanho real (1:1)', font=F8, fill=(255, 236, 120))
    img.save(f'{OUT}/previews/{name}.png')
    return rows

def gif(name, cases, posefn, side_list=('front', 'back')):
    seq = []
    for side in side_list:
        keys = [(a, b) for _, a, b in cases] + [(0, 0)]
        for (a0, b0), (a1, b1) in zip(keys[:-1], keys[1:]):
            for t in np.linspace(0, 1, 6, endpoint=False):
                o = RT.render(posefn(side, a0 + (a1 - a0) * t, b0 + (b1 - b0) * t), canvas=(CW, 184), shift=(SH, 0))
                im = RT.on(o).resize((CW * 3, 184 * 3), Image.NEAREST); d = ImageDraw.Draw(im); d.fontmode = '1'
                d.text((6, 6), f'{side}', font=F8, fill=(255, 255, 255))
                seq.append(im.convert('RGB').quantize(colors=96, dither=Image.Dither.NONE))
    seq[0].save(f'{OUT}/previews/{name}.gif', save_all=True, append_images=seq[1:], duration=90, loop=0)

if __name__ == '__main__':
    sheet('arm_bending_test_poses', ARM, arm_pose)
    sheet('leg_bending_test_poses', LEG, leg_pose)
    gif('arm_bending_test', ARM, arm_pose)
    gif('leg_bending_test', LEG, leg_pose)
    print('ok')
