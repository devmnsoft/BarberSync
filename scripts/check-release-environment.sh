#!/usr/bin/env bash
set -u

ok=0 warn=0 fail=0
has() { command -v "$1" >/dev/null 2>&1; }
report() {
  local level="$1" message="$2"
  printf '%s: %s\n' "$level" "$message"
  case "$level" in OK) ok=$((ok + 1));; WARN) warn=$((warn + 1));; FAIL) fail=$((fail + 1));; esac
}
for tool in git gh docker dotnet psql node npm curl; do
  if has "$tool"; then report OK "$tool encontrado"; else report FAIL "$tool não encontrado"; fi
done

compose_ok=false
if has docker && docker compose version >/dev/null 2>&1; then
  compose_ok=true; report OK "docker compose encontrado"
else
  report FAIL "docker compose não encontrado ou indisponível"
fi

gh_auth_ok=false
if has gh && gh auth status >/dev/null 2>&1; then
  gh_auth_ok=true; report OK "gh autenticado"
elif has gh; then
  report WARN "gh encontrado, mas não autenticado"
else
  report WARN "gh auth status indisponível"
fi

token_ok=false
if [[ -n "${GH_TOKEN:-}" || -n "${GITHUB_TOKEN:-}" ]]; then
  token_ok=true; report OK "GH_TOKEN/GITHUB_TOKEN disponível (valor oculto)"
else
  report WARN "GH_TOKEN/GITHUB_TOKEN não definido"
fi

database_ok=false
if [[ -n "${DATABASE_URL:-}" ]]; then
  database_ok=true; report OK "DATABASE_URL disponível (valor oculto)"
else
  report FAIL "DATABASE_URL não definido"
fi

docker_gate=false
if has docker && [[ "$compose_ok" == true ]] && has node && has npm && has curl; then docker_gate=true; fi
host_gate=false
if has dotnet && has psql && has node && has npm && has curl && [[ "$database_ok" == true ]]; then host_gate=true; fi
actions_gate=false
if has gh && { [[ "$gh_auth_ok" == true ]] || [[ "$token_ok" == true ]]; }; then actions_gate=true; fi

printf '\nResumo: %d OK, %d WARN, %d FAIL\n' "$ok" "$warn" "$fail"
printf 'Rota Docker local: %s\n' "$([[ "$docker_gate" == true ]] && echo DISPONÍVEL || echo INDISPONÍVEL)"
printf 'Rota local sem Docker: %s\n' "$([[ "$host_gate" == true ]] && echo DISPONÍVEL || echo INDISPONÍVEL)"
printf 'Rota GitHub Actions: %s\n' "$([[ "$actions_gate" == true ]] && echo DISPONÍVEL || echo INDISPONÍVEL)"
if [[ "$docker_gate" == true || "$host_gate" == true || "$actions_gate" == true ]]; then
  echo 'OK: ao menos uma rota de gate real pode ser executada.'
  exit 0
fi
echo 'FAIL: nenhuma rota de gate real pode ser executada neste ambiente.'
exit 1
