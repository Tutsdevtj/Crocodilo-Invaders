const fs=require('fs'),path=require('path');
const sharp=require('C:/Users/Arthu/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const dir='SourceArt/CrocodiloIdle64-v02/';
const rgba=(d,k)=>d[k+3]?[...d.slice(k,k+4)]:[0,0,0,0];
const same=(a,b)=>a.every((v,i)=>v===b[i]);
async function read(file,opts={}){return sharp(file,opts).ensureAlpha().raw().toBuffer({resolveWithObject:true})}
(async()=>{
 let old=[],frames=[];for(let f=0;f<4;f++){old.push((await read('SourceArt/CrocodiloIdle64-v01/frames/idle_0'+f+'.png')).data);frames.push(await read(dir+'frames/idle_0'+f+'.png'))}
 const head=(await read('SourceArt/CrocodiloIdle64-v01/working/head-mask.png')).data;
 const allowed=(await read(dir+'working/allowed-mask.png')).data;
 const tail=(await read(dir+'working/tail_tip-mask.png')).data;
 const sheet=await read(dir+'crocodilo-idle-revisado-sheet.png');
 const gif=await read(dir+'crocodilo-idle-revisado-x8.gif',{animated:true});
 const cg=await read(dir+'crocodilo-idle-comparacao.gif',{animated:true});
 const gm=await sharp(dir+'crocodilo-idle-revisado-x8.gif',{animated:true}).metadata();
 const cm=await sharp(dir+'crocodilo-idle-comparacao.gif',{animated:true}).metadata();
 const palette=new Set();for(let k=0;k<old[0].length;k+=4)if(old[0][k+3])palette.add(rgba(old[0],k).join(','));
 let reportFrames=[],patches=[],errors=[];
 for(let f=0;f<4;f++){
  let d=frames[f].data,changed=0,newChanges=0,bootErrors=0,headErrors=0,maskErrors=0,gifErrors=0,comparisonErrors=0,sheetErrors=0,rootErrors=0;
  let alpha=new Set(),newColors=new Set(),bb=[64,64,0,0],diff=[];
  for(let y=0;y<64;y++)for(let x=0;x<64;x++){
   let k=(y*64+x)*4,p=rgba(d,k),b=rgba(old[0],k),v=rgba(old[f],k);alpha.add(p[3]);
   if(p[3]){bb[0]=Math.min(bb[0],x);bb[1]=Math.min(bb[1],y);bb[2]=Math.max(bb[2],x+1);bb[3]=Math.max(bb[3],y+1);if(!palette.has(p.join(',')))newColors.add(p.join(','))}
   if(!same(p,b))changed++;
   if(!same(p,v)){newChanges++;if(!allowed[k+3])maskErrors++;diff.push({x,y,before:v,after:p})}
   if(y>=55&&!same(p,b))bootErrors++;
   if(head[k+3]&&!same(rgba(d,((y-(f===2?1:0))*64+x)*4),b))headErrors++;
   // Hip attachment of the tail, as inspected in the original artwork.
   if(y>=38&&y<=46&&x>=26&&x<=28&&!same(p,b))rootErrors++;
   if(!same(rgba(sheet.data,(y*256+f*64+x)*4),p))sheetErrors++;
   const expected=p[3]?p:[226,228,230,255];
   for(let yy=0;yy<8;yy++)for(let xx=0;xx<8;xx++){let gk=((f*512+y*8+yy)*512+x*8+xx)*4;if(!same([...gif.data.slice(gk,gk+4)],expected))gifErrors++}
   for(let panel=0;panel<2;panel++)for(let yy=0;yy<6;yy++)for(let xx=0;xx<6;xx++){
    let source=panel?p:v,e=source[3]?source:[226,228,230,255],ck=((f*384+y*6+yy)*816+panel*432+x*6+xx)*4;
    if(!same([...cg.data.slice(ck,ck+4)],e))comparisonErrors++;
   }
  }
  let seen=new Set(),components=[];for(let pos=0;pos<4096;pos++){if(!d[pos*4+3]||seen.has(pos))continue;let q=[pos];seen.add(pos);for(let at=0;at<q.length;at++){let x=q[at]%64,y=Math.floor(q[at]/64);for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++){let xx=x+dx,yy=y+dy,k=yy*64+xx;if(xx>=0&&xx<64&&yy>=0&&yy<64&&d[k*4+3]&&!seen.has(k)){seen.add(k);q.push(k)}}}components.push(q.length)}
  let errs=[];if(frames[f].info.width!==64||frames[f].info.height!==64)errs.push('size');if(f===0&&changed)errs.push('F0');if(bootErrors||headErrors||maskErrors||rootErrors)errs.push('preservation');if(newColors.size||[...alpha].some(a=>a!==0&&a!==255))errs.push('palette/alpha');if(sheetErrors||gifErrors||comparisonErrors)errs.push('export mismatch');if(components.length!==1)errs.push('detached visible cluster');
  reportFrames.push({frame:f,durationMs:[450,250,450,350][f],changedFromF0:changed,changedFromPriorIdle:newChanges,bootsChanged:bootErrors,headIdentityErrors:headErrors,tailRootChanges:rootErrors,outsideAllowedMask:maskErrors,alpha:[...alpha],newColors:[...newColors],bboxExclusive:bb,connectedComponents8:components,sheetErrors,gifErrors,comparisonErrors,errors:errs});patches.push(diff);errors.push(...errs);
 }
 const adjacent=frames.map((frame,f)=>{let n=0;for(let k=0;k<frame.data.length;k+=4)if(!same(rgba(frame.data,k),rgba(frames[(f+1)%4].data,k)))n++;return n});
 if(!same(gm.delay,[450,250,450,350])||!same(cm.delay,[450,250,450,350])||gm.pages!==4||cm.pages!==4)errors.push('timing');
 const report={canvas:[64,64],paletteColors:palette.size,durationMs:1500,cellOrigin:[0,0],frames:reportFrames,adjacentChanges:adjacent,uniqueDrawings:new Set(frames.map(f=>f.data.toString('base64'))).size,bootsProtectedRows:[55,63],oldFullLowerLockRemoved:true,gif:{size:[512,512],delay:gm.delay},comparison:{size:[816,384],left:'prior',right:'revised',delay:cm.delay},technicalChecksPassed:errors.length===0,playbackReview:'pending real browser review',errors};
 fs.writeFileSync(dir+'validation.json',JSON.stringify(report,null,2)+'\n');fs.writeFileSync(dir+'working/patches.json',JSON.stringify({source:'current idle v01 frames',frames:patches},null,2)+'\n');console.log(JSON.stringify(report,null,2));if(errors.length)process.exitCode=1;
})();
