# Exporta Croc_Rig_Ready: módulos (PNG cortados), manifesto, previews finais e validação.
import os, json, shutil, numpy as np
from PIL import Image, ImageDraw
import rigprep as RP
import rigtest as RT
import bendtest as BT

OUT = RP.OUT; PV = f'{OUT}/previews'
LAY, J = RP.LAY, RP.JOINT
NODE_JOINT = {n: p for n, (_, p, _) in RT.NODES.items()}
manifest_sprites = {}
for n in RP.ORDER:
    L = LAY[n].copy(); L[..., 3] = np.where(L[..., 3] > 0, 255, 0)
    ys, xs = np.where(L[..., 3] > 0)
    x0, y0, x1, y1 = int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1
    crop = L[y0:y1, x0:x1]; Image.fromarray(crop).save(f'{OUT}/modules/{n}.png')
    node = RT.SPRITE_NODE[n]; px, py = NODE_JOINT[node]; w, h = x1 - x0, y1 - y0
    vis = int(RP.own[n].sum()) if n in RP.own else 0
    manifest_sprites[n] = dict(file=f'modules/{n}.png', w=w, h=h, x=x0, y=y0, node=node, sorting_order=RP.ORDER.index(n),
                               pivot_canvas=[px, py], pivot_local_px=[px - x0, py - y0],
                               unity_pivot=[round((px - x0) / w, 6), round((y1 - py) / h, 6)],
                               alt_bottom_left_pivot=dict(unity_pivot=[0, 0], local_position_px=[x0 - px, py - y1]),
                               visible_pixels_at_rest=vis, hidden_pixels=int((crop[..., 3] > 0).sum()) - vis,
                               role=('anel de contorno da articulação (só aparece no lado de fora da dobra)' if n.startswith('ring_') else
                                     'pelve/assento escondido atrás das coxas' if n == 'pelvis_under' else
                                     'contorno da lateral direita do tronco atrás do braço de trás' if n == 'torso_side_back' else
                                     'overlay da piscada (só no frame de piscada)' if n == 'eyelid_closed' else 'peça do personagem'))
nodes = {}
for n, (par, p, sprites) in RT.NODES.items():
    pp = NODE_JOINT[par] if par else (48, 180)
    dx, dy = p[0] - pp[0], p[1] - pp[1]
    nodes[n] = dict(parent=par or 'CharacterRoot', joint_canvas=list(p), local_px=[dx, dy], local_unity_px=[dx, -dy],
                    local_units_ppu100=[round(dx / 100, 4), round(-dy / 100, 4)], sprites=sprites)
idle = []
for i, fr in enumerate(RT.IDLE_TABLE):
    pose = RT.idle_pose(fr)
    idle.append(dict(frame=i, ms=166, local_offset_px_ydown={k: v[2] for k, v in pose.items() if v[2]},
                     local_offset_unity_y={k: -v[2] for k, v in pose.items() if v[2]}))
