param(
    [switch]$SkipDockerBuild,
    [switch]$SkipDotnetBuild,
    [int]$WarmupSeconds = 20
)

$ErrorActionPreference = 'Stop'
$results = New-Object System.Collections.Generic.List[object]
$failures = New-Object System.Collections.Generic.List[string]

function Add-Result {
    param([string]$Name, [string]$Target, [string]$Status, [string]$Detail = '')
    $results.Add([pscustomobject]@{ Name = $Name; Target = $Target; Status = $Status; Detail = $Detail }) | Out-Null
    $icon = if ($Status -eq 'OK') { '✅' } elseif ($Status -eq 'WARN') { '⚠️' } else { '❌' }
    Write-Host "$icon $Name -> $Target $Detail"
    if ($Status -eq 'FAIL') { $failures.Add("$Name -> $Target $Detail") | Out-Null }
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
    @{Name='AdminApi Dashboard'; Url='http://localhost:8081/AdminApi/dashboard'},
    @{Name='AdminApi Clients'; Url='http://localhost:8081/AdminApi/clients'},
    @{Name='AdminApi Professionals'; Url='http://localhost:8081/AdminApi/professionals'},
    @{Name='AdminApi Services'; Url='http://localhost:8081/AdminApi/services'},
    @{Name='AdminApi Appointments'; Url='http://localhost:8081/AdminApi/appointments'},
    @{Name='AdminApi Service Orders'; Url='http://localhost:8081/AdminApi/service-orders'},
    @{Name='AdminApi Products'; Url='http://localhost:8081/AdminApi/products'},
    @{Name='PublicApi Services'; Url='http://localhost:8082/PublicApi/services'},
    @{Name='PublicApi Professionals'; Url='http://localhost:8082/PublicApi/professionals'},
    @{Name='KioskApi Services'; Url='http://localhost:8083/KioskApi/services?deviceCode=KIOSK-DEMO-001'},
    @{Name='KioskApi Professionals'; Url='http://localhost:8083/KioskApi/professionals?serviceId=demo&deviceCode=KIOSK-DEMO-001'}
)

$assets = @(
    @{Name='Admin asset'; Url='http://localhost:8081/css/admin-design-system.css'},
    @{Name='Admin asset'; Url='http://localhost:8081/css/admin-layout.css'},
    @{Name='Admin asset'; Url='http://localhost:8081/js/admin-dashboard.js'},
    @{Name='Admin asset'; Url='http://localhost:8081/js/admin-demo-store.js'},
    @{Name='Admin asset'; Url='http://localhost:8081/js/admin-event-bus.js'},
    @{Name='Admin asset'; Url='http://localhost:8081/img/logo-barbersync.svg'},
    @{Name='Public asset'; Url='http://localhost:8082/css/public-design-system.css'},
    @{Name='Public asset'; Url='http://localhost:8082/js/public.js'},
    @{Name='Public asset'; Url='http://localhost:8082/img/logo-barbersync.svg'},
    @{Name='Kiosk asset'; Url='http://localhost:8083/css/kiosk-design-system.css'},
    @{Name='Kiosk asset'; Url='http://localhost:8083/js/kiosk.js'},
    @{Name='Kiosk asset'; Url='http://localhost:8083/js/kiosk-flow.js'},
    @{Name='Kiosk asset'; Url='http://localhost:8083/img/logo-barbersync.svg'}
)

Write-Host "`n=== Endpoints ===" -ForegroundColor Cyan
$endpoints | ForEach-Object { Test-Url200 $_.Name $_.Url }
Write-Host "`n=== Static files ===" -ForegroundColor Cyan
$assets | ForEach-Object { Test-Url200 $_.Name $_.Url }
Write-Host "`n=== Auditoria browser/server ===" -ForegroundColor Cyan
Assert-NoForbiddenBrowserUrl
Assert-CleanLogs

Write-Host "`n=== Relatório Final BarberSync Quality Gate + Demo Ready 16.0 ===" -ForegroundColor Cyan
$results | Format-Table -AutoSize
if ($failures.Count -gt 0) {
    Write-Host "`nQuality Gate FALHOU:" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}
Write-Host "`nQuality Gate APROVADO: build, Docker, endpoints, assets, proxies, front e logs validados." -ForegroundColor Green
exit 0
