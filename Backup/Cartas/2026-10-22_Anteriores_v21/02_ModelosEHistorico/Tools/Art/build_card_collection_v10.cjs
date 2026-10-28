// Mythic revision only. v09 and approved snapshots remain intact.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));
const {renderLabels}=require('./card_typography_v08.cjs');
const {inside,overlayWeapons}=require('./build_foil_preview_v06.cjs');
const project=path.resolve(__dirname,'../..'),root=path.join(project,'SourceArt/CardCollection_v10_MythicRed'),previous=path.join(project,'Docs/ArtPreviews/CardCollection_v09'),out=path.join(project,'Docs/ArtPreviews/CardCollection_v10');
const ids=['AttackDamage','AttackSpeed','MaximumHealth','MoveSpeed','SpecialDamage'];
const ranks=['Common','Rare','Epic','Legendary','Mythic'],names=['COMUM','RARO','ÉPICO','LENDÁRIO','MÍTICO'];
const manifest=JSON.parse(fs.readFileSync(path.join(project,'SourceArt/CardCollection_v09/manifest.json'),'utf8'));
const W=180,H=268,hash=b=>crypto.createHash('sha256').update(b).digest('hex');
const png=b=>sharp(b,{raw:{width:W,height:H,channels:4}}).png().toBuffer();
const head=[[25,82],[21,66],[29,48],[45,37],[66,31],[92,32],[113,40],[125,53],[123,67],[140,70],[158,80],[164,93],[156,108],[136,121],[105,127],[74,115],[48,102],[34,95]];
const armor=[[0,79],[22,80],[41,96],[72,113],[111,119],[143,128],[180,139],[180,213],[137,230],[78,229],[34,213],[0,190]];
async function mythic(f,index){
 const meta=await sharp(path.join(root,'mythic_red_atlas.png')).metadata(),col=index%3,row=Math.floor(index/3),left=Math.floor(col*meta.width/3),top=Math.floor(row*meta.height/2),width=Math.floor((col+1)*meta.width/3)-left,height=Math.floor((row+1)*meta.height/2)-top;
 const art=await sharp(path.join(root,'mythic_red_atlas.png')).extract({left,top,width,height}).resize(W,H,{fit:'fill',kernel:'nearest'}).ensureAlpha().raw().toBuffer();
 fs.mkdirSync(path.join(root,f.id),{recursive:true});fs.writeFileSync(path.join(root,f.id,'art_4.png'),await png(art));
 const base=await sharp(path.join(project,'SourceArt/CardsCroc_v03/master_4.png')).ensureAlpha().raw().toBuffer();
 for(let y=48;y<208;y++)for(let x=156;x<W;x++){const p=(y*W+x)*4,q=(y*W+W-1-x)*4;base.copy(base,p,q,q+4);}
 const scaled=await sharp(art,{raw:{width:W,height:H,channels:4}}).resize(146,190,{fit:'fill',kernel:'nearest'}).raw().toBuffer(),aligned=Buffer.alloc(W*H*4);
 for(let y=37;y<227;y++)for(let x=17;x<163;x++){const p=(y*W+x)*4,q=((y-37)*146+x-17)*4;scaled.copy(aligned,p,q,q+4);if(x>=24&&x<156&&y>=48&&y<208)aligned.copy(base,p,p,p+4);}
 // A restrained shoulder overlap, only 7px beyond the art ring, never across text.
 const polygons=[[[17,53],[35,53],[42,81],[34,104],[17,104]],[[144,54],[162,54],[162,103],[146,103]]];
 const projection=overlayWeapons(base,aligned,polygons,true,true),mask=Buffer.alloc(W*H*4);
 for(let y=0;y<H;y++)for(let x=0;x<W;x++){const p=(y*W+x)*4,picture=x>=24&&x<156&&y>=48&&y<208,plate=x>25&&x<155&&(y>11&&y<42||y>214&&y<254),sx=(x-17)*180/146,sy=(y-37)*268/190;
 mask[p]=plate?0:95;mask[p+1]=plate?0:picture||projection[y*W+x]?128:255;mask[p+2]=picture&&(inside(sx,sy,head)||inside(sx,sy,armor))||projection[y*W+x]?255:0;mask[p+3]=255;}
 return {base:await png(base),materials:await png(mask),labels:await renderLabels(4,{title:f.title,attribute:f.attribute,percent:f.values[4]})};
}
async function main(){fs.mkdirSync(out,{recursive:true});const collection=[],checks=[];
 for(let index=0;index<ids.length;index++){const f=manifest.families.find(f=>f.id===ids[index]),dir=path.join(out,f.id);fs.mkdirSync(dir,{recursive:true});const cards=[];
 for(let rank=0;rank<5;rank++){const layers=rank===4?await mythic(f,index):Object.fromEntries(['base','materials','labels'].map(key=>[key,fs.readFileSync(path.join(previous,f.id,`${key}_${rank}.png`))]));
 for(const[key,value]of Object.entries(layers)){fs.writeFileSync(path.join(dir,`${key}_${rank}.png`),value);if(rank<4||key==='labels'){if(hash(value)!==hash(fs.readFileSync(path.join(previous,f.id,`${key}_${rank}.png`))))throw Error('Preserved layer changed '+f.id+key+rank);}}
 const face=await sharp(layers.base).resize(360,536,{kernel:'nearest'}).composite([{input:layers.labels}]).png().toBuffer();fs.writeFileSync(path.join(dir,ranks[rank]+'.png'),face);
 if(rank<4&&hash(face)!==hash(fs.readFileSync(path.join(previous,f.id,ranks[rank]+'.png'))))throw Error('Other rarity changed '+f.id+rank);
 const inlineBase=await sharp(layers.base).webp({quality:90,effort:6}).toBuffer();cards.push({rank,name:names[rank],base:'data:image/webp;base64,'+inlineBase.toString('base64'),materials:'data:image/png;base64,'+layers.materials.toString('base64'),labels:'data:image/png;base64,'+layers.labels.toString('base64')});checks.push({family:f.id,rank:ranks[rank],sha256:hash(face)});}
 collection.push({id:f.id,label:f.label,title:f.title,cards});}
 const shared=fs.readFileSync(path.join(__dirname,'foil_preview_v06.html'),'utf8');let shader=shared.slice(shared.indexOf('const vert='),shared.indexOf('function shader('));
 shader=shader.replace('frameFinish);holo=1.;','frameFinish);holo=step(.1,mat.g);');
 shader=shader.replace('float strength=holo*(rank>3.5?mix(.95,.48,foreground):.86);','if(rank>3.5){float redSheen=rainbow.r;rainbow=mix(vec3(.72,.025,.055),vec3(1.,.34,.29),redSheen);}\n  float strength=holo*(rank>3.5?mix(.76,.18,foreground):.86);');
 shader=shader.replace('pattern*sheen*.16*mix(1.,.4,foreground)','pattern*sheen*.075*mix(1.,.25,foreground)').replace('c+=fine*strength*.045','c+=fine*strength*.018');
 const html=fs.readFileSync(path.join(__dirname,'card_collection_v09.html'),'utf8').replace('__COLLECTION_DATA__',JSON.stringify(collection)).replace('__SHADER_CODE__',shader).replace('Arte prismática · moldura holográfica escovada','Carmesim prismático · moldura foil escovada');
 if(Buffer.byteLength(html)>1000000)throw Error('Preview exceeds 1MB');fs.writeFileSync(path.join(out,'cartas-miticas-vermelhas.html'),html);
 fs.writeFileSync(path.join(root,'manifest.json'),JSON.stringify({version:10,change:'Five mythics: crimson palette, simplified artwork, restrained red foil, limited shoulder overlap',unchanged_cards:20,unchanged_labels:25,cards:checks},null,2));
 console.log(JSON.stringify({cards:25,mythics_updated:5,other_cards:'20 UNCHANGED',all_text_layers:'UNCHANGED',inline_bytes:Buffer.byteLength(html),output:out}));
}
main().catch(e=>{console.error(e);process.exit(1)});
