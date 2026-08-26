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
json_value() { python3 - "$tmp/$1.json" "$2" <<'PY'
import json,sys
value=json.load(open(sys.argv[1],encoding="utf-8"))
for part in sys.argv[2].split('.'):
    if isinstance(value,dict) and part in value: value=value[part]
    else: raise SystemExit("missing JSON path: "+sys.argv[2])
print(value)
PY
}
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
call team_dashboard GET /api/team/dashboard "$admin" 200; contains team_dashboard active_professionals
call team_professionals GET /api/professionals "$admin" 200; contains team_professionals Readiness
echo EVIDENCE:AUTH_SMOKE_TEAM:PASS
call finance_dashboard GET /api/finance/dashboard "$admin" 200; contains finance_dashboard revenue_month
call finance_categories GET /api/finance/categories "$admin" 200; contains finance_categories 'Readiness Expense'
call finance_suppliers GET /api/suppliers "$admin" 200; contains finance_suppliers 'Readiness Supplier'
call finance_payables GET /api/finance/payables "$admin" 200; contains finance_payables 'Readiness payable'
call finance_receivables GET /api/finance/receivables "$admin" 200; contains finance_receivables 'Readiness receivable'
echo EVIDENCE:AUTH_SMOKE_FINANCE:PASS
call inventory_dashboard GET /api/inventory/dashboard "$admin" 200; contains inventory_dashboard active_products
call inventory_products GET /api/inventory/products "$admin" 200; contains inventory_products 'Readiness Pomade'
call inventory_purchases GET /api/inventory/purchase-orders "$admin" 200; contains inventory_purchases 'PC-READINESS'
call inventory_replenishment GET /api/inventory/replenishment "$admin" 200; contains inventory_replenishment 'ProductionReadiness replenishment'
echo EVIDENCE:AUTH_SMOKE_INVENTORY:PASS
call analytics_executive GET /api/analytics/executive "$admin" 200; contains analytics_executive revenue_total
call analytics_operations GET /api/analytics/operations "$admin" 200; contains analytics_operations scheduled
call analytics_finance GET /api/analytics/finance "$admin" 200; contains analytics_finance cash_in
call analytics_alerts GET /api/analytics/alerts "$admin" 200; contains analytics_alerts 'Evento controlado de readiness'
echo EVIDENCE:AUTH_SMOKE_ANALYTICS:PASS

client=$(login client_login "$READINESS_CLIENT_EMAIL" "$READINESS_CLIENT_PASSWORD")
call mobile_client GET /api/mobile/summary "$client" 200; contains mobile_client '"role":"Client"'; not_contains mobile_client commissions; not_contains mobile_client blocks
echo EVIDENCE:AUTH_SMOKE_MOBILE_CLIENT:PASS
professional=$(login professional_login "$READINESS_PROFESSIONAL_EMAIL" "$READINESS_PROFESSIONAL_PASSWORD")
call mobile_professional GET /api/mobile/summary "$professional" 200; contains mobile_professional '"role":"Professional"'; contains mobile_professional appointments; contains mobile_professional commissions; contains mobile_professional goals; contains mobile_professional blocks; not_contains mobile_professional history
echo EVIDENCE:AUTH_SMOKE_MOBILE_PROFESSIONAL:PASS
echo EVIDENCE:AUTH_SMOKE_PROFESSIONAL_MOBILE:PASS

call notifications_before GET /api/notifications "$admin" 200; contains notifications_before 'Readiness notification'
call notifications_read POST /api/notifications/read-all "$admin" 200,204
call notifications_after GET /api/notifications "$admin" 200; not_contains notifications_after '"status":"Unread"'
echo EVIDENCE:AUTH_SMOKE_NOTIFICATIONS:PASS
call stock GET /api/stock "$admin" 200; contains stock READINESS-PRODUCT; contains stock currentStock; contains stock minimumStock
echo EVIDENCE:AUTH_SMOKE_STOCK:PASS
cashier=$(login cashier_login "$READINESS_CASHIER_EMAIL" "$READINESS_CASHIER_PASSWORD")
call cash GET /api/cash-registers/current "$cashier" 200; contains cash "$READINESS_BRANCH_ID"
echo EVIDENCE:AUTH_SMOKE_CASH_REGISTER:PASS

