# B. Crocodilo — preparação para rig 2D (Unity). Parte do pacote IDLE aprovado (módulos + subpintura originais)
# e só ACRESCENTA estrutura escondida: cortes nas articulações, juntas "esfera" (disco do pai + anel de contorno),
# preenchimentos ocultos desenhados com a textura do próprio uniforme e uma pelve oculta.
import os, json, numpy as np
from PIL import Image
from scipy import ndimage

SRC = '/home/claude/proj/crocidle/Croc_Idle_Modular'
OUT = '/home/claude/proj/crocrig/Croc_Rig_Ready'
for d in ('master', 'modules', 'idle', 'previews'): os.makedirs(f'{OUT}/{d}', exist_ok=True)
M = np.array(Image.open(f'{SRC}/croc_platformer_master.png').convert('RGBA'))
H, W = M.shape[:2]
OP = M[..., 3] > 0
K = np.array([13, 13, 21])
YY, XX = np.mgrid[0:H, 0:W]
OLD = json.load(open(f'{SRC}/croc_idle.json'))


def old_full(name):
    m = OLD['modules'][name]; img = np.array(Image.open(f"{SRC}/{m['file']}").convert('RGBA'))
    o = np.zeros_like(M); o[m['y']:m['y'] + img.shape[0], m['x']:m['x'] + img.shape[1]] = img
    return o


OLDIMG = {n: old_full(n) for n in OLD['modules']}
# dono VISÍVEL de cada pixel no repouso = módulo mais à frente que tem o pixel com a cor do master
old_order = OLD['render_order']
owner_old = np.full((H, W), '', dtype=object)
for n in old_order:
    a = OLDIMG[n][..., 3] > 0
    owner_old[a & OP] = n
assert (owner_old[OP] != '').all()

# ---------------------------------------------------------------- articulações medidas na arte aprovada (px do canvas 96x184)
JOINT = dict(neck=(49, 58),
             shoulder_front=(21, 60), elbow_front=(15, 82), wrist_front=(17, 103),
             shoulder_back=(65, 61), elbow_back=(68, 83), wrist_back=(72, 102),
             hip_front=(36, 100), knee_front=(31, 131), ankle_front=(26, 158),
             hip_back=(56, 100), knee_back=(58, 131), ankle_back=(59, 158),
             hips=(46, 94))
DISC = dict(elbow_front=7, elbow_back=5, knee_front=9, knee_back=8)       # raio da "esfera" (≈ meia largura do membro)

# ---------------------------------------------------------------- nova partição
def split(old, joint, r):
    jx, jy = JOINT[joint]
    m = owner_old == old
    disc = (XX - jx) ** 2 + (YY - jy) ** 2 <= r * r
    upper = m & ((YY < jy) | disc)
    return upper, m & ~upper, disc


own = {}
for n in ('helmet', 'head', 'torso', 'hips', 'hand_front', 'hand_back', 'boot_front', 'boot_back'):
    own[n] = owner_old == n
own['arm_front_upper'], own['arm_front_lower'], DF = split('arm_front', 'elbow_front', DISC['elbow_front'])
own['arm_back_upper'], own['arm_back_lower'], DB = split('arm_back', 'elbow_back', DISC['elbow_back'])
own['leg_front_upper'], own['leg_front_lower'], KF = split('leg_front', 'knee_front', DISC['knee_front'])
own['leg_back_upper'], own['leg_back_lower'], KB = split('leg_back', 'knee_back', DISC['knee_back'])

