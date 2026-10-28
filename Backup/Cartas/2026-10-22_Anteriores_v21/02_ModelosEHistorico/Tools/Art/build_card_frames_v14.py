"""v14: replace the complete illustration aperture, never leave an old portrait under its rim."""
from pathlib import Path
from PIL import Image, ImageDraw
import hashlib, json, sys
import numpy as np

PROJECT=Path(__file__).resolve().parents[2]
ROOT=PROJECT/'SourceArt/CardCollection_v14_FrameRepair'
OUT=PROJECT/'Docs/ArtPreviews/CardCollection_v14'
RANKS=['Common','Rare','Epic','Legendary','Mythic']
FAMILIES=['AttackDamage','AttackSpeed','MaximumHealth','MoveSpeed','SpecialDamage']
COLORS=[('#747a49','#323a27'),('#3e93bb','#173b54'),('#a66bc4','#4d285d'),('#d8b65e','#72521e'),('#d55143','#631f25')]
OUTER=[(21,42),(159,42),(162,45),(162,211),(159,214),(21,214),(18,211),(18,45)]
METAL=[(22,43),(158,43),(161,46),(161,210),(158,213),(22,213),(19,210),(19,46)]
INNER=[(23,44),(157,44),(160,47),(160,209),(157,212),(23,212),(20,209),(20,47)]
OPENING=[(23,46),(157,46),(158,47),(158,209),(157,210),(23,210),(22,209),(22,47)]
def digest(path):
    path=Path(path);data=path.read_bytes()
    if path.suffix=='.meta':data=data.replace(b'\r\n',b'\n')
    return hashlib.sha256(data).hexdigest()
def polygon(points):
    mask=Image.new('L',(180,268));ImageDraw.Draw(mask).polygon(points,fill=255);return mask
def main():
    if '--check' in sys.argv:
        manifest=json.loads((ROOT/'manifest.json').read_text())
        assert len(manifest['cards'])==25
        for card in manifest['cards']:
            assert digest(PROJECT/card['asset'])==card['sha256'],card['asset']
            assert digest(PROJECT/card['labels'])==card['labels_sha256']
            assert digest(str(PROJECT/card['asset'])+'.meta')==card['meta_sha256']
        print('V14 ACTIVE CARD CHECK PASSED: 25 faces, preserved labels and sprite metadata.');return
    ROOT.mkdir(parents=True,exist_ok=True);OUT.mkdir(parents=True,exist_ok=True)
    outline=polygon(OUTER);opening=polygon(OPENING)
    provenance=np.array(outline)>0
    records=[]
    for family in FAMILIES:
        folder=OUT/family;folder.mkdir(exist_ok=True)
        for rank,name in enumerate(RANKS):
            source=PROJECT/'Docs/ArtPreviews/CardCollection_v12'/family
            base=Image.open(source/f'base_{rank}.png').convert('RGBA')
            original=np.array(base)
            # The previous 132x160 crop left strips of the previous portrait around it.
            # Recompose the entire opening, with one uninterrupted image and a closed bevel.
            art=base.crop((24,48,156,208)).resize((140,168),Image.Resampling.NEAREST)
            tile=Image.new('RGBA',(180,268));tile.paste(art,(20,44))
            draw=ImageDraw.Draw(base)
            draw.polygon(OUTER,fill='#050c13')
            draw.polygon(METAL,fill=COLORS[rank][0])
            draw.polygon(INNER,fill=COLORS[rank][1])
            base.paste(tile,(0,0),opening)
            # Only mythic hardware may cross the rim; transform its existing cutout
            # with the same mapping as the illustration so the two stay connected.
            projection=Image.new('L',(180,268))
            if rank==4:
                material=np.array(Image.open(source/f'materials_{rank}.png').convert('RGBA'))
                mask=(material[:,:,2]==255)
                mask[48:208,24:156]=False
                mask[:48,:]=False;mask[112:,:]=False
                original_cutout=Image.fromarray((mask*255).astype('uint8'))
                inverse=(132/140,0,24-20*132/140,0,160/168,48-44*160/168)
                projection=original_cutout.transform((180,268),Image.Transform.AFFINE,inverse,Image.Resampling.NEAREST)
                hardware=Image.fromarray(original).transform((180,268),Image.Transform.AFFINE,inverse,Image.Resampling.NEAREST)
                base.paste(hardware,(0,0),projection)
            # Every overwritten pixel must come from the new window, bevel or true hardware.
            changed=np.any(np.array(base)!=original,axis=2)
            allowed=provenance | (np.array(projection)>0)
            assert not np.any(changed & ~allowed),'Changed outer frame/text/art outside repair'
            image_pixels=(np.array(opening)>0) & (np.array(projection)==0)
            assert np.array_equal(np.array(base)[image_pixels],np.array(tile)[image_pixels]),'Mixed illustrations in opening'
            rim_pixels=provenance & (np.array(opening)==0) & (np.array(projection)==0)
            palette={tuple(bytes.fromhex(c.lstrip('#')))+ (255,) for c in ['#050c13',*COLORS[rank]]}
            assert all(tuple(c) in palette for c in np.array(base)[rim_pixels]),'Old portrait survived under rim'
            assert rank==4 or not np.any(np.array(projection)),'Only mythic may overhang'
            labels_path=PROJECT/'Docs/ArtPreviews/CardCollection_v13'/family/f'labels_{rank}.png'
            labels=Image.open(labels_path).convert('RGBA')
            face=Image.alpha_composite(base.resize((360,536),Image.Resampling.NEAREST),labels)
            base.save(folder/f'base_{rank}.png');face.save(folder/(name+'.png'))
            projection.save(folder/f'projection_{rank}.png')
            asset=PROJECT/'Assets/Resources/Cards'/family/(name+'.png')
            if '--apply' in sys.argv:face.save(asset)
            records.append(dict(family=family,rarity=name,asset=asset.relative_to(PROJECT).as_posix(),
                sha256=digest(folder/(name+'.png')),meta_sha256=digest(str(asset)+'.meta'),
                labels=labels_path.relative_to(PROJECT).as_posix(),labels_sha256=digest(labels_path),
                replacement_pixels=int(changed.sum()),overhang_pixels=int(np.count_nonzero(np.array(projection))),
                window_source='single v12 illustration',old_background_pixels_in_rebuilt_opening=0))
        sheet=Image.new('RGB',(1800,536),'#12202e')
        for rank,name in enumerate(RANKS):sheet.paste(Image.open(folder/(name+'.png')),(rank*360,0))
        sheet.save(OUT/(family+'-all.png'))
    outline.save(ROOT/'complete-replacement-mask.png');opening.save(ROOT/'illustration-mask.png')
    (ROOT/'manifest.json').write_text(json.dumps(dict(version=14,cards=records,applied='--apply' in sys.argv,
        previous_crop=[24,48,156,208],replacement_bounds=[18,42,163,215],
        aperture_polygon=OPENING,unmodified=['v13 labels','outer frames except rim','expressions','audio','card back','rarity effects']),
        ensure_ascii=False,indent=2),encoding='utf8')
    print(json.dumps(dict(cards=25,mode='APPLIED' if '--apply' in sys.argv else 'PREVIEW',
                          complete_apertures=True,labels='UNCHANGED',mythic_only_overhang=True)))
if __name__=='__main__':main()