client_id=70000000-0000-4000-8000-000000000013
professional_id=70000000-0000-4000-8000-000000000012
service_id=70000000-0000-4000-8000-000000000030
product_id=70000000-0000-4000-8000-000000000040
initial_stock=$(python3 - "$tmp/stock.json" "$product_id" <<'PY'
import json,sys
def rows(v):
    if isinstance(v,list): return v
    if isinstance(v,dict):
        for k in ('data','items'):
            if k in v: return rows(v[k])
    return []
for row in rows(json.load(open(sys.argv[1]))):
    if str(row.get('id','')).lower()==sys.argv[2].lower(): print(row.get('currentStock')); break
else: raise SystemExit('readiness product not returned by /api/stock')
PY
)
call order_open POST /api/service-orders/open "$cashier" 201 "{\"clientId\":\"$client_id\",\"notes\":\"Production readiness authenticated smoke\"}"
order_id=$(json_value order_open id); [[ -n $order_id ]]
call order_service POST "/api/service-orders/$order_id/items/services" "$cashier" 200 "{\"serviceId\":\"$service_id\",\"professionalId\":\"$professional_id\",\"quantity\":1}"
call order_product POST "/api/service-orders/$order_id/items/products" "$cashier" 200 "{\"productId\":\"$product_id\",\"quantity\":1}"
[[ $(json_value order_product total) == 65* ]] || { echo 'FAIL POS total is not 65.00' >&2; exit 1; }
echo EVIDENCE:AUTH_SMOKE_SERVICE_ORDER:PASS
idempotency_key="readiness-$order_id"
call payment POST "/api/service-orders/$order_id/payments" "$cashier" 200 "{\"idempotencyKey\":\"$idempotency_key\",\"splits\":[{\"method\":\"Cash\",\"amount\":65,\"receivedAmount\":65}],\"note\":\"Production readiness\"}"
payment_id=$(json_value payment id); [[ $(json_value payment orderStatus) == Paid ]]
call order_paid GET "/api/service-orders/$order_id" "$cashier" 200; contains order_paid '"status":"Paid"'
echo EVIDENCE:AUTH_SMOKE_PAYMENT:PASS
call stock_after GET /api/stock "$admin" 200
final_stock=$(python3 - "$tmp/stock_after.json" "$product_id" <<'PY'
import json,sys
def rows(v):
    if isinstance(v,list): return v
    if isinstance(v,dict):
        for k in ('data','items'):
            if k in v:return rows(v[k])
    return []
for row in rows(json.load(open(sys.argv[1]))):
    if str(row.get('id','')).lower()==sys.argv[2].lower(): print(row.get('currentStock'));break
else: raise SystemExit('readiness product missing after payment')
PY
)
python3 - "$initial_stock" "$final_stock" <<'PY'
import sys
assert float(sys.argv[2]) == float(sys.argv[1])-1, 'product stock did not decrease exactly once'
PY
call stock_movements GET /api/stock/movements "$admin" 200; contains stock_movements "$order_id"
echo EVIDENCE:AUTH_SMOKE_STOCK_MOVEMENT:PASS
call cash_after GET /api/cash-registers/current "$cashier" 200; contains cash_after '"type":"Sale"'; contains cash_after "$payment_id"
echo EVIDENCE:AUTH_SMOKE_CASH_MOVEMENT:PASS
call finance GET /api/finance "$admin" 200; contains finance "$payment_id"; contains finance "$order_id"
echo EVIDENCE:AUTH_SMOKE_FINANCIAL_ENTRY:PASS
call commissions GET /api/commissions "$admin" 200; contains commissions "$payment_id"; contains commissions "$professional_id"
echo EVIDENCE:AUTH_SMOKE_COMMISSION:PASS
call audit GET /api/audit "$admin" 200; contains audit "$payment_id"; contains audit 'Payment.Register'
echo EVIDENCE:AUTH_SMOKE_POS:PASS
call kiosk_missing GET /api/kiosk/services '' 400
call kiosk_explicit GET "/api/kiosk/services?deviceCode=$READINESS_KIOSK_DEVICE_CODE" '' 200; contains kiosk_explicit 'Readiness Haircut'; not_contains kiosk_explicit 'KIOSK-001'
echo EVIDENCE:AUTH_SMOKE_KIOSK:PASS