# fronteira braço da frente / corpo: tudo que está à direita da fenda (ou da linha da axila) é CORPO, não braço
_line = {64: 29, 65: 29, 66: 29, 67: 29, 68: 28, 69: 28, 70: 28, 71: 28, 72: 27, 73: 27, 74: 27, 75: 26, 76: 26, 77: 26, 78: 25}
for y in range(50, 113):
    arm_row = own['arm_front_upper'][y] | own['arm_front_lower'][y]
    if not arm_row.any(): continue
    if y in _line: cut = _line[y] + 1
    else:
        gap = np.where(~OP[y, 10:40])[0] + 10
        gap = gap[(gap > 18)]
        if not len(gap): continue
        cut = gap.max() + 1
    body = arm_row & (XX[y] >= cut)
    if not body.any(): continue
    dest = 'torso' if y < 86 else 'hips' if y <= 101 else 'leg_front_upper'
    for p in ('arm_front_upper', 'arm_front_lower'): own[p][y] &= ~body
    own[dest][y] |= body
# pedacinhos soltos de mangas/luvas de trás que na verdade são contorno da calça → voltam para a perna
for mv, st in (('arm_back_lower', 'leg_back_upper'), ('hand_back', 'leg_back_upper'), ('arm_front_lower', 'leg_front_upper')):
    lab, k = _nd.label(own[mv], structure=np.ones((3, 3))) if False else (None, 0)

# pedacinhos VISÍVEIS soltos de peças paradas (ex.: contorno da luva que caiu no recorte da perna) passam para a
# peça móvel mais próxima — assim acompanham a mão/braço em vez de ficar flutuando
from scipy import ndimage as _nd
REASSIGNED = []
for st in ('leg_front', 'leg_back', 'hips', 'torso'):
    pass
_static = {'leg_front_upper': ['hand_front', 'arm_front_lower'], 'leg_back_upper': ['hand_back', 'arm_back_lower'],
           'hips': ['hand_front', 'hand_back', 'arm_front_lower', 'arm_back_lower'], 'torso': ['arm_front_upper', 'arm_back_upper']}
for st, movers in _static.items():
    lab, k = _nd.label(own[st], structure=np.ones((3, 3)))
    if k <= 1: continue
    sizes = _nd.sum(own[st], lab, range(1, k + 1)); main = 1 + int(np.argmax(sizes))
    for i in range(1, k + 1):
        if i == main: continue
        comp = lab == i
        dists = {mv: _nd.distance_transform_edt(~own[mv])[comp].min() for mv in movers}
        mv = min(dists, key=dists.get)
        own[st] &= ~comp; own[mv] |= comp; REASSIGNED.append((st, mv, int(comp.sum())))
for mv, st in (('arm_back_lower', 'leg_back_upper'), ('hand_back', 'leg_back_upper'), ('arm_front_lower', 'leg_front_upper'), ('hand_front', 'leg_front_upper')):
    lab, k = _nd.label(own[mv], structure=np.ones((3, 3)))
    if k <= 1: continue
    sizes = _nd.sum(own[mv], lab, range(1, k + 1)); main = 1 + int(np.argmax(sizes))
    for i in range(1, k + 1):
        comp = lab == i
        if i != main and comp.sum() < 15 and (YY[comp] >= 100).all():
            own[mv] &= ~comp; own[st] |= comp; REASSIGNED.append((mv, st, int(comp.sum())))
print('pedaços visíveis soltos reatribuídos:', REASSIGNED)

ORDER = ['pelvis_under', 'boot_back', 'ring_knee_back', 'leg_back_lower', 'leg_back_upper',
         'boot_front', 'ring_knee_front', 'leg_front_lower', 'leg_front_upper', 'hips',
         'ring_elbow_back', 'arm_back_lower', 'arm_back_upper', 'hand_back', 'torso', 'head', 'helmet', 'eyelid_closed',
         'ring_elbow_front', 'arm_front_lower', 'arm_front_upper', 'hand_front']
RANK = {n: i for i, n in enumerate(ORDER)}
vis_owner = np.full((H, W), '', dtype=object)
for n, m in own.items(): vis_owner[m] = n
assert (vis_owner[OP] != '').all() and (vis_owner[~OP] == '').all()
rank_map = np.full((H, W), -1)
for n in own: rank_map[own[n]] = RANK[n]


