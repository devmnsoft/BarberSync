#!/usr/bin/env bash
set -u
root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
evidence="$root_dir/artifacts/release-evidence"
production="$root_dir/artifacts/production-readiness"
mkdir -p "$evidence"
cd "$root_dir"

capture() { local file="$1"; shift; { echo '```text'; "$@"; local rc=$?; echo '```'; echo; echo "Exit code: $rc"; return "$rc"; } >> "$file" 2>&1; }

cat > "$evidence/git-status.md" <<EOF_GIT
# Estado Git

- Branch/ref: ${GITHUB_REF:-$(git branch --show-current 2>/dev/null || echo INDISPONÍVEL)}
- Commit: ${GITHUB_SHA:-$(git rev-parse HEAD 2>/dev/null || echo INDISPONÍVEL)}

## git status
EOF_GIT
capture "$evidence/git-status.md" git status --short --branch || true

echo '# Pré-checagem do ambiente' > "$evidence/environment.md"
capture "$evidence/environment.md" "$root_dir/scripts/check-release-environment.sh" || true

echo '# Validação estática de contratos de readiness' > "$evidence/readiness-contracts-static.md"
capture "$evidence/readiness-contracts-static.md" "$root_dir/scripts/validate-readiness-contracts.sh" || true

cat > "$evidence/pr-status.md" <<'EOF_PR'
# Estado dos PRs
EOF_PR
if command -v gh >/dev/null 2>&1 && gh auth status >/dev/null 2>&1; then
  capture "$evidence/pr-status.md" gh pr list -R devmnsoft/BarberSync --limit 20 || true
  capture "$evidence/pr-status.md" gh pr view 201 -R devmnsoft/BarberSync --json number,state,title,url || true
else
  echo 'GitHub CLI não autenticado; verificar e fechar manualmente o PR #201 antes do GO.' >> "$evidence/pr-status.md"
fi

{
  echo '# Markers encontrados'; echo
  if [[ -d "$production" ]]; then
    grep -R -h 'EVIDENCE:' "$production" 2>/dev/null | sort -u || true
  else
    echo 'Nenhum diretório de logs encontrado.'
  fi
} > "$evidence/markers.txt"
cp "$evidence/markers.txt" "$evidence/markers.md"

{
  echo '# Caudas dos logs críticos'; echo
  for name in dotnet-restore.log dotnet-build-debug.log dotnet-build-release.log sql-apply-1.log sql-apply-2.log schema-validation.log readiness-seed.log api-run.log health.log production-smoke.log authenticated-production-smoke.log frontend.log mobile-smoke.log totem-smoke.log totem-build.log; do
    echo "## $name"; echo '```text'
    if [[ -f "$production/$name" ]]; then tail -n 80 "$production/$name"; else echo 'LOG AUSENTE'; fi
    echo '```'; echo
  done
} > "$evidence/critical-log-tails.md"

{
  echo '# Status das ferramentas'; echo
  for tool in git docker dotnet node npm psql; do
    if command -v "$tool" >/dev/null 2>&1; then echo "- $tool: disponível"; else echo "- $tool: indisponível"; fi
  done
} > "$evidence/tooling-status.md"

{
  echo '# Production readiness'; echo
  echo 'O coletor preserva o gate já executado e não o executa novamente.'; echo
  echo '## Logs disponíveis'
  if [[ -d "$production" ]]; then find "$production" -maxdepth 1 -type f -printf '%f\n' | sort; else echo 'Nenhum diretório de logs encontrado.'; fi
} > "$evidence/production-readiness-summary.md"

if [[ ! -f "$evidence/go-no-go.md" ]]; then
  printf '# GO/NO-GO\n\nStatus: NO-GO\n\nExecute summarize-release-evidence.sh para avaliar todos os markers obrigatórios.\n' > "$evidence/go-no-go.md"
fi

echo "Evidências coletadas em $evidence"
