$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$Production = Join-Path $Root "artifacts/production-readiness"
$Evidence = Join-Path $Root "artifacts/release-evidence"
New-Item -ItemType Directory -Force -Path $Evidence | Out-Null
$Criteria = @(
    @{ Label="contratos estáticos de readiness"; Marker="READINESS_CONTRACTS_STATIC"; File=(Join-Path $Production "readiness-contracts-static.log") },
    @{ Label="dotnet restore"; Marker="DOTNET_RESTORE"; File=(Join-Path $Production "dotnet-restore.log") },
    @{ Label="build Debug"; Marker="BUILD_DEBUG"; File=(Join-Path $Production "dotnet-build-debug.log") },
    @{ Label="build Release"; Marker="BUILD_RELEASE"; File=(Join-Path $Production "dotnet-build-release.log") },
    @{ Label="SQL primeira aplicação"; Marker="SQL_APPLY_1"; File=(Join-Path $Production "sql-apply-1.log") },
    @{ Label="SQL segunda aplicação"; Marker="SQL_APPLY_2"; File=(Join-Path $Production "sql-apply-2.log") },
    @{ Label="schema validation"; Marker="SCHEMA_VALIDATION"; File=(Join-Path $Production "sql-apply-2.log") },
    @{ Label="API runtime"; Marker="API_RUNTIME"; File=(Join-Path $Production "api-run.log") },
    @{ Label="/health"; Marker="HEALTH"; File=(Join-Path $Production "health.log") },
    @{ Label="production-smoke"; Marker="PRODUCTION_SMOKE"; File=(Join-Path $Production "production-smoke.log") },
    @{ Label="node --check"; Marker="FRONTEND_CHECKS"; File=(Join-Path $Evidence "frontend-checks.md") },
    @{ Label="Mobile smoke"; Marker="MOBILE_SMOKE"; File=(Join-Path $Evidence "frontend-checks.md") },
    @{ Label="Totem smoke"; Marker="TOTEM_SMOKE"; File=(Join-Path $Evidence "frontend-checks.md") },
    @{ Label="Totem build"; Marker="TOTEM_BUILD"; File=(Join-Path $Evidence "frontend-checks.md") },
    @{ Label="PR #201 fechado ou inexistente"; Marker="PR_201_RESOLVED"; File=(Join-Path $Evidence "pr-status.md") },
    @{ Label="readiness seed"; Marker="READINESS_SEED"; File=(Join-Path $Production "readiness-seed.log") },
    @{ Label="authenticated smoke"; Marker="AUTHENTICATED_PRODUCTION_SMOKE"; File=(Join-Path $Production "authenticated-production-smoke.log") },
    @{ Label="auth login"; Marker="AUTH_SMOKE_LOGIN"; File=(Join-Path $Production "authenticated-production-smoke.log") },
    @{ Label="auth dashboard"; Marker="AUTH_SMOKE_DASHBOARD"; File=(Join-Path $Production "authenticated-production-smoke.log") },
    @{ Label="mobile client"; Marker="AUTH_SMOKE_MOBILE_CLIENT"; File=(Join-Path $Production "authenticated-production-smoke.log") },
    @{ Label="mobile professional"; Marker="AUTH_SMOKE_MOBILE_PROFESSIONAL"; File=(Join-Path $Production "authenticated-production-smoke.log") },
    @{ Label="notifications"; Marker="AUTH_SMOKE_NOTIFICATIONS"; File=(Join-Path $Production "authenticated-production-smoke.log") },
    @{ Label="stock"; Marker="AUTH_SMOKE_STOCK"; File=(Join-Path $Production "authenticated-production-smoke.log") },
    @{ Label="cash register"; Marker="AUTH_SMOKE_CASH_REGISTER"; File=(Join-Path $Production "authenticated-production-smoke.log") },
    @{ Label="service order"; Marker="AUTH_SMOKE_SERVICE_ORDER"; File=(Join-Path $Production "authenticated-production-smoke.log") },
    @{ Label="payment"; Marker="AUTH_SMOKE_PAYMENT"; File=(Join-Path $Production "authenticated-production-smoke.log") },
    @{ Label="stock movement"; Marker="AUTH_SMOKE_STOCK_MOVEMENT"; File=(Join-Path $Production "authenticated-production-smoke.log") },
    @{ Label="cash movement"; Marker="AUTH_SMOKE_CASH_MOVEMENT"; File=(Join-Path $Production "authenticated-production-smoke.log") },
    @{ Label="financial entry"; Marker="AUTH_SMOKE_FINANCIAL_ENTRY"; File=(Join-Path $Production "authenticated-production-smoke.log") },
    @{ Label="commission"; Marker="AUTH_SMOKE_COMMISSION"; File=(Join-Path $Production "authenticated-production-smoke.log") },
    @{ Label="POS"; Marker="AUTH_SMOKE_POS"; File=(Join-Path $Production "authenticated-production-smoke.log") }
)
$Missing = 0; $Rows = @()
foreach ($Criterion in $Criteria) {
    $Expected = "EVIDENCE:$($Criterion.Marker):PASS"
    $Passed = (Test-Path $Criterion.File) -and [bool](Get-Content $Criterion.File | Where-Object { $_ -ceq $Expected })
    if (-not $Passed) { $Missing++ }
    $Rows += "| $($Criterion.Label) | $(if ($Passed) {'PASS'} else {'AUSENTE/REPROVADO'}) |"
}
$Decision = if ($Missing -eq 0) { "GO" } else { "NO-GO" }
$Reason = if ($Missing -eq 0) { "Todas as evidências obrigatórias foram encontradas." } else { "$Missing evidência(s) obrigatória(s) ausente(s) ou reprovada(s)." }
@"
# Resumo GO/NO-GO

- Decisão: **$Decision**
- Motivo: $Reason
- Gerado em UTC: $((Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"))

| Critério | Resultado |
| --- | --- |
$($Rows -join "`n")

Ausência de arquivo ou marcador explícito nunca é interpretada como sucesso. Mensagens como ``Docker is required`` não satisfazem nenhum critério.
"@ | Set-Content -Encoding UTF8 (Join-Path $Evidence "go-no-go.md")
Write-Host "${Decision}: $Reason"
if ($Decision -eq "GO") { exit 0 } else { exit 1 }
