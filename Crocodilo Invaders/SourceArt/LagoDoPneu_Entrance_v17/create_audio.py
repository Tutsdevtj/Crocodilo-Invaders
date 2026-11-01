from pathlib import Path
import math,random,struct,wave,shutil
random.seed(1701);rate=44100
out=Path('SourceArt/LagoDoPneu_Entrance_v17');runtime=Path('Assets/Resources/LakeEntranceV17')
out.mkdir(parents=True,exist_ok=True);runtime.mkdir(parents=True,exist_ok=True)
def save(name,samples):
    peak=max(abs(x) for x in samples) or 1
    data=b''.join(struct.pack('<h',int(max(-1,min(1,x/peak*.65))*32767)) for x in samples)
    with wave.open(str(out/name),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(rate);f.writeframes(data)
    shutil.copyfile(out/name,runtime/name)
water=[];low=0;phase=0
for i in range(int(rate*.95)):
    t=i/rate;low+=.045*(random.uniform(-1,1)-low);phase+=2*math.pi*(105+38*math.sin(t*11))/rate
    envelope=math.sin(math.pi*t/.95)**1.4
    water.append((low*.85+math.sin(phase)*.035*(.5+.5*math.sin(t*41)))*envelope)
save('water-swell.wav',water)
croak=[];phase=0;low=0
for i in range(int(rate*.68)):
    t=i/rate;phase+=2*math.pi*(84-26*t/.68+3*math.sin(t*29))/rate
    low+=.16*(random.uniform(-1,1)-low)
    pulse=.55+.45*math.sin(t*43)**2;env=min(1,t/.035)*max(0,1-t/.68)**.6
    tone=math.sin(phase)*.55+math.sin(phase*2)*.25+math.sin(phase*5)*.11+low*.1
    croak.append(tone*env*pulse)
save('cururu-croak.wav',croak)
print('Created two original mono 44.1kHz PCM16 effects')
