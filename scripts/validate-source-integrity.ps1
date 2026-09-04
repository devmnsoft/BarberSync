$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
  bash ./scripts/validate-source-integrity.sh
  if ($LASTEXITCODE -ne 0) { throw "Source integrity failed ($LASTEXITCODE)." }
  Write-Output 'EVIDENCE:SOURCE_INTEGRITY_STATIC:PASS'
} finally { Pop-Location }
# Sprint 58 parity: Atendimento 360 is covered by the shell gate for fake financial/stock outcomes and binary money types.
# Sprint 61 parity: Finance360 fake outcomes, binary money and technical-ID inputs are covered by the shell gate.
# Sprint 62 parity: Inventory360 fabricated stock/purchase/COGS, binary costs and technical IDs are covered by the shell gate.
