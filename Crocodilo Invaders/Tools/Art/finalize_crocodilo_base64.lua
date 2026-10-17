-- Finalize the reference-driven concept as ONE native 64x64 Aseprite frame.
local root = assert(app.params.root, 'Pass the project root'):gsub('\\','/')
local source = Sprite{fromFile=root .. '/SourceArt/CrocodiloBase64/crocodilo-base-64-concept.png'}
assert(#source.frames == 1, 'The concept must contain a single frame')
local image = Image(source.width,source.height,ColorMode.RGB)
image:drawSprite(source,1)
image:resize(64,64) -- Aseprite defaults to nearest-neighbour; no interpolated pixels.

-- Native pixel art has binary alpha. Preserve the generated transparent background.
for p in image:pixels() do
  local c = p()
  if app.pixelColor.rgbaA(c) < 128 then
    p(0)
  else
    p(app.pixelColor.rgba(app.pixelColor.rgbaR(c),app.pixelColor.rgbaG(c),app.pixelColor.rgbaB(c),255))
  end
end

local sprite = Sprite(64,64,ColorMode.RGB)
sprite.layers[1].name = 'Crocodilo da base - pose'
sprite:newCel(sprite.layers[1],1,image,Point(0,0))
sprite.data = '64x64 single frame. Portrait-driven redesign; olive helmet, dark-green crocodile, sand jaw, service uniform, tail and boots. ImageGen concept finalized in Aseprite.'
app.activeSprite = sprite
app.command.ColorQuantization{ui=false,withAlpha=true,maxColors=32,algorithm='octree'}
app.command.ChangePixelFormat{ui=false,format='indexed',dithering='none',rgbmap='octree'}
assert(sprite.width == 64 and sprite.height == 64 and #sprite.frames == 1)
sprite:saveAs(root .. '/SourceArt/CrocodiloBase64/crocodilo-base-64-pose.aseprite')

local render = Image(64,64,ColorMode.RGB)
render:drawSprite(sprite,1)
local used = {}
local transparentCount=0
for p in render:pixels() do
  local c=p()
  local alpha=app.pixelColor.rgbaA(c)
  assert(alpha==0 or alpha==255, 'Unexpected partial alpha')
  if alpha==0 then transparentCount=transparentCount+1 else used[c]=true end
end
local opaqueColours=0
for _ in pairs(used) do opaqueColours=opaqueColours+1 end
assert(opaqueColours<=32)
assert(transparentCount>100, 'Transparent background missing')
local bounds=render:shrinkBounds()
assert(bounds.x>0 and bounds.y>0 and bounds.x+bounds.width<64 and bounds.y+bounds.height<64,'Figure touches canvas edge')
render:saveAs(root .. '/Assets/Art/Characters/CrocodiloBase64/crocodilo-base-64-pose.png')

local preview = Image(64,64,ColorMode.RGB)
preview:clear(app.pixelColor.rgba(219,217,202,255))
preview:drawImage(render)
preview:resize(512,512)
preview:saveAs(root .. '/Docs/ArtPreviews/crocodilo-base-64-pose-x8.png')
local transparentPreview=Image(render)
transparentPreview:resize(512,512)
transparentPreview:saveAs(root .. '/Docs/ArtPreviews/crocodilo-base-64-pose-transparent-x8.png')

local report=assert(io.open(root .. '/Docs/ArtPreviews/crocodilo-base-64-validation.txt','w'))
report:write(string.format('Canvas: 64x64\nFrames: 1\nOpaque colours: %d\nTransparent pixels: %d\nAlpha: binary\nBounds: (%d,%d,%d,%d)\nEnlargement: nearest-neighbour 8x\n',opaqueColours,transparentCount,bounds.x,bounds.y,bounds.width,bounds.height))
report:close()
