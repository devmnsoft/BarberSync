#!/usr/bin/env bash
set -euo pipefail

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
output_arg="${1:-artifacts/release-evidence-package}"
[[ "$output_arg" = /* ]] && output_dir="$output_arg" || output_dir="$root_dir/$output_arg"
stamp="$(date -u +%Y%m%d-%H%M%S)"
stage="$(mktemp -d "${TMPDIR:-/tmp}/barbersync-evidence.XXXXXX")"
trap 'rm -rf "$stage"' EXIT
mkdir -p "$output_dir"

is_excluded() {
  [[ "$1" =~ (^|/)(node_modules|bin|obj|dist|secrets?)(/|$) ]] ||
    [[ "$1" =~ (^|/)\.env($|\.) ]] || [[ "$1" =~ appsettings\.Production\.json$ ]]
}
sanitize() {
  perl -pe 's/(DATABASE_URL|ConnectionStrings|Password|Token|Secret|Jwt|ApiKey)(\s*[=:]\s*)("[^"]*"|'"'"'[^'"'"']*'"'"'|[^\s"'"'"';,]+)/$1.$2."***REDACTED***"/ige' "$1" > "$2"
}

for tree in artifacts/production-readiness artifacts/release-evidence; do
  [[ -d "$root_dir/$tree" ]] || continue
  while IFS= read -r -d '' source; do
    relative="${source#"$root_dir/"}"
    is_excluded "$relative" && continue
    if ! grep -Iq . "$source"; then
      printf 'AVISO: arquivo binário ignorado: %s\n' "$relative" >&2
      continue
    fi
    target="$stage/$relative"; mkdir -p "$(dirname "$target")"
    if grep -Eiq '(DATABASE_URL|ConnectionStrings|Password|Token|Secret|Jwt|ApiKey)[[:space:]]*[=:]' "$source"; then
      printf 'AVISO: padrão sensível mascarado em %s\n' "$relative" >&2
    fi
    sanitize "$source" "$target"
  done < <(find "$root_dir/$tree" -type f -print0)
done

cat > "$stage/README_DO_PACOTE.txt" <<EOF
BARBERSYNC - PACOTE DE EVIDÊNCIAS DE RELEASE
Gerado em: $(date -u +%Y-%m-%dT%H:%M:%SZ)
Leia artifacts/release-evidence/go-no-go.md primeiro.
Arquivos potencialmente secretos foram excluídos e padrões sensíveis foram mascarados.
Ausência de arquivo ou marker nunca é interpretada como sucesso.
EOF

if command -v zip >/dev/null 2>&1; then
  archive="$output_dir/barbersync-release-evidence-$stamp.zip"
  (cd "$stage" && zip -qr "$archive" .)
else
  archive="$output_dir/barbersync-release-evidence-$stamp.tar.gz"
  tar -C "$stage" -czf "$archive" .
fi
printf 'Pacote criado: %s\n' "$archive"
