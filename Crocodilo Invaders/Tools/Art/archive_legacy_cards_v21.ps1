param([switch]$Apply)
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$projectRoot = Join-Path $repoRoot 'Crocodilo Invaders'
$backupRoot = Join-Path $repoRoot 'Backup/Cartas/2026-10-22_Anteriores_v21'
$plan = [Collections.Generic.List[object]]::new()

function Add-Entry([string]$relative, [string]$group, [bool]$move) {
    $source = [IO.Path]::GetFullPath((Join-Path $projectRoot $relative))
    if (-not $source.StartsWith($projectRoot + [IO.Path]::DirectorySeparatorChar)) { throw "Origem fora do projeto: $source" }
    if (Test-Path -LiteralPath $source) {
        $destination = [IO.Path]::GetFullPath((Join-Path (Join-Path $backupRoot $group) $relative))
        if (-not $destination.StartsWith($backupRoot + [IO.Path]::DirectorySeparatorChar)) { throw "Destino fora do backup: $destination" }
        $plan.Add([pscustomobject]@{ Relative=$relative; Source=$source; Destination=$destination; Move=$move })
    }
}

Add-Entry 'Assets/Resources/Cards' '01_EmUsoNoJogo' $false
Add-Entry 'Assets/Resources/Cards.meta' '01_EmUsoNoJogo' $false
Add-Entry 'Assets/Art/Cards' '01_EmUsoNoJogo' $false
Add-Entry 'Assets/Art/Cards.meta' '01_EmUsoNoJogo' $false
Add-Entry 'Docs/Cartas.md' '01_EmUsoNoJogo' $false
foreach ($folder in Get-ChildItem -LiteralPath (Join-Path $projectRoot 'SourceArt') -Directory) {
    if ($folder.Name -match '^(Cards|CardCollection_)' -and $folder.Name -ne 'CardCollection_v21_Integration') {
        Add-Entry ('SourceArt/' + $folder.Name) '02_ModelosEHistorico' $true
    }
}
foreach ($folder in Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Docs/ArtPreviews') -Directory) {
    if ($folder.Name -match '^(Cards|CardCollection_|CardReview)' -and $folder.Name -ne 'CardCollection_v21') {
        Add-Entry ('Docs/ArtPreviews/' + $folder.Name) '02_ModelosEHistorico' $true
    }
}
$legacyToolPattern = '^(apply_card_|build_card_|build_croc_cards_|build_foil_|build_military_cards|card_collection_|card_typography_|foil_preview_|inspect_card_|prepare_card_|prepare_mouth_|prepare_mythic_|prepare_verify_card_|verify_card_|verify_foil_)'
foreach ($file in Get-ChildItem -LiteralPath $PSScriptRoot -File) {
    if ($file.Name -match $legacyToolPattern) { Add-Entry ('Tools/Art/' + $file.Name) '02_ModelosEHistorico' $true }
}
foreach ($name in @('AjustesCartas_v13.md','RevisaoMolduras_v14.md','TransparenciaCartas_v15.md','CartasSistemasNave_v17.md','CartasSistemasNave_v18.md','CartasMoldurasFullArt_v19.md','CartasFullArtRelevo_v20.md')) {
    Add-Entry ('Docs/' + $name) '02_ModelosEHistorico' $true
}
if (-not $Apply) {
    [pscustomobject]@{ Backup=$backupRoot; Entries=$plan.Count; Moves=@($plan | Where-Object Move).Count; Mode='PREVIEW' } | ConvertTo-Json
    exit 0
}
if (Test-Path -LiteralPath $backupRoot) { throw "Backup já existe; não sobrescrever: $backupRoot" }
New-Item -ItemType Directory -Path $backupRoot -Force | Out-Null
$records = [Collections.Generic.List[object]]::new()
foreach ($entry in $plan) {
    $item = Get-Item -LiteralPath $entry.Source
    $files = if ($item.PSIsContainer) { @(Get-ChildItem -LiteralPath $entry.Source -Recurse -File) } else { @($item) }
    $expected = foreach ($file in $files) {
        $suffix = if ($item.PSIsContainer) { $file.FullName.Substring($entry.Source.Length + 1) } else { '' }
        [pscustomobject]@{ Suffix=$suffix; SHA256=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash; Bytes=$file.Length }
    }
    New-Item -ItemType Directory -Path (Split-Path $entry.Destination -Parent) -Force | Out-Null
    # Exact, checked workspace paths, one PowerShell operation end-to-end.
    if ($entry.Move) { Move-Item -LiteralPath $entry.Source -Destination $entry.Destination }
    else { Copy-Item -LiteralPath $entry.Source -Destination $entry.Destination -Recurse }
    foreach ($file in $expected) {
        $destination = if ($file.Suffix) { Join-Path $entry.Destination $file.Suffix } else { $entry.Destination }
        if ((Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash -ne $file.SHA256) { throw "Falha de integridade: $destination" }
        $records.Add([pscustomobject]@{ Original=($entry.Relative + $(if ($file.Suffix) { '/' + $file.Suffix.Replace('\','/') } else { '' })); Backup=$destination.Substring($backupRoot.Length+1).Replace('\','/'); SHA256=$file.SHA256; Bytes=$file.Bytes; Moved=$entry.Move })
    }
}
$manifest = [pscustomobject]@{ Date='2026-10-22'; Purpose='Preservar as cartas antigas e retirar seus históricos do projeto ativo antes da integração v21'; Files=$records }
[IO.File]::WriteAllText((Join-Path $backupRoot 'manifest.json'), ($manifest | ConvertTo-Json -Depth 8), [Text.UTF8Encoding]::new($false))
$readme = @'
BACKUP DAS CARTAS — 22/10/2026

01_EmUsoNoJogo: cópia exata das 25 frentes e metadados que estavam em uso,
verso e documentação anterior. A aplicação da coleção nova preserva os GUIDs.
02_ModelosEHistorico: modelos, fontes, prévias, prompts e geradores antigos
movidos para fora da pasta Unity, incluindo as revisões de design mais recentes.

manifest.json registra caminho original, localização, tamanho e SHA256 de cada
arquivo. Todos os arquivos foram conferidos após cópia/movimentação.
Para restaurar uma coleção, copie somente suas imagens escolhidas para os
caminhos correspondentes do projeto, mantendo os metadados corretos.
Não execute geradores históricos no projeto atual: eles restauram modelos antigos.

Ativos atuais: Crocodilo Invaders/Assets/Resources/Cards.
Fonte nova: Crocodilo Invaders/SourceArt/CardCollection_v21_Integration.
O Backup fica ao lado de Crocodilo Invaders e Gaming portugol, fora da Unity.
'@
[IO.File]::WriteAllText((Join-Path $backupRoot 'LEIA-ME.txt'), $readme, [Text.UTF8Encoding]::new($false))
[pscustomobject]@{ Backup=$backupRoot; Files=$records.Count; Bytes=($records | Measure-Object Bytes -Sum).Sum; Entries=$plan.Count; Mode='ARCHIVED_AND_VERIFIED' } | ConvertTo-Json
