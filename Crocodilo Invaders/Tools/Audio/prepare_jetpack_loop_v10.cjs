const fs=require('node:fs'),path=require('node:path');
const rate=22050,n=rate*2,pcm=Buffer.alloc(n*2),two=Math.PI*2;
for(let i=0;i<n;i++){
 const t=i/rate,flutter=.72+.17*Math.sin(two*13*t)+.11*Math.sin(two*29*t);
 const rumble=Math.sin(two*64*t)*.25+Math.sin(two*128*t)*.13+Math.sin(two*192*t)*.08;
 const hiss=Math.sin(two*977*t)*Math.sin(two*1721*t)*.06+Math.sin(two*2437*t)*.025;
 pcm.writeInt16LE(Math.round((rumble*flutter+hiss)*18000),i*2);
}
const header=Buffer.alloc(44);header.write('RIFF');header.writeUInt32LE(36+pcm.length,4);header.write('WAVEfmt ',8);header.writeUInt32LE(16,16);header.writeUInt16LE(1,20);header.writeUInt16LE(1,22);header.writeUInt32LE(rate,24);header.writeUInt32LE(rate*2,28);header.writeUInt16LE(2,32);header.writeUInt16LE(16,34);header.write('data',36);header.writeUInt32LE(pcm.length,40);
fs.writeFileSync(path.resolve(__dirname,'../../Assets/Audio/Boss/Macaquini/jetpack_loop_v10.wav'),Buffer.concat([header,pcm]));console.log('Seamless two-second motor loop exported.');
