const http=require('http'),fs=require('fs'),path=require('path');
const root=path.resolve('SourceArt/CrocodiloIdle64-v02');
const port=Number(process.argv[2]||8777);
http.createServer((req,res)=>{let url=new URL(req.url,'http://localhost');let file=path.resolve(root,'.'+(url.pathname==='/'?'/preview.html':decodeURIComponent(url.pathname)));if(!file.startsWith(root+path.sep)){res.writeHead(403);return res.end()}fs.readFile(file,(err,data)=>{if(err){res.writeHead(404);return res.end()}res.setHeader('Content-Type',path.extname(file)==='.html'?'text/html; charset=utf-8':path.extname(file)==='.png'?'image/png':'application/octet-stream');res.end(data)})}).listen(port,'127.0.0.1',()=>console.log('Comparison preview: http://localhost:'+port+'/'));
