const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
let sharp;try{sharp=require('sharp');}catch{sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));}
const root=path.resolve(__dirname,'../..'),source=path.join(root,'SourceArt/AchievementIcons_v03'),output=path.join(root,'Assets/Resources/Achievements/Icons_v03'),preview=path.join(root,'Docs/ArtPreviews/AchievementIcons_v03');
const order=['first_wave','sport_mode','boss_special','clean_wave','gusini_defeated','macaquini_defeated','sapeneu_defeated','all_card_kinds','first_mythic','last_tooth'];
const labels=[['Primeiro','Expediente'],['Modo','Esportivo'],['Bafo de','Respeito'],['Casco','Intacto'],['Ganso','Depenado'],['Descascou','o Problema'],['Borracha','Furada'],['Carteira','Assinada'],['Hoje Eu','Tô Mítico'],['Por Um','Dente']];
const hex=['0b1018','101b26','213443','3d5464','575a62','7b7e86','b6bbc0','f1ead8','fff8df',
 '26381e','3d5125','637734','879744','b4ba67','1b4f2a','367a3d','64a33a','9bc94c',
 '75552b','b38b4b','e9c97b','8b601c','c69425','ecc445','fff0a6','5b1c28','972934','d94539','f17150',
 '362148','613178','9255ad','c893cf','123e54','1e6e8b','22afc0','62e6e5','bafaff','114b92','207bbd','46b9ee',
 '3f2c25','70503a','a77750','d4a77b'];
const palette=hex.map(h=>[0,2,4].map(i=>parseInt(h.slice(i,i+2),16)));
const hash=b=>crypto.createHash('sha256').update(b).digest('hex');
async function main(){
 fs.mkdirSync(output,{recursive:true});fs.mkdirSync(preview,{recursive:true});fs.mkdirSync(path.join(source,'Sources'),{recursive:true});
 const inputs=JSON.parse(fs.readFileSync(path.join(source,'generations.json'),'utf8'));
 const manifest={mode:'built-in image_gen; nearest-neighbor export and shared pixel palette',size:[64,64],palette:hex,icons:[]};
 const layers=[];
 for(let index=0;index<order.length;index++){
  const id=order[index],entry=inputs.find(e=>e.id===id);if(!entry)throw Error('Missing source '+id);
  const local=path.join(source,'Sources',id+'.png');if(!fs.existsSync(local))fs.copyFileSync(entry.path,local);
  const original=fs.readFileSync(local),raw=await sharp(original).resize(64,64,{kernel:'nearest'}).removeAlpha().raw().toBuffer();
  const pixels=Buffer.alloc(64*64*4);
  for(let y=0;y<64;y++)for(let x=0;x<64;x++){
   const i=y*64+x;let best=palette[0],distance=Infinity;
   for(let p=0;p<palette.length;p++){
    if(id==='macaquini_defeated'&&raw[i*3]>raw[i*3+1]*1.55&&raw[i*3+1]>raw[i*3+2]*1.15&&p>=25&&p<=28)continue;
    const color=palette[p],d=(color[0]-raw[i*3])**2*2+(color[1]-raw[i*3+1])**2*3+(color[2]-raw[i*3+2])**2;if(d<distance){distance=d;best=color;}
   }
   if(x===0||y===0||x===63||y===63)best=palette[3];
   for(let c=0;c<3;c++)pixels[i*4+c]=best[c];pixels[i*4+3]=255;
  }
  const png=await sharp(pixels,{raw:{width:64,height:64,channels:4}}).png().toBuffer();fs.writeFileSync(path.join(output,id+'.png'),png);
  const colors=new Set();for(let i=0;i<pixels.length;i+=4)colors.add(pixels.subarray(i,i+3).toString('hex'));
  manifest.icons.push({id,source:'Sources/'+id+'.png',sourceSha256:hash(original),sha256:hash(png),colors:colors.size});
  const left=(index%5)*200+36,top=Math.floor(index/5)*220+16;
  layers.push({input:await sharp(png).resize(128,128,{kernel:'nearest'}).png().toBuffer(),left,top});
  const title=`<svg width="200" height="62"><style>text{font-family:Segoe UI,Arial;fill:#eee9d2;font-size:18px;text-anchor:middle}</style><text x="100" y="23">${labels[index][0]}</text><text x="100" y="47">${labels[index][1]}</text></svg>`;
  layers.push({input:Buffer.from(title),left:(index%5)*200,top:top+136});
 }
 if(new Set(manifest.icons.map(e=>e.sha256)).size!==10)throw Error('Achievement icons must all be distinct');
 fs.writeFileSync(path.join(source,'manifest.json'),JSON.stringify(manifest,null,2)+'\n');
 await sharp({create:{width:1000,height:440,channels:4,background:'#101820'}}).composite(layers).png().toFile(path.join(preview,'achievement-icons.png'));
 console.log('ACHIEVEMENT_ICON_EXPORT_OK: ten distinct 64x64 PNGs, shared palette, original generation sources preserved.');
}
main().catch(e=>{console.error(e);process.exitCode=1;});
