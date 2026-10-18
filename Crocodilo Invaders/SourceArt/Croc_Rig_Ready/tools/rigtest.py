# Testes do rig: hierarquia de Transforms com rotação nearest-neighbor (como SpriteRenderer com filtro Point na Unity),
# reprodução do IDLE original e previews.
import os, json, numpy as np
from PIL import Image, ImageDraw, ImageFont
import rigprep as RP

M, LAY, J, H, W = RP.M, RP.LAY, RP.JOINT, RP.H, RP.W
OUT = RP.OUT
F8 = ImageFont.truetype('/mnt/user-data/uploads/Crocodilo Invaders/Assets/Fonts/PressStart2P/PressStart2P-Regular.ttf', 8)

# hierarquia: nó → (pai, pivô no canvas, sprites)
TORSO_PIV = (46, 86)
NODES = {
    'Hips': (None, J['hips'], ['hips', 'pelvis_under']),
    'Torso': ('Hips', TORSO_PIV, ['torso', 'torso_side_back']),
    'NeckHead': ('Torso', J['neck'], ['head', 'helmet', 'eyelid_closed']),
    'ArmBackUpper': ('Torso', J['shoulder_back'], ['arm_back_upper', 'ring_elbow_back']),
    'ArmBackLower': ('ArmBackUpper', J['elbow_back'], ['arm_back_lower']),
    'HandBack': ('ArmBackLower', J['wrist_back'], ['hand_back']),
    'ArmFrontUpper': ('Torso', J['shoulder_front'], ['arm_front_upper', 'ring_elbow_front']),
    'ArmFrontLower': ('ArmFrontUpper', J['elbow_front'], ['arm_front_lower']),
    'HandFront': ('ArmFrontLower', J['wrist_front'], ['hand_front']),
    'LegBackUpper': ('Hips', J['hip_back'], ['leg_back_upper', 'ring_knee_back']),
    'LegBackLower': ('LegBackUpper', J['knee_back'], ['leg_back_lower']),
    'BootBack': ('LegBackLower', J['ankle_back'], ['boot_back']),
    'LegFrontUpper': ('Hips', J['hip_front'], ['leg_front_upper', 'ring_knee_front']),
    'LegFrontLower': ('LegFrontUpper', J['knee_front'], ['leg_front_lower']),
    'BootFront': ('LegFrontLower', J['ankle_front'], ['boot_front']),
}
SPRITE_NODE = {s: n for n, (_, _, ss) in NODES.items() for s in ss}


def world(pose):
    """pose: {nó: (ângulo_graus, dx, dy)} → matriz afim 3x3 (canvas de repouso → tela) por nó"""
    T = {}
    def get(n):
        if n in T: return T[n]
        par, p, _ = NODES[n]
        ang, dx, dy = pose.get(n, (0, 0, 0))
        t = np.radians(ang); c, s = np.cos(t), np.sin(t)
        R = np.array([[c, s, 0], [-s, c, 0], [0, 0, 1]])       # + = anti-horário na tela (como a Unity)
        A = np.array([[1, 0, p[0] + dx], [0, 1, p[1] + dy], [0, 0, 1]]) @ R @ np.array([[1, 0, -p[0]], [0, 1, -p[1]], [0, 0, 1]])
        T[n] = (get(par) if par else np.eye(3)) @ A
        return T[n]
    for n in NODES: get(n)
    return T


def render(pose, blink=False, canvas=(W, H), shift=(0, 0)):
    T = world(pose); cw, ch = canvas
    o = np.zeros((ch, cw, 4), np.uint8)
    qy, qx = np.mgrid[0:ch, 0:cw]
    Q = np.stack([qx + 0.5 - shift[0], qy + 0.5 - shift[1], np.ones_like(qx, float)], -1)
    for n in RP.ORDER:
        if n == 'eyelid_closed' and not blink: continue
        L = LAY[n]
        if not (L[..., 3] > 0).any(): continue
        Ti = np.linalg.inv(T[SPRITE_NODE[n]])
        S = Q @ Ti.T
        sx, sy = np.floor(S[..., 0]).astype(int), np.floor(S[..., 1]).astype(int)
        ok = (sx >= 0) & (sx < W) & (sy >= 0) & (sy < H)
        smp = np.zeros_like(o); smp[ok] = L[sy[ok], sx[ok]]
        a = smp[..., 3] > 0; o[a] = smp[a]
    return o


def idle_pose(fr):
    t, h, hd = fr['torso'], fr['head'], fr['hands']
    return {'Torso': (0, 0, t), 'NeckHead': (0, 0, h - t), 'HandFront': (0, 0, hd - t), 'HandBack': (0, 0, hd - t)}


