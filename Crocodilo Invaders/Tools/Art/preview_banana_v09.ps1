$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$root=Join-Path $project 'SourceArt/BananaPossessora_Fase1_Master_v09_Modular'
$transform=Join-Path $project 'SourceArt/BananaPossessora_Transformacao_v01'
$parts=@((Get-Content "$root/layout.json" -Raw | ConvertFrom-Json).layers)
foreach($p in $parts){$p | Add-Member -NotePropertyName src -NotePropertyValue ('data:image/png;base64,'+[Convert]::ToBase64String([IO.File]::ReadAllBytes((Join-Path $root $p.file))))}
$frames=@((Get-Content "$transform/animation.json" -Raw | ConvertFrom-Json).frames)
foreach($f in $frames){$f | Add-Member -NotePropertyName src -NotePropertyValue ('data:image/png;base64,'+[Convert]::ToBase64String([IO.File]::ReadAllBytes((Join-Path $transform $f.file))))}
$html=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'banana_preview_template.html')).Replace('__PARTS__',($parts | ConvertTo-Json -Depth 8 -Compress)).Replace('__FRAMES__',($frames | ConvertTo-Json -Depth 8 -Compress))
$preview=Join-Path $project 'Docs/ArtPreviews/BananaPossessora_v09/preview.html'
[IO.File]::WriteAllText($preview,$html,[Text.Encoding]::UTF8)
Write-Output $preview
