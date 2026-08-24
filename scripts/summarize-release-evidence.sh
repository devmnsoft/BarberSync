#!/usr/bin/env bash
set -u
root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
production="$root_dir/artifacts/production-readiness"
evidence="$root_dir/artifacts/release-evidence"
output="$evidence/go-no-go.md"
mkdir -p "$evidence"

markers=(
  READINESS_CONTRACTS_STATIC RESTORE BUILD_DEBUG BUILD_RELEASE SQL_APPLY_1 SQL_APPLY_2 SCHEMA_VALIDATION
  READINESS_SEED API_RUNTIME HEALTH PRODUCTION_SMOKE AUTHENTICATED_PRODUCTION_SMOKE
  AUTH_SMOKE_LOGIN AUTH_SMOKE_DASHBOARD AUTH_SMOKE_MOBILE_CLIENT AUTH_SMOKE_MOBILE_PROFESSIONAL
  AUTH_SMOKE_NOTIFICATIONS AUTH_SMOKE_STOCK AUTH_SMOKE_CASH_REGISTER AUTH_SMOKE_SERVICE_ORDER
  AUTH_SMOKE_PAYMENT AUTH_SMOKE_STOCK_MOVEMENT AUTH_SMOKE_CASH_MOVEMENT AUTH_SMOKE_FINANCIAL_ENTRY
  AUTH_SMOKE_COMMISSION AUTH_SMOKE_POS FRONTEND_CHECKS MOBILE_SMOKE TOTEM_SMOKE TOTEM_BUILD
)
files=(
  readiness-contracts-static.log dotnet-restore.log dotnet-build-debug.log dotnet-build-release.log
  sql-apply-1.log sql-apply-2.log schema-validation.log readiness-seed.log api-run.log health.log
  production-smoke.log authenticated-production-smoke.log authenticated-production-smoke.log
  authenticated-production-smoke.log authenticated-production-smoke.log authenticated-production-smoke.log
  authenticated-production-smoke.log authenticated-production-smoke.log authenticated-production-smoke.log
  authenticated-production-smoke.log authenticated-production-smoke.log authenticated-production-smoke.log
  authenticated-production-smoke.log authenticated-production-smoke.log authenticated-production-smoke.log
  authenticated-production-smoke.log frontend.log mobile-smoke.log totem-smoke.log totem-build.log
)
passed=() missing=() failed=()
for i in "${!markers[@]}"; do
  marker="${markers[$i]}" file="$production/${files[$i]}"
  if [[ ! -f "$file" ]]; then
    missing+=("$marker (${files[$i]} ausente)")
  elif grep -Fqx "EVIDENCE:$marker:PASS" "$file"; then
    passed+=("$marker")
  elif grep -Eq "EVIDENCE:${marker}:(FAIL|ERROR|SKIPPED)" "$file"; then
    failed+=("$marker ($(grep -E "EVIDENCE:${marker}:(FAIL|ERROR|SKIPPED)" "$file" | tail -1))")
  else
    missing+=("$marker (marker PASS ausente em ${files[$i]})")
  fi
done

if ((${#missing[@]} == 0 && ${#failed[@]} == 0)); then
  decision=GO; reason='Todos os markers obrigatórios foram encontrados com PASS.'
else
  decision=NO-GO; reason="faltam ${#missing[@]} marker(s) e ${#failed[@]} marker(s) falharam."
fi
list_section() {
  local title="$1"; shift
  echo "## $title"
  if (($# == 0)); then echo '- Nenhum.'; else printf -- '- `%s`\n' "$@"; fi
  echo
}
{
  echo '# GO/NO-GO'; echo; echo "Status: $decision"; echo
  list_section 'Passed markers' "${passed[@]}"
  list_section 'Missing markers' "${missing[@]}"
  list_section 'Failed markers' "${failed[@]}"
  echo '## Decision'; echo
  if [[ "$decision" == GO ]]; then echo 'GO: todos os markers obrigatórios passaram.'; else echo "NO-GO porque $reason"; fi
  echo; echo 'Ausência de arquivo ou marker explícito nunca é interpretada como sucesso.'
  echo "Gerado em UTC: $(date -u '+%Y-%m-%dT%H:%M:%SZ')"
} > "$output"
printf '%s: %s\n' "$decision" "$reason"
[[ "$decision" == GO ]]
