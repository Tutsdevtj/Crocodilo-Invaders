const fs=require('node:fs'),path=require('node:path');
let code=fs.readFileSync(path.join(__dirname,'verify_card_collection_v10.cjs'),'utf8');
code=code.replace("out=path.join(project,'Docs/ArtPreviews/CardCollection_v10')","out=path.join(project,'Docs/ArtPreviews/CardCollection_v11')").replace("'cartas-miticas-vermelhas.html'","'cartas-bocas-corrigidas.html'");
code=code.replace("if(rank===4&&part!=='labels')continue;","if(part==='base'&&(id==='AttackSpeed'&&(rank===1||rank===2)||id==='MoveSpeed'&&rank===1))continue;");
code=code.replace("path.join(project,'Docs/ArtPreviews/CardCollection_v09',id,name)","path.join(project,'Docs/ArtPreviews/CardCollection_v10',id,name)");
code=code.replace("other_rarities:'20 CARDS UNCHANGED',typography:'25 LABEL LAYERS UNCHANGED'","other_cards:'22 CARDS UNCHANGED',typography:'25 LABEL LAYERS UNCHANGED',finish_masks:'25 UNCHANGED'");
fs.writeFileSync(path.join(__dirname,'verify_card_collection_v11.cjs'),code);
