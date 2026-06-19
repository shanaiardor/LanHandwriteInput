$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

# Build frontend first
$buildWebScript = Join-Path $scriptRoot "build-web.ps1"
if (Test-Path $buildWebScript) {
    Write-Host "Building frontend..."
    & $buildWebScript
    if ($LASTEXITCODE -ne 0) {
        throw "build-web.ps1 failed with exit code $LASTEXITCODE"
    }
}

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

$scriptRootFullPath = [System.IO.Path]::GetFullPath($scriptRoot)
$publishFullPath = [System.IO.Path]::GetFullPath($publishPath)
if (-not $publishFullPath.StartsWith($scriptRootFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to clean publish path outside project folder: $publishFullPath"
}

if (Test-Path $publishFullPath) {
    Remove-Item -LiteralPath $publishFullPath -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $publishPath, $installerPath | Out-Null

dotnet publish $projectPath `
    -c Release `
    -r win-x64 `
    --self-contained false `
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

$installerFile = Get-ChildItem -Path $installerPath -Filter "*.exe" | Select-Object -First 1
if ($installerFile) {
    Copy-Item -Path $installerFile.FullName -Destination $scriptRoot -Force
    Write-Host "Installer created in: $installerPath"
    Write-Host "Copied to: $(Join-Path $scriptRoot $installerFile.Name)"
}
else {
    throw "No installer .exe found in $installerPath"
}
