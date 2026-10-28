"""Original non-musical sparkle beds; no melody, chords, tempo, or external audio."""
import json
import numpy as np
import prepare_card_audio_v24 as base

RATE=base.RATE
SOURCE=base.PROJECT/'SourceArt/CardAudio_v25_Sparkle'


def build():
    base.build()  # Original reveal/choose/reroll cues, then replace both musical beds.
    SOURCE.mkdir(parents=True,exist_ok=True)
    outputs=[]
    for mythic in [False,True]:
        rng=np.random.default_rng(25021+int(mythic))
        y=np.zeros(round(6.4*RATE))
        for index in range(17 if mythic else 11):
            at=rng.uniform(.08,6.25)
            duration=rng.uniform(.18,.40)
            t=np.arange(round(duration*RATE))/RATE
            root=rng.uniform(2300,3900)
            glint=np.zeros(len(t))
            # Inharmonic, short glass-like transients, irregularly spaced, no tune.
            for ratio,gain in [(1,.50),(1.513,.24),(2.173,.14),(2.731,.07)]:
                glint+=gain*np.sin(2*np.pi*root*ratio*t+rng.uniform(0,6.28))*np.exp(-t/(.045+ratio*.014))
            glint=base.fade(glint,.003,.04)
            strength=rng.uniform(.20,.48)
            for delay,atten in [(0,1),(.083,.16),(.197,.07)]:
                positions=(np.arange(len(glint))+round((at+delay)*RATE))%len(y)
                np.add.at(y,positions,glint*strength*atten)
        name='MythicTreasureLoop.wav' if mythic else 'LegendaryTreasureLoop.wav'
        outputs.append(base.master(name,y,.026 if mythic else .018,.14 if mythic else .10))
    inherited=json.loads((base.SOURCE/'manifest.json').read_text(encoding='utf-8'))
    outputs=[x for x in inherited['outputs'] if 'TreasureLoop' not in x['file']]+outputs
    manifest={'version':25,'external_audio_assets':[],'type':'Non-musical intermittent shimmer; no pads, chords, melody or rhythmic arpeggios',
      'overlap':'One shared sparkle source per offer; high-rarity stingers divided by number of legendary/mythic cards', 'outputs':outputs}
    (SOURCE/'manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    (SOURCE/'ORIGENS.txt').write_text('AUDIO ORIGINAL V25 — BRILHO SEM MUSICA\nRevelacoes/confirmacao/reroll: sintese original v24.\nFundo lendario/mitico: cintilacoes curtas inarmonicas, espacadas irregularmente, sem melodia, acordes ou ritmo.\nUm brilho compartilhado por oferta. Volume de revelacao das cartas altas dividido pela quantidade delas.\nSem audio de terceiros. Receitas prepare_card_audio_v24.py e prepare_card_audio_v25.py.\n',encoding='utf-8')
    for x in outputs: print(x)

if __name__=='__main__': build()
