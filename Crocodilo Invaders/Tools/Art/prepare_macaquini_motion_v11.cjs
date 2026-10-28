const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const sharp=require('C:/Users/berna/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=path.resolve(__dirname,'../..'),src=path.join(root,'SourceArt/Macaquini_v11'),out=path.join(root,'Assets/Art/JungleStage/Macaquini_v11');
function components(data,w,h,test){
 const labels=new Int32Array(w*h),found=[];let id=0;
 for(let p=0;p<labels.length;p++)if(!labels[p]&&test(p)){
  let queue=[p],x0=w,y0=h,x1=0,y1=0,sx=0,sy=0;labels[p]=++id;
  for(let q=0;q<queue.length;q++){let k=queue[q],x=k%w,y=Math.floor(k/w);x0=Math.min(x0,x);x1=Math.max(x1,x);y0=Math.min(y0,y);y1=Math.max(y1,y);sx+=x;sy+=y;
   for(let yy=Math.max(0,y-1);yy<=Math.min(h-1,y+1);yy++)for(let xx=Math.max(0,x-1);xx<=Math.min(w-1,x+1);xx++){let n=yy*w+xx;if(!labels[n]&&test(n)){labels[n]=id;queue.push(n);}}}
  found.push({id,x:x0,y:y0,w:x1-x0+1,h:y1-y0+1,area:queue.length,cx:sx/queue.length,cy:sy/queue.length});
 }
 return {labels,found};
}
function peelAnchor(data,w,h,c){
 const candidates=[];let bottom=0;
 for(let y=c.y;y<c.y+c.h;y++)for(let x=Math.floor(c.x+c.w*.30);x<c.x+c.w*.73;x++){
  let p=(y*w+x)*4,r=data[p],g=data[p+1],b=data[p+2];
  if(data[p+3]>=128&&r>130&&g>120&&g>r*.76&&g<r*1.15&&b<r*.55){candidates.push({x,y});bottom=Math.max(bottom,y);}
 }
 const base=candidates.filter(p=>p.y>=bottom-5);if(base.length<3)throw Error('Missing peel anchor');
 return {x:Math.round(base.reduce((s,p)=>s+p.x,0)/base.length),y:bottom};
}
function feet(data,pivot,width=384){
 const c=components(data,width,width,p=>{let x=p%width,y=Math.floor(p/width),o=p*4,r=data[o],g=data[o+1],b=data[o+2];return data[o+3]>0&&x>pivot.x+10&&y>pivot.y+12&&r>150&&g>r*.58&&g<r*.85&&b>r*.30&&b<g*.85;});
 return c.found.filter(v=>v.area>30).sort((a,b)=>b.area-a.area).slice(0,2).sort((a,b)=>a.cx-b.cx).map(v=>({x:v.cx,y:v.cy,area:v.area}));
}
const dist=(a,b)=>a.foot.length===2&&b.foot.length===2?Math.hypot(a.foot[0].x-b.foot[0].x,a.foot[0].y-b.foot[0].y,a.foot[1].x-b.foot[1].x,a.foot[1].y-b.foot[1].y):0;
(async()=>{
 fs.mkdirSync(out,{recursive:true});let manifest={version:11,canvas:[384,384],forms:[]};
 for(const form of ['flight','half']){
  const refPath=path.join(root,'Assets/Art/JungleStage/Macaquini_v10',form+'_00.png');
  const reference=await sharp(refPath).ensureAlpha().raw().toBuffer();const rc=components(reference,384,384,p=>reference[p*4+3]>=128).found.sort((a,b)=>b.area-a.area)[0];
  const originalTarget=peelAnchor(reference,384,384,rc),originalPivot=form==='flight'?{x:230,y:240}:{x:178,y:200},ppu=form==='flight'?80:60;
  const canvas=form==='half'?512:384,pad=(canvas-384)/2,target={x:originalTarget.x+pad,y:originalTarget.y+pad},pivot={x:originalPivot.x+pad,y:originalPivot.y+pad};
  let frames=[];
  for(const sheet of ['a','b']){
   const file=path.join(src,form+'_'+sheet+'_source.png'),{data,info}=await sharp(file).ensureAlpha().raw().toBuffer({resolveWithObject:true});
   const {labels,found}=components(data,info.width,info.height,p=>data[p*4+3]>=128);
   const major=found.filter(c=>c.area>10000).sort((a,b)=>Math.floor((a.y+8)/(info.height/4))-Math.floor((b.y+8)/(info.height/4))||a.x-b.x);
   if(major.length!==16)throw Error(form+sheet+' major components '+major.length);
   for(let i=0;i<16;i++){
    if(major[i].y+major[i].h>=info.height){console.log('Rejected clipped source',form,sheet,i);continue;}
    let c=major[i],anchor=peelAnchor(data,info.width,info.height,c),dx=target.x-anchor.x,dy=target.y-anchor.y,output=Buffer.alloc(canvas*canvas*4);
    for(let y=c.y;y<c.y+c.h;y++)for(let x=c.x;x<c.x+c.w;x++){
     let p=y*info.width+x;if(labels[p]!==c.id)continue;let xx=x+dx,yy=y+dy;if(xx<0||xx>=canvas||yy<0||yy>=canvas)throw Error('Clipped '+form+sheet+i);
     let o=(yy*canvas+xx)*4;data.copy(output,o,p*4,p*4+3);output[o+3]=255;
    }
    frames.push({source:form+'_'+sheet,sourceCell:i,registration:{x:dx,y:dy},data:output,foot:feet(output,pivot,canvas)});
   }
  }
  // Exclude outlying lower-body poses rather than introduce a kick to fill an arbitrary count.
  if(form==='half'){
   const median=values=>values.sort((a,b)=>a-b)[Math.floor(values.length/2)];
   const center={foot:[0,1].map(k=>({x:median(frames.map(f=>f.foot[k].x)),y:median(frames.map(f=>f.foot[k].y))}))};
   frames=frames.sort((a,b)=>dist(a,center)-dist(b,center)).slice(0,24);
  }
  // Try every endpoint so the last transition does not inherit the greedy route's largest jump.
  let ordered=null,bestScore=Infinity;
  for(let start=0;start<frames.length;start++){
   let remaining=frames.slice(),route=[remaining.splice(start,1)[0]];
   while(remaining.length){let last=route[route.length-1],best=0,cost=Infinity;for(let i=0;i<remaining.length;i++){let v=dist(last,remaining[i]);if(v<cost){cost=v;best=i;}}route.push(remaining.splice(best,1)[0]);}
   const steps=route.slice(1).map((f,i)=>dist(route[i],f)),score=Math.max(...steps)*100+steps.reduce((a,b)=>a+b,0);
   if(score<bestScore){ordered=route;bestScore=score;}
  }
  let old=[];for(let i=0;i<16;i++){let data=await sharp(path.join(root,'Assets/Art/JungleStage/Macaquini_v10',form+'_'+String(i).padStart(2,'0')+'.png')).ensureAlpha().raw().toBuffer();old.push({foot:feet(data,originalPivot)});}
  const stats=arr=>{let values=arr.slice(1).map((f,i)=>dist(arr[i],f));return {meanAdjacentFootPixels:values.reduce((a,b)=>a+b,0)/values.length,maxAdjacentFootPixels:Math.max(...values)};};
  const entry={form,canvas,pivot,ppu,oldMotion:stats(old),newMotion:stats(ordered),frames:[]};
  if(entry.newMotion.maxAdjacentFootPixels>4||entry.newMotion.meanAdjacentFootPixels>entry.oldMotion.meanAdjacentFootPixels*.2)throw Error('Foot continuity quality gate failed: '+form);
  for(let i=0;i<ordered.length;i++){
   let f=ordered[i],name=form+'_'+String(i).padStart(2,'0')+'.png',file=path.join(out,name);await sharp(f.data,{raw:{width:canvas,height:canvas,channels:4}}).png().toFile(file);
   let min=canvas,sum=0,n=0;for(let y=0;y<canvas;y++)for(let x=0;x<canvas;x++)if(f.data[(y*canvas+x)*4+3])min=Math.min(min,x);
   for(let y=0;y<canvas;y++)for(let x=min;x<=min+2;x++)if(f.data[(y*canvas+x)*4+3]){sum+=y;n++;}
   let top=canvas,topX=0,topCount=0;for(let y=0;y<canvas;y++)for(let x=0;x<canvas;x++)if(f.data[(y*canvas+x)*4+3])top=Math.min(top,y);
   for(let y=top;y<top+3;y++)for(let x=0;x<canvas;x++)if(f.data[(y*canvas+x)*4+3]){topX+=x;topCount++;}
   entry.frames.push({name,source:f.source,sourceCell:f.sourceCell,registration:f.registration,foot:f.foot,muzzle:{x:(min-pivot.x)/ppu,y:(pivot.y-sum/n)/ppu},vineTop:{x:(topX/topCount-pivot.x)/ppu,y:(pivot.y-top)/ppu},sha256:crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex')});
  }
  manifest.forms.push(entry);console.log(form,JSON.stringify({target,old:entry.oldMotion,new:entry.newMotion}));
 }
 fs.writeFileSync(path.join(src,'layout.json'),JSON.stringify(manifest,null,2));console.log(manifest.forms.reduce((n,g)=>n+g.frames.length,0)+' whole-character cels selected at native pixel resolution.');
})().catch(e=>{console.error(e);process.exitCode=1;});
