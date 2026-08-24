#!/usr/bin/env bash
set -u
root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
evidence="$root_dir/artifacts/release-evidence"
production="$root_dir/artifacts/production-readiness"
mkdir -p "$evidence"
cd "$root_dir"

capture() { local file="$1"; shift; { echo '```text'; "$@"; local rc=$?; echo '```'; echo; echo "Exit code: $rc"; return "$rc"; } >> "$file" 2>&1; }

static="$evidence/readiness-contracts-static.md"
echo '# Validação estática de contratos de readiness' > "$static"
capture "$static" "$root_dir/scripts/validate-readiness-contracts.sh" || true

cat > "$evidence/git-status.md" <<EOF_GIT
# Estado Git

- Branch: $(git branch --show-current 2>/dev/null || echo INDISPONÍVEL)
- Commit: $(git rev-parse HEAD 2>/dev/null || echo INDISPONÍVEL)

## git status
EOF_GIT
capture "$evidence/git-status.md" git status --short --branch || true

cat > "$evidence/environment.md" <<'EOF_ENV'
# Pré-checagem do ambiente
EOF_ENV
capture "$evidence/environment.md" "$root_dir/scripts/check-release-environment.sh" || true

cat > "$evidence/pr-status.md" <<'EOF_PR'
# Estado dos PRs

EOF_PR
if command -v gh >/dev/null 2>&1 && gh auth status >/dev/null 2>&1; then
  capture "$evidence/pr-status.md" gh pr list -R devmnsoft/BarberSync --limit 10 || true
  pr_state="$(gh pr view 201 -R devmnsoft/BarberSync --json state --jq .state 2>/dev/null || true)"
  if [[ "$pr_state" == CLOSED || "$pr_state" == MERGED ]]; then
    printf '\nPR #201: %s\nEVIDENCE:PR_201_RESOLVED:PASS\n' "$pr_state" >> "$evidence/pr-status.md"
  elif [[ -z "$pr_state" ]] && ! gh pr view 201 -R devmnsoft/BarberSync >/dev/null 2>&1; then
    printf '\nPR #201: inexistente ou inacessível; confirmação manual necessária (não marcado como PASS).\n' >> "$evidence/pr-status.md"
  else printf '\nPR #201: %s\n' "$pr_state" >> "$evidence/pr-status.md"; fi
else
  echo 'GitHub CLI não autenticado; PR #201 permanece sem evidência de fechamento.' >> "$evidence/pr-status.md"
fi

cat > "$evidence/production-readiness-summary.md" <<'EOF_READY'
# Production readiness

## Execução
EOF_READY
capture "$evidence/production-readiness-summary.md" "$root_dir/scripts/run-production-readiness.sh" || true
{
  echo; echo '## Logs disponíveis';
  if [[ -d "$production" ]]; then
    find "$production" -maxdepth 1 -type f -printf '%f\n' | sort
  else echo 'Nenhum diretório de logs encontrado.'; fi
} >> "$evidence/production-readiness-summary.md"

frontend="$evidence/frontend-checks.md"
echo '# Checks frontend' > "$frontend"
run_frontend() {
  local label="$1" marker="$2"; shift 2
  echo -e "\n## $label\n\n\`\`\`text" >> "$frontend"
  "$@" >> "$frontend" 2>&1; local rc=$?
  echo '```' >> "$frontend"
  if [[ $rc -eq 0 ]]; then echo "EVIDENCE:${marker}:PASS" >> "$frontend"; else echo "EVIDENCE:${marker}:FAIL (exit $rc)" >> "$frontend"; fi
}
run_frontend 'node --check' FRONTEND_CHECKS bash -c 'find Web MobileApp Totem -name "*.js" -not -path "*/node_modules/*" -not -path "*/dist/*" -print0 | xargs -0 -r -n1 node --check'
run_frontend 'Mobile smoke' MOBILE_SMOKE npm test --prefix MobileApp
run_frontend 'Totem smoke' TOTEM_SMOKE npm test --prefix Totem
run_frontend 'Totem build' TOTEM_BUILD npm run build --prefix Totem

scan="$evidence/demo-fallback-scan.md"
echo '# Scan demo/fallback' > "$scan"
echo 'Resultados literais devem ser confrontados com docs/DEMO_FALLBACK_CLASSIFICATION.md; ocorrência não é aprovada automaticamente.' >> "$scan"
capture "$scan" rg -n 'DemoStore|localStorage|sessionStorage|mock|fake|fallback|TODO|NotImplementedException|throw new NotImplementedException|em breve|coming soon|href="#"|onclick=""|00000000|11111111|22222222|PublicConfigController|ConfigurationService' Backend Web MobileApp Totem --glob '!**/node_modules/**' --glob '!**/dist/**' || true

"$root_dir/scripts/summarize-release-evidence.sh" || true
echo "Evidências coletadas em $evidence"
