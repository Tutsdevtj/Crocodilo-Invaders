"""Edit AntumDeluge's CC0 monkey imitations into restrained stereo forest calls."""
from pathlib import Path
import numpy as np
import wave, hashlib, json

ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'SourceArt/MacaquiniJungleVoice_v26'
OUT=ROOT/'Assets/Resources/Audio/MacaquiniJungleVoiceV26'
PREVIEW=ROOT/'Docs/ArtPreviews/MacaquiniJungleVoice_v26'
RATE=44100
for p in (OUT,PREVIEW):p.mkdir(parents=True,exist_ok=True)

def read(path):
    with wave.open(str(path)) as w:
        assert w.getsampwidth()==2 and w.getframerate()==RATE
        return np.frombuffer(w.readframes(w.getnframes()),'<i2').astype(np.float64).reshape(-1,w.getnchannels()).mean(axis=1)/32768

def tone(x,low=105,high=5600):
    n=1<<(len(x)-1).bit_length(); f=np.fft.rfftfreq(n,1/RATE)
    response=(1/(1+(f/high)**6))*(1-1/(1+(f/max(1,low))**4))
    return np.fft.irfft(np.fft.rfft(x,n)*response,n)[:len(x)]

def convolve(a,b):
    n=1<<(len(a)+len(b)-2).bit_length()
    return np.fft.irfft(np.fft.rfft(a,n)*np.fft.rfft(b,n),n)[:len(a)+len(b)-1]

def prepare(name,filename,pitch,tail,peak):
    original=read(SOURCE/'Originals'/filename); original-=original.mean()
    nonzero=np.flatnonzero(np.abs(original)>.018*np.max(np.abs(original)))
    original=original[max(0,nonzero[0]-220):min(len(original),nonzero[-1]+660)]
    dry=np.interp(np.arange(0,len(original)-1,pitch),np.arange(len(original)),original)
    dry=tone(dry); edge=min(700,len(dry)//8);dry[:edge]*=np.linspace(0,1,edge);dry[-edge:]*=np.linspace(1,0,edge)
    count=len(dry)+round(tail*RATE); result=np.zeros((count,2));result[:len(dry)]+=dry[:,None]*.78
    echoes=[(.17,.25,0),(.29,.22,1),(.43,.16,0),(.67,.12,1),(1.03,.07,0),(1.47,.035,1)]
    for index,(delay,gain,side) in enumerate(echoes):
        start=round(delay*RATE)
        echo=tone(dry,150,4600-index*550)
        length=min(len(echo),count-start)
        result[start:start+length,side]+=echo[:length]*gain
        result[start:start+length,1-side]+=echo[:length]*gain*.24
    rng=np.random.default_rng(2600+len(name)); irlen=round(tail*RATE); ir=np.zeros((irlen,2))
    for channel in range(2):
        # Sparse, dark reflections between trees; no cathedral-like sustained pad.
        for _ in range(150):
            t=rng.uniform(.12,tail); pos=round(t*RATE)
            ir[pos,channel]+=rng.uniform(-1,1)*np.exp(-t/.48)*.018
        diffuse=convolve(dry,ir[:,channel]);result[:min(count,len(diffuse)),channel]+=diffuse[:count]
    fade=round(.12*RATE);result[-fade:]*=np.linspace(1,0,fade)[:,None]
    result*=peak/max(.00001,np.max(np.abs(result)))
    write(OUT/name,result)
    dryend=len(dry)/RATE
    stats={'file':name,'duration':count/RATE,'dry_duration':dryend,'tail_seconds':tail,'peak':float(np.max(np.abs(result))),
        'rms':float(np.sqrt(np.mean(result**2))),'stereo_difference_rms':float(np.sqrt(np.mean((result[:,0]-result[:,1])**2))),
        'tail_rms':float(np.sqrt(np.mean(result[len(dry):]**2))),'source':filename,'pitch_ratio':pitch,
        'sha256':hashlib.sha256((OUT/name).read_bytes()).hexdigest()}
    assert np.isfinite(result).all() and stats['peak']<.75 and stats['tail_rms']>.0001 and stats['stereo_difference_rms']>.001
    return result,stats

def write(path,data):
    if data.ndim==1:data=data[:,None]
    with wave.open(str(path),'wb') as w:
        w.setnchannels(data.shape[1]);w.setsampwidth(2);w.setframerate(RATE)
        w.writeframes(np.round(np.clip(data,-1,1)*32767).astype('<i2').tobytes())

records=[]; clips=[]
for spec in [('effort_woods.wav','monkey-1.wav',.96,1.65,.58),('tear_call.wav','monkey-2.wav',.86,1.7,.62),('power_call.wav','monkey-3.wav',.76,1.9,.68)]:
    audio,stats=prepare(*spec);records.append(stats);clips.append(audio)
demo=np.zeros((RATE*11,2))
for start,audio,gain in [(0,clips[0],.44),(1.75,clips[0],.32),(3.25,clips[1],.48),(5.5,clips[2],.62)]:
    pos=round(start*RATE);demo[pos:pos+len(audio)]+=audio*gain
assert np.max(np.abs(demo))<.65
write(PREVIEW/'transformacao-vozes-eco-preview.wav',demo)
manifest={'source_url':'https://opengameart.org/content/monkey-sounds','download':'https://opengameart.org/sites/default/files/monkey-sounds_0.zip',
    'author':'AntumDeluge','license':'CC0 1.0','description':'Human-performed monkey imitations, not wildlife field recordings.',
    'edits':'Trim, DC removal, band limiting, modest pitch lowering, stereo tree reflections and short dark diffusion; no music.',
    'sample_rate':RATE,'channels':2,'clips':records,'voice_mix_preview_peak':float(np.max(np.abs(demo)))}
(SOURCE/'manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print(json.dumps(manifest,indent=2))
