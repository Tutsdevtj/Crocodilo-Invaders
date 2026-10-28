const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
let sharp;try{sharp=require('sharp')}catch{sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'))}
const project=path.resolve(__dirname,'../..'),root=path.join(project,'SourceArt/BananaAlienWeapon_v02_Power'),preview=path.join(project,'Docs/ArtPreviews/BananaAlienWeapon_v02_Power');
const colors=['130e15','8c5a10','c68f08','f2c612','ffe94a','fff3ae','52301d','8d5b35','291431','552869','8a4c9b','073b17','168322','41ed26','b6ff72','383044','806e88','d79b21','ffda53','fff8ce'];
const palette=colors.map(h=>[0,2,4].map(i=>parseInt(h.slice(i,i+2),16))),cache=new Map();
const durations=[160,80,80,40,60,80,80,80,80,80,80,100],recoil=[0,0,0,0,4,8,8,6,4,2,1,0];
function quantize(data){for(let p=0;p<data.length;p+=4){if(data[p+3]<128){data[p]=data[p+1]=data[p+2]=data[p+3]=0;continue}let key=(data[p]<<16)|(data[p+1]<<8)|data[p+2],c=cache.get(key);if(!c){let best=Infinity;for(const candidate of palette){let d=2*(data[p]-candidate[0])**2+3*(data[p+1]-candidate[1])**2+(data[p+2]-candidate[2])**2;if(d<best){best=d;c=candidate}}cache.set(key,c)}data[p]=c[0];data[p+1]=c[1];data[p+2]=c[2];data[p+3]=255}return data}
async function encode(raw,w,h){return sharp(raw,{raw:{width:w,height:h,channels:4}}).png().toBuffer()}
function stamp(canvas,w,h,raw,rw,rh,x,y){for(let yy=0;yy<rh;yy++)for(let xx=0;xx<rw;xx++){const a=(yy*rw+xx)*4,tx=x+xx,ty=y+yy;if(tx<0||ty<0||tx>=w||ty>=h||!raw[a+3])continue;raw.copy(canvas,(ty*w+tx)*4,a,a+4)}}
async function main(){
 for(const dir of [root,path.join(root,'frames'),path.join(root,'projectile'),preview])fs.mkdirSync(dir,{recursive:true});
 const raws=[],frames=[];
 for(let i=0;i<12;i++){
  const local=i<6?i:[3,4,2,1,0,5][i-6],file=path.join(root,`sources/weapon_${i<6?'01':'02'}.png`);
  const {data,info}=await sharp(file).extract({left:(local%3)*512,top:Math.floor(local/3)*512,width:Math.min(548,1536-(local%3)*512),height:512}).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  for(let p=0;p<data.length;p+=4){if(data[p+3]<128)data[p]=data[p+1]=data[p+2]=data[p+3]=0;else data[p+3]=255}
  const small=await sharp(data,{raw:{width:info.width,height:info.height,channels:4}}).resize(Math.round(info.width*.18),92,{kernel:'nearest'}).ensureAlpha().raw().toBuffer();quantize(small);const sw=Math.round(info.width*.18);
  // The generated sheet has occasional neighboring-cell overshoot. Weapon is one connected prop.
  const seen=new Uint8Array(sw*92),components=[];
  for(let k=0;k<seen.length;k++){if(seen[k]||!small[k*4+3])continue;const pixels=[k];seen[k]=1;
   for(let q=0;q<pixels.length;q++){const v=pixels[q],x=v%sw,y=Math.floor(v/sw);for(const n of [x>0?v-1:-1,x<sw-1?v+1:-1,y>0?v-sw:-1,y<91?v+sw:-1])if(n>=0&&!seen[n]&&small[n*4+3]){seen[n]=1;pixels.push(n)}}components.push(pixels);
  }
  components.sort((a,b)=>b.length-a.length);for(const comp of components.slice(1))for(const k of comp)small[k*4]=small[k*4+1]=small[k*4+2]=small[k*4+3]=0;
  // Locate the lower purple grip, then align every drawing before applying authored recoil.
  let bottom=-1;for(let y=0;y<92;y++)for(let x=0;x<sw;x++){const p=(y*sw+x)*4;if(small[p+3]&&small[p+2]>small[p+1]*1.3&&small[p]>small[p+1]*1.3&&small[p+2]>40)bottom=Math.max(bottom,y)}
  let sum=0,n=0;for(let y=Math.max(0,bottom-12);y<=bottom;y++)for(let x=0;x<sw;x++){const p=(y*sw+x)*4;if(small[p+3]&&small[p+2]>small[p+1]*1.3&&small[p]>small[p+1]*1.3&&small[p+2]>40){sum+=x;n++}}
  if(!n)throw Error('Missing purple grip');
  const anchor=[Math.round(sum/n),bottom-9],dx=86+recoil[i]-anchor[0],dy=60-anchor[1];
  const out=Buffer.alloc(128*96*4);stamp(out,128,96,small,sw,92,dx,dy);raws.push(out);
  frames.push({index:i,file:`frames/weapon_${String(i).padStart(2,'0')}.png`,duration_ms:durations[i],hand_socket_px:[86+recoil[i],60],recoil_px:[recoil[i],0],muzzle_px:[32+recoil[i],34],fire_event:i===3});
 }
 // Identical first/last closed pose makes the repeated cycle join cleanly.
 raws[11]=Buffer.from(raws[0]);
 for(let i=0;i<12;i++)fs.writeFileSync(path.join(root,frames[i].file),await encode(raws[i],128,96));
 fs.copyFileSync(path.join(root,frames[0].file),path.join(root,'weapon_idle.png'));
 const proj=await sharp(path.join(root,'sources/projectile.png')).trim({threshold:10,background:'#00000000'}).resize(32,16,{fit:'contain',kernel:'nearest',background:'#00000000'}).ensureAlpha().raw().toBuffer();quantize(proj);
 const projectile=await encode(proj,32,16);fs.writeFileSync(path.join(root,'projectile/banana.png'),projectile);
 const sheet=await sharp({create:{width:768,height:192,channels:4,background:'#00000000'}}).composite(frames.map((f,i)=>({input:path.join(root,f.file),left:(i%6)*128,top:Math.floor(i/6)*96}))).png().toBuffer();fs.writeFileSync(path.join(root,'spritesheet_6x2.png'),sheet);
 await sharp(sheet).resize(3072,768,{kernel:'nearest'}).png().toFile(path.join(preview,'weapon_frames_x4.png'));
 await sharp(path.join(root,'weapon_idle.png')).resize(768,576,{kernel:'nearest'}).png().toFile(path.join(preview,'weapon_idle_x6.png'));
 await sharp(Buffer.concat(raws),{raw:{width:128,height:96*12,channels:4,pageHeight:96}}).gif({delay:durations,loop:0,colours:32,dither:0}).toFile(path.join(preview,'weapon_only_native.gif'));
 // Preview composition uses independent fruit sprite; it never gets baked into weapon frames.
 const endTimes=[];let total=0;for(const d of durations){total+=d;endTimes.push(total)}
 const release=durations.slice(0,3).reduce((a,b)=>a+b,0),pages=[];
 for(let t=0;t<total;t+=40){const index=endTimes.findIndex(end=>t<end),canvas=Buffer.alloc(224*112*4);stamp(canvas,224,112,raws[index],128,96,88,8);
  if(t>=release&&t<release+400){const fruitX=Math.round(96-(t-release)*.36);stamp(canvas,224,112,proj,32,16,fruitX,36)}
  const page=await sharp(canvas,{raw:{width:224,height:112,channels:4}}).resize(896,448,{kernel:'nearest'}).raw().toBuffer();pages.push(page);
  if(t===360)fs.writeFileSync(path.join(preview,'firing_pose_x4.png'),await encode(page,896,448));
 }
 await sharp(Buffer.concat(pages),{raw:{width:896,height:448*pages.length,channels:4,pageHeight:448}}).gif({delay:40,loop:0,colours:32,dither:0,effort:7}).toFile(path.join(preview,'banana_weapon_fire_x4.gif'));
 const manifest={name:'Heavy banana alien cannon',canvas:[128,96],native_pixel_art:true,opaque_palette_limit:20,pixels_per_unit_suggestion:40,facing:'left',duration_ms:total,loop:true,hand_socket_idle_px:[86,60],nominal_pivot_normalized:[86/128,1-60/96],muzzle_idle_px:[32,34],projectile:{file:'projectile/banana.png',canvas:[32,16],spawn_frame:3,spawn_time_ms:release,facing:'left'},frames};
 fs.writeFileSync(path.join(root,'animation.json'),JSON.stringify(manifest,null,2));fs.writeFileSync(path.join(root,'palette.json'),JSON.stringify({transparent:true,opaque:colors.map(c=>'#'+c)},null,2));
 const used=new Set();for(const raw of [...raws,proj])for(let p=0;p<raw.length;p+=4){if(raw[p+3]!==0&&raw[p+3]!==255)throw Error('Soft alpha');if(raw[p+3])used.add(raw.subarray(p,p+3).toString('hex'))}
 const metadata=await sharp(path.join(preview,'banana_weapon_fire_x4.gif'),{animated:true}).metadata();
 const qa={weapon_frames:12,projectile_separate:true,native_canvas:[128,96],projectile_canvas:[32,16],actual_opaque_colors:used.size,palette_limit:20,binary_alpha:true,preview_integer_scale:4,preview_frames:metadata.pages,cycle_duration_ms:total,first_last_match:raws[0].equals(raws[11]),gameplay_integrated:false,sha256:frames.map(f=>crypto.createHash('sha256').update(fs.readFileSync(path.join(root,f.file))).digest('hex'))};fs.writeFileSync(path.join(root,'qa_report.json'),JSON.stringify(qa,null,2));console.log(JSON.stringify({...qa,sha256:undefined}));
}
main().catch(e=>{console.error(e);process.exitCode=1});
