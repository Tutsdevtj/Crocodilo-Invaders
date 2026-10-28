const fs=require('fs'),path=require('path');
const root=path.resolve(__dirname,'../..');
const a=JSON.parse(fs.readFileSync(path.join(root,'SourceArt/Macaquini_v13/layout.json'),'utf8'));
const b=JSON.parse(fs.readFileSync(path.join(root,'SourceArt/Macaquini_v11/layout.json'),'utf8'));
const ppu=80,anchor={x:1.05,y:-.35},entries=[];
function cel(form,file,oldPpu,oldPivot,feet,muzzle,vine){
 const center={x:feet.reduce((s,p)=>s+p.x,0)/feet.length,y:feet.reduce((s,p)=>s+p.y,0)/feet.length};
 const pivot={x:center.x-anchor.x*ppu,y:center.y+anchor.y*ppu};
 function point(v){return{x:(oldPivot.x+v.x*oldPpu-pivot.x)/ppu,y:(pivot.y-oldPivot.y+v.y*oldPpu)/ppu};}
 entries.push({form,path:file,oldPpu,ppu,pivot,muzzle:point(muzzle),vine:point(vine||{x:0,y:0})});
}
for(const form of a.forms)for(const c of form.frames){
 const feet=c.feet.map(p=>({x:c.pivot.x+p.x*c.ppu,y:c.pivot.y-p.y*c.ppu}));
 cel(form.form,'Assets/Art/JungleStage/Macaquini_v13/'+c.name,c.ppu,c.pivot,feet,c.muzzle,c.vineTop);
}
const half=b.forms.find(f=>f.form==='half');
for(const c of half.frames)cel('swinging','Assets/Art/JungleStage/Macaquini_v11/'+c.name,half.ppu,half.pivot,c.foot,c.muzzle,c.vineTop);
const dir=path.join(root,'SourceArt/MacaquiniScale_v16');fs.mkdirSync(dir,{recursive:true});
fs.writeFileSync(path.join(dir,'registration.json'),JSON.stringify({ppu,anchor,policy:'One pixel-to-world scale for every full-body drawing; register the mean of both feet to one anchor. No pixel edits, no runtime zoom, no interpolation of PPU between sheets.',entries},null,2));
console.log('Registered '+entries.length+' sprites at '+ppu+' PPU without changing their pixels.');
