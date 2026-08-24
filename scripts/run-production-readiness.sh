#!/usr/bin/env bash
set -euo pipefail

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
compose_file="$root_dir/docker-compose.production-readiness.yml"
log_dir="$root_dir/artifacts/production-readiness"
project_name="barbersync-production-readiness"
compose=(docker compose -p "$project_name" -f "$compose_file")

mkdir -p "$log_dir"
rm -f "$log_dir"/*.log
"$root_dir/scripts/validate-readiness-contracts.sh" 2>&1 | tee "$log_dir/readiness-contracts-static.log"
printf 'EVIDENCE:READINESS_CONTRACTS_STATIC:PASS\n' | tee -a "$log_dir/readiness-contracts-static.log"

command -v docker >/dev/null 2>&1 || { echo "ERROR: Docker is required." >&2; exit 1; }
docker info >/dev/null 2>&1 || { echo "ERROR: the Docker daemon is unavailable." >&2; exit 1; }
docker compose version >/dev/null 2>&1 || { echo "ERROR: Docker Compose v2 is required." >&2; exit 1; }

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

mark_pass() {
  printf 'EVIDENCE:%s:PASS\n' "$1" | tee -a "$log_dir/$2"
}

echo "Starting PostgreSQL 16..."
"${compose[@]}" up -d --wait postgres
run_logged dotnet-info.log "${compose[@]}" run --rm --no-deps api dotnet --info
run_logged dotnet-restore.log "${compose[@]}" run --rm --no-deps api dotnet restore BarberSync.sln
mark_pass DOTNET_RESTORE dotnet-restore.log
run_logged dotnet-build-debug.log "${compose[@]}" run --rm --no-deps api dotnet build BarberSync.sln --configuration Debug --no-restore
mark_pass BUILD_DEBUG dotnet-build-debug.log
run_logged dotnet-build-release.log "${compose[@]}" run --rm --no-deps api dotnet build BarberSync.sln --configuration Release --no-restore
mark_pass BUILD_RELEASE dotnet-build-release.log

run_logged sql-apply-1.log bash -c 'docker compose -p "$1" -f "$2" exec -T postgres psql "host=localhost port=5432 dbname=barber user=postgres password=postgres" -v ON_ERROR_STOP=1 < "$3"' _ "$project_name" "$compose_file" "$root_dir/ScriptsSQL/script_completo.sql"
mark_pass SQL_APPLY_1 sql-apply-1.log
run_logged sql-apply-2.log bash -c 'docker compose -p "$1" -f "$2" exec -T postgres psql "host=localhost port=5432 dbname=barber user=postgres password=postgres" -v ON_ERROR_STOP=1 < "$3"' _ "$project_name" "$compose_file" "$root_dir/ScriptsSQL/script_completo.sql"
mark_pass SQL_APPLY_2 sql-apply-2.log
docker compose -p "$project_name" -f "$compose_file" exec -T postgres psql "host=localhost port=5432 dbname=barber user=postgres password=postgres" -v ON_ERROR_STOP=1 < "$root_dir/scripts/validate-production-schema.sql" 2>&1 | tee -a "$log_dir/sql-apply-2.log"
mark_pass SCHEMA_VALIDATION sql-apply-2.log
run_logged readiness-seed.log bash -c 'docker compose -p "$1" -f "$2" exec -T postgres psql "host=localhost port=5432 dbname=barber user=postgres password=postgres options=-cbarbersync.environment=ProductionReadiness" -v ON_ERROR_STOP=1 < "$3"' _ "$project_name" "$compose_file" "$root_dir/ScriptsSQL/production_readiness_seed.sql"
mark_pass READINESS_SEED readiness-seed.log

"${compose[@]}" up -d api
ready=false
for _ in {1..60}; do
  if "${compose[@]}" exec -T api bash -lc 'curl -fsS http://localhost:5080/health' >"$log_dir/health.log" 2>&1; then ready=true; break; fi
  sleep 2
done
"${compose[@]}" logs --no-color api >"$log_dir/api-run.log" 2>&1
[[ "$ready" == true ]] || { echo "ERROR: API did not become healthy within 120 seconds." >&2; exit 1; }
mark_pass API_RUNTIME api-run.log
mark_pass HEALTH health.log
cat "$log_dir/health.log"

run_logged production-smoke.log "${compose[@]}" run --rm --no-deps api bash -lc './scripts/production-smoke.sh http://api:5080'
mark_pass PRODUCTION_SMOKE production-smoke.log
export READINESS_ADMIN_EMAIL=${READINESS_ADMIN_EMAIL:-admin@readiness.local} READINESS_ADMIN_PASSWORD=${READINESS_ADMIN_PASSWORD:-ReadinessOnly\!2026}
export READINESS_CASHIER_EMAIL=${READINESS_CASHIER_EMAIL:-cashier@readiness.local} READINESS_CASHIER_PASSWORD=${READINESS_CASHIER_PASSWORD:-ReadinessOnly\!2026}
export READINESS_PROFESSIONAL_EMAIL=${READINESS_PROFESSIONAL_EMAIL:-professional@readiness.local} READINESS_PROFESSIONAL_PASSWORD=${READINESS_PROFESSIONAL_PASSWORD:-ReadinessOnly\!2026}
export READINESS_CLIENT_EMAIL=${READINESS_CLIENT_EMAIL:-client@readiness.local} READINESS_CLIENT_PASSWORD=${READINESS_CLIENT_PASSWORD:-ReadinessOnly\!2026}
export READINESS_TENANT_ID=${READINESS_TENANT_ID:-70000000-0000-4000-8000-000000000001} READINESS_BRANCH_ID=${READINESS_BRANCH_ID:-70000000-0000-4000-8000-000000000002} READINESS_KIOSK_DEVICE_CODE=${READINESS_KIOSK_DEVICE_CODE:-READINESS-KIOSK-001}
run_logged authenticated-production-smoke.log "${compose[@]}" run --rm --no-deps -e READINESS_ADMIN_EMAIL -e READINESS_ADMIN_PASSWORD -e READINESS_CASHIER_EMAIL -e READINESS_CASHIER_PASSWORD -e READINESS_PROFESSIONAL_EMAIL -e READINESS_PROFESSIONAL_PASSWORD -e READINESS_CLIENT_EMAIL -e READINESS_CLIENT_PASSWORD -e READINESS_TENANT_ID -e READINESS_BRANCH_ID -e READINESS_KIOSK_DEVICE_CODE api bash -lc './scripts/authenticated-production-smoke.sh http://api:5080'
mark_pass AUTHENTICATED_PRODUCTION_SMOKE authenticated-production-smoke.log
run_logged frontend.log "${compose[@]}" --profile tools run --rm node 'find Web -name "*.js" -print0 | xargs -0 -r -n1 node --check'
mark_pass FRONTEND_CHECKS frontend.log
run_logged mobile-smoke.log "${compose[@]}" --profile tools run --rm node 'npm test --prefix MobileApp'
mark_pass MOBILE_SMOKE mobile-smoke.log
run_logged totem-smoke.log "${compose[@]}" --profile tools run --rm node 'npm test --prefix Totem'
mark_pass TOTEM_SMOKE totem-smoke.log
run_logged totem-build.log "${compose[@]}" --profile tools run --rm node 'npm run build --prefix Totem'
mark_pass TOTEM_BUILD totem-build.log

echo "Production readiness passed. Logs: $log_dir"
