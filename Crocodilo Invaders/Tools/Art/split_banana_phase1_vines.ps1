param([Parameter(Mandatory=$true)][string]$Source,[Parameter(Mandatory=$true)][ValidateRange(1,4)][int]$Segment)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$root=Join-Path $project 'SourceArt/BananaPhase1_VinesShoot_v01'
New-Item -ItemType Directory -Force "$root/sources","$root/frames_source" | Out-Null
Copy-Item -LiteralPath $Source -Destination ("$root/sources/segmento_{0:D2}.png" -f $Segment)
$im=[Drawing.Bitmap]::new((Resolve-Path -LiteralPath $Source).Path)
if($im.Width -ne 1536 -or $im.Height -ne 1024){throw 'Unexpected sheet size'}
for($i=0;$i -lt 6;$i++){
 $rect=[Drawing.Rectangle]::new(($i%3)*512,([int][Math]::Floor($i/3)*512),512,512)
 $frame=$im.Clone($rect,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $index=($Segment-1)*6+$i
 $frame.Save(("$root/frames_source/phase1_{0:D2}.png" -f $index),[Drawing.Imaging.ImageFormat]::Png);$frame.Dispose()
}
$im.Dispose()
Write-Output ("$root/frames_source/phase1_{0:D2}.png" -f ($Segment*6-1))
