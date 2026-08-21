#!/usr/bin/env bash
set -euo pipefail

base_url="${1:-http://localhost:5080}"
work_dir="$(mktemp -d)"
trap 'rm -rf "$work_dir"' EXIT

request() {
  local name="$1" method="$2" path="$3" expected="$4" body="${5:-}"
  local headers="$work_dir/${name}.headers" response="$work_dir/${name}.json" status
  local args=(-sS -D "$headers" -o "$response" -w '%{http_code}' -X "$method")
  if [[ -n "$body" ]]; then
    args+=(-H 'Content-Type: application/json' --data "$body")
  fi
  status="$(curl "${args[@]}" "${base_url}${path}")"
  if [[ ",$expected," != *",$status,"* ]]; then
    echo "FAIL ${name}: ${method} ${path} returned ${status}, expected ${expected}" >&2
    cat "$response" >&2
    return 1
  fi
  echo "PASS ${name}: ${method} ${path} -> ${status}"
}

wait_for_health() {
  local attempts="${SMOKE_HEALTH_ATTEMPTS:-60}" delay="${SMOKE_HEALTH_DELAY_SECONDS:-2}"
  local attempt

  for ((attempt = 1; attempt <= attempts; attempt++)); do
    if request health GET /health 200; then
      return 0
    fi
    echo "WAIT health: attempt ${attempt}/${attempts}; retrying in ${delay}s" >&2
    sleep "$delay"
  done

  echo "FAIL health: API did not become ready after ${attempts} attempts" >&2
  return 1
}

require_trace() {
  local name="$1"
  if ! grep -qi '^X-Trace-Id: .' "$work_dir/${name}.headers" &&
     ! grep -Eq '"traceId"[[:space:]]*:[[:space:]]*"[^"[:space:]]+' "$work_dir/${name}.json"; then
    echo "FAIL ${name}: error response has no traceId or X-Trace-Id" >&2
    cat "$work_dir/${name}.json" >&2
    return 1
  fi
}

wait_for_health
grep -Eq '"database"[[:space:]]*:[[:space:]]*"Healthy"' "$work_dir/health.json" || {
  echo 'FAIL health: PostgreSQL is not healthy' >&2
  cat "$work_dir/health.json" >&2
  exit 1
}

# Use a route that is actually mapped by DashboardController.  Testing the
# controller prefix alone exercises the 404 path instead of the authorization
# policy and makes the readiness gate fail before it can validate protection.
request protected GET /api/dashboard/summary 401
request invalid_login POST /api/auth/login 400,401 '{"email":"nobody@example.invalid","password":"DefinitelyInvalid123!","tenantSlug":"missing"}'
require_trace invalid_login
request notifications GET /api/notifications 401
request finance GET /api/finance 401
request stock GET /api/stock 401
request cash_registers GET /api/cash-registers/current 401
request service_orders GET /api/service-orders 401
request purchases GET /api/purchases 401
request service_recognition GET /api/service-recognition/suggestions 401
request ai_settings GET /api/system/ai-settings 401
request copilot_suggestions GET '/api/copilot/suggestions?tenantId=11111111-1111-1111-1111-111111111111' 401

for name in protected notifications finance stock cash_registers service_orders purchases service_recognition ai_settings copilot_suggestions; do
  require_trace "$name"
done

echo 'Production HTTP smoke checks passed.'
