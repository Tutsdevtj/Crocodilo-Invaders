// Gradual expressions in rare/epic/legendary. Common and mythic preserved.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));
const {inside}=require('./build_foil_preview_v06.cjs');
const project=path.resolve(__dirname,'../..'),root=path.join(project,'SourceArt/CardCollection_v12_Expressions'),previous=path.join(project,'Docs/ArtPreviews/CardCollection_v11'),out=path.join(project,'Docs/ArtPreviews/CardCollection_v12');
const families=JSON.parse(fs.readFileSync(path.join(project,'SourceArt/CardCollection_v09/manifest.json'),'utf8')).families;
const ranks=['Common','Rare','Epic','Legendary','Mythic'],names=['COMUM','RARO','ÉPICO','LENDÁRIO','MÍTICO'];
const masks={
 AttackDamage:[[26,40],[40,31],[65,31],[86,40],[92,53],[116,64],[116,113],[78,108],[47,96],[24,80],[20,57]],
 AttackSpeed:[[32,37],[49,31],[68,33],[79,44],[98,52],[106,59],[106,94],[73,100],[43,88],[30,71],[26,52]],
 MaximumHealth:[[30,33],[48,27],[67,30],[77,41],[98,51],[107,59],[107,93],[74,99],[42,86],[28,67],[25,48]],
 MoveSpeed:[[41,32],[55,28],[73,31],[84,40],[99,47],[105,55],[105,83],[75,88],[48,77],[38,59],[36,43]],
 SpecialDamage:[[31,29],[48,25],[66,28],[78,38],[99,48],[108,56],[108,89],[73,95],[42,83],[29,65],[25,45]]
};
const hash=b=>crypto.createHash('sha256').update(b).digest('hex');
// Rares keep the anatomically corrected jaw: anger begins in the brow and eyes.
const rareMasks={
 AttackDamage:[[26,40],[40,31],[65,31],[86,40],[92,53],[116,64],[116,83],[90,76],[60,62],[40,50],[26,48]],
 AttackSpeed:[[32,37],[49,31],[68,33],[79,44],[98,52],[104,59],[100,69],[78,61],[54,51],[34,48]],
 MaximumHealth:[[30,33],[48,27],[67,30],[77,41],[98,51],[104,58],[101,67],[76,58],[52,47],[32,44]],
 MoveSpeed:[[41,32],[55,28],[73,31],[84,40],[99,47],[103,53],[100,61],[77,53],[55,43],[41,42]],
 SpecialDamage:[[31,29],[48,25],[66,28],[78,38],[99,48],[105,55],[102,64],[78,55],[53,44],[32,41]]
};
async function main(){fs.mkdirSync(out,{recursive:true});const collection=[],audit=[];for(const f of families){const dir=path.join(out,f.id);fs.mkdirSync(dir,{recursive:true});const cards=[];
 for(let rank=0;rank<5;rank++){let base=fs.readFileSync(path.join(previous,f.id,`base_${rank}.png`));const original=await sharp(base).ensureAlpha().raw().toBuffer();let changed=0;
 if(rank>0&&rank<4){const file=path.join(root,`${f.id}_edited.png`),m=await sharp(file).metadata(),left=Math.floor((rank-1)*m.width/3),width=Math.floor(rank*m.width/3)-left;
 const corrected=await sharp(file).extract({left,top:0,width,height:m.height}).resize(116,116,{fit:'fill',kernel:'nearest'}).ensureAlpha().raw().toBuffer(),patched=Buffer.from(original),polygon=rank===1?rareMasks[f.id]:masks[f.id];
 for(let y=0;y<116;y++)for(let x=0;x<116;x++){if(!inside(x+.5,y+.5,polygon))continue;const p=((y+44)*180+x+32)*4,q=(y*116+x)*4;corrected.copy(patched,p,q,q+4);}
 for(let y=0;y<268;y++)for(let x=0;x<180;x++){const p=(y*180+x)*4;if(!original.subarray(p,p+4).equals(patched.subarray(p,p+4))){changed++;if(!inside(x-32+.5,y-44+.5,polygon))throw Error('Change outside expression mask');}}
 if(changed<100)throw Error('Missing expression '+f.id+rank);base=await sharp(patched,{raw:{width:180,height:268,channels:4}}).png().toBuffer();}
 const materials=fs.readFileSync(path.join(previous,f.id,`materials_${rank}.png`)),labels=fs.readFileSync(path.join(previous,f.id,`labels_${rank}.png`));
 for(const[key,value]of Object.entries({base,materials,labels}))fs.writeFileSync(path.join(dir,`${key}_${rank}.png`),value);
 const face=await sharp(base).resize(360,536,{kernel:'nearest'}).composite([{input:labels}]).png().toBuffer();fs.writeFileSync(path.join(dir,ranks[rank]+'.png'),face);
 if((rank===0||rank===4)&&hash(face)!==hash(fs.readFileSync(path.join(previous,f.id,ranks[rank]+'.png'))))throw Error('Endpoint changed');
 const inline=await sharp(base).webp({quality:90,effort:6}).toBuffer();cards.push({rank,name:names[rank],base:'data:image/webp;base64,'+inline.toString('base64'),materials:'data:image/png;base64,'+materials.toString('base64'),labels:'data:image/png;base64,'+labels.toString('base64')});audit.push({family:f.id,rank:ranks[rank],changed_pixels:changed,sha256:hash(face)});}
 collection.push({id:f.id,label:f.label,title:f.title,cards});}
 const oldHtml=fs.readFileSync(path.join(previous,'cartas-bocas-corrigidas.html'),'utf8'),shader=oldHtml.slice(oldHtml.indexOf('const vert='),oldHtml.indexOf('function shader('));
 const html=fs.readFileSync(path.join(__dirname,'card_collection_v09.html'),'utf8').replace('__COLLECTION_DATA__',JSON.stringify(collection)).replace('__SHADER_CODE__',shader).replace('Arte prismática · moldura holográfica escovada','Carmesim prismático · moldura foil escovada');
 if(Buffer.byteLength(html)>1000000)throw Error('Preview exceeds 1MB');fs.writeFileSync(path.join(out,'cartas-expressoes-progressivas.html'),html);
 fs.writeFileSync(path.join(root,'manifest.json'),JSON.stringify({version:12,expression_sequence:['Confiante','Irritado / mandíbula cerrada','Furioso / rosnado aberto','Quase insano / berro','Insano / gargalhada aberta'],masks,rareMasks,unchanged_cards:10,unchanged_labels:25,unchanged_materials:25,shader:'Identical to v11',cards:audit},null,2));
 console.log(JSON.stringify({changed_cards:15,unchanged_endpoints:10,inline_bytes:Buffer.byteLength(html)}));}
main().catch(e=>{console.error(e);process.exit(1)});
