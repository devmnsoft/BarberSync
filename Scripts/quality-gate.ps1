param(
    [switch]$SkipDockerBuild,
    [switch]$SkipDotnetBuild,
    [switch]$SkipDotnetTest,
    [int]$WarmupSeconds = 20
)

$ErrorActionPreference = 'Stop'
$results = New-Object System.Collections.Generic.List[object]
$failures = New-Object System.Collections.Generic.List[string]
$warnings = New-Object System.Collections.Generic.List[string]
$reportPath = Join-Path (Get-Location) 'Docs/quality-gate-last-run.md'

function Add-Result {
    param([string]$Name, [string]$Target, [string]$Status, [string]$Detail = '')
    $results.Add([pscustomobject]@{ Name = $Name; Target = $Target; Status = $Status; Detail = $Detail }) | Out-Null
    $icon = if ($Status -eq 'OK') { '✅' } elseif ($Status -eq 'WARN') { '⚠️' } else { '❌' }
    $color = if ($Status -eq 'OK') { 'Green' } elseif ($Status -eq 'WARN') { 'Yellow' } else { 'Red' }
    Write-Host "$icon $Name -> $Target $Detail" -ForegroundColor $color
    if ($Status -eq 'FAIL') { $failures.Add("$Name -> $Target $Detail") | Out-Null }
    if ($Status -eq 'WARN') { $warnings.Add("$Name -> $Target $Detail") | Out-Null }
}

function Save-QualityGateReport {
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add('# BarberSync Quality Gate - Última execução') | Out-Null
    $lines.Add('') | Out-Null
    $lines.Add("- Executado em UTC: $((Get-Date).ToUniversalTime().ToString('yyyy-MM-dd HH:mm:ss'))") | Out-Null
    $lines.Add("- Resultado final: $(if ($failures.Count -eq 0) { 'APROVADO' } else { 'REPROVADO' })") | Out-Null
    $lines.Add("- Passos executados: $($results.Count)") | Out-Null
    $lines.Add("- Falhas críticas: $($failures.Count)") | Out-Null
    $lines.Add("- Alertas: $($warnings.Count)") | Out-Null
    $lines.Add('') | Out-Null
    $lines.Add('| Validação | Alvo | Status | Detalhe |') | Out-Null
    $lines.Add('|---|---|---|---|') | Out-Null
    foreach ($result in $results) {
        $detail = (($result.Detail -replace '\r?\n', '<br>') -replace '\|', '\|')
        $lines.Add("| $($result.Name) | $($result.Target) | $($result.Status) | $detail |") | Out-Null
    }
    $lines.Add('') | Out-Null
    $lines.Add('## Falhas encontradas') | Out-Null
    if ($failures.Count -eq 0) { $lines.Add('- Nenhuma falha crítica encontrada.') | Out-Null } else { foreach ($failure in $failures) { $lines.Add("- $failure") | Out-Null } }
    $lines.Add('') | Out-Null
    $lines.Add('## Como corrigir') | Out-Null
    $lines.Add('- Falhas em endpoints/proxies: confira se `docker compose up -d` subiu API, AdminWeb, PublicWeb e KioskWeb e valide os fallbacks MVC.') | Out-Null
    $lines.Add('- Falhas em assets: confira publicação de `wwwroot` e caminhos citados neste relatório.') | Out-Null
    $lines.Add('- Falhas de browser URL: remova chamadas diretas a hosts internos Docker dos arquivos web expostos ao navegador.') | Out-Null
    $lines.Add('- Falhas em logs: revise as linhas listadas antes de uma demonstração comercial.') | Out-Null
    New-Item -ItemType Directory -Force -Path (Split-Path $reportPath) | Out-Null
    Set-Content -Path $reportPath -Value $lines -Encoding UTF8
}

function Invoke-Step {
    param([string]$Name, [scriptblock]$Block)
    Write-Host "`n=== $Name ===" -ForegroundColor Cyan
    try {
        & $Block
        Add-Result $Name '-' 'OK'
    } catch {
        Add-Result $Name '-' 'FAIL' $_.Exception.Message
    }
}

