const fs=require('node:fs'),path=require('node:path');
let code=fs.readFileSync(path.join(__dirname,'verify_card_collection_v09.cjs'),'utf8');
code=code.replace("out=path.join(project,'Docs/ArtPreviews/CardCollection_v09')","out=path.join(project,'Docs/ArtPreviews/CardCollection_v10')").replace("'cartas-colecao.html'","'cartas-miticas-vermelhas.html'");
const start=code.indexOf('for(let rank=0;rank<5;rank++)for(const part'),end=code.indexOf('if(errors.length)',start);
if(start<0||end<0)throw Error('Verifier anchor missing');
code=code.slice(0,start)+`for(const id of ids)for(let rank=0;rank<5;rank++)for(const part of ['base','materials','labels']){if(rank===4&&part!=='labels')continue;const name=part+'_'+rank+'.png';if(hash(fs.readFileSync(path.join(project,'Docs/ArtPreviews/CardCollection_v09',id,name)))!==hash(fs.readFileSync(path.join(out,id,name))))throw Error('Preserved layer changed '+id+name);}\n`+code.slice(end);
code=code.replace("approved_attack:'UNCHANGED'","other_rarities:'20 CARDS UNCHANGED',typography:'25 LABEL LAYERS UNCHANGED'");
fs.writeFileSync(path.join(__dirname,'verify_card_collection_v10.cjs'),code);
