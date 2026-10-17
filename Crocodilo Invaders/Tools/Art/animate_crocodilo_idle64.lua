-- Deterministic four-position breathing idle of the approved existing artwork.
local root=assert(app.params.root):gsub('\\','/')
local dir=root..'/SourceArt/CrocodiloIdle64-v01/'
local source=Sprite{fromFile=root..'/SourceArt/CrocodiloNovo64-v01/crocodilo-novo-base-64.png'}
assert(source.width==64 and source.height==64)
local base=Image(64,64,ColorMode.RGB);base:drawSprite(source,1)
local function opaque(c) return app.pixelColor.rgbaA(c)>0 end
local function maskRange(mask,y,left,right)
 for x=left,right do if opaque(base:getPixel(x,y)) then mask[y*64+x]=true end end
end
local head={}
for y=3,15 do maskRange(head,y,25,46) end
maskRange(head,16,29,46);maskRange(head,17,29,46);maskRange(head,18,31,44)
local caps={}
local left={ [19]={22,30},[20]={21,30},[21]={21,29},[22]={20,29},[23]={20,28},[24]={19,28} }
local right={ [20]={40,44},[21]={40,44},[22]={39,44},[23]={40,44},[24]={40,44} }
for y,r in pairs(left) do maskRange(caps,y,r[1],r[2]) end
for y,r in pairs(right) do maskRange(caps,y,r[1],r[2]) end
for k in pairs(caps) do assert(not head[k],'Shoulder mask contains head pixels') end
local upper={}
for y=3,25 do maskRange(upper,y,0,63) end
local allowed={}
local function lift(mask)
 local result=Image(base)
 for k in pairs(mask) do local x,y=k%64,k//64;result:drawPixel(x,y,0);allowed[k]=true;allowed[(y-1)*64+x]=true end
 for k in pairs(mask) do local x,y=k%64,k//64;result:drawPixel(x,y-1,base:getPixel(x,y)) end
 -- Resolve the lower attachment row only, keeping the lower torso fixed.
 for k in pairs(mask) do
  local x,y=k%64,k//64
  if not mask[(y+1)*64+x] and opaque(base:getPixel(x,y+1)) and not opaque(result:getPixel(x,y)) then
   result:drawPixel(x,y,base:getPixel(x,y+1))
  end
 end
 return result
end
-- Build the maximum first, then the anticipation. F3 deliberately reuses F1.
local maximum=lift(upper)
local inhale=lift(caps)
local frames={Image(base),inhale,maximum,Image(inhale)}
local durations={450,250,450,350}
local order={'repouso','inicio_da_inspiracao','apice','expiracao'}
local paletteColors={};local paletteSet={}
for p in base:pixels() do if opaque(p()) and not paletteSet[p()] then paletteSet[p()]=true;paletteColors[#paletteColors+1]=p() end end
assert(#paletteColors==23)
local palette=Palette(25);palette:setColor(0,Color{r=0,g=0,b=0,a=0})
for i,c in ipairs(paletteColors) do palette:setColor(i,Color{r=app.pixelColor.rgbaR(c),g=app.pixelColor.rgbaG(c),b=app.pixelColor.rgbaB(c),a=255}) end
palette:setColor(24,Color{r=226,g=228,b=230,a=255})
local sprite=Sprite(64,64,ColorMode.RGB);sprite:setPalette(palette)
sprite.layers[1].name='Idle - copies and local articulation'
local locked=sprite:newLayer();locked.name='Approved F0 - hidden reference';locked.isVisible=false
sprite:newCel(locked,1,Image(base),Point(0,0))
local sheet=Image(256,64,ColorMode.RGB)
local editable=Image(64,64,ColorMode.RGB)
for k in pairs(allowed) do editable:drawPixel(k%64,k//64,app.pixelColor.rgba(255,255,255,255)) end
editable:saveAs(dir..'working/editable-mask.png')
local headMask=Image(64,64,ColorMode.RGB)
for k in pairs(head) do headMask:drawPixel(k%64,k//64,app.pixelColor.rgba(255,255,255,255)) end
headMask:saveAs(dir..'working/head-mask.png')
local changes={}
for i,im in ipairs(frames) do
 if i>1 then sprite:newEmptyFrame(i) end
 sprite.frames[i].duration=durations[i]/1000
 sprite:newCel(sprite.layers[1],i,im,Point(0,0))
 im:saveAs(dir..string.format('frames/idle_%02d.png',i-1))
 sheet:drawImage(im,Point((i-1)*64,0))
 local diffs={}
 for y=0,63 do for x=0,63 do
  local before,after=base:getPixel(x,y),im:getPixel(x,y)
  if before~=after then
   assert(y<34,'Protected lower region changed')
   assert(allowed[y*64+x],'Outside allowed mask')
   assert(not opaque(after) or paletteSet[after],'Unapproved colour')
   diffs[#diffs+1]=string.format('{"x":%d,"y":%d,"before":%d,"after":%d}',x,y,before,after)
  end
 end end
 changes[#changes+1]='['..table.concat(diffs,',')..']'
end
local tag=sprite:newTag(1,4);tag.name='Idle';tag.aniDir=AniDir.FORWARD
sprite.data='64x64 breathing idle. Approved base preserved. Four timeline positions, three unique drawings; F3 reuses F1. Lower rows 34-63 fixed. 450/250/450/350 ms.'
sprite:saveAs(dir..'crocodilo-idle-64.aseprite')
sheet:saveAs(dir..'crocodilo-idle-sheet-64.png')
local previewSprite=Sprite(512,512,ColorMode.RGB);previewSprite:setPalette(palette)
for i,im in ipairs(frames) do
 if i>1 then previewSprite:newEmptyFrame(i) end
 previewSprite.frames[i].duration=durations[i]/1000
 local tile=Image(64,64,ColorMode.RGB);tile:clear(app.pixelColor.rgba(226,228,230,255));tile:drawImage(im);tile:resize(512,512)
 previewSprite:newCel(previewSprite.layers[1],i,tile,Point(0,0))
end
app.activeSprite=previewSprite
app.command.ChangePixelFormat{ui=false,format='indexed',dithering='none'}
previewSprite:saveAs(dir..'crocodilo-idle-preview-x8.gif')
local review=Image(256,64,ColorMode.RGB);review:clear(app.pixelColor.rgba(226,228,230,255))
review:drawImage(sheet);review:resize(2048,512);review:saveAs(dir..'working/frames-x8.png')
local manifest=assert(io.open(dir..'working/patches.json','w'))
manifest:write('{"base":"approved_original_pixels","operation":"integer_translation_and_attachment_repair","protected_rows":[34,63],"reuse":{"F3":"F1"},"frames":['..table.concat(changes,',')..']}\n');manifest:close()
local meta=assert(io.open(dir..'crocodilo-idle-timing.json','w'))
meta:write('{"canvas":[64,64],"sheet":"crocodilo-idle-sheet-64.png","sheet_size":[256,64],"loop":true,"total_ms":1500,"frames":[')
for i=1,4 do if i>1 then meta:write(',') end;meta:write(string.format('{"file":"frames/idle_%02d.png","phase":"%s","x":%d,"y":0,"w":64,"h":64,"duration_ms":%d}',i-1,order[i],(i-1)*64,durations[i])) end
meta:write('],"reuse":{"idle_03.png":"idle_01.png"}}\n');meta:close()
