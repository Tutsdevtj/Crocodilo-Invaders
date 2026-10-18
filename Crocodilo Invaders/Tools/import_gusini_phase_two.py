"""Copy only original runtime PNGs; normalize the supplied timing documents.

Run with the package directory as the only argument, from the Unity project root.
Does not regenerate artwork or execute scripts from the package.
"""
import json
import hashlib
import shutil
import sys
from pathlib import Path

source = Path(sys.argv[1]).resolve()
art = Path('Assets/Art/Boss/GusiniPhase2')
sources = Path('SourceArt/GusiniPhase2')
sources.mkdir(parents=True, exist_ok=True)
sprites, clips = {}, []

def copy(pack, relative, pivot=(.5, .5), ui=False):
    origin = source / pack / relative
    target = (Path('Assets/Art/UI/GusiniBossBarF2') if ui else art / pack) / relative
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(origin, target)
    from PIL import Image
    with Image.open(origin) as im:
        width, height = im.size
    sprites[target.as_posix()] = dict(path=target.as_posix(), key=origin.stem,
        pivotX=pivot[0], pivotY=pivot[1], width=width, height=height,
        source_relative=f'{pack}/{relative}', sha256=hashlib.sha256(origin.read_bytes()).hexdigest())
    return target.as_posix()

def clip(pack, name, frames, loop=False, extra=None):
    paths, durations, events = [], [], []
    elapsed = 0
    for frame in frames:
        paths.append(copy(pack, frame['file'], (120/256, 78/196)))
        durations.append(frame['duration_ms'])
        event = frame.get('event') or frame.get('tag')
        if event:
            events.append(dict(time=elapsed/1000, value=event))
        elapsed += frame['duration_ms']
    events.extend(extra or [])
    if not loop:
        events.append(dict(time=elapsed/1000, value='clip_end'))
    clips.append(dict(name=name, frames=paths, durations=durations, loop=loop, events=events))

for pack in source.iterdir():
    if not pack.is_dir():
        continue
    out = sources / pack.name
    out.mkdir(exist_ok=True)
    for document in list(pack.glob('*.json')) + list(pack.glob('*.md')):
        shutil.copyfile(document, out / document.name)
    if pack.name.startswith('BossBar'):
        for folder in ('parts_x2', 'fx_x2'):
            for png in (pack / folder).glob('*.png'):
                copy(pack.name, png.relative_to(pack).as_posix(), ui=True)
        continue
    spec = json.loads(next(pack.glob('*.json')).read_text(encoding='utf-8'))
    if 'Transform' in pack.name:
        clip(pack.name, 'P2Transform', spec['frames'])
        clip(pack.name, 'P2Idle', [dict(file=spec['frames'][-1]['layers']['sprite'], duration_ms=1000)], True)
    elif 'Tentaculos' in pack.name:
        sequences = list(spec['clips'].values())
        clip(pack.name, 'P2Start', sequences[0]['frames'])
        clip(pack.name, 'P2Windup', sequences[1]['frames'][:2], True)
        clip(pack.name, 'P2Lash', sequences[1]['frames'][2:])
        clip(pack.name, 'P2Resist', sequences[2]['frames'][:4])
        clip(pack.name, 'P2Weak', sequences[2]['frames'][4:6], True)
        clip(pack.name, 'P2Close', sequences[2]['frames'][6:])
    else:
        name = 'P2Energy' if 'Ataque1' in pack.name else 'P2Egg' if 'Ataque2' in pack.name else 'P2Call'
        extra = [dict(time=.3, value='edge_warning')] if name == 'P2Call' else None
        clip(pack.name, name, spec['frames'], extra=extra)
        if name == 'P2Energy':
            first = [dict(f) for f in spec['frames']]
            first[4]['duration_ms'] += 300
            clip(pack.name, 'P2EnergyFirst', first)
    folders = {'Ataque1': ('projectile',), 'Ataque2': ('egg', 'parachute', 'yolks', 'fx'),
               'Ataque3': ('gansinho', 'fx'), 'Ataque4': ('lane_tentacle', 'lane', 'fx')}
    for token, subfolders in folders.items():
        if token not in pack.name:
            continue
        for folder in subfolders:
            for png in (pack / folder).glob('*.png'):
                if 'sheet' in png.stem or '_x2' in png.stem:
                    continue
                pivot = (19/36, .5) if folder == 'gansinho' else (1, .5) if folder == 'lane_tentacle' else (.5, .5)
                copy(pack.name, png.relative_to(pack).as_posix(), pivot)

shutil.copyfile(source / 'LEIA_PRIMEIRO_CODEX.md', sources / 'LEIA_PRIMEIRO_CODEX.md')
(sources / 'RuntimeManifest.json').write_text(json.dumps(dict(sprites=list(sprites.values()), clips=clips), indent=2), encoding='utf-8')
print(f'{len(sprites)} original PNGs; {len(clips)} variable-timed clips; source documents preserved.')
