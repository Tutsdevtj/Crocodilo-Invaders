const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
let sharp;try{sharp=require('sharp')}catch{sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'))}
const project=path.resolve(__dirname,'../..'),root=path.join(project,'SourceArt/BananaPhase1_VinesShoot_v01'),preview=path.join(project,'Docs/ArtPreviews/BananaPhase1_VinesShoot_v01');
const oldWeapon=path.join(project,'SourceArt/BananaAlienWeapon_v04_Organic');
const paletteHex=[...JSON.parse(fs.readFileSync(path.join(oldWeapon,'palette.json'),'utf8')).opaque,'#853b2b','#b75335','#ee976d','#ffbd94'];
const palette=paletteHex.map(h=>[1,3,5].map(i=>parseInt(h.slice(i,i+2),16))),cache=new Map();
const durations=[220,140,140,120,100,140,120,120,140,140,180,300,180,500,120,80,80,140,160,300,140,120,140,260];
const labels=['Pendurado no cipó A','Balanço para trás','Prepara salto','Impulso','Solta o cipó A','Salta entre cipós','Alcança o cipó B','Agarra o cipó B','Amortece a chegada','Oscilação de retorno','Reduz o balanço','Para pendurado','Levanta a arma','Mira e espera','Casca da arma abre','Dispara a banana','Recoil inicial','Recoil máximo','Retorna e fecha a arma','Pausa após o tiro','Prepara novo salto','Solta o cipó B','Alcança o cipó A','Retoma o cipó A'];
function quantize(data){for(let p=0;p<data.length;p+=4){if(data[p+3]<128){data[p]=data[p+1]=data[p+2]=data[p+3]=0;continue}const key=(data[p]<<16)|(data[p+1]<<8)|data[p+2];let c=cache.get(key);if(!c){let best=Infinity;for(const candidate of palette){const d=2*(data[p]-candidate[0])**2+3*(data[p+1]-candidate[1])**2+(data[p+2]-candidate[2])**2;if(d<best){best=d;c=candidate}}cache.set(key,c)}data[p]=c[0];data[p+1]=c[1];data[p+2]=c[2];data[p+3]=255}return data}
async function encode(raw,w,h){return sharp(raw,{raw:{width:w,height:h,channels:4}}).png().toBuffer()}
function stamp(dst,w,h,src,sw,sh,x,y){for(let yy=0;yy<sh;yy++)for(let xx=0;xx<sw;xx++){const p=(yy*sw+xx)*4,tx=x+xx,ty=y+yy;if(tx<0||ty<0||tx>=w||ty>=h||!src[p+3])continue;src.copy(dst,(ty*w+tx)*4,p,p+4)}}
async function main(){
 for(const dir of ['frames','clips','projectile'])fs.mkdirSync(path.join(root,dir),{recursive:true});fs.mkdirSync(preview,{recursive:true});
 const frames=[],raws=[],checks=[],used=new Set();let timeline=0;
 for(let i=0;i<24;i++){
  const name=`phase1_${String(i).padStart(2,'0')}.png`;
  const raw=await sharp(path.join(root,'frames_source',name)).resize(160,160,{kernel:'nearest'}).ensureAlpha().raw().toBuffer();quantize(raw);
  const png=await encode(raw,160,160);fs.writeFileSync(path.join(root,'frames',name),png);raws.push(raw);
  // Uppermost peach hand is the support/reach hand. Contact status is authored separately.
  let minY=160,minX=160,maxX=-1,maxY=-1;const points=[];
  for(let y=0;y<160;y++)for(let x=0;x<160;x++){const p=(y*160+x)*4;if(raw[p+3])used.add(raw.subarray(p,p+3).toString('hex'));if(raw[p+3]&&raw[p]>180&&raw[p+1]>90&&raw[p+2]>50&&raw[p+2]<raw[p+1]){minY=Math.min(minY,y);points.push([x,y])}}
  for(const [x,y]of points)if(y<=minY+14){minX=Math.min(minX,x);maxX=Math.max(maxX,x);maxY=Math.max(maxY,y)}
  const phase=i<6?'swing_jump':i<12?'catch_stop':i<18?'aim_shoot':'recover_return';
  const contact=i<4?'A':i<7?null:i<21?'B':i<22?null:'A';
  const f={index:i,file:'frames/'+name,label:labels[i],clip:phase,start_ms:timeline,duration_ms:durations[i],support_hand_px:maxX>=0?[Math.round((minX+maxX)/2),Math.round((minY+maxY)/2)]:null,vine_contact:contact,fire_event:i===15};
  frames.push(f);timeline+=durations[i];checks.push({index:i,sha256:crypto.createHash('sha256').update(png).digest('hex')});
 }
 if(new Set(checks.map(c=>c.sha256)).size!==24)throw Error('Expected24distinctdrawings');
 const projectileBuffer=fs.readFileSync(path.join(oldWeapon,'projectile/banana.png'));fs.writeFileSync(path.join(root,'projectile/banana.png'),projectileBuffer);
 const projectile=await sharp(projectileBuffer).ensureAlpha().raw().toBuffer();
 const sheet=await sharp({create:{width:1280,height:480,channels:4,background:'#00000000'}}).composite(frames.map((f,i)=>({input:path.join(root,f.file),left:(i%8)*160,top:Math.floor(i/8)*160}))).png().toBuffer();fs.writeFileSync(path.join(root,'spritesheet_8x3.png'),sheet);
 for(const [clip,start,end]of [['swing_jump',0,5],['catch_stop',6,11],['aim_shoot',12,17],['recover_return',18,23]]){
  const selected=frames.slice(start,end+1),strip=await sharp({create:{width:960,height:160,channels:4,background:'#00000000'}}).composite(selected.map((f,i)=>({input:path.join(root,f.file),left:i*160,top:0}))).png().toBuffer();fs.writeFileSync(path.join(root,'clips',clip+'.png'),strip);
 }
 await sharp(Buffer.concat(raws),{raw:{width:160,height:160*24,channels:4,pageHeight:160}}).gif({delay:durations,loop:0,colours:32,dither:0,effort:7}).toFile(path.join(preview,'phase1_native.gif'));
 // Projectile remains separate; placement is authored for the visible gun muzzle at release.
 const release=frames[15].start_ms,muzzle=[34,84];frames[15].muzzle_px=muzzle;
 const sampleMs=40,pages=[];
 for(let t=0;t<timeline;t+=sampleMs){let index=frames.findIndex(f=>t>=f.start_ms&&t<f.start_ms+f.duration_ms);if(index<0)index=23;const canvas=Buffer.alloc(320*192*4);stamp(canvas,320,192,raws[index],160,160,144,16);
  if(t>=release&&t<release+650){const x=Math.round(144+muzzle[0]-64-(t-release)*.32),y=16+muzzle[1]-16;stamp(canvas,320,192,projectile,64,32,x,y)}
  const page=await sharp(canvas,{raw:{width:320,height:192,channels:4}}).resize(640,384,{kernel:'nearest'}).raw().toBuffer();pages.push(page);
  if(t>=release+40&&t<release+80)fs.writeFileSync(path.join(preview,'firing_pose_x2.png'),await encode(page,640,384));
 }
 await sharp(Buffer.concat(pages),{raw:{width:640,height:384*pages.length,channels:4,pageHeight:384}}).gif({loop:0,delay:sampleMs,colours:32,dither:0,effort:7}).toFile(path.join(preview,'phase1_vines_shoot_x2.gif'));
 // Integer 2x contact sheet; no fractional scaling of pixels.
 await sharp(sheet).resize(2560,960,{kernel:'nearest'}).png().toFile(path.join(preview,'contact_sheet_x2.png'));
 const manifest={name:'Banana boss phase1 vine travel and shoot',phase:1,body_peel:'closed_all_frames',native_canvas:[160,160],frame_count:24,duration_ms:timeline,palette_limit:24,composition_includes_vines:true,world_motion_baked_into_frames:true,loop_preview:true,clips:[{id:'swing_jump',from:0,to:5},{id:'catch_stop',from:6,to:11},{id:'aim_shoot',from:12,to:17},{id:'recover_return',from:18,to:23}],projectile:{file:'projectile/banana.png',canvas:[64,32],spawn_frame:15,spawn_time_ms:release,muzzle_px:muzzle},frames};
 fs.writeFileSync(path.join(root,'animation.json'),JSON.stringify(manifest,null,2));fs.writeFileSync(path.join(root,'palette.json'),JSON.stringify({transparent:true,opaque:paletteHex},null,2));
 const inline=frames.map(f=>({...f,src:'data:image/png;base64,'+fs.readFileSync(path.join(root,f.file)).toString('base64')}));
 let html=fs.readFileSync(path.join(__dirname,'banana_transformacao_v02_preview.html'),'utf8').replace('__FRAMES__',JSON.stringify(inline)).replaceAll('Banana possessora — transformação em 24 frames','Banana fase1 — cipós e tiro').replaceAll('Banana possessora — transformação para a fase 2','Banana fase 1 — cipós e tiro').replace('24 desenhos · força crescente → arranca a ponta → joga fora → rasga a casca → rugido','24 quadros · balanço e salto → chegada e pausa → mira e tiro → retorno').replace('A ponta arrancada permanece fora do corpo ao final.','A banana do corpo permanece fechada; apenas a casca da arma abre. O projétil é composto no GIF e fica separado dos PNGs.').replace('minmax(320px,640px)','640px');
 new(require('node:vm').Script)(html.match(/<script>([\s\S]*?)<\/script>/)[1]);fs.writeFileSync(path.join(preview,'preview.html'),html);
 const gif=await sharp(path.join(preview,'phase1_native.gif'),{animated:true}).metadata();if(gif.pages!==24||JSON.stringify(gif.delay)!==JSON.stringify(durations))throw Error('Animation count or timing changed');
 const qa={frame_count:24,distinct_frames:24,native_canvas:[160,160],palette_limit:24,actual_frame_colors:used.size,binary_alpha:true,projectile_separate:true,phase1_body_closed_visual_review:true,gif_frames:gif.pages,duration_ms:timeline,preview_javascript_parses:true,unity_editor_validated:false,gameplay_integrated:false,checks};fs.writeFileSync(path.join(root,'qa_report.json'),JSON.stringify(qa,null,2));console.log(JSON.stringify({...qa,checks:undefined}));
}
main().catch(e=>{console.error(e);process.exitCode=1});
