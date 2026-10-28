// Deterministic export of original imagegen sources; never redraws the approved boss.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const sharp = require('C:/Users/berna/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root = path.resolve(__dirname, '../..');
const source = path.join(root, 'SourceArt/JungleStage_v01');
const art = path.join(root, 'Assets/Art/JungleStage');
const preview = path.join(root, 'Docs/ArtPreviews/JungleStage');
const generated = 'C:/Users/berna/.codex/generated_images/01a164ae-b7b0-7f90-8b04-583b1c9a507b';
const specs = [
  ['bat', 'exec-0867d1c2-6abb-4496-80f6-f2de32bf4336.png', 96, 72],
  ['wasp', 'exec-7c397d0a-129d-4f64-a004-00ba50ef55bb.png', 80, 64],
  ['jungle', 'exec-0ec16b1e-fffc-40c7-a935-d4f7922b3dea.png', 480, 320],
  ['vine_cluster', 'exec-8407cf57-6fc5-400f-ba19-50a416906318.png', 96, 256]
];
const sha = p => crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
(async () => {
  for (const dir of [source, art, preview]) fs.mkdirSync(dir, {recursive:true});
  const manifest = {version:1, authoring:'Built-in image_gen; native exports use nearest-neighbor and a closed palette.', sprites:[]};
  for (const [name, file, width, height] of specs) {
    const original = path.join(source, name + '_source.png');
    if (!fs.existsSync(original)) fs.copyFileSync(path.join(generated, file), original);
    const output = path.join(art, name + '.png');
    let pipeline = sharp(original).ensureAlpha();
    if (name !== 'jungle') {
      // Derive the occupied bounds from source alpha, retaining transparent margins.
      const raw = await pipeline.raw().toBuffer({resolveWithObject:true});
      let minX=raw.info.width, minY=raw.info.height, maxX=0,maxY=0;
      for(let y=0;y<raw.info.height;y++) for(let x=0;x<raw.info.width;x++)
        if(raw.data[(y*raw.info.width+x)*4+3]>127){minX=Math.min(minX,x);minY=Math.min(minY,y);maxX=Math.max(maxX,x);maxY=Math.max(maxY,y);}
      pipeline=sharp(original).extract({left:minX,top:minY,width:maxX-minX+1,height:maxY-minY+1})
        .resize(width-4,height-4,{fit:'inside',kernel:'nearest'}).extend({top:2,bottom:2,left:2,right:2,background:'#00000000'});
    } else pipeline=pipeline.resize(width,height,{kernel:'nearest'});
    const raw=await pipeline.ensureAlpha().raw().toBuffer({resolveWithObject:true});
    for(let i=3;i<raw.data.length;i+=4) raw.data[i]=raw.data[i]>127?255:0;
    // Explicit median-cut keeps the closed palette independent of libvips' quality heuristics.
    const histogram=new Map();
    for(let i=0;i<raw.data.length;i+=4) if(raw.data[i+3]) {
      const key=raw.data.subarray(i,i+3).toString('hex');
      const entry=histogram.get(key); if(entry)entry.n++;else histogram.set(key,{rgb:[...raw.data.subarray(i,i+3)],n:1});
    }
    const boxes=[[...histogram.values()]];
    while(boxes.length<24){
      let choice=-1,channel=0,score=-1;
      boxes.forEach((b,index)=>{if(b.length<2)return;for(let c=0;c<3;c++){
        const values=b.map(e=>e.rgb[c]);const range=Math.max(...values)-Math.min(...values);
        const weight=range*Math.sqrt(b.reduce((sum,e)=>sum+e.n,0));
        if(weight>score){score=weight;choice=index;channel=c;}
      }});
      if(choice<0)break;
      const box=boxes.splice(choice,1)[0].sort((a,b)=>a.rgb[channel]-b.rgb[channel]);
      const half=box.reduce((sum,e)=>sum+e.n,0)/2;let n=0,split=1;
      for(let i=0;i<box.length-1;i++){n+=box[i].n;if(n>=half){split=i+1;break;}}
      boxes.push(box.slice(0,split),box.slice(split));
    }
    const palette=boxes.map(b=>{const total=b.reduce((s,e)=>s+e.n,0);return [0,1,2].map(c=>Math.round(b.reduce((s,e)=>s+e.rgb[c]*e.n,0)/total));});
    const mapped=new Map();
    for(const [key,entry] of histogram){let best=0,distance=Infinity;palette.forEach((p,j)=>{const d=p.reduce((s,v,c)=>s+(v-entry.rgb[c])**2,0);if(d<distance){distance=d;best=j;}});mapped.set(key,palette[best]);}
    for(let i=0;i<raw.data.length;i+=4)if(raw.data[i+3]){const rgb=mapped.get(raw.data.subarray(i,i+3).toString('hex'));for(let c=0;c<3;c++)raw.data[i+c]=rgb[c];}
    await sharp(raw.data,{raw:{width:raw.info.width,height:raw.info.height,channels:4}}).png().toFile(output);
    const result=await sharp(output).ensureAlpha().raw().toBuffer({resolveWithObject:true});
    const alphas=new Set(), colours=new Set();
    for(let i=0;i<result.data.length;i+=4){alphas.add(result.data[i+3]);if(result.data[i+3]) colours.add(result.data.subarray(i,i+3).toString('hex'));}
    if([...alphas].some(a=>a!==0&&a!==255)||colours.size>24)throw Error('Palette/alpha failure '+name);
    await sharp(output).resize({width:result.info.width*4,kernel:'nearest'}).png().toFile(path.join(preview,name+'_x4.png'));
    manifest.sprites.push({name,width:result.info.width,height:result.info.height,colours:colours.size,alpha:[...alphas],sha256:sha(output),source_sha256:sha(original)});
  }
  const weaponSource=path.join(root,'SourceArt/BananaAlienWeapon_v04_Organic');
  const weaponOut=path.join(art,'Weapon');fs.mkdirSync(weaponOut,{recursive:true});
  for(const name of fs.readdirSync(path.join(weaponSource,'frames')).filter(n=>n.endsWith('.png')))
    fs.copyFileSync(path.join(weaponSource,'frames',name),path.join(weaponOut,name));
  fs.copyFileSync(path.join(weaponSource,'projectile/banana.png'),path.join(art,'banana.png'));
  fs.copyFileSync(path.join(weaponSource,'animation.json'),path.join(source,'weapon_animation.json'));
  fs.writeFileSync(path.join(source,'manifest.json'),JSON.stringify(manifest,null,2)+'\n');
  console.log(JSON.stringify(manifest,null,2));
})().catch(error=>{console.error(error);process.exitCode=1;});
