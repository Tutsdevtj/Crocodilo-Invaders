// Typography only. Fonts are the same licensed font files already in the game.
const path=require('node:path');
const sharp=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp'));
const project=path.resolve(__dirname,'../..');
const fonts={display:path.join(project,'Assets/Fonts/PressStart2P/PressStart2P-Regular.ttf'),support:path.join(project,'Assets/Fonts/PixelifySans/PixelifySans-VariableFont_wght.ttf')};
const names=['COMUM','RARO','ÉPICO','LENDÁRIO','MÍTICO'],colors=['#c5c6ad','#a8d7e8','#d6a6f9','#ffe0a0','#ffb18d'];
const metrics=[];
async function ink(text,style,size,color,tracking=0){const font=style==='display'?`Press Start 2P ${size}`:`Pixelify Sans Bold ${size}`;return sharp({text:{text:`<span foreground="${color}" letter_spacing="${tracking}">${text}</span>`,font,fontfile:fonts[style],rgba:true,dpi:72}}).trim().png().toBuffer({resolveWithObject:true});}
async function shadow(input){const {data,info}=await sharp(input).ensureAlpha().raw().toBuffer({resolveWithObject:true});for(let i=0;i<data.length;i+=4){data[i]=5;data[i+1]=11;data[i+2]=17;data[i+3]=Math.round(data[i+3]*.85);}return sharp(data,{raw:info}).png().toBuffer();}
async function renderLabels(rank,options={}){const title=options.title||'ATAQUE',attribute=options.attribute||'DANO DE ATAQUE',percent=options.percent??[5,10,15,20,30][rank];const layers=[];
async function centerLine(text,style,size,color,centerY,box,name,tracking=0,depth=false){const part=await ink(text,style,size,color,tracking),left=Math.round((360-part.info.width)/2),top=Math.round(centerY-part.info.height/2);if(left<box[0]||top<box[1]||left+part.info.width>box[2]||top+part.info.height>box[3])throw Error(`Text does not fit ${name}: ${text}`);if(depth)layers.push({input:await shadow(part.data),left,top:top+2});layers.push({input:part.data,left,top});metrics.push({rank,name,text,bounds:[left,top,part.info.width,part.info.height],safe_box:box});}
await centerLine(names[rank],'support',18,colors[rank],35,[58,23,302,47],'rarity',1024);
await centerLine(title,'display',22,'#fff0ce',64,[58,48,302,80],'title',0,true);
await centerLine(attribute,'support',18,colors[rank],443,[56,428,304,454],'attribute',0);
const digits=await ink(String(percent),'display',44,'#fff0ce'),plus=await ink('+','display',27,'#e8d8b5'),suffix=await ink('%','display',27,'#e8d8b5'),gap=6,total=digits.info.width+plus.info.width+suffix.info.width+gap*2;
if(total>244||digits.info.height>44)throw Error('Bonus exceeds safe area');let x=Math.round((360-total)/2);const baseline=501;
for(const [name,part]of[['plus',plus],['percent_value',digits],['percent_symbol',suffix]]){const top=baseline-part.info.height;layers.push({input:await shadow(part.data),left:x,top:top+2},{input:part.data,left:x,top});metrics.push({rank,name,text:name==='percent_value'?String(percent):name==='plus'?'+':'%',bounds:[x,top,part.info.width,part.info.height],safe_box:[58,457,302,505]});x+=part.info.width+gap;}
return sharp({create:{width:360,height:536,channels:4,background:'#00000000'}}).composite(layers).png().toBuffer();}
module.exports={renderLabels,metrics,fonts};
