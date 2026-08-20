#!/usr/bin/env bash
set -euo pipefail

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
compose_file="$root_dir/docker-compose.production-readiness.yml"
log_dir="$root_dir/artifacts/production-readiness"
project_name="barbersync-production-readiness"
compose=(docker compose -p "$project_name" -f "$compose_file")

command -v docker >/dev/null 2>&1 || { echo "ERROR: Docker is required." >&2; exit 1; }
docker info >/dev/null 2>&1 || { echo "ERROR: the Docker daemon is unavailable." >&2; exit 1; }
docker compose version >/dev/null 2>&1 || { echo "ERROR: Docker Compose v2 is required." >&2; exit 1; }

mkdir -p "$log_dir"
rm -f "$log_dir"/*.log
cd "$root_dir"

cleanup() {
  local status=$?
  if [[ $status -ne 0 ]]; then
    "${compose[@]}" logs --no-color api >"$log_dir/api-run.log" 2>&1 || true
  fi
  "${compose[@]}" down --remove-orphans >/dev/null 2>&1 || true
  exit "$status"
}
trap cleanup EXIT INT TERM

run_logged() {
  local log="$1"; shift
  "$@" 2>&1 | tee "$log_dir/$log"
}

echo "Starting PostgreSQL 16..."
"${compose[@]}" up -d --wait postgres
run_logged dotnet-info.log "${compose[@]}" run --rm --no-deps api dotnet --info
run_logged dotnet-restore.log "${compose[@]}" run --rm --no-deps api dotnet restore BarberSync.sln
run_logged dotnet-build-debug.log "${compose[@]}" run --rm --no-deps api dotnet build BarberSync.sln --configuration Debug --no-restore
run_logged dotnet-build-release.log "${compose[@]}" run --rm --no-deps api dotnet build BarberSync.sln --configuration Release --no-restore

run_logged sql-apply-1.log bash -c 'docker compose -p "$1" -f "$2" exec -T postgres psql "host=localhost port=5432 dbname=barber user=postgres password=postgres" -v ON_ERROR_STOP=1 < "$3"' _ "$project_name" "$compose_file" "$root_dir/ScriptsSQL/script_completo.sql"
run_logged sql-apply-2.log bash -c 'docker compose -p "$1" -f "$2" exec -T postgres psql "host=localhost port=5432 dbname=barber user=postgres password=postgres" -v ON_ERROR_STOP=1 < "$3"' _ "$project_name" "$compose_file" "$root_dir/ScriptsSQL/script_completo.sql"
docker compose -p "$project_name" -f "$compose_file" exec -T postgres psql "host=localhost port=5432 dbname=barber user=postgres password=postgres" -v ON_ERROR_STOP=1 < "$root_dir/scripts/validate-production-schema.sql" 2>&1 | tee -a "$log_dir/sql-apply-2.log"

"${compose[@]}" up -d api
ready=false
for _ in {1..60}; do
  if "${compose[@]}" exec -T api bash -lc 'curl -fsS http://localhost:5080/health' >"$log_dir/health.log" 2>&1; then ready=true; break; fi
  sleep 2
done
"${compose[@]}" logs --no-color api >"$log_dir/api-run.log" 2>&1
[[ "$ready" == true ]] || { echo "ERROR: API did not become healthy within 120 seconds." >&2; exit 1; }
cat "$log_dir/health.log"

run_logged production-smoke.log "${compose[@]}" run --rm --no-deps api bash -lc './scripts/production-smoke.sh http://api:5080'
run_logged frontend.log "${compose[@]}" --profile tools run --rm node 'find Web -name "*.js" -print0 | xargs -0 -r -n1 node --check'
run_logged mobile-smoke.log "${compose[@]}" --profile tools run --rm node 'npm test --prefix MobileApp'
run_logged totem-smoke.log "${compose[@]}" --profile tools run --rm node 'npm test --prefix Totem'
run_logged totem-build.log "${compose[@]}" --profile tools run --rm node 'npm run build --prefix Totem'

echo "Production readiness passed. Logs: $log_dir"
