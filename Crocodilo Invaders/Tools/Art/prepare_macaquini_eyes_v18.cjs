const fs=require('fs'),path=require('path'),crypto=require('crypto');
const sharp=require('C:/Users/berna/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=path.resolve(__dirname,'../..'),out=path.join(root,'Assets/Art/JungleStage/Macaquini_v18'),src=path.join(root,'SourceArt/MacaquiniEyes_v18'),preview=path.join(root,'Docs/ArtPreviews/MacaquiniEyes_v18');
const hash=b=>crypto.createHash('sha256').update(b).digest('hex');
(async()=>{
 fs.mkdirSync(out,{recursive:true});const entries=[];
 for(const form of ['transformation','swinging'])for(let frame=form==='transformation'?8:0;frame<(form==='transformation'?42:24);frame++){
  const name=(form==='transformation'?'transform_':'half_')+String(frame).padStart(2,'0')+'.png';
  const version=form==='swinging'?'Macaquini_v11':frame>=18&&frame<=27?'Macaquini_v17':'Macaquini_v13';
  const source='Assets/Art/JungleStage/'+version+'/'+name,original=fs.readFileSync(path.join(root,source));
  const {data,info}=await sharp(original).ensureAlpha().raw().toBuffer({resolveWithObject:true});const result=Buffer.from(data),green=new Uint8Array(info.width*info.height),mask=new Uint8Array(green.length);
  const roi=form==='transformation'?[185,210,315,286]:[140,150,235,218];const seeds=[];
  for(let y=roi[1];y<roi[3];y++)for(let x=roi[0];x<roi[2];x++){
   const k=y*info.width+x,p=k*4,r=data[p],g=data[p+1],b=data[p+2];
   if(data[p+3]&&g>100&&r<g*.65&&b<g*.7){green[k]=1;seeds.push(k);}
  }
  function white(k){let p=k*4,r=data[p],g=data[p+1],b=data[p+2];return data[p+3]&&r>85&&g>85&&b>70&&Math.max(r,g,b)-Math.min(r,g,b)<85&&g>r*.8&&b>g*.78;}
  const queue=[];
  for(const k of seeds){const x=k%512,y=Math.floor(k/512);for(let yy=Math.max(roi[1],y-4);yy<Math.min(roi[3],y+5);yy++)for(let xx=Math.max(roi[0],x-4);xx<Math.min(roi[2],x+5);xx++){let n=yy*512+xx;if(!mask[n]&&white(n)){mask[n]=1;queue.push(n);}}}
  // Fill only connected sclera/highlights near the original green iris, never face skin or teeth.
  for(let i=0;i<queue.length;i++){let k=queue[i],x=k%512,y=Math.floor(k/512);for(let yy=Math.max(roi[1],y-1);yy<Math.min(roi[3],y+2);yy++)for(let xx=Math.max(roi[0],x-1);xx<Math.min(roi[2],x+2);xx++){let n=yy*512+xx;if(!mask[n]&&white(n)){mask[n]=1;queue.push(n);}}}
  for(const k of queue){let p=k*4,l=(data[p]+data[p+1]+data[p+2])/3;result[p]=Math.round(25+l*.32);result[p+1]=Math.min(255,Math.round(150+l*.41));result[p+2]=Math.round(5+l*.06);}
  if(!queue.length)continue;
  for(let k=0;k<mask.length;k++){let p=k*4;if(result[p+3]!==data[p+3]||!mask[k]&&!result.subarray(p,p+4).equals(data.subarray(p,p+4)))throw Error('Protected pixels changed');}
  const file=(form==='transformation'?'transform_':'half_')+String(frame).padStart(2,'0')+'.png',final=await sharp(result,{raw:{width:512,height:512,channels:4}}).png().toBuffer();fs.writeFileSync(path.join(out,file),final);
  entries.push({form,frame,source,path:'Assets/Art/JungleStage/Macaquini_v18/'+file,changedPixels:queue.length,originalHash:hash(original),finalHash:hash(final),mask:queue,alphaIdentical:true,outsideEyesIdentical:true});
 }
 fs.writeFileSync(path.join(src,'processing.json'),JSON.stringify({tool:'image_gen.imagegen',mode:'built-in',reference:'green_eye_reference.png',policy:'Only neutral sclera and neutral glints connected to original green eyes are recolored. Green iris, black eyelids, face and all other native pixels remain identical.',entries},null,2));
 const crops=[];for(const folder of ['Macaquini_v13','Macaquini_v18'])for(const f of [29,33,35,37]){let name='transform_'+f+'.png',file=path.join(root,'Assets/Art/JungleStage',folder,name);if(!fs.existsSync(file))file=path.join(root,'Assets/Art/JungleStage/Macaquini_v13',name);crops.push(await sharp(file).extract({left:195,top:202,width:135,height:103}).resize(405,309,{kernel:'nearest'}).toBuffer());}
 await sharp({create:{width:1620,height:618,channels:4,background:'#122218'}}).composite(crops.map((input,i)=>({input,left:i%4*405,top:Math.floor(i/4)*309}))).png().toFile(path.join(preview,'olhos-antes-depois.png'));
 console.log(entries.map(e=>({form:e.form,frame:e.frame,pixels:e.changedPixels})));
})().catch(e=>{console.error(e);process.exitCode=1});
