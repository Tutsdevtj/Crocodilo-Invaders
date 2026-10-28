// Versioned image_gen sources, deterministic point exports and shared closed palettes.
const fs=require('node:fs'), path=require('node:path'), crypto=require('node:crypto');
const sharp=require('C:/Users/berna/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=path.resolve(__dirname,'../..'), src=path.join(root,'SourceArt/JungleMotion_v02'), out=path.join(root,'Assets/Art/JungleStage/Motion_v02');
const preview=path.join(root,'Docs/ArtPreviews/JungleMotion_v02');
function palette(raw,limit=24){
 const h=new Map();for(let i=0;i<raw.length;i+=4)if(raw[i+3]){const k=raw.subarray(i,i+3).toString('hex');const e=h.get(k);if(e)e.n++;else h.set(k,{rgb:[...raw.subarray(i,i+3)],n:1});}
 const boxes=[[...h.values()]];
 while(boxes.length<limit){let choice=-1,channel=0,score=-1;boxes.forEach((b,j)=>{if(b.length<2)return;for(let c=0;c<3;c++){let lo=255,hi=0,total=0;for(const e of b){lo=Math.min(lo,e.rgb[c]);hi=Math.max(hi,e.rgb[c]);total+=e.n;}const s=(hi-lo)*Math.sqrt(total);if(s>score){score=s;choice=j;channel=c;}}});if(choice<0)break;const b=boxes.splice(choice,1)[0].sort((a,b)=>a.rgb[channel]-b.rgb[channel]);const half=b.reduce((s,e)=>s+e.n,0)/2;let sum=0,split=1;for(let i=0;i<b.length-1;i++){sum+=b[i].n;if(sum>=half){split=i+1;break;}}boxes.push(b.slice(0,split),b.slice(split));}
 const colors=boxes.map(b=>{const n=b.reduce((s,e)=>s+e.n,0);return [0,1,2].map(c=>Math.round(b.reduce((s,e)=>s+e.n*e.rgb[c],0)/n));});const map=new Map();for(const [k,e]of h){let best=colors[0],score=Infinity;for(const p of colors){const d=p.reduce((s,v,c)=>s+(v-e.rgb[c])**2,0);if(d<score){score=d;best=p;}}map.set(k,best);}for(let i=0;i<raw.length;i+=4)if(raw[i+3]){const p=map.get(raw.subarray(i,i+3).toString('hex'));for(let c=0;c<3;c++)raw[i+c]=p[c];}return colors;
}
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
async function write(name,data,w,h){const file=path.join(out,name+'.png');await sharp(data,{raw:{width:w,height:h,channels:4}}).png().toFile(file);return {file:'Assets/Art/JungleStage/Motion_v02/'+name+'.png',width:w,height:h,sha256:hash(file)};}
(async()=>{
 for(const d of [src,out,preview])fs.mkdirSync(d,{recursive:true});
 const sources=JSON.parse(fs.readFileSync(path.join(src,'sources.json'),'utf8'));const manifest={version:2,tool:'built-in image_gen',art:[],sources:[]};
 for(const [name,file]of Object.entries(sources)){const dest=path.join(src,name+'_source.png');if(!fs.existsSync(dest))fs.copyFileSync(file,dest);manifest.sources.push({name,sha256:hash(dest)});}
 for(const name of ['deep','far','trunks','canopy']){
  const r=await sharp(path.join(src,name+'_source.png')).resize(480,320,{kernel:'nearest'}).ensureAlpha().raw().toBuffer();
  for(let i=3;i<r.length;i+=4)r[i]=name==='deep'?255:r[i]>127?255:0;
  palette(r);
  if(name==='canopy'){
   const top=Buffer.from(r),bottom=Buffer.from(r);for(let y=0;y<320;y++)for(let x=0;x<480;x++){if(y>210)top[(y*480+x)*4+3]=0;if(y<210)bottom[(y*480+x)*4+3]=0;}
   manifest.art.push(await write('canopy',top,480,320),await write('ferns',bottom,480,320));
  }else manifest.art.push(await write(name,r,480,320));
 }
 // The action sheet is exported after its measured crop specification is reviewed.
 const specPath=path.join(src,'poses.json');
 if(fs.existsSync(specPath)){
  const spec=JSON.parse(fs.readFileSync(specPath,'utf8')), buffers=[];
  for(const f of spec.frames){
   const resized=await sharp(path.join(src,'poses_source.png')).extract(f.rect).resize(Math.round(f.rect.width*.4),Math.round(f.rect.height*.4),{kernel:'nearest'}).png().toBuffer();
   const left=Math.round(96-(f.body[0]-f.rect.left)*.4),top=Math.round(90-(f.body[1]-f.rect.top)*.4);
   const r=await sharp({create:{width:f.width,height:f.height,channels:4,background:'#00000000'}}).composite([{input:resized,left,top}]).raw().toBuffer();for(let i=3;i<r.length;i+=4)r[i]=r[i]>127?255:0;
   const seen=new Set();for(let p=0;p<f.width*f.height;p++){if(seen.has(p)||!r[p*4+3])continue;const island=[p];seen.add(p);for(let j=0;j<island.length;j++){const q=island[j],x=q%f.width,y=Math.floor(q/f.width);for(const n of [x>0?q-1:-1,x<f.width-1?q+1:-1,y>0?q-f.width:-1,y<f.height-1?q+f.width:-1])if(n>=0&&!seen.has(n)&&r[n*4+3]){seen.add(n);island.push(n);}}if(island.length<8)for(const q of island)r[q*4+3]=0;}
   buffers.push(r);
  }
  const all=Buffer.concat(buffers);const colors=palette(all);let offset=0;
  for(let i=0;i<spec.frames.length;i++){const f=spec.frames[i],length=f.width*f.height*4;manifest.art.push(await write('pose_'+String(i).padStart(2,'0'),all.subarray(offset,offset+length),f.width,f.height));offset+=length;}
  manifest.pose_palette=colors;
 }
 for(const f of manifest.art){const r=await sharp(path.join(root,f.file)).ensureAlpha().raw().toBuffer();const colors=new Set(),alpha=new Set();for(let i=0;i<r.length;i+=4){alpha.add(r[i+3]);if(r[i+3])colors.add(r.subarray(i,i+3).toString('hex'));}if(colors.size>24||[...alpha].some(x=>x!==0&&x!==255))throw Error('Invalid pixel export '+f.file);f.colors=colors.size;f.alpha=[...alpha];}
 fs.writeFileSync(path.join(src,'manifest.json'),JSON.stringify(manifest,null,2)+'\n');console.log(JSON.stringify(manifest.art));
})().catch(e=>{console.error(e);process.exitCode=1;});
