// Layer-specific finishes and mythic-only frame break. Preview; no Assets writes.
const fs=require('node:fs'),path=require('node:path');
const {renderLabels:labels,metrics:typographyMetrics}=require('./card_typography_v08.cjs');
const sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));
const project=path.resolve(__dirname,'../..'),arts=path.join(project,'SourceArt/CardsFoil_v05_AltArt'),out=path.join(project,'Docs/ArtPreviews/CardsFoil_v06');
const viz=path.join(process.env.USERPROFILE,'.codex/visualizations/2026/10/20/01a15baa-92dc-7e70-91ec-db634318efdb/cartas-acabamentos.html');
const names=['COMUM','RARO','ÉPICO','LENDÁRIO','MÍTICO'],colors=['#c5c6ad','#a8d7e8','#d6a6f9','#ffe0a0','#ffb18d'];
const W=180,H=268;
function rect(b,x,y,w,h,c){const rgb=c.slice(1).match(/../g).map(v=>parseInt(v,16));for(let yy=y;yy<y+h;yy++)for(let xx=x;xx<x+w;xx++){const p=(yy*W+xx)*4;for(let j=0;j<3;j++)b[p+j]=rgb[j];b[p+3]=255;}}
function inside(x,y,poly){let hit=false;for(let i=0,j=poly.length-1;i<poly.length;j=i++)if((poly[i][1]>y)!==(poly[j][1]>y)&&x<(poly[j][0]-poly[i][0])*(y-poly[i][1])/(poly[j][1]-poly[i][1])+poly[i][0])hit=!hit;return hit;}
const legendHead=[[31,67],[25,51],[33,40],[50,31],[76,27],[104,31],[121,41],[122,55],[118,61],[128,65],[138,68],[145,76],[145,89],[138,101],[122,110],[105,114],[84,110],[64,101],[48,93],[38,82]];
const legendArmor=[[0,77],[21,76],[36,84],[60,107],[79,111],[109,117],[125,125],[146,133],[169,144],[180,149],[180,181],[157,190],[140,201],[109,212],[78,209],[61,206],[39,205],[28,193],[13,177],[0,174]];
const mythicRig=[[0,27],[8,20],[21,20],[38,37],[47,43],[55,38],[73,36],[90,39],[108,46],[118,56],[127,54],[145,53],[180,46],[180,132],[172,142],[180,155],[180,235],[164,239],[148,227],[126,221],[110,239],[84,223],[61,223],[38,209],[25,198],[0,191],[0,76],[11,65],[0,58]];
// Complete weapon silhouettes. The previous horizontal bands sliced across
// barrels and copied pieces of sky onto the frame at their bottom boundary.
const mythicOverhangs=[
 [[0,29],[7,27],[10,24],[15,24],[20,27],[25,32],[30,30],[37,33],[40,38],[46,39],[51,44],[57,47],[62,55],[62,63],[57,69],[51,74],[44,77],[33,75],[25,71],[18,67],[12,63],[5,60],[0,56]],
 [[120,57],[125,54],[132,56],[143,54],[153,51],[162,48],[168,49],[173,54],[171,60],[165,66],[153,70],[140,74],[126,74]],
 [[124,78],[137,76],[149,72],[161,69],[169,71],[174,76],[172,83],[166,88],[151,92],[138,96],[127,93]],
 [[127,102],[140,100],[151,96],[164,91],[172,94],[177,100],[174,107],[168,113],[152,117],[138,121],[128,117]]
];
function overlayWeapons(base,art){
 const shapes=new Uint8Array(W*H),seed=new Uint8Array(W*H),cutout=new Uint8Array(W*H);
 for(let y=0;y<H;y++)for(let x=0;x<W;x++){const k=y*W+x;if(!mythicOverhangs.some(poly=>inside(x,y,poly)))continue;shapes[k]=1;const p=k*4,r=art[p],g=art[p+1],b=art[p+2];
  // Select actual brass, reactor and metal pixels, not dark blue sky/smoke.
  const brass=r>65&&g>b*1.18&&r>g*.88;
  const reactor=g>60&&g>r*1.12&&g>b*1.06;
  const steel=r>35&&r<205&&Math.abs(r-g)<21&&Math.abs(g-b)<27;
  seed[k]=brass||reactor||steel?1:0;
 }
 // Recover the dark one-pixel outlines bordering selected hardware.
 for(let y=1;y<H-1;y++)for(let x=1;x<W-1;x++){const k=y*W+x;if(!shapes[k])continue;let connected=seed[k];for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++)connected|=seed[(y+dy)*W+x+dx];if(connected)cutout[k]=1;}
 // Remove isolated sparks: overhang consists of connected hardware only.
 const seen=new Uint8Array(W*H);for(let k=0;k<cutout.length;k++){if(!cutout[k]||seen[k])continue;const group=[k];seen[k]=1;for(let q=0;q<group.length;q++){const at=group[q],x=at%W,y=Math.floor(at/W);for(const n of [x>0?at-1:-1,x<W-1?at+1:-1,y>0?at-W:-1,y<H-1?at+W:-1])if(n>=0&&cutout[n]&&!seen[n]){seen[n]=1;group.push(n);}}if(group.length<16)for(const n of group)cutout[n]=0;}
 const picture=(x,y)=>x>=24&&x<156&&y>=48&&y<208;
 // A one-pixel contact shadow sits on the metal, not in the illustration.
 for(let y=2;y<H-2;y++)for(let x=2;x<W-2;x++){const k=y*W+x;if(cutout[k]||picture(x,y))continue;let edge=false;for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++)edge||=Boolean(cutout[(y+dy)*W+x+dx]);if(edge){const p=k*4;base[p]=9;base[p+1]=15;base[p+2]=20;}}
 for(let k=0;k<cutout.length;k++)if(cutout[k])art.copy(base,k*4,k*4,k*4+4);
 return cutout;
}
async function rawPng(b,palette=false){return sharp(b,{raw:{width:W,height:H,channels:4}}).png(palette?{palette:true,colours:128,dither:0}:{}).toBuffer();}
async function main(){fs.mkdirSync(out,{recursive:true});const data=[];const common=await sharp(path.join(project,'SourceArt/CardsCroc_v03/master_0.png')).extract({left:24,top:48,width:132,height:160}).png().toBuffer();
for(let r=0;r<5;r++){let base,original,weaponOverhang;
if(r===0){base=await sharp(path.join(project,'SourceArt/CardsCroc_v03/master_0.png')).composite([{input:common,left:24,top:48}]).ensureAlpha().raw().toBuffer();for(let y=48;y<208;y++)for(let x=156;x<W;x++){const p=(y*W+x)*4,q=(y*W+(W-1-x))*4;base.copy(base,p,q,q+4);}}
else if(r<4){const art=await sharp(path.join(arts,`art_${r}.png`)).resize(W,H,{fit:'cover',kernel:'nearest'}).png().toBuffer().then(b=>sharp(b).extract({left:18,top:20,width:132,height:160}).png().toBuffer());base=await sharp(path.join(project,`SourceArt/CardsCroc_v03/master_${r}.png`)).composite([{input:art,left:24,top:48}]).ensureAlpha().raw().toBuffer();}
else{original=await sharp(path.join(arts,'art_4.png')).resize(W,H,{fit:'cover',kernel:'nearest'}).ensureAlpha().raw().toBuffer();base=await sharp(path.join(project,'SourceArt/CardsCroc_v03/master_4.png')).ensureAlpha().raw().toBuffer();for(let y=48;y<208;y++)for(let x=156;x<W;x++){const p=(y*W+x)*4,q=(y*W+(W-1-x))*4;base.copy(base,p,q,q+4);}for(let y=48;y<208;y++)for(let x=24;x<156;x++){const p=(y*W+x)*4;original.copy(base,p,p,p+4);} // Complete standard frame first; only selected parts of the rig overlap it.
weaponOverhang=overlayWeapons(base,original);}
const mask=Buffer.alloc(W*H*4);for(let y=0;y<H;y++)for(let x=0;x<W;x++){const p=(y*W+x)*4,picture=x>=24&&x<156&&y>=48&&y<208,plates=(x>25&&x<155&&(y>11&&y<42||y>214&&y<254));let neutral=0,holo=0,fg=0,visible=255;
if(r===1){const ring=x>=21&&x<159&&y>=45&&y<211&&!picture;neutral=ring?255:0;}
if(r===2){neutral=picture?210:0;holo=!picture&&!plates?230:0;}
if(r===3){const sx=x-24+18,sy=y-48+20;fg=picture&&(inside(sx,sy,legendHead)||inside(sx,sy,legendArmor))?255:0;holo=picture&&!fg?245:0;neutral=!picture&&!plates?165:0;}
if(r===4){fg=inside(x,y,mythicRig)?255:0;neutral=130;holo=picture||weaponOverhang[y*W+x]?128:255;}
mask[p]=neutral;mask[p+1]=holo;mask[p+2]=fg;mask[p+3]=visible;}
const b=await rawPng(base,true),m=await rawPng(mask),l=await labels(r);for(const [key,buffer]of[['base',b],['materials',m],['labels',l]])fs.writeFileSync(path.join(out,`${key}_${r}.png`),buffer);data.push({rank:r,name:names[r],base:'data:image/png;base64,'+b.toString('base64'),materials:'data:image/png;base64,'+m.toString('base64'),labels:'data:image/png;base64,'+l.toString('base64')});}
fs.writeFileSync(path.join(out,'typography_metrics.json'),JSON.stringify(typographyMetrics,null,2));
const html=fs.readFileSync(path.join(__dirname,'foil_preview_v06.html'),'utf8').replace('__CARD_DATA__',JSON.stringify(data));if(Buffer.byteLength(html)>1000000)throw Error('Preview >1MB');fs.writeFileSync(viz,html);fs.writeFileSync(path.join(out,'preview.html'),html);console.log(JSON.stringify({preview:viz,bytes:Buffer.byteLength(html),production_assets:'UNCHANGED'}));}
main().catch(e=>{console.error(e);process.exit(1)});
