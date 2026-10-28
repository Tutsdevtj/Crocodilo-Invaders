"""Static modular boss master. Raster drawing only; Pillow + numpy, no image generator.

Run with Python from any directory. Does not import into or modify Unity assets.
"""
from pathlib import Path
import json
import hashlib
import shutil
import zipfile
import numpy as np
from PIL import Image, ImageDraw, ImageFont

PROJECT = next(p for p in Path(__file__).resolve().parents if (p/'Assets').is_dir() and (p/'SourceArt').is_dir())
OUT = PROJECT / 'SourceArt' / 'ChimpaniziMaster_v01'
PREVIEW = PROJECT / 'Docs' / 'ArtPreviews' / 'ChimpaniziMaster_v01'
W, H = 256, 256
ROOT = (132, 154)
P = {
    'outline': (13, 13, 21),
    'fur_deep': (28, 35, 28), 'fur_shadow': (43, 56, 36),
    'fur_base': (64, 79, 43), 'fur_light': (94, 113, 53), 'fur_glint': (131, 145, 67),
    'skin_deep': (77, 42, 31), 'skin_shadow': (115, 60, 38),
    'skin_base': (157, 88, 47), 'skin_light': (195, 123, 64),
    'peel_deep': (119, 66, 29), 'peel_shadow': (177, 111, 30),
    'peel_base': (226, 165, 37), 'peel_light': (250, 207, 67), 'peel_glint': (255, 231, 119),
    'pulp_deep': (185, 147, 76), 'pulp_shadow': (224, 191, 111),
    'pulp_base': (247, 222, 153), 'pulp_light': (255, 242, 193),
    'stalk_deep': (57, 62, 30), 'stalk_base': (97, 104, 42), 'stalk_light': (154, 152, 60),
    'eye': (247, 244, 206), 'iris': (206, 103, 32), 'mouth': (61, 33, 30),
    'tooth': (250, 231, 175), 'nail': (98, 76, 48),
}
FUR = ['fur_deep', 'fur_shadow', 'fur_base', 'fur_light', 'fur_glint']
SKIN = ['skin_deep', 'skin_shadow', 'skin_base', 'skin_light']
PEEL = ['peel_deep', 'peel_shadow', 'peel_base', 'peel_light', 'peel_glint']
PULP = ['pulp_deep', 'pulp_shadow', 'pulp_base', 'pulp_light']
PARTS = []


def poly(points):
    m = Image.new('L', (W, H)); ImageDraw.Draw(m).polygon(points, fill=255)
    return np.array(m) > 0


def ellipse(box):
    m = Image.new('L', (W, H)); ImageDraw.Draw(m).ellipse(box, fill=255)
    return np.array(m) > 0


def bez(points, n=40):
    a = np.array(points, float); result = []
    for t in np.linspace(0, 1, n):
        q = a.copy()
        while len(q) > 1:
            q = q[:-1] * (1 - t) + q[1:] * t
        result.append(tuple(np.rint(q[0]).astype(int)))
    return result


def shape(*segments):
    return poly([p for seg in segments for p in bez(seg)])


def shifted(m, dx, dy):
    o = np.zeros_like(m)
    x0, x1 = max(0, dx), min(W, W + dx)
    y0, y1 = max(0, dy), min(H, H + dy)
    o[y0:y1, x0:x1] = m[y0-dy:y1-dy, x0-dx:x1-dx]
    return o


def dilation(m):
    return m | shifted(m, -1, 0) | shifted(m, 1, 0) | shifted(m, 0, -1) | shifted(m, 0, 1)


