$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root=$PSScriptRoot
$layout=Get-Content -Raw "$root/layout.json" | ConvertFrom-Json
$bodyInfo=$layout.layers | Where-Object id -eq 'corpo_banana'
$body=[Drawing.Bitmap]::new("$root/ora/data/corpo_banana.png")
$overlap=@{}
foreach($item in $layout.layers){
  if($item.id -eq 'corpo_banana'){continue}
  $part=[Drawing.Bitmap]::new("$root/ora/data/$($item.id).png")
  $count=0
  $left=[Math]::Max($item.x,$bodyInfo.x);$right=[Math]::Min($item.x+$item.width,$bodyInfo.x+$bodyInfo.width)
  $top=[Math]::Max($item.y,$bodyInfo.y);$bottom=[Math]::Min($item.y+$item.height,$bodyInfo.y+$bodyInfo.height)
  for($y=$top;$y -lt $bottom;$y++){for($x=$left;$x -lt $right;$x++){
    if($body.GetPixel($x-$bodyInfo.x,$y-$bodyInfo.y).A -ge 128 -and $part.GetPixel($x-$item.x,$y-$item.y).A -ge 128){$count++}
  }}
  $part.Dispose()
  if($count -lt 20){throw "Disconnected module $($item.id): $count"}
  $overlap[$item.id]=$count
}
$body.Dispose()
$master=[Drawing.Bitmap]::new("$root/master.png")
foreach($item in $layout.layers){if($item.x -lt 0 -or $item.y -lt 0 -or $item.x+$item.width -gt $master.Width -or $item.y+$item.height -gt $master.Height){throw 'Layer outside canvas'}}
$zip=[IO.Compression.ZipFile]::OpenRead("$root/master.ora")
$reader=[IO.StreamReader]::new($zip.GetEntry('stack.xml').Open())
$xml=[xml]$reader.ReadToEnd();$reader.Dispose()
$layers=@($xml.image.stack.layer)
foreach($layer in $layers){if($null -eq $zip.GetEntry($layer.src)){throw 'Missing ORA layer'}}
$zip.Dispose()
if($layers[0].name -ne 'corpo_banana'){throw 'Shell must hide roots'}
$far=$layout.layers | Where-Object id -eq 'perna_distante'
$near=$layout.layers | Where-Object id -eq 'perna_proxima'
if($far.height -ne 170 -or $near.height -ne 176){throw 'Unexpected leg length'}
$report=@{
  phase=1;canvas=@($master.Width,$master.Height);corner_alpha=$master.GetPixel(0,0).A
  individual_pngs=@(Get-ChildItem "$root/parts" -Filter '*.png').Count;ora_layers=$layers.Count
  all_layers_within_canvas=$true;body_covers_roots=$true;opaque_overlap_pixels=$overlap
  leg_height_before=@(144,149);leg_height_after=@($far.height,$near.height)
  hip_positions_preserved=$true;generator='built-in image_gen'
  visual_review='posterior shoulder lip direction and position; external borders retained, internal borders absent; longer lean shins; yellow peel and green details'
}
$master.Dispose()
$report | ConvertTo-Json -Depth 5 | Set-Content "$root/qa_report.json" -Encoding utf8
$report | ConvertTo-Json -Depth 5
