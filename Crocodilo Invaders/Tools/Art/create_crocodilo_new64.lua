-- New original-portrait generation, finalized on a blank 64x64 native canvas.
-- Does not read any earlier full-body artwork or its construction.
local root=assert(app.params.root):gsub('\\','/')
local dir=root..'/SourceArt/CrocodiloNovo64-v01/'
local study=Sprite{fromFile=dir..'working/new-study.png'}
local pixels=Image(study.width,study.height,ColorMode.RGB)
pixels:drawSprite(study,1)
pixels:resize(64,64)
for p in pixels:pixels() do
 local c=p()
 if app.pixelColor.rgbaA(c)<128 then p(0)
 else p(app.pixelColor.rgba(app.pixelColor.rgbaR(c),app.pixelColor.rgbaG(c),app.pixelColor.rgbaB(c),255)) end
end
local native=Sprite(64,64,ColorMode.RGB)
native:newCel(native.layers[1],1,pixels,Point(0,0))
app.activeSprite=native
app.command.ColorQuantization{ui=false,withAlpha=true,maxColors=25,algorithm='octree'}
app.command.ChangePixelFormat{ui=false,format='indexed',dithering='none',rgbmap='octree'}
local out=Image(64,64,ColorMode.RGB)
out:drawSprite(native,1)
-- Consolidate redundant cloth shades into larger clusters, leaving face and joints intact.
local function rgb(hex) return app.pixelColor.rgba((hex>>16)&255,(hex>>8)&255,hex&255,255) end
local cloth={ [rgb(0x776d46)]=rgb(0x6c6336), [rgb(0x403e1d)]=rgb(0x4f4b29) }
for p in out:pixels() do
 if p.y>=18 and p.y<=32 and cloth[p()] then p(cloth[p()]) end
end
-- Remove isolated speckles within the shirt, preserving seams and pocket edges.
local cleaned=Image(out)
local base,shadow=rgb(0x6c6336),rgb(0x4f4b29)
for y=20,31 do for x=24,42 do
 local c=out:getPixel(x,y)
 if c==base or c==shadow then
  local same,opposite=0,0
  for dy=-1,1 do for dx=-1,1 do if dx~=0 or dy~=0 then
   local n=out:getPixel(x+dx,y+dy)
   if n==c then same=same+1 elseif n==(c==base and shadow or base) then opposite=opposite+1 end
  end end end
  if same==0 and opposite>=5 then cleaned:drawPixel(x,y,c==base and shadow or base) end
 end
end end
out=cleaned
out:saveAs(dir..'crocodilo-novo-base-64.png')
local enlarged=Image(64,64,ColorMode.RGB)
enlarged:clear(app.pixelColor.rgba(226,228,230,255))
enlarged:drawImage(out)
enlarged:resize(512,512)
enlarged:saveAs(dir..'crocodilo-novo-preview-x8.png')
local silhouette=Image(out)
for p in silhouette:pixels() do if app.pixelColor.rgbaA(p())>0 then p(app.pixelColor.rgba(25,39,30,255)) end end
local board=Image(256,64,ColorMode.RGB)
for n,c in ipairs({0xffe2e4e6,0xffe2e4e6,0xff242720,0xff8a8b82}) do
 local tile=Image(64,64,ColorMode.RGB);tile:clear(c)
 tile:drawImage(n==1 and silhouette or out)
 board:drawImage(tile,Point((n-1)*64,0))
end
board:resize(1536,384);board:saveAs(dir..'working/review-x6.png')
