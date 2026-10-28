// Native RGB is untouched. Whole connected poses are registered by their feet, never by the tearing seam.
const fs=require('node:fs'),path=require('node:path'),sharp=require('C:/Users/berna/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=path.resolve(__dirname,'../..'),src=path.join(root,'SourceArt/Macaquini_v13'),out=path.join(root,'Assets/Art/JungleStage/Macaquini_v13');
function components(d,w,h,test){let seen=new Uint8Array(w*h),result=[];for(let p=0;p<w*h;p++)if(!seen[p]&&test(p)){let q=[p],x0=w,y0=h,x1=0,y1=0,sx=0,sy=0;seen[p]=1;for(let k=0;k<q.length;k++){let n=q[k],x=n%w,y=Math.floor(n/w);sx+=x;sy+=y;x0=Math.min(x0,x);y0=Math.min(y0,y);x1=Math.max(x1,x);y1=Math.max(y1,y);for(let yy=Math.max(0,y-1);yy<=Math.min(h-1,y+1);yy++)for(let xx=Math.max(0,x-1);xx<=Math.min(w-1,x+1);xx++){let v=yy*w+xx;if(!seen[v]&&test(v)){seen[v]=1;q.push(v);}}}if(q.length>30)result.push({pixels:q,x:x0,y:y0,w:x1-x0+1,h:y1-y0+1,cx:sx/q.length,cy:sy/q.length,area:q.length});}return result;}
async function read(file){let {data:d,info:i}=await sharp(file).ensureAlpha().raw().toBuffer({resolveWithObject:true});return{d,i,parts:components(d,i.width,i.height,p=>d[p*4+3]>=128)};}
function feet(image,c){let {d,i}=image;let f=components(d,i.width,i.height,p=>{let x=p%i.width,y=Math.floor(p/i.width),o=p*4,r=d[o],g=d[o+1],b=d[o+2];return x>c.x+c.w*.45&&x<c.x+c.w&&y>c.y+c.h*.74&&y<c.y+c.h&&d[o+3]>=128&&r>150&&g>r*.58&&g<r*.85&&b>r*.30&&b<g*.85;}).sort((a,b)=>b.area-a.area).slice(0,2).sort((a,b)=>a.cx-b.cx);if(f.length!==2)throw Error('Missing feet '+JSON.stringify({x:c.x,y:c.y,w:c.w,h:c.h}));return f;}
const center=f=>({x:(f[0].cx+f[1].cx)/2,y:(f[0].cy+f[1].cy)/2}),area=f=>f[0].area+f[1].area,median=a=>a.sort((a,b)=>a-b)[Math.floor(a.length/2)],mix=(a,b,t)=>a+(b-a)*t;
(async()=>{
 fs.mkdirSync(out,{recursive:true});let ref=await read(path.join(root,'Assets/Art/JungleStage/Macaquini_v12/flight_00.png')),rf=feet(ref,ref.parts.sort((a,b)=>b.area-a.area)[0]),rc=center(rf),reference={ppu:80,area:area(rf),world:{x:(rc.x-294)/80,y:(304-rc.y)/80}};
 let half=await read(path.join(root,'Assets/Art/JungleStage/Macaquini_v11/half_00.png')),halfBody=half.parts.sort((a,b)=>b.area-a.area)[0],hf=feet(half,halfBody),hc=center(hf),halfRef={ppu:60,width:halfBody.w,world:{x:(hc.x-242)/60,y:(264-hc.y)/60}};
 let manifest={version:13,canvas:512,forms:[{form:'flight',frames:[]},{form:'transform',frames:[]}],rejected:[{source:'catch',cell:0,reason:'hand clipped at image edge'},{source:'catch',cell:11,reason:'wrong arm reaches toward vine'}]};
 for(let name of ['flight','flight_b','cap','peel','catch']){
  let image=await read(path.join(src,name+'_source.png')),parts=image.parts.filter(c=>c.area>10000).sort((a,b)=>Math.floor((a.y+12)/(image.i.height/4))-Math.floor((b.y+12)/(image.i.height/4))||a.x-b.x);
  let expected=name==='cap'?12:16;if(parts.length!==expected)throw Error(name+' connected poses '+parts.length+' expected '+expected);
  let entries=parts.map((c,cell)=>({c,cell,foot:feet(image,c)})),sheetArea=median(entries.map(v=>area(v.foot)));
  for(let e of entries){if(name==='catch'&&[0,11].includes(e.cell))continue;
   let form=name.startsWith('flight')?manifest.forms[0]:manifest.forms[1],time=name==='cap'?e.cell*(1.65/11):name==='peel'?mix(1.75,4.875,e.cell/15):name==='catch'?mix(5,7.88,(e.cell-1)/14):0;
   let blend=name==='catch'?Math.max(0,Math.min(1,(time-6.45)/(7.88-6.45))):0,ppu=mix(reference.ppu*Math.sqrt(sheetArea/reference.area),halfRef.ppu*parts.at(-1).w/halfRef.width,blend),world={x:mix(reference.world.x,halfRef.world.x,blend),y:mix(reference.world.y,halfRef.world.y,blend)};
   let fc=center(e.foot),dx=Math.round(385-fc.x),dy=Math.round(400-fc.y),buffer=Buffer.alloc(512*512*4),pivot={x:fc.x+dx-world.x*ppu,y:fc.y+dy+world.y*ppu};
   for(let p of e.c.pixels){let x=p%image.i.width+dx,y=Math.floor(p/image.i.width)+dy;if(x<0||x>=512||y<0||y>=512)throw Error('Clipped output '+name+e.cell);let o=(y*512+x)*4;image.d.copy(buffer,o,p*4,p*4+3);buffer[o+3]=255;}
   let file=form.form+'_'+String(form.frames.length).padStart(2,'0')+'.png';await sharp(buffer,{raw:{width:512,height:512,channels:4}}).png().toFile(path.join(out,file));
   let min=512,sum=0,n=0,top=512,topX=0,topN=0;for(let p=0;p<512*512;p++)if(buffer[p*4+3]){min=Math.min(min,p%512);top=Math.min(top,Math.floor(p/512));}for(let p=0;p<512*512;p++)if(buffer[p*4+3]){if(p%512<=min+2){sum+=Math.floor(p/512);n++;}if(Math.floor(p/512)<top+3){topX+=p%512;topN++;}}
   form.frames.push({name:file,source:name,cell:e.cell,pivot,ppu,opaqueWidth:e.c.w,start:time/8,headExposed:name==='cap'?e.cell>=8:!name.startsWith('flight'),packAttached:name!=='catch',vineHand:name==='catch'&&e.cell>=9?'screen-right':null,hasVine:name==='catch'&&e.cell>=12,muzzle:{x:(min-pivot.x)/ppu,y:(pivot.y-sum/n)/ppu},vineTop:{x:(topX/topN-pivot.x)/ppu,y:(pivot.y-top)/ppu},feet:e.foot.map(f=>({x:(f.cx+dx-pivot.x)/ppu,y:(pivot.y-f.cy-dy)/ppu}))});
  }
 }
 const flight=manifest.forms[0].frames,steps=flight.slice(1).map((f,i)=>Math.hypot(...[0,1].flatMap(k=>[f.feet[k].x-flight[i].feet[k].x,f.feet[k].y-flight[i].feet[k].y]))*80);manifest.flightMotion={meanFootPixelsAt80PPU:steps.reduce((s,v)=>s+v,0)/steps.length,maxFootPixelsAt80PPU:Math.max(...steps)};
 const last=manifest.forms[1].frames.at(-1);manifest.handover={lastWidth:last.opaqueWidth/last.ppu,halfWidth:halfRef.width/halfRef.ppu,ratio:(last.opaqueWidth/last.ppu)/(halfRef.width/halfRef.ppu)};
 fs.writeFileSync(path.join(src,'layout.json'),JSON.stringify(manifest,null,2));console.log(JSON.stringify({forms:manifest.forms.map(f=>[f.form,f.frames.length]),motion:manifest.flightMotion,handover:manifest.handover,rejected:manifest.rejected}));
})().catch(e=>{console.error(e);process.exitCode=1;});