def hidden_for(n):
    """pixels onde a peça n pode ter pintura escondida: opacos no master e cobertos por peça mais à frente"""
    return OP & (rank_map > RANK[n])


OLD_OF = dict(arm_front_upper='arm_front', arm_front_lower='arm_front', arm_back_upper='arm_back', arm_back_lower='arm_back',
              leg_front_upper='leg_front', leg_front_lower='leg_front', leg_back_upper='leg_back', leg_back_lower='leg_back')

LAY = {}
REPORT = {}
for n in own:
    o = np.zeros_like(M); o[own[n]] = M[own[n]]
    # subpintura ORIGINAL do idle aprovado (mantida pixel a pixel → o idle reproduz idêntico)
    old = OLD_OF.get(n, n); src = OLDIMG[old]
    sub = (src[..., 3] > 0) & ~(owner_old == old) & hidden_for(n)
    if old != n:                                   # divide a subpintura entre as duas metades pelo lado da junta
        jn = 'elbow_front' if old == 'arm_front' else 'elbow_back' if old == 'arm_back' else 'knee_front' if old == 'leg_front' else 'knee_back'
        jy = JOINT[jn][1]
        sub &= (YY < jy) if n.endswith('upper') else (YY >= jy)
    o[sub] = src[sub]
    LAY[n] = o
OLDSUB = {n: (LAY[n][..., 3] > 0) & ~own[n] for n in LAY}      # pixels de subpintura antiga
# Só fica a subpintura antiga que aparece em algum frame do idle aprovado (garante o idle idêntico). O resto era
# "esticamento" de 3 px que viraria fragmento solto quando um membro se move; ele é removido e trocado por
# continuações anatômicas (abaixo).
_NEED = np.load('/tmp/claude-0/need.npy', allow_pickle=True).item() if os.path.exists('/tmp/claude-0/need.npy') else None
if _NEED is not None:
    for n in LAY:
        drop = OLDSUB[n] & ~_NEED[n]
        LAY[n][drop] = 0
        OLDSUB[n] &= _NEED[n]


def add_fill(n, region, colors, note):
    """acrescenta preenchimento oculto só onde: escondido no repouso e ainda sem pintura (não toca na subpintura antiga)"""
    free = region & hidden_for(n) & (LAY[n][..., 3] == 0)
    LAY[n][free] = colors[free]
    REPORT.setdefault(n, []).append((note, int(free.sum())))
    return free


def shifted(dx, dy, mask):
    """textura do próprio master deslocada (cópia de bloco do tecido, não esticamento de borda)"""
    o = np.zeros_like(M)
    ys, xs = np.where(mask); Y, X = ys - dy, xs - dx
    ok = (Y >= 0) & (Y < H) & (X >= 0) & (X < W)
    o[Y[ok], X[ok]] = M[ys[ok], xs[ok]]
    return o


def textured(region, src_mask, candidates):
    """preenche a região com a textura de src_mask copiada por deslocamentos (o primeiro que cobrir cada pixel)"""
    o = np.zeros_like(M)
    for dx, dy in candidates:
        t = shifted(dx, dy, src_mask)
        put = region & (o[..., 3] == 0) & (t[..., 3] > 0) & ~np.all(t[..., :3] == K, -1)
        o[put] = t[put]
    rest = region & (o[..., 3] == 0)
    if rest.any():
        filled = o[..., 3] > 0
        if filled.any():
            _, (iy, ix) = ndimage.distance_transform_edt(~filled, return_indices=True); o[rest] = o[iy[rest], ix[rest]]
    return o


def outline_side(region, side):
    """contorno de 1 px na borda da região voltada para 'side' (left/right/bottom)"""
    e = np.zeros_like(region)
    for y in range(H):
        xs = np.where(region[y])[0]
        if not len(xs): continue
        if side == 'left': e[y, xs.min()] = True
        if side == 'right': e[y, xs.max()] = True
    if side == 'bottom':
        for x in range(W):
            ys = np.where(region[:, x])[0]
            if len(ys): e[ys.max(), x] = True
    return e


