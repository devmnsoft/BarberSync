#!/usr/bin/env bash
set -euo pipefail

BASE_URL=${1:-http://localhost:5080}
: "${READINESS_ADMIN_EMAIL:?required}" "${READINESS_ADMIN_PASSWORD:?required}" "${READINESS_CASHIER_EMAIL:?required}" "${READINESS_CASHIER_PASSWORD:?required}"
: "${READINESS_PROFESSIONAL_EMAIL:?required}" "${READINESS_PROFESSIONAL_PASSWORD:?required}" "${READINESS_CLIENT_EMAIL:?required}" "${READINESS_CLIENT_PASSWORD:?required}"
: "${READINESS_TENANT_ID:?required}" "${READINESS_BRANCH_ID:?required}" "${READINESS_KIOSK_DEVICE_CODE:?required}"
tenant_slug=${READINESS_TENANT_SLUG:-production-readiness}
tmp=$(mktemp -d); trap 'rm -rf "$tmp"' EXIT

call() { # name method path token expected [body]
  local n=$1 m=$2 p=$3 token=$4 expected=$5 body=${6:-} status args=(-sS -o "$tmp/$n.json" -w '%{http_code}' -X "$m")
  [[ -z $token ]] || args+=(-H "Authorization: Bearer $token")
  [[ -z $body ]] || args+=(-H 'Content-Type: application/json' --data "$body")
  status=$(curl "${args[@]}" "$BASE_URL$p")
  [[ ",$expected," == *",$status,"* ]] || { echo "FAIL $n: $m $p -> $status (expected $expected)" >&2; cat "$tmp/$n.json" >&2; return 1; }
}
contains() { grep -Fqi -- "$2" "$tmp/$1.json" || { echo "FAIL $1: response does not contain expected readiness scope" >&2; cat "$tmp/$1.json" >&2; exit 1; }; }
not_contains() { ! grep -Fqi -- "$2" "$tmp/$1.json" || { echo "FAIL $1: response leaked forbidden scope" >&2; exit 1; }; }
login() {
  local n=$1 email=$2 password=$3
  call "$n" POST /api/auth/login '' 200 "{\"email\":\"$email\",\"password\":\"$password\",\"tenantSlug\":\"$tenant_slug\"}"
  sed -n 's/.*"accessToken"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "$tmp/$n.json" | head -1
}

call invalid_login POST /api/auth/login '' 400,401 '{"email":"invalid@readiness.local","password":"invalid-password","tenantSlug":"production-readiness"}'
contains invalid_login traceId
admin=$(login admin_login "$READINESS_ADMIN_EMAIL" "$READINESS_ADMIN_PASSWORD"); [[ -n $admin ]]
echo EVIDENCE:AUTH_SMOKE_LOGIN:PASS
call dashboard GET /api/dashboard/summary "$admin" 200; contains dashboard "$READINESS_TENANT_ID"; echo EVIDENCE:AUTH_SMOKE_DASHBOARD:PASS

client=$(login client_login "$READINESS_CLIENT_EMAIL" "$READINESS_CLIENT_PASSWORD")
call mobile_client GET /api/mobile/summary "$client" 200; contains mobile_client '"role":"Client"'; not_contains mobile_client commissions; not_contains mobile_client blocks
echo EVIDENCE:AUTH_SMOKE_MOBILE_CLIENT:PASS
professional=$(login professional_login "$READINESS_PROFESSIONAL_EMAIL" "$READINESS_PROFESSIONAL_PASSWORD")
call mobile_professional GET /api/mobile/summary "$professional" 200; contains mobile_professional '"role":"Professional"'; contains mobile_professional commissions; contains mobile_professional blocks; not_contains mobile_professional history
echo EVIDENCE:AUTH_SMOKE_MOBILE_PROFESSIONAL:PASS

call notifications_before GET /api/notifications "$admin" 200; contains notifications_before 'Readiness notification'
call notifications_read POST /api/notifications/read-all "$admin" 200,204
call notifications_after GET /api/notifications "$admin" 200; not_contains notifications_after '"status":"Unread"'
echo EVIDENCE:AUTH_SMOKE_NOTIFICATIONS:PASS
call stock GET /api/stock "$admin" 200; contains stock READINESS-PRODUCT; contains stock currentStock; contains stock minimumStock
echo EVIDENCE:AUTH_SMOKE_STOCK:PASS
cashier=$(login cashier_login "$READINESS_CASHIER_EMAIL" "$READINESS_CASHIER_PASSWORD")
call cash GET /api/cash-registers/current "$cashier" 200; contains cash "$READINESS_BRANCH_ID"
echo EVIDENCE:AUTH_SMOKE_CASH_REGISTER:PASS

# There is no stable, documented all-in-one create/add/pay/ledger verification contract yet.
echo EVIDENCE:AUTH_SMOKE_POS:SKIPPED_CONTRACT_NOT_FOUND
call kiosk_missing GET /api/kiosk/services '' 400
call kiosk_explicit GET "/api/kiosk/services?deviceCode=$READINESS_KIOSK_DEVICE_CODE" '' 200; contains kiosk_explicit 'Readiness Haircut'; not_contains kiosk_explicit 'KIOSK-001'
echo EVIDENCE:AUTH_SMOKE_KIOSK:PASS
