const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const sharp=require('C:/Users/berna/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const palette=require('./pixel_palette.cjs');
const root=path.resolve(__dirname,'../..'),src=path.join(root,'SourceArt/Macaquini_v04'),out=path.join(root,'Assets/Art/JungleStage/Macaquini_v04');
const inside=(x,y,p)=>{let v=false;for(let i=0,j=p.length-1;i<p.length;j=i++){const a=p[i],b=p[j];if((a[1]>y)!=(b[1]>y)&&x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0])v=!v;}return v;};
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
async function read(p){return sharp(p).ensureAlpha().raw().toBuffer({resolveWithObject:true});}
async function write(name,data,w,h){for(let i=3;i<data.length;i+=4)data[i]=data[i]>127?255:0;const file=path.join(out,name+'.png');await sharp(data,{raw:{width:w,height:h,channels:4}}).png().toFile(file);const colors=new Set(),alpha=new Set();for(let i=0;i<data.length;i+=4){alpha.add(data[i+3]);if(data[i+3])colors.add(data.subarray(i,i+3).toString('hex'));}return {name,width:w,height:h,colors:colors.size,alpha:[...alpha],sha256:sha(file)};}
(async()=>{
 fs.mkdirSync(out,{recursive:true});const sprites=[];
 const original=await read(path.join(root,'SourceArt/BananaPossessora_Fase1_Master_v09_Modular/master.png'));
 const parts=[
  {id:'gun_upper',pivot:[99,106],end:[56,145],poly:[[87,96],[104,97],[112,111],[95,132],[67,153],[51,151],[46,138],[69,114]]},
  {id:'gun_lower',pivot:[56,145],end:[28,174],poly:[[47,136],[68,143],[58,159],[40,177],[24,181],[17,174],[25,156]]},
  {id:'support_upper',pivot:[154,105],end:[181,139],poly:[[143,95],[163,93],[177,108],[194,134],[190,149],[176,149],[159,124]]},
  {id:'support_lower',pivot:[181,139],end:[185,174],poly:[[174,130],[191,132],[199,158],[197,176],[179,181],[173,161]]}
 ];
 for(const p of parts){const mask=Buffer.alloc(original.data.length);for(let y=0;y<272;y++)for(let x=0;x<256;x++)if(inside(x,y,p.poly))original.data.copy(mask,(y*256+x)*4,(y*256+x)*4,(y*256+x)*4+4);sprites.push({name:p.id,data:mask,w:256,h:272});}
 const core=await read(path.join(src,'core_source.png'));
 let minX=core.info.width,minY=core.info.height,maxY=0;for(let y=0;y<core.info.height;y++)for(let x=0;x<core.info.width;x++)if(core.data[(y*core.info.width+x)*4+3]>127){minX=Math.min(minX,x);minY=Math.min(minY,y);maxY=Math.max(maxY,y);}
 const scale=250/(maxY-minY+1),offsetX=30-minX*scale,offsetY=8-minY*scale;
 const normalized=[];
 for(const name of ['core','halfcore']){
  const s=name==='core'?core:await read(path.join(src,name+'_source.png')),b=Buffer.alloc(256*272*4);
  for(let y=0;y<272;y++)for(let x=0;x<256;x++){const sx=Math.floor((x-offsetX)/scale*s.info.width/core.info.width),sy=Math.floor((y-offsetY)/scale*s.info.height/core.info.height);if(sx>=0&&sx<s.info.width&&sy>=0&&sy<s.info.height){const q=(sy*s.info.width+sx)*4;s.data.copy(b,(y*256+x)*4,q,q+4);}}
  normalized.push(b);sprites.push({name,data:b,w:256,h:272});
 }
 const handSource=await read(path.join(src,'hands_source.png'));
 const cells=[{x:0,w:530,wrist:300},{x:570,w:530,wrist:270},{x:1160,w:460,wrist:245},{x:1690,w:482,wrist:230}];
 for(let i=0;i<4;i++){
  const b=Buffer.alloc(96*96*4),c=cells[i],s=.055;
  for(let y=0;y<96;y++)for(let x=0;x<96;x++){const sx=Math.floor((x-48)/s+c.wrist+c.x),sy=Math.floor((y-56)/s+560);if(sx>=c.x&&sx<Math.min(c.x+c.w,handSource.info.width)&&sy>=0&&sy<handSource.info.height){const q=(sy*handSource.info.width+sx)*4;handSource.data.copy(b,(y*96+x)*4,q,q+4);}}
  sprites.push({name:'grip_'+i,data:b,w:96,h:96});
 }
 for(const s of sprites)for(let i=3;i<s.data.length;i+=4)s.data[i]=s.data[i]>127?255:0;
 const all=Buffer.concat(sprites.map(s=>s.data));const colors=palette(all,24);let cursor=0;const manifest={version:4,identity:'Actual monkey enclosed by banana; upper half tears off in second form; v03 werewolf rejected.',body_pivot:[128,148],parts,exports:[],shared_palette:colors};
 for(const s of sprites){const length=s.data.length;s.data=all.subarray(cursor,cursor+length);cursor+=length;manifest.exports.push(await write(s.name,s.data,s.w,s.h));}
 // The original medallion frame is kept; only the inner character silhouette changes.
 const med=await read(path.join(root,'Assets/Art/UI/GusiniBossBarF1/Parts/emblem_roundel.png'));
 const face=await sharp(sprites.find(s=>s.name==='core').data,{raw:{width:256,height:272,channels:4}}).extract({left:95,top:38,width:56,height:63}).resize(28,32,{kernel:'nearest'}).raw().toBuffer();
 for(let y=0;y<50;y++)for(let x=0;x<50;x++)if((x-25)**2+(y-25)**2<17**2){const o=(y*50+x)*4;med.data[o]=52;med.data[o+1]=30;med.data[o+2]=82;med.data[o+3]=255;const fx=x-11,fy=y-8;if(fx>=0&&fx<28&&fy>=0&&fy<32){const q=(fy*28+fx)*4;if(face[q+3]>127)face.copy(med.data,o,q,q+4);}}
 manifest.exports.push(await write('hud_portrait',med.data,50,50));
 const halfMed=await read(path.join(root,'Assets/Art/UI/GusiniBossBarF1/Parts/emblem_roundel.png'));
 const halfFace=await sharp(sprites.find(s=>s.name==='halfcore').data,{raw:{width:256,height:272,channels:4}}).extract({left:89,top:31,width:60,height:64}).resize(30,32,{kernel:'nearest'}).raw().toBuffer();
 for(let y=0;y<50;y++)for(let x=0;x<50;x++)if((x-25)**2+(y-25)**2<17**2){const o=(y*50+x)*4;halfMed.data[o]=52;halfMed.data[o+1]=30;halfMed.data[o+2]=82;halfMed.data[o+3]=255;const fx=x-10,fy=y-8;if(fx>=0&&fx<30&&fy>=0&&fy<32){const q=(fy*30+fx)*4;if(halfFace[q+3]>127)halfFace.copy(halfMed.data,o,q,q+4);}}
 manifest.exports.push(await write('hud_portrait_half',halfMed.data,50,50));
 fs.copyFileSync(path.join(src,'hud_title_source.png'),path.join(out,'hud_title.png'));
 const transform=JSON.parse(fs.readFileSync(path.join(root,'SourceArt/BananaPossessora_Transformacao_v03_PixelArt/animation.json'),'utf8'));
 for(const frame of transform.frames)fs.copyFileSync(path.join(root,'SourceArt/BananaPossessora_Transformacao_v03_PixelArt',frame.file),path.join(out,path.basename(frame.file)));
 fs.writeFileSync(path.join(src,'layout.json'),JSON.stringify(manifest,null,2)+'\n');console.log(JSON.stringify(manifest.exports));
})().catch(e=>{console.error(e);process.exitCode=1;});
