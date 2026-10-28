"""Reuse painted vine pixels; measure upper joins without changing any approved pose."""
from pathlib import Path
from PIL import Image
import re,json,math,hashlib
ROOT=Path(__file__).resolve().parents[2]; prefab=(ROOT/'Assets/Prefabs/Boss/Macaquini.prefab').read_text(encoding='utf-8')
OUT=ROOT/'Assets/Resources/Art/MacaquiniVineV28'; SOURCE=ROOT/'SourceArt/MacaquiniVine_v28'
OUT.mkdir(parents=True,exist_ok=True); SOURCE.mkdir(parents=True,exist_ok=True)
guid_paths={}
for meta in (ROOT/'Assets').rglob('*.png.meta'):
    match=re.search(r'^guid: (\w+)',meta.read_text(encoding='utf-8'),re.M)
    if match:guid_paths[match[1]]=Path(str(meta)[:-5])
def refs(name,section=prefab):
    block=re.search(r'^  '+name+r':\n((?:  - .*\n)+)',section,re.M)
    return re.findall(r'guid: (\w+)',block.group(1))
def points(name):
    block=re.search(r'^  '+name+r':\n((?:  - .*\n)+)',prefab,re.M)
    return [(float(x),float(y)) for x,y in re.findall(r'x: ([\d.e+-]+), y: ([\d.e+-]+)',block.group(1))]
records=[]
def measure(guid,expected):
    path=guid_paths[guid]; meta=Path(str(path)+'.meta').read_text(encoding='utf-8'); image=Image.open(path).convert('RGBA')
    ppu=float(re.search(r'spritePixelsToUnits: ([\d.]+)',meta).group(1)); pivot=tuple(map(float,re.search(r'spritePivot: \{x: ([\d.]+), y: ([\d.]+)\}',meta).groups()))
    ex=pivot[0]*image.width+expected[0]*ppu;ey=(1-pivot[1])*image.height-expected[1]*ppu
    rows=[]
    for y in range(max(0,round(ey)-7),min(image.height,round(ey)+18)):
        xs=[x for x in range(max(0,round(ex)-13),min(image.width,round(ex)+14)) if image.getpixel((x,y))[3]>127]
        if xs:rows.append((y,(min(xs)+max(xs))/2,max(xs)-min(xs)+1))
    if not rows:raise ValueError(path)
    upper=rows[:3];lower=[r for r in rows if rows[0][0]+7<=r[0]<=rows[0][0]+13]
    if not lower:lower=rows[-3:]
    x=sum(r[1] for r in upper)/len(upper);y=rows[0][0]+1
    lx=sum(r[1] for r in lower)/len(lower);ly=sum(r[0] for r in lower)/len(lower)
    dx=x-lx;dy=max(3,ly-y);length=math.hypot(dx,dy)
    width=sorted(r[2] for r in rows[:12])[len(rows[:12])//2]
    records.append({'name':path.stem,'x':(x-pivot[0]*image.width)/ppu,'y':((1-pivot[1])*image.height-y)/ppu,
                    'dx':dx/length,'dy':dy/length,'width':max(.07,min(.27,width/ppu)),'source':str(path.relative_to(ROOT)).replace('\\','/'),
                    'sha256':hashlib.sha256(path.read_bytes()).hexdigest()})
for group,anchors in [('swinging','swingVines'),('transformation','transformationVines')]:
    for guid,point in zip(refs(group),points(anchors)):measure(guid,point)
kick=prefab.split('m_EditorClassIdentifier: Assembly-CSharp::MacaquiniSwingKick')[1]
poses=refs('poses',kick)
for i,point in [(0,(.68,3.07)),(5,(.16,3.20))]:measure(poses[i],point)
# Straighten a small bark strip from the approved vine, retaining its own shaded pixels.
base=Image.open(guid_paths[refs('swinging')[0]]).convert('RGBA'); tile=Image.new('RGBA',(32,32))
for row in range(32):
    sy=74+(row if row<16 else 31-row)
    xs=[x for x in range(239,268) if base.getpixel((x,sy))[3]>127]
    center=round((min(xs)+max(xs))/2)
    for x in range(32):tile.putpixel((x,row),base.getpixel((center+x-16,sy)))
tile.save(OUT/'vine_bark.png')
(OUT/'joins.json').write_text(json.dumps({'joins':records},indent=2),encoding='utf-8')
(SOURCE/'manifest.json').write_text(json.dumps({'source':'Assets/Art/JungleStage/Macaquini_v11/half_00.png','method':'Original painted pixels; straightened bark tile and per-pose upper tip/tangent measurements. Original poses untouched.','joins':records},indent=2),encoding='utf-8')
tile.resize((128,256),Image.Resampling.NEAREST).save(SOURCE/'bark-preview-x8.png')
print('Measured',len(records),'joins; exported original bark pixels.')
