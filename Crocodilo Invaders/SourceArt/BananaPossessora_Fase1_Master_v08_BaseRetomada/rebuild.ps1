$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root=$PSScriptRoot
New-Item -ItemType Directory -Force "$root/parts","$root/ora/data" | Out-Null
$names=@('corpo_banana','braco_distante','braco_proximo','perna_distante','perna_proxima','rabo')
# Same source PNGs, widths, roots, banana and tail as the user-selected base.
$placements=@(@(200,30,299,594),@(145,328,139,163),@(377,332,137,202),@(155,487,132,170),@(334,502,134,176),@(416,386,241,148))
# Far leg lies behind banana, including its lower tip.
$order=@(5,3,0,4,1,2)
$canvas=[Drawing.Bitmap]::new(760,730,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g=[Drawing.Graphics]::FromImage($canvas)
$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
$g.Clear([Drawing.Color]::Transparent)
$svg=@('<svg xmlns="http://www.w3.org/2000/svg" width="760" height="730" viewBox="0 0 760 730" style="image-rendering:pixelated">')
$layers=@();$layout=@()
foreach($i in $order){
  $name=$names[$i];$p=$placements[$i];$x=$p[0];$y=$p[1];$w=$p[2];$h=$p[3]
  Copy-Item "$root/parts_base/$name.png" "$root/parts/$name.png" -Force
  $part=[Drawing.Bitmap]::new("$root/parts/$name.png")
  $g.DrawImage($part,[Drawing.Rectangle]::new($x,$y,$w,$h))
  $encoded=[Convert]::ToBase64String([IO.File]::ReadAllBytes("$root/parts/$name.png"))
  $svg+="<g id='$name'><image x='$x' y='$y' width='$w' height='$h' href='data:image/png;base64,$encoded'/></g>"
  $layer=[Drawing.Bitmap]::new($w,$h,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $lg=[Drawing.Graphics]::FromImage($layer)
  $lg.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
  $lg.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
  $lg.DrawImage($part,[Drawing.Rectangle]::new(0,0,$w,$h));$lg.Dispose();$part.Dispose()
  $layer.Save("$root/ora/data/$name.png",[Drawing.Imaging.ImageFormat]::Png);$layer.Save("$root/parts/$name.png",[Drawing.Imaging.ImageFormat]::Png);$layer.Dispose()
  $layers+="<layer name='$name' src='data/$name.png' x='$x' y='$y' opacity='1.0' visibility='visible' composite-op='svg:src-over'/>"
  $layout+=@{id=$name;file="parts/$name.png";source="parts_base/$name.png";x=$x;y=$y;width=$w;height=$h}
}
$g.Dispose();$canvas.Save("$root/master.png",[Drawing.Imaging.ImageFormat]::Png);$canvas.Dispose()
$svg+='</svg>';[IO.File]::WriteAllText("$root/master.svg",($svg -join "`n"))
[array]::Reverse($layers)
[IO.File]::WriteAllText("$root/ora/mimetype",'image/openraster')
[IO.File]::WriteAllText("$root/ora/stack.xml",("<?xml version='1.0' encoding='UTF-8'?><image version='0.0.3' w='760' h='730' name='Banana fase 1 - base retomada'><stack>"+($layers -join '')+'</stack></image>'))
Copy-Item "$root/master.png" "$root/ora/mergedimage.png" -Force
if(Test-Path "$root/master.ora"){Remove-Item -LiteralPath "$root/master.ora"}
[IO.Compression.ZipFile]::CreateFromDirectory("$root/ora","$root/master.ora")
@{canvas=@(760,730);draw_order='back_to_front';base='user-selected v03, not later redesigns';layers=$layout} | ConvertTo-Json -Depth 6 | Set-Content "$root/layout.json" -Encoding utf8