function Test-Url200 {
    param([string]$Name, [string]$Url)
    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 30 -MaximumRedirection 3
        if ([int]$response.StatusCode -eq 200) {
            Add-Result $Name $Url 'OK' "HTTP $($response.StatusCode)"
        } else {
            Add-Result $Name $Url 'FAIL' "HTTP $($response.StatusCode)"
        }
    } catch {
        Add-Result $Name $Url 'FAIL' $_.Exception.Message
    }
}


function Test-HttpPost200 {
    param([string]$Name, [string]$Url, [string]$Json = '{}')
    try {
        $response = Invoke-WebRequest -Uri $Url -Method Post -Body $Json -ContentType 'application/json' -UseBasicParsing -TimeoutSec 30 -MaximumRedirection 3
        if ([int]$response.StatusCode -eq 200) {
            Add-Result $Name $Url 'OK' "HTTP $($response.StatusCode)"
        } else {
            Add-Result $Name $Url 'FAIL' "HTTP $($response.StatusCode)"
        }
    } catch {
        Add-Result $Name $Url 'FAIL' $_.Exception.Message
    }
}

function Assert-NoForbiddenBrowserUrl {
    $patterns = @('http://api:8080','api:8080','localhost:8083/api','8083/api')
    $frontFiles = Get-ChildItem -Path Web -Recurse -File -Include *.js,*.cshtml,*.html |
        Where-Object { $_.FullName -notmatch 'bin|obj|node_modules|appsettings' }
    $matches = @()
    foreach ($file in $frontFiles) {
        foreach ($pattern in $patterns) {
            $hit = Select-String -Path $file.FullName -Pattern ([regex]::Escape($pattern)) -SimpleMatch:$false -ErrorAction SilentlyContinue
            if ($hit) { $matches += $hit }
        }
    }
    if ($matches.Count -gt 0) {
        $detail = ($matches | Select-Object -First 20 | ForEach-Object { "$($_.Path):$($_.LineNumber) $($_.Line.Trim())" }) -join "`n"
        Add-Result 'Busca URL proibida no front' 'Web/**/*.{js,cshtml,html}' 'FAIL' "`n$detail"
    } else {
        Add-Result 'Busca URL proibida no front' 'Web/**/*.{js,cshtml,html}' 'OK' 'sem chamadas diretas para host interno Docker'
    }
}

function Assert-CleanLogs {
    $pattern = 'ERR|ERROR|Exception|fail|Failed|FATAL|Unhandled|500|404|409|ERR_NAME_NOT_RESOLVED'
    try {
        $logs = docker compose logs --tail=300 2>&1 | Select-String -Pattern $pattern
        $filtered = $logs | Where-Object { $_.Line -notmatch 'Usando fallback demo|fallback demo|Health checks are disabled' }
        if ($filtered) {
            $detail = ($filtered | Select-Object -First 40 | ForEach-Object { $_.Line }) -join "`n"
            Add-Result 'Logs críticos Docker' 'docker compose logs --tail=300' 'FAIL' "`n$detail"
        } else {
            Add-Result 'Logs críticos Docker' 'docker compose logs --tail=300' 'OK' 'sem padrões críticos bloqueantes'
        }
    } catch {
        Add-Result 'Logs críticos Docker' 'docker compose logs --tail=300' 'FAIL' $_.Exception.Message
    }
}

if (-not $SkipDotnetBuild) { Invoke-Step 'dotnet build' { dotnet build } }
if (-not $SkipDotnetTest) { Invoke-Step 'dotnet test' { dotnet test } }
if (-not $SkipDockerBuild) { Invoke-Step 'docker compose build' { docker compose build } }
Invoke-Step 'docker compose up -d' { docker compose up -d }
Invoke-Step 'docker compose ps' { docker compose ps }

Write-Host "`nAguardando $WarmupSeconds segundos para aquecimento dos containers..." -ForegroundColor Yellow
Start-Sleep -Seconds $WarmupSeconds

