const fs=require('node:fs'),path=require('node:path');
let code=fs.readFileSync(path.join(__dirname,'verify_card_collection_v11.cjs'),'utf8');
code=code.replace("out=path.join(project,'Docs/ArtPreviews/CardCollection_v11')","out=path.join(project,'Docs/ArtPreviews/CardCollection_v12')").replace("'cartas-bocas-corrigidas.html'","'cartas-expressoes-progressivas.html'");
code=code.replace("if(part==='base'&&(id==='AttackSpeed'&&(rank===1||rank===2)||id==='MoveSpeed'&&rank===1))continue;","if(part==='base'&&rank>0&&rank<4)continue;");
code=code.replace("path.join(project,'Docs/ArtPreviews/CardCollection_v10',id,name)","path.join(project,'Docs/ArtPreviews/CardCollection_v11',id,name)");
code=code.replace("other_cards:'22 CARDS UNCHANGED'","endpoint_cards:'10 COMMON AND MYTHIC CARDS UNCHANGED'");
fs.writeFileSync(path.join(__dirname,'verify_card_collection_v12.cjs'),code);