IDLE_TABLE = [dict(torso=0, head=0, hands=0), dict(torso=-1, head=0, hands=0), dict(torso=-1, head=-1, hands=-1),
              dict(torso=-1, head=-1, hands=-1), dict(torso=0, head=-1, hands=-1), dict(torso=0, head=0, hands=0)]


def on(a, bg=(98, 160, 220, 255)):
    im = Image.new('RGBA', (a.shape[1], a.shape[0]), bg); im.alpha_composite(Image.fromarray(a)); return im


if __name__ == '__main__':
    PV = f'{OUT}/previews'
    # 1) repouso pelo rig (com transforms) == master?
    rest = render({})
    diff_rest = (rest != M).any(-1)
    print('repouso via hierarquia == master:', not diff_rest.any(), int(diff_rest.sum()))
    on(rest).resize((W * 4, H * 4), Image.NEAREST).save(f'{PV}/assembled_rest.png')
    # original x reconstruído + mapa de diferença
    c = Image.new('RGBA', (W * 3 * 4 + 40, H * 4 + 24), (20, 20, 30, 255)); d = ImageDraw.Draw(c); d.fontmode = '1'
    dm = np.zeros_like(M); dm[..., :3] = 40; dm[..., 3] = 255; dm[M[..., 3] > 0] = (90, 90, 90, 255); dm[diff_rest] = (255, 0, 0, 255)
    for j, (a, t) in enumerate(((M, 'MASTER ORIGINAL'), (rest, 'RECONSTRUIDO (rig)'), (dm, f'DIFERENCA: {int(diff_rest.sum())} px'))):
        c.alpha_composite(on(a).resize((W * 4, H * 4), Image.NEAREST), (j * (W * 4 + 20), 24)); d.text((j * (W * 4 + 20) + 4, 6), t, font=F8, fill=(255, 236, 120))
    c.save(f'{PV}/original_vs_reconstructed.png')
    # 2) IDLE: 6 frames originais x rig
    orig = [np.array(Image.open(f'{RP.SRC}/frames/croc_idle_{i:02d}.png').convert('RGBA')) for i in range(6)]
    new = [render(idle_pose(f)) for f in IDLE_TABLE]
    blink_o = np.array(Image.open(f'{RP.SRC}/frames/croc_idle_blink.png').convert('RGBA')); blink_n = render({}, blink=True)
    res = [int((a != b).any(-1).sum()) for a, b in zip(orig, new)] + [int((blink_o != blink_n).any(-1).sum())]
    print('idle: pixels diferentes por frame (0..5, piscada):', res)
    frames = []
    for i in range(6):
        t = Image.new('RGBA', (W * 3 * 3 + 40, H * 3 + 20), (20, 20, 30, 255)); dd = ImageDraw.Draw(t); dd.fontmode = '1'
        df = np.zeros_like(M); df[..., 3] = 255; df[..., :3] = 40; df[orig[i][..., 3] > 0] = (90, 90, 90, 255); df[(orig[i] != new[i]).any(-1)] = (255, 0, 0, 255)
        for j, (a, lab) in enumerate(((orig[i], 'IDLE ORIGINAL'), (new[i], 'IDLE PELO RIG'), (df, f'DIF: {res[i]} px'))):
            t.alpha_composite(on(a).resize((W * 3, H * 3), Image.NEAREST), (j * (W * 3 + 20), 20)); dd.text((j * (W * 3 + 20) + 4, 6), f'{lab}  f{i}', font=F8, fill=(255, 236, 120))
        frames.append(t.convert('RGB').quantize(colors=128, dither=Image.Dither.NONE))
    frames[0].save(f'{PV}/idle_comparison.gif', save_all=True, append_images=frames[1:], duration=166, loop=0)
    # copia o idle aprovado
    import shutil
    for i in range(6): shutil.copy(f'{RP.SRC}/frames/croc_idle_{i:02d}.png', f'{OUT}/idle/croc_idle_{i:02d}.png')
    shutil.copy(f'{RP.SRC}/frames/croc_idle_blink.png', f'{OUT}/idle/croc_idle_blink.png')
    shutil.copy(f'{RP.SRC}/croc_idle_sheet.png', f'{OUT}/idle/croc_idle_sheet.png')
    shutil.copy(f'{RP.SRC}/croc_platformer_master.png', f'{OUT}/master/croc_platformer_master.png')
    json.dump(dict(rest_diff=int(diff_rest.sum()), idle_diff=res), open('/tmp/claude-0/rig_check.json', 'w'))
