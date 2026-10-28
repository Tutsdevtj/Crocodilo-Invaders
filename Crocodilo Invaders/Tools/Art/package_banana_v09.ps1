param([string]$ModularSource,[string]$TransformationSource)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$root=Join-Path $project 'SourceArt/BananaPossessora_Fase1_Master_v09_Modular'
$preview=Join-Path $project 'Docs/ArtPreviews/BananaPossessora_v09'
$transform=Join-Path $project 'SourceArt/BananaPossessora_Transformacao_v01'
$assets=Join-Path $project 'Assets/Art/Boss/BananaPossessora/Fase1'
foreach($dir in @("$root/parts","$root/ora/data",$preview,"$transform/frames",$assets)){New-Item -ItemType Directory -Force $dir | Out-Null}
if($ModularSource){Copy-Item -LiteralPath $ModularSource -Destination "$root/atlas_fonte.png"}
if($TransformationSource){Copy-Item -LiteralPath $TransformationSource -Destination "$transform/atlas_fonte.png"}
function New-Canvas([int]$w,[int]$h){return [Drawing.Bitmap]::new($w,$h,[Drawing.Imaging.PixelFormat]::Format32bppArgb)}
function Get-Graphics($bitmap){
 $g=[Drawing.Graphics]::FromImage($bitmap)
 $g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
 $g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
 $g.CompositingMode=[Drawing.Drawing2D.CompositingMode]::SourceOver
 $g.Clear([Drawing.Color]::Transparent)
 return $g
}
function Save-Png($im,$path){$im.Save($path,[Drawing.Imaging.ImageFormat]::Png)}
function Get-Bounds($im,[int[]]$region){
 $l=$region[0]+$region[2];$t=$region[1]+$region[3];$r=-1;$b=-1
 for($y=$region[1];$y -lt $region[1]+$region[3];$y++){for($x=$region[0];$x -lt $region[0]+$region[2];$x++){
  if($im.GetPixel($x,$y).A -ge 128){$l=[Math]::Min($l,$x);$t=[Math]::Min($t,$y);$r=[Math]::Max($r,$x);$b=[Math]::Max($b,$y)}
 }}
 if($r -lt 0){throw 'Empty sprite region'}
 return [Drawing.Rectangle]::new($l,$t,$r-$l+1,$b-$t+1)
}
function Hard-Alpha($im){
 for($y=0;$y -lt $im.Height;$y++){for($x=0;$x -lt $im.Width;$x++){
  $c=$im.GetPixel($x,$y)
  if($c.A -lt 128){$im.SetPixel($x,$y,[Drawing.Color]::Transparent)}else{$im.SetPixel($x,$y,[Drawing.Color]::FromArgb(255,$c.R,$c.G,$c.B))}
 }}
}
# Only extraction, nearest-neighbour export and registration; the artwork comes from image_gen.
$atlas=[Drawing.Bitmap]::new("$root/atlas_fonte.png")
$specs=@(
 @{id='corpo_banana';region=@(0,0,512,600);height=180;x=80;y=8;anchor=@(0.5,0.5)},
 @{id='braco_distante';region=@(512,0,512,600);height=105;x=13;y=96;anchor=@(0.89,0.10)},
 @{id='braco_proximo';region=@(1024,0,512,600);height=110;x=141;y=92;anchor=@(0.19,0.12)},
 @{id='perna_distante';region=@(0,600,512,424);height=101;x=30;y=157;anchor=@(0.82,0.10)},
 @{id='perna_proxima';region=@(512,600,512,424);height=104;x=126;y=154;anchor=@(0.19,0.12)},
 @{id='rabo';region=@(1024,600,512,424);height=65;x=130;y=132;anchor=@(0.10,0.64)}
)
$items=@{}
foreach($s in $specs){
 $rect=Get-Bounds $atlas $s.region
 $crop=$atlas.Clone($rect,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $w=[int][Math]::Round($crop.Width*$s.height/$crop.Height)
 $im=New-Canvas $w $s.height;$g=Get-Graphics $im
 $g.DrawImage($crop,[Drawing.Rectangle]::new(0,0,$w,$s.height));$g.Dispose();$crop.Dispose();Hard-Alpha $im
 Save-Png $im "$root/parts/$($s.id).png"
 $s.width=$w;$s.source_rect=@($rect.X,$rect.Y,$rect.Width,$rect.Height)
 $s.pivot_px=@([int][Math]::Round($s.anchor[0]*$w),[int][Math]::Round($s.anchor[1]*$s.height))
 $items[$s.id]=$im
}
$atlas.Dispose()
$order=@('rabo','perna_distante','braco_distante','perna_proxima','braco_proximo','corpo_banana')
$master=New-Canvas 256 272;$g=Get-Graphics $master
$layers=@();$svg=@('<svg xmlns="http://www.w3.org/2000/svg" width="256" height="272" viewBox="0 0 256 272" style="image-rendering:pixelated">');$layout=@()
foreach($id in $order){
 $s=$specs | Where-Object id -eq $id
 $im=$items[$id];$g.DrawImageUnscaled($im,$s.x,$s.y)
 Copy-Item "$root/parts/$id.png" "$root/ora/data/$id.png" -Force
 $layers+="<layer name='$id' src='data/$id.png' x='$($s.x)' y='$($s.y)' opacity='1.0' visibility='visible' composite-op='svg:src-over'/>"
 $encoded=[Convert]::ToBase64String([IO.File]::ReadAllBytes("$root/parts/$id.png"))
 $svg+="<g id='$id'><image x='$($s.x)' y='$($s.y)' width='$($s.width)' height='$($s.height)' href='data:image/png;base64,$encoded'/></g>"
 $layout+=@{id=$id;file="parts/$id.png";x=$s.x;y=$s.y;width=$s.width;height=$s.height;pivot_top_left_px=$s.pivot_px;source_rect=$s.source_rect;sorting_order=$layout.Count}
}
$g.Dispose();Save-Png $master "$root/master.png"
$svg+='</svg>';[IO.File]::WriteAllText("$root/master.svg",($svg -join "`n"))
[array]::Reverse($layers)
[IO.File]::WriteAllText("$root/ora/mimetype",'image/openraster')
[IO.File]::WriteAllText("$root/ora/stack.xml",("<?xml version='1.0' encoding='UTF-8'?><image version='0.0.3' w='256' h='272' name='Banana possessora v09'><stack>"+($layers -join '')+'</stack></image>'))
Copy-Item "$root/master.png" "$root/ora/mergedimage.png" -Force
if(Test-Path "$root/master.ora"){Remove-Item -LiteralPath "$root/master.ora"}
[IO.Compression.ZipFile]::CreateFromDirectory("$root/ora","$root/master.ora")
@{canvas=@(256,272);pixels_per_unit=100;draw_order='back_to_front';root_top_left_px=@(128,258);layers=$layout} | ConvertTo-Json -Depth 8 | Set-Content "$root/layout.json" -Encoding utf8
$zoom=New-Canvas 768 816;$g=Get-Graphics $zoom;$g.DrawImage($master,[Drawing.Rectangle]::new(0,0,768,816));$g.Dispose();Save-Png $zoom "$preview/master_x3.png";$zoom.Dispose()
# Exploded parts preview, true alpha, uniform 1:1 native pixel grid.
$partsPreview=New-Canvas 480 400;$g=Get-Graphics $partsPreview
for($i=0;$i -lt $specs.Count;$i++){$s=$specs[$i];$im=$items[$s.id];$g.DrawImageUnscaled($im,([int](($i%3)*160+(160-$im.Width)/2)),([int]([Math]::Floor($i/3)*200+(200-$im.Height)/2)))}
$g.Dispose();Save-Png $partsPreview "$preview/pecas.png";$partsPreview.Dispose()
# Unity import settings copied from a native Goosini sprite, with unique GUIDs and joint pivots.
$template=Get-Content (Join-Path $project 'Assets/Art/Boss/GusiniPhase1/gusini_p1_idle/gusini_p1_idle_0.png.meta') -Raw
foreach($s in $specs){
 $dest="$assets/$($s.id).png";Copy-Item "$root/parts/$($s.id).png" $dest -Force
  $guid=[guid]::NewGuid().ToString('N')
  if(Test-Path "$dest.meta"){$guid=([regex]::Match((Get-Content "$dest.meta" -Raw),'(?m)^guid: (\w+)')).Groups[1].Value}
  $px=($s.pivot_px[0]/$s.width).ToString('0.######',[Globalization.CultureInfo]::InvariantCulture)
  $py=(1-$s.pivot_px[1]/$s.height).ToString('0.######',[Globalization.CultureInfo]::InvariantCulture)
  $meta=$template -replace '(?m)^guid: .*',("guid: "+$guid) -replace 'gusini_p1_idle_0_0',$s.id -replace 'spritePivot: \{x: [^}]+\}',"spritePivot: {x: $px, y: $py}" -replace 'textureCompression: 1','textureCompression: 0'
  [IO.File]::WriteAllText("$dest.meta",$meta)
}
$master.Dispose();foreach($im in $items.Values){$im.Dispose()}
# Six registered keyframes, native 256x256; source cells kept intact to avoid independently stretching figures.
$atlas=[Drawing.Bitmap]::new("$transform/atlas_fonte.png")
$timings=@(350,220,220,240,260,900);$frames=@();$strip=New-Canvas 1536 256;$sg=Get-Graphics $strip
for($i=0;$i -lt 6;$i++){
 $rect=[Drawing.Rectangle]::new(($i%3)*512,([int][Math]::Floor($i/3)*512),512,512)
 $crop=$atlas.Clone($rect,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $im=New-Canvas 256 256;$g=Get-Graphics $im;$g.DrawImage($crop,[Drawing.Rectangle]::new(0,0,256,256));$g.Dispose();$crop.Dispose();Hard-Alpha $im
 $name='transformacao_{0:D2}.png' -f $i;Save-Png $im "$transform/frames/$name"
 $sg.DrawImageUnscaled($im,($i*256),0)
 $frames+=@{index=$i;file="frames/$name";duration_ms=$timings[$i];canvas=@(256,256)};$im.Dispose()
}
$sg.Dispose();$atlas.Dispose();Save-Png $strip "$transform/spritesheet.png";$strip.Dispose()
@{type='design_keyframes';loop=$false;duration_ms=($timings | Measure-Object -Sum).Sum;frames=$frames;note='Six design keyframes; intermediate animation and gameplay not implemented.'} | ConvertTo-Json -Depth 6 | Set-Content "$transform/animation.json" -Encoding utf8
$checks=@();foreach($s in $specs){$im=[Drawing.Bitmap]::new("$root/parts/$($s.id).png");$checks+=@{id=$s.id;rgba=($im.PixelFormat -eq [Drawing.Imaging.PixelFormat]::Format32bppArgb);inside_canvas=($s.x -ge 0 -and $s.y -ge 0 -and $s.x+$im.Width -le 256 -and $s.y+$im.Height -le 272);pivot_inside=($s.pivot_px[0] -ge 0 -and $s.pivot_px[0] -lt $im.Width -and $s.pivot_px[1] -ge 0 -and $s.pivot_px[1] -lt $im.Height);sha256=(Get-FileHash "$root/parts/$($s.id).png").Hash};$im.Dispose()}
@{parts_count=$checks.Count;checks=$checks;native_canvas=@(256,272);unity_editor_validated=$false;gameplay_integrated=$false} | ConvertTo-Json -Depth 6 | Set-Content "$root/qa_report.json" -Encoding utf8
Write-Output ($layout | ConvertTo-Json -Depth 5)