KC = np.zeros_like(M); KC[...] = (*K, 255)

# --- 1) cotovelos/joelhos: a peça de baixo leva por baixo do disco os PIXELS REAIS do master (superfície verdadeira)
for low, disc in (('arm_front_lower', DF), ('arm_back_lower', DB), ('leg_front_lower', KF), ('leg_back_lower', KB)):
    reg = disc & own[low.replace('lower', 'upper')]
    add_fill(low, reg, M, 'disco da articulação (pixels reais do master)')

# --- 2) ombro da frente: lateral do tronco escondida sob o braço (textura da jaqueta do próprio tronco + contorno lateral)
reg = (own['arm_front_upper'] | own['arm_front_lower']) & (XX >= 23) & (YY >= 50) & (YY <= 80)
tex = textured(reg, own['torso'], [(8, 0), (10, 0), (12, 0), (8, 2), (14, 0)])
f = add_fill('torso', reg, tex, 'lateral do tronco sob o braço da frente (textura da jaqueta)')
edge = outline_side(f, 'left'); LAY['torso'][edge & f] = (*K, 255)

# --- 3) braço de trás: o master só mostra a metade de fora da manga (a de dentro some atrás do tronco).
#        Desenho a metade escondida: 4 px de manga sólida (2 tons do uniforme, luz da arte) + contorno, por baixo do tronco.
#        Só a parte de cima do braço (até o cotovelo), que é o que aparece quando ele balança.
armb = own['arm_back_upper']
reg = np.zeros((H, W), bool); edge = np.zeros((H, W), bool)
for y in range(54, JOINT['elbow_back'][1] + 1):
    xs = np.where(armb[y])[0]
    if not len(xs): continue
    x0 = xs.min(); reg[y, max(0, x0 - 4):x0] = True; edge[y, max(0, x0 - 4)] = True
reg &= own['torso']
tex = np.zeros_like(M); tex[reg] = (64, 65, 34, 255); tex[reg & (np.roll(reg, -2, 1) == False)] = (47, 48, 29, 255)
tex[reg & ((XX + YY) % 7 == 0)] = (47, 48, 29, 255)          # manchas da camuflagem no mesmo ritmo do tecido
f = add_fill('arm_back_upper', reg, tex, 'metade interna da manga de trás (escondida atrás do tronco)')
LAY['arm_back_upper'][edge & f] = (*K, 255)

# --- 4) quadril: coxas sobem por baixo da aba da jaqueta (textura da calça da própria coxa)
for side, x0, x1 in (('front', 24, 46), ('back', 47, 67)):
    reg = own['hips'] & (YY >= 90) & (XX >= x0) & (XX <= x1)
    tex = textured(reg, own[f'leg_{side}_upper'], [(0, -12), (0, -10), (0, -14), (1, -12)])
    add_fill(f'leg_{side}_upper', reg, tex, 'coxa sob a aba da jaqueta (textura da calça)')

# --- 5) PELVE oculta (assento/virilha): atrás das coxas, aparece quando uma coxa vai à frente e a outra atrás
pel = ((XX - 46) / 20.0) ** 2 + ((YY - 106) / 12.0) ** 2 <= 1
pel &= (own['hips'] | own['leg_front_upper'] | own['leg_back_upper'])
LAY['pelvis_under'] = np.zeros_like(M)
legs_up = own['leg_front_upper'] | own['leg_back_upper']
tex = textured(pel, legs_up, [(0, 0), (0, -12), (0, -10), (2, -12), (-2, -12)])
f = add_fill('pelvis_under', pel, tex, 'pelve/assento oculto (textura da calça)')
LAY['pelvis_under'][outline_side(f, 'bottom') & f & (YY > 101)] = (*K, 255)

