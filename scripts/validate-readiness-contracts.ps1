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
$EnterpriseData = Join-Path $Root "Backend/Presentation/BarberSync.Api/Services/Enterprise/EnterpriseDataService.cs"
$ServiceOrderRepo = Join-Path $Root "Backend/Infrastructure/BarberSync.Infrastructure/Repositories/PostgresServiceOrderRepository.cs"
$Schema = Join-Path $Root "ScriptsSQL/script_completo.sql"
$Seed = Join-Path $Root "ScriptsSQL/production_readiness_seed.sql"
$Markers = @("AUTH_SMOKE_SERVICE_ORDER","AUTH_SMOKE_PAYMENT","AUTH_SMOKE_STOCK_MOVEMENT","AUTH_SMOKE_CASH_MOVEMENT","AUTH_SMOKE_FINANCIAL_ENTRY","AUTH_SMOKE_COMMISSION","AUTH_SMOKE_POS")

Test-Contract "CashMovementResponse exposes nullable Guid PaymentId" { Has $CashDto 'CashMovementResponse\([^)]*Guid\? PaymentId' }
Test-Contract "cash register repository selects payment_id" { Has $CashRepo 'SELECT .*payment_id FROM barber\.cash_movements' }
Test-Contract "cash movement read is tenant and branch scoped" { Has $CashRepo 'cash_register_id=@id AND tenant_id=@tenant AND branch_id=@branch' }
Test-Contract "cash_movements schema has nullable payment_id" { Has $Schema 'cash_movements \([^;]*payment_id uuid REFERENCES barber\.payments\(id\)' }
Test-Contract "StockController requires authentication" { Has $Stock '\[ApiController, Authorize\]' }
Test-Contract "StockController exposes authenticated GET movements route" { Has $Stock '\[HttpGet\("movements"\), RequirePermission\("Stock\.View"\)\]' }
Test-Contract "stock movement projection exposes POS correlation fields" { Has $EnterpriseData "'productId'.*'serviceOrderId'.*'quantity'.*'balanceAfter'" }
Test-Contract "enterprise list reads are tenant and branch scoped" { Has $EnterpriseData 'where tenant_id = @tenantScope and branch_id = @branchScope' }
Test-Contract "POS writes payment-correlated cash movements" { Has $ServiceOrderRepo 'cash_movements\([^)]*payment_id[^)]*\).*@payment' }
Test-Contract "POS financial effects have a retry-safe unique index" { Has $Schema "ux_financial_entries_pos_payment.*payload->>'paymentId'" }
Test-Contract "POS commissions have a retry-safe unique index" { Has $Schema 'ux_commissions_payment_item.*payment_id,service_order_item_id' }
foreach ($Marker in $Markers) {
    Test-Contract "authenticated smoke shell emits $Marker" { Has (Join-Path $Root "scripts/authenticated-production-smoke.sh") "EVIDENCE:${Marker}:PASS" }
    Test-Contract "authenticated smoke PowerShell emits $Marker" { Has (Join-Path $Root "scripts/authenticated-production-smoke.ps1") "EVIDENCE:${Marker}:PASS" }
    Test-Contract "shell summarizer requires $Marker" { Has (Join-Path $Root "scripts/summarize-release-evidence.sh") "\b${Marker}\b" }
    Test-Contract "PowerShell summarizer requires $Marker" { Has (Join-Path $Root "scripts/summarize-release-evidence.ps1") "Marker=`"${Marker}`"" }
}
Test-Contract "readiness seed is guarded by ProductionReadiness setting" { Has $Seed "current_setting\('barbersync\.environment'.*ProductionReadiness" }
Test-Contract "readiness seed has no destructive TRUNCATE or DROP statement" { -not (Has $Seed '(?im)^\s*(TRUNCATE|DROP)\s') }
$Forbidden = "SKIPPED_CONTRACT_" + "NOT_FOUND"
foreach ($ScriptFile in Get-ChildItem (Join-Path $Root "scripts") -File | Where-Object { $_.Extension -in ".sh", ".ps1" -and $_.BaseName -ne "validate-readiness-contracts" }) {
    Test-Contract "$($ScriptFile.Name) has no obsolete POS skip path" { -not (Has $ScriptFile.FullName ([regex]::Escape($Forbidden))) }
}
if ($script:Failures -gt 0) { Write-Error "FAIL: $($script:Failures) readiness contract(s) failed."; exit 1 }
Write-Host "OK: all readiness contracts passed static validation."
