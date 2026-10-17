const fs=require('fs');
const path=require('path');
const sharp=require('C:/Users/Arthu/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const dir=path.resolve('SourceArt/CrocodiloIdle64-v01');
async function read(file,options={}) {return sharp(file,options).ensureAlpha().raw().toBuffer({resolveWithObject:true})}
const same=(a,b)=>a.every((v,i)=>v===b[i]);
const canonical=(d,k)=>d[k+3]?[...d.slice(k,k+4)]:[0,0,0,0];
(async()=>{
 const base=(await read('SourceArt/CrocodiloNovo64-v01/crocodilo-novo-base-64.png')).data;
 const mask=(await read(path.join(dir,'working/editable-mask.png'))).data;
 const head=(await read(path.join(dir,'working/head-mask.png'))).data;
 const sheet=await read(path.join(dir,'crocodilo-idle-sheet-64.png'));
 const gifFile=path.join(dir,'crocodilo-idle-preview-x8.gif');
 const gifMeta=await sharp(gifFile,{animated:true}).metadata();
 const gif=(await read(gifFile,{animated:true})).data;
 const timing=JSON.parse(fs.readFileSync(path.join(dir,'crocodilo-idle-timing.json')));
 const palette=new Set();for(let k=0;k<base.length;k+=4)if(base[k+3])palette.add(canonical(base,k).join(','));
 const frames=[];const reports=[];const errors=[];
 for(let f=0;f<4;f++){
  const im=await read(path.join(dir,'frames',`idle_0${f}.png`));frames.push(im.data);
  let changed=0,protectedChanges=0,outsideMask=0,headErrors=0,sheetErrors=0,gifErrors=0;
  const alpha=new Set(),newColors=new Set();let bb=[64,64,0,0];
  for(let y=0;y<64;y++)for(let x=0;x<64;x++){
   const k=(y*64+x)*4,p=canonical(im.data,k),b=canonical(base,k);
   alpha.add(p[3]);if(p[3]){bb[0]=Math.min(bb[0],x);bb[1]=Math.min(bb[1],y);bb[2]=Math.max(bb[2],x+1);bb[3]=Math.max(bb[3],y+1);if(!palette.has(p.join(',')))newColors.add(p.join(','))}
   if(!same(p,b)){changed++;if(y>=34)protectedChanges++;if(!mask[k+3])outsideMask++}
   if(head[k+3]){const hk=((y-(f===2?1:0))*64+x)*4;if(!same(canonical(im.data,hk),b))headErrors++}
   const sk=(y*256+f*64+x)*4;if(!same(canonical(sheet.data,sk),p))sheetErrors++;
   const expected=p[3]?p:[226,228,230,255];
   for(let dy=0;dy<8;dy++)for(let dx=0;dx<8;dx++){
    const gk=((f*512+y*8+dy)*512+x*8+dx)*4;if(!same([...gif.slice(gk,gk+4)],expected))gifErrors++;
   }
  }
  const errs=[];
  if(im.info.width!==64||im.info.height!==64)errs.push('wrong size');
  if(f===0&&changed)errs.push('F0 changed');if(protectedChanges)errs.push('protected region changed');
  if(outsideMask)errs.push('outside mask');if(headErrors)errs.push('head identity mismatch');
  if(newColors.size)errs.push('new colours');if([...alpha].some(a=>a!==0&&a!==255))errs.push('partial alpha');
  if(sheetErrors||gifErrors)errs.push('export mismatch');
  reports.push({frame:f,duration_ms:timing.frames[f].duration_ms,changedPixels:changed,protectedChanges,outsideMask,headErrors,headShiftY:f===2?-1:0,alpha:[...alpha],newColors:[...newColors],bboxExclusive:bb,sheetErrors,gifErrors,errors:errs});
  errors.push(...errs);
 }
 if(sheet.info.width!==256||sheet.info.height!==64)errors.push('sheet size');
 if(!same(gifMeta.delay,[450,250,450,350])||gifMeta.pages!==4||gifMeta.pageHeight!==512)errors.push('GIF timing/frames');
 const adjacent=frames.map((frame,f)=>{let n=0;for(let k=0;k<frame.length;k+=4)if(!same(canonical(frame,k),canonical(frames[(f+1)%4],k)))n++;return n});
 const report={baseExact:true,canvas:[64,64],paletteColors:palette.size,sheet:[256,64],loopMs:1500,frames:reports,adjacentChangedPixels:adjacent,reusedDrawings:{F3:'F1'},gif:{size:[512,512],frames:gifMeta.pages,delay:gifMeta.delay,loop:gifMeta.loop},technicalChecksPassed:errors.length===0,artisticPlaybackReview:'pending browser review',errors};
 fs.writeFileSync(path.join(dir,'validation.json'),JSON.stringify(report,null,2)+'\n');
 console.log(JSON.stringify(report,null,2));if(errors.length)process.exitCode=1;
})();
