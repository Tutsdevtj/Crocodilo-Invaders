const fs=require('node:fs'),path=require('node:path');
const sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));
const project=path.resolve(__dirname,'../..'),root=path.join(project,'SourceArt/CardCollection_v11_MouthFix');
const ids=['AttackDamage','AttackSpeed','MaximumHealth','MoveSpeed','SpecialDamage'];
async function main(){fs.mkdirSync(root,{recursive:true});const tiles=[];for(let row=0;row<5;row++)for(let rank=0;rank<5;rank++){const input=await sharp(path.join(project,'Docs/ArtPreviews/CardCollection_v10',ids[row],`base_${rank}.png`)).extract({left:32,top:52,width:116,height:94}).resize(464,376,{kernel:'nearest'}).png().toBuffer();tiles.push({input,left:rank*464,top:row*376});}await sharp({create:{width:2320,height:1880,channels:4,background:'#101923'}}).composite(tiles).png().toFile(path.join(root,'mouth_audit_before.png'));}
main().catch(e=>{console.error(e);process.exit(1)});
