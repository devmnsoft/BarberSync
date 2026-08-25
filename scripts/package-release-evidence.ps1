param([string]$OutputDir = "artifacts/release-evidence-package")
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$Destination = if ([IO.Path]::IsPathRooted($OutputDir)) { $OutputDir } else { Join-Path $Root $OutputDir }
$Stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$Stage = Join-Path ([IO.Path]::GetTempPath()) "barbersync-evidence-$Stamp-$PID"
$Archive = Join-Path $Destination "barbersync-release-evidence-$Stamp.zip"
$Sensitive = '(?i)(DATABASE_URL|ConnectionStrings|Password|Token|Secret|Jwt|ApiKey)(\s*[=:]\s*)([^\s"'';,]+|"[^"]*"|''[^'']*'')'
$Excluded = '(?i)(^|[\\/])(node_modules|bin|obj|dist|secrets?)([\\/]|$)|(^|[\\/])\.env($|\.)|appsettings\.Production\.json$'
New-Item -ItemType Directory -Force -Path $Destination,$Stage | Out-Null
try {
  foreach ($relative in @('artifacts/production-readiness','artifacts/release-evidence')) {
    $source = Join-Path $Root $relative
    if (-not (Test-Path $source)) { continue }
    Get-ChildItem $source -Recurse -File | Where-Object { $_.FullName -notmatch $Excluded } | ForEach-Object {
      $rel = $_.FullName.Substring($Root.Length).TrimStart([char[]]'\/')
      $target = Join-Path $Stage $rel
      New-Item -ItemType Directory -Force -Path (Split-Path $target) | Out-Null
      $bytes = [IO.File]::ReadAllBytes($_.FullName)
      if ($bytes -contains 0) { return }
      $text = [Text.Encoding]::UTF8.GetString($bytes)
      if ($text -match $Sensitive) { Write-Warning "Padrão sensível mascarado em $rel" }
      [IO.File]::WriteAllText($target, ($text -replace $Sensitive, '$1$2***REDACTED***'), [Text.UTF8Encoding]::new($false))
    }
  }
  @"
BARBERSYNC - PACOTE DE EVIDÊNCIAS DE RELEASE
Gerado em: $(Get-Date -Format o)
Leia artifacts/release-evidence/go-no-go.md primeiro.
Arquivos potencialmente secretos foram excluídos e padrões sensíveis foram mascarados.
O pacote não substitui os markers obrigatórios nem transforma ausência de evidência em sucesso.
"@ | Set-Content -Encoding UTF8 (Join-Path $Stage 'README_DO_PACOTE.txt')
  if (Test-Path $Archive) { Remove-Item $Archive -Force }
  Compress-Archive -Path (Join-Path $Stage '*') -DestinationPath $Archive -CompressionLevel Optimal
  Write-Host "Pacote criado: $Archive"
} finally { Remove-Item $Stage -Recurse -Force -ErrorAction SilentlyContinue }
