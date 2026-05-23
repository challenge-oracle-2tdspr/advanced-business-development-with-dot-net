#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/AgroTech"
COMPOSE_FILE="$PROJECT_DIR/compose.yaml"
PROJECT_NAME="agrotech"

echo "Parando processos .NET antigos (retrocompatibilidade)..."

for file in \
  "$SCRIPT_DIR/.run/agrotech-api.pid" \
  "$SCRIPT_DIR/.run/agrotech-worker-alerts.pid" \
  "$SCRIPT_DIR/.run/agrotech-worker-recommendations.pid" \
  "$SCRIPT_DIR/.run/agrotech-worker-readings.pid" \
  "$SCRIPT_DIR/.run/agrotech-mongodb.pid"
do
  if [ -f "$file" ]; then
    pid=$(cat "$file")
    if kill -0 "$pid" 2>/dev/null; then
      kill "$pid" || true
    fi
    rm -f "$file"
  fi
done

echo "Removendo containers e rede do ambiente..."
docker compose -p "$PROJECT_NAME" -f "$COMPOSE_FILE" down --remove-orphans

echo "Ambiente parado."