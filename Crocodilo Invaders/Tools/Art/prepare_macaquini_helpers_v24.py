"""Register eight full-body flight cels on a fixed grid; preserve source sheets and pixel edges."""
from pathlib import Path
from PIL import Image, ImageDraw
from collections import deque
import json, shutil, sys, hashlib

root = Path(__file__).resolve().parents[2]
source = root/'SourceArt/MacaquiniHelpers_v24'
output = root/'Assets/Resources/MacaquiniHelpersV24'
preview = root/'Docs/ArtPreviews/MacaquiniHelpers_v24'
for folder in (source, output, preview): folder.mkdir(parents=True, exist_ok=True)
if len(sys.argv) == 3:
    for species, path in zip(('bat','moth'), sys.argv[1:]): shutil.copy2(path, source/f'{species}-source.png')

def largest_component(cel):
    pixels=cel.load(); seen=set(); largest=set()
    for y in range(cel.height):
        for x in range(cel.width):
            if (x,y) in seen or not pixels[x,y][3]: continue
            part={(x,y)}; queue=deque([(x,y)]); seen.add((x,y))
            while queue:
                px,py=queue.popleft()
                for dx,dy in ((1,0),(-1,0),(0,1),(0,-1),(1,1),(-1,1),(1,-1),(-1,-1)):
                    nx,ny=px+dx,py+dy
                    if 0<=nx<cel.width and 0<=ny<cel.height and (nx,ny) not in seen and pixels[nx,ny][3]:
                        seen.add((nx,ny)); part.add((nx,ny)); queue.append((nx,ny))
            if len(part)>len(largest): largest=part
    for y in range(cel.height):
        for x in range(cel.width):
            if (x,y) not in largest: pixels[x,y]=(0,0,0,0)
    return cel

def eye_anchor(cel):
    # The green eye is a stable anatomical landmark; wing bounding boxes are not.
    pixels=cel.load(); candidates=[]; seen=set()
    for y in range(round(cel.height*.35),round(cel.height*.85)):
        for x in range(round(cel.width*.4)):
            r,g,b,a=pixels[x,y]
            if a and g>100 and g>r*1.25 and g>b*1.25: candidates.append((x,y))
    allowed=set(candidates); largest=[]
    for point in candidates:
        if point in seen: continue
        part=[point]; queue=deque([point]); seen.add(point)
        while queue:
            x,y=queue.popleft()
            for dx,dy in ((1,0),(-1,0),(0,1),(0,-1),(1,1),(-1,1),(1,-1),(-1,-1)):
                p=(x+dx,y+dy)
                if p in allowed and p not in seen: seen.add(p);part.append(p);queue.append(p)
        if len(part)>len(largest): largest=part
    if not largest: raise ValueError('No eye anchor found')
    return (sum(p[0] for p in largest)/len(largest),sum(p[1] for p in largest)/len(largest))

manifest=[]; all_frames={}; proof=Image.new('RGBA',(128*8,128*2),'#142831')
for row,species in enumerate(('bat','moth')):
    sheet=Image.open(source/f'{species}-source.png').convert('RGBA'); frames=[]
    for i in range(8):
        # The generated moth's returning diagonal was placed before its horizontal upstroke.
        order=[0,1,2,3,4,6,5,7] if species=='moth' else list(range(8))
        original=order[i]; col,sy=original%4,original//4
        rect=(round(sheet.width*col/4),round(sheet.height*sy/2),round(sheet.width*(col+1)/4),round(sheet.height*(sy+1)/2))
        cel=sheet.crop(rect)
        cel.putalpha(cel.getchannel('A').point(lambda a:255 if a>=180 else 0))
        # Uniform density, then registration by the eye, never by the changing wing silhouette.
        cel=cel.resize((96,96),Image.Resampling.NEAREST)
        cel=largest_component(cel)
        eye=eye_anchor(cel); offset=(round(44-eye[0]),round(72-eye[1]))
        frame=Image.new('RGBA',(128,128)); frame.alpha_composite(cel,offset); frames.append(frame)
    # One palette per species, not a different palette for every frame.
    strip=Image.new('RGB',(128*8,128))
    for i,frame in enumerate(frames): strip.paste(frame.convert('RGB'),(128*i,0))
    palette=strip.quantize(colors=48,method=Image.Quantize.MEDIANCUT)
    final=[]
    for i,frame in enumerate(frames):
        native=frame.convert('RGB').quantize(palette=palette,dither=Image.Dither.NONE).convert('RGBA'); native.putalpha(frame.getchannel('A'))
        native.putdata([(r,g,b,a) if a else (0,0,0,0) for r,g,b,a in native.get_flattened_data()])
        name=f'{species}_{i}.png'; native.save(output/name); final.append(native)
        box=native.getbbox(); assert box and box[0]>=6 and box[1]>=6 and box[2]<=122 and box[3]<=122
        proof.alpha_composite(native,(i*128,row*128))
        original=([0,1,2,3,4,6,5,7] if species=='moth' else list(range(8)))[i]
        manifest.append({'file':name,'ppu':64,'pivot':[.5,.5],'source_grid':[original%4,original//4],'eye_px':[44,72],'sha256':hashlib.sha256(native.tobytes()).hexdigest()})
    assert len({hashlib.sha256(f.tobytes()).hexdigest() for f in final})==8
    all_frames[species]=final
proof.resize((2048,512),Image.Resampling.NEAREST).convert('RGB').save(preview/'voadores-8-quadros.png')
animation=[]
for tick in range(48):
    canvas=Image.new('RGBA',(320,160),'#142831')
    canvas.alpha_composite(all_frames['bat'][(tick//3)%8],(16,12))
    canvas.alpha_composite(all_frames['moth'][(tick//2)%8],(176,12))
    animation.append(canvas.resize((960,480),Image.Resampling.NEAREST).convert('RGB'))
animation[0].save(preview/'batida-de-asas.gif',save_all=True,append_images=animation[1:],duration=[21,21,20]*16,loop=0,disposal=2)
(source/'layout.json').write_text(json.dumps({'tool':'built-in image_gen','frames':manifest,'animation':{'bat_fps':16,'moth_fps':24,'frames_per_species':8}},indent=2),encoding='utf-8')
print('Exported 16 distinct registered cels and animated preview',output)
