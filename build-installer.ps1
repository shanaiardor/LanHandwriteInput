$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $scriptRoot "LanHandwriteInput\LanHandwriteInput.csproj"
$distPath = Join-Path $scriptRoot "LanHandwriteInput\dist"
$publishPath = Join-Path $scriptRoot "artifacts\publish\win-x64"
$installerPath = Join-Path $scriptRoot "artifacts\installer"
$innoCompiler = Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe"
$installerScript = Join-Path $scriptRoot "installer.iss"

if (-not (Test-Path $distPath)) {
    throw "Missing frontend dist folder: $distPath"
}

if (-not (Test-Path $innoCompiler)) {
    throw "Inno Setup compiler not found: $innoCompiler"
}

New-Item -ItemType Directory -Force -Path $publishPath, $installerPath | Out-Null

dotnet publish $projectPath `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -o $publishPath

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Push-Location $scriptRoot
try {
    & $innoCompiler $installerScript
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup compiler failed with exit code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}

Write-Host "Installer created in: $installerPath"
