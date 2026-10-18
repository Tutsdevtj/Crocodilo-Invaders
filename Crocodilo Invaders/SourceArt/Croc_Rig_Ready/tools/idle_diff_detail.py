# Detalhe ampliado (x8) das diferenças restantes entre o IDLE aprovado e o IDLE reproduzido pelo rig.
import numpy as np
from PIL import Image, ImageDraw
import rigtest as RT
OUT = RT.OUT
orig = [np.array(Image.open(f'{RT.RP.SRC}/frames/croc_idle_{i:02d}.png').convert('RGBA')) for i in range(6)]
rows = []
for i in (1, 2, 4):
    n = RT.render(RT.idle_pose(RT.IDLE_TABLE[i])); d = (orig[i] != n).any(-1); ys, xs = np.where(d)
    box = (0, 40, 96, 124)
    tiles = []
    for a, lab in ((orig[i], 'IDLE APROVADO'), (n, 'IDLE PELO RIG')):
        im = RT.on(a).crop(box); im = im.resize((im.width * 6, im.height * 6), Image.NEAREST); dd = ImageDraw.Draw(im)
        for y, x in zip(ys, xs):
            X, Y = (x - box[0]) * 6, (y - box[1]) * 6; dd.rectangle([X - 1, Y - 1, X + 6, Y + 6], outline=(255, 0, 0), width=2)
        tiles.append((f'{lab}  frame {i}  ({int(d.sum())} px marcados)', im))
    rows.append(tiles)
tw, th = rows[0][0][1].size
o = Image.new('RGBA', (tw * 2 + 20, (th + 22) * len(rows)), (20, 20, 30, 255)); dr = ImageDraw.Draw(o); dr.fontmode = '1'
for r, tiles in enumerate(rows):
    for c, (lab, im) in enumerate(tiles):
        o.alpha_composite(im, (c * (tw + 20), r * (th + 22) + 18)); dr.text((c * (tw + 20) + 4, r * (th + 22) + 5), lab, font=RT.F8, fill=(255, 236, 120))
o.save(f'{OUT}/previews/idle_differences_detail.png'); print('ok', o.size)
