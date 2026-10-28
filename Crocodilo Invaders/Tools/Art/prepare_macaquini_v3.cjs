const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const sharp=require('C:/Users/berna/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const palette=require('./pixel_palette.cjs');
const root=path.resolve(__dirname,'../..'),src=path.join(root,'SourceArt/Macaquini_v03'),out=path.join(root,'Assets/Art/JungleStage/Macaquini_v03');
const inside=(x,y,p)=>{let yes=false;for(let i=0,j=p.length-1;i<p.length;j=i++){const a=p[i],b=p[j];if((a[1]>y)!=(b[1]>y)&&x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0])yes=!yes;}return yes;};
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
async function save(name,raw,w,h){for(let i=3;i<raw.length;i+=4)raw[i]=raw[i]>127?255:0;palette(raw,32);const file=path.join(out,name+'.png');await sharp(raw,{raw:{width:w,height:h,channels:4}}).png().toFile(file);return {file:name,width:w,height:h,sha256:hash(file)};}
(async()=>{
 fs.mkdirSync(out,{recursive:true});
 const raw=await sharp(path.join(src,'master_source.png')).resize(320,384,{kernel:'nearest'}).ensureAlpha().raw().toBuffer();
 const parts=[
  {id:'gun_upper',pivot:[108,146],end:[79,196],poly:[[93,131],[118,138],[122,158],[100,193],[81,207],[64,195],[78,169]]},
  {id:'gun_lower',pivot:[79,196],end:[36,275],poly:[[66,181],[100,193],[80,231],[51,277],[29,284],[22,271],[41,237]]},
  {id:'support_upper',pivot:[207,138],end:[242,196],poly:[[190,126],[215,119],[237,138],[254,178],[258,198],[240,212],[222,197],[208,174]]},
  {id:'support_lower',pivot:[242,196],end:[265,281],poly:[[228,184],[253,181],[266,214],[277,256],[276,283],[261,291],[248,275],[246,244],[234,218]]}
 ];
 const manifest={version:3,parts:[],exports:[],source_sha256:hash(path.join(src,'master_source.png')),body_pivot:[160,208]};
 for(const p of parts){const masked=Buffer.alloc(raw.length);for(let y=0;y<384;y++)for(let x=0;x<320;x++)if(inside(x,y,p.poly))raw.copy(masked,(y*320+x)*4,(y*320+x)*4,(y*320+x)*4+4);manifest.exports.push(await save(p.id,masked,320,384));manifest.parts.push(p);}
 const core=await sharp(path.join(src,'core_source.png')).resize(320,384,{kernel:'nearest'}).ensureAlpha().raw().toBuffer();
 manifest.exports.push(await save('core',core,320,384));
 // Four measured wrist anchors, not uniformly spaced guessed cells.
 const hands=[{rect:{left:0,top:0,width:540,height:788},wrist:[300,555]}, {rect:{left:540,top:0,width:480,height:788},wrist:[265,555]}, {rect:{left:1080,top:0,width:410,height:788},wrist:[200,555]}, {rect:{left:1580,top:0,width:360,height:788},wrist:[180,555]}];
 for(let i=0;i<hands.length;i++){
  const p=hands[i],s=.065,bytes=await sharp(path.join(src,'hands_source.png')).extract(p.rect).resize(Math.round(p.rect.width*s),Math.round(p.rect.height*s),{kernel:'nearest'}).png().toBuffer();
  const hand=await sharp({create:{width:96,height:96,channels:4,background:'#00000000'}}).composite([{input:bytes,left:Math.round(48-p.wrist[0]*s),top:Math.round(56-p.wrist[1]*s)}]).raw().toBuffer();
  manifest.exports.push(await save('grip_'+i,hand,96,96));
 }
 // Preserve the original commander's medallion rim, replacing only its inner portrait.
 const emblem=await sharp(path.join(root,'Assets/Art/UI/GusiniBossBarF1/Parts/emblem_roundel.png')).ensureAlpha().raw().toBuffer();
 const face=await sharp(raw,{raw:{width:320,height:384,channels:4}}).extract({left:105,top:79,width:52,height:70}).resize(30,36,{kernel:'nearest'}).ensureAlpha().raw().toBuffer();
 for(let y=0;y<50;y++)for(let x=0;x<50;x++)if((x-25)**2+(y-25)**2<17**2){const o=(y*50+x)*4;emblem[o]=52;emblem[o+1]=30;emblem[o+2]=82;emblem[o+3]=255;const fx=x-10,fy=y-7;if(fx>=0&&fx<30&&fy>=0&&fy<36){const q=(fy*30+fx)*4;if(face[q+3]>127)face.copy(emblem,o,q,q+4);}}
 manifest.exports.push(await save('hud_portrait',emblem,50,50));
 fs.writeFileSync(path.join(src,'layout.json'),JSON.stringify(manifest,null,2)+'\n');console.log(JSON.stringify(manifest.exports));
})().catch(e=>{console.error(e);process.exitCode=1;});
