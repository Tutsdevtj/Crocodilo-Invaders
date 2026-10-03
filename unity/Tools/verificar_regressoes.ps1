param(
    [string]$EditorPath = 'C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe'
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $EditorPath)) { throw "Unity não encontrada em $EditorPath" }
$taskProjectRoot = Split-Path -Parent $PSScriptRoot
$taskValidationRoot = Join-Path $env:TEMP ('crocodilo-regression-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $taskValidationRoot | Out-Null
foreach ($taskFolder in @('Assets', 'Packages', 'ProjectSettings')) {
    Copy-Item -LiteralPath (Join-Path $taskProjectRoot $taskFolder) -Destination $taskValidationRoot -Recurse
}
Copy-Item -LiteralPath (Join-Path $taskProjectRoot 'Tests/Editor/CrocodiloRegressionChecks.cs') -Destination (Join-Path $taskValidationRoot 'Assets/Editor/CrocodiloRegressionChecks.cs')
$taskLog = Join-Path $taskValidationRoot 'regression.log'
$taskProcess = Start-Process -FilePath $EditorPath -ArgumentList @(
    '-batchmode', '-nographics', '-projectPath', ('"' + $taskValidationRoot + '"'),
    '-executeMethod', 'CrocodiloRegressionChecks.Run', '-logFile', ('"' + $taskLog + '"')
) -WindowStyle Hidden -PassThru
$taskProcess.WaitForExit()
Select-String -LiteralPath $taskLog -Pattern 'REGRESSION_' | ForEach-Object { $_.Line }
if ($taskProcess.ExitCode -ne 0) {
    Get-Content -LiteralPath $taskLog -Tail 60
    throw "Verificação falhou. Log: $taskLog"
}
Write-Output "Log completo: $taskLog"
