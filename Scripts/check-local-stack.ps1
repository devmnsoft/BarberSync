[CmdletBinding()]
param(
  [string]$ApiUrl = 'https://localhost:7088',
  [string]$AdminUrl = 'https://localhost:7188',
  [string]$PublicUrl = 'https://localhost:7288',
  [string]$KioskUrl = 'https://localhost:7388'
)

$ErrorActionPreference = 'Continue'; $failed = $false
$apiProject = Join-Path $PSScriptRoot '..\Backend\Presentation\BarberSync.Api\BarberSync.Api.csproj'
function Fail([string]$Message,[string]$Action) { $script:failed=$true; Write-Host "[FAIL] $Message" -ForegroundColor Red; Write-Host "Ação sugerida: $Action" -ForegroundColor Yellow }
function Secrets([string]$Project) { @(dotnet user-secrets list --project $Project 2>$null) }
function SecretValue($Lines,[string]$Key) { $line=$Lines | Where-Object { $_ -match "^$([regex]::Escape($Key))\s*=" } | Select-Object -First 1; if ($line) { ($line -split '=',2)[1].Trim() } }
function Check-Url([string]$Name,[string]$Url,[string]$Action) {
  try {
    $r=Invoke-WebRequest $Url -TimeoutSec 8 -SkipCertificateCheck -MaximumRedirection 5
    if ($r.StatusCode -lt 200 -or $r.StatusCode -ge 400) { throw "HTTP $($r.StatusCode)" }
    Write-Host "[OK] $Name HTTP $($r.StatusCode)" -ForegroundColor Green
  } catch {
    $status = if ($_.Exception.Response.StatusCode) { " HTTP $([int]$_.Exception.Response.StatusCode)" } else { '' }
    Fail "$Name não respondeu com sucesso em $Url.$status" $Action
  }
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { Fail 'dotnet SDK ausente.' 'Instale o SDK indicado em global.json.' }
else {
  $apiSecrets=Secrets $apiProject; $connection=SecretValue $apiSecrets 'ConnectionStrings:DefaultConnection'; $jwt=SecretValue $apiSecrets 'Jwt:SigningKey'
  if (-not $connection) { Fail 'ConnectionStrings:DefaultConnection não foi configurada.' '.\Scripts\setup-api-local-dev.ps1 -ConnectionString "Host=..."' }
  if (-not $jwt -or $jwt.Length -lt 32) { Fail 'Jwt:SigningKey deve possuir pelo menos 32 caracteres.' '.\Scripts\setup-api-local-dev.ps1 -ConnectionString "Host=..."' }
  if ($connection -and (Get-Command psql -ErrorAction SilentlyContinue)) {
    try { . (Join-Path $PSScriptRoot 'local-database-common.ps1'); & psql (Convert-ToLocalPostgresUri $connection) --no-psqlrc --set ON_ERROR_STOP=1 --tuples-only --command "SELECT current_database(),to_regnamespace('barber'),(SELECT count(*) FROM barber.schema_versions);" | Out-Host; if ($LASTEXITCODE -ne 0) { throw 'psql falhou' } } catch { Fail 'PostgreSQL/schema barber indisponível.' '.\Scripts\apply-local-database.ps1' }
  } elseif ($connection) { Fail 'psql não encontrado; conexão PostgreSQL não pôde ser validada.' 'Instale as ferramentas cliente do PostgreSQL.' }
  $web=@{AdminWeb='BarberSync.AdminWeb';PublicWeb='BarberSync.PublicWeb';KioskWeb='BarberSync.KioskWeb'}
  foreach($name in $web.Keys) { $project=Join-Path $PSScriptRoot "..\Web\$($web[$name])\$($web[$name]).csproj"; $secrets=Secrets $project; $base=SecretValue $secrets 'ApiSettings:BaseUrl'; if (-not $base) { Fail "$name não possui ApiSettings:BaseUrl." '.\Scripts\setup-web-local-dev.ps1 -ApiBaseUrl "https://localhost:7088"' } elseif ($base.TrimEnd('/') -ne $ApiUrl.TrimEnd('/')) { Fail "$name está configurado para API $base, mas a URL esperada é $ApiUrl." ".\Scripts\setup-web-local-dev.ps1 -ApiBaseUrl `"$ApiUrl`"" }; if ($name -eq 'KioskWeb' -and -not (SecretValue $secrets 'Kiosk:DeviceCode')) { Fail 'Kiosk:DeviceCode não foi configurado.' '.\Scripts\setup-web-local-dev.ps1 -KioskDeviceCode "KIOSK-LOCAL-001"' } }
}
Check-Url 'API health' "$ApiUrl/health" 'Verifique os logs da API e a conexão PostgreSQL em artifacts/local-stack/logs.'
Check-Url 'API pública' "$ApiUrl/api/public/services" 'Verifique os logs da API e o seed local.'
Check-Url 'AdminWeb login' "$AdminUrl/Account/Login" 'Verifique os logs do AdminWeb e ApiSettings:BaseUrl.'
Check-Url 'PublicWeb' $PublicUrl 'Verifique os logs do PublicWeb e ApiSettings:BaseUrl.'
Check-Url 'KioskWeb' "$KioskUrl/Kiosk" 'Verifique os logs do KioskWeb, ApiSettings:BaseUrl e Kiosk:DeviceCode.'
if ($failed) { exit 1 }; Write-Host '[OK] Stack local íntegra.' -ForegroundColor Green
