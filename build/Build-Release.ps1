param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^0\.0\.\d+$')]
    [string]$Version,
    [switch]$SkipInstaller
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$artifactsPath = Join-Path $repositoryRoot 'artifacts'
$publishPath = Join-Path $artifactsPath 'publish'
$portableStage = Join-Path $artifactsPath 'portable'

if (Test-Path -LiteralPath $artifactsPath) {
    $resolvedArtifacts = (Resolve-Path -LiteralPath $artifactsPath).Path
    if (-not $resolvedArtifacts.StartsWith($repositoryRoot, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to clean a path outside the repository: $resolvedArtifacts"
    }
    Remove-Item -LiteralPath $resolvedArtifacts -Recurse -Force
}
New-Item -ItemType Directory -Path $publishPath | Out-Null

dotnet restore (Join-Path $repositoryRoot 'Api_Loggin.csproj')
if ($LASTEXITCODE -ne 0) { throw 'Falha ao restaurar a API.' }
dotnet restore (Join-Path $repositoryRoot 'Loggin.Desktop.Tests\Loggin.Desktop.Tests.csproj')
if ($LASTEXITCODE -ne 0) { throw 'Falha ao restaurar os testes.' }
dotnet build (Join-Path $repositoryRoot 'Api_Loggin.csproj') --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar a API.' }
dotnet test (Join-Path $repositoryRoot 'Loggin.Desktop.Tests\Loggin.Desktop.Tests.csproj') --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Falha nos testes.' }
dotnet publish (Join-Path $repositoryRoot 'Loggin.Desktop\Loggin.Desktop.csproj') `
    --configuration Release --runtime win-x64 --self-contained true `
    -p:Version=$Version -p:AssemblyVersion="$Version.0" -p:FileVersion="$Version.0" `
    --output $publishPath
if ($LASTEXITCODE -ne 0) { throw 'Falha ao publicar o cliente desktop.' }

Copy-Item -LiteralPath $publishPath -Destination $portableStage -Recurse
Copy-Item -LiteralPath (Join-Path $portableStage 'Loggin.Desktop.exe') `
    -Destination (Join-Path $portableStage 'Loggin.Desktop-portable.exe')
$portableZip = Join-Path $artifactsPath "Loggin-v$Version-win-x64-portable.zip"
Compress-Archive -Path (Join-Path $portableStage '*') -DestinationPath $portableZip -CompressionLevel Optimal

if (-not $SkipInstaller) {
    $isccCandidates = @(
        (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe')
    )
    $iscc = $isccCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
    if (-not $iscc) {
        throw 'Inno Setup 6 não foi encontrado. Instale-o ou use -SkipInstaller.'
    }
    & $iscc "/DMyAppVersion=$Version" "/DPublishDir=$publishPath" "/DOutputDir=$artifactsPath" `
        (Join-Path $repositoryRoot 'installer\Loggin.iss')
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao gerar o instalador.' }
}

Write-Output "Release artifacts created in $artifactsPath"
