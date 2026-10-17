-- One-frame base outfit. ImageGen concept normalized and reviewed in Aseprite.
local root = assert(app.params.root):gsub('\\', '/')
local dir = root .. '/SourceArt/CrocodiloBaseRelaxed32-v01/'
local source = Sprite{fromFile=dir .. 'concept.png'}
local img = Image(source.width, source.height, ColorMode.RGB)
img:drawSprite(source, 1)
img:resize(32, 32)
for p in img:pixels() do
  local c = p()
  if app.pixelColor.rgbaA(c) < 128 then p(0)
  else p(app.pixelColor.rgba(app.pixelColor.rgbaR(c), app.pixelColor.rgbaG(c), app.pixelColor.rgbaB(c),255)) end
end
local sprite = Sprite(32,32,ColorMode.RGB)
sprite.layers[1].name = 'Personagem - roupa da base'
sprite:newCel(sprite.layers[1],1,img,Point(0,0))
app.activeSprite = sprite
-- Explicit native-pixel cleanup, after reviewing the automatic reduction.
-- Palette is a proposal informed by the portrait, not a claimed exact extraction.
local hex={O=0x14231d,S=0x253b29,G=0x354e2e,H=0x4c623e,
 J=0xb99555,K=0xdabd78,T=0x54502b,D=0x373a1f,L=0x787747,
 P=0x604933,Q=0x403125,B=0x27291f,b=0x4b4a35,
 E=0xd4a143,C=0xead7a2,R=0x978a54}
local colors={}
for name,value in pairs(hex) do colors[name]=app.pixelColor.rgba((value>>16)&255,(value>>8)&255,value&255,255) end
local function px(x,y,s) img:drawPixel(x,y,colors[s] or 0) end
local function stamp(x,y,rows)
 for row,line in ipairs(rows) do
  for col=1,#line do local s=line:sub(col,col); if s~='.' then px(x+col-1,y+row-1,s) end end
 end
end
-- Remap sampled masses to deliberate material ramps, preserving the concept pose.
for p in img:pixels() do
 local c=p()
 if app.pixelColor.rgbaA(c)>0 then
  local r,g,b=app.pixelColor.rgbaR(c),app.pixelColor.rgbaG(c),app.pixelColor.rgbaB(c)
  local name
  if r<28 and g<30 and b<30 then name='O'
  elseif g>r*1.08 and g>b*1.15 then name=g>67 and 'G' or 'S'
  elseif p.y>=28 then name=(r+g+b)>130 and 'b' or 'B'
  elseif p.y>=22 and r>g*1.18 then name=r>61 and 'P' or 'Q'
  elseif r>g*1.28 and r>90 then name=r>180 and 'K' or 'J'
  elseif r+g+b<120 then name='D'
  else name='T' end
  p(colors[name])
 end
end
-- Remove the sampled head and redraw clean connected skull/cap/muzzle clusters.
for y=3,13 do for x=12,27 do img:drawPixel(x,y,0) end end
stamp(11,3,{
 '.....OOOOO......',
 '...OODLLTDO.....',
 '..ODLLTTTTDO....',
 '..ODTTDDDDDDOO..',
 '..OOSGGGSSOOOO..',
 '..OSGGEOSSGSO...',
 '..OSGGGSSGHGGGO.',
 '..OSGGSSGGGHHGO.',
 '..OSJJOOCGCOOOO.',
 '...OJKKKJJJJJO..',
 '....OJJKKKJOO...',
 '.....OSJKKJO....',
})
-- Economical collar, sleeve, cloth light and folds.
stamp(10,13,{'OTLDO','OTLTDO','OTLTTDO','OTTTDO'})
stamp(15,15,{'DLT','OTLT','OTLTT','DTLTT'})
stamp(20,15,{'OTDO','OTTO','OTDO'})
stamp(12,20,{'DDTTLTTTD','OTTTTTTD','OPPPQO'})
-- Near relaxed forearm and hand, far hand, with single-pixel separation.
stamp(9,18,{'OGSO','OGSO','OGSO','OGGO','OGGSO','OSSO'})
stamp(22,19,{'OSGO','OSGO','OSGO','OGGO','OGSO','OOO'})
-- Two independent trouser legs, articulated shadows and distinct boot material.
stamp(12,23,{'PPPQOQPPPO','PPPPOOPPPO','PPPQ..PPPO','PPPO..PPQO','OPQO..OQQO'})
stamp(11,28,{'OBbO...OBbOO','OBbBO..OBbbBO','OBBBO..OBBBBBO'})
sprite.cels[1].image=img
local palette=Palette(17)
palette:setColor(0,Color{r=0,g=0,b=0,a=0})
local order={'O','S','G','H','J','K','T','D','L','P','Q','B','b','E','C','R'}
for i,name in ipairs(order) do
 local v=hex[name]; palette:setColor(i,Color{r=(v>>16)&255,g=(v>>8)&255,b=v&255,a=255})
end
sprite:setPalette(palette)
app.command.ChangePixelFormat{ui=false,format='indexed',dithering='none'}
sprite.data = 'Single frame. 32x32. Relaxed off-duty crocodile. ImageGen concept retouched on native integer grid in Aseprite. Proposed 16-color palette. No animations.'
sprite:saveAs(dir .. 'crocodilo-base-relaxado-32.aseprite')
local render = Image(32,32,ColorMode.RGB)
render:drawSprite(sprite,1)
render:saveAs(dir .. 'crocodilo-base-relaxado-32.png')
local transparent = Image(render)
transparent:resize(256,256)
transparent:saveAs(dir .. 'crocodilo-base-relaxado-preview-x8.png')
local contexts = Image(96,32,ColorMode.RGB)
for i,c in ipairs({0xffd7dedf,0xff24221c,0xff727c78}) do
  local tile=Image(32,32,ColorMode.RGB)
  tile:clear(c)
  tile:drawImage(render)
  contexts:drawImage(tile,Point((i-1)*32,0))
end
contexts:resize(768,256)
contexts:saveAs(dir .. 'contextos-preview-x8.png')
