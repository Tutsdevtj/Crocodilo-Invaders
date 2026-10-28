// Packages image_gen drawings without synthesizing or interpolating animation frames.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
let sharp;
try { sharp = require('sharp'); } catch {
  sharp = require(path.join(process.env.USERPROFILE, '.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));
}
const project = path.resolve(__dirname, '../..');
const root = path.join(project, 'SourceArt/BananaPossessora_Transformacao_v02_24Frames');
const preview = path.join(project, 'Docs/ArtPreviews/BananaPossessora_Transformacao_v02');
const durations = [220,150,150,150,150,220,120,120,120,190,100,180,120,90,100,130,160,160,140,140,100,150,160,900];
const names = ['Tensão inicial','Fecha os dedos','Fecha os punhos','Flexiona','Força aumenta','Raiva crescente','Alcança a ponta','Ergue a mão','Prepara a pegada','Agarra a ponta','Arranca a ponta','Ponta separada','Prepara o arremesso','Solta a ponta','Ponta voando','Busca a casca','Agarra o meio','Inicia o rasgo','Puxa para os lados','Amplia o rasgo','Rasga com força','Rugido','Solta a casca','Forma 2 pronta'];
const hash = b => crypto.createHash('sha256').update(b).digest('hex');
async function main() {
  fs.mkdirSync(path.join(root,'frames'),{recursive:true});fs.mkdirSync(preview,{recursive:true});
  const frames=[], buffers=[], records=[];
  for(let i=0;i<24;i++){
    const filename=`transformacao_${String(i).padStart(2,'0')}.png`;
    const source=path.join(root,'frames_source',filename);
    const {data,info}=await sharp(source).ensureAlpha().raw().toBuffer({resolveWithObject:true});
    // Discard only small disconnected fragments touching crop edges from adjacent grid cells.
    // Keep the character, flying cap and internal debris intact.
    const seen=new Uint8Array(info.width*info.height),components=[];
    for(let k=0;k<seen.length;k++){
      if(seen[k]||data[k*4+3]<128)continue;
      const pixels=[k];seen[k]=1;let edge=false;
      for(let q=0;q<pixels.length;q++){
        const p=pixels[q],x=p%info.width,y=Math.floor(p/info.width);
        if(x===0||y===0||x===info.width-1||y===info.height-1)edge=true;
        for(const n of [x>0?p-1:-1,x<info.width-1?p+1:-1,y>0?p-info.width:-1,y<info.height-1?p+info.width:-1]){
          if(n>=0&&!seen[n]&&data[n*4+3]>=128){seen[n]=1;pixels.push(n);}
        }
      }
      components.push({pixels,edge});
    }
    components.sort((a,b)=>b.pixels.length-a.pixels.length);
    for(const c of components.slice(1))if(c.edge&&c.pixels.length<2500)for(const k of c.pixels)data[k*4+3]=0;
    let footLeft=info.width,footRight=-1,footBottom=-1,left=info.width,top=info.height,right=-1,bottom=-1;
    for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++){
      const p=(y*info.width+x)*4;
      if(data[p+3]<128){data[p]=data[p+1]=data[p+2]=data[p+3]=0;continue;}
      data[p+3]=255;
      left=Math.min(left,x);top=Math.min(top,y);right=Math.max(right,x);bottom=Math.max(bottom,y);
      if(y>=info.height-100 && data[p]>140 && data[p+1]>70 && data[p]>data[p+1]*1.15 && data[p+1]>data[p+2]*1.12){
        footLeft=Math.min(footLeft,x);footRight=Math.max(footRight,x);footBottom=Math.max(footBottom,y);
      }
    }
    if(footRight<0)throw Error('Cannot locate feet: '+filename);
    const dx=Math.round(384-(footLeft+footRight)/2),dy=704-bottom;
    if(dx+left<0||dy+top<0||dx+right>=768||dy+bottom>=768)throw Error('Registration outside canvas: '+filename);
    const rgba=await sharp(data,{raw:{width:info.width,height:info.height,channels:4}})
      .extract({left,top,width:right-left+1,height:bottom-top+1}).png().toBuffer();
    const registered=await sharp({create:{width:768,height:768,channels:4,background:'#00000000'}})
      .composite([{input:rgba,left:dx+left,top:dy+top}]).png().toBuffer();
    const png=await sharp(registered).resize(384,384,{kernel:'nearest'}).png().toBuffer();
    fs.writeFileSync(path.join(root,'frames',filename),png);
    const raw=await sharp(png).ensureAlpha().raw().toBuffer();buffers.push(raw);
    const f={index:i,file:'frames/'+filename,label:names[i],duration_ms:durations[i],pivot_px:[192,352]};frames.push(f);
    records.push({index:i,sha256:hash(png),source_sha256:hash(fs.readFileSync(source)),translation_source_px:[dx,dy],foot_source_bbox:[footLeft,footRight,footBottom]});
  }
  if(new Set(records.map(r=>r.sha256)).size!==24)throw Error('Expected 24 different drawings');
  const inputs=frames.map((f,i)=>({input:path.join(root,f.file),left:(i%8)*384,top:Math.floor(i/8)*384}));
  await sharp({create:{width:3072,height:1152,channels:4,background:'#00000000'}}).composite(inputs).png().toFile(path.join(root,'spritesheet_8x3.png'));
  await sharp({create:{width:9216,height:384,channels:4,background:'#00000000'}}).composite(frames.map((f,i)=>({input:path.join(root,f.file),left:i*384,top:0}))).png().toFile(path.join(root,'spritesheet_horizontal.png'));
  const animated=()=>sharp(Buffer.concat(buffers),{raw:{width:384,height:384*24,channels:4,pageHeight:384}});
  await animated().gif({loop:0,delay:durations,colours:256,effort:7,dither:0}).toFile(path.join(preview,'transformacao_24frames.gif'));
  // Lossless animated WebP retains per-frame alpha and RGB without GIF palette reduction.
  await animated().webp({loop:1,delay:durations,lossless:true,effort:5}).toFile(path.join(preview,'transformacao_24frames.webp'));
  const manifest={version:2,type:'frame_by_frame',frame_count:24,canvas:[384,384],duration_ms:durations.reduce((a,b)=>a+b,0),loop:false,pixels_per_unit:100,root_pivot:[0.5,1-352/384],spritesheet:{file:'spritesheet_8x3.png',columns:8,rows:3,cell:[384,384],order:'left_to_right_top_to_bottom'},frames};
  fs.writeFileSync(path.join(root,'animation.json'),JSON.stringify(manifest,null,2));
  const inline=frames.map(f=>({...f,src:'data:image/png;base64,'+fs.readFileSync(path.join(root,f.file)).toString('base64')}));
  const template=fs.readFileSync(path.join(__dirname,'banana_transformacao_v02_preview.html'),'utf8');
  const html=template.replace('__FRAMES__',JSON.stringify(inline));
  new (require('node:vm').Script)(html.match(/<script>([\s\S]*?)<\/script>/)[1]);
  fs.writeFileSync(path.join(preview,'preview.html'),html);
  await sharp(path.join(root,'spritesheet_8x3.png')).flatten({background:'#16202c'}).resize(1280,480,{kernel:'nearest'}).png().toFile(path.join(preview,'contact_sheet.png'));
  const gif=await sharp(path.join(preview,'transformacao_24frames.gif'),{animated:true}).metadata();
  if(gif.pages!==24 || JSON.stringify(gif.delay)!==JSON.stringify(durations))throw Error('Incorrect animation timing');
  fs.writeFileSync(path.join(root,'qa_report.json'),JSON.stringify({frame_count:24,unique_frames:24,all_rgba:true,binary_alpha:true,canvas:[384,384],registered_feet:true,gif_frames:gif.pages,gif_delay_ms:gif.delay,preview_javascript_parses:true,browser_visual_check:false,unity_editor_validated:false,records},null,2));
  console.log(JSON.stringify({frames:24,duration_ms:manifest.duration_ms,preview:path.join(preview,'transformacao_24frames.gif')}));
}
main().catch(e=>{console.error(e);process.exitCode=1});
