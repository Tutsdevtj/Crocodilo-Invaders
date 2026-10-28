// Export only: preserve painted RGB, isolate connected characters, register the peel at the pelvis.
const fs=require('node:fs'),path=require('node:path');
const sharp=require('C:/Users/berna/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=path.resolve(__dirname,'../..'),src=path.join(root,'SourceArt/Macaquini_v12'),out=path.join(root,'Assets/Art/JungleStage/Macaquini_v12');
function components(d,w,h){let seen=new Uint8Array(w*h),result=[];for(let p=0;p<w*h;p++)if(!seen[p]&&d[p*4+3]>=128){let q=[p],x0=w,y0=h,x1=0,y1=0;seen[p]=1;for(let k=0;k<q.length;k++){let n=q[k],x=n%w,y=Math.floor(n/w);x0=Math.min(x0,x);y0=Math.min(y0,y);x1=Math.max(x1,x);y1=Math.max(y1,y);for(let yy=Math.max(0,y-1);yy<=Math.min(h-1,y+1);yy++)for(let xx=Math.max(0,x-1);xx<=Math.min(w-1,x+1);xx++){let v=yy*w+xx;if(!seen[v]&&d[v*4+3]>=128){seen[v]=1;q.push(v);}}}if(q.length>1000)result.push({pixels:q,x:x0,y:y0,w:x1-x0+1,h:y1-y0+1});}return result;}
function anchor(d,w,c){let candidates=[],bottom=0;for(let p of c.pixels){let x=p%w,y=Math.floor(p/w),o=p*4,r=d[o],g=d[o+1],b=d[o+2];if(x>c.x+c.w*.30&&x<c.x+c.w*.73&&r>130&&g>120&&g>r*.76&&g<r*1.15&&b<r*.55){candidates.push({x,y});bottom=Math.max(bottom,y);}}let base=candidates.filter(v=>v.y>=bottom-5);if(!base.length)throw Error('No banana pelvis');return{x:Math.round(base.reduce((s,v)=>s+v.x,0)/base.length),y:bottom};}
async function read(file){let {data,info}=await sharp(file).ensureAlpha().raw().toBuffer({resolveWithObject:true});return{data,info,parts:components(data,info.width,info.height)};}
(async()=>{
 fs.mkdirSync(out,{recursive:true});let reference=await read(path.join(root,'Assets/Art/JungleStage/Macaquini_v11/flight_00.png')),a=anchor(reference.data,reference.info.width,reference.parts[0]),offset={x:a.x-230,y:a.y-240};
 let layout={version:12,forms:[],debris:[]};
 for(let form of ['flight','transform']){
  const canvas=512,pivot=form==='flight'?{x:294,y:304}:{x:256,y:320};let frames=[];
  for(let sheet of form==='flight'?['flight','flight_b']:['tear_a','tear_b']){
   let image=await read(path.join(src,sheet+'_source.png')),major=image.parts.filter(c=>c.pixels.length>10000).sort((a,b)=>Math.floor((a.y+8)/(image.info.height/4))-Math.floor((b.y+8)/(image.info.height/4))||a.x-b.x);
   if(major.length!==16)throw Error(sheet+' components '+major.length);
   let indices=sheet==='tear_b'?[2,2,3,3,4,5,6,7,8,9,10,11,12,13,14,15]:Array.from({length:16},(_,i)=>i);
   for(let i of indices){let c=major[i],a=anchor(image.data,image.info.width,c),dx=pivot.x+offset.x-a.x,dy=pivot.y+offset.y-a.y,buffer=Buffer.alloc(canvas*canvas*4);
    for(let p of c.pixels){let x=p%image.info.width+dx,y=Math.floor(p/image.info.width)+dy;if(x<0||x>=canvas||y<0||y>=canvas)throw Error('Clipped output '+sheet+i);let o=(y*canvas+x)*4;image.data.copy(buffer,o,p*4,p*4+3);buffer[o+3]=255;}
    let name=form+'_'+String(frames.length).padStart(2,'0')+'.png';await sharp(buffer,{raw:{width:canvas,height:canvas,channels:4}}).png().toFile(path.join(out,name));
    let min=canvas,sum=0,n=0;for(let p=0;p<canvas*canvas;p++)if(buffer[p*4+3])min=Math.min(min,p%canvas);for(let p=0;p<canvas*canvas;p++)if(buffer[p*4+3]&&p%canvas<=min+2){sum+=Math.floor(p/canvas);n++;}
    frames.push({name,source:sheet,cell:i,muzzle:{x:(min-pivot.x)/80,y:(pivot.y-sum/n)/80}});
   }
  }
  layout.forms.push({form,canvas,pivot,ppu:80,frames});
 }
 let debris=await read(path.join(src,'debris_source.png'));let props=debris.parts.sort((a,b)=>a.y-b.y);if(props.length!==2)throw Error('Expected cap and peel/pack assembly');
 for(let i=0;i<2;i++){let c=props[i],canvas=Math.max(c.w,c.h)+16,buffer=Buffer.alloc(canvas*canvas*4),dx=Math.floor((canvas-c.w)/2)-c.x,dy=Math.floor((canvas-c.h)/2)-c.y;for(let p of c.pixels){let o=((Math.floor(p/debris.info.width)+dy)*canvas+p%debris.info.width+dx)*4;debris.data.copy(buffer,o,p*4,p*4+3);buffer[o+3]=255;}let name=i?'peel_jetpack.png':'banana_cap.png';await sharp(buffer,{raw:{width:canvas,height:canvas,channels:4}}).png().toFile(path.join(out,name));layout.debris.push({name,canvas,ppu:i?340:400});}
 fs.writeFileSync(path.join(src,'layout.json'),JSON.stringify(layout,null,2));console.log('V12 export: 32 closed expressive poses, 32 transformation timings, two isolated debris sprites; RGB untouched.');
})().catch(e=>{console.error(e);process.exitCode=1;});