class Piece:
    def __init__(self, name, pivot, parent='body', socket=None):
        self.name, self.pivot, self.parent, self.socket = name, pivot, parent, socket
        self.a = np.zeros((H, W, 4), np.uint8)

    def paint(self, mask, color):
        self.a[mask] = (*P[color], 255)

    def shade(self, mask, ramp, base=2):
        self.paint(mask, ramp[base])
        shadow = mask & ~shifted(mask, -4, -3)
        deep = mask & ~shifted(mask, -2, -1)
        light = mask & ~shifted(mask, 3, 2) & ~shadow
        self.paint(shadow, ramp[max(0, base-1)])
        self.paint(deep, ramp[0])
        self.paint(light, ramp[min(base+1, len(ramp)-1)])
        edge = dilation(mask) & ~mask
        self.paint(edge, 'outline')
        self.paint(shifted(edge, 1, 1) & ~mask, 'outline')
        return mask

    def line(self, points, color, width=1, clip=True):
        m = Image.new('L', (W, H)); ImageDraw.Draw(m).line(points, fill=255, width=width)
        mask = np.array(m) > 0
        if clip: mask &= self.a[..., 3] > 0
        self.paint(mask, color)

    def fur(self, centers):
        for x, y in centers:
            self.line([(x-3, y+1), (x, y-1), (x+4, y)], 'fur_light')
            self.line([(x-1, y+3), (x+2, y+2)], 'fur_shadow')

    def finish(self):
        PARTS.append(self)
        return self


def limb(name, pivot, parent, points, tufts):
    p = Piece(name, pivot, parent)
    m = shape(*points)
    # Deliberate fur breaks in the silhouette, not scattered surface noise.
    fur_edges = {
        'arm_back_upper': [[(159,87),(161,81),(168,84)],[(172,76),(177,69),(178,78)]],
        'arm_back_forearm': [[(178,60),(175,53),(183,54)],[(195,40),(196,34),(202,39)]],
        'arm_front_upper': [[(92,112),(95,105),(101,110)],[(73,128),(67,130),(73,135)]],
        'arm_front_forearm': [[(59,126),(62,120),(67,125)],[(62,145),(67,150),(70,142)]],
        'leg_front_thigh': [[(104,170),(99,172),(103,179)]],
        'leg_front_shin': [[(95,208),(90,211),(94,218)]],
        'leg_back_thigh': [[(159,170),(166,172),(163,179)]],
        'leg_back_shin': [[(176,199),(184,202),(179,207)]],
    }
    for tuft in fur_edges.get(name, []): m |= poly(tuft)
    p.shade(m, FUR)
    p.fur(tufts)
    return p.finish()


def hand(name, pivot, parent, raised=False):
    p = Piece(name, pivot, parent)
    x, y = pivot
    if raised:
        # Curled fingers with a large thumb; gap is the future vine grip socket.
        m = poly([(x-10,y+7),(x-11,y-6),(x-7,y-14),(x+2,y-17),
                  (x+10,y-15),(x+13,y-8),(x+11,y-3),(x+4,y-2),
                  (x+1,y-7),(x-2,y-6),(x-2,y-1),(x+9,y+2),
                  (x+8,y+8),(x+2,y+11),(x-5,y+11)])
        p.shade(m, SKIN)
        for i in range(3): p.line([(x+3+i*3,y-13),(x+3+i*3,y-8)],'skin_deep')
        p.line([(x-5,y+3),(x+3,y+5),(x+6,y+3)], 'skin_shadow', 2)
    else:
        m = poly([(x+10,y-9),(x-1,y-13),(x-10,y-10),(x-15,y-4),
                  (x-15,y+3),(x-10,y+10),(x+2,y+12),(x+10,y+6),
                  (x+11,y-2),(x+6,y-5)])
        p.shade(m, SKIN)
        for i in range(3):
            p.line([(x-11+i*4,y+1),(x-9+i*4,y+7)], 'skin_deep')
            p.line([(x-11+i*4,y-5),(x-9+i*4,y-6)], 'skin_light', 2)
        p.line([(x+2,y-8),(x-1,y-2),(x+2,y+2),(x+6,y)], 'skin_deep', 2)
    return p.finish()


