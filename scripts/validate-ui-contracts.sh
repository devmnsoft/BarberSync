#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"
fail=0
check() { local label="$1" pattern="$2"; shift 2; local found; found="$(rg -n --glob '!**/node_modules/**' --glob '!**/dist/**' --glob '!**/bin/**' --glob '!**/obj/**' "$pattern" "$@" || true)"; if [[ -n "$found" ]]; then printf 'UI contract violation (%s):\n%s\n' "$label" "$found" >&2; fail=1; fi; }
check 'empty anchor' "href=['\"]#['\"]" Web MobileApp Totem
check 'empty onclick' "onclick=['\"][[:space:]]*['\"]" Web MobileApp Totem
check 'Razor page directive inside MVC Views' '^[[:space:]]*@page([[:space:]]|$)' Web --glob '**/Views/**/*.cshtml'
check 'technical ID text field' "<input[^>]+type=['\"]text['\"][^>]+name=['\"][^'\"]*(tenant|branch|client|professional|service|appointment|serviceOrder|resource|room|chair|package|coupon|payment|schedule|waitlist|plan|membership|wallet|giftCard|voucher|combo|subscription|benefit|invoice)Id['\"]" Web MobileApp Totem
check 'technical ID placeholder' "placeholder=['\"][^'\"]*(ID|Id técnico|identificador técnico)[^'\"]*['\"]" Web MobileApp Totem
check 'catalog technical ID input' "<input[^>]+name=['\"][^'\"]*(tenant|branch|service|product|combo|package|pricingRule|commissionRule|professional|partner|category|supplier)Id['\"]" Web/BarberSync.AdminWeb/Views/Catalog
check 'service execution technical ID input' "<input[^>]+name=['\"][^'\"]*(tenant|branch|client|professional|service|product|serviceOrder|appointment|payment|cashSession|commission|stockMovement|wallet|voucher|coupon|giftCard|package|membership|partner)Id['\"]" Web/BarberSync.AdminWeb/Views/ServiceExecution
check 'finance360 technical ID input' "<input[^>]+name=['\"][^'\"]*(tenant|branch|payment|receivable|payable|cashSession|checkoutSession|serviceOrder|commission|settlement|payroll|payout|client|professional|partner|supplier|account|category|costCenter)Id['\"]" Web/BarberSync.AdminWeb/Views/Finance360
check 'inventory360 technical ID input' "<input[^>]+name=['\"][^'\"]*(tenant|branch|product|supply|supplier|service|batch|stockMovement|purchaseOrder|purchaseItem|transfer|inventoryCount|loss|costCenter|financePayable)Id['\"]" Web/BarberSync.AdminWeb/Views/Inventory360
check 'unfinished scheduling operation' '[Ee]m breve' Web/BarberSync.AdminWeb/Views/Scheduling Web/BarberSync.AdminWeb/wwwroot/js/scheduling.js Web/BarberSync.PublicWeb/Views/Booking Web/BarberSync.PublicWeb/wwwroot/js/public-booking.js
[[ $fail -eq 0 ]] || exit 1
printf 'EVIDENCE:UI_CONTRACTS_STATIC:PASS\n'
