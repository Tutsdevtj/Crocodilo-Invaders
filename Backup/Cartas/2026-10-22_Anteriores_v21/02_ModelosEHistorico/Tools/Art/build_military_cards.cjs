// Export the generated atlas into native pixel cards. No runtime dependencies.
const fs = require('node:fs'), path = require('node:path'), crypto = require('node:crypto');
let sharp; try { sharp = require('sharp'); } catch { sharp = require(path.join(process.env.USERPROFILE, '.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp')); }
const project = path.resolve(__dirname, '../..');
const source = path.join(project, 'SourceArt/CardsMilitary_v01');
const preview = path.join(project, 'Docs/ArtPreviews/CardsMilitary_v01');
const W = 180, H = 268;
// Hand-set 5x7 bitmap type, including Portuguese accents; no font rasterizer.
const glyphs = {
 A:['01110','10001','10001','11111','10001','10001','10001'], B:['11110','10001','10001','11110','10001','10001','11110'],
 C:['01111','10000','10000','10000','10000','10000','01111'], D:['11110','10001','10001','10001','10001','10001','11110'],
 E:['11111','10000','10000','11110','10000','10000','11111'], F:['11111','10000','10000','11110','10000','10000','10000'],
 G:['01111','10000','10000','10111','10001','10001','01111'], H:['10001','10001','10001','11111','10001','10001','10001'],
 I:['11111','00100','00100','00100','00100','00100','11111'], J:['00111','00010','00010','00010','10010','10010','01100'],
 L:['10000','10000','10000','10000','10000','10000','11111'], M:['10001','11011','10101','10101','10001','10001','10001'],
 N:['10001','11001','11001','10101','10011','10011','10001'], O:['01110','10001','10001','10001','10001','10001','01110'],
 P:['11110','10001','10001','11110','10000','10000','10000'], Q:['01110','10001','10001','10001','10101','10010','01101'],
 R:['11110','10001','10001','11110','10100','10010','10001'], S:['01111','10000','10000','01110','00001','00001','11110'],
 T:['11111','00100','00100','00100','00100','00100','00100'], U:['10001','10001','10001','10001','10001','10001','01110'],
 V:['10001','10001','10001','10001','10001','01010','00100'], X:['10001','10001','01010','00100','01010','10001','10001'],
 '0':['01110','10001','10011','10101','11001','10001','01110'], '1':['00100','01100','00100','00100','00100','00100','01110'],
 '2':['01110','10001','00001','00010','00100','01000','11111'], '3':['11110','00001','00001','01110','00001','00001','11110'],
 '4':['00010','00110','01010','10010','11111','00010','00010'], '5':['11111','10000','10000','11110','00001','00001','11110'],
 '6':['01110','10000','10000','11110','10001','10001','01110'], '7':['11111','00001','00010','00100','01000','01000','01000'],
 '8':['01110','10001','10001','01110','10001','10001','01110'], '9':['01110','10001','10001','01111','00001','00001','01110'],
 '+':['00000','00100','00100','11111','00100','00100','00000'], '%':['11001','11010','00100','00100','01000','10110','00110'],
 '.':['00000','00000','00000','00000','00000','00110','00110'], '-':['00000','00000','00000','11111','00000','00000','00000'],
 ' ':['00000','00000','00000','00000','00000','00000','00000']
};
const colors = {ink:'#08131f', navy:'#102337', mid:'#18394c', blue:'#265669', ivory:'#f5e8bc', gold:'#e9b542', dim:'#91abae'};
const rarities = [ ['Common','COMUM','#b0c5c9'], ['Rare','RARO','#45b9f2'], ['Epic','ÉPICO','#bb81f0'], ['Legendary','LENDÁRIO','#ffc64b'], ['Mythic','MÍTICO','#ff6950'] ];
const families = [
 ['AttackDamage','MUNIÇÃO','DANO DE ATAQUE',[5,10,15,20,30]],
 ['AttackSpeed','RAJADA','CADÊNCIA',[5,10,15,20,30]],
 ['MaximumHealth','BLINDAGEM','VIDA MÁXIMA',[10,15,20,30,40]],
 ['MoveSpeed','PROPULSÃO','MOVIMENTO',[3,5,8,10,15]],
 ['SpecialDamage','ESPECIAL','DANO ESPECIAL',[5,10,15,20,30]]
];
const rgb = c => c.slice(1).match(/../g).map(v => parseInt(v,16));
function rect(b,x,y,w,h,c){const p=rgb(c);for(let yy=y;yy<y+h;yy++)for(let xx=x;xx<x+w;xx++){if(xx<0||xx>=W||yy<0||yy>=H)throw Error('Drawing outside card');const i=(yy*W+xx)*4;b[i]=p[0];b[i+1]=p[1];b[i+2]=p[2];b[i+3]=255;}}
function text(b,label,y,s,c){let x=Math.round((W-(label.length*6-1)*s)/2);for(const ch of label){const base=ch.normalize('NFD')[0],g=glyphs[base];if(!g)throw Error('Missing glyph '+ch);g.forEach((row,yy)=>[...row].forEach((v,xx)=>{if(v==='1')rect(b,x+xx*s,y+yy*s,s,s,c);}));if(ch==='Ã'){rect(b,x+s,y-3*s,s,s,c);rect(b,x+2*s,y-4*s,s,s,c);rect(b,x+3*s,y-3*s,s,s,c);}else if(ch==='Ç'){rect(b,x+2*s,y+7*s,s,s,c);rect(b,x+s,y+8*s,s,s,c);}else if(ch!==base){rect(b,x+2*s,y-2*s,s,s,c);rect(b,x+3*s,y-3*s,s,s,c);}x+=6*s;}}
function frame(accent,rank){const b=Buffer.alloc(W*H*4);rect(b,3,0,W-6,H,colors.ink);rect(b,0,3,W,H-6,colors.ink);rect(b,4,3,W-8,H-6,colors.gold);rect(b,3,4,W-6,H-8,colors.gold);rect(b,6,6,W-12,H-12,colors.navy);rect(b,9,9,W-18,H-18,colors.blue);rect(b,10,10,W-20,H-20,colors.navy);rect(b,12,12,W-24,18,accent);rect(b,12,31,W-24,23,colors.mid);rect(b,12,56,W-24,146,colors.ink);rect(b,14,58,W-28,142,colors.navy);rect(b,12,204,W-24,2,colors.gold);rect(b,12,248,W-24,8,colors.mid);
 for(const x of [7,W-10])for(const y of [7,H-10]){rect(b,x,y,3,3,colors.ivory);rect(b,x+1,y+1,1,1,colors.ink);}
 for(let i=0;i<=rank;i++){const x=Math.round(W/2-(rank+1)*7/2+i*7);rect(b,x,251,4,2,accent);}
 return b;}
