param([string]$Source)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$root=$PSScriptRoot
Copy-Item -LiteralPath $Source -Destination "$root/pernas_fonte.png"
$sheet=[Drawing.Bitmap]::new($Source)
$half=[int]($sheet.Width/2)
$records=@()
for($i=0;$i -lt 2;$i++){
  $left=$half;$top=$sheet.Height;$right=0;$bottom=0
  for($y=0;$y -lt $sheet.Height;$y++){for($x=0;$x -lt $half;$x++){
    if($sheet.GetPixel($i*$half+$x,$y).A -ge 128){$left=[Math]::Min($left,$x);$right=[Math]::Max($right,$x);$top=[Math]::Min($top,$y);$bottom=[Math]::Max($bottom,$y)}
  }}
  $rect=[Drawing.Rectangle]::new($i*$half+$left,$top,$right-$left+1,$bottom-$top+1)
  $part=$sheet.Clone($rect,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $name=if($i -eq 0){'perna_distante'}else{'perna_proxima'}
  $part.Save("$root/parts_base/$name.png",[Drawing.Imaging.ImageFormat]::Png)
  $part.Dispose()
  $records+=@{id=$name;source='pernas_fonte.png';source_rect=@($rect.X,$rect.Y,$rect.Width,$rect.Height)}
}
$sheet.Dispose()
$records | ConvertTo-Json -Depth 4 | Set-Content "$root/pernas_recortes.json" -Encoding utf8
$records | ConvertTo-Json -Depth 4
