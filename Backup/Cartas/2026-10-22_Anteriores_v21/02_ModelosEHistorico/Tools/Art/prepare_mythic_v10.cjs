const fs=require('node:fs'),path=require('node:path');
const sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));
const project=path.resolve(__dirname,'../..'),root=path.join(project,'SourceArt/CardCollection_v10_MythicRed');
const ids=['AttackDamage','AttackSpeed','MaximumHealth','MoveSpeed','SpecialDamage'];
async function main(){fs.mkdirSync(root,{recursive:true});const images=[];for(let i=0;i<ids.length;i++){const file=i===0?path.join(project,'SourceArt/CardsModel_Approved_v08_20261021/sources/art_4.png'):path.join(project,'SourceArt/CardCollection_v09',ids[i],'art_4.png');images.push({input:await sharp(file).resize(360,536,{fit:'cover',kernel:'nearest'}).png().toBuffer(),left:i%3*360,top:Math.floor(i/3)*536});}await sharp({create:{width:1080,height:1072,channels:4,background:'#14212c'}}).composite(images).png().toFile(path.join(root,'mythic_edit_target.png'));}
main().catch(e=>{console.error(e);process.exit(1)});
