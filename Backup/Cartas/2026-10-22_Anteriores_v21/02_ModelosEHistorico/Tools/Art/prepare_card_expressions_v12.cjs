const fs=require('node:fs'),path=require('node:path');
const sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));
const project=path.resolve(__dirname,'../..'),root=path.join(project,'SourceArt/CardCollection_v12_Expressions'),ids=['AttackDamage','AttackSpeed','MaximumHealth','MoveSpeed','SpecialDamage'];
async function main(){fs.mkdirSync(root,{recursive:true});for(const id of ids){const tiles=[];for(let rank=1;rank<4;rank++){const input=await sharp(path.join(project,'Docs/ArtPreviews/CardCollection_v11',id,`base_${rank}.png`)).extract({left:32,top:44,width:116,height:116}).resize(464,464,{kernel:'nearest'}).png().toBuffer();tiles.push({input,left:(rank-1)*464,top:0});}await sharp({create:{width:1392,height:464,channels:4,background:'#101923'}}).composite(tiles).png().toFile(path.join(root,`${id}_target.png`));}}
main().catch(e=>{console.error(e);process.exit(1)});
