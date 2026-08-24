$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$Production = Join-Path $Root "artifacts/production-readiness"
$Evidence = Join-Path $Root "artifacts/release-evidence"
New-Item -ItemType Directory -Force -Path $Evidence | Out-Null
$Definitions = @(
    @{ Marker="READINESS_CONTRACTS_STATIC"; Name="readiness-contracts-static.log" },
    @{ Marker="RESTORE"; Name="dotnet-restore.log" },
    @{ Marker="BUILD_DEBUG"; Name="dotnet-build-debug.log" },
    @{ Marker="BUILD_RELEASE"; Name="dotnet-build-release.log" },
    @{ Marker="SQL_APPLY_1"; Name="sql-apply-1.log" },
    @{ Marker="SQL_APPLY_2"; Name="sql-apply-2.log" },
    @{ Marker="SCHEMA_VALIDATION"; Name="schema-validation.log" },
    @{ Marker="READINESS_SEED"; Name="readiness-seed.log" },
    @{ Marker="API_RUNTIME"; Name="api-run.log" },
    @{ Marker="HEALTH"; Name="health.log" },
    @{ Marker="PRODUCTION_SMOKE"; Name="production-smoke.log" },
    @{ Marker="AUTHENTICATED_PRODUCTION_SMOKE"; Name="authenticated-production-smoke.log" },
    @{ Marker="AUTH_SMOKE_LOGIN"; Name="authenticated-production-smoke.log" },
    @{ Marker="AUTH_SMOKE_DASHBOARD"; Name="authenticated-production-smoke.log" },
    @{ Marker="AUTH_SMOKE_MOBILE_CLIENT"; Name="authenticated-production-smoke.log" },
    @{ Marker="AUTH_SMOKE_MOBILE_PROFESSIONAL"; Name="authenticated-production-smoke.log" },
    @{ Marker="AUTH_SMOKE_NOTIFICATIONS"; Name="authenticated-production-smoke.log" },
    @{ Marker="AUTH_SMOKE_STOCK"; Name="authenticated-production-smoke.log" },
    @{ Marker="AUTH_SMOKE_CASH_REGISTER"; Name="authenticated-production-smoke.log" },
    @{ Marker="AUTH_SMOKE_SERVICE_ORDER"; Name="authenticated-production-smoke.log" },
    @{ Marker="AUTH_SMOKE_PAYMENT"; Name="authenticated-production-smoke.log" },
    @{ Marker="AUTH_SMOKE_STOCK_MOVEMENT"; Name="authenticated-production-smoke.log" },
    @{ Marker="AUTH_SMOKE_CASH_MOVEMENT"; Name="authenticated-production-smoke.log" },
    @{ Marker="AUTH_SMOKE_FINANCIAL_ENTRY"; Name="authenticated-production-smoke.log" },
    @{ Marker="AUTH_SMOKE_COMMISSION"; Name="authenticated-production-smoke.log" },
    @{ Marker="AUTH_SMOKE_POS"; Name="authenticated-production-smoke.log" },
    @{ Marker="FRONTEND_CHECKS"; Name="frontend.log" },
    @{ Marker="MOBILE_SMOKE"; Name="mobile-smoke.log" },
    @{ Marker="TOTEM_SMOKE"; Name="totem-smoke.log" },
    @{ Marker="TOTEM_BUILD"; Name="totem-build.log" }
)
$Passed=@(); $Missing=@(); $Failed=@()
foreach ($Definition in $Definitions) {
  $Marker=$Definition.Marker; $Name=$Definition.Name; $File=Join-Path $Production $Name
  if (-not (Test-Path $File)) { $Missing += "$Marker ($Name ausente)"; continue }
  $Lines=Get-Content $File
  if ($Lines -ccontains "EVIDENCE:${Marker}:PASS") { $Passed += $Marker }
  elseif ($Lines -match "EVIDENCE:${Marker}:(FAIL|ERROR|SKIPPED)") { $Failed += $Marker }
  else { $Missing += "$Marker (marker PASS ausente em $Name)" }
}
$Decision=if (($Missing.Count+$Failed.Count)-eq 0){'GO'}else{'NO-GO'}
function Section($Title,$Items) { "## $Title`n"; if ($Items.Count -eq 0) {'- Nenhum.'} else {$Items|ForEach-Object{"- ``$_``"}}; '' }
$Report=@('# GO/NO-GO','',"Status: $Decision",'')
$Report += Section 'Passed markers' $Passed; $Report += Section 'Missing markers' $Missing; $Report += Section 'Failed markers' $Failed
$Report += '## Decision',''
$Report += if($Decision-eq'GO'){'GO: todos os markers obrigatórios passaram.'}else{"NO-GO porque faltam $($Missing.Count) marker(s) e $($Failed.Count) marker(s) falharam."}
$Report += '','Ausência de arquivo ou marker explícito nunca é interpretada como sucesso.'
$Report | Set-Content -Encoding UTF8 (Join-Path $Evidence 'go-no-go.md')
Write-Host "${Decision}: $($Missing.Count) ausente(s), $($Failed.Count) falho(s)."
if($Decision-eq'GO'){exit 0}else{exit 1}