# --- 6) tornozelos: cano da bota continua por baixo da barra da calça
for side in ('front', 'back'):
    b = own[f'boot_{side}']; top = np.where(b.any(1))[0].min()
    cols = np.where(b[top:top + 4].any(0))[0]
    reg = own[f'leg_{side}_lower'] & (YY >= top - 7) & (YY < top) & (XX >= cols.min()) & (XX <= cols.max())
    tex = textured(reg, b, [(0, -6), (0, -7), (0, -5), (0, -8)])
    add_fill(f'boot_{side}', reg, tex, 'cano da bota sob a barra da calça (textura da bota)')

# --- 7) pulsos: punho da manga continua por baixo da luva
for side in ('front', 'back'):
    hnd = own[f'hand_{side}']; wy = JOINT[f'wrist_{side}'][1]
    reg = hnd & (YY <= wy + 6)
    tex = textured(reg, own[f'arm_{side}_lower'], [(0, 6), (0, 5), (0, 7), (0, 8)])
    add_fill(f'arm_{side}_lower', reg, tex, 'punho da manga sob a luva (textura da manga)')

# --- 8) anéis de contorno das articulações (metade de baixo, escondidos sob a peça de baixo no repouso)
for ring, jn, low in (('ring_elbow_front', 'elbow_front', 'arm_front_lower'), ('ring_elbow_back', 'elbow_back', 'arm_back_lower'),
                      ('ring_knee_front', 'knee_front', 'leg_front_lower'), ('ring_knee_back', 'knee_back', 'leg_back_lower')):
    jx, jy = JOINT[jn]; r = DISC[jn]
    d = np.hypot(XX - jx, YY - jy)
    reg = (d >= r + 0.5) & (d < r + 1.5) & (YY >= jy) & own[low]
    LAY[ring] = np.zeros_like(M); LAY[ring][reg & hidden_for(ring)] = (*K, 255)
    REPORT[ring] = [('anel de contorno da junta (aparece só na parte de fora da dobra)', int((reg & hidden_for(ring)).sum()))]

# --- 9) continuação anatômica de peças paradas ATRÁS dos membros móveis (borda real interpolada + contorno)
def continuation(piece, occ, side):
    pm = own[piece]; reg = np.zeros_like(pm); edges = {}
    rows = [y for y in range(H) if pm[y].any()]
    occl = {}
    for y in rows:
        xs = np.where(pm[y])[0]; e = xs.min() if side == 'left' else xs.max()
        nb = e - 1 if side == 'left' else e + 1
        occl[y] = 0 <= nb < W and occ[y, nb]
        edges[y] = e
    free = [y for y in rows if not occl[y]]
    for y in rows:
        if not occl[y]: continue
        up = [f for f in free if f < y]; dn = [f for f in free if f > y]
        if not up or not dn: continue
        a, b = up[-1], dn[0]; t = (y - a) / (b - a)
        tx = int(round(edges[a] + (edges[b] - edges[a]) * t))
        if side == 'left' and tx < edges[y]: reg[y, tx:edges[y]] = True
        if side == 'right' and tx > edges[y]: reg[y, edges[y] + 1:tx + 1] = True
    reg &= occ
    return reg


def add_continuation(target, piece, occ_names, side, note, dxs):
    occ = np.zeros((H, W), bool)
    for o_ in occ_names: occ |= own[o_]
    reg = continuation(piece, occ, side)
    tex = textured(reg, own[piece], [(d, 0) for d in dxs])
    f = add_fill(target, reg, tex, note)
    LAY[target][outline_side(f, side) & f] = (*K, 255)


