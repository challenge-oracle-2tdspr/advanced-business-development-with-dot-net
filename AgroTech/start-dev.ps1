$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Join-Path $ScriptDir "AgroTech"
$ComposeFile = Join-Path $ProjectDir "compose.yaml"

New-Item -ItemType Directory -Force (Join-Path $ScriptDir "logs") | Out-Null
New-Item -ItemType Directory -Force (Join-Path $ScriptDir ".run") | Out-Null

Write-Host "[1/4] Subindo containers..."
docker compose -f $ComposeFile up -d --build rabbitmq sensor-simulator node-red

Write-Host "[2/4] Subindo API..."
$apiLog = Join-Path $ScriptDir "logs/agrotech-api.log"
$api = Start-Process dotnet -ArgumentList "run --project `"$ProjectDir/AgroTech`"" -RedirectStandardOutput $apiLog -RedirectStandardError $apiLog -PassThru
$api.Id | Set-Content (Join-Path $ScriptDir ".run/agrotech-api.pid")

Write-Host "[3/4] Subindo Worker Alerts..."
$alertsLog = Join-Path $ScriptDir "logs/agrotech-worker-alerts.log"
$alerts = Start-Process dotnet -ArgumentList "run --project `"$ProjectDir/AgroTech.Worker.Alerts`"" -RedirectStandardOutput $alertsLog -RedirectStandardError $alertsLog -PassThru
$alerts.Id | Set-Content (Join-Path $ScriptDir ".run/agrotech-worker-alerts.pid")

Write-Host "[4/4] Subindo Worker Recommendations..."
$recsLog = Join-Path $ScriptDir "logs/agrotech-worker-recommendations.log"
$recs = Start-Process dotnet -ArgumentList "run --project `"$ProjectDir/AgroTech.Worker.Recommendations`"" -RedirectStandardOutput $recsLog -RedirectStandardError $recsLog -PassThru
$recs.Id | Set-Content (Join-Path $ScriptDir ".run/agrotech-worker-recommendations.pid")

Write-Host ""
Write-Host "Ambiente iniciado."
Write-Host "API:         http://localhost:5081"
Write-Host "Swagger:     http://localhost:5081/swagger"
Write-Host "RabbitMQ UI: http://localhost:15672"
Write-Host "Node-RED:    http://localhost:1880"
