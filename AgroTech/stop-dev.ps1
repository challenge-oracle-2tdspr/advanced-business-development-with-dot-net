$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Join-Path $ScriptDir "AgroTech"
$ComposeFile = Join-Path $ProjectDir "compose.yaml"

Write-Host "Parando processos .NET antigos (retrocompatibilidade)..."

$PidFiles = @(
    (Join-Path $ScriptDir ".run/agrotech-api.pid"),
    (Join-Path $ScriptDir ".run/agrotech-worker-alerts.pid"),
    (Join-Path $ScriptDir ".run/agrotech-worker-recommendations.pid"),
    (Join-Path $ScriptDir ".run/agrotech-worker-readings.pid"),
    (Join-Path $ScriptDir ".run/agrotech-mongodb.pid")
)

foreach ($file in $PidFiles) {
    if (Test-Path $file) {
        $pidValue = Get-Content $file -ErrorAction SilentlyContinue
        if ($pidValue) {
            try {
                $proc = Get-Process -Id $pidValue -ErrorAction Stop
                Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
            }
            catch {
            }
        }
        Remove-Item $file -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "Parando containers..."
docker compose -f $ComposeFile stop `
  rabbitmq `
  sensor-simulator `
  node-red `
  api `
  worker-alerts `
  worker-recommendations `
  worker-readings`
  mongodb

Write-Host "Ambiente parado."