ARMF = ['arm_front_upper', 'arm_front_lower', 'hand_front']; ARMB = ['arm_back_upper', 'arm_back_lower', 'hand_back']
add_continuation('leg_front_upper', 'leg_front_upper', ARMF, 'left', 'coxa continua atrás da mão da frente', [-3, -5, -7])
add_continuation('hips', 'hips', ARMF, 'left', 'aba continua atrás do braço da frente', [-3, -5, -7])
add_continuation('leg_back_upper', 'leg_back_upper', ARMB, 'right', 'coxa de trás continua atrás da mão de trás', [3, 5, 7])
add_continuation('hips', 'hips', ARMB, 'right', 'aba continua atrás do braço de trás', [3, 5, 7])
# lateral direita do tronco atrás do braço de trás: o tronco fica NA FRENTE do braço, então o contorno da lateral
# vai num sprite próprio abaixo do braço (escondido por ele no repouso)
ts = np.zeros((H, W), bool)
for y in range(50, 90):
    xs = np.where(own['torso'][y])[0]
    if len(xs) and xs.max() + 1 < W and (own['arm_back_upper'][y, xs.max() + 1] or own['arm_back_lower'][y, xs.max() + 1]):
        ts[y, xs.max() + 1] = True
RANK['torso_side_back'] = RANK['ring_elbow_back'] - 0.5
LAY['torso_side_back'] = np.zeros_like(M)
f = ts & OP & (rank_map > RANK['torso_side_back'])
LAY['torso_side_back'][f] = (*K, 255)
REPORT['torso_side_back'] = [('contorno da lateral direita do tronco (aparece quando o braço de trás sai da frente)', int(f.sum()))]
ORDER.insert(ORDER.index('ring_elbow_back'), 'torso_side_back')

# --- 10) nenhum pedaço de pintura oculta pode ficar solto: componentes pequenos (< 20 px) feitos só de
#          pintura escondida (não visível no repouso) são removidos — eles virariam fragmentos voando nas dobras
DROPPED = {}
for n in list(LAY):
    if n.startswith('ring_') or n in ('eyelid_closed', 'torso_side_back'): continue
    a = LAY[n][..., 3] > 0
    lab, k = ndimage.label(a, structure=np.ones((3, 3)))
    for i in range(1, k + 1):
        comp = lab == i
        if comp.sum() < 20 and not (comp & own.get(n, np.zeros_like(comp))).any() and not ((comp & OLDSUB.get(n, np.zeros_like(comp))).any() and not n.startswith('leg_')):
            # (subpintura antiga necessária ao idle é mantida quando fica sob a peça que a cobre e se move junto —
            #  tronco sob a cabeça, manga sob a luva; nas coxas ela ficaria solta fora da silhueta quando a mão sai)
            LAY[n][comp] = 0; DROPPED[n] = DROPPED.get(n, 0) + int(comp.sum())
print('fragmentos ocultos removidos:', DROPPED)
# ponta diagonal da subpintura antiga da aba (canto direito, sob o braço de trás): vira um pixel solto
# quando o braço de trás sai da frente → removida
LAY['hips'][101, 66] = 0


# --- pálpebra (overlay do idle, inalterada)
LAY['eyelid_closed'] = OLDIMG['eyelid_closed']


# ---------------------------------------------------------------- montagem / verificação
def compose(offsets=None, layers=LAY, order=ORDER, skip=('eyelid_closed',)):
    offsets = offsets or {}
    o = np.zeros_like(M)
    for n in order:
        if n in skip: continue
        L = layers[n]; dy, dx = offsets.get(n, (0, 0))
        a = L[..., 3] > 0; ys, xs = np.where(a); Y, X = ys + dy, xs + dx
        ok = (Y >= 0) & (Y < H) & (X >= 0) & (X < W); o[Y[ok], X[ok]] = L[ys[ok], xs[ok]]
    return o


REST = compose()
assert (REST == M).all(), 'repouso difere do master'
print('repouso == master: OK (pixel a pixel)')
for n, r in REPORT.items(): print(n, r)
np.save('/tmp/claude-0/rig_layers.npy', {k: v for k, v in LAY.items()}, allow_pickle=True)
