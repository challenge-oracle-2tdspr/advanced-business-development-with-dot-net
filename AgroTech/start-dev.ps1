$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Join-Path $ScriptDir "AgroTech"
$ComposeFile = Join-Path $ProjectDir "compose.yaml"

New-Item -ItemType Directory -Force -Path (Join-Path $ScriptDir "logs") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $ScriptDir ".run") | Out-Null

Write-Host "[1/1] Subindo containers..."
docker compose -f $ComposeFile up -d --build `
  rabbitmq `
  sensor-simulator `
  node-red `
  api `
  worker-alerts `
  worker-recommendations `
  worker-readings

Write-Host ""
Write-Host "Ambiente iniciado."
Write-Host "API:                   http://localhost:5081"
Write-Host "Swagger:               http://localhost:5081/swagger"
Write-Host "RabbitMQ UI:           http://localhost:15672"
Write-Host "Node-RED:              http://localhost:1880"
Write-Host ""
Write-Host "Serviços:"
Write-Host "  - rabbitmq"
Write-Host "  - sensor-simulator"
Write-Host "  - node-red"
Write-Host "  - api"
Write-Host "  - worker-alerts"
Write-Host "  - worker-recommendations"
Write-Host "  - worker-readings"
Write-Host ""
Write-Host "Logs úteis:"
Write-Host "  docker compose -f `"$ComposeFile`" logs -f api"
Write-Host "  docker compose -f `"$ComposeFile`" logs -f worker-alerts"
Write-Host "  docker compose -f `"$ComposeFile`" logs -f worker-recommendations"
Write-Host "  docker compose -f `"$ComposeFile`" logs -f worker-readings"
Write-Host "  docker compose -f `"$ComposeFile`" logs -f node-red"
Write-Host "  docker compose -f `"$ComposeFile`" logs -f sensor-simulator"
Write-Host ""
Write-Host "Status:"
docker compose -f $ComposeFile ps
