"""Export the generated sheets into native, registered game cels; never redraw source art."""
from pathlib import Path
from PIL import Image, ImageDraw
import json, shutil, sys
from collections import deque

root = Path(__file__).resolve().parents[2]
source = root / 'SourceArt/MacaquiniHelpers_v23'
output = root / 'Assets/Resources/MacaquiniHelpersV23'
preview = root / 'Docs/ArtPreviews/MacaquiniHelpers_v23'
for folder in (source, output, preview): folder.mkdir(parents=True, exist_ok=True)
if len(sys.argv) == 3:
    for species, path in zip(('bat', 'frog'), sys.argv[1:]): shutil.copy2(path, source / f'{species}-source.png')
proof = Image.new('RGBA', (384, 224), '#142831')
manifest = []
for row, species in enumerate(('bat', 'frog')):
    sheet = Image.open(source / f'{species}-source.png').convert('RGBA')
    for i in range(3):
        cell = sheet.crop((round(sheet.width*i/3), 0, round(sheet.width*(i+1)/3), sheet.height))
        alpha = cell.getchannel('A').point(lambda x: 255 if x >= 180 else 0)
        cell.putalpha(alpha)
        box = alpha.getbbox()
        if not box: raise ValueError(f'Empty {species} {i}')
        cell = cell.crop(box)
        # Identical density within each sheet preserves pose proportions and wing expansion.
        scale = .135 if species == 'bat' else .12
        cell = cell.resize((round(cell.width*scale), round(cell.height*scale)), Image.Resampling.NEAREST)
        # Remove disconnected fragments from the neighboring cell, preserving the complete silhouette.
        pixels = cell.load(); seen = set(); largest = set()
        for yy in range(cell.height):
            for xx in range(cell.width):
                if (xx, yy) in seen or pixels[xx, yy][3] == 0: continue
                component = {(xx, yy)}; queue = deque([(xx, yy)]); seen.add((xx, yy))
                while queue:
                    x, y = queue.popleft()
                    for dx, dy in ((1,0),(-1,0),(0,1),(0,-1),(1,1),(-1,1),(1,-1),(-1,-1)):
                        nx, ny = x+dx, y+dy
                        if 0 <= nx < cell.width and 0 <= ny < cell.height and (nx,ny) not in seen and pixels[nx,ny][3]:
                            seen.add((nx,ny)); component.add((nx,ny)); queue.append((nx,ny))
                if len(component) > len(largest): largest = component
        for yy in range(cell.height):
            for xx in range(cell.width):
                if (xx,yy) not in largest: pixels[xx,yy] = (0,0,0,0)
        cel = Image.new('RGBA', (128, 112))
        if cell.width > 120 or cell.height > 104: raise ValueError('Unsafe sprite margin')
        cel.alpha_composite(cell, ((128-cell.width)//2, (112-cell.height)//2))
        rgb = cel.convert('RGB').quantize(colors=48, method=Image.Quantize.MEDIANCUT).convert('RGBA')
        rgb.putalpha(cel.getchannel('A'))
        # Fully transparent pixels have zero RGB, avoiding halos in import tools.
        data = [(r,g,b,a) if a else (0,0,0,0) for r,g,b,a in rgb.getdata()]
        rgb.putdata(data)
        path = output / f'{species}_{i}.png'; rgb.save(path)
        proof.alpha_composite(rgb, (128*i, row*112))
        manifest.append({'file':path.name, 'pixels':[128,112], 'ppu':64, 'pivot':[.5,.5], 'source_crop':box})
proof.resize((1152,672), Image.Resampling.NEAREST).convert('RGB').save(preview/'modelos-x3.png')
(source/'layout.json').write_text(json.dumps({'tool':'built-in image_gen', 'frames':manifest}, indent=2), encoding='utf-8')
print('Exported six cels and preview', output)
