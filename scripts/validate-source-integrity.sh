#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
fail=0
report(){ printf 'SOURCE_INTEGRITY: %s\n' "$1" >&2; fail=1; }
scan(){ local pattern=$1; shift; if rg -n "$pattern" "$@" --glob '!**/bin/**' --glob '!**/obj/**' --glob '!**/node_modules/**' --glob '!**/dist/**' --glob '!docs/SOURCE_CODE_AUDIT_REPORT.md' --glob '!scripts/validate-source-integrity.*' --glob '!**/*smoke-test.js'; then report "padrão proibido: $pattern"; fi; }
scan 'href="#"|onclick=""|NotImplementedException|mobile-demo-token' Backend Web MobileApp Totem
scan 'KIOSK-001' Backend Web MobileApp Totem --glob '!**/appsettings*.json'
scan 'placeholder="[^"]*(ID|Id)|type="text"[^>]+name="[^"]*[Ii]d"' Web MobileApp Totem
if find Web -path '*/Views/*' -name '*.cshtml' -type f -print0 | xargs -0 rg -n '^\s*@page\b'; then report '@page dentro de Views MVC'; fi
if find Web -path '*/Views/*/_*.cshtml' -type f -print0 | xargs -0 rg -n '^\s*@section\b'; then report '@section em partial'; fi
# Validate local Admin assets referenced by Razor.
python3 - <<'PYASSET' || { report "referência de asset ausente"; }
import pathlib,re,sys
root=pathlib.Path('Web/BarberSync.AdminWeb')
bad=[]
for view in (root/'Views').rglob('*.cshtml'):
    text=view.read_text(errors='ignore')
    for ref in re.findall(r'(?:src|href)="~?/((?:css|js)/[^"?]+\.(?:css|js))',text):
        if not (root/'wwwroot'/ref).is_file(): bad.append(f'{view}: {ref}')
if bad:
    print('\n'.join(bad));sys.exit(1)
PYASSET
# Every permission demanded by API code must exist in the canonical SQL seed.
while IFS= read -r permission; do rg -Fq "$permission" ScriptsSQL/script_completo.sql || report "permissão sem seed: $permission"; done < <(rg -o 'RequirePermission\("[^"]+"' Backend/Presentation/BarberSync.Api | cut -d'"' -f2 | sort -u)
# Core JavaScript must parse.
if command -v node >/dev/null; then find Web MobileApp Totem -name '*.js' -not -path '*/node_modules/*' -not -path '*/dist/*' -print0 | xargs -0 -r -n1 node --check || report 'node --check'; fi
# Catalog financial contracts reject binary floating-point and manual technical IDs.
if rg -n '\b(double|float)\b' Backend/Presentation/BarberSync.Api/Services/Catalog Backend/Presentation/BarberSync.Api/Controllers/CatalogControllers.cs; then report 'double/float no catálogo financeiro'; fi
if rg -n 'fake (price|commission|payment)|fakePrice|fakeCommission|fakePayment' Backend/Presentation/BarberSync.Api/Services/Catalog Backend/Presentation/BarberSync.Api/Controllers/CatalogControllers.cs Web/BarberSync.AdminWeb/Views/Catalog Web/BarberSync.AdminWeb/wwwroot/js/catalog.js -i; then report 'simulação falsa no catálogo'; fi
# Command Center contract is deliberately checked both ways.
for route in dashboard executive operations health alerts incidents tasks integrations reports/export filter-options; do if [[ "$route" == 'reports/export' ]]; then rg -Fq 'HttpGet("export")' Backend/Presentation/BarberSync.Api/Controllers/CommandCenterControllers.cs; else rg -Fq "$route" Backend/Presentation/BarberSync.Api/Controllers/CommandCenterControllers.cs; fi || report "rota Command Center ausente: $route"; rg -Fq "$route" docs/API_ROUTE_CONTRACTS.md || report "rota Command Center não documentada: $route"; done
# Atendimento 360: dinheiro decimal, sem fabricação e sem campo de ID técnico.
if rg -n '\b(double|float)\b' Backend/Presentation/BarberSync.Api/Services/ServiceExecution Backend/Presentation/BarberSync.Api/Controllers/ServiceExecutionControllers.cs; then report 'double/float no fluxo financeiro Atendimento 360'; fi
if rg -ni 'fake(payment| checkout| commission| stock)|fakePayment|fakeCheckout|fakeCommission|fakeStock' Backend/Presentation/BarberSync.Api/Services/ServiceExecution Backend/Presentation/BarberSync.Api/Controllers/ServiceExecutionControllers.cs Web/BarberSync.AdminWeb/Views/ServiceExecution Web/BarberSync.AdminWeb/wwwroot/js/service-execution.js; then report 'resultado fabricado no Atendimento 360'; fi

(( fail == 0 )) || exit 1
echo 'EVIDENCE:SOURCE_INTEGRITY_STATIC:PASS'