$endpoints = @(
    @{Name='API Swagger UI'; Url='http://localhost:8080/swagger'},
    @{Name='API Swagger JSON'; Url='http://localhost:8080/swagger/v1/swagger.json'},
    @{Name='API Services'; Url='http://localhost:8080/api/services'},
    @{Name='API Professionals'; Url='http://localhost:8080/api/professionals'},
    @{Name='API Kiosk Services'; Url='http://localhost:8080/api/kiosk/services?deviceCode=KIOSK-DEMO-001'},
    @{Name='API Kiosk Professionals'; Url='http://localhost:8080/api/kiosk/professionals?serviceId=demo&deviceCode=KIOSK-DEMO-001'},
    @{Name='AdminApi Dashboard'; Url='http://localhost:8081/AdminApi/dashboard'},
    @{Name='AdminApi Clients'; Url='http://localhost:8081/AdminApi/clients'},
    @{Name='AdminApi Professionals'; Url='http://localhost:8081/AdminApi/professionals'},
    @{Name='AdminApi Services'; Url='http://localhost:8081/AdminApi/services'},
    @{Name='AdminApi Appointments'; Url='http://localhost:8081/AdminApi/appointments'},
    @{Name='AdminApi Service Orders'; Url='http://localhost:8081/AdminApi/service-orders'},
    @{Name='AdminApi Products'; Url='http://localhost:8081/AdminApi/products'},
    @{Name='AdminApi Stock Critical'; Url='http://localhost:8081/AdminApi/stock-critical'},
    @{Name='AdminApi Campaigns'; Url='http://localhost:8081/AdminApi/campaigns'},
    @{Name='AdminApi Coupons'; Url='http://localhost:8081/AdminApi/coupons'},
    @{Name='AdminApi Reviews'; Url='http://localhost:8081/AdminApi/reviews'},
    @{Name='AdminApi Loyalty'; Url='http://localhost:8081/AdminApi/loyalty'},
    @{Name='AdminApi Copilot Suggestions'; Url='http://localhost:8081/AdminApi/copilot-suggestions'},
    @{Name='PublicApi Services'; Url='http://localhost:8082/PublicApi/services'},
    @{Name='PublicApi Professionals'; Url='http://localhost:8082/PublicApi/professionals'},
    @{Name='KioskApi Services'; Url='http://localhost:8083/KioskApi/services?deviceCode=KIOSK-DEMO-001'},
    @{Name='KioskApi Professionals'; Url='http://localhost:8083/KioskApi/professionals?serviceId=demo&deviceCode=KIOSK-DEMO-001'}
)

$routes = @(
    @{Name='Admin route'; Url='http://localhost:8081/Admin'},
    @{Name='Admin route'; Url='http://localhost:8081/Admin/Dashboard'},
    @{Name='Admin route'; Url='http://localhost:8081/Admin/Diagnostics'},
    @{Name='Admin route'; Url='http://localhost:8081/Admin/FullServiceFlow'},
    @{Name='Admin route'; Url='http://localhost:8081/Admin/Clients'},
    @{Name='Admin route'; Url='http://localhost:8081/Admin/Professionals'},
    @{Name='Admin route'; Url='http://localhost:8081/Admin/Services'},
    @{Name='Admin route'; Url='http://localhost:8081/Admin/Appointments'},
    @{Name='Admin route'; Url='http://localhost:8081/Admin/ServiceOrders'},
    @{Name='Admin route'; Url='http://localhost:8081/Admin/Stock'},
    @{Name='Admin route'; Url='http://localhost:8081/Admin/Reviews'},
    @{Name='Admin route'; Url='http://localhost:8081/Admin/Operations'},
    @{Name='Admin route'; Url='http://localhost:8081/Admin/Copilot'},
    @{Name='Admin route'; Url='http://localhost:8081/Admin/Reports'},
    @{Name='Admin route'; Url='http://localhost:8081/Admin/Help'},
    @{Name='Public route'; Url='http://localhost:8082/'},
    @{Name='Kiosk route'; Url='http://localhost:8083/Kiosk/Services'},
    @{Name='Kiosk route'; Url='http://localhost:8083/Kiosk/Client'},
    @{Name='Kiosk route'; Url='http://localhost:8083/Kiosk/Professional'},
    @{Name='Kiosk route'; Url='http://localhost:8083/Kiosk/Confirm'},
    @{Name='Kiosk route'; Url='http://localhost:8083/Kiosk/Payment'},
    @{Name='Kiosk route'; Url='http://localhost:8083/Kiosk/Success'},
    @{Name='Kiosk route'; Url='http://localhost:8083/Kiosk/Review'}
)

