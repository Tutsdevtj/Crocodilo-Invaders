"""Original Crocodilo Invaders reward audio: procedural synthesis, no external audio.
Run with Python + NumPy. WAV output uses the standard library only.
"""
from pathlib import Path
import hashlib
import json
import wave
import numpy as np

PROJECT=Path(__file__).resolve().parents[2]
SOURCE=PROJECT/'SourceArt/CardAudio_v24_Original'
RATE=44100
SEED=24022


def hz(midi): return 440.0*2**((midi-69)/12)


def fade(signal,attack=.004,release=.04):
    signal=signal.copy()
    a=min(len(signal),round(attack*RATE))
    r=min(len(signal),round(release*RATE))
    signal[:a]*=np.linspace(0,1,a)
    signal[-r:]*=np.linspace(1,0,r)
    return signal


def tone(note,duration=.7,kind='crystal'):
    t=np.arange(round(duration*RATE))/RATE
    f=hz(note)
    phase=2*np.pi*f*t
    if kind=='arcade':
        # Soft band-limited pulse/triangle lead: a playful power-up, not a buzzer.
        carrier=np.sin(phase)+.16*np.sin(3*phase)/3+.10*np.sin(5*phase)/5
        y=carrier*np.exp(-t/0.18)
        y+=.24*np.sin(phase*2)*np.exp(-t/.095)
    elif kind=='pad':
        # Major chord bed with a gentle attack and low harmonic content.
        y=(np.sin(phase)+.17*np.sin(phase*2)+.10*np.sin(phase*1.0018))*np.exp(-t/1.1)
        return fade(y,.11,.30)*.65
    else:
        # Original FM crystal with a bright onset that settles into a pure bell.
        index=1.5*np.exp(-t/.085)
        y=np.sin(phase+index*np.sin(phase*2))*np.exp(-t/.32)
        y+=.20*np.sin(phase*3)*np.exp(-t/.11)
        y+=.10*np.sin(phase*4)*np.exp(-t/.055)
    return fade(y,.0035,.08)*.75


def mix(seconds,events,cyclic=False):
    out=np.zeros(round(seconds*RATE))
    for start,note,gain,kind,duration in events:
        sound=tone(note,duration,kind)
        echoes=[(0,1),(.092,.17),(.211,.10),(.367,.065),(.593,.035)]
        for delay,atten in echoes:
            ix=np.arange(len(sound))+round((start+delay)*RATE)
            if cyclic: np.add.at(out,ix%len(out),sound*gain*atten)
            else:
                valid=ix<len(out)
                out[ix[valid]]+=sound[valid]*gain*atten
    return out


def run(notes,gap,start=0,gain=.7,kind='arcade'):
    return [(start+i*gap,n,gain*(.85+.15*i/max(1,len(notes)-1)),kind,.65) for i,n in enumerate(notes)]


def chord(at,notes,gain=.12,duration=1.35):
    return [(at,n,gain,'pad',duration) for n in notes]


def master(name,y,target_rms,peak):
    y-=np.mean(y)
    y*=target_rms/max(np.sqrt(np.mean(y*y)),1e-12)
    # Gentle limiting controls transients while retaining audible sustained sparkle.
    y=np.tanh(y/(peak*.85))*(peak*.85)
    y*=min(1,peak/max(np.max(abs(y)),1e-12))
    y=fade(y,.005,.018)
    path=PROJECT/'Assets/Audio/CardRewards'/name
    pcm=np.round(np.clip(y,-1,1)*32767).astype('<i2')
    with wave.open(str(path),'wb') as w:
        w.setnchannels(1);w.setsampwidth(2);w.setframerate(RATE);w.writeframes(pcm.tobytes())
    return {'file':path.relative_to(PROJECT).as_posix(),'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),
      'duration':len(y)/RATE,'rms':float(np.sqrt(np.mean(y*y))),'peak':float(np.max(abs(y))),'channels':1}


