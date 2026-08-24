#!/usr/bin/env bash
set -u
root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
production="$root_dir/artifacts/production-readiness"
evidence="$root_dir/artifacts/release-evidence"
output="$evidence/go-no-go.md"
mkdir -p "$evidence"

labels=(
  'dotnet restore' 'build Debug' 'build Release' 'SQL primeira aplicação'
  'SQL segunda aplicação' 'schema validation' 'API runtime' '/health'
  'production-smoke' 'node --check' 'Mobile smoke' 'Totem smoke' 'Totem build' 'PR #201 fechado ou inexistente'
  'readiness seed' 'authenticated smoke' 'auth login' 'auth dashboard' 'mobile client' 'mobile professional' 'notifications' 'stock' 'cash register'
)
markers=(
  DOTNET_RESTORE BUILD_DEBUG BUILD_RELEASE SQL_APPLY_1 SQL_APPLY_2 SCHEMA_VALIDATION API_RUNTIME HEALTH
  PRODUCTION_SMOKE FRONTEND_CHECKS MOBILE_SMOKE TOTEM_SMOKE TOTEM_BUILD PR_201_RESOLVED
  READINESS_SEED AUTHENTICATED_PRODUCTION_SMOKE AUTH_SMOKE_LOGIN AUTH_SMOKE_DASHBOARD AUTH_SMOKE_MOBILE_CLIENT AUTH_SMOKE_MOBILE_PROFESSIONAL AUTH_SMOKE_NOTIFICATIONS AUTH_SMOKE_STOCK AUTH_SMOKE_CASH_REGISTER
)
files=(
  "$production/dotnet-restore.log" "$production/dotnet-build-debug.log" "$production/dotnet-build-release.log"
  "$production/sql-apply-1.log" "$production/sql-apply-2.log" "$production/sql-apply-2.log"
  "$production/api-run.log" "$production/health.log" "$production/production-smoke.log"
  "$evidence/frontend-checks.md" "$evidence/frontend-checks.md" "$evidence/frontend-checks.md"
  "$evidence/frontend-checks.md" "$evidence/pr-status.md"
  "$production/readiness-seed.log" "$production/authenticated-production-smoke.log" "$production/authenticated-production-smoke.log" "$production/authenticated-production-smoke.log" "$production/authenticated-production-smoke.log" "$production/authenticated-production-smoke.log" "$production/authenticated-production-smoke.log" "$production/authenticated-production-smoke.log" "$production/authenticated-production-smoke.log"
)
missing=0
rows=''
for i in "${!markers[@]}"; do
  status='AUSENTE/REPROVADO'
  if [[ -f "${files[$i]}" ]] && grep -Fqx "EVIDENCE:${markers[$i]}:PASS" "${files[$i]}"; then status='PASS'; else missing=$((missing + 1)); fi
  rows+="| ${labels[$i]} | $status |"$'\n'
done
if [[ $missing -eq 0 ]]; then decision='GO'; reason='Todas as evidências obrigatórias foram encontradas.'; else decision='NO-GO'; reason="$missing evidência(s) obrigatória(s) ausente(s) ou reprovada(s)."; fi
cat > "$output" <<REPORT
# Resumo GO/NO-GO

- Decisão: **$decision**
- Motivo: $reason
- Gerado em UTC: $(date -u '+%Y-%m-%dT%H:%M:%SZ')

| Critério | Resultado |
| --- | --- |
$rows
Ausência de arquivo ou marcador explícito nunca é interpretada como sucesso. Mensagens como \`Docker is required\` não satisfazem nenhum critério.
REPORT
printf '%s: %s\n' "$decision" "$reason"
[[ "$decision" == GO ]]