check = json.load(open('/tmp/claude-0/rig_check.json'))
manifest = dict(
    character='B. Crocodilo (a pé) — rig 2D', canvas=[RP.W, RP.H], character_pivot=[48, 180],
    coords='px do canvas 96x184, origem no canto SUPERIOR esquerdo, y para baixo (local_unity_px já inverte o y). Articulação (x,y) = canto superior esquerdo do pixel (x,y), mesma convenção do croc_idle.json (pivot geral 48,180).',
    ground_y=180,
    import_settings=dict(filter='Point (no filter)', compression='None', ppu='usar o mesmo PPU do idle; local_units_ppu100 é só referência',
                         sprite_mode='Single', pivot='Custom = unity_pivot de cada sprite'),
    hierarchy_note='cada nó é um Transform vazio posicionado na articulação; os SpriteRenderers são filhos com localPosition (0,0) e pivô = articulação',
    nodes=nodes, sprites=manifest_sprites, render_order=RP.ORDER,
    joints_canvas={k: list(v) for k, v in J.items()}, joint_disc_radius=RP.DISC,
    rotation_conventions=dict(positive='anti-horário na tela (padrão da Unity, eixo Z)',
                              suggested_limits_deg=dict(ArmUpper=[-60, 60], ArmLower=[0, 110], Hand=[-20, 20],
                                                        LegUpper=[-45, 45], LegLower=[-110, 0], Boot=[-30, 25], Torso=[-8, 8], NeckHead=[-6, 6])),
    old_to_new={'arm_front': ['arm_front_upper', 'arm_front_lower'], 'arm_back': ['arm_back_upper', 'arm_back_lower'],
                'leg_front': ['leg_front_upper', 'leg_front_lower'], 'leg_back': ['leg_back_upper', 'leg_back_lower'],
                'hand_front': ['hand_front'], 'hand_back': ['hand_back'], 'boot_front': ['boot_front'], 'boot_back': ['boot_back'],
                'hips': ['hips', 'pelvis_under (novo)'], 'torso': ['torso', 'torso_side_back (novo)'], 'head': ['head'], 'helmet': ['helmet'],
                'eyelid_closed': ['eyelid_closed'], 'novos_aneis': ['ring_elbow_front', 'ring_elbow_back', 'ring_knee_front', 'ring_knee_back']},
    reassigned_visible_pixels=[dict(de=a, para=b, px=c) for a, b, c in RP.REASSIGNED],
    idle=dict(fps=6, frames=idle, blink='ativar eyelid_closed por 1 frame (~120 ms) a cada 3–5 loops'),
    validation=dict(rest_vs_master_diff_px=check['rest_diff'], idle_diff_px_per_frame=check['idle_diff'][:6], blink_diff_px=check['idle_diff'][6]),
    careful_joints=['Rotação na Unity com filtro Point pode "pontilhar" o contorno em ângulos que não são múltiplos de 90°; prefira ângulos inteiros e Pixel Perfect Camera.',
                    'Os anéis (ring_*) precisam ficar ABAIXO da peça de baixo da junta (ver sorting_order); se ficarem acima, aparecem no repouso.',
                    'pelvis_under precisa ficar abaixo de TODAS as peças das pernas.',
                    'torso_side_back fica abaixo do braço de trás inteiro, mas é filho do Torso.',
                    'O braço de trás no master só mostra a metade de fora; a metade de dentro foi desenhada escondida sob o tronco (4 px) para o braço não ficar fino ao balançar.'],
)
json.dump(manifest, open(f'{OUT}/rig_manifest.json', 'w'), indent=1, ensure_ascii=False)

# vista explodida
expl = {'NeckHead': (0, 0, -10), 'Torso': (0, 0, -8), 'ArmFrontUpper': (0, -10, 0), 'ArmFrontLower': (0, -3, 6), 'HandFront': (0, 0, 6),
        'ArmBackUpper': (0, 10, 0), 'ArmBackLower': (0, 3, 6), 'HandBack': (0, 0, 6), 'LegFrontUpper': (0, -6, 6), 'LegFrontLower': (0, -2, 8),
        'BootFront': (0, 0, 7), 'LegBackUpper': (0, 6, 6), 'LegBackLower': (0, 2, 8), 'BootBack': (0, 0, 7)}
ex = RT.render(expl, canvas=(150, 240), shift=(27, 14))
RT.on(ex).resize((150 * 3, 240 * 3), Image.NEAREST).save(f'{PV}/module_exploded_view.png')
# folha de módulos (cada PNG separado, ampliado)
tiles = [(n, np.array(Image.open(f'{OUT}/modules/{n}.png'))) for n in RP.ORDER]
cols = 6; cw = max(t.shape[1] for _, t in tiles) * 2 + 10; ch = max(t.shape[0] for _, t in tiles) * 2 + 22
sheet = Image.new('RGBA', (cw * cols, ch * ((len(tiles) + cols - 1) // cols)), (40, 40, 52, 255)); d = ImageDraw.Draw(sheet); d.fontmode = '1'
for i, (n, t) in enumerate(tiles):
    x, y = (i % cols) * cw, (i // cols) * ch
    bg = Image.new('RGBA', (t.shape[1], t.shape[0]), (98, 160, 220, 255)); bg.alpha_composite(Image.fromarray(t))
    sheet.alpha_composite(bg.resize((t.shape[1] * 2, t.shape[0] * 2), Image.NEAREST), (x + 5, y + 18)); d.text((x + 4, y + 4), n, font=RT.F8, fill=(255, 236, 120))
sheet.save(f'{PV}/modules_sheet_x2.png')
# checagem de arquivos
missing = [s['file'] for s in manifest_sprites.values() if not os.path.exists(f"{OUT}/{s['file']}")]
alpha_bad = [n for n in RP.ORDER if not set(np.unique(np.array(Image.open(f'{OUT}/modules/{n}.png'))[..., 3])).issubset({0, 255})]
print('arquivos faltando:', missing, 'alpha ruim:', alpha_bad, 'sprites:', len(manifest_sprites))
for f in ('rigprep.py', 'rigtest.py', 'bendtest.py', 'export_rig.py'):
    os.makedirs(f'{OUT}/tools', exist_ok=True); shutil.copy(f'/home/claude/proj/crocrig/{f}', f'{OUT}/tools/{f}')