def foot(name, pivot, parent, back=False):
    p = Piece(name, pivot, parent); x,y=pivot
    m = shape([(x+6,y-6),(x+14,y+5),(x+12,y+12),(x-2,y+12)],
              [(x-2,y+12),(x-20,y+15),(x-25,y+10),(x-19,y+5)],
              [(x-19,y+5),(x-9,y+1),(x-8,y-8),(x+6,y-6)])
    # Opposable big toe: an ape's grasping foot rather than a boot or paw.
    m |= poly([(x+3,y+3),(x+15,y+4),(x+18,y+9),(x+16,y+14),
               (x+11,y+15),(x+6,y+10)])
    p.shade(m, SKIN)
    for i in range(3):
        p.line([(x-17+i*5,y+7),(x-17+i*5,y+12)], 'skin_deep')
        p.line([(x-20+i*5,y+6),(x-17+i*5,y+6)], 'nail')
    p.line([(x-3,y+4),(x+3,y+6),(x+6,y+5)], 'skin_shadow', 2)
    p.line([(x+8,y+7),(x+9,y+11),(x+13,y+13)],'skin_deep')
    p.line([(x+12,y+6),(x+15,y+8)],'nail',2)
    return p.finish()


def build():
    # Far-side limbs: intentional underpainting at every articulation.
    limb('arm_back_upper',(151,92),'body',[
        [(143,93),(153,76),(173,74),(183,62)],
        [(183,62),(192,63),(195,73),(186,82)],
        [(186,82),(171,100),(160,108),(145,103)]],[(162,88),(174,78)])
    limb('arm_back_forearm',(185,73),'arm_back_upper',[
        [(175,79),(176,60),(187,43),(196,34)],
        [(196,34),(203,33),(211,40),(208,49)],
        [(208,49),(202,68),(195,82),(181,86)]],[(188,53),(195,45),(185,66)])
    hand('hand_back_grip',(204,29),'arm_back_forearm',True)
    limb('leg_back_thigh',(147,162),'body',[
        [(139,159),(153,157),(166,172),(173,183)],
        [(173,183),(172,193),(158,197),(150,186)],
        [(150,186),(140,180),(135,170),(139,159)]],[(150,172),(160,183)])
    limb('leg_back_shin',(163,188),'leg_back_thigh',[
        [(153,187),(168,181),(180,196),(184,211)],
        [(184,211),(178,219),(166,217),(161,205)],
        [(161,205),(157,201),(153,195),(153,187)]],[(165,200)])
    foot('foot_back',(176,216),'leg_back_shin',True)

    # Main banana silhouette: bend and tapered brown end remain visible.
    p = Piece('body',ROOT,None)
    m = shape([(114,52),(131,37),(155,57),(156,88)],
              [(156,88),(154,124),(163,152),(182,171)],
              [(182,171),(191,184),(183,199),(168,196)],
              [(168,196),(130,193),(111,171),(105,145)],
              [(105,145),(96,111),(96,75),(114,52)])
    p.shade(m,PEEL)
    p.paint(shape([(128,57),(145,62),(146,91),(144,117)],
                  [(144,117),(146,149),(157,169),(173,183)],
                  [(173,183),(144,178),(123,154),(118,122)],
                  [(118,122),(110,82),(115,65),(128,57)]) & m,'peel_light')
    p.line(bez([(137,66),(133,118),(143,166),(172,187)]),'peel_glint',3)
    p.line(bez([(107,100),(105,156),(131,180),(157,188)]),'peel_shadow',2)
    p.shade(poly([(176,183),(188,185),(190,190),(183,200),(174,197)]),
            ['skin_deep','peel_deep','skin_shadow','skin_base'])
    p.line([(116,151),(119,154),(118,156)],'peel_deep',2)
    p.line([(147,174),(150,175)],'peel_deep',2)
    p.finish()

    p=Piece('stalk',(126,56),'body')
    p.shade(poly([(117,57),(117,43),(121,31),(130,27),(138,31),
                  (136,39),(132,39),(130,49),(131,58)]),
            ['stalk_deep','stalk_base','stalk_light','peel_glint'],1)
    p.line([(123,42),(126,33),(131,32)],'stalk_light',2);p.finish()

    # Exposed fruit is the face substrate, not a separate chimp head.
    p=Piece('pulp_face',(120,92),'body')
    m=shape([(115,53),(126,47),(143,56),(146,76)],
            [(146,76),(149,101),(140,120),(126,130)],
            [(126,130),(110,124),(101,109),(103,89)],
            [(103,89),(103,68),(108,59),(115,53)])
    p.shade(m,PULP)
    p.line([(112,66),(116,60),(123,57)],'pulp_light',2)
    p.line([(137,68),(140,79),(139,85)],'pulp_shadow',2)
    p.finish()

    # Peel flaps: separately anchored at their roots, not ribbons floating free.
    p=Piece('peel_back_flap',(141,111),'body')
    p.shade(shape([(139,108),(157,106),(171,114),(180,132)],
                  [(180,132),(187,150),(183,164),(177,169)],
                  [(177,169),(180,143),(166,129),(142,122)]),PEEL)
    p.line(bez([(149,114),(174,127),(182,148),(179,160)]),'peel_glint',2);p.finish()

    # Front legs with distinct bent knees and long, grasping ape feet.
    limb('leg_front_thigh',(122,158),'body',[
        [(117,151),(131,158),(132,173),(122,187)],
        [(122,187),(115,197),(102,195),(98,187)],
        [(98,187),(98,177),(109,162),(117,151)]],[(115,170),(107,183)])
    limb('leg_front_shin',(109,189),'leg_front_thigh',[
        [(100,182),(114,185),(113,204),(108,222)],
        [(108,222),(98,227),(89,221),(91,211)],
        [(91,211),(95,197),(94,191),(100,182)]],[(101,200),(97,215)])
    foot('foot_front',(98,225),'leg_front_shin')

    limb('arm_front_upper',(110,111),'body',[
        [(115,105),(119,116),(103,136),(88,140)],
        [(88,140),(76,144),(66,135),(71,124)],
        [(71,124),(81,117),(94,106),(106,103)]],[(95,118),(83,129)])
    limb('arm_front_forearm',(77,131),'arm_front_upper',[
        [(82,120),(86,135),(71,148),(51,149)],
        [(51,149),(42,145),(41,133),(49,126)],
        [(49,126),(62,123),(72,118),(82,120)]],[(61,131),(72,128),(53,141)])
    hand('hand_front_fist',(36,137),'arm_front_forearm')

    p=Piece('peel_front_flap',(111,116),'body')
    p.shade(shape([(106,110),(116,113),(125,127),(126,148)],
                  [(126,148),(125,164),(116,178),(107,181)],
                  [(107,181),(115,166),(115,149),(108,137)],
                  [(108,137),(98,126),(98,115),(106,110)]),PEEL)
    p.line(bez([(106,119),(121,138),(122,160),(111,174)]),'peel_glint',2);p.finish()

    # Expression layers allow later local editing while the pulp is unchanged.
    p=Piece('eyes_brows',(121,88),'pulp_face')
    p.paint(poly([(105,84),(117,86),(120,93),(111,96),(105,92)]),'outline')
    p.paint(poly([(107,87),(116,89),(117,92),(110,93),(107,91)]),'eye')
    p.paint(poly([(125,88),(139,82),(141,88),(136,95),(126,96)]),'outline')
    p.paint(poly([(128,90),(138,86),(138,90),(134,93),(128,94)]),'eye')
    p.paint(poly([(108,88),(111,89),(111,93),(108,92)]),'iris')
    p.paint(poly([(129,89),(132,88),(132,94),(129,94)]),'iris')
    p.paint(poly([(108,89),(109,89),(109,92),(108,92)]),'outline')
    p.paint(poly([(129,90),(130,90),(130,93),(129,93)]),'outline')
    p.line([(104,80),(110,81),(119,86)],'peel_deep',3,False)
    p.line([(125,86),(134,79),(141,79)],'peel_deep',3,False)
    p.line([(119,81),(121,83)],'pulp_deep',1,False);p.finish()

    p=Piece('mouth',(119,108),'pulp_face')
    p.paint(poly([(108,105),(116,102),(129,103),(137,108),(133,115),
                  (119,116),(109,113)]),'outline')
    p.paint(poly([(111,107),(118,105),(128,106),(133,109),(129,113),
                  (118,113),(112,111)]),'mouth')
    p.paint(poly([(112,106),(118,105),(128,106),(132,108),(130,110),
                  (118,109),(113,110)]),'tooth')
    p.line([(119,105),(119,108)],'pulp_deep',1,False)
    p.line([(126,106),(126,109)],'pulp_deep',1,False)
    p.line([(113,100),(117,98),(122,99)],'pulp_deep',1,False)
    p.line([(115,121),(124,123),(129,121)],'pulp_deep',1,False);p.finish()


