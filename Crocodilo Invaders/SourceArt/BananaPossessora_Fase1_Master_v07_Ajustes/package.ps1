param([string]$Source)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root=$PSScriptRoot
New-Item -ItemType Directory -Force "$root/parts","$root/ora/data" | Out-Null
Copy-Item -LiteralPath $Source -Destination "$root/corpo_fonte.png"
$bodySource=[Drawing.Bitmap]::new($Source)
$left=$bodySource.Width;$top=$bodySource.Height;$right=0;$bottom=0
for($y=0;$y -lt $bodySource.Height;$y++){for($x=0;$x -lt $bodySource.Width;$x++){
  if($bodySource.GetPixel($x,$y).A -ge 128){$left=[Math]::Min($left,$x);$right=[Math]::Max($right,$x);$top=[Math]::Min($top,$y);$bottom=[Math]::Max($bottom,$y)}
}}
$bodyRect=[Drawing.Rectangle]::new($left,$top,$right-$left+1,$bottom-$top+1)
$body=$bodySource.Clone($bodyRect,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
$bodySource.Dispose()
$names=@('corpo_banana','braco_distante','braco_proximo','perna_distante','perna_proxima','rabo')
$images=@($body)
for($i=1;$i -lt 6;$i++){$images+=[Drawing.Bitmap]::new("$root/parts_base/$($names[$i]).png")}
$bodyScale=594.0/$body.Height
$bodyX=[int][Math]::Round(349.5-($body.Width*$bodyScale/2))
$placements=@(@($bodyX,30,$bodyScale),@(223,328,0.5),@(423,332,0.55),@(165,487,0.5),@(334,502,0.5),@(416,386,0.53))
# Whole shell above roots: external peel lips retained; interior ring halves removed.
$order=@(5,3,1,4,2,0)
$canvas=[Drawing.Bitmap]::new(760,700,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g=[Drawing.Graphics]::FromImage($canvas)
$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
$g.Clear([Drawing.Color]::Transparent)
$svg=@('<svg xmlns="http://www.w3.org/2000/svg" width="760" height="700" viewBox="0 0 760 700" style="image-rendering:pixelated">')
$layers=@();$layout=@()
foreach($i in $order){
  $part=$images[$i];$name=$names[$i];$p=$placements[$i]
  $w=[int][Math]::Round($part.Width*$p[2]);$h=[int][Math]::Round($part.Height*$p[2]);$x=[int]$p[0];$y=[int]$p[1]
  if($i -eq 3){$w=132;$h=170};if($i -eq 4){$w=134;$h=176}
  if($i -eq 0){$part.Save("$root/parts/$name.png",[Drawing.Imaging.ImageFormat]::Png)}else{Copy-Item "$root/parts_base/$name.png" "$root/parts/$name.png" -Force}
  $g.DrawImage($part,[Drawing.Rectangle]::new($x,$y,$w,$h))
  $encoded=[Convert]::ToBase64String([IO.File]::ReadAllBytes("$root/parts/$name.png"))
  $svg+="<g id='$name'><image x='$x' y='$y' width='$w' height='$h' href='data:image/png;base64,$encoded'/></g>"
  $layer=[Drawing.Bitmap]::new($w,$h,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $lg=[Drawing.Graphics]::FromImage($layer);$lg.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor;$lg.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
  $lg.DrawImage($part,[Drawing.Rectangle]::new(0,0,$w,$h));$lg.Dispose()
  $layer.Save("$root/ora/data/$name.png",[Drawing.Imaging.ImageFormat]::Png);$layer.Dispose()
  $layers+="<layer name='$name' src='data/$name.png' x='$x' y='$y' opacity='1.0' visibility='visible' composite-op='svg:src-over'/>"
  $source=if($i -eq 0){'corpo_fonte.png'}else{"parts_base/$name.png"}
  $layout+=@{id=$name;file="parts/$name.png";x=$x;y=$y;width=$w;height=$h;scale=$p[2];source=$source}
}
$g.Dispose();$canvas.Save("$root/master.png",[Drawing.Imaging.ImageFormat]::Png);$canvas.Dispose()
$svg+='</svg>';[IO.File]::WriteAllText("$root/master.svg",($svg -join "`n"))
[array]::Reverse($layers)
[IO.File]::WriteAllText("$root/ora/mimetype",'image/openraster')
[IO.File]::WriteAllText("$root/ora/stack.xml",("<?xml version='1.0' encoding='UTF-8'?><image version='0.0.3' w='760' h='700' name='Banana fase 1 - encaixes posteriores e pernas longas'><stack>"+($layers -join '')+'</stack></image>'))
Copy-Item "$root/master.png" "$root/ora/mergedimage.png" -Force
if(Test-Path "$root/master.ora"){Remove-Item -LiteralPath "$root/master.ora"}
[IO.Compression.ZipFile]::CreateFromDirectory("$root/ora","$root/master.ora")
@{canvas=@(760,700);draw_order='back_to_front';body_source_rect=@($bodyRect.X,$bodyRect.Y,$bodyRect.Width,$bodyRect.Height);layers=$layout} | ConvertTo-Json -Depth 6 | Set-Content "$root/layout.json" -Encoding utf8
foreach($part in $images){$part.Dispose()}
$layout | ConvertTo-Json -Depth 4



