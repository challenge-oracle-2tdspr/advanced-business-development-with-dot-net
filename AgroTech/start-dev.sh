#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/AgroTech"
COMPOSE_FILE="$PROJECT_DIR/compose.yaml"

mkdir -p "$SCRIPT_DIR/logs" "$SCRIPT_DIR/.run"

echo "[1/4] Subindo containers..."
docker compose -f "$COMPOSE_FILE" up -d --build rabbitmq sensor-simulator node-red

echo "[2/4] Subindo API..."
nohup dotnet run --project "$PROJECT_DIR/AgroTech" > "$SCRIPT_DIR/logs/agrotech-api.log" 2>&1 &
echo $! > "$SCRIPT_DIR/.run/agrotech-api.pid"

echo "[3/4] Subindo Worker Alerts..."
nohup dotnet run --project "$PROJECT_DIR/AgroTech.Worker.Alerts" > "$SCRIPT_DIR/logs/agrotech-worker-alerts.log" 2>&1 &
echo $! > "$SCRIPT_DIR/.run/agrotech-worker-alerts.pid"

echo "[4/4] Subindo Worker Recommendations..."
nohup dotnet run --project "$PROJECT_DIR/AgroTech.Worker.Recommendations" > "$SCRIPT_DIR/logs/agrotech-worker-recommendations.log" 2>&1 &
echo $! > "$SCRIPT_DIR/.run/agrotech-worker-recommendations.pid"

echo
echo "Ambiente iniciado."
echo "API:         http://localhost:5081"
echo "Swagger:     http://localhost:5081/swagger"
echo "RabbitMQ UI: http://localhost:15672"
echo "Node-RED:    http://localhost:1880"
echo
echo "Logs úteis:"
echo "  tail -f $SCRIPT_DIR/logs/agrotech-api.log"
echo "  tail -f $SCRIPT_DIR/logs/agrotech-worker-alerts.log"
echo "  tail -f $SCRIPT_DIR/logs/agrotech-worker-recommendations.log"
echo "  docker compose -f $COMPOSE_FILE logs -f sensor-simulator"
echo "  docker compose -f $COMPOSE_FILE logs -f node-red"
