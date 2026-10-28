const fs=require('fs'),path=require('path'),crypto=require('crypto');
const sharp=require('C:/Users/berna/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=path.resolve(__dirname,'../..'),src=path.join(root,'SourceArt/MacaquiniChest_v17'),out=path.join(root,'Assets/Art/JungleStage/Macaquini_v17'),preview=path.join(root,'Docs/ArtPreviews/MacaquiniChest_v17');
// Only the exposed pectorals/abdomen; shoulder fur and the banana retain their native pixels.
const wide=[[210,301],[219,294],[235,291],[260,281],[282,272],[301,278],[316,291],[319,308],[308,324],[302,346],[293,375],[279,377],[255,362],[234,343],[220,327]];
const narrow=[[228,312],[232,305],[241,303],[256,295],[268,288],[275,298],[281,308],[282,322],[284,337],[287,354],[289,373],[278,368],[260,354],[243,338],[233,325]];
let polygon=wide;
function inside(x,y){let c=false;for(let i=0,j=polygon.length-1;i<polygon.length;j=i++){let a=polygon[i],b=polygon[j];if((a[1]>y)!=(b[1]>y)&&x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0])c=!c;}return c;}
const hash=b=>crypto.createHash('sha256').update(b).digest('hex');
(async()=>{
 fs.mkdirSync(out,{recursive:true});fs.mkdirSync(preview,{recursive:true});const frames=[];
 for(let frame=18;frame<=27;frame++){
  polygon=frame===18?[[251,306],[265,301],[277,307],[280,325],[275,352],[264,341],[252,325]]:wide;
  const protectedHand=frame===19?[292,292,48,53]:frame===20?[302,291,47,49]:null;
  const name='transform_'+String(frame).padStart(2,'0')+'.png',original=fs.readFileSync(path.join(root,'Assets/Art/JungleStage/Macaquini_v13',name));
  const {data,info}=await sharp(original).ensureAlpha().raw().toBuffer({resolveWithObject:true});let result=Buffer.from(data),count=0;
  for(let y=270;y<378;y++)for(let x=208;x<322;x++){
   let p=(y*info.width+x)*4,r=data[p],g=data[p+1],b=data[p+2];
   if(!inside(x,y)||protectedHand&&x>=protectedHand[0]&&x<protectedHand[0]+protectedHand[2]&&y>=protectedHand[1]&&y<protectedHand[1]+protectedHand[3]||!data[p+3]||r<50||g/r<.27||g/r>.65||b>g*.85)continue;
   // Lift the chest into the peach palette; keep dark creases and native per-pixel shading.
   let t=Math.max(0,Math.min(1,(r-50)/95));t=t*t*(3-2*t);
   const target=[Math.min(255,r*.85+82),Math.min(255,g*1.15+48),Math.min(255,b*1.2+42)];
   for(let k=0;k<3;k++)result[p+k]=Math.round(data[p+k]+(target[k]-data[p+k])*t);
   if(result[p]!==r||result[p+1]!==g||result[p+2]!==b)count++;
  }
  for(let p=0;p<data.length;p+=4){if(result[p+3]!==data[p+3])throw Error('Alpha changed');if(!inside((p/4)%512,Math.floor(p/4/512))&&!result.subarray(p,p+4).equals(data.subarray(p,p+4)))throw Error('Outside chest changed');}
  const final=await sharp(result,{raw:{width:512,height:512,channels:4}}).png().toBuffer();fs.writeFileSync(path.join(out,name),final);
  frames.push({frame,name,polygon,protectedHand,changedPixels:count,originalHash:hash(original),finalHash:hash(final),outsideMaskIdentical:true,alphaIdentical:true});
 }
 const names=['transform_19.png','transform_23.png','transform_27.png'];const crops=[];
 for(const folder of ['Macaquini_v13','Macaquini_v17'])for(const name of names)crops.push(await sharp(path.join(root,'Assets/Art/JungleStage',folder,name)).extract({left:140,top:190,width:310,height:245}).resize(465,368,{kernel:'nearest'}).toBuffer());
 await sharp({create:{width:1395,height:736,channels:4,background:'#122218'}}).composite(crops.map((input,i)=>({input,left:(i%3)*465,top:Math.floor(i/3)*368}))).png().toFile(path.join(preview,'peito-antes-depois.png'));
 fs.writeFileSync(path.join(src,'processing.json'),JSON.stringify({tool:'image_gen.imagegen',mode:'built-in',reference:'chest_color_reference.png',policy:'Local palette correction on original RGB only; original alpha, silhouette, registration, pixel resolution and all outside-mask pixels verified identical.',polygon,frames},null,2));
 console.log(frames.map(f=>({frame:f.frame,changed:f.changedPixels})));
})().catch(e=>{console.error(e);process.exitCode=1});
