const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
let sharp;try{sharp=require('sharp')}catch{sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'))}
const project=path.resolve(__dirname,'../..'),root=path.join(project,'SourceArt/BananaPossessora_Transformacao_v03_PixelArt'),preview=path.join(project,'Docs/ArtPreviews/BananaPossessora_Transformacao_v03_PixelArt');
const base=JSON.parse(fs.readFileSync(path.join(project,'SourceArt/BananaPossessora_Transformacao_v02_24Frames/animation.json'),'utf8'));
const hex=['120e0c','322019','552b20','853b2b','b75335','d97949','ab5e40','ee976d','ffbd94','80540d','be870b','edbd0d','ffe747','fff596','217818','4cda2b','b6ff73','b2383d','e97169','fff1ca'];
const palette=hex.map(h=>[0,2,4].map(i=>parseInt(h.slice(i,i+2),16))),cache=new Map();
function nearest(r,g,b){const key=(r<<16)|(g<<8)|b;if(cache.has(key))return cache.get(key);let best=0,distance=Infinity;palette.forEach((p,i)=>{const d=(r-p[0])**2*2+(g-p[1])**2*3+(b-p[2])**2;if(d<distance){best=i;distance=d}});cache.set(key,palette[best]);return palette[best]}
async function main(){
 fs.mkdirSync(path.join(root,'frames'),{recursive:true});fs.mkdirSync(preview,{recursive:true});
 const buffers=[],frames=[],qa=[],used=new Set();
 for(let i=0;i<24;i++){
  const sheet=Math.floor(i/6)+1,cell=i%6,meta=await sharp(path.join(root,`sources/segmento_${String(sheet).padStart(2,'0')}.png`)).metadata();
  if(meta.width!==1536||meta.height!==1024)throw Error('Unexpected sheet dimensions');
  const x=(cell%3)*512,y=Math.floor(cell/3)*512,over=y?12:0;
  const {data,info}=await sharp(path.join(root,`sources/segmento_${String(sheet).padStart(2,'0')}.png`)).extract({left:x,top:y-over,width:Math.min(532,1536-x),height:512+over}).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  // Remove small fragments touching crop boundaries from neighboring cells.
  const seen=new Uint8Array(info.width*info.height),components=[];
  for(let k=0;k<seen.length;k++){if(seen[k]||data[k*4+3]<128)continue;const pixels=[k];seen[k]=1;let edge=false;
   for(let q=0;q<pixels.length;q++){const p=pixels[q],xx=p%info.width,yy=Math.floor(p/info.width);if(xx===0||yy===0||xx===info.width-1||yy===info.height-1)edge=true;for(const n of [xx>0?p-1:-1,xx<info.width-1?p+1:-1,yy>0?p-info.width:-1,yy<info.height-1?p+info.width:-1])if(n>=0&&!seen[n]&&data[n*4+3]>=128){seen[n]=1;pixels.push(n)}}components.push({pixels,edge});
  }
  components.sort((a,b)=>b.pixels.length-a.pixels.length);for(const c of components.slice(1))if(c.edge&&c.pixels.length<2500)for(const k of c.pixels)data[k*4+3]=0;
  let left=info.width,right=-1,top=info.height,bottom=-1,fl=info.width,fr=-1;
  for(let yy=0;yy<info.height;yy++)for(let xx=0;xx<info.width;xx++){const p=(yy*info.width+xx)*4;if(data[p+3]<128){data[p]=data[p+1]=data[p+2]=data[p+3]=0;continue}data[p+3]=255;left=Math.min(left,xx);right=Math.max(right,xx);top=Math.min(top,yy);bottom=Math.max(bottom,yy);if(yy>=info.height-100&&data[p]>140&&data[p+1]>70&&data[p]>data[p+1]*1.15&&data[p+1]>data[p+2]*1.12){fl=Math.min(fl,xx);fr=Math.max(fr,xx)}}
  if(fr<0)throw Error('Missing foot anchor');
  const dx=Math.round(320-(fl+fr)/2),dy=600-bottom;
  if(dx+left<0||dy+top<0||dx+right>=640||dy+bottom>=640)throw Error('Sprite outside export canvas');
  const part=await sharp(data,{raw:{width:info.width,height:info.height,channels:4}}).extract({left,top,width:right-left+1,height:bottom-top+1}).png().toBuffer();
  const registered=await sharp({create:{width:640,height:640,channels:4,background:'#00000000'}}).composite([{input:part,left:dx+left,top:dy+top}]).png().toBuffer();
  const native=await sharp(registered).resize(128,128,{kernel:'nearest'}).ensureAlpha().raw().toBuffer();
  for(let p=0;p<native.length;p+=4){if(native[p+3]<128){native[p]=native[p+1]=native[p+2]=native[p+3]=0;continue}const c=nearest(native[p],native[p+1],native[p+2]);native[p]=c[0];native[p+1]=c[1];native[p+2]=c[2];native[p+3]=255;used.add(c.join(','))}
  const file=`frames/transformacao_${String(i).padStart(2,'0')}.png`,png=await sharp(native,{raw:{width:128,height:128,channels:4}}).png().toBuffer();fs.writeFileSync(path.join(root,file),png);buffers.push(native);
  frames.push({...base.frames[i],file,pivot_px:[64,120]});qa.push({index:i,sha256:crypto.createHash('sha256').update(png).digest('hex'),width:128,height:128});
 }
 if(new Set(qa.map(f=>f.sha256)).size!==24)throw Error('Duplicate animation frames');
 const spritesheet=await sharp({create:{width:1024,height:384,channels:4,background:'#00000000'}}).composite(frames.map((f,i)=>({input:path.join(root,f.file),left:(i%8)*128,top:Math.floor(i/8)*128}))).png().toBuffer();fs.writeFileSync(path.join(root,'spritesheet_8x3.png'),spritesheet);
 await sharp(spritesheet).resize(3072,1152,{kernel:'nearest'}).png().toFile(path.join(preview,'contact_sheet_x3.png'));
 const delay=frames.map(f=>f.duration_ms),raw=Buffer.concat(buffers);
 const anim=()=>sharp(raw,{raw:{width:128,height:128*24,channels:4,pageHeight:128}});
 await anim().gif({loop:0,delay,colours:32,dither:0,effort:7}).toFile(path.join(preview,'transformacao_native128.gif'));
 const bigPages=await Promise.all(buffers.map(b=>sharp(b,{raw:{width:128,height:128,channels:4}}).resize(512,512,{kernel:'nearest'}).raw().toBuffer()));
 await sharp(Buffer.concat(bigPages),{raw:{width:512,height:512*24,channels:4,pageHeight:512}}).gif({loop:0,delay,colours:32,dither:0,effort:7}).toFile(path.join(preview,'transformacao_pixelart_x4.gif'));
 const manifest={...base,version:3,canvas:[128,128],pixels_per_unit:40,root_pivot:[0.5,0.0625],palette:{file:'palette.json',opaque_colors:palette.length},spritesheet:{file:'spritesheet_8x3.png',columns:8,rows:3,cell:[128,128],order:'left_to_right_top_to_bottom'},frames};
 fs.writeFileSync(path.join(root,'animation.json'),JSON.stringify(manifest,null,2));fs.writeFileSync(path.join(root,'palette.json'),JSON.stringify({transparent:true,opaque_colors:hex.map(h=>'#'+h)},null,2));
 const inline=frames.map(f=>({...f,src:'data:image/png;base64,'+fs.readFileSync(path.join(root,f.file)).toString('base64')}));
 let html=fs.readFileSync(path.join(__dirname,'banana_transformacao_v02_preview.html'),'utf8').replace('__FRAMES__',JSON.stringify(inline)).replace('24 desenhos ·','Pixel art nativa 128×128 · 20 cores · 24 desenhos ·').replace('max-width:1150px','max-width:1050px').replace('minmax(320px,640px)','512px').replace('width:100%;image-rendering:pixelated','width:512px;max-width:100%;image-rendering:pixelated');
 new(require('node:vm').Script)(html.match(/<script>([\s\S]*?)<\/script>/)[1]);fs.writeFileSync(path.join(preview,'preview.html'),html);
 const gif=await sharp(path.join(preview,'transformacao_pixelart_x4.gif'),{animated:true}).metadata();if(gif.pages!==24)throw Error('Incorrect GIF frame count');
 const result={frames:24,distinct_frames:24,native_canvas:[128,128],palette_limit:20,actual_opaque_colors:used.size,binary_alpha:true,preview_integer_scale:4,gif_frames:gif.pages,duration_ms:base.duration_ms,frames_sha256:qa};fs.writeFileSync(path.join(root,'qa_report.json'),JSON.stringify(result,null,2));console.log(JSON.stringify({...result,frames_sha256:undefined}));
}
main().catch(e=>{console.error(e);process.exitCode=1});
