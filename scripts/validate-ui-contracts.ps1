$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$bash = Get-Command bash -ErrorAction SilentlyContinue
if (-not $bash) { throw 'bash is required to run the canonical UI contract validator.' }
& $bash.Source (Join-Path $PSScriptRoot 'validate-ui-contracts.sh')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
# Sprint 58 parity: ServiceExecution views must use selections, never visible technical-ID inputs.
