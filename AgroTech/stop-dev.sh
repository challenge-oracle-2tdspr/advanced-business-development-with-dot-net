#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/AgroTech"
COMPOSE_FILE="$PROJECT_DIR/compose.yaml"

echo "Parando processos .NET antigos (retrocompatibilidade)..."

for file in \
  "$SCRIPT_DIR/.run/agrotech-api.pid" \
  "$SCRIPT_DIR/.run/agrotech-worker-alerts.pid" \
  "$SCRIPT_DIR/.run/agrotech-worker-recommendations.pid" \
  "$SCRIPT_DIR/.run/agrotech-worker-readings.pid"
do
  if [ -f "$file" ]; then
    pid=$(cat "$file")
    if kill -0 "$pid" 2>/dev/null; then
      kill "$pid" || true
    fi
    rm -f "$file"
  fi
done

echo "Parando containers..."
docker compose -f "$COMPOSE_FILE" stop \
  rabbitmq \
  sensor-simulator \
  node-red \
  api \
  worker-alerts \
  worker-recommendations \
  worker-readings

echo "Ambiente parado."
