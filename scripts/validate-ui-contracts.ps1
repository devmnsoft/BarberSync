$ErrorActionPreference = 'Stop'
$bash = Get-Command bash -ErrorAction SilentlyContinue
if (-not $bash) { throw 'bash is required to run the canonical UI contract validator.' }
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
  & $bash.Source ./scripts/validate-ui-contracts.sh
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
} finally { Pop-Location }
# Sprint 58 parity: ServiceExecution views must use selections, never visible technical-ID inputs.
# Sprint 61 parity: Finance360 visible forms are checked for technical identifiers by the canonical shell validator.
# Sprint 62 parity: Inventory360 forms are checked for visible technical identifiers by the canonical shell validator.
