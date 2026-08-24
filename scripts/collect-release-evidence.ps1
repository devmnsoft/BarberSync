$ErrorActionPreference = "Continue"
Set-StrictMode -Version Latest
$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$Evidence = Join-Path $Root "artifacts/release-evidence"
$Production = Join-Path $Root "artifacts/production-readiness"
New-Item -ItemType Directory -Force -Path $Evidence | Out-Null
Push-Location $Root
function Invoke-Captured([string]$File, [scriptblock]$Command) {
    "``````text" | Add-Content $File; & $Command 2>&1 | Out-File -Append -Encoding UTF8 $File
    $Code = $LASTEXITCODE; "```````n`nExit code: $Code" | Add-Content $File; return $Code
}
try {
    $Static = Join-Path $Evidence "readiness-contracts-static.md"; "# Validação estática de contratos de readiness" | Set-Content $Static
    Invoke-Captured $Static { & (Join-Path $PSScriptRoot "validate-readiness-contracts.ps1") } | Out-Null
    $Branch = git branch --show-current; $Commit = git rev-parse HEAD
    "# Estado Git`n`n- Branch: $Branch`n- Commit: $Commit`n`n## git status" | Set-Content (Join-Path $Evidence "git-status.md")
    Invoke-Captured (Join-Path $Evidence "git-status.md") { git status --short --branch } | Out-Null
    "# Pré-checagem do ambiente" | Set-Content (Join-Path $Evidence "environment.md")
    Invoke-Captured (Join-Path $Evidence "environment.md") { & (Join-Path $PSScriptRoot "check-release-environment.ps1") } | Out-Null

    $PrFile = Join-Path $Evidence "pr-status.md"; "# Estado dos PRs`n" | Set-Content $PrFile
    $Gh = Get-Command gh -ErrorAction SilentlyContinue; $Authenticated = $false
    if ($Gh) { gh auth status *> $null; $Authenticated = ($LASTEXITCODE -eq 0) }
    if ($Authenticated) {
        Invoke-Captured $PrFile { gh pr list -R devmnsoft/BarberSync --limit 10 } | Out-Null
        $State = gh pr view 201 -R devmnsoft/BarberSync --json state --jq .state 2>$null
        if ($State -in @("CLOSED", "MERGED")) { "`nPR #201: $State`nEVIDENCE:PR_201_RESOLVED:PASS" | Add-Content $PrFile }
        else { "`nPR #201: $(if ($State) {$State} else {'sem confirmação; validação manual necessária'})" | Add-Content $PrFile }
    } else { "GitHub CLI não autenticado; PR #201 permanece sem evidência de fechamento." | Add-Content $PrFile }

    $Ready = Join-Path $Evidence "production-readiness-summary.md"; "# Production readiness`n`n## Execução" | Set-Content $Ready
    Invoke-Captured $Ready { & (Join-Path $PSScriptRoot "run-production-readiness.ps1") } | Out-Null
    "`n## Logs disponíveis" | Add-Content $Ready
    if (Test-Path $Production) { Get-ChildItem $Production -File | Sort-Object Name | Select-Object -ExpandProperty Name | Add-Content $Ready } else { "Nenhum diretório de logs encontrado." | Add-Content $Ready }

    $Frontend = Join-Path $Evidence "frontend-checks.md"; "# Checks frontend" | Set-Content $Frontend
    function Run-Frontend([string]$Label, [string]$Marker, [scriptblock]$Command) {
        "`n## $Label`n`n``````text" | Add-Content $Frontend; & $Command 2>&1 | Out-File -Append -Encoding UTF8 $Frontend
        $Code = $LASTEXITCODE; "``````" | Add-Content $Frontend
        if ($Code -eq 0) { "EVIDENCE:${Marker}:PASS" | Add-Content $Frontend } else { "EVIDENCE:${Marker}:FAIL (exit $Code)" | Add-Content $Frontend }
    }
    Run-Frontend "node --check" "FRONTEND_CHECKS" { $NodeExit = 0; Get-ChildItem Web,MobileApp,Totem -Recurse -Filter *.js | Where-Object { $_.FullName -notmatch '[\\/](node_modules|dist)[\\/]' } | ForEach-Object { node --check $_.FullName; if ($LASTEXITCODE -ne 0) { $NodeExit = $LASTEXITCODE } }; $global:LASTEXITCODE = $NodeExit }
    Run-Frontend "Mobile smoke" "MOBILE_SMOKE" { npm test --prefix MobileApp }
    Run-Frontend "Totem smoke" "TOTEM_SMOKE" { npm test --prefix Totem }
    Run-Frontend "Totem build" "TOTEM_BUILD" { npm run build --prefix Totem }

    $Scan = Join-Path $Evidence "demo-fallback-scan.md"
    "# Scan demo/fallback`n`nResultados literais devem ser confrontados com docs/DEMO_FALLBACK_CLASSIFICATION.md; ocorrência não é aprovada automaticamente." | Set-Content $Scan
    Invoke-Captured $Scan { rg -n 'DemoStore|localStorage|sessionStorage|mock|fake|fallback|TODO|NotImplementedException|throw new NotImplementedException|em breve|coming soon|href="#"|onclick=""|00000000|11111111|22222222|PublicConfigController|ConfigurationService' Backend Web MobileApp Totem --glob '!**/node_modules/**' --glob '!**/dist/**' } | Out-Null
    & (Join-Path $PSScriptRoot "summarize-release-evidence.ps1")
    Write-Host "Evidências coletadas em $Evidence"
} finally { Pop-Location }
