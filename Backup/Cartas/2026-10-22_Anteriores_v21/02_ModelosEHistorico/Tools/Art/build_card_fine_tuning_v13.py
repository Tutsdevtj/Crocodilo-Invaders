"""Rebuild only editable text layers, a genuine card back, and four original arcade stingers."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import hashlib, json, shutil, sys, wave
import numpy as np

PROJECT = Path(__file__).resolve().parents[2]
ROOT = PROJECT / 'SourceArt/CardCollection_v13_FineTuning'
PREVIEW = PROJECT / 'Docs/ArtPreviews/CardCollection_v13'
RANKS = ['Common', 'Rare', 'Epic', 'Legendary', 'Mythic']
NAMES = ['COMUM', 'RARO', 'ÉPICO', 'LENDÁRIO', 'MÍTICO']
COLORS = ['#c5c6ad', '#a8d7e8', '#d6a6f9', '#ffe0a0', '#ffb18d']
FONT = PROJECT / 'Assets/Fonts'
FAMILIES = json.loads((PROJECT / 'SourceArt/CardCollection_v09/manifest.json').read_text(encoding='utf8'))['families']
GENERATED = ROOT / 'Back-source.png'
RATE = 44100

def sha(data):
    return hashlib.sha256(data).hexdigest()

def font(style, size):
    if style == 'display':
        return ImageFont.truetype(str(FONT / 'PressStart2P/PressStart2P-Regular.ttf'), size)
    result = ImageFont.truetype(str(FONT / 'PixelifySans/PixelifySans-VariableFont_wght.ttf'), size)
    result.set_variation_by_axes([700])
    return result

def labels(family, rank):
    layer = Image.new('RGBA', (360, 536))
    draw = ImageDraw.Draw(layer)
    draw.fontmode = '1'
    specs = [
        ('rarity', NAMES[rank], 'support', 16, 47, (64, 30, 296, 49)),
        ('title', family['title'], 'display', 20, 73, (64, 51, 296, 79)),
        ('attribute', family['attribute'], 'support', 16, 454, (64, 435, 296, 457)),
        ('bonus', '+' + str(family['values'][rank]) + '%', 'display', 32, 492, (64, 458, 296, 498)),
    ]
    metrics = []
    for name, text, style, size, bottom, safe in specs:
        selected = font(style, size)
        box = draw.textbbox((0, 0), text, font=selected)
        x = round(180 - (box[0] + box[2]) / 2)
        y = bottom - box[3]
        rect = (x + box[0], y + box[1], x + box[2], y + box[3])
        assert rect[0] >= safe[0] and rect[1] >= safe[1] and rect[2] <= safe[2] and rect[3] + 1 <= safe[3], (text, rect)
        if style == 'display':
            draw.text((x, y + 1), text, font=selected, fill=(5, 11, 17, 180))
        draw.text((x, y), text, font=selected, fill=COLORS[rank] if style == 'support' else '#fff0ce')
        metrics.append(dict(role=name, text=text, size=size, bounds=rect, safe_box=safe))
    return layer, metrics

def stinger(rank, duration):
    # Distinct orchestration, not one sample pitch-shifted four ways.
    count = round(duration * RATE)
    output = np.zeros(count, dtype=np.float64)
    random = np.random.default_rng(7300 + rank)
    def add(start, signal):
        offset = round(start * RATE)
        end = min(count, offset + len(signal))
        output[offset:end] += signal[:end-offset]
    def tone(start, hz, length, amp, bright=0.35):
        t = np.arange(round(length * RATE)) / RATE
        attack = np.minimum(1, t / .004)
        release = np.minimum(1, (length - t) / .03)
        env = attack * release * np.exp(-t * (5 if rank == 1 else 3.5))
        body = np.sin(2*np.pi*hz*t) + bright*np.sin(2*np.pi*hz*2*t) + .10*np.sin(2*np.pi*hz*3*t)
        add(start, body * env * amp)
    # Mechanical snap and low impact. Increasing bass and layers with rarity.
    t = np.arange(round(.16 * RATE)) / RATE
    click = random.normal(0, 1, len(t)) * np.exp(-t*85) * np.minimum(1, t/.001) * .035
    add(0, click)
    bass_t = np.arange(round((.15 + rank*.045) * RATE)) / RATE
    phase = 2*np.pi*((100 + rank*10)*bass_t - 65*bass_t*bass_t)
    bass = np.sin(phase) * np.exp(-bass_t*15) * np.minimum(1, bass_t/.003)
    add(0, bass * (.055 + rank*.025))
    notes = {
        1: [(0, 659.255), (.09, 987.767)],
        2: [(0, 440), (.065, 554.365), (.13, 659.255), (.195, 880)],
        3: [(0, 293.665), (.055, 440), (.11, 587.33), (.165, 739.989), (.22, 880), (.275, 1174.66)],
        4: [(0, 146.832), (.045, 293.665), (.09, 440), (.135, 587.33), (.18, 739.989), (.225, 880), (.27, 1174.66)],
    }[rank]
    for i, (start, hz) in enumerate(notes):
        tone(start, hz, .25 + rank*.07, .12 if rank == 1 else .075, .24 + rank*.045)
    if rank >= 3:
        start = .30 if rank == 3 else .32
        for hz in [293.665, 440, 587.33, 739.989]:
            tone(start, hz, duration-start-.03, .038 if rank == 3 else .05, .2)
    if rank == 4:
        # A short energy swell, octave shimmer and a second low punch.
        t = np.arange(round(.34*RATE)) / RATE
        noise = random.normal(0, 1, len(t))
        noise = np.convolve(noise, np.ones(8)/8, mode='same')
        env = np.sin(np.pi*np.clip(t/.34, 0, 1)) ** 2
        add(.02, noise * env * .08)
        tone(.34, 1174.66, .92, .04, .35)
        tone(.39, 1480, .82, .025, .2)
        tone(.31, 73.416, .46, .08, .08)
    # End at zero; preserve headroom even with three overlapping mythics.
    output *= np.minimum(1, np.arange(count)[::-1]/(.04*RATE))
    output -= output.mean()
    peak = np.max(np.abs(output))
    if rank == 4:
        output = np.tanh(output / peak * 2.4)
        peak = np.max(np.abs(output))
    output *= [0, .28, .32, .36, .32][rank] / peak
    pcm = np.round(output*32767).astype('<i2')
    return pcm, dict(duration=duration, peak=float(np.max(np.abs(output))), rms=float(np.sqrt(np.mean(output**2))),
                     notes=[hz for _,hz in notes], sample_rate=RATE, channels=1)

def main():
    if '--check' in sys.argv:
        manifest = json.loads((ROOT/'manifest.json').read_text(encoding='utf8'))
        assert len(manifest['cards']) == 25
        for record in manifest['cards']:
            asset = PROJECT/'Assets/Resources/Cards'/record['family']/(record['rarity']+'.png')
            assert sha(asset.read_bytes()) == record['sha256'], str(asset)
        assert sha((PROJECT/manifest['back']['file']).read_bytes()) == manifest['back']['sha256']
        for record in manifest['audio']:
            assert sha((PROJECT/record['file']).read_bytes()) == record['sha256']
            assert record['three_card_mix_peak'] < .95
        print('CARD FINE TUNING SOURCE CHECK PASSED: 25 faces, back and four stingers.')
        return
    ROOT.mkdir(parents=True, exist_ok=True)
    PREVIEW.mkdir(parents=True, exist_ok=True)
    records = []
    for family in FAMILIES:
        target = PREVIEW / family['id']
        target.mkdir(exist_ok=True)
        for rank, name in enumerate(RANKS):
            original = PROJECT / 'Docs/ArtPreviews/CardCollection_v12' / family['id']
            base = Image.open(original / f'base_{rank}.png').convert('RGBA').resize((360,536), Image.Resampling.NEAREST)
            layer, metrics = labels(family, rank)
            face = Image.alpha_composite(base, layer)
            previous = np.array(Image.open(original / f'{name}.png').convert('RGBA'))
            current = np.array(face)
            changed = np.any(previous != current, axis=2)
            allowed = np.zeros((536,360), dtype=bool)
            allowed[20:83,50:310] = True
            allowed[428:506,50:310] = True
            assert not np.any(changed & ~allowed), 'Modified artwork outside text plates'
            layer.save(target / f'labels_{rank}.png')
            face.save(target / f'{name}.png')
            asset = PROJECT / 'Assets/Resources/Cards' / family['id'] / f'{name}.png'
            meta_hash = sha(Path(str(asset)+'.meta').read_bytes())
            shutil.copyfile(target / f'{name}.png', asset)
            assert sha(Path(str(asset)+'.meta').read_bytes()) == meta_hash
            records.append(dict(family=family['id'], rarity=name, sha256=sha(asset.read_bytes()),
                                source_base_sha256=sha((original/f'base_{rank}.png').read_bytes()), meta_sha256=meta_hash,
                                changed_pixels=int(changed.sum()), labels=metrics))
    back = PROJECT / 'Assets/Art/Cards/Back.png'
    backup = ROOT / 'Back-before.png'
    if not backup.exists():
        shutil.copyfile(back, backup)
    Image.open(GENERATED).convert('RGBA').resize((180,268), Image.Resampling.NEAREST).save(PREVIEW/'Back.png')
    shutil.copyfile(PREVIEW/'Back.png', back)
    audio = []
    for rank, (name, duration) in enumerate([('RareReveal', .42),('EpicReveal',.74),('LegendaryReveal',1.04),('MythicReveal',1.42)], 1):
        destination = PROJECT / 'Assets/Audio/CardRewards' / (name+'.wav')
        backup = ROOT / (name+'-before.wav')
        if destination.exists() and not backup.exists():
            shutil.copyfile(destination, backup)
        pcm, info = stinger(rank, duration)
        with wave.open(str(destination), 'wb') as stream:
            stream.setnchannels(1);stream.setsampwidth(2);stream.setframerate(RATE);stream.writeframes(pcm.tobytes())
        gain = [0, .60, .72, .84, .96][rank]
        mixed = np.zeros(len(pcm) + round(.4*RATE))
        for offset in [0, round(.2*RATE), round(.4*RATE)]:
            mixed[offset:offset+len(pcm)] += pcm.astype(float) / 32767 * gain
        assert np.max(np.abs(mixed)) < .95, 'Three-card mix exceeds headroom'
        audio.append(dict(rarity=RANKS[rank], file=str(destination.relative_to(PROJECT)),sha256=sha(destination.read_bytes()),
                          playback_gain=gain, three_card_mix_peak=float(np.max(np.abs(mixed))), **info))
    manifest = dict(version=13, text_renderer='Pillow/FreeType on full padded canvas; no Pango clipping',
                    card_size=[360,536], artwork_outside_text_plates='UNCHANGED', cards=records, audio=audio,
                    back=dict(file='Assets/Art/Cards/Back.png', sha256=sha(back.read_bytes()), size=[180,268],
                              source='Back-source.png', generator='Built-in image_gen', prompt_file='Back-prompt.txt'))
    (ROOT/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf8')
    for family in FAMILIES:
        sheet=Image.new('RGB',(1800,536),'#102030')
        for rank,name in enumerate(RANKS):
            sheet.paste(Image.open(PREVIEW/family['id']/f'{name}.png'),(rank*360,0))
        sheet.save(PREVIEW/(family['id']+'-all.png'))
    print(json.dumps(dict(cards=len(records), outside_text_plates='UNCHANGED', back='NEW', audio=audio),ensure_ascii=False))
if __name__=='__main__': main()
