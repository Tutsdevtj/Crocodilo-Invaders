# Monta o personagem usando SOMENTE rig_manifest.json + modules/*.png (como o prefab faria na Unity)
# e compara com o master pixel a pixel. Uso: python3 verify_manifest.py <pasta Croc_Rig_Ready>
import sys, json, numpy as np
from PIL import Image
R = sys.argv[1] if len(sys.argv) > 1 else '.'
m = json.load(open(f'{R}/rig_manifest.json'))
W, H = m['canvas']; root = m['character_pivot']
def node_pos(n):                               # posição (px, y para baixo) do nó no canvas, somando local_px desde o root
    nd = m['nodes'][n]; p = nd['parent']
    base = root if p == 'CharacterRoot' else node_pos(p)
    return [base[0] + nd['local_px'][0], base[1] + nd['local_px'][1]]
out = np.zeros((H, W, 4), np.uint8); bad = []
for n in m['render_order']:
    s = m['sprites'][n]
    if n == 'eyelid_closed': continue
    img = np.array(Image.open(f"{R}/{s['file']}").convert('RGBA'))
    h, w = img.shape[:2]; px, py = node_pos(s['node'])
    x0 = px - s['unity_pivot'][0] * w; y0 = py - (1 - s['unity_pivot'][1]) * h
    if abs(x0 - round(x0)) > 1e-3 or abs(y0 - round(y0)) > 1e-3 or (round(x0), round(y0)) != (s['x'], s['y']) or (w, h) != (s['w'], s['h']):
        bad.append(n)
    x0, y0 = int(round(x0)), int(round(y0)); a = img[..., 3] > 0
    out[y0:y0 + h, x0:x0 + w][a] = img[a]
M = np.array(Image.open(f'{R}/master/croc_platformer_master.png').convert('RGBA'))
print('sprites com posição/pivô inconsistente:', bad)
print('montagem pelo manifesto == master:', bool((out == M).all()), int((out != M).any(-1).sum()), 'px diferentes')