def composite(parts):
    im=Image.new('RGBA',(W,H))
    for p in parts: im.alpha_composite(Image.fromarray(p.a))
    return im


def font(size=14):
    for f in ['C:/Windows/Fonts/consola.ttf','C:/Windows/Fonts/arial.ttf']:
        if Path(f).exists(): return ImageFont.truetype(f,size)
    return ImageFont.load_default()


def card(im,scale=3,bg=(35,42,45),pad=18):
    o=Image.new('RGB',(im.width*scale+pad*2,im.height*scale+pad*2),bg)
    enlarged=im.resize((im.width*scale,im.height*scale),Image.Resampling.NEAREST)
    o.paste(enlarged,(pad,pad),enlarged)
    return o


def export():
    for folder in [OUT/'parts',OUT/'master',OUT/'previews',OUT/'tools',PREVIEW]:
        folder.mkdir(parents=True,exist_ok=True)
    assembled=composite(PARTS)
    assembled.save(OUT/'master/chimpanizi_p1_master.png')
    entries=[]
    for order,p in enumerate(PARTS):
        full=Image.fromarray(p.a); bbox=full.getbbox(); x0,y0,x1,y1=bbox
        crop=full.crop(bbox); filename=f'chimpanizi_p1_{p.name}.png'
        crop.save(OUT/'parts'/filename)
        px,py=p.pivot; cw,ch=crop.size
        parent=next((q for q in PARTS if q.name==p.parent),None)
        entries.append({'id':p.name,'file':'parts/'+filename,'parent':p.parent,
            'sorting_order':order,'offset_in_canvas_px':[x0,y0],
            'offset_in_boss_px':[x0-ROOT[0],y0-ROOT[1]],
            'size_px':[cw,ch],'pivot_canvas_px':[px,py],
            'pivot_local_px':[px-x0,py-y0],
            'pivot_unity_normalized':[(px-x0)/cw,1-(py-y0)/ch],
            'joint_from_parent_px':[px-parent.pivot[0],py-parent.pivot[1]] if parent else [0,0],
            'sha256':hashlib.sha256((OUT/'parts'/filename).read_bytes()).hexdigest()})
    sockets={'grip_back':[207,23],'grip_front':[30,136],
             'shoulder_back':[151,92],'elbow_back':[185,73],'wrist_back':[204,29],
             'shoulder_front':[110,111],'elbow_front':[77,131],'wrist_front':[36,137],
             'hip_front':[122,158],'knee_front':[109,189],'ankle_front':[98,225],
             'hip_back':[147,162],'knee_back':[163,188],'ankle_back':[176,216],
             'peel_front_root':[111,116],'peel_back_root':[141,111]}
    manifest={'character':'Chimpanizi Bananini','stage':'static phase-one master',
        'version':'v01','status':'proposal_pending_user_review','canvas':{'w':W,'h':H},
        'pivot_px':list(ROOT),'pivot_unity_normalized':[ROOT[0]/W,1-ROOT[1]/H],
        'coordinates':'Top-left origin, y down. Unity y = -pixel y; integer pixel offsets.',
        'facing':'left / three-quarter','light':'top-left',
        'palette':{k:list(v) for k,v in P.items()},'parts':entries,'sockets_canvas_px':sockets,
        'animation':None,'gameplay':None,
        'suggested_import':{'filter':'Point','compression':'None','mipmaps':False,
            'mesh_type':'FullRect','ppu':100,'ppu_status':'provisional; compare in gameplay before integration'},
        'note':'Native modular source; assembled frames may be exported for sprite-swap animation. No rig or Unity integration supplied.'}
    (OUT/'chimpanizi_p1_master.json').write_text(json.dumps(manifest,indent=2,ensure_ascii=False),encoding='utf-8')
    # Readable presentation: actual sprite plus nearest-neighbour enlargement.
    board=Image.new('RGB',(1060,870),(22,29,33));d=ImageDraw.Draw(board)
    d.text((28,20),'CHIMPANIZI BANANINI / MASTER MODULAR v01',font=font(25),fill=(251,222,153))
    d.text((28,58),'FORMA 1 - BANANA + MEMBROS DE CHIMPANZE / PROPOSTA ESTATICA',font=font(15),fill=(154,178,164))
    large=assembled.resize((768,768),Image.Resampling.NEAREST)
    board.paste(large,(270,85),large)
    board.paste(assembled,(10,230),assembled)
    d.text((28,510),'1x / 256 x 256',font=font(17),fill=(251,222,153))
    d.text((28,550),f'{len(PARTS)} pecas separadas',font=font(15),fill=(180,196,183))
    d.text((28,580),'Alpha 0 / 255',font=font(15),fill=(180,196,183))
    d.text((28,608),'Paleta fechada',font=font(15),fill=(180,196,183))
    d.text((28,636),'Luz: cima / esquerda',font=font(15),fill=(180,196,183))
    d.text((28,686),'Sem animacao',font=font(15),fill=(180,196,183))
    d.text((28,714),'Sem integracao Unity',font=font(15),fill=(180,196,183))
    board.save(OUT/'previews/master_review.png')
    card(assembled,4).save(OUT/'previews/master_x4.png')
    # Contact sheet exposes every part; labels are preview-only.
    columns=5;cellw=260;cellh=400;rows=(len(PARTS)+columns-1)//columns
    sheet=Image.new('RGB',(columns*cellw,rows*cellh+50),(28,35,39));sd=ImageDraw.Draw(sheet)
    sd.text((16,12),'PECAS NATIVAS / AMPLIACAO 2x / ORDEM DE DESENHO',font=font(18),fill=(247,222,153))
    for i,e in enumerate(entries):
        x=(i%columns)*cellw;y=(i//columns)*cellh+50
        crop=Image.open(OUT/e['file']).convert('RGBA');s=2
        tile=crop.resize((crop.width*s,crop.height*s),Image.Resampling.NEAREST)
        tx=x+(cellw-tile.width)//2;ty=y+35+(320-tile.height)//2
        sheet.paste(tile,(tx,ty),tile)
        sd.text((x+10,y+8),f'{i:02} {e["id"]}',font=font(14),fill=(228,212,164))
        sd.text((x+10,y+373),f'{crop.width}x{crop.height}  pivot {e["pivot_local_px"]}',font=font(12),fill=(145,166,156))
    sheet.save(OUT/'previews/parts_contact_sheet.png')
    # Joint layout, not a claim that an articulated rig was implemented.
    rig=card(assembled,3);rd=ImageDraw.Draw(rig)
    for name,(x,y) in sockets.items():
        sx,sy=18+x*3,18+y*3
        rd.ellipse((sx-4,sy-4,sx+4,sy+4),fill=(245,89,89),outline=(255,233,180))
    rig.save(OUT/'previews/joint_points.png')
    # Silhouette and bright backdrop QA.
    sil=assembled.copy();sa=np.array(sil);sa[sa[...,3]>0,:3]=(13,13,21);sil=Image.fromarray(sa)
    card(sil,3,(216,224,211)).save(OUT/'previews/silhouette_x3.png')
    # Compare the native game sprites at identical image-pixel scale.
    croc=Image.open(PROJECT/'Assets/Art/Player/Bombardiro/Idle/bombardiro_idle_0.png').convert('RGBA')
    goose=Image.open(PROJECT/'Assets/Art/Boss/GusiniPhase1/gusini_p1_idle/gusini_p1_idle_0.png').convert('RGBA')
    compare=Image.new('RGBA',(760,300),(41,54,53,255))
    compare.alpha_composite(croc,(10,108));compare.alpha_composite(goose,(204,60));compare.alpha_composite(assembled,(490,12))
    cd=ImageDraw.Draw(compare)
    for x,label in [(15,'CROC'),(215,'GUSINI'),(510,'CHIMPANIZI')]:cd.text((x,276),label,font=font(15),fill=(251,222,153))
    compare.convert('RGB').save(OUT/'previews/native_scale_comparison.png')
    # Context preview only. Preserve the original jungle concept unchanged.
    jungle=PROJECT/'Docs/ArtPreviews/ClosedJungle/closed-jungle-concept-v01.png'
    if jungle.exists():
        bg=Image.open(jungle).convert('RGBA');scale=3
        sprite=assembled.resize((W*scale,H*scale),Image.Resampling.NEAREST)
        origin=(bg.width-855,95)
        bg.alpha_composite(sprite,origin)
        plane=croc.resize((croc.width*2,croc.height*2),Image.Resampling.NEAREST)
        bg.alpha_composite(plane,(190,390))
        bg.convert('RGB').save(OUT/'previews/forest_context_mockup.png')
    # Recompose from the cropped exports + metadata and check exact equality.
    reconstructed=Image.new('RGBA',(W,H))
    for e in entries:
        reconstructed.alpha_composite(Image.open(OUT/e['file']).convert('RGBA'),tuple(e['offset_in_canvas_px']))
    original=np.array(assembled);rec=np.array(reconstructed)
    palette={tuple(v) for v in P.values()};checks={}
    checks['recomposition_pixel_difference']=int(np.any(original!=rec,axis=2).sum())
    checks['part_count']=len(entries)
    checks['alpha_values']=sorted({int(v) for p in PARTS for v in np.unique(p.a[...,3])})
    checks['colors_outside_palette']=len({tuple(c) for p in PARTS for c in np.unique(p.a[p.a[...,3]>0,:3],axis=0)}-palette)
    checks['all_pivots_inside_export']=all(0<=e['pivot_local_px'][0]<=e['size_px'][0] and 0<=e['pivot_local_px'][1]<=e['size_px'][1] for e in entries)
    checks['opaque_bounds_px']=list(assembled.getbbox())
    checks['palette_used']=len(np.unique(original[original[...,3]>0,:3],axis=0))
    checks['animation_created']=False;checks['unity_modified']=False
    (OUT/'qa_report.json').write_text(json.dumps(checks,indent=2),encoding='utf-8')
    assert checks['recomposition_pixel_difference']==0
    assert checks['alpha_values']==[0,255]
    assert checks['colors_outside_palette']==0
    assert checks['all_pivots_inside_export']
    shutil.copy2(__file__,OUT/'tools/master.py')
    for f in (OUT/'previews').glob('*.png'):shutil.copy2(f,PREVIEW/f.name)
    print(json.dumps(checks))


if __name__=='__main__':
    build();export()
