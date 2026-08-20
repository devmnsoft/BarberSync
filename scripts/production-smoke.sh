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
  if [[ "$status" != "$expected" ]]; then
    echo "FAIL ${name}: ${method} ${path} returned ${status}, expected ${expected}" >&2
    cat "$response" >&2
    return 1
  fi
  echo "PASS ${name}: ${method} ${path} -> ${status}"
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

request health GET /health 200
grep -Eq '"database"[[:space:]]*:[[:space:]]*"Healthy"' "$work_dir/health.json" || {
  echo 'FAIL health: PostgreSQL is not healthy' >&2
  cat "$work_dir/health.json" >&2
  exit 1
}

request protected GET /api/dashboard 401
request invalid_login POST /api/auth/login 401 '{"email":"nobody@example.invalid","password":"DefinitelyInvalid123!","tenantSlug":"missing"}'
require_trace invalid_login
request notifications GET /api/notifications 401
request reports GET /api/finance 401
request stock GET /api/stock 401
request cash_registers GET /api/cash-registers/current 401
request service_orders GET /api/service-orders 401

for name in protected notifications reports stock cash_registers service_orders; do
  require_trace "$name"
done

echo 'Production HTTP smoke checks passed.'
