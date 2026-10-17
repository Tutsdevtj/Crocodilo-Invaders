-- Original 32x32 pixel artwork, authored on a fixed pixel grid in Aseprite.
-- Run in batch with --script-param root=<project directory> --script <this file>.
local root = assert(app.params.root, 'Pass --script-param root=<project directory>'):gsub('\\', '/')
local sourcePath = root .. '/SourceArt/CrocodiloBase/crocodilo-base-idle.aseprite'
local artPath = root .. '/Assets/Art/Characters/CrocodiloBase/'
local previewPath = root .. '/Docs/ArtPreviews/'

-- Sixteen opaque colours. Zero is transparent; no anti-aliasing or partial alpha.
local colours = {
  {'O', 0x192a25, 'Contour'},
  {'S', 0x223328, 'Skin shade'},
  {'G', 0x3b4d31, 'Skin'},
  {'H', 0x697d44, 'Skin light'},
  {'C', 0xe2d09b, 'Teeth'},
  {'D', 0x393e2b, 'Uniform shade'},
  {'T', 0x5d6041, 'Shirt and helmet'},
  {'L', 0x81805a, 'Uniform light'},
  {'P', 0x6c6949, 'Trousers'},
  {'Q', 0x4b4b34, 'Trousers shade'},
  {'B', 0x38372d, 'Boots'},
  {'b', 0x595041, 'Boots light'},
  {'E', 0xc6a04c, 'Iris and brass'},
  {'R', 0x948967, 'Cloth light'},
  {'J', 0xb3a272, 'Pale jaw'},
  {'F', 0x344530, 'Skin mid shade'},
}
local index = { ['.'] = 0 }
local palette = Palette(#colours + 1)
palette:setColor(0, Color{r=0, g=0, b=0, a=0})
for i, entry in ipairs(colours) do
  index[entry[1]] = i
  local hex = entry[2]
  palette:setColor(i, Color{
    r=(hex >> 16) & 255, g=(hex >> 8) & 255, b=hex & 255, a=255
  })
end

local sprite = Sprite(32, 32, ColorMode.INDEXED)
sprite:setPalette(palette)
sprite.transparentColor = 0
sprite.data = 'Revision 3: identity matched to supplied portrait. Wide dark-green head, round olive helmet, narrow amber eyes, heavy muzzle and sand-coloured lower jaw. Resting uniform. 32x32, 16 colours; 8-frame idle. Pivot (16,31).'
local layers = { sprite.layers[1] }
layers[1].name = '01 Cauda'
for _, name in ipairs({'02 Calca e botas', '03 Camisa', '04 Bracos', '05 Cabeca e capacete'}) do
  local layer = sprite:newLayer()
  layer.name = name
  table.insert(layers, layer)
end

local function pixel(image, x, y, symbol)
  y = y - 1 -- One clear pixel below the soles, even at the relaxed extreme.
  assert(x >= 0 and x < 32 and y >= 0 and y < 32, 'Pixel outside 32x32')
  image:drawPixel(x, y, assert(index[symbol], 'Unknown palette symbol: ' .. symbol))
end

local function row(image, x, y, text)
  for i = 1, #text do
    local symbol = text:sub(i,i)
    if symbol ~= '.' then pixel(image, x+i-1, y, symbol) end
  end
end

local function stamp(image, x, y, rows)
  for r, text in ipairs(rows) do row(image, x, y+r-1, text) end
end

local function tail(image)
  stamp(image, 4, 21, {
    '........OOO',
    '......OOGGSO',
    '....OOHGGSSO',
    '...OHGGSSOO',
    '..OGGSSOO',
    '.OGSSOO',
    'OSSOO',
    '.OO',
  })
  pixel(image, 10, 22, 'S')
  pixel(image, 8, 24, 'S')
end

local function trousers(image)
  stamp(image, 11, 23, {
    '.OBBBBEBBBBO',
    '.OPRPPPPQQQO',
    '..OPPQOPRPO',
    '..ORPQ.OPRQO',
    '..OPPQ.OPPQO',
    '..OQQO.OQQQO',
    '.ObbBO.ObbbBO',
    'OBBBBO.OBBBBBO',
    '.OOOOO.OOOOOO',
  })
end

local shirtRows = {
  {12, 13, 'OOOLJJJLDOO'},
  {10, 14, 'OODLLJDJLTTDOO'},
  { 8, 15, 'ODTLLTTDTLLTTDDO'},
  { 8, 16, 'OTTTLTTDTLTTTTDO'},
  {10, 17, 'ODDTLLTDTLLDDO'},
  {12, 18, 'OTLLTDTLLDO'},
  {12, 19, 'OTTDTDTTDDO'},
  {12, 20, 'OTDDTDTDDDO'},
  {12, 21, 'OLTTTDTTDDO'},
  {12, 22, 'ODTTTDTTDDO'},
}

local function shirt(image, raised, expanded)
  for _, r in ipairs(shirtRows) do row(image, r[1], r[2]-raised, r[3]) end
  if raised == 1 then row(image, 12, 22, 'ODTTTDTTDDO') end
  if expanded == 1 then
    -- Expand the ribcage by one pixel only; pelvis and feet remain fixed.
    row(image, 21, 18, 'TDO')
    row(image, 21, 19, 'DDO')
    row(image, 21, 20, 'DDO')
  end
end

local function arms(image, raised)
  stamp(image, 9, 17-raised, {'ODDO', 'OSGO', 'OFGO', 'OFGO', 'OSGO', 'OGFO', 'OGSO', '.OO'})
  stamp(image, 23, 17-raised, {'ODO', 'OHGO', 'OHGSO', 'OGGSO', 'OGGSO', 'OHGSO', 'OGFSO', '.OOO'})
end

local headRows = {
  '.....OOOOOO.........',
  '...OOTTTTTTOO.......',
  '..ODTTLLLLTTTO......',
  '.ODTTLLLTTTTTDO.....',
  'OODTTTTTTTTTTDOO....',
  '.OOODDDDDDDDOOO.....',
  '.OSGGSEOOSGEEO......',
  'OSGFFGOOSGGHFOOOO...',
  'OSGGGGGFGGHHGGGSFOO.',
  '.OSGGSGGGGGSOGGGSFOO',
  '..OSGGGGOCOCOOOOOOO.',
  '...OSGJJJJJJJJJFSOO.',
  '....OSFJJJJJJFOO....',
}

local function head(image, raised)
  stamp(image, 9, 3-raised, headRows)
  pixel(image, 26, 11-raised, 'O') -- One nostril on the broad muzzle.
end

-- Inhale: shoulders lead, head follows, then hands. Reverse this on exhale.
-- Last and first poses intentionally match to hold the relaxed extreme.
local poses = {
  {shoulders=0, chest=0, head=0, arms=0, duration=0.30},
  {shoulders=1, chest=0, head=0, arms=0, duration=0.20},
  {shoulders=1, chest=1, head=1, arms=0, duration=0.20},
  {shoulders=1, chest=1, head=1, arms=1, duration=0.36},
  {shoulders=1, chest=0, head=1, arms=1, duration=0.20},
  {shoulders=0, chest=0, head=1, arms=1, duration=0.20},
  {shoulders=0, chest=0, head=0, arms=1, duration=0.20},
  {shoulders=0, chest=0, head=0, arms=0, duration=0.30},
}

for f, pose in ipairs(poses) do
  if f > 1 then sprite:newEmptyFrame(f) end
  sprite.frames[f].duration = pose.duration
  local images = {}
  for i = 1, #layers do images[i] = Image(32,32,ColorMode.INDEXED) end
  tail(images[1])
  trousers(images[2])
  shirt(images[3], pose.shoulders, pose.chest)
  arms(images[4], pose.arms)
  head(images[5], pose.head)
  for i, layer in ipairs(layers) do sprite:newCel(layer, f, images[i], Point(0,0)) end
end

local tag = sprite:newTag(1,8)
tag.name = 'Idle_Respirando'
tag.aniDir = AniDir.FORWARD

-- Exact pixel exports. Enlarged previews are nearest-neighbour copies.
sprite:saveAs(sourcePath)
local sheet = Image(256,32,ColorMode.RGB)
local renders = {}
local coloursUsed, totalTime = {}, 0
for f = 1,8 do
  local render = Image(32,32,ColorMode.RGB)
  render:drawSprite(sprite, f)
  renders[f] = render
  sheet:drawImage(render, Point((f-1)*32,0))
  totalTime = totalTime + sprite.frames[f].duration
  for p in render:pixels() do
    local c = p()
    local alpha = app.pixelColor.rgbaA(c)
    assert(alpha == 0 or alpha == 255, 'Unexpected partial alpha')
    if alpha == 255 then coloursUsed[c] = true end
  end
end
for f = 2,8 do
  for y=25,31 do
    for x=0,31 do
      assert(renders[1]:getPixel(x,y) == renders[f]:getPixel(x,y), 'Feet or base drift')
    end
  end
end
assert(renders[1]:isEqual(renders[8]), 'Loop does not close cleanly')
local colourCount = 0
for _ in pairs(coloursUsed) do colourCount = colourCount + 1 end
assert(colourCount <= 16, 'Palette exceeded sixteen opaque colours')
sheet:saveAs(artPath .. 'crocodilo-base-idle.png')
renders[1]:saveAs(artPath .. 'crocodilo-base-idle-pose.png')
local metadata = {
  frames = {},
  meta = {
    app = 'https://www.aseprite.org/',
    image = 'crocodilo-base-idle.png',
    format = 'RGBA8888', size = {w=256,h=32}, scale='1',
    frameTags = {{name='Idle_Respirando',from=0,to=7,direction='forward'}},
  },
}
for f=1,8 do
  table.insert(metadata.frames, {
    filename=string.format('CrocodiloBase_Idle_%02d',f-1),
    frame={x=(f-1)*32,y=0,w=32,h=32},
    rotated=false, trimmed=false,
    spriteSourceSize={x=0,y=0,w=32,h=32}, sourceSize={w=32,h=32},
    duration=math.floor(sprite.frames[f].duration*1000+0.5),
  })
end
local metadataFile = assert(io.open(artPath .. 'crocodilo-base-idle.json','w'))
metadataFile:write(json.encode(metadata))
metadataFile:close()
local matte = app.pixelColor.rgba(219,217,202,255)
local function previewImage(render)
  local result = Image(32,32,ColorMode.RGB)
  result:clear(matte)
  result:drawImage(render)
  return result
end
local stillPreview = previewImage(renders[1])
stillPreview:resize(256,256)
stillPreview:saveAs(previewPath .. 'crocodilo-base-idle-pose-x8.png')
stillPreview:saveAs(previewPath .. 'crocodilo-base-idle-v3-pose-x8.png')
local sheetPreview = Image(256,32,ColorMode.RGB)
sheetPreview:clear(matte)
sheetPreview:drawImage(sheet)
sheetPreview:resize(2048,256)
sheetPreview:saveAs(previewPath .. 'crocodilo-base-idle-frames-x8.png')
sheetPreview:saveAs(previewPath .. 'crocodilo-base-idle-v3-frames-x8.png')

local preview = Sprite(256,256,ColorMode.RGB)
for f=1,8 do
  if f>1 then preview:newEmptyFrame(f) end
  preview.frames[f].duration = sprite.frames[f].duration
  local enlarged = previewImage(renders[f])
  enlarged:resize(256,256)
  preview:newCel(preview.layers[1], f, enlarged, Point(0,0))
end
preview:saveCopyAs(previewPath .. 'crocodilo-base-idle-x8.gif')
preview:saveCopyAs(previewPath .. 'crocodilo-base-idle-v3-x8.gif')

local info = assert(io.open(previewPath .. 'crocodilo-base-idle-validation.txt','w'))
info:write(string.format('Canvas: 32x32\nFrames: 8\nOpaque colours: %d\nDuration: %.2fs\nAlpha: binary\nFeet: fixed\nLoop: first and last pose equal\n', colourCount,totalTime))
local signatures = {}
local uniquePoses = 0
for f=1,8 do
  local signature = {}
  local bounds = renders[f]:shrinkBounds()
  local previous = renders[f==1 and 8 or f-1]
  local changed = 0
  for p in renders[f]:pixels() do
    table.insert(signature,tostring(p()))
    if p() ~= previous:getPixel(p.x,p.y) then changed = changed + 1 end
  end
  local key=table.concat(signature,',')
  if not signatures[key] then signatures[key]=true; uniquePoses=uniquePoses+1 end
  info:write(string.format('Frame %d: %dms; bounds (%d,%d,%d,%d); changed pixels %d\n',
    f,math.floor(sprite.frames[f].duration*1000+0.5),bounds.x,bounds.y,bounds.width,bounds.height,changed))
end
info:write('Unique poses: ' .. uniquePoses .. '\n')
info:close()
