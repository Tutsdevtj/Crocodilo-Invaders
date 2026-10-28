// Apply approved static faces only. Dynamic foil integration is a later task.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));
const project=path.resolve(__dirname,'../..'),source=path.join(project,'Docs/ArtPreviews/CardCollection_v12'),recordRoot=path.join(project,'SourceArt/CardCollection_v12_Integration');
const hash=b=>crypto.createHash('sha256').update(b).digest('hex');
const identity=s=>({guid:s.match(/^guid: (.+)$/m)?.[1],spriteID:s.match(/^\s+spriteID: (\S+)/m)?.[1],fileID:s.match(/^\s+Card: (\d+)/m)?.[1]});
async function main(){
 const manifest=JSON.parse(fs.readFileSync(path.join(project,'SourceArt/CardCollection_v12_Expressions/manifest.json'),'utf8'));
 const catalog=fs.readFileSync(path.join(project,'Assets/Data/UpgradeCards.asset'));
 const values=JSON.parse(fs.readFileSync(path.join(project,'SourceArt/CardCollection_v09/manifest.json'),'utf8')).families;
 const prepared=[];
 for(const c of manifest.cards){const png=fs.readFileSync(path.join(source,c.family,c.rank+'.png')),relative=`Assets/Resources/Cards/${c.family}/${c.rank}.png`,file=path.join(project,relative),oldPng=fs.readFileSync(file),meta=fs.readFileSync(file+'.meta','utf8'),m=await sharp(png).metadata();
 if(hash(png)!==c.sha256||m.width!==360||m.height!==536)throw Error('Invalid approved image '+relative);
 const rank=['Common','Rare','Epic','Legendary','Mythic'].indexOf(c.rank),percent=values.find(f=>f.id===c.family).values[rank];
 if(!new RegExp('percent: '+percent+'\\s+artworkPath: Cards/'+c.family+'/'+c.rank+'(?:\\r?\\n|$)').test(catalog.toString()))throw Error('Catalog mismatch '+relative);
 const originalIdentity=identity(meta);if(Object.values(originalIdentity).some(v=>!v))throw Error('Missing sprite identity '+relative);
 let updated=meta.replace(/(rect:\r?\n\s+serializedVersion: 2\r?\n)\s+x: \d+\r?\n\s+y: \d+\r?\n\s+width: \d+\r?\n\s+height: \d+/,'$1        x: 0\n        y: 0\n        width: 360\n        height: 536');
 updated=updated.replace(/spritePixelsToUnits: \d+/,'spritePixelsToUnits: 200').replace(/maxTextureSize: \d+/g,'maxTextureSize: 1024').replace(/textureCompression: \d+/g,'textureCompression: 0');
 if(JSON.stringify(identity(updated))!==JSON.stringify(originalIdentity)||!/width: 360\s+height: 536/.test(updated)||!updated.includes('filterMode: 0')||!updated.includes('enableMipMap: 0'))throw Error('Invalid import '+relative);
 prepared.push({c,relative,file,png,oldPng,meta,updated,originalIdentity,percent});}
 if(prepared.length!==25)throw Error('Expected 25 cards');
 if(process.argv.includes('--check')){for(const p of prepared){if(hash(p.oldPng)!==hash(p.png)||p.meta!==p.updated)throw Error('Integration mismatch '+p.relative);}console.log(JSON.stringify({cards:25,resources:'PASS',catalog:'PASS',sprite_rects:'360x536',filter:'Point',mipmaps:false,compression:'None',pixels_per_unit:200,mode:'CHECK'}));return;}
 fs.mkdirSync(recordRoot,{recursive:true});const records=[];
 for(const p of prepared){const backup=path.join(recordRoot,'before',p.c.family,p.c.rank+'.png');fs.mkdirSync(path.dirname(backup),{recursive:true});if(!fs.existsSync(backup)){fs.writeFileSync(backup,p.oldPng);fs.writeFileSync(backup+'.meta.txt',p.meta);}
 fs.writeFileSync(p.file,p.png);fs.writeFileSync(p.file+'.meta',p.updated);
 records.push({family:p.c.family,rarity:p.c.rank,percent:p.percent,file:p.relative,sha256:hash(p.png),...p.originalIdentity,dimensions:[360,536],pixels_per_unit:200});}
 if(hash(catalog)!==hash(fs.readFileSync(path.join(project,'Assets/Data/UpgradeCards.asset'))))throw Error('Catalog unexpectedly changed');
 fs.writeFileSync(path.join(recordRoot,'manifest.json'),JSON.stringify({version:12,mode:'Static approved faces',dynamic_foil:'Deferred by user',catalog_sha256:hash(catalog),cards:records},null,2));
 console.log(JSON.stringify({cards:25,static_faces:'APPLIED',sprite_identifiers:'PRESERVED',catalog:'UNCHANGED',backup:recordRoot,dynamic_foil:'DEFERRED'}));
}
main().catch(e=>{console.error(e);process.exit(1)});
