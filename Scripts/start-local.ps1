[CmdletBinding()]
param(
  [string]$PgHost = $(if ($env:PGHOST) {$env:PGHOST} else {'localhost'}),
  [int]$PgPort = $(if ($env:PGPORT) {[int]$env:PGPORT} else {5432}),
  [string]$PgUser = $(if ($env:PGUSER) {$env:PGUSER} else {'postgres'}),
  [string]$Database = 'barber',
  [switch]$CreateDatabase,
  [switch]$WithPublic,
  [switch]$WithKiosk
)
$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot
function Require-Command([string]$Name, [string]$Hint) {
  if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) { throw "Comando '$Name' não encontrado. $Hint" }
}
Require-Command dotnet 'Instale o .NET SDK 10 e reinicie o terminal.'
Require-Command psql 'Adicione a pasta bin do PostgreSQL ao PATH (ex.: C:\Program Files\PostgreSQL\17\bin).'
if (-not $env:PGPASSWORD) { throw 'Defina PGPASSWORD no ambiente antes de iniciar; senhas não são armazenadas no repositório.' }
$psqlArgs = @('-h',$PgHost,'-p',$PgPort,'-U',$PgUser,'-v','ON_ERROR_STOP=1')
& psql @psqlArgs -d postgres -c 'select 1' | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Falha ao conectar ao PostgreSQL em ${PgHost}:${PgPort}." }
$exists = (& psql @psqlArgs -d postgres -tAc "select 1 from pg_database where datname='$Database'").Trim()
if (-not $exists) {
  if (-not $CreateDatabase) { throw "Database '$Database' não existe. Execute novamente com -CreateDatabase." }
  & psql @psqlArgs -d postgres -c "create database `"$Database`""
  if ($LASTEXITCODE -ne 0) { throw "Não foi possível criar o database '$Database'." }
}
& psql @psqlArgs -d $Database -f (Join-Path $Root 'ScriptsSQL\script_completo.sql')
if ($LASTEXITCODE -ne 0) { throw 'A atualização do schema falhou; nenhuma aplicação foi iniciada.' }
$env:ConnectionStrings__DefaultConnection = "Host=$PgHost;Port=$PgPort;Database=$Database;Username=$PgUser;Password=$env:PGPASSWORD"
$env:Cors__AllowedOrigins__0 = 'http://localhost:5081'
$env:Cors__AllowedOrigins__1 = 'http://localhost:5082'
$env:Cors__AllowedOrigins__2 = 'http://localhost:5083'
$jobs = @()
$jobs += Start-Process dotnet -ArgumentList 'run','--project','Backend/Presentation/BarberSync.Api/BarberSync.Api.csproj','--urls','http://localhost:5080' -WorkingDirectory $Root -PassThru
$jobs += Start-Process dotnet -ArgumentList 'run','--project','Web/BarberSync.AdminWeb/BarberSync.AdminWeb.csproj','--urls','http://localhost:5081' -WorkingDirectory $Root -PassThru
if ($WithPublic) { $jobs += Start-Process dotnet -ArgumentList 'run','--project','Web/BarberSync.PublicWeb/BarberSync.PublicWeb.csproj','--urls','http://localhost:5082' -WorkingDirectory $Root -PassThru }
if ($WithKiosk) { $jobs += Start-Process dotnet -ArgumentList 'run','--project','Web/BarberSync.KioskWeb/BarberSync.KioskWeb.csproj','--urls','http://localhost:5083' -WorkingDirectory $Root -PassThru }
Write-Host 'BarberSync iniciado sem Docker:' -ForegroundColor Green
Write-Host '  API:    http://localhost:5080/swagger'
Write-Host '  Admin:  http://localhost:5081'
if ($WithPublic) { Write-Host '  Site:   http://localhost:5082' }
if ($WithKiosk) { Write-Host '  Totem:  http://localhost:5083' }
Write-Host "Processos: $($jobs.Id -join ', '). Pressione Ctrl+C para encerrar."
try { Wait-Process -Id $jobs.Id } finally { $jobs | Stop-Process -ErrorAction SilentlyContinue }
