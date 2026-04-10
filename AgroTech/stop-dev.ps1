$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Join-Path $ScriptDir "AgroTech"
$ComposeFile = Join-Path $ProjectDir "compose.yaml"

Write-Host "Parando processos .NET..."

$pidFiles = @(
    (Join-Path $ScriptDir ".run/agrotech-api.pid"),
    (Join-Path $ScriptDir ".run/agrotech-worker-alerts.pid"),
    (Join-Path $ScriptDir ".run/agrotech-worker-recommendations.pid")
)

foreach ($file in $pidFiles) {
    if (Test-Path $file) {
        $pid = Get-Content $file
        if ($pid) {
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
        }
        Remove-Item $file -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "Parando containers..."
docker compose -f $ComposeFile stop rabbitmq sensor-simulator node-red

Write-Host "Ambiente parado."
