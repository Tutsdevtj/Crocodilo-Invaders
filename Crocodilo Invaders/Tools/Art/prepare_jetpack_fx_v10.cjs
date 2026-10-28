const fs=require('node:fs'),path=require('node:path');
const sharp=require('C:/Users/berna/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=path.resolve(__dirname,'../..'),src=path.join(root,'SourceArt/Macaquini_v10/jetpack_fx_source.png'),out=path.join(root,'Assets/Art/JungleStage/Macaquini_v10');
(async()=>{
 const m=await sharp(src).metadata();
 for(let row=0;row<4;row++){
  const top=Math.floor(row*m.height/4)+(row<2?82:row===3?35:0),bottom=Math.floor((row+1)*m.height/4);
  let {data,info}=await sharp(src).extract({left:0,top,width:m.width,height:bottom-top}).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  for(let p=3;p<data.length;p+=4)data[p]=data[p]>=128?255:0;
  await sharp(data,{raw:{width:info.width,height:info.height,channels:4}}).png().toFile(path.join(out,['fx_flame','fx_blue','fx_smoke','fx_sparks'][row]+'.png'));
 }
 console.log('Four native particle strips exported; each strip has four animation frames.');
})().catch(e=>{console.error(e);process.exitCode=1;});