def build():
    SOURCE.mkdir(parents=True,exist_ok=True)
    records=[]
    reveals={
      'CommonReveal':(.26,run([84,91],.055,gain=.45)+run([96],0,.07,.15,'crystal'),.037,.16),
      'RareReveal':(.62,run([72,76,79,84],.065)+run([91],0,.20,.28,'crystal'),.065,.22),
      'EpicReveal':(.94,run([72,76,79,84,88],.075)+chord(.22,[72,76,79],.10)+run([91,96],.10,.35,.26,'crystal'),.09,.29),
      'LegendaryReveal':(1.40,run([72,76,79,84,88,91],.075)+chord(.30,[60,64,67,72],.16)+run([84,88,91,96],.095,.45,.40,'crystal'),.12,.35),
      'MythicReveal':(1.95,run([72,76,79,84,88,91,96],.07)+chord(.32,[60,64,67,72],.21)+run([84,88,91,96],.085,.42,.48,'crystal')+run([88,91,96,100],.095,.94,.30,'crystal'),.15,.40),
      'ChooseReward':(.65,run([79,84,88],.065,gain=.55)+run([96],0,.19,.28,'crystal')+chord(.15,[72,76,79],.10,.5),.075,.25),
    }
    for name,(duration,events,rms,peak) in reveals.items():
        records.append(master(name+'.wav',mix(duration,events),rms,peak))
    # Original short airy shuffle with a small rising electronic tick.
    rng=np.random.default_rng(SEED)
    t=np.arange(round(.20*RATE))/RATE
    noise=rng.normal(0,1,len(t)); noise=np.concatenate(([0],np.diff(noise)))
    noise=noise/(max(abs(noise))+1e-12)*np.exp(-t/.025)
    tick=np.sin(2*np.pi*(1100*t+1600*t*t))*np.exp(-t/.025)
    records.append(master('Reroll.wav',fade(.20*noise+.32*tick),.025,.12))
    # Major-key treasure music: warm major chords + tiny ascending crystal motifs.
    harmonies=[[60,64,67,72],[65,69,72,76],[67,71,74,79],[60,64,67,72]]
    motifs=[[84,88,91,96],[81,84,88,93],[79,83,86,91],[84,88,91,96]]
    for mythic in [False,True]:
        events=[]
        for bar,(harmony,notes) in enumerate(zip(harmonies,motifs)):
            start=bar*1.6
            events+=chord(start,harmony,.13 if mythic else .10,2.1)
            for i,n in enumerate(notes):
                events.append((start+i*.4,n,.37 if mythic else .30,'crystal',.85))
                if mythic: events.append((start+i*.4+.20,n+12,.09,'crystal',.55))
        name='MythicTreasureLoop' if mythic else 'LegendaryTreasureLoop'
        records.append(master(name+'.wav',mix(6.4,events,True),.10 if mythic else .075,.40 if mythic else .33))
    # Assert the audible progression used by the three real card slots.
    ranks=[next(x for x in records if x['file'].endswith(n+'Reveal.wav')) for n in ['Rare','Epic','Legendary','Mythic']]
    assert all(b['duration']>a['duration'] and b['rms']*gb>a['rms']*ga for a,b,ga,gb in zip(ranks,ranks[1:],[.60,.72,.84],[.72,.84,.96]))
    manifest={'version':24,'external_audio_assets':[],'composition':'Original procedural reward sounds for Crocodilo Invaders',
      'instruments':['soft arcade pulse/triangle','FM crystal chime','warm sine pad','seeded noise shuffle'],
      'recipe':'Tools/Audio/prepare_card_audio_v24.py','seed':SEED,'sample_rate':RATE,'outputs':records}
    (SOURCE/'manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    (SOURCE/'ORIGENS.txt').write_text('SONS ORIGINAIS DAS CARTAS — V24\nComposicoes e timbres sintetizados para Crocodilo Invaders.\nSem samples, loops, vinhetas ou pacotes de audio de terceiros.\nGeracao: Tools/Audio/prepare_card_audio_v24.py (Python, NumPy e wave).\nRevelacoes: pulso arcade suave, frases ascendentes, acorde maior e cristais FM.\nFundos: acordes maiores, sinos cintilantes e arpejos; mitica com mais brilho.\nNove WAVs PCM mono 44,1kHz. Manifesto de hashes acompanha esta receita.\n',encoding='utf-8')
    for item in records: print(Path(item['file']).name,'RMS',round(item['rms'],4),'peak',round(item['peak'],3))

if __name__=='__main__': build()
