param([Parameter(Mandatory=$true)][string]$Source,[Parameter(Mandatory=$true)][ValidateRange(1,4)][int]$Segment)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$root=Join-Path $project 'SourceArt/BananaPossessora_Transformacao_v02_24Frames'
New-Item -ItemType Directory -Force "$root/sources","$root/frames_source" | Out-Null
Copy-Item -LiteralPath $Source -Destination ("$root/sources/segmento_{0:D2}.png" -f $Segment)
$im=[Drawing.Bitmap]::new((Resolve-Path -LiteralPath $Source).Path)
if($im.Width -ne 1536 -or $im.Height -ne 1024){throw "Unexpected sheet size: $($im.Width)x$($im.Height)"}
for($i=0;$i -lt 6;$i++){
 # Preserve slight overshoot into the empty inter-row gutter (notably the lifted cap).
 $top=[int][Math]::Floor($i/3)*512
 $overshoot=0;if($i -ge 3){$overshoot=12}
 $left=($i%3)*512
 $width=[Math]::Min(532,$im.Width-$left)
 $rect=[Drawing.Rectangle]::new($left,($top-$overshoot),$width,(512+$overshoot))
 $frame=$im.Clone($rect,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $index=($Segment-1)*6+$i
 $frame.Save(("$root/frames_source/transformacao_{0:D2}.png" -f $index),[Drawing.Imaging.ImageFormat]::Png)
 $frame.Dispose()
}
$im.Dispose()
Write-Output ("$root/frames_source/transformacao_{0:D2}.png" -f ($Segment*6-1))
