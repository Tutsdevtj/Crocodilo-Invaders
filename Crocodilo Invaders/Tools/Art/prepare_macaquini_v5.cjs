const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const sharp=require('C:/Users/berna/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const palette=require('./pixel_palette.cjs');
const root=path.resolve(__dirname,'../..'),src=path.join(root,'SourceArt/Macaquini_v05'),out=path.join(root,'Assets/Art/JungleStage/Macaquini_v05');
const inside=(x,y,p)=>{let yes=false;for(let i=0,j=p.length-1;i<p.length;j=i++){const a=p[i],b=p[j];if((a[1]>y)!=(b[1]>y)&&x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0])yes=!yes;}return yes;};
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
async function native(file,w=320,h=384){return sharp(file).resize(w,h,{kernel:'nearest'}).ensureAlpha().raw().toBuffer();}
async function write(s){for(let i=3;i<s.data.length;i+=4)s.data[i]=s.data[i]>127?255:0;const p=path.join(out,s.name+'.png');await sharp(s.data,{raw:{width:s.w,height:s.h,channels:4}}).png().toFile(p);const colors=new Set(),alpha=new Set();for(let i=0;i<s.data.length;i+=4){alpha.add(s.data[i+3]);if(s.data[i+3])colors.add(s.data.subarray(i,i+3).toString('hex'));}return {name:s.name,width:s.w,height:s.h,colors:colors.size,alpha:[...alpha],sha256:sha(p)};}
(async()=>{
 fs.mkdirSync(out,{recursive:true});const sprites=[];
 const master=await native(path.join(src,'master_source.png'));
 const parts=[
 {id:'gun_upper',pivot:[100,127],end:[77,175],poly:[[89,109],[111,118],[114,141],[96,171],[84,187],[63,181],[68,159],[79,134]]},
 {id:'gun_lower',pivot:[77,175],end:[35,257],poly:[[65,164],[90,172],[80,201],[55,238],[44,261],[26,265],[20,252],[35,218],[49,189]]},
 {id:'support_upper',pivot:[207,116],end:[238,167],poly:[[192,102],[215,102],[233,123],[250,153],[255,176],[234,182],[219,160],[204,139]]},
 {id:'support_lower',pivot:[238,167],end:[267,260],poly:[[226,156],[251,154],[264,184],[275,225],[280,258],[263,271],[251,256],[245,215],[234,189]]}
 ];
 for(const p of parts){const b=Buffer.alloc(master.length);for(let y=0;y<384;y++)for(let x=0;x<320;x++)if(inside(x,y,p.poly))master.copy(b,(y*320+x)*4,(y*320+x)*4,(y*320+x)*4+4);sprites.push({name:p.id,data:b,w:320,h:384});}
 for(const n of ['core','halfcore'])sprites.push({name:n,data:await native(path.join(src,n+'_source.png')),w:320,h:384});
 const core=sprites.find(s=>s.name==='core').data;
 const left=Buffer.alloc(core.length),right=Buffer.alloc(core.length);
 for(let y=0;y<222;y++)for(let x=75;x<240;x++){const split=151+(y-120)*.1+(Math.floor(y/6)%2?2:-2);const b=x<split?left:right;core.copy(b,(y*320+x)*4,(y*320+x)*4,(y*320+x)*4+4);}
 sprites.push({name:'peel_left',data:left,w:320,h:384},{name:'peel_right',data:right,w:320,h:384});
 const handInfo=await sharp(path.join(src,'hands_source.png')).metadata();
 const handData=await sharp(path.join(src,'hands_source.png')).ensureAlpha().raw().toBuffer();
 const cells=[{x:0,w:540,wrist:300},{x:540,w:480,wrist:275},{x:1080,w:410,wrist:200},{x:1580,w:403,wrist:200}];
 for(let i=0;i<4;i++){
  const b=Buffer.alloc(96*96*4),p=cells[i],scale=.075;
  for(let y=0;y<96;y++)for(let x=0;x<96;x++){const sx=Math.floor((x-48)/scale+p.wrist+p.x),sy=Math.floor((y-56)/scale+580);if(sx>=p.x&&sx<Math.min(p.x+p.w,handInfo.width)&&sy>=0&&sy<handInfo.height){const q=(sy*handInfo.width+sx)*4;handData.copy(b,(y*96+x)*4,q,q+4);}}
  sprites.push({name:'grip_'+i,data:b,w:96,h:96});
 }
 for(const s of sprites)for(let i=3;i<s.data.length;i+=4)s.data[i]=s.data[i]>127?255:0;
 const all=Buffer.concat(sprites.map(s=>s.data)),colors=palette(all,32);let offset=0;
 const manifest={version:5,body_pivot:[160,208],peel_pivot:[160,218],parts,shared_palette:colors,exports:[],sources:['master','core','halfcore','hands'].map(name=>({name,sha256:sha(path.join(src,name+'_source.png'))}))};
 for(const s of sprites){const length=s.data.length;s.data=all.subarray(offset,offset+length);offset+=length;manifest.exports.push(await write(s));}
 let difference=0;const quantCore=sprites.find(s=>s.name==='core').data,quantLeft=sprites.find(s=>s.name==='peel_left').data,quantRight=sprites.find(s=>s.name==='peel_right').data;
 for(let y=0;y<222;y++)for(let x=75;x<240;x++){const o=(y*320+x)*4;if(!quantCore[o+3])continue;const b=quantLeft[o+3]?quantLeft:quantRight;for(let c=0;c<4;c++)if(b[o+c]!==quantCore[o+c]){difference++;break;}}
 if(difference)throw Error('Peel reconstruction mismatch '+difference);manifest.shell_reconstruction_differences=difference;
 for(const [name,source,crop] of [['hud_portrait','core',{left:98,top:62,width:90,height:100}],['hud_portrait_half','halfcore',{left:76,top:34,width:98,height:110}]]){
  const med=await sharp(path.join(root,'Assets/Art/UI/GusiniBossBarF1/Parts/emblem_roundel.png')).ensureAlpha().raw().toBuffer();
  const face=await sharp(sprites.find(s=>s.name===source).data,{raw:{width:320,height:384,channels:4}}).extract(crop).resize(30,34,{kernel:'nearest'}).raw().toBuffer();
  for(let y=0;y<50;y++)for(let x=0;x<50;x++)if((x-25)**2+(y-25)**2<17**2){const o=(y*50+x)*4;med[o]=52;med[o+1]=30;med[o+2]=82;med[o+3]=255;const fx=x-10,fy=y-8;if(fx>=0&&fx<30&&fy>=0&&fy<34){const q=(fy*30+fx)*4;if(face[q+3]>127)face.copy(med,o,q,q+4);}}
  manifest.exports.push(await write({name,data:med,w:50,h:50}));
 }
 fs.copyFileSync(path.join(root,'SourceArt/Macaquini_v04/hud_title_source.png'),path.join(out,'hud_title.png'));
 fs.writeFileSync(path.join(src,'layout.json'),JSON.stringify(manifest,null,2)+'\n');console.log(JSON.stringify(manifest.exports));
})().catch(e=>{console.error(e);process.exitCode=1;});
