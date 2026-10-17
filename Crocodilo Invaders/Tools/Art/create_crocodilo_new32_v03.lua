-- Blank native canvas, only the newly generated original-portrait study is read.
local root=assert(app.params.root):gsub('\\','/')
local dir=root..'/SourceArt/CrocodiloNovo32-v03/'
local study=Sprite{fromFile=dir..'working/new-concept.png'}
local sampled=Image(study.width,study.height,ColorMode.RGB)
sampled:drawSprite(study,1)
sampled:resize(32,32)
for p in sampled:pixels() do
 local c=p()
 if app.pixelColor.rgbaA(c)<128 then p(0)
 else p(app.pixelColor.rgba(app.pixelColor.rgbaR(c),app.pixelColor.rgbaG(c),app.pixelColor.rgbaB(c),255)) end
end
local native=Sprite(32,32,ColorMode.RGB)
native:newCel(native.layers[1],1,sampled,Point(0,0))
app.activeSprite=native
app.command.ColorQuantization{ui=false,withAlpha=true,maxColors=19,algorithm='octree'}
app.command.ChangePixelFormat{ui=false,format='indexed',dithering='none',rgbmap='octree'}
local out=Image(32,32,ColorMode.RGB)
out:drawSprite(native,1)
out:saveAs(dir..'working/study32.png')
local large=Image(out);large:resize(256,256)
large:saveAs(dir..'working/study-x8.png')
local board=Image(96,32,ColorMode.RGB)
for n,c in ipairs({0xffe2e4e6,0xff242720,0xff8a8b82}) do
 local tile=Image(32,32,ColorMode.RGB);tile:clear(c);tile:drawImage(out)
 board:drawImage(tile,Point((n-1)*32,0))
end
board:resize(768,256);board:saveAs(dir..'working/context-x8.png')

-- New native master, built on an empty image from the new study only.
-- No previous sprite, palette, layer, mask or script is loaded.
local pal={A=0x19291f,B=0x405d38,C=0x69814b,D=0x293e2c,
 E=0x82915b,F=0x9c8864,G=0xd1b578,H=0x302b24,
 I=0xe9d69f,J=0x504332,K=0x39372b,L=0x82774e,
 M=0x535237,N=0xa09160,O=0x716348,P=0x61583b,
 Q=0x233728,R=0xb09a70,X=0xd8ad4d}
local rgba={}
for ch,v in pairs(pal) do rgba[ch]=app.pixelColor.rgba((v>>16)&255,(v>>8)&255,v&255,255) end
local master=Image(32,32,ColorMode.RGB)
local function row(x,y,text)
 for i=1,#text do local ch=text:sub(i,i);if ch~='.' then master:drawPixel(x+i-1,y,assert(rgba[ch])) end end
end
-- Fresh pixel masses transcribed from the newly generated body study.
local rows={
 [11]='...........KJLLFGFAJ............',
 [12]='..........MJLALLGGLLLA..........',
 [13]='..........JLMLLLAANLLL..........',
 [14]='.........ALJOKLLLLLLJL..........',
 [15]='.........OALKJJPLLPOQL..........',
 [16]='.........ALL.JLLLJLLJLA.........',
 [17]='.........BCA.JJPLJLLABQ.........',
 [18]='.........BC..AQPAPQA.BB.........',
 [19]='.........BC..OFFRFRF..C.........',
 [20]='.........ACQ.OFOOONFA.CA........',
 [21]='.........BCC.OFFAPRFF.AC........',
 [22]='.........AC.POFO.JFFP.AA........',
 [23]='............AFOJ.AFPP...........',
 [24]='............OOFA..JFF..........',
 [25]='...........AFMOA..NMJ..........',
 [26]='...........AOFO...PFR...........',
 [27]='............PHA...HHJ..........',
 [28]='............KK.....JK..........',
}
for y=11,28 do row(0,y,rows[y]) end
-- Group shadow clusters by material instead of retaining generated texture.
for y=11,18 do for x=10,21 do
 local c=master:getPixel(x,y)
 if c==rgba.J or c==rgba.K or c==rgba.P or c==rgba.O then master:drawPixel(x,y,rgba.M) end
end end
for y=20,27 do for x=12,21 do
 local c=master:getPixel(x,y)
 if c==rgba.J or c==rgba.M or c==rgba.N then master:drawPixel(x,y,rgba.O) end
 if c==rgba.P then master:drawPixel(x,y,rgba.O) end
end end
-- A fresh rounded helmet and thick cheek; short blunt muzzle and distinct jaw.
local head={
 {14,2,'AAAAA'},
 {12,3,'AABBBCCCAA'},
 {11,4,'ABBCCBBBBBA'},
 {11,5,'ABBBBBBBBBA'},
 {12,6,'ADDDDDDDDDA'},
 {11,7,'.ADXXADDDDA'},
 {11,8,'ADBBDDBBBCCBBBBAA'},
 {11,9,'ADBBBBBCCBBBBBDDA'},
 {12,10,'ADBBBDDAIADIAAAAA'},
 {13,11,'ADFGGGFFFFFFFA'},
 {14,12,'ADFGGGFFFFAA'},
}
for _,r in ipairs(head) do row(r[1],r[2],r[3]) end
master:drawPixel(25,8,rgba.A) -- Nostril on the newly extended blunt muzzle.
-- Neck and collar are separate connected shapes, rather than a mouth-to-shirt smear.
row(14,13,'AMGFMLA')
row(14,14,'LMMAMNL')
-- Slightly bent human forearms and small closed hands, separated from the waist.
row(9,17,'BCA');row(9,18,'BCA');row(9,19,'BDA')
row(9,20,'ABCA');master:drawPixel(12,20,0)
row(9,21,'BCDA');master:drawPixel(12,21,0)
row(9,22,'AAA')
-- Boots with both soles on y=30, no border pixels.
for y=28,31 do for x=9,24 do master:drawPixel(x,y,0) end end
row(12,28,'AKJA');row(18,28,'AKJA')
row(11,29,'AKJJA');row(18,29,'AKJJJAA')
row(10,30,'AKKKKA');row(18,30,'AKKKKKA')
master:saveAs(dir..'crocodilo-novo-base-32.png')
local enlarged=Image(master);enlarged:resize(256,256)
enlarged:saveAs(dir..'crocodilo-novo-preview-x8.png')
local silhouette=Image(master)
for p in silhouette:pixels() do if app.pixelColor.rgbaA(p())>0 then p(rgba.A) end end
local studyboard=Image(128,32,ColorMode.RGB)
for n,c in ipairs({0xffe2e4e6,0xffe2e4e6,0xff242720,0xff8a8b82}) do
 local tile=Image(32,32,ColorMode.RGB);tile:clear(c)
 tile:drawImage(n==1 and silhouette or master)
 studyboard:drawImage(tile,Point((n-1)*32,0))
end
studyboard:resize(1024,256);studyboard:saveAs(dir..'working/final-review-x8.png')
