// Original achievement sting: short mechanical click and three rising bell notes.
const fs=require('node:fs'),path=require('node:path');
const root=path.resolve(__dirname,'../..');
const rate=44100,duration=.64,count=Math.ceil(rate*duration),samples=new Float64Array(count);
let seed=47219;
function noise(){seed=(Math.imul(seed,1664525)+1013904223)>>>0;return seed/2147483648-1;}
for(let i=0;i<count;i++){
 const t=i/rate;
 let s=t<.035?noise()*.11*Math.exp(-t*170):0;
 for(const [start,freq] of [[.025,783.991],[.145,987.767],[.27,1174.659]]){
  const q=t-start;if(q<0)continue;
  const env=Math.min(1,q/.004)*Math.exp(-q*14)*Math.min(1,(duration-t)/.035);
  s+=env*(Math.sin(2*Math.PI*freq*q)*.20+Math.sin(2*Math.PI*freq*2*q)*.055+Math.sin(2*Math.PI*freq*3.99*q)*.018);
 }samples[i]=s;
}
const peak=Math.max(...samples.map(Math.abs));const gain=.48/peak;
const wav=Buffer.alloc(44+count*2);wav.write('RIFF');wav.writeUInt32LE(36+count*2,4);wav.write('WAVEfmt ',8);
wav.writeUInt32LE(16,16);wav.writeUInt16LE(1,20);wav.writeUInt16LE(1,22);wav.writeUInt32LE(rate,24);wav.writeUInt32LE(rate*2,28);wav.writeUInt16LE(2,32);wav.writeUInt16LE(16,34);wav.write('data',36);wav.writeUInt32LE(count*2,40);
for(let i=0;i<count;i++)wav.writeInt16LE(Math.round(samples[i]*gain*32767),44+i*2);
const file=path.join(root,'Assets/Resources/Achievements/achievement_unlock.wav');fs.mkdirSync(path.dirname(file),{recursive:true});fs.writeFileSync(file,wav);
const source=path.join(root,'SourceArt/Achievements_v01');fs.mkdirSync(source,{recursive:true});fs.writeFileSync(path.join(source,'audio.json'),JSON.stringify({recipe:'Tools/Audio/generate_achievement_chime.cjs',original:true,sampleRate:rate,channels:1,duration,peak:.48,notesHz:[783.991,987.767,1174.659]},null,2)+'\n');
console.log('ACHIEVEMENT_SOUND_OK mono PCM16 44100 Hz, 0.64 s, peak 0.48');
