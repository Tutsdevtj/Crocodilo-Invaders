param([string]$PackageName = 'CrocodiloInvaders-DEV-Windows.zip')
$ErrorActionPreference = 'Stop'
$buildsRoot = Join-Path (Split-Path $PSScriptRoot -Parent) 'Builds'
$buildRoot = Join-Path $buildsRoot 'Windows-DEV'
$packageRoot = Join-Path $buildsRoot 'Pacotes'
$executable = Join-Path $buildRoot 'CrocodiloInvaders.exe'
$assembly = Join-Path $buildRoot 'CrocodiloInvaders_Data/Managed/Assembly-CSharp.dll'
foreach ($required in @($executable, (Join-Path $buildRoot 'UnityPlayer.dll'), $assembly)) {
    if (-not (Test-Path -LiteralPath $required)) { throw "Arquivo obrigatório ausente: $required" }
}
$assemblyText = [System.Text.Encoding]::UTF8.GetString([System.IO.File]::ReadAllBytes($assembly))
foreach ($type in @('DevelopmentCardSession','DevelopmentCardTest','DeveloperMenuView','CardRarityFeedback','ApplyCardSpacing','CelebrateChoice','UpdateTreasure','ProtectedColor')) {
    if (-not $assemblyText.Contains($type)) { throw "Build sem o modo DEV atual: $type" }
}
$readme = @'
CROCODILO INVADERS - TESTE DAS CARTAS

1. Extraia TODOS os arquivos deste ZIP para uma pasta.
2. Abra CrocodiloInvaders.exe. Nao precisa instalar nem abrir a Unity.
3. No menu inicial, clique no pequeno DEV no canto superior direito ou pressione F8.
4. Escolha sorteio normal, raridade fixa ou ciclo das 25 cartas.
5. Clique INICIAR TESTE DE CARTAS e pressione ENTER quando o loading estiver pronto.
6. Escolha com mouse ou 1/2/3. R faz rerolls infinitos no modo DEV.
7. Depois de escolher, F8 abre o painel para gerar outra oferta, zerar bonus ou voltar ao menu.

V21: 25 cartas de sistemas instalados na nave, sem personagem na frente.
Comum simples, rara azul, epica roxa, lendaria dourada full art e mitica
vermelha full art com relevo ilustrado e partes da arte sobrepostas na borda.
No Ataque mitico, so a bala atravessa a borda direita; a esquerda fica inteira.
Fundo externo transparente, verso e quatro sons de revelacao preservados.
V25: sons originais criados por sintese para Crocodilo Invaders.
Sem samples, loops, vinhetas ou pacotes de audio de terceiros nas cartas.
Power-up arcade, frases ascendentes e cristais sonoros por raridade.
Lendarias e miticas tem so cintilacao discreta, sem musica; um brilho por oferta.
Duas/tres cartas altas dividem o volume das revelacoes, evitando acumulo.
Pausa, reroll, escolha e menu interrompem o audio corretamente.
Efeitos mais graduais: comum discreta, rara sutil, epica com energia,
lendaria com estrelas douradas e mitica com arcos curtos junto da moldura.
Procedencia dos sons proprios em Creditos/Audio.
Efeitos externos progressivos por raridade incluidos: faiscas azuis, energia roxa,
raios dourados e explosao carmesim com relampagos na mitica. Mais espaco entre cartas.
A carta mais rara revela por ultimo. Escolher uma carta dispara outro impacto.
Foils/holograficos dinamicos dentro da arte continuam para uma etapa posterior.
O teste usa a cena e os controladores reais e nao registra resultados no ranking.

Mantenha o EXE, UnityPlayer.dll, a pasta CrocodiloInvaders_Data e MonoBleedingEdge juntos.
'@
[System.IO.File]::WriteAllText((Join-Path $buildRoot 'LEIA-ME.txt'), $readme, [System.Text.UTF8Encoding]::new($false))
New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null
$zipPath = Join-Path $packageRoot $PackageName
if (Test-Path -LiteralPath $zipPath) { throw "Pacote ja existe: $zipPath" }
if ([IO.Path]::GetExtension($PackageName).ToLowerInvariant() -eq '.rar') {
    $rar = Join-Path $buildsRoot 'PackagingTools/Rar.exe'
    if (-not (Test-Path -LiteralPath $rar)) { throw "Compactador RAR oficial nao encontrado: $rar" }
    $listPath = Join-Path $buildsRoot 'CardAudio-v25-package-files.txt'
    $files = @(Get-ChildItem -LiteralPath $buildRoot -File -Recurse | ForEach-Object {
        $relative = $_.FullName.Substring($buildRoot.Length + 1)
        if ($relative -notmatch '(?i)(DoNotShip|BackUpThisFolder|\.pdb$)') { $relative }
    })
    [IO.File]::WriteAllLines($listPath, $files, [Text.UTF8Encoding]::new($false))
    Push-Location -LiteralPath $buildRoot
    try {
        & $rar a -ma5 -m3 -idq -y -scfl -apCrocodiloInvaders-DEV $zipPath ('@' + $listPath)
        if ($LASTEXITCODE -ne 0) { throw "Falha ao criar RAR: $LASTEXITCODE" }
    } finally { Pop-Location }
    & $rar t -idq $zipPath
    if ($LASTEXITCODE -ne 0) { throw "Teste de integridade do RAR falhou: $LASTEXITCODE" }
    $entries = @(& $rar lb $zipPath | ForEach-Object { $_.Replace('\','/') })
    foreach ($required in @('CrocodiloInvaders.exe','UnityPlayer.dll','CrocodiloInvaders_Data/Managed/Assembly-CSharp.dll','LEIA-ME.txt','Creditos/Audio/ORIGENS.txt')) {
        if ($entries -notcontains ('CrocodiloInvaders-DEV/' + $required)) { throw "RAR incompleto: $required" }
    }
    $hash = Get-FileHash -LiteralPath $zipPath -Algorithm SHA256
    [IO.File]::WriteAllText((Join-Path $packageRoot 'SHA256SUMS-v25.txt'), $hash.Hash.ToLowerInvariant() + '  ' + $PackageName)
    Get-Item -LiteralPath $zipPath | Select-Object FullName, Length
    exit 0
}
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::Open($zipPath, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($file in Get-ChildItem -LiteralPath $buildRoot -File -Recurse) {
        $relative = $file.FullName.Substring($buildRoot.Length + 1)
        if ($relative -match '(?i)(DoNotShip|BackUpThisFolder|\.pdb$)') { continue }
        $entry = 'CrocodiloInvaders-DEV/' + $relative.Replace('\','/')
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $file.FullName, $entry, [System.IO.Compression.CompressionLevel]::Fastest) | Out-Null
    }
} finally { $archive.Dispose() }
$check = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    foreach ($entry in @('CrocodiloInvaders.exe','UnityPlayer.dll','CrocodiloInvaders_Data/Managed/Assembly-CSharp.dll','LEIA-ME.txt')) {
        if ($null -eq $check.GetEntry('CrocodiloInvaders-DEV/' + $entry)) { throw "ZIP incompleto: $entry" }
    }
} finally { $check.Dispose() }
$hash = Get-FileHash -LiteralPath $zipPath -Algorithm SHA256
[System.IO.File]::WriteAllText((Join-Path $packageRoot 'SHA256SUMS.txt'), $hash.Hash.ToLowerInvariant() + '  ' + [System.IO.Path]::GetFileName($zipPath))
Get-Item -LiteralPath $zipPath | Select-Object FullName, Length
