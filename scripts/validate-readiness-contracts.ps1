$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$script:Failures = 0

function Test-Contract([string]$Description, [scriptblock]$Test) {
    if (& $Test) { Write-Host "OK: $Description" } else { Write-Error "FAIL: $Description" -ErrorAction Continue; $script:Failures++ }
}
function Has([string]$Path, [string]$Pattern) { return [bool](Select-String -Path $Path -Pattern $Pattern -Quiet) }

$CashDto = Join-Path $Root "Backend/Application/BarberSync.Application/Operations/CashRegisters.cs"
$CashRepo = Join-Path $Root "Backend/Infrastructure/BarberSync.Infrastructure/Repositories/PostgresCashRegisterRepository.cs"
$Stock = Join-Path $Root "Backend/Presentation/BarberSync.Api/Controllers/StockController.cs"
$Schema = Join-Path $Root "ScriptsSQL/script_completo.sql"
$Seed = Join-Path $Root "ScriptsSQL/production_readiness_seed.sql"
$Markers = @("AUTH_SMOKE_SERVICE_ORDER","AUTH_SMOKE_PAYMENT","AUTH_SMOKE_STOCK_MOVEMENT","AUTH_SMOKE_CASH_MOVEMENT","AUTH_SMOKE_FINANCIAL_ENTRY","AUTH_SMOKE_COMMISSION","AUTH_SMOKE_POS")

Test-Contract "CashMovementResponse exposes nullable Guid PaymentId" { Has $CashDto 'CashMovementResponse\([^)]*Guid\? PaymentId' }
Test-Contract "cash register repository selects payment_id" { Has $CashRepo 'SELECT .*payment_id FROM barber\.cash_movements' }
Test-Contract "cash_movements schema has nullable payment_id" { Has $Schema 'cash_movements \([^;]*payment_id uuid REFERENCES barber\.payments\(id\)' }
Test-Contract "StockController exposes authenticated GET movements route" { Has $Stock '\[HttpGet\("movements"\), RequirePermission\("Stock\.View"\)\]' }
foreach ($Marker in $Markers) {
    Test-Contract "authenticated smoke shell emits $Marker" { Has (Join-Path $Root "scripts/authenticated-production-smoke.sh") "EVIDENCE:${Marker}:PASS" }
    Test-Contract "authenticated smoke PowerShell emits $Marker" { Has (Join-Path $Root "scripts/authenticated-production-smoke.ps1") "EVIDENCE:${Marker}:PASS" }
    Test-Contract "shell summarizer requires $Marker" { Has (Join-Path $Root "scripts/summarize-release-evidence.sh") "\b${Marker}\b" }
    Test-Contract "PowerShell summarizer requires $Marker" { Has (Join-Path $Root "scripts/summarize-release-evidence.ps1") "Marker=`"${Marker}`"" }
}
Test-Contract "readiness seed is guarded by ProductionReadiness setting" { Has $Seed "current_setting\('barbersync\.environment'.*ProductionReadiness" }
Test-Contract "readiness seed has no destructive TRUNCATE or DROP statement" { -not (Has $Seed '(?im)^\s*(TRUNCATE|DROP)\s') }
$Forbidden = "SKIPPED_CONTRACT_" + "NOT_FOUND"
foreach ($Smoke in @("authenticated-production-smoke.sh","authenticated-production-smoke.ps1")) {
    Test-Contract "$Smoke has no obsolete POS skip path" { -not (Has (Join-Path $Root "scripts/$Smoke") ([regex]::Escape($Forbidden))) }
}
if ($script:Failures -gt 0) { Write-Error "FAIL: $($script:Failures) readiness contract(s) failed."; exit 1 }
Write-Host "OK: all readiness contracts passed static validation."
