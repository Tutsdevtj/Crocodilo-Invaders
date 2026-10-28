"""Native pixel-art export: clean binary alpha, shared palettes, safe margins and common anchors."""
from pathlib import Path
from PIL import Image,ImageDraw
from collections import deque
import numpy as np,json,shutil,hashlib
root=Path(__file__).resolve().parents[2]
out=root/'Assets/Art/JungleStage/Macaquini_v22';out.mkdir(parents=True,exist_ok=True)
source=root/'SourceArt/MacaquiniFighter_v22';source.mkdir(parents=True,exist_ok=True)
helper=Path('C:/Users/berna/.codex/generated_images/01a164ae-b7b0-7f90-8b04-583b1c9a507b/exec-241019ae-1e00-46b6-9c58-2d7130f0c2bd.png')
boss=root/'Docs/Concepts/MacaquiniFighter_v01/poses-conceituais.png'
shutil.copy2(helper,source/'helpers-clean-source.png');shutil.copy2(boss,source/'fighter-source.png')
manifest=[];proof=[]
def clean(cel):
    a=np.array(cel);mask=a[:,:,3]>=180;seen=np.zeros(mask.shape,dtype=bool);best=[]
    for yy,xx in zip(*np.where(mask)):
        if seen[yy,xx]:continue
        q=deque([(int(xx),int(yy))]);seen[yy,xx]=True;component=[]
        while q:
            x,y=q.popleft();component.append((x,y))
            for dx,dy in ((1,0),(-1,0),(0,1),(0,-1),(1,1),(1,-1),(-1,1),(-1,-1)):
                nx,ny=x+dx,y+dy
                if 0<=nx<mask.shape[1] and 0<=ny<mask.shape[0] and mask[ny,nx] and not seen[ny,nx]:seen[ny,nx]=True;q.append((nx,ny))
        if len(component)>len(best):best=component
    keep=np.zeros(mask.shape,dtype=bool)
    for x,y in best:keep[y,x]=True
    a[~keep]=0;a[:,:,3]=keep.astype('uint8')*255
    return Image.fromarray(a)
def export(sheet,rects,hips,scale,canvas_size,anchor,pivot,prefix,colors):
    cells=[]
    for rect,hip in zip(rects,hips):
        cel=clean(sheet.crop(rect));cel=cel.resize((round(cel.width*scale),round(cel.height*scale)),Image.Resampling.NEAREST)
        frame=Image.new('RGBA',(canvas_size,canvas_size));frame.paste(cel,(round(anchor[0]-(hip[0]-rect[0])*scale),round(anchor[1]-(hip[1]-rect[1])*scale)))
        cells.append(frame)
    atlas=Image.new('RGB',(canvas_size*len(cells),canvas_size))
    for i,cel in enumerate(cells):atlas.paste(cel.convert('RGB'),(canvas_size*i,0))
    palette=atlas.quantize(colors=colors-6,method=Image.Quantize.MEDIANCUT)
    entries=palette.getpalette()
    # Reserve the infection's green eye colors: tiny irises must not quantize to brown fur.
    for i,color in enumerate([(12,80,9),(18,145,14),(20,220,20),(90,255,65),(155,255,110),(5,35,4)]):entries[(colors-6+i)*3:(colors-5+i)*3]=list(color)
    palette.putpalette(entries)
    for i,cel in enumerate(cells):
        alpha=cel.getchannel('A');cel=cel.convert('RGB').quantize(palette=palette,dither=Image.Dither.NONE).convert('RGBA');cel.putalpha(alpha)
        a=np.array(cel);a[a[:,:,3]==0]=0;cel=Image.fromarray(a)
        rgb=a[:,:,:3].astype(float);assert ((rgb[:,:,1]>rgb[:,:,0]*1.4)&(rgb[:,:,1]>rgb[:,:,2]*1.4)&(rgb[:,:,1]>70)).sum()>0,(prefix,i,'green eyes lost')
        bounds=alpha.getbbox();assert bounds and bounds[0]>=4 and bounds[1]>=4 and bounds[2]<=canvas_size-4 and bounds[3]<=canvas_size-4,(prefix,i,bounds)
        path=out/f'{prefix}_{i:02}.png';cel.save(path)
        manifest.append({'name':path.name,'ppu':64,'pivot':[pivot[0]/canvas_size,1-pivot[1]/canvas_size],'alpha_values':np.unique(a[:,:,3]).tolist(),'bounds':bounds,'colors':len(set(map(tuple,a.reshape(-1,4)))),'sha256':hashlib.sha256(path.read_bytes()).hexdigest()})
        tile=Image.new('RGBA',(320,320));d=ImageDraw.Draw(tile)
        for y in range(0,320,16):
            for x in range(0,320,16):d.rectangle((x,y,x+15,y+15),fill=(65,74,77,255) if (x//16+y//16)%2 else (95,104,105,255))
        preview=cel.resize((320,320),Image.Resampling.NEAREST);tile.alpha_composite(preview);proof.append(tile)
export(Image.open(boss).convert('RGBA'),[(0,0,530,512),(530,0,1010,512),(1010,0,1536,512),(0,512,605,1024),(605,512,1050,1024),(1050,512,1536,1024)],[(275,290),(835,325),(1365,325),(300,805),(810,790),(1350,720)],.5,384,(192,237),(203,255),'fighter',96)
# These are the minion source's registered pelvis positions, separate from the full-sized boss.
export(Image.open(helper).convert('RGBA'),[(0,0,512,512),(512,0,1024,512),(1024,0,1536,512),(0,512,555,1024),(555,512,1050,1024),(1050,512,1536,1024)],[(280,355),(760,355),(1300,355),(300,805),(830,815),(1290,800)],.25,160,(80,112),(80,112),'helper',64)
(source/'layout.json').write_text(json.dumps({'ppu':64,'frames':manifest,'alpha_policy':'binary alpha >=180, largest connected body, zero RGB outside','palette_policy':'shared 96 boss /64 helpers; point filtering, nearest export'},indent=2),encoding='utf-8')
preview=Image.new('RGBA',(1920,640))
for i,tile in enumerate(proof):preview.paste(tile,((i%6)*320,(i//6)*320))
dest=root/'Docs/ArtPreviews/MacaquiniFighter_v22';dest.mkdir(parents=True,exist_ok=True);preview.convert('RGB').save(dest/'pixel-art-clean-cutouts.png')
print('12 clean pixel-art sprites; binary alpha; shared palettes; all silhouettes have safe margins')
