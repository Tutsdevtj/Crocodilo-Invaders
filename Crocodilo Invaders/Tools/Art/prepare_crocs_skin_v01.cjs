// Finish the image_gen hull on the native grid and retain the production pilot cels.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
let sharp;
try { sharp = require('sharp'); }
catch { sharp = require(path.join(process.env.USERPROFILE, '.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp')); }
const root = path.resolve(__dirname, '../..');
const source = path.join(root, 'SourceArt/CrocsSkin_v01');
const preview = path.join(root, 'Docs/ArtPreviews/CrocsSkin_v01');
const palette = ['101018','30391e','47572a','5d6e32','738642','8b9e48','a5b653','c3d66c','e6e6a5','a99761','cbb77c','ebd8a6','fff0ce','42464c','6d7276','b5b9ae'].map(h=>[0,2,4].map(i=>parseInt(h.slice(i,i+2),16)));
const pilot = (x,y) => x <= 130 && ((x >= (y < 51 ? 44 : 47) && y < 59) ||
  (x >= 63 && y < 62) || (x >= 68 && y < 65) || (x >= 75 && y < 69) || (x >= 80 && x <= 123 && y < 71));
const hull = (x,y) => y >= 71 || (x < 49 && y >= 54) || (y >= 67 && x < 75);
const hash = b => crypto.createHash('sha256').update(b).digest('hex');
async function main() {
  fs.mkdirSync(source,{recursive:true}); fs.mkdirSync(preview,{recursive:true});
  const master = await sharp(path.join(source,'generated-master.png')).resize(160,120,{kernel:'nearest'}).ensureAlpha().raw().toBuffer();
  const shoe = Buffer.alloc(160*120*4);
  for(let y=0;y<120;y++)for(let x=0;x<160;x++) {
    const i=(y*160+x)*4;
    if(!hull(x,y)||master[i+3]<160)continue;
    const color=palette.reduce((best,c)=>{const d=c.reduce((v,n,j)=>v+(n-master[i+j])**2,0);return d<best.d?{c,d}:best;},{d:Infinity}).c;
    for(let j=0;j<3;j++)shoe[i+j]=color[j]; shoe[i+3]=255;
  }
  // Rear engine remains at the heel; front port aligns to the existing release origin.
  for(let y=79;y<89;y++)for(let x=123;x<131;x++)shoe.fill(0,(y*160+x)*4,(y*160+x)*4+4);
  for(let y=0;y<7;y++)for(let x=0;x<6;x++) {
    const i=((76+y)*160+124+x)*4;
    const border=y==0||y==6||x==0||x==5;
    const c=palette[border?0:x<3?14:13];
    for(let j=0;j<3;j++)shoe[i+j]=c[j];shoe[i+3]=255;
  }
  await sharp(shoe,{raw:{width:160,height:120,channels:4}}).png().toFile(path.join(source,'shoe-native.png'));
  const manifest={version:1,mode:'built-in image_gen edit + native-grid finishing',canvas:[160,120],ppu:100,pivot:[.45,1/3],frames:[]};
  const layers=[];
  for(const state of ['Idle','Fly','Shoot','Special','Hurt']) {
    const originalDir=path.join(root,'Assets/Art/Player/Bombardiro',state);
    const outputDir=path.join(root,'Assets/Art/Player/Crocs',state);fs.mkdirSync(outputDir,{recursive:true});
    for(const name of fs.readdirSync(originalDir).filter(n=>n.endsWith('.png')).sort()) {
      const input=fs.readFileSync(path.join(originalDir,name));
      const original=await sharp(input).ensureAlpha().raw().toBuffer();
      let top=120;
      for(let y=0;y<40;y++)for(let x=44;x<130;x++)if(original[(y*160+x)*4+3]>127)top=Math.min(top,y);
      const dy=top-12, result=Buffer.alloc(shoe.length);
      for(let y=0;y<120;y++)for(let x=0;x<160;x++) {
        const i=(y*160+x)*4, sy=y-dy;
        if(sy>=0&&sy<120)shoe.copy(result,i,(sy*160+x)*4,(sy*160+x)*4+4);
        // Exact original pilot pixels, including blink/recoil/hurt expressions.
        // Keep original muzzle flashes and charging effects beyond the hull.
        if((pilot(x,sy)||x>=132)&&original[i+3]>0)original.copy(result,i,i,i+4);
      }
      const png=await sharp(result,{raw:{width:160,height:120,channels:4}}).png().toBuffer();
      fs.writeFileSync(path.join(outputDir,name),png);
      const n=manifest.frames.length;
      layers.push({input:await sharp(png).resize(480,360,{kernel:'nearest'}).toBuffer(),left:(n%6)*480,top:Math.floor(n/6)*360});
      manifest.frames.push({state,name,originalSha256:hash(input),sha256:hash(png),offsetY:dy});
      if(state==='Idle'&&name.endsWith('_0.png'))await sharp(png).resize(960,720,{kernel:'nearest'}).toFile(path.join(preview,'crocs-idle-x6.png'));
    }
  }
  if(manifest.frames.length!==26)throw new Error('Expected 26 production frames');
  fs.writeFileSync(path.join(source,'manifest.json'),JSON.stringify(manifest,null,2)+'\n');
  await sharp({create:{width:2880,height:1800,channels:4,background:'#253044'}}).composite(layers).png().toFile(path.join(preview,'crocs-frames.png'));
  console.log('Exported 26 skin frames; original pilot pixels, states and effects retained.');
}
main().catch(e=>{console.error(e);process.exitCode=1;});
