#!/usr/bin/env bash
# Starts dev infrastructure for local API development.
# Usage:
#   ./dev-infra.sh              # full: postgres + redis + pgadmin + mailpit
#   ./dev-infra.sh local        # redis + mailpit only (use with a LOCAL postgres)
#   ./dev-infra.sh stop         # stop everything
#   ./dev-infra.sh local-stop   # stop only the local-mode services
#   ./dev-infra.sh logs         # tail logs

set -euo pipefail

SERVICES="postgres redis"
TOOLS_SERVICES="pgadmin mailpit"
LOCAL_TOOLS="mailpit"   # tools kept in local mode (skips pgadmin — talks to docker pg)

case "${1:-start}" in
  start)
    echo "Starting dev infrastructure (postgres + redis + pgadmin + mailpit)..."
    docker compose up -d $SERVICES
    docker compose --profile tools up -d $TOOLS_SERVICES
    echo "Done."
    echo "  API:     dotnet run --project Feezbow.API"
    echo "  pgAdmin: http://localhost:${PGADMIN_PORT:-5050}"
    echo "  Mailpit: http://localhost:${MAILPIT_UI_PORT:-8025}"
    ;;
  local)
    # Redis + Mailpit only. Postgres runs locally on the host (no container),
    # so we skip the docker postgres to avoid the 5432 port clash.
    echo "Starting local-mode infra (redis + mailpit)... using LOCAL postgres."
    docker compose up -d redis
    docker compose --profile tools up -d $LOCAL_TOOLS
    echo "Done."
    echo "  API:     dotnet run --project Feezbow.API   (postgres → localhost:5432)"
    echo "  Redis:   localhost:${REDIS_PORT:-6379}"
    echo "  Mailpit: http://localhost:${MAILPIT_UI_PORT:-8025}"
    ;;
  stop)
    docker compose --profile tools stop $TOOLS_SERVICES
    docker compose stop $SERVICES
    ;;
  local-stop)
    docker compose --profile tools stop $LOCAL_TOOLS
    docker compose stop redis
    ;;
  logs)
    docker compose --profile tools logs -f $SERVICES $TOOLS_SERVICES
    ;;
  *)
    echo "Usage: $0 [start|local|stop|local-stop|logs]"
    exit 1
    ;;
esac
