$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$webRoot = Join-Path $scriptRoot "LanHandwriteInput\web"

if (-not (Test-Path $webRoot)) {
    throw "Web project folder not found: $webRoot"
}

Push-Location $webRoot
try {
    if (-not (Test-Path "node_modules")) {
        npm install
    }

    npm run build
}
finally {
    Pop-Location
}
