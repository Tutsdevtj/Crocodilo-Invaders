-- Revise the existing frames only. No portrait, regeneration or palette changes.
local root=assert(app.params.root):gsub('\\','/')
local prior=root..'/SourceArt/CrocodiloIdle64-v01/'
local dir=root..'/SourceArt/CrocodiloIdle64-v02/'
local old={};local frames={}
for f=1,4 do
 local source=Sprite{fromFile=prior..string.format('frames/idle_%02d.png',f-1)}
 local im=Image(64,64,ColorMode.RGB);im:drawSprite(source,1);old[f]=im;frames[f]=Image(im)
end
local base=old[1]
local function opaque(c) return app.pixelColor.rgbaA(c)>0 end
local masks={}
local function mask(name,ranges)
 local m={}
 for y,r in pairs(ranges) do for x=r[1],r[2] do if opaque(base:getPixel(x,y)) then m[y*64+x]=true end end end
 masks[name]=m;return m
end
local larm={}
for y=26,29 do larm[y]={18,26} end
larm[30]={18,25};for y=31,34 do larm[y]={18,23} end
larm[35]={19,23};larm[36]={20,24};larm[37]={20,23};larm[38]={20,24}
for y=39,41 do larm[y]={20,25} end
larm[42]={19,25};larm[43]={20,24};larm[44]={23,25}
local rarm={}
for y=26,30 do rarm[y]={42,45} end
rarm[31]={43,45};for y=32,35 do rarm[y]={42,46} end
for y=36,37 do rarm[y]={43,46} end
rarm[38]={44,46};for y=39,43 do rarm[y]={43,46} end;rarm[44]={43,45}
local left=mask('left_arm',larm);local right=mask('right_arm',rarm)
local chest=mask('chest',{[31]={26,29},[32]={26,29}})
local pelvis=mask('pelvis',{[34]={26,41},[35]={26,41},[36]={26,41}})
local knee=mask('near_knee',{[48]={26,30},[49]={24,30},[50]={25,30},[51]={24,30}})
local tr={};for y=47,53 do tr[y]={10,18} end
local tail=mask('tail_tip',tr)
local allowed={}
local operations={}
local function translate(frame,m,dx,dy,reason,repair)
 local src=Image(frames[frame]);local result=Image(src)
 for k in pairs(m) do
  local x,y=k%64,k//64
  result:drawPixel(x,y,0)
  allowed[k]=true;allowed[(y+dy)*64+x+dx]=true
 end
 for k in pairs(m) do local x,y=k%64,k//64;result:drawPixel(x+dx,y+dy,src:getPixel(x,y)) end
 if repair=='lower_arm' then
  for k in pairs(m) do
   local x,y=k%64,k//64
   if y>=40 and not opaque(result:getPixel(x,y)) and not m[(y+1)*64+x] and opaque(src:getPixel(x,y+1)) then
    result:drawPixel(x,y,src:getPixel(x,y+1))
   end
  end
 elseif repair=='chest' then
  for y=31,32 do result:drawPixel(29,y,src:getPixel(30,y));allowed[y*64+29]=true end
 elseif repair=='tail' then
  -- Blend the moving distal piece into the stationary mid-tail, not the hip.
  for y=47,51 do
   if not opaque(result:getPixel(18,y)) and opaque(src:getPixel(19,y)) then
    result:drawPixel(18,y,src:getPixel(19,y));allowed[y*64+18]=true
   end
  end
 end
 frames[frame]=result
 operations[#operations+1]=string.format('{"frame":%d,"dx":%d,"dy":%d,"reason":"%s"}',frame-1,dx,dy,reason)
end
-- Anticipation: actual shirt contour expands locally while shoulders keep old motion.
translate(2,chest,-1,0,'Chest expands before limb response','chest')
translate(3,chest,-1,0,'Chest maintains inspiration amplitude','chest')
-- Delayed response: cuffs, forearms and hands follow the shoulders at the peak.
for _,f in ipairs({3,4}) do
 translate(f,left,0,-1,'Left cuff elbow and hand respond with delayed one-pixel rise','lower_arm')
 translate(f,right,0,-1,'Right cuff elbow and hand respond with delayed one-pixel rise','lower_arm')
end
-- Small lateral counterweight, rather than vertical squatting.
translate(3,pelvis,-1,0,'Upper pelvis makes a lateral one-pixel counterweight')
-- Knee contour remains stable: the trial lateral shift opened distracting tail/leg gaps.
-- Pelvis counterweight provides the optional lower-body accommodation without a squat.
-- Tail responds last. Its root at the hip and the central length are stationary.
translate(4,tail,0,-1,'Distal tail tip follows on exhalation','tail')
local colors={};local paletteSet={}
for p in base:pixels() do if opaque(p()) and not paletteSet[p()] then colors[#colors+1]=p();paletteSet[p()]=true end end
assert(#colors==23)
local palette=Palette(25);palette:setColor(0,Color{r=0,g=0,b=0,a=0})
for i,c in ipairs(colors) do palette:setColor(i,Color{r=app.pixelColor.rgbaR(c),g=app.pixelColor.rgbaG(c),b=app.pixelColor.rgbaB(c),a=255}) end
local bg=app.pixelColor.rgba(226,228,230,255)
palette:setColor(24,Color{r=226,g=228,b=230,a=255})
local durations={450,250,450,350}
local sprite=Sprite(64,64,ColorMode.RGB);sprite:setPalette(palette)
sprite.layers[1].name='Revised idle - coordinated response'
local reference=sprite:newLayer();reference.name='Approved neutral - hidden';reference.isVisible=false
sprite:newCel(reference,1,Image(base),Point(0,0))
local sheet=Image(256,64,ColorMode.RGB)
for f,im in ipairs(frames) do
 if f>1 then sprite:newEmptyFrame(f) end
 sprite.frames[f].duration=durations[f]/1000
 sprite:newCel(sprite.layers[1],f,im,Point(0,0))
 im:saveAs(dir..string.format('frames/idle_%02d.png',f-1));sheet:drawImage(im,Point((f-1)*64,0))
 for y=0,63 do for x=0,63 do
  local c=im:getPixel(x,y)
  assert(not opaque(c) or paletteSet[c],'Colour outside approved palette')
  if y>=55 then assert(c==base:getPixel(x,y),'Boot pixels changed') end
  if c~=old[f]:getPixel(x,y) then assert(allowed[y*64+x],'Change outside recorded mask') end
 end end
end
local tag=sprite:newTag(1,4);tag.name='Idle';tag.aniDir=AniDir.FORWARD
sprite.data='Revised existing 64x64 idle. Chest/head lead; arms follow and linger; distal tail and knee settle last. Four distinct drawings, 1.5 sec. Boots rows55-63 invariant; cell origin unchanged.'
sprite:saveAs(dir..'crocodilo-idle-revisado-64.aseprite')
sheet:saveAs(dir..'crocodilo-idle-revisado-sheet.png')
local maskImage=Image(64,64,ColorMode.RGB)
for k in pairs(allowed) do maskImage:drawPixel(k%64,k//64,app.pixelColor.rgba(255,255,255,255)) end
maskImage:saveAs(dir..'working/allowed-mask.png')
for name,m in pairs(masks) do
 local mi=Image(64,64,ColorMode.RGB);for k in pairs(m) do mi:drawPixel(k%64,k//64,app.pixelColor.rgba(255,255,255,255)) end
 mi:saveAs(dir..'working/'..name..'-mask.png')
end
local function exportGif(filename,comparison)
 local width=comparison and 136 or 64;local scale=comparison and 6 or 8
 local gif=Sprite(width*scale,64*scale,ColorMode.RGB);gif:setPalette(palette)
 for f,im in ipairs(frames) do
  if f>1 then gif:newEmptyFrame(f) end;gif.frames[f].duration=durations[f]/1000
  local canvas=Image(width,64,ColorMode.RGB);canvas:clear(bg)
  if comparison then canvas:drawImage(old[f],Point(0,0));canvas:drawImage(im,Point(72,0)) else canvas:drawImage(im) end
  canvas:resize(width*scale,64*scale);gif:newCel(gif.layers[1],f,canvas,Point(0,0))
 end
 app.activeSprite=gif;app.command.ChangePixelFormat{ui=false,format='indexed',dithering='none'};gif:saveAs(dir..filename)
end
exportGif('crocodilo-idle-revisado-x8.gif',false)
exportGif('crocodilo-idle-comparacao.gif',true)
local board=Image(256,128,ColorMode.RGB);board:clear(bg)
for f,im in ipairs(frames) do board:drawImage(old[f],Point((f-1)*64,0));board:drawImage(im,Point((f-1)*64,64)) end
board:resize(1536,768);board:saveAs(dir..'working/before-after-frames-x6.png')
local meta=assert(io.open(dir..'timing.json','w'))
meta:write('{"canvas":[64,64],"cell_origin":[0,0],"sheet_size":[256,64],"loop":true,"total_ms":1500,"durations_ms":[450,250,450,350],"order":["frames/idle_00.png","frames/idle_01.png","frames/idle_02.png","frames/idle_03.png"],"comparison":{"left":"previous","right":"revised","scale":6},"operations":['..table.concat(operations,',')..']}\n');meta:close()
