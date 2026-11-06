const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
let sharp;try{sharp=require('sharp');}catch{sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));}
const root=path.resolve(__dirname,'../..'),source=path.join(root,'SourceArt/AchievementMenuButton_v04'),output=path.join(root,'Assets/Art/AchievementsMenu'),preview=path.join(root,'Docs/ArtPreviews/AchievementMenuButton_v04');
async function main(){
 const generation=JSON.parse(fs.readFileSync(path.join(source,'generation.json'),'utf8'));const master=path.join(source,'master.png');
 if(!fs.existsSync(master))fs.copyFileSync(generation.path,master);
 fs.mkdirSync(output,{recursive:true});fs.mkdirSync(preview,{recursive:true});
 const input=fs.readFileSync(master),{data,info}=await sharp(input).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 let left=info.width,top=info.height,right=-1,bottom=-1;
 for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=160){left=Math.min(left,x);top=Math.min(top,y);right=Math.max(right,x);bottom=Math.max(bottom,y);}
 if(right<left)throw Error('Empty generated medal');
 const box={left,top,width:right-left+1,height:bottom-top+1};
 const raw=await sharp(input).extract(box).resize(128,128,{fit:'contain',kernel:'nearest',background:{r:0,g:0,b:0,alpha:0}}).ensureAlpha().raw().toBuffer();
 let transparent=0;for(let i=0;i<raw.length;i+=4){if(raw[i+3]<160){raw.fill(0,i,i+4);transparent++;}else raw[i+3]=255;}
 if(transparent<512)throw Error('Medal must have a transparent silhouette');
 const png=await sharp(raw,{raw:{width:128,height:128,channels:4}}).png({palette:true,colours:64,dither:0}).toBuffer();
 fs.writeFileSync(path.join(output,'achievements_medal.png'),png);
 await sharp(png).resize(512,512,{kernel:'nearest'}).toFile(path.join(preview,'medal-x4.png'));
 fs.writeFileSync(path.join(source,'manifest.json'),JSON.stringify({mode:'built-in image_gen edit; alpha trim and nearest-neighbor native export',size:[128,128],crop:box,transparentPixels:transparent,sourceSha256:crypto.createHash('sha256').update(input).digest('hex'),sha256:crypto.createHash('sha256').update(png).digest('hex')},null,2)+'\n');
 console.log('ACHIEVEMENT_MENU_ART_OK: 128x128 transparent pixel-art medal, matching leaderboard plaque.');
}
main().catch(e=>{console.error(e);process.exitCode=1;});