async function png(b){return sharp(b,{raw:{width:W,height:H,channels:4}}).png().toBuffer();}
function importRect(file){const meta=file+'.meta';let s=fs.readFileSync(meta,'utf8');s=s.replace(/(rect:\r?\n\s+serializedVersion: 2\r?\n)\s+x: \d+\r?\n\s+y: \d+\r?\n\s+width: \d+\r?\n\s+height: \d+/, '$1        x: 0\n        y: 0\n        width: 180\n        height: 268');fs.writeFileSync(meta,s);}
async function main(){fs.mkdirSync(source,{recursive:true});fs.mkdirSync(preview,{recursive:true});const input=process.argv[2];const atlas=path.join(source,'equipment_atlas_generated.png');if(input)fs.copyFileSync(input,atlas);if(!fs.existsSync(atlas))throw Error('Pass generated atlas path on first run');
 const srcMeta=await sharp(atlas).metadata();if(srcMeta.width!==1536||srcMeta.height!==1024)throw Error('Expected 3x2 atlas of 512px cells');
 const tiles=[];for(let i=0;i<6;i++){// Export at 64px native, fixed palette, enlarge only by integer factors.
 const tile=await sharp(atlas).extract({left:i%3*512,top:Math.floor(i/3)*512,width:512,height:512}).resize(64,64,{kernel:'nearest'}).png({palette:true,colours:32,dither:0}).toBuffer();fs.writeFileSync(path.join(source,`icon_${i}.png`),tile);tiles.push(await sharp(tile).resize(128,128,{kernel:'nearest'}).png().toBuffer());}
 const records=[],contact=[];
 for(let k=0;k<5;k++)for(let r=0;r<5;r++){const [kind,title,stat,values]=families[k],[rarity,label,accent]=rarities[r];const dest=path.join(project,`Assets/Resources/Cards/${kind}/${rarity}.png`),backup=path.join(source,`originals/${kind}/${rarity}.png`);fs.mkdirSync(path.dirname(backup),{recursive:true});if(!fs.existsSync(backup))fs.copyFileSync(dest,backup);
 const b=frame(accent,r);text(b,label,18,1,colors.ink);text(b,title,38,2,colors.ivory);text(b,stat,212,1,colors.dim);text(b,`+${values[r]}%`,225,3,colors.ivory);
 const out=await sharp(await png(b)).composite([{input:tiles[k],left:26,top:65}]).png().toBuffer();fs.writeFileSync(dest,out);importRect(dest);records.push({kind,rarity,percent:values[r],file:path.relative(project,dest).replaceAll('\\','/'),sha256:crypto.createHash('sha256').update(out).digest('hex')});contact.push({input:out,left:r*(W+12)+12,top:k*(H+24)+12});
 }
 const backDest=path.join(project,'Assets/Art/Cards/Back.png'),oldBack=path.join(source,'originals/Back.png');if(!fs.existsSync(oldBack))fs.copyFileSync(backDest,oldBack);
 const b=frame(colors.gold,4);text(b,'B. CROCODILO',18,1,colors.ink);text(b,'INVADERS',38,2,colors.ivory);text(b,'COMANDO AÉREO',213,1,colors.ivory);text(b,'MELHORIA DE CAMPO',231,1,colors.dim);const back=await sharp(await png(b)).composite([{input:tiles[5],left:26,top:65}]).png().toBuffer();fs.writeFileSync(backDest,back);importRect(backDest);
 await sharp({create:{width:5*(W+12)+12,height:5*(H+24),channels:4,background:'#08131f'}}).composite(contact).png().toFile(path.join(preview,'all_25_cards.png'));
 const showcase=[0,6,12,18,24].map((j,i)=>({input:contact[j].input,left:12+i*192,top:12}));showcase.push({input:back,left:12+5*192,top:12});
 const strip=await sharp({create:{width:1164,height:292,channels:4,background:'#08131f'}}).composite(showcase).png().toBuffer();
 await sharp(strip).resize(2328,584,{kernel:'nearest'}).png().toFile(path.join(preview,'five_families_and_back_x2.png'));
 const manifest={version:1,canvas:[W,H],icon_native:[64,64],icon_display_scale:2,rarities:rarities.map(([id,label,color],rank)=>({id,label,color,insignia_marks:rank+1})),cards:records,back:'Assets/Art/Cards/Back.png',source:'AI-generated atlas, referenced to project Crocodilo portrait and navy/gold UI. Bitmap typography and framing composed by build_military_cards.cjs.'};fs.writeFileSync(path.join(source,'manifest.json'),JSON.stringify(manifest,null,2));
 // Check the real catalog and the serialized crop/GUIDs used by the existing UI.
 const catalog=fs.readFileSync(path.join(project,'Assets/Data/UpgradeCards.asset'),'utf8');
 for(const card of records){const entry=new RegExp('percent: '+card.percent+'\\s+artworkPath: Cards/'+card.kind+'/'+card.rarity+'(?:\\r?\\n|$)');if(!entry.test(catalog))throw Error('Catalog value mismatch '+card.file);}
 for(const file of [...records.map(r=>path.join(project,r.file)),backDest]){const m=await sharp(file).metadata();if(m.width!==W||m.height!==H)throw Error('Invalid dimensions '+file);const meta=fs.readFileSync(file+'.meta','utf8');if(!/x: 0\s+y: 0\s+width: 180\s+height: 268/.test(meta)||!meta.includes('filterMode: 0')||!meta.includes('enableMipMap: 0'))throw Error('Invalid import '+file);}
 console.log(JSON.stringify({cards:records.length,unique_images:new Set(records.map(r=>r.sha256)).size,canvas:[W,H],catalog_and_imports:'PASS',preview:path.join(preview,'five_families_and_back_x2.png')},null,2));}
module.exports = { glyphs };
if (require.main === module) main().catch(e=>{console.error(e);process.exit(1);});
