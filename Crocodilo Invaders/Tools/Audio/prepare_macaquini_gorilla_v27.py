"""Heavy transformation roar, built from CC0 gorilla recordings, not monkey imitations."""
from pathlib import Path
import subprocess, wave, hashlib, json
import numpy as np

ROOT=Path(__file__).resolve().parents[2]; SOURCE=ROOT/'SourceArt/MacaquiniGorillaRoar_v27'
OUT=ROOT/'Assets/Resources/Audio/MacaquiniGorillaV27'; PREVIEW=ROOT/'Docs/ArtPreviews/MacaquiniGorillaRoar_v27'
FFMPEG=ROOT/'Builds/VideoDependencies/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe'; RATE=44100
for p in (OUT,PREVIEW,SOURCE/'Working'):p.mkdir(parents=True,exist_ok=True)

def read(path):
    with wave.open(str(path)) as w:return np.frombuffer(w.readframes(w.getnframes()),'<i2').astype(float).reshape(-1,w.getnchannels()).mean(axis=1)/32768

def process(name,file,filters):
    path=SOURCE/'Working'/name
    subprocess.run([str(FFMPEG),'-hide_banner','-loglevel','error','-y','-i',str(SOURCE/'Originals'/file),'-af',filters,'-ar',str(RATE),'-ac','1','-c:a','pcm_s16le',str(path)],check=True)
    return read(path)

def filtered(x,low,high):
    n=1<<(len(x)-1).bit_length();freq=np.fft.rfftfreq(n,1/RATE)
    response=(1-1/(1+(freq/low)**4))/(1+(freq/high)**6)
    return np.fft.irfft(np.fft.rfft(x,n)*response,n)[:len(x)]

def master(name,dry,tail,peak,drive):
    dry-=dry.mean(); dry=filtered(dry,48,7000)
    active=np.flatnonzero(np.abs(dry)>.018*np.max(np.abs(dry)))
    dry=dry[max(0,active[0]-180):min(len(dry),active[-1]+700)]
    # Dense, chesty harmonics and a controlled, forward midrange scream.
    dry=np.tanh(dry/(np.sqrt(np.mean(dry*dry))+1e-8)*drive)
    dry=filtered(dry,45,7800)
    edge=min(440,len(dry)//10);dry[:edge]*=np.linspace(0,1,edge);dry[-round(.15*RATE):]*=np.linspace(1,0,round(.15*RATE))
    out=np.zeros((len(dry)+round(tail*RATE),2));out[:len(dry)]=dry[:,None]
    # Echo supports the roar; the direct gorilla stays centered and dominant.
    for delay,gain,side,cut in [(.12,.13,0,4200),(.24,.1,1,3600),(.43,.065,0,2700),(.72,.035,1,2000)]:
        start=round(delay*RATE);echo=filtered(dry,100,cut);length=min(len(echo),len(out)-start)
        out[start:start+length,side]+=echo[:length]*gain;out[start:start+length,1-side]+=echo[:length]*gain*.2
    out*=peak/max(1e-8,np.max(np.abs(out)));out[-round(.1*RATE):]*=np.linspace(1,0,round(.1*RATE))[:,None]
    write(OUT/name,out)
    stats={'file':name,'seconds':len(out)/RATE,'dry_seconds':len(dry)/RATE,'peak':float(np.max(np.abs(out))),
           'rms':float(np.sqrt(np.mean(out*out))),'dry_rms':float(np.sqrt(np.mean(out[:len(dry)]**2))),
           'sha256':hashlib.sha256((OUT/name).read_bytes()).hexdigest()}
    assert stats['peak']<=.95 and stats['dry_rms']>.12 and np.isfinite(out).all()
    return out,stats

def write(path,x):
    with wave.open(str(path),'wb') as w:
        w.setnchannels(2);w.setsampwidth(2);w.setframerate(RATE);w.writeframes(np.round(np.clip(x,-1,1)*32767).astype('<i2').tobytes())

grunt=process('grunt.wav','gorilla_snarl_03.flac','asetrate=39690,aresample=44100,atempo=1.15')
rising=process('rising.wav','gorilla_roar.flac','asetrate=39690,aresample=44100,atempo=0.85')
rising*=np.linspace(.3,1,len(rising))
main=process('main.wav','gorilla_roar.flac','asetrate=39690,aresample=44100,atempo=0.68')
body=process('body.wav','gorilla_roar.flac','asetrate=31752,aresample=44100,atempo=0.85')
body=np.pad(body,(0,max(0,len(main)-len(body))))[:len(main)]
main=main*.85+filtered(body,48,1500)*.48
clips=[];stats=[]
for spec in [('gorilla_effort.wav',grunt,.5,.78,.6),('gorilla_rising.wav',rising,.7,.86,.85),('gorilla_power.wav',main,1.15,.94,1.05)]:
    clip,record=master(*spec);clips.append(clip);stats.append(record)
timeline=np.zeros((11*RATE,2))
for time,clip,gain in [(0,clips[0],.62),(3.25,clips[1],.82),(5.5,clips[2],.96)]:
    start=round(time*RATE);timeline[start:start+len(clip)]+=clip*gain
assert np.max(np.abs(timeline))<.98
write(PREVIEW/'rugido-gorila-preview.wav',clips[2]*.96)
write(PREVIEW/'transformacao-gorila-preview.wav',timeline)
manifest={'source':'https://opengameart.org/content/gorilla-sounds','roar_author':'J0ck0','original_roar':'https://freesound.org/s/397054/',
          'snarl_source':'mediacollege.com','editor':'AntumDeluge','license':'CC0 1.0','edits':'Gorilla roar, modest pitch lowering/time stretch, low body layer, strong saturation, short quiet stereo echo.',
          'clips':stats,'timeline_peak':float(np.max(np.abs(timeline))),'main_gain':.96,
          'main_rms_gain_over_v26':float(np.sqrt(np.mean((clips[2]*.96)**2))/.04694747)}
(SOURCE/'manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8');print(json.dumps(manifest,indent=2))
