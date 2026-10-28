// All five families in the approved v08 model. Export only; no Unity Assets writes.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));
const {renderLabels,metrics}=require('./card_typography_v08.cjs');
const {inside,overlayWeapons}=require('./build_foil_preview_v06.cjs');
const project=path.resolve(__dirname,'../..'),root=path.join(project,'SourceArt/CardCollection_v09'),out=path.join(project,'Docs/ArtPreviews/CardCollection_v09');
const approved=path.join(project,'SourceArt/CardsModel_Approved_v08_20261021/layers'),ranks=['Common','Rare','Epic','Legendary','Mythic'],names=['COMUM','RARO','ÉPICO','LENDÁRIO','MÍTICO'];
const families=[
 {id:'AttackDamage',label:'Ataque',title:'ATAQUE',attribute:'DANO DE ATAQUE',values:[5,10,15,20,30]},
 {id:'AttackSpeed',label:'Cadência',title:'RAJADA',attribute:'CADÊNCIA',values:[5,10,15,20,30]},
 {id:'MaximumHealth',label:'Vida',title:'BLINDAGEM',attribute:'VIDA MÁXIMA',values:[10,15,20,30,40]},
 {id:'MoveSpeed',label:'Movimento',title:'PROPULSÃO',attribute:'MOVIMENTO',values:[3,5,8,10,15]},
 {id:'SpecialDamage',label:'Especial',title:'ESPECIAL',attribute:'DANO ESPECIAL',values:[5,10,15,20,30]}
];
const W=180,H=268;
const head=[[25,82],[21,66],[29,48],[45,37],[66,31],[92,32],[113,40],[125,53],[123,67],[140,70],[158,80],[164,93],[156,108],[136,121],[105,127],[74,115],[48,102],[34,95]];
const armor=[[0,79],[22,80],[41,96],[72,113],[111,119],[143,128],[180,139],[180,213],[137,230],[78,229],[34,213],[0,190]];
// Each family has its own projecting hardware: guns, armor, thrusters, emitter.
const overhangs={
 AttackSpeed:[[[5,52],[15,45],[30,48],[45,62],[43,85],[25,95],[6,89]],[[145,56],[166,47],[179,52],[179,92],[157,106],[145,92]]],
 MaximumHealth:[[[3,62],[13,50],[27,51],[42,66],[40,94],[22,108],[5,99]],[[146,57],[161,50],[175,56],[179,85],[168,106],[150,101]]],
 MoveSpeed:[[[2,71],[9,55],[28,50],[44,68],[41,96],[20,109],[4,99]],[[147,65],[167,59],[179,70],[179,108],[160,122],[146,105]]],
 SpecialDamage:[[[3,75],[10,59],[28,58],[44,73],[41,103],[19,119],[4,106]],[[146,67],[163,61],[178,70],[179,114],[159,126],[146,109]]]
};
async function rawPng(b,palette=false){return sharp(b,{raw:{width:W,height:H,channels:4}}).png(palette?{palette:true,colours:128,dither:0}:{}).toBuffer();}
async function extractAtlas(family){const dir=path.join(root,family.id),atlas=path.join(dir,'atlas.png'),m=await sharp(atlas).metadata(),cells=[];fs.mkdirSync(dir,{recursive:true});for(let rank=0;rank<5;rank++){const col=rank%3,row=Math.floor(rank/3),left=Math.floor(col*m.width/3),top=Math.floor(row*m.height/2),width=Math.floor((col+1)*m.width/3)-left,height=Math.floor((row+1)*m.height/2)-top;const image=await sharp(atlas).extract({left,top,width,height}).resize(W,H,{fit:'cover',kernel:'nearest'}).ensureAlpha().raw().toBuffer();fs.writeFileSync(path.join(dir,`art_${rank}.png`),await rawPng(image));cells.push(image);}return {cells,atlas_canvas:[m.width,m.height]};}
async function compose(family,rank,art){const picture=(x,y)=>x>=24&&x<156&&y>=48&&y<208;
let base=await sharp(path.join(approved,`base_${rank}.png`)).ensureAlpha().raw().toBuffer();
if(rank===4){base=await sharp(path.join(project,'SourceArt/CardsCroc_v03/master_4.png')).ensureAlpha().raw().toBuffer();for(let y=48;y<208;y++)for(let x=156;x<W;x++){const p=(y*W+x)*4,q=(y*W+W-1-x)*4;base.copy(base,p,q,q+4);}}
const aligned=Buffer.alloc(W*H*4);for(let y=28;y<H;y++)for(let x=6;x<W;x++){const p=(y*W+x)*4,q=((y-28)*W+x-6)*4;art.copy(aligned,p,q,q+4);}
const picturePixels=rank<4?await sharp(art,{raw:{width:W,height:H,channels:4}}).resize(132,160,{fit:'cover',kernel:'nearest'}).ensureAlpha().raw().toBuffer():null;
for(let y=48;y<208;y++)for(let x=24;x<156;x++){const p=(y*W+x)*4;if(rank<4){const q=((y-48)*132+x-24)*4;picturePixels.copy(base,p,q,q+4);}else aligned.copy(base,p,p,p+4);}
const projection=rank===4?overlayWeapons(base,aligned,overhangs[family.id],true):null;
const mask=Buffer.alloc(W*H*4);for(let y=0;y<H;y++)for(let x=0;x<W;x++){const p=(y*W+x)*4,insidePicture=picture(x,y),plates=x>25&&x<155&&(y>11&&y<42||y>214&&y<254),sx=rank<4?(x-24)*180/132:x-6,sy=rank<4?(y-48+18.267)*180/132:y-28;let neutral=0,holo=0,fg=0;
if(rank===1)neutral=x>=21&&x<159&&y>=45&&y<211&&!insidePicture?255:0;
if(rank===2){neutral=insidePicture?210:0;holo=!insidePicture&&!plates?230:0;}
if(rank===3){fg=insidePicture&&(inside(sx,sy,head)||inside(sx,sy,armor))?255:0;holo=insidePicture&&!fg?245:0;neutral=!insidePicture&&!plates?165:0;}
if(rank===4){fg=inside(sx,sy,head)||inside(sx,sy,armor)||projection[y*W+x]?255:0;neutral=130;holo=insidePicture||projection[y*W+x]?128:255;}
mask[p]=neutral;mask[p+1]=holo;mask[p+2]=fg;mask[p+3]=255;}
return {base:await rawPng(base,true),materials:await rawPng(mask),labels:await renderLabels(rank,{title:family.title,attribute:family.attribute,percent:family.values[rank]})};}
async function main(){fs.mkdirSync(root,{recursive:true});fs.mkdirSync(out,{recursive:true});const collection=[],manifest={version:9,approved_model:'CardsModel_Approved_v08_20261021',canvas:[W,H],font_layout:'card_typography_v08.cjs',finish_shader:'foil_preview_v06.html',families:[]},catalog=fs.readFileSync(path.join(project,'Assets/Data/UpgradeCards.asset'),'utf8');
for(const f of families){const dir=path.join(out,f.id);fs.mkdirSync(dir,{recursive:true});const atlas=f.id==='AttackDamage'?null:await extractAtlas(f),cards=[];
for(let rank=0;rank<5;rank++){const layers=f.id==='AttackDamage'?Object.fromEntries(['base','materials','labels'].map(key=>[key,fs.readFileSync(path.join(approved,`${key}_${rank}.png`))])):await compose(f,rank,atlas.cells[rank]);
const face=await sharp(layers.base).resize(360,536,{kernel:'nearest'}).composite([{input:layers.labels}]).png().toBuffer();for(const [key,value]of Object.entries(layers))fs.writeFileSync(path.join(dir,`${key}_${rank}.png`),value);fs.writeFileSync(path.join(dir,`${ranks[rank]}.png`),face);
if(!new RegExp('percent: '+f.values[rank]+'\\s+artworkPath: Cards/'+f.id+'/'+ranks[rank]+'(?:\\r?\\n|$)').test(catalog))throw Error('Catalog mismatch '+f.id+rank);
const inlineBase=await sharp(layers.base).webp({quality:90,effort:6}).toBuffer();cards.push({rank,name:names[rank],base:'data:image/webp;base64,'+inlineBase.toString('base64'),materials:'data:image/png;base64,'+layers.materials.toString('base64'),labels:'data:image/png;base64,'+layers.labels.toString('base64')});}
collection.push({id:f.id,label:f.label,title:f.title,cards});manifest.families.push({...f,atlas_canvas:atlas?.atlas_canvas??null,cards:ranks.map((rank,i)=>({rank,percent:f.values[i],file:`${f.id}/${rank}.png`,sha256:crypto.createHash('sha256').update(fs.readFileSync(path.join(dir,rank+'.png'))).digest('hex')}))});}
const sharedShader=fs.readFileSync(path.join(__dirname,'foil_preview_v06.html'),'utf8'),shaderCode=sharedShader.slice(sharedShader.indexOf('const vert='),sharedShader.indexOf('function shader('));
const html=fs.readFileSync(path.join(__dirname,'card_collection_v09.html'),'utf8').replace('__COLLECTION_DATA__',JSON.stringify(collection)).replace('__SHADER_CODE__',shaderCode);if(Buffer.byteLength(html)>1000000)throw Error('Collection preview >1MB: '+Buffer.byteLength(html));fs.writeFileSync(path.join(out,'cartas-colecao.html'),html);fs.writeFileSync(path.join(root,'manifest.json'),JSON.stringify(manifest,null,2));fs.writeFileSync(path.join(out,'typography_metrics.json'),JSON.stringify(metrics,null,2));console.log(JSON.stringify({cards:25,new_artwork:20,inline_bytes:Buffer.byteLength(html),preview:path.join(out,'cartas-colecao.html'),catalog:'PASS',production_assets:'UNCHANGED'}));}
main().catch(e=>{console.error(e);process.exit(1)});
