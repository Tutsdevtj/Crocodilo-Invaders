-- Original portrait identity; native 32x32 silhouette and anatomy reconstruction.
local root=assert(app.params.root):gsub('\\','/')
local dir=root..'/SourceArt/CrocodiloBase32-v02/'
local hex={O=0x192820,S=0x2b3c2b,G=0x415635,H=0x647648,
 J=0xae9054,K=0xd4b875,C=0xe9d49b,E=0xd5ab4d,
 D=0x48482f,T=0x77714b,L=0x999061,U=0x52613b,
 P=0x9b8361,Q=0x68543c,B=0x302e28,b=0x574c39}
local colors={}
for name,v in pairs(hex) do colors[name]=app.pixelColor.rgba((v>>16)&255,(v>>8)&255,v&255,255) end
local image=Image(32,32,ColorMode.RGB)
local parts={}
local function newpart(name) local p=Image(32,32,ColorMode.RGB); parts[#parts+1]={name,p}; return p end
local function row(im,x,y,str)
 for n=1,#str do local ch=str:sub(n,n);if ch~='.' then
  assert(x+n-1>=0 and x+n-1<32 and y>=0 and y<32)
  im:drawPixel(x+n-1,y,assert(colors[ch]))
 end end
end
local function stamp(im,x,y,rows) for n,str in ipairs(rows) do row(im,x,y+n-1,str) end end

-- Stage A/B: joint-connected silhouettes. Masses are designed independently.
local tail=newpart('Cauda')
stamp(tail,4,24,{
 '.......OSO',
 '...OOOOGSO',
 '.OOGHGGSO.',
 'OGGJJJOO..',
 '.OJJOO....',
})
local legs=newpart('Pelve, pernas humanas e botas')
row(legs,12,22,'OQQQQEQQQO')
row(legs,12,23,'OPPQO.OPPQO')
row(legs,12,24,'OPPQO.OPPQO')
row(legs,11,25,'OPPPQO..OPQO')
row(legs,11,26,'OPPQO...OPQO')
row(legs,12,27,'OPQO...OPQO')
row(legs,12,28,'OBBO...OBBO')
row(legs,11,29,'OBbbBO..OBbbBO')
row(legs,10,30,'OBBBBBO..OBBBBBO')
local body=newpart('Camisa e gola')
row(body,10,14,'OODLJJDLDOOO')
row(body,9,15,'ODLTTDJDLTTDOOO')
row(body,9,16,'OTLTDDTDTLTTTDO')
row(body,10,17,'ODTDTTTDTLTTDO')
row(body,12,18,'OTLTTDTTDO')
row(body,12,19,'OTTTTDTTDO')
row(body,12,20,'OTLTTDTTDO')
row(body,12,21,'ODTTTDTTDO')
local arms=newpart('Bracos humanos')
stamp(arms,7,16,{
 '.OTDO',
 'OTLDO',
 'ODDO.',
 'OGHO.',
 'OGHO.',
 'OGSO.',
 '.OGO.',
 '.OO..',
})
stamp(arms,23,16,{
 'ODTO',
 'OTDO',
 '.ODO',
 '.OGO',
 '.OGO',
 '.OGSO',
 '..OGO',
 '..OO.',
})
local head=newpart('Cabeca, focinho e capacete')
stamp(head,10,3,{
 '.....OOOOOO........',
 '...OOUUUHHUOO......',
 '..OSUUHHHHUUUO.....',
 '.OSUUHHUUUUUUSO....',
 '.OOSSSSSSSSSSSOO...',
 '.OSGGSEOOSGGGSO....',
 '.OSGGGSSGGHHHGOOO..',
 '.OSGGSSGGGHGGGGGGO.',
 '..OSGGGSOCOCOOOOOO.',
 '....OSJKKKJJJJJJO..',
 '.....OSJKKKJJJOO...',
})
-- One nostril, a blunt muzzle tip, and a continuous dark mouth seam.
head:drawPixel(25,10,colors.O)
row(head,16,14,'OJKKJO')

-- Color masses and economical shadows share the same geometry.
for _,part in ipairs(parts) do image:drawImage(part[2]) end
image:saveAs(dir..'crocodilo-base-32.png')
local preview=Image(image);preview:resize(256,256)
preview:saveAs(dir..'crocodilo-base-preview-x8.png')
-- Working-only evidence: silhouette, colored anatomy and context previews.
local silhouette=Image(image)
for p in silhouette:pixels() do if app.pixelColor.rgbaA(p())>0 then p(colors.O) end end
local blockout=Image(32,32,ColorMode.RGB)
for n,part in ipairs(parts) do
 local mask=Image(part[2]);local flat=({colors.H,colors.P,colors.L,colors.G,colors.J})[n]
 for p in mask:pixels() do if app.pixelColor.rgbaA(p())>0 then p(flat) end end
 blockout:drawImage(mask)
end
local board=Image(160,32,ColorMode.RGB)
for n,c in ipairs({0xffdbdce0,0xffdbdce0,0xffdbdce0,0xff272722,0xff777d72}) do
 local tile=Image(32,32,ColorMode.RGB);tile:clear(c)
 tile:drawImage(n==1 and silhouette or n==2 and blockout or image)
 board:drawImage(tile,Point((n-1)*32,0))
end
board:resize(1280,256);board:saveAs(dir..'working/review-x8.png')
