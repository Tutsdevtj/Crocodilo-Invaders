const fs=require('node:fs'),path=require('node:path');
const sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));
const project=path.resolve(__dirname,'../..'),root=path.join(project,'SourceArt/CardCollection_v11_MouthFix');
const edits=[['AttackSpeed',1],['AttackSpeed',2],['MoveSpeed',1]];
async function main(){for(const[id,rank]of edits){await sharp(path.join(project,'Docs/ArtPreviews/CardCollection_v10',id,`base_${rank}.png`)).extract({left:32,top:52,width:116,height:94}).resize(696,564,{kernel:'nearest'}).png().toFile(path.join(root,`${id}_${rank}_target.png`));}}
main().catch(e=>{console.error(e);process.exit(1)});
