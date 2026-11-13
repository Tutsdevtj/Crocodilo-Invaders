// SUPERSEDED by prepare_crocs_skin_v02.cjs (jet wings + Crocs helmet). Running v01 restores the v01 look;
// v02 reads SourceArt/CrocsSkin_v01/shoe-native.png produced here.
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
// v01.1: the pilot and hull masks leave an enclosed gap between neck and shoe opening.
// Fill only enclosed transparent pixels with the croc body sinking into the shoe.
function fillNeckHole(px) {
  const W=160,H=120,outside=new Uint8Array(W*H),stack=[];
  const opaque=i=>px[i*4+3]>0;
  for(let x=0;x<W;x++)stack.push(x,(H-1)*W+x);
  for(let y=0;y<H;y++)stack.push(y*W,y*W+W-1);
  while(stack.length){const i=stack.pop();if(outside[i]||opaque(i))continue;outside[i]=1;const x=i%W,y=(i/W)|0;
    if(x>0)stack.push(i-1);if(x<W-1)stack.push(i+1);if(y>0)stack.push(i-W);if(y<H-1)stack.push(i+W);}
  const hole=[];let top=H;
  for(let i=0;i<W*H;i++)if(!opaque(i)&&!outside[i]){hole.push(i);top=Math.min(top,(i/W)|0);}
  const base=palette[2],light=palette[3],shade=palette[1];
  const isHole=new Set(hole);
  for(const i of hole){
    const x=i%W,y=(i/W)|0,ry=y-top;
    const below=i+W<W*H&&!isHole.has(i+W), above=!isHole.has(i-W);
    // Shadow under the jaw and where the body meets the shoe rim; soft camo blobs elsewhere.
    let c=base;
    if(below||ry>=9)c=shade;
    else if(above&&x>=62)c=shade;
    else if(((Math.floor(x/3)+Math.floor(ry/2)*2)%5)===0)c=light;
    for(let j=0;j<3;j++)px[i*4+j]=c[j];px[i*4+3]=255;
  }
  return hole.length;
}
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
  // Rear engine remains at the heel; the toe stays clean (no front port block).
  for(let y=79;y<89;y++)for(let x=123;x<131;x++)shoe.fill(0,(y*160+x)*4,(y*160+x)*4+4);
  // v01.2: the floating gray front port block was removed at the user's request.
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
        // v01.3: the airplane's muzzle flashes are not copied in front of the shoe (user request).
        if(pilot(x,sy)&&original[i+3]>0)original.copy(result,i,i,i+4);
      }
      const filled=fillNeckHole(result);
      const png=await sharp(result,{raw:{width:160,height:120,channels:4}}).png().toBuffer();
      fs.writeFileSync(path.join(outputDir,name),png);
      const n=manifest.frames.length;
      layers.push({input:await sharp(png).resize(480,360,{kernel:'nearest'}).toBuffer(),left:(n%6)*480,top:Math.floor(n/6)*360});
      manifest.frames.push({state,name,originalSha256:hash(input),sha256:hash(png),offsetY:dy,filledNeckPixels:filled});
      if(state==='Idle'&&name.endsWith('_0.png'))await sharp(png).resize(960,720,{kernel:'nearest'}).toFile(path.join(preview,'crocs-idle-x6.png'));
    }
  }
  if(manifest.frames.length!==26)throw new Error('Expected 26 production frames');
  fs.writeFileSync(path.join(source,'manifest.json'),JSON.stringify(manifest,null,2)+'\n');
  await sharp({create:{width:2880,height:1800,channels:4,background:'#253044'}}).composite(layers).png().toFile(path.join(preview,'crocs-frames.png'));
  console.log('Exported 26 skin frames; original pilot pixels, states and effects retained.');
}
main().catch(e=>{console.error(e);process.exitCode=1;});
