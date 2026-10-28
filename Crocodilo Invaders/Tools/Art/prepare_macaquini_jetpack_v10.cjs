// Export complete animation cells, including the detached cap and discarded pack.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const sharp=require('C:/Users/berna/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=path.resolve(__dirname,'../..'),src=path.join(root,'SourceArt/Macaquini_v10'),out=path.join(root,'Assets/Art/JungleStage/Macaquini_v10');
(async()=>{
 fs.mkdirSync(out,{recursive:true});let manifest={canvas:[384,384],nativeSampling:true,frames:[]};
 for(const group of ['flight','transform']){
  let file=path.join(src,group+'_source.png'),{data,info}=await sharp(file).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  if(info.width!==1536||info.height!==1024)throw Error('Expected native 384x256 cells');
  for(let i=0;i<16;i++){
   let pixels=Buffer.alloc(384*384*4),count=0;
   for(let y=0;y<256;y++)for(let x=0;x<384;x++){
    const p=((Math.floor(i/4)*256+y)*info.width+(i%4)*384+x)*4,o=((y+64)*384+x)*4;
    if(data[p+3]>=128){data.copy(pixels,o,p,p+3);pixels[o+3]=255;count++;}
   }
   // Remove tiny isolated marks crossing a cell boundary (the next row's vine).
   const seen=new Uint8Array(384*384);
   for(let p=0;p<seen.length;p++)if(!seen[p]&&pixels[p*4+3]){
    let queue=[p];seen[p]=1;let edge=false;
    for(let q=0;q<queue.length;q++){let k=queue[q],x=k%384,y=Math.floor(k/384);if(y>=315||x===0||x===383)edge=true;
     for(let yy=Math.max(0,y-1);yy<=Math.min(383,y+1);yy++)for(let xx=Math.max(0,x-1);xx<=Math.min(383,x+1);xx++){let n=yy*384+xx;if(!seen[n]&&pixels[n*4+3]){seen[n]=1;queue.push(n);}}}
    if(edge&&queue.length<800)for(let k of queue)pixels[k*4+3]=0;
   }
   if(count<3000)throw Error('Missing opaque character '+group+i);
   let name=group+'_'+String(i).padStart(2,'0')+'.png';
   if(group==='transform'){
    const hips=[[211,172],[240,174],[242,181],[265,174],[230,177],[215,188],[215,188],[225,185],[220,177],[226,178],[126,177],[246,176],[240,189],[244,185],[264,190],[240,188]];
    const canvas=Buffer.alloc(512*512*4),dx=256-hips[i][0],dy=320-(hips[i][1]+64);
    for(let y=0;y<384;y++)for(let x=0;x<384;x++){let xx=x+dx,yy=y+dy;if(xx>=0&&xx<512&&yy>=0&&yy<512)pixels.copy(canvas,(yy*512+xx)*4,(y*384+x)*4,(y*384+x)*4+4);}
    await sharp(canvas,{raw:{width:512,height:512,channels:4}}).png().toFile(path.join(out,name));
   }else await sharp(pixels,{raw:{width:384,height:384,channels:4}}).png().toFile(path.join(out,name));
   manifest.frames.push({name,opaquePixels:count,sha256:crypto.createHash('sha256').update(fs.readFileSync(path.join(out,name))).digest('hex')});
  }
 }
 for(const prefix of ['half','half_attack'])for(let i=0;i<(prefix==='half'?16:8);i++){
  let name=prefix+'_'+String(i).padStart(2,'0')+'.png';fs.copyFileSync(path.join(root,'Assets/Art/JungleStage/Macaquini_v08',name),path.join(out,name));
 }
 manifest.muzzles=[];
 for(const prefix of ['flight','half','half_attack'])for(let i=0;i<(prefix==='half_attack'?8:16);i++){
  let name=prefix+'_'+String(i).padStart(2,'0')+'.png',data=await sharp(path.join(out,name)).ensureAlpha().raw().toBuffer(),min=384,sum=0,n=0;
  for(let y=0;y<384;y++)for(let x=0;x<384;x++)if(data[(y*384+x)*4+3]>=128)min=Math.min(min,x);
  for(let y=0;y<384;y++)for(let x=min;x<=min+2;x++)if(data[(y*384+x)*4+3]>=128){sum+=y;n++;}
  let ppu=prefix==='flight'?80:60,px=prefix==='flight'?230:178,py=prefix==='flight'?240:200;
  manifest.muzzles.push({name,offset:{x:(min-px)/ppu,y:(py-sum/n)/ppu}});
 }
 fs.writeFileSync(path.join(src,'layout.json'),JSON.stringify(manifest,null,2));console.log('32 native full-body cels exported, RGB preserved; binary alpha only.');
})().catch(e=>{console.error(e);process.exitCode=1;});
