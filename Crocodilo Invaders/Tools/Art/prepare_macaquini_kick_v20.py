"""Split generated complete poses, keeping one scale and a registered pelvis."""
from pathlib import Path
from PIL import Image
import json, shutil
root=Path(__file__).resolve().parents[2]
source=Path(r'C:/Users/berna/.codex/generated_images/01a164ae-b7b0-7f90-8b04-583b1c9a507b/exec-807bce49-bcd3-4ce0-9537-05998a2423b0.png')
dest=root/'Assets/Art/JungleStage/Macaquini_v20';dest.mkdir(parents=True,exist_ok=True)
archive=root/'SourceArt/MacaquiniKick_v20';archive.mkdir(parents=True,exist_ok=True)
shutil.copy2(source,archive/'kick-generated.png')
sheet=Image.open(source).convert('RGBA')
rects=[(0,0,530,512),(530,0,1010,512),(1010,0,1536,512),(0,512,605,1024),(605,512,1050,1024),(1050,512,1536,1024)]
hips=[(310,357),(800,358),(1335,400),(350,880),(820,865),(1370,879)]
for i,(rect,hip) in enumerate(zip(rects,hips)):
    cel=sheet.crop(rect);cel=cel.resize((round(cel.width*.64),round(cel.height*.64)),Image.Resampling.NEAREST)
    canvas=Image.new('RGBA',(512,512));point=(round(256-(hip[0]-rect[0])*.64),round(300-(hip[1]-rect[1])*.64))
    canvas.paste(cel,point);canvas.save(dest/f'kick_{i:02}.png')
(archive/'layout.json').write_text(json.dumps({'scale':.64,'ppu':80,'pivot':[270/512,1-322/512],'rects':rects,'hips':hips},indent=2))
print('6 complete kick poses, 80 PPU, common pelvis and scale')
