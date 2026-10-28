// Export the approved sheets deterministically; do not regenerate or repaint artwork.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));
const project=path.resolve(__dirname,'../..');
const source=path.join(project,'SourceArt/CardCollection_v21_Integration');
const output=path.join(project,'Docs/ArtPreviews/CardCollection_v21');
const ranks=['Common','Rare','Epic','Legendary','Mythic'];
const families=['AttackDamage','AttackSpeed','MaximumHealth','MoveSpeed','SpecialDamage'];
const values=[[5,5,10,3,5],[10,10,15,5,10],[15,15,20,8,15],[20,20,30,10,20],[30,30,40,15,30]];
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
const digest=p=>sha(fs.readFileSync(p));
const metaDigest=p=>sha(fs.readFileSync(p,'utf8').replace(/\r\n/g,'\n'));
function requireThat(value,message){if(!value)throw Error(message);}

async function separate(file){
 const {data,info}=await sharp(file).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 const {width:w,height:h}=info,n=w*h,exterior=new Uint8Array(n),queue=new Int32Array(n);
 function background(i){const j=i*4,r=data[j],g=data[j+1],b=data[j+2];return r<=18&&g<=43&&b<=85&&g-r>=10&&b-g>=10;}
 let head=0,tail=0;
 function enqueue(i){if(!exterior[i]&&background(i)){exterior[i]=1;queue[tail++]=i;}}
 for(let x=0;x<w;x++){enqueue(x);enqueue((h-1)*w+x);}
 for(let y=0;y<h;y++){enqueue(y*w);enqueue(y*w+w-1);}
 while(head<tail){const i=queue[head++],x=i%w,y=Math.floor(i/w);if(x)enqueue(i-1);if(x<w-1)enqueue(i+1);if(y)enqueue(i-w);if(y<h-1)enqueue(i+w);}
 // Eight-connected solid silhouettes retain entire illustration interiors and overhangs.
 const labels=new Int32Array(n);
 function componentList(){const components=[];let id=0;labels.fill(0);
 for(let i=0;i<n;i++){
  if(exterior[i]||labels[i])continue;
  id++;head=0;tail=1;queue[0]=i;labels[i]=id;
  let x0=w,y0=h,x1=0,y1=0,count=0;
  while(head<tail){const p=queue[head++],x=p%w,y=Math.floor(p/w);count++;x0=Math.min(x0,x);y0=Math.min(y0,y);x1=Math.max(x1,x);y1=Math.max(y1,y);
   for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++){const xx=x+dx,yy=y+dy;if(xx<0||xx>=w||yy<0||yy>=h)continue;const q=yy*w+xx;if(!exterior[q]&&!labels[q]){labels[q]=id;queue[tail++]=q;}}
  }
  if(count>30000)components.push({id,x0,y0,x1,y1,count});
 }
 components.sort((a,b)=>a.x0-b.x0);return components;}
 let components=componentList();
 if(components.length!==5){
  // Some sheet corner ornaments touch by 1-2 pixels. Follow the navy gutter
  // with a minimum-cost vertical seam rather than cropping fixed-width slots.
  for(let border=1;border<5;border++){
   const center=Math.round(w*border/5),lo=center-55,span=111,back=new Int8Array(h*span);
   let previous=new Float64Array(span),current=new Float64Array(span);
   for(let y=0;y<h;y++){
    for(let t=0;t<span;t++){
     const i=y*w+lo+t,j=i*4,cost=(background(i)?0:2+Math.max(data[j],data[j+1],data[j+2])/32)+Math.abs(t-55)*.0005;
     let pick=t,best=previous[t];
     for(let delta=-1;delta<=1;delta++){const q=t+delta;if(q>=0&&q<span&&previous[q]<best){best=previous[q];pick=q;}}
     current[t]=cost+(y?best:0);back[y*span+t]=pick-t;
    }
    [previous,current]=[current,previous];
   }
   let t=0;for(let q=1;q<span;q++)if(previous[q]<previous[t])t=q;
   let touched=0;
   for(let y=h-1;y>=0;y--){const x=lo+t;for(let dx=0;dx<=1;dx++){const p=y*w+x+dx;if(!exterior[p])touched++;exterior[p]=1;}t+=back[y*span+t];}
   requireThat(touched<1450,'Gutter seam crosses artwork: '+file+' / '+border+' / '+touched);
  }
  components=componentList();
 }
 requireThat(components.length===5,`Expected five independent silhouettes in ${file}, found ${JSON.stringify(components)}`);
 return {data,w,h,labels,components};
}
async function main(){
 if(process.argv.includes('--check')){
  const m=JSON.parse(fs.readFileSync(path.join(source,'manifest.json'),'utf8'));
  requireThat(m.cards.length===25,'Expected 25 cards');
  for(const c of m.cards){const asset=path.join(project,c.asset),{data,info}=await sharp(asset).ensureAlpha().raw().toBuffer({resolveWithObject:true});
   requireThat(digest(asset)===c.sha256,c.asset+' checksum');requireThat(metaDigest(asset+'.meta')===c.metaSha256,c.asset+' sprite identity');
   requireThat(info.width===360&&info.height===536,c.asset+' size');
   let transparent=0;for(let i=3;i<data.length;i+=4)if(data[i]===0)transparent++;
   requireThat(transparent===c.transparentPixels&&transparent>1000,c.asset+' alpha');
   for(let x=0;x<360;x++)requireThat(data[x*4+3]===0&&data[((535*360+x)*4)+3]===0,c.asset+' vertical clipping');
  }
  requireThat(digest(path.join(project,'Assets/Data/UpgradeCards.asset'))===m.catalogSha256,'Catalog changed');
  requireThat(digest(path.join(project,'Assets/Art/Cards/Back.png'))===m.backSha256,'Back changed');
  console.log('V21 CHECK PASSED: 25 complete transparent fronts, exact hashes, unchanged sprite metadata/catalog/back.');return;
 }
 fs.mkdirSync(output,{recursive:true});const cards=[],sheets=[];
 const catalogPath=path.join(project,'Assets/Data/UpgradeCards.asset'),catalog=fs.readFileSync(catalogPath,'utf8');
 for(let rank=0;rank<5;rank++){
  const rarity=ranks[rank],file=path.join(source,'Sheets',rarity+'.png'),split=await separate(file);
  sheets.push({rarity,file:path.relative(project,file).replaceAll('\\','/'),sha256:digest(file),silhouettes:split.components});
  for(let f=0;f<5;f++){
   const c=split.components[f],width=c.x1-c.x0+1,height=c.y1-c.y0+1,raw=Buffer.alloc(width*height*4);
   requireThat(c.x0>0&&c.y0>0&&c.x1<split.w-1&&c.y1<split.h-1,'Source silhouette clipped: '+rarity+'/'+families[f]);
   for(let y=0;y<height;y++)for(let x=0;x<width;x++){const from=(c.y0+y)*split.w+c.x0+x,j=(y*width+x)*4;if(split.labels[from]===c.id){split.data.copy(raw,j,from*4,from*4+4);}}
   // Standardize the uneven sheet panel proportions onto the game's shared portrait
   // grid. Keep the entire silhouette (including mythic overhangs), with no crop.
   const fitted=await sharp(raw,{raw:{width,height,channels:4}}).resize(172,256,{fit:'fill',kernel:'nearest'}).png().toBuffer();
   const dim=await sharp(fitted).metadata();
   const image=await sharp({create:{width:180,height:268,channels:4,background:{r:0,g:0,b:0,alpha:0}}})
    .composite([{input:fitted,left:Math.floor((180-dim.width)/2),top:Math.floor((268-dim.height)/2)}]).png().toBuffer();
   const final=await sharp(image).resize(360,536,{kernel:'nearest'}).png().toBuffer();
   const family=families[f],relative=`Assets/Resources/Cards/${family}/${rarity}.png`,asset=path.join(project,relative),preview=path.join(output,family,rarity+'.png');
   const meta=fs.readFileSync(asset+'.meta','utf8');requireThat(meta.includes('width: 360')&&meta.includes('height: 536'),'Sprite bounds mismatch '+relative);
   requireThat(new RegExp('percent: '+values[rank][f]+'\\s+artworkPath: Cards/'+family+'/'+rarity+'(?:\\r?\\n|$)').test(catalog),'Catalog mismatch '+relative);
   fs.mkdirSync(path.dirname(preview),{recursive:true});fs.writeFileSync(preview,final);
   const pixels=await sharp(final).raw().toBuffer();let transparent=0;for(let i=3;i<pixels.length;i+=4)if(pixels[i]===0)transparent++;
   if(process.argv.includes('--apply')){const stage=path.join(source,'staging');fs.mkdirSync(stage,{recursive:true});const temporary=path.join(stage,family+'-'+rarity+'.png');fs.writeFileSync(temporary,final);fs.renameSync(temporary,asset);}
   cards.push({family,rarity,asset:relative,sha256:sha(final),metaSha256:metaDigest(asset+'.meta'),percent:values[rank][f],transparentPixels:transparent,sourceBounds:[c.x0,c.y0,width,height],logicalSize:[dim.width,dim.height]});
  }
 }
 const manifest={version:21,approved:'v19 Common/Rare/Epic + v20 Legendary full art / Mythic corrected left Attack rail',applied:process.argv.includes('--apply'),catalogSha256:digest(catalogPath),backSha256:digest(path.join(project,'Assets/Art/Cards/Back.png')),cards,sheets};
 fs.writeFileSync(path.join(source,'manifest.json'),JSON.stringify(manifest,null,2)+'\n');
 for(const rarity of ranks){const tiles=await Promise.all(families.map(f=>sharp(path.join(output,f,rarity+'.png')).png().toBuffer()));
  const raw=Buffer.alloc(1900*580*4);for(let y=0;y<580;y++)for(let x=0;x<1900;x++){const v=((Math.floor(x/16)+Math.floor(y/16))%2)?64:43,j=(y*1900+x)*4;raw[j]=v;raw[j+1]=v+5;raw[j+2]=v+10;raw[j+3]=255;}
  await sharp(raw,{raw:{width:1900,height:580,channels:4}}).composite(tiles.map((input,i)=>({input,left:10+i*380,top:22}))).png().toFile(path.join(output,rarity+'-checker.png'));
 }
 console.log(JSON.stringify({mode:manifest.applied?'APPLIED':'PREVIEW',cards:cards.length,sheets:sheets.map(s=>({rarity:s.rarity,bounds:s.silhouettes.map(c=>[c.x0,c.y0,c.x1,c.y1])})),metadata:'UNCHANGED',back:'UNCHANGED'}));
}
main().catch(e=>{console.error(e.message);process.exitCode=1;});
