const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),out=path.join(root,'Assets/Audio/Boss/Macaquini');fs.mkdirSync(out,{recursive:true});
let seed=2159;const noise=()=>{seed=(Math.imul(seed,1664525)+1013904223)>>>0;return seed/2147483648-1;};
const specs=[['launch',.3],['charge',.64],['whip',.24],['growl',.56],['hurt',.13]];
const manifest={provenance:'Original deterministic PCM synthesis; no external samples, music or speech.',clips:[]};
for(const [name,duration] of specs){
 const rate=44100,count=Math.floor(duration*rate),samples=new Float64Array(count);let phase=0,filtered=0,peak=0;
 for(let i=0;i<count;i++){
  const t=i/rate,u=t/duration,n=noise();filtered=filtered*.82+n*.18;let value=0;
  const edge=Math.min(1,t/.008)*Math.min(1,(duration-t)/.024);
  if(name==='launch'){phase+=Math.PI*2*(45+160*Math.exp(-t*20))/rate;value=(.7*Math.sin(phase)*Math.exp(-t*12)+filtered*.9*Math.exp(-t*17))*edge;}
  if(name==='charge'){phase+=Math.PI*2*(110+210*u*u)/rate;value=(Math.sin(phase+Math.sin(t*71)*.7)*.28+filtered*.6)*Math.sin(Math.PI*u)*edge;}
  if(name==='whip')value=(n*.55+filtered*.8)*Math.exp(-t*21)*edge;
  if(name==='growl'){phase+=Math.PI*2*(64-28*u)/rate;value=(Math.tanh(Math.sin(phase)*2)*.46+filtered*.9)*(.6+.4*Math.sin(t*92))*Math.sin(Math.PI*u)*edge;}
  if(name==='hurt'){phase+=Math.PI*2*(240-170*u)/rate;value=(Math.sin(phase)*.5+n*.15)*Math.exp(-t*25)*edge;}
  samples[i]=value;peak=Math.max(peak,Math.abs(value));
 }
 const bytes=Buffer.alloc(44+count*2);bytes.write('RIFF');bytes.writeUInt32LE(bytes.length-8,4);bytes.write('WAVEfmt ',8);bytes.writeUInt32LE(16,16);bytes.writeUInt16LE(1,20);bytes.writeUInt16LE(1,22);bytes.writeUInt32LE(rate,24);bytes.writeUInt32LE(rate*2,28);bytes.writeUInt16LE(2,32);bytes.writeUInt16LE(16,34);bytes.write('data',36);bytes.writeUInt32LE(count*2,40);
 for(let i=0;i<count;i++)bytes.writeInt16LE(Math.round(samples[i]/Math.max(peak,.001)*.68*32767),44+i*2);
 fs.writeFileSync(path.join(out,name+'.wav'),bytes);manifest.clips.push({name,duration,sample_rate:rate,peak:.68,sha256:crypto.createHash('sha256').update(bytes).digest('hex')});
}
fs.mkdirSync(path.join(root,'SourceArt/JungleStage_v01'),{recursive:true});fs.writeFileSync(path.join(root,'SourceArt/JungleStage_v01/audio_manifest.json'),JSON.stringify(manifest,null,2)+'\n');console.log('Five original Macaquini effects exported');
