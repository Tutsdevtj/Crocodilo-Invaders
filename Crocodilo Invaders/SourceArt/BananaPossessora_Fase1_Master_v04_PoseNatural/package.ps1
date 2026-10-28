param([string]$Source)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root=$PSScriptRoot
New-Item -ItemType Directory -Force "$root/parts","$root/ora/data" | Out-Null
Copy-Item -LiteralPath $Source -Destination "$root/atlas.png"
$atlas=[Drawing.Bitmap]::new($Source)
$originalAtlas=[Drawing.Bitmap]::new("$root/atlas_preservado.png")
$names=@('corpo_banana','braco_distante','braco_proximo','perna_distante','perna_proxima','rabo')
$items=@()
for($i=0;$i -lt 6;$i++){
  $partAtlas=if($i -eq 1 -or $i -eq 2){$atlas}else{$originalAtlas}
  $ox=($i%3)*512; $oy=[Math]::Floor($i/3)*512; $cellHeight=512; if($i -eq 0){$cellHeight=675}; if($i -ge 3){$oy=675;$cellHeight=349}
  if($i -eq 1 -or $i -eq 2){$cellHeight=630}
  $cellWidth=512; if($i -eq 4){$cellWidth=450}; if($i -eq 5){$ox=1000;$cellWidth=536}
  $left=$cellWidth;$top=$cellHeight;$right=0;$bottom=0
  for($y=0;$y -lt $cellHeight;$y++){for($x=0;$x -lt $cellWidth;$x++){
    if($partAtlas.GetPixel($ox+$x,$oy+$y).A -ge 128){$left=[Math]::Min($left,$x);$right=[Math]::Max($right,$x);$top=[Math]::Min($top,$y);$bottom=[Math]::Max($bottom,$y)}
  }}
  $rect=[Drawing.Rectangle]::new($ox+$left,$oy+$top,$right-$left+1,$bottom-$top+1)
  $part=$partAtlas.Clone($rect,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
  
  $path="$root/parts/$($names[$i]).png"
  $part.Save($path,[Drawing.Imaging.ImageFormat]::Png)
  $items+=@{id=$names[$i];index=$i;source_rect=@($rect.X,$rect.Y,$rect.Width,$rect.Height);file="parts/$($names[$i]).png";image=$part}
}
# All placements are editable in the SVG and layout file. Rightmost limb is near the viewer.
$placements=@(@(200,30,0.9),@(173,328,0.5),@(423,332,0.55),@(165,487,0.5),@(334,502,0.5),@(416,386,0.53))
$order=@(5,3,0,4,1,2)
$canvas=[Drawing.Bitmap]::new(760,700,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g=[Drawing.Graphics]::FromImage($canvas)
$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
$g.Clear([Drawing.Color]::Transparent)
$svg=@('<svg xmlns="http://www.w3.org/2000/svg" xmlns:inkscape="http://www.inkscape.org/namespaces/inkscape" width="760" height="700" viewBox="0 0 760 700" style="image-rendering:pixelated">')
$layers=@();$layout=@()
foreach($i in $order){
  $item=$items[$i];$p=$placements[$i];$w=[int][Math]::Round($item.image.Width*$p[2]);$h=[int][Math]::Round($item.image.Height*$p[2]);$x=[int]$p[0];$y=[int]$p[1]
  $g.DrawImage($item.image,[Drawing.Rectangle]::new($x,$y,$w,$h))
  $encoded=[Convert]::ToBase64String([IO.File]::ReadAllBytes("$root/$($item.file)"))
  $svg+="<g id='$($item.id)'><image x='$x' y='$y' width='$w' height='$h' href='data:image/png;base64,$encoded'/></g>"
  $layer=[Drawing.Bitmap]::new($w,$h,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $lg=[Drawing.Graphics]::FromImage($layer);$lg.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor;$lg.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
  $lg.DrawImage($item.image,[Drawing.Rectangle]::new(0,0,$w,$h));$lg.Dispose()
  $layer.Save("$root/ora/data/$($item.id).png",[Drawing.Imaging.ImageFormat]::Png);$layer.Dispose()
  $layers+="<layer name='$($item.id)' src='data/$($item.id).png' x='$x' y='$y' opacity='1.0' visibility='visible' composite-op='svg:src-over'/>"
  $layout+=@{id=$item.id;file=$item.file;x=$x;y=$y;width=$w;height=$h;scale=$p[2];source_rect=$item.source_rect}
}
$g.Dispose();$canvas.Save("$root/master.png",[Drawing.Imaging.ImageFormat]::Png);$canvas.Dispose()
$svg+='</svg>';[IO.File]::WriteAllText("$root/master.svg",($svg -join "`n"))
[array]::Reverse($layers)
[IO.File]::WriteAllText("$root/ora/mimetype",'image/openraster')
[IO.File]::WriteAllText("$root/ora/stack.xml",("<?xml version='1.0' encoding='UTF-8'?><image version='0.0.3' w='760' h='700' name='Banana amarela - fase 1 - pose natural'><stack>"+($layers -join '')+'</stack></image>'))
Copy-Item "$root/master.png" "$root/ora/mergedimage.png"
if(Test-Path "$root/master.ora"){Remove-Item -LiteralPath "$root/master.ora"}; [IO.Compression.ZipFile]::CreateFromDirectory("$root/ora","$root/master.ora")
@{canvas=@(760,700);draw_order='back_to_front';layers=$layout} | ConvertTo-Json -Depth 6 | Set-Content "$root/layout.json" -Encoding utf8
$atlas.Dispose();$originalAtlas.Dispose();foreach($item in $items){$item.image.Dispose()}
$layout | ConvertTo-Json -Depth 4











