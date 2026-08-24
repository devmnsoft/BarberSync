#!/usr/bin/env bash
set -u

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
failures=0

check() {
  local description="$1"; shift
  if "$@"; then printf 'OK: %s\n' "$description"; else printf 'FAIL: %s\n' "$description" >&2; failures=$((failures + 1)); fi
}
contains() { grep -Eq "$2" "$1"; }

cash_dto="$root_dir/Backend/Application/BarberSync.Application/Operations/CashRegisters.cs"
cash_repo="$root_dir/Backend/Infrastructure/BarberSync.Infrastructure/Repositories/PostgresCashRegisterRepository.cs"
stock_controller="$root_dir/Backend/Presentation/BarberSync.Api/Controllers/StockController.cs"
schema="$root_dir/ScriptsSQL/script_completo.sql"
seed="$root_dir/ScriptsSQL/production_readiness_seed.sql"
pos_markers=(AUTH_SMOKE_SERVICE_ORDER AUTH_SMOKE_PAYMENT AUTH_SMOKE_STOCK_MOVEMENT AUTH_SMOKE_CASH_MOVEMENT AUTH_SMOKE_FINANCIAL_ENTRY AUTH_SMOKE_COMMISSION AUTH_SMOKE_POS)

check 'CashMovementResponse exposes nullable Guid PaymentId' contains "$cash_dto" 'CashMovementResponse\([^)]*Guid\? PaymentId'
check 'cash register repository selects payment_id' contains "$cash_repo" 'SELECT .*payment_id FROM barber\.cash_movements'
check 'cash_movements schema has nullable payment_id' contains "$schema" 'cash_movements \([^;]*payment_id uuid REFERENCES barber\.payments\(id\)'
check 'StockController exposes authenticated GET movements route' contains "$stock_controller" '\[HttpGet\("movements"\), RequirePermission\("Stock\.View"\)\]'

for marker in "${pos_markers[@]}"; do
  check "authenticated smoke shell emits $marker" contains "$root_dir/scripts/authenticated-production-smoke.sh" "EVIDENCE:${marker}:PASS"
  check "authenticated smoke PowerShell emits $marker" contains "$root_dir/scripts/authenticated-production-smoke.ps1" "EVIDENCE:${marker}:PASS"
  check "shell summarizer requires $marker" contains "$root_dir/scripts/summarize-release-evidence.sh" "(^|[[:space:]])${marker}([[:space:]]|$)"
  check "PowerShell summarizer requires $marker" contains "$root_dir/scripts/summarize-release-evidence.ps1" "Marker=\"${marker}\""
done

check 'readiness seed is guarded by ProductionReadiness setting' contains "$seed" "current_setting\('barbersync\.environment'.*ProductionReadiness"
check 'readiness seed has no destructive TRUNCATE or DROP statement' bash -c "! grep -Eiq '^[[:space:]]*(TRUNCATE|DROP)[[:space:]]' \"$seed\""
for smoke in "$root_dir/scripts/authenticated-production-smoke.sh" "$root_dir/scripts/authenticated-production-smoke.ps1"; do
  forbidden='SKIPPED_CONTRACT_'"NOT_FOUND"
  check "$(basename "$smoke") has no obsolete POS skip path" bash -c "! grep -Fq '$forbidden' \"$smoke\""
done

if (( failures > 0 )); then printf 'FAIL: %d readiness contract(s) failed.\n' "$failures" >&2; exit 1; fi
printf 'OK: all readiness contracts passed static validation.\n'
