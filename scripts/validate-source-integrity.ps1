$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
  bash ./scripts/validate-source-integrity.sh
  if ($LASTEXITCODE -ne 0) { throw "Source integrity failed ($LASTEXITCODE)." }
  Write-Output 'EVIDENCE:SOURCE_INTEGRITY_STATIC:PASS'
} finally { Pop-Location }
