// Shared baselines and font sizes; whole bonus string uses one font/size.
const path=require('node:path');
const sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));
const project=path.resolve(__dirname,'../..');
const fonts={display:path.join(project,'Assets/Fonts/PressStart2P/PressStart2P-Regular.ttf'),support:path.join(project,'Assets/Fonts/PixelifySans/PixelifySans-VariableFont_wght.ttf')};
const names=['COMUM','RARO','ÉPICO','LENDÁRIO','MÍTICO'],colors=['#c5c6ad','#a8d7e8','#d6a6f9','#ffe0a0','#ffb18d'];
const metrics=[];
const layout={rarity:{font:'support',size:16,bottom:47,box:[64,32,296,48],tracking:1024},title:{font:'display',size:20,bottom:73,box:[64,49,296,78],tracking:0},attribute:{font:'support',size:16,bottom:458,box:[64,442,296,462],tracking:0},bonus:{font:'display',size:32,bottom:496,box:[64,466,296,501],tracking:0}};
async function ink(text,line,color){return sharp({text:{text:`<span foreground="${color}" letter_spacing="${line.tracking}">${text}</span>`,font:line.font==='display'?`Press Start 2P ${line.size}`:`Pixelify Sans Bold ${line.size}`,fontfile:fonts[line.font],rgba:true,dpi:72}}).trim().png().toBuffer({resolveWithObject:true});}
async function shadow(input){const {data,info}=await sharp(input).ensureAlpha().raw().toBuffer({resolveWithObject:true});for(let i=0;i<data.length;i+=4){data[i]=5;data[i+1]=11;data[i+2]=17;data[i+3]=Math.round(data[i+3]*.7);}return sharp(data,{raw:info}).png().toBuffer();}
async function renderLabels(rank,options={}){const percent=options.percent??[5,10,15,20,30][rank],strings={rarity:names[rank],title:options.title||'ATAQUE',attribute:options.attribute||'DANO DE ATAQUE',bonus:`+${percent}%`},layers=[];
for(const [name,text]of Object.entries(strings)){const line=layout[name],part=await ink(text,line,name==='rarity'||name==='attribute'?colors[rank]:'#fff0ce'),left=Math.round((360-part.info.width)/2),top=line.bottom-part.info.height,depth=name==='title'||name==='bonus',bottom=line.bottom+(depth?1:0);
if(left<line.box[0]||top<line.box[1]||left+part.info.width>line.box[2]||bottom>line.box[3])throw Error(`Text outside padded plate: ${name} ${text}`);
if(depth)layers.push({input:await shadow(part.data),left,top:top+1});layers.push({input:part.data,left,top});
metrics.push({rank,name,text,font_size:line.size,alignment:'center',baseline:line.bottom,bounds:[left,top,part.info.width,part.info.height],safe_box:line.box});}
return sharp({create:{width:360,height:536,channels:4,background:'#00000000'}}).composite(layers).png().toBuffer();}
module.exports={renderLabels,metrics,fonts,layout};
