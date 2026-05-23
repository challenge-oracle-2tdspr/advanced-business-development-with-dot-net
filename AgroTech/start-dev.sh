#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/AgroTech"
COMPOSE_FILE="$PROJECT_DIR/compose.yaml"
PROJECT_NAME="agrotech"

mkdir -p "$SCRIPT_DIR/logs" "$SCRIPT_DIR/.run"

echo "[1/2] Removendo containers antigos..."
docker compose -p "$PROJECT_NAME" -f "$COMPOSE_FILE" down --remove-orphans || true

echo "[2/2] Subindo containers..."
docker compose -p "$PROJECT_NAME" -f "$COMPOSE_FILE" up -d --build \
  rabbitmq \
  sensor-simulator \
  node-red \
  mongodb \
  api \
  worker-alerts \
  worker-recommendations \
  worker-readings

echo
echo "Ambiente iniciado."
echo "API:                   http://localhost:5081"
echo "Swagger:               http://localhost:5081/swagger"
echo "RabbitMQ UI:           http://localhost:15672"
echo "Node-RED:              http://localhost:1880"
echo
echo "Serviços:"
echo "  - rabbitmq"
echo "  - sensor-simulator"
echo "  - node-red"
echo "  - mongodb"
echo "  - api"
echo "  - worker-alerts"
echo "  - worker-recommendations"
echo "  - worker-readings"
echo
echo "Logs úteis:"
echo "  docker compose -p \"$PROJECT_NAME\" -f \"$COMPOSE_FILE\" logs -f api"
echo "  docker compose -p \"$PROJECT_NAME\" -f \"$COMPOSE_FILE\" logs -f worker-alerts"
echo "  docker compose -p \"$PROJECT_NAME\" -f \"$COMPOSE_FILE\" logs -f worker-recommendations"
echo "  docker compose -p \"$PROJECT_NAME\" -f \"$COMPOSE_FILE\" logs -f worker-readings"
echo "  docker compose -p \"$PROJECT_NAME\" -f \"$COMPOSE_FILE\" logs -f node-red"
echo "  docker compose -p \"$PROJECT_NAME\" -f \"$COMPOSE_FILE\" logs -f sensor-simulator"
echo "  docker compose -p \"$PROJECT_NAME\" -f \"$COMPOSE_FILE\" logs -f mongodb"
echo
echo "Status:"
docker compose -p "$PROJECT_NAME" -f "$COMPOSE_FILE" ps