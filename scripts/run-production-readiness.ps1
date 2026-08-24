$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$ComposeFile = Join-Path $Root "docker-compose.production-readiness.yml"
$LogDir = Join-Path $Root "artifacts/production-readiness"
$Project = "barbersync-production-readiness"

function Invoke-Checked {
    param([string]$Log, [scriptblock]$Command)
    & $Command 2>&1 | Tee-Object -FilePath (Join-Path $LogDir $Log)
    if ($LASTEXITCODE -ne 0) { throw "Command failed (exit $LASTEXITCODE). See $Log." }
}

function Add-PassMarker {
    param([string]$Marker, [string]$Log)
    "EVIDENCE:${Marker}:PASS" | Tee-Object -FilePath (Join-Path $LogDir $Log) -Append
}

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { throw "Docker is required." }
docker info *> $null
if ($LASTEXITCODE -ne 0) { throw "The Docker daemon is unavailable." }
docker compose version *> $null
if ($LASTEXITCODE -ne 0) { throw "Docker Compose v2 is required." }

New-Item -ItemType Directory -Force -Path $LogDir | Out-Null
Remove-Item (Join-Path $LogDir "*.log") -Force -ErrorAction SilentlyContinue
Push-Location $Root
try {
    docker compose -p $Project -f $ComposeFile up -d --wait postgres
    if ($LASTEXITCODE -ne 0) { throw "PostgreSQL 16 did not become ready." }

    Invoke-Checked "dotnet-info.log" { docker compose -p $Project -f $ComposeFile run --rm --no-deps api dotnet --info }
    Invoke-Checked "dotnet-restore.log" { docker compose -p $Project -f $ComposeFile run --rm --no-deps api dotnet restore BarberSync.sln }
    Add-PassMarker "DOTNET_RESTORE" "dotnet-restore.log"
    Invoke-Checked "dotnet-build-debug.log" { docker compose -p $Project -f $ComposeFile run --rm --no-deps api dotnet build BarberSync.sln --configuration Debug --no-restore }
    Add-PassMarker "BUILD_DEBUG" "dotnet-build-debug.log"
    Invoke-Checked "dotnet-build-release.log" { docker compose -p $Project -f $ComposeFile run --rm --no-deps api dotnet build BarberSync.sln --configuration Release --no-restore }
    Add-PassMarker "BUILD_RELEASE" "dotnet-build-release.log"

    Invoke-Checked "sql-apply-1.log" { Get-Content -Raw "ScriptsSQL/script_completo.sql" | docker compose -p $Project -f $ComposeFile exec -T postgres psql "host=localhost port=5432 dbname=barber user=postgres password=postgres" -v ON_ERROR_STOP=1 }
    Add-PassMarker "SQL_APPLY_1" "sql-apply-1.log"
    Invoke-Checked "sql-apply-2.log" { Get-Content -Raw "ScriptsSQL/script_completo.sql" | docker compose -p $Project -f $ComposeFile exec -T postgres psql "host=localhost port=5432 dbname=barber user=postgres password=postgres" -v ON_ERROR_STOP=1 }
    Add-PassMarker "SQL_APPLY_2" "sql-apply-2.log"
    Get-Content -Raw "scripts/validate-production-schema.sql" | docker compose -p $Project -f $ComposeFile exec -T postgres psql "host=localhost port=5432 dbname=barber user=postgres password=postgres" -v ON_ERROR_STOP=1 2>&1 | Tee-Object -FilePath (Join-Path $LogDir "sql-apply-2.log") -Append
    if ($LASTEXITCODE -ne 0) { throw "Critical schema validation failed. See sql-apply-2.log." }
    Add-PassMarker "SCHEMA_VALIDATION" "sql-apply-2.log"
    Invoke-Checked "readiness-seed.log" { Get-Content -Raw "ScriptsSQL/production_readiness_seed.sql" | docker compose -p $Project -f $ComposeFile exec -T postgres psql "host=localhost port=5432 dbname=barber user=postgres password=postgres options=-cbarbersync.environment=ProductionReadiness" -v ON_ERROR_STOP=1 }
    Add-PassMarker "READINESS_SEED" "readiness-seed.log"

    docker compose -p $Project -f $ComposeFile up -d api
    if ($LASTEXITCODE -ne 0) { throw "The API container could not start." }
    $Healthy = $false
    for ($Attempt = 1; $Attempt -le 60; $Attempt++) {
        docker compose -p $Project -f $ComposeFile exec -T api bash -lc "curl -fsS http://localhost:5080/health" *> (Join-Path $LogDir "health.log")
        if ($LASTEXITCODE -eq 0) { $Healthy = $true; break }
        Start-Sleep -Seconds 2
    }
    docker compose -p $Project -f $ComposeFile logs --no-color api *> (Join-Path $LogDir "api-run.log")
    if (-not $Healthy) { throw "API did not become healthy within 120 seconds." }
    Add-PassMarker "API_RUNTIME" "api-run.log"
    Add-PassMarker "HEALTH" "health.log"

    Invoke-Checked "production-smoke.log" { docker compose -p $Project -f $ComposeFile run --rm --no-deps api bash -lc "./scripts/production-smoke.sh http://api:5080" }
    Add-PassMarker "PRODUCTION_SMOKE" "production-smoke.log"
    $Defaults=@{READINESS_ADMIN_EMAIL="admin@readiness.local";READINESS_ADMIN_PASSWORD="ReadinessOnly!2026";READINESS_CASHIER_EMAIL="cashier@readiness.local";READINESS_CASHIER_PASSWORD="ReadinessOnly!2026";READINESS_PROFESSIONAL_EMAIL="professional@readiness.local";READINESS_PROFESSIONAL_PASSWORD="ReadinessOnly!2026";READINESS_CLIENT_EMAIL="client@readiness.local";READINESS_CLIENT_PASSWORD="ReadinessOnly!2026";READINESS_TENANT_ID="70000000-0000-4000-8000-000000000001";READINESS_BRANCH_ID="70000000-0000-4000-8000-000000000002";READINESS_KIOSK_DEVICE_CODE="READINESS-KIOSK-001"}
    foreach($Key in $Defaults.Keys){if(-not [Environment]::GetEnvironmentVariable($Key)){[Environment]::SetEnvironmentVariable($Key,$Defaults[$Key])}}
    Invoke-Checked "authenticated-production-smoke.log" { & (Join-Path $Root "scripts/authenticated-production-smoke.ps1") -BaseUrl http://localhost:5080 }
    Add-PassMarker "AUTHENTICATED_PRODUCTION_SMOKE" "authenticated-production-smoke.log"
    Invoke-Checked "frontend.log" { docker compose -p $Project -f $ComposeFile --profile tools run --rm node 'find Web -name "*.js" -print0 | xargs -0 -r -n1 node --check' }
    Add-PassMarker "FRONTEND_CHECKS" "frontend.log"
    Invoke-Checked "mobile-smoke.log" { docker compose -p $Project -f $ComposeFile --profile tools run --rm node "npm test --prefix MobileApp" }
    Add-PassMarker "MOBILE_SMOKE" "mobile-smoke.log"
    Invoke-Checked "totem-smoke.log" { docker compose -p $Project -f $ComposeFile --profile tools run --rm node "npm test --prefix Totem" }
    Add-PassMarker "TOTEM_SMOKE" "totem-smoke.log"
    Invoke-Checked "totem-build.log" { docker compose -p $Project -f $ComposeFile --profile tools run --rm node "npm run build --prefix Totem" }
    Add-PassMarker "TOTEM_BUILD" "totem-build.log"
    Write-Host "Production readiness passed. Logs: $LogDir"
}
finally {
    docker compose -p $Project -f $ComposeFile down --remove-orphans *> $null
    Pop-Location
}
