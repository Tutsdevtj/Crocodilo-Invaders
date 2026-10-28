"""Remove edge-connected exterior matte only; preserve RGB and all protected card content."""
from pathlib import Path
from collections import deque
from PIL import Image, ImageDraw
import hashlib,json,sys
import os
import numpy as np

P=Path(__file__).resolve().parents[2]
ROOT=P/'SourceArt/CardCollection_v15_Transparency'
OUT=P/'Docs/ArtPreviews/CardCollection_v15'
RANKS=['Common','Rare','Epic','Legendary','Mythic']
FAMILIES=['AttackDamage','AttackSpeed','MaximumHealth','MoveSpeed','SpecialDamage']
def sha(path):
    path=Path(path);data=path.read_bytes()
    if path.suffix=='.meta':data=data.replace(b'\r\n',b'\n')
    return hashlib.sha256(data).hexdigest()
def install(image, asset):
    staging=ROOT/'staging';staging.mkdir(exist_ok=True)
    temporary=staging/(asset.parent.name+'-'+asset.name)
    image.save(temporary)
    os.replace(temporary,asset)
def cutout(image):
    original=np.array(image.convert('RGBA'))
    h,w=original.shape[:2]
    small=np.array(image.convert('RGBA').resize((180,268),Image.Resampling.NEAREST))
    rgb=small[:,:,:3]
    # The source matte is dark blue/black. Never key dark pixels inside the card.
    y,x=np.indices((268,180))
    exterior=(x<18)|(x>=162)|(y<10)|(y>=258)
    candidate=(rgb[:,:,0]<=22)&(rgb[:,:,1]<=32)&(rgb[:,:,2]<=42)&exterior
    connected=np.zeros((268,180),bool)
    queue=deque()
    for yy in range(268):
        for xx in [0,179]:
            if candidate[yy,xx]:connected[yy,xx]=True;queue.append((xx,yy))
    for xx in range(180):
        for yy in [0,267]:
            if candidate[yy,xx] and not connected[yy,xx]:connected[yy,xx]=True;queue.append((xx,yy))
    while queue:
        xx,yy=queue.popleft()
        for nx,ny in [(xx-1,yy),(xx+1,yy),(xx,yy-1),(xx,yy+1)]:
            if 0<=nx<180 and 0<=ny<268 and candidate[ny,nx] and not connected[ny,nx]:
                connected[ny,nx]=True;queue.append((nx,ny))
    # Retain a one-pixel dark ink edge alongside real metal, not a rectangular matte.
    foreground=~connected
    padded=np.pad(foreground,1,constant_values=False)
    near=np.zeros_like(foreground)
    for dy in range(3):
        for dx in range(3):near|=padded[dy:dy+268,dx:dx+180]
    remove=connected & ~near
    mask=np.array(Image.fromarray((remove*255).astype('uint8')).resize((w,h),Image.Resampling.NEAREST))>0
    result=original.copy();result[mask,3]=0
    assert np.array_equal(result[:,:,:3],original[:,:,:3]),'RGB changed'
    assert np.array_equal(result[~mask],original[~mask]),'Opaque content changed'
    assert remove.sum()>10,'No external matte found'
    assert not np.any(remove[10:258,18:162]),'Removed inner content'
    return Image.fromarray(result),int(mask.sum())
def checker(size):
    y,x=np.indices((size[1],size[0]));a=np.zeros((size[1],size[0],4),np.uint8)
    c=((x//16+y//16)%2)*32+108;a[:,:,:3]=c[:,:,None];a[:,:,3]=255
    return Image.fromarray(a)
def main():
    if '--check' in sys.argv:
        m=json.loads((ROOT/'manifest.json').read_text())
        for r in m['assets']:
            assert sha(P/r['file'])==r['sha256']
            assert sha(str(P/r['file'])+'.meta')==r['meta_sha256']
        print('V15 ALPHA CHECK PASSED: 25 fronts and back, RGB/content and metadata preserved.');return
    ROOT.mkdir(parents=True,exist_ok=True);OUT.mkdir(parents=True,exist_ok=True);records=[]
    for family in FAMILIES:
        folder=OUT/family;folder.mkdir(exist_ok=True)
        for rank,name in enumerate(RANKS):
            source=P/'Docs/ArtPreviews/CardCollection_v14'/family/(name+'.png')
            image=Image.open(source).convert('RGBA');result,count=cutout(image)
            final=folder/(name+'.png');result.save(final)
            asset=P/'Assets/Resources/Cards'/family/(name+'.png')
            if '--apply' in sys.argv:install(result,asset)
            records.append(dict(file=asset.relative_to(P).as_posix(),sha256=sha(final),meta_sha256=sha(str(asset)+'.meta'),
                                transparent_pixels=count,source_sha256=sha(source)))
        board=checker((1800,536))
        for i,name in enumerate(RANKS):board.alpha_composite(Image.open(folder/(name+'.png')),(i*360,0))
        board.convert('RGB').save(OUT/(family+'-checker.png'))
    old=P/'Docs/ArtPreviews/CardCollection_v13/Back.png'
    result,count=cutout(Image.open(old));result.save(OUT/'Back.png')
    asset=P/'Assets/Art/Cards/Back.png'
    if '--apply' in sys.argv:install(result,asset)
    records.append(dict(file=asset.relative_to(P).as_posix(),sha256=sha(OUT/'Back.png'),
        meta_sha256=sha(str(asset)+'.meta'),transparent_pixels=count,source_sha256=sha(old)))
    (ROOT/'manifest.json').write_text(json.dumps(dict(version=15,assets=records,applied='--apply' in sys.argv,
        changes='Alpha only; connected exterior matte. One pixel ink edge preserved.',rgb='UNCHANGED'),indent=2),encoding='utf8')
    print(json.dumps(dict(assets=len(records),transparent_pixels=[r['transparent_pixels'] for r in records],
                          mode='APPLIED' if '--apply' in sys.argv else 'PREVIEW')))
if __name__=='__main__':main()
