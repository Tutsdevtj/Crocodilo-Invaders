// Three local mouth corrections; all other pixels, labels and materials preserved.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));
const {inside}=require('./build_foil_preview_v06.cjs');
const project=path.resolve(__dirname,'../..'),root=path.join(project,'SourceArt/CardCollection_v11_MouthFix'),previous=path.join(project,'Docs/ArtPreviews/CardCollection_v10'),out=path.join(project,'Docs/ArtPreviews/CardCollection_v11');
const families=JSON.parse(fs.readFileSync(path.join(project,'SourceArt/CardCollection_v09/manifest.json'),'utf8')).families;
const ranks=['Common','Rare','Epic','Legendary','Mythic'],names=['COMUM','RARO','ÉPICO','LENDÁRIO','MÍTICO'];
const edits=[
 {id:'AttackSpeed',rank:1,polygon:[[33,37],[44,39],[50,47],[66,52],[92,58],[97,56],[97,67],[90,72],[64,68],[57,78],[34,75],[31,53]]},
 {id:'AttackSpeed',rank:2,polygon:[[33,37],[44,39],[50,47],[66,52],[92,58],[97,56],[97,67],[90,72],[64,68],[57,78],[34,75],[31,53]]},
 {id:'MoveSpeed',rank:1,polygon:[[43,29],[51,33],[58,39],[75,44],[96,49],[98,54],[91,59],[69,55],[62,60],[45,58],[42,43]]}
];
const hash=b=>crypto.createHash('sha256').update(b).digest('hex');
async function main(){fs.mkdirSync(out,{recursive:true});const collection=[],audit=[];
 for(const f of families){const dir=path.join(out,f.id);fs.mkdirSync(dir,{recursive:true});const cards=[];
 for(let rank=0;rank<5;rank++){let base=fs.readFileSync(path.join(previous,f.id,`base_${rank}.png`));const original=await sharp(base).ensureAlpha().raw().toBuffer(),edit=edits.find(e=>e.id===f.id&&e.rank===rank);let changed=0;
 if(edit){const corrected=await sharp(path.join(root,`${f.id}_${rank}_edited.png`)).resize(116,94,{fit:'fill',kernel:'nearest'}).ensureAlpha().raw().toBuffer(),patched=Buffer.from(original);
 for(let y=0;y<94;y++)for(let x=0;x<116;x++){if(!inside(x+.5,y+.5,edit.polygon))continue;const p=((y+52)*180+x+32)*4,q=(y*116+x)*4;corrected.copy(patched,p,q,q+4);}
 for(let y=0;y<268;y++)for(let x=0;x<180;x++){const p=(y*180+x)*4;if(!original.subarray(p,p+4).equals(patched.subarray(p,p+4))){changed++;if(!inside(x-32+.5,y-52+.5,edit.polygon))throw Error('Change outside mouth');}}
 if(changed<40)throw Error('Missing mouth correction '+f.id+rank);base=await sharp(patched,{raw:{width:180,height:268,channels:4}}).png().toBuffer();
 await sharp(base).extract({left:32,top:52,width:116,height:94}).resize(696,564,{kernel:'nearest'}).png().toFile(path.join(root,`${f.id}_${rank}_after.png`));}
 const materials=fs.readFileSync(path.join(previous,f.id,`materials_${rank}.png`)),labels=fs.readFileSync(path.join(previous,f.id,`labels_${rank}.png`));
 for(const[key,value]of Object.entries({base,materials,labels}))fs.writeFileSync(path.join(dir,`${key}_${rank}.png`),value);
 const face=await sharp(base).resize(360,536,{kernel:'nearest'}).composite([{input:labels}]).png().toBuffer();fs.writeFileSync(path.join(dir,ranks[rank]+'.png'),face);
 if(!edit&&hash(face)!==hash(fs.readFileSync(path.join(previous,f.id,ranks[rank]+'.png'))))throw Error('Unrelated card changed');
 const inline=await sharp(base).webp({quality:90,effort:6}).toBuffer();cards.push({rank,name:names[rank],base:'data:image/webp;base64,'+inline.toString('base64'),materials:'data:image/png;base64,'+materials.toString('base64'),labels:'data:image/png;base64,'+labels.toString('base64')});audit.push({family:f.id,rank:ranks[rank],changed_pixels:changed,sha256:hash(face)});}
 collection.push({id:f.id,label:f.label,title:f.title,cards});}
 const oldHtml=fs.readFileSync(path.join(previous,'cartas-miticas-vermelhas.html'),'utf8'),shader=oldHtml.slice(oldHtml.indexOf('const vert='),oldHtml.indexOf('function shader('));
 const html=fs.readFileSync(path.join(__dirname,'card_collection_v09.html'),'utf8').replace('__COLLECTION_DATA__',JSON.stringify(collection)).replace('__SHADER_CODE__',shader).replace('Arte prismática · moldura holográfica escovada','Carmesim prismático · moldura foil escovada');
 if(Buffer.byteLength(html)>1000000)throw Error('Preview exceeds 1MB');fs.writeFileSync(path.join(out,'cartas-bocas-corrigidas.html'),html);fs.writeFileSync(path.join(root,'manifest.json'),JSON.stringify({version:11,edited:edits,unchanged_cards:22,unchanged_labels:25,unchanged_materials:25,shader:'Identical to v10',cards:audit},null,2));
 console.log(JSON.stringify({changed_cards:3,pixel_changes:audit.filter(a=>a.changed_pixels),unchanged_cards:22,inline_bytes:Buffer.byteLength(html)}));
}
main().catch(e=>{console.error(e);process.exit(1)});
