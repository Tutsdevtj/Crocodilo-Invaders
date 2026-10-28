function palette(raw,limit=24){
 const h=new Map();for(let i=0;i<raw.length;i+=4)if(raw[i+3]){const k=raw.subarray(i,i+3).toString('hex');const e=h.get(k);if(e)e.n++;else h.set(k,{rgb:[...raw.subarray(i,i+3)],n:1});}
 const boxes=[[...h.values()]];
 while(boxes.length<limit){let choice=-1,channel=0,score=-1;boxes.forEach((b,j)=>{if(b.length<2)return;for(let c=0;c<3;c++){let lo=255,hi=0,total=0;for(const e of b){lo=Math.min(lo,e.rgb[c]);hi=Math.max(hi,e.rgb[c]);total+=e.n;}const s=(hi-lo)*Math.sqrt(total);if(s>score){score=s;choice=j;channel=c;}}});if(choice<0)break;const b=boxes.splice(choice,1)[0].sort((a,b)=>a.rgb[channel]-b.rgb[channel]);const half=b.reduce((s,e)=>s+e.n,0)/2;let sum=0,split=1;for(let i=0;i<b.length-1;i++){sum+=b[i].n;if(sum>=half){split=i+1;break;}}boxes.push(b.slice(0,split),b.slice(split));}
 const colors=boxes.map(b=>{const n=b.reduce((s,e)=>s+e.n,0);return [0,1,2].map(c=>Math.round(b.reduce((s,e)=>s+e.n*e.rgb[c],0)/n));});const map=new Map();for(const [k,e]of h){let best=colors[0],score=Infinity;for(const p of colors){const d=p.reduce((s,v,c)=>s+(v-e.rgb[c])**2,0);if(d<score){score=d;best=p;}}map.set(k,best);}for(let i=0;i<raw.length;i+=4)if(raw[i+3]){const p=map.get(raw.subarray(i,i+3).toString('hex'));for(let c=0;c<3;c++)raw[i+c]=p[c];}return colors;
}

module.exports=palette;