$assets = @(
    @{Name='Admin asset'; Url='http://localhost:8081/css/admin-design-system.css'},
    @{Name='Admin asset'; Url='http://localhost:8081/css/admin-layout.css'},
    @{Name='Admin asset'; Url='http://localhost:8081/css/admin-components.css'},
    @{Name='Admin asset'; Url='http://localhost:8081/css/admin-dashboard.css'},
    @{Name='Admin asset'; Url='http://localhost:8081/js/admin-api-client.js'},
    @{Name='Admin asset'; Url='http://localhost:8081/js/admin-dashboard.js'},
    @{Name='Admin asset'; Url='http://localhost:8081/js/admin-crud.js'},
    @{Name='Admin asset'; Url='http://localhost:8081/js/admin-demo-store.js'},
    @{Name='Admin asset'; Url='http://localhost:8081/js/admin-event-bus.js'},
    @{Name='Admin asset'; Url='http://localhost:8081/js/admin-full-service-flow.js'},
    @{Name='Admin asset'; Url='http://localhost:8081/img/logo-barbersync.svg'},
    @{Name='Public asset'; Url='http://localhost:8082/css/public-design-system.css'},
    @{Name='Public asset'; Url='http://localhost:8082/css/public.css'},
    @{Name='Public asset'; Url='http://localhost:8082/js/public.js'},
    @{Name='Public asset'; Url='http://localhost:8082/img/logo-barbersync.svg'},
    @{Name='Kiosk asset'; Url='http://localhost:8083/css/kiosk-design-system.css'},
    @{Name='Kiosk asset'; Url='http://localhost:8083/css/kiosk.css'},
    @{Name='Kiosk asset'; Url='http://localhost:8083/js/kiosk.js'},
    @{Name='Kiosk asset'; Url='http://localhost:8083/js/kiosk-flow.js'},
    @{Name='Kiosk asset'; Url='http://localhost:8083/img/logo-barbersync.svg'}
)

$postEndpoints = @(
    @{Name='PublicApi Appointment POST'; Url='http://localhost:8082/PublicApi/appointments'; Json='{"name":"Quality Gate","phone":"(11) 99999-0000","serviceId":"srv-001"}'},
    @{Name='PublicApi Lead POST'; Url='http://localhost:8082/PublicApi/leads'; Json='{"name":"Quality Gate Lead","phone":"(11) 99999-0001"}'},
    @{Name='KioskApi Find Phone POST'; Url='http://localhost:8083/KioskApi/client/find-by-phone'; Json='{"phone":"(11) 99999-0002"}'},
    @{Name='KioskApi Quick Register POST'; Url='http://localhost:8083/KioskApi/client/quick-register'; Json='{"name":"Cliente Totem","phone":"(11) 99999-0003"}'},
    @{Name='KioskApi Payment Mock POST'; Url='http://localhost:8083/KioskApi/payment/mock'; Json='{"amount":75,"method":"PIX"}'},
    @{Name='KioskApi Review POST'; Url='http://localhost:8083/KioskApi/review'; Json='{"rating":5,"comment":"Quality Gate"}'}
)

Write-Host "`n=== Endpoints ===" -ForegroundColor Cyan
$endpoints | ForEach-Object { Test-Url200 $_.Name $_.Url }
Write-Host "`n=== Visual routes ===" -ForegroundColor Cyan
$routes | ForEach-Object { Test-Url200 $_.Name $_.Url }
Write-Host "`n=== Static files ===" -ForegroundColor Cyan
$assets | ForEach-Object { Test-Url200 $_.Name $_.Url }
Write-Host "`n=== POST smoke endpoints ===" -ForegroundColor Cyan
$postEndpoints | ForEach-Object { Test-HttpPost200 $_.Name $_.Url $_.Json }
Write-Host "`n=== Auditoria browser/server ===" -ForegroundColor Cyan
Assert-NoForbiddenBrowserUrl
Assert-CleanLogs

Write-Host "`n=== Relatório Final BarberSync Quality Gate + Demo Ready 17.0 ===" -ForegroundColor Cyan
$results | Format-Table -AutoSize
Save-QualityGateReport
Write-Host "`nRelatório salvo em Docs/quality-gate-last-run.md" -ForegroundColor Cyan
if ($failures.Count -gt 0) {
    Write-Host "`nQuality Gate FALHOU:" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}
Write-Host "`nQuality Gate APROVADO: build, Docker, endpoints, assets, proxies, front e logs validados." -ForegroundColor Green
exit 0
