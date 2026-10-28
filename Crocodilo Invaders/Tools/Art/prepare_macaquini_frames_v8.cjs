const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const sharp=require('C:/Users/berna/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const palette=require('./pixel_palette.cjs');
const root=path.resolve(__dirname,'../..'),src=path.join(root,'SourceArt/Macaquini_v08'),out=path.join(root,'Assets/Art/JungleStage/Macaquini_v08');
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
function components(data,w,h){
 const labels=new Int32Array(w*h),queue=new Int32Array(w*h),found=[];let id=0;
 for(let p=0;p<labels.length;p++){
  if(labels[p]||data[p*4+3]<128)continue;
  id++;let head=0,tail=1,minX=w,minY=h,maxX=0,maxY=0;queue[0]=p;labels[p]=id;
  while(head<tail){const q=queue[head++],x=q%w,y=Math.floor(q/w);minX=Math.min(minX,x);maxX=Math.max(maxX,x);minY=Math.min(minY,y);maxY=Math.max(maxY,y);
   for(let yy=Math.max(0,y-1);yy<=Math.min(h-1,y+1);yy++)for(let xx=Math.max(0,x-1);xx<=Math.min(w-1,x+1);xx++){const n=yy*w+xx;if(!labels[n]&&data[n*4+3]>=128){labels[n]=id;queue[tail++]=n;}}
  }
  found.push({id,area:tail,x:minX,y:minY,w:maxX-minX+1,h:maxY-minY+1});
 }
 return {labels,found};
}
(async()=>{
 fs.mkdirSync(out,{recursive:true});let sprites=[],manifest={version:8,canvas:[384,384],anchor:[192,12],fps:10,forms:{}};
 for(const form of ['half','closed','attack','tear']){
  const file=path.join(src,form+'_source.png'),meta=await sharp(file).metadata(),data=await sharp(file).ensureAlpha().raw().toBuffer(),w=meta.width,h=meta.height;
  const rows=form==='tear'?2:4,expected=rows*4;
  const {labels,found}=components(data,w,h),major=found.filter(c=>c.area>10000).sort((a,b)=>Math.floor((a.y+a.h*.5)/(h/rows))-Math.floor((b.y+b.h*.5)/(h/rows))||a.x-b.x);
  console.log(form,JSON.stringify(major));
  if(process.argv.includes('--analyze'))continue;
  if(major.length!==expected)throw Error(form+': expected'+expected+'wholecharactercomponents, got'+major.length);
  const entries=[];
  for(let i=0;i<expected;i++){
   const c=major[i],pixels=Buffer.alloc(c.w*c.h*4);
   // Whole connected sprite plus its tiny isolated fur highlights; exclude other complete cels.
   const otherMajor=new Set(major.filter(m=>m.id!==c.id).map(m=>m.id));
   for(let y=0;y<c.h;y++)for(let x=0;x<c.w;x++){const q=(c.y+y)*w+c.x+x,o=(y*c.w+x)*4;if(data[q*4+3]>=128&&!otherMajor.has(labels[q])){data.copy(pixels,o,q*4,q*4+3);pixels[o+3]=255;}}
   // Register to the top of the actual painted vine, never to the changing body bounds.
   let sum=0,count=0;for(let y=0;y<Math.min(5,c.h);y++)for(let x=0;x<c.w;x++)if(pixels[(y*c.w+x)*4+3]){sum+=x;count++;}
   const anchorX=sum/count,scale=.84*299.75/(h/rows),target=Buffer.alloc(384*384*4),offsetX=192-anchorX*scale,offsetY=12;
   for(let y=0;y<384;y++)for(let x=0;x<384;x++){const sx=Math.floor((x-offsetX)/scale),sy=Math.floor((y-offsetY)/scale);if(sx>=0&&sx<c.w&&sy>=0&&sy<c.h){const a=(sy*c.w+sx)*4;pixels.copy(target,(y*384+x)*4,a,a+4);}}
   const group=form==='attack'?(i<8?'closed_attack':'half_attack'):form,index=form==='attack'?i%8:i;
   const name=group+'_'+String(index).padStart(2,'0');sprites.push({name,group,index,data:target});entries.push({name,component:c,source_anchor:[c.x+anchorX,c.y],offset:[offsetX,offsetY],scale});
  }
  manifest.forms[form]={source_sha256:hash(file),frames:entries};
 }
 if(process.argv.includes('--analyze'))return;
 const all=Buffer.concat(sprites.map(s=>s.data));manifest.palette=palette(all,40);
 for(let i=0;i<sprites.length;i++){const s=sprites[i],buffer=all.subarray(i*384*384*4,(i+1)*384*384*4),file=path.join(out,s.name+'.png');await sharp(buffer,{raw:{width:384,height:384,channels:4}}).png().toFile(file);const entry=Object.values(manifest.forms).flatMap(v=>v.frames).find(f=>f.name===s.name);entry.sha256=hash(file);}
 fs.writeFileSync(path.join(src,'layout.json'),JSON.stringify(manifest,null,2)+'\n');
 console.log('EXPORTED '+sprites.length+' complete painted frames');
})().catch(e=>{console.error(e);process.exitCode=1;});
