[CmdletBinding()]
param(
  [string]$ConnectionString,
  [string]$SqlPath = '.\ScriptsSQL\script_completo.sql',
  [switch]$ValidateIdempotency
)

$ErrorActionPreference = 'Stop'
$apiProject = Join-Path $PSScriptRoot '..\Backend\Presentation\BarberSync.Api\BarberSync.Api.csproj'
if (-not (Get-Command psql -ErrorAction SilentlyContinue)) { throw 'psql não encontrado no PATH. Instale as ferramentas cliente do PostgreSQL.' }
if (-not $ConnectionString) {
  if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'dotnet SDK não encontrado; informe -ConnectionString.' }
  $secret = dotnet user-secrets list --project $apiProject 2>$null | Where-Object { $_ -match '^ConnectionStrings:DefaultConnection\s*=' } | Select-Object -First 1
  if ($secret) { $ConnectionString = ($secret -split '=', 2)[1].Trim() }
}
if (-not $ConnectionString) { throw 'ConnectionStrings:DefaultConnection não foi configurada. Execute .\Scripts\setup-api-local-dev.ps1 -ConnectionString "Host=...;Password=...".' }
$resolvedSql = (Resolve-Path -LiteralPath $SqlPath -ErrorAction Stop).Path

function Convert-ToPostgresUri([string]$value) {
  if ($value -match '^postgres(ql)?://') { return $value }
  $parts = @{}; $value -split ';' | ForEach-Object { if ($_ -match '^\s*([^=]+)=(.*)$') { $parts[$matches[1].Trim().ToLowerInvariant()] = $matches[2].Trim() } }
  $hostName = $parts['host']; $port = if ($parts['port']) { $parts['port'] } else { '5432' }
  $database = $parts['database']; $user = if ($parts['username']) { $parts['username'] } else { $parts['user id'] }; $password = $parts['password']
  if (-not $hostName -or -not $database -or -not $user) { throw 'ConnectionString inválida: Host, Database e Username são obrigatórios.' }
  return "postgresql://$([Uri]::EscapeDataString($user)):$([Uri]::EscapeDataString($password))@$hostName`:$port/$([Uri]::EscapeDataString($database))"
}

$databaseUrl = Convert-ToPostgresUri $ConnectionString
function Invoke-Schema { & psql $databaseUrl --no-psqlrc --set ON_ERROR_STOP=1 --file $resolvedSql; if ($LASTEXITCODE -ne 0) { throw 'Falha ao aplicar o schema.' } }
Write-Host 'Aplicando schema barber (credenciais ocultas)...'
Invoke-Schema
if ($ValidateIdempotency) { Write-Host 'Reaplicando para validar idempotência...'; Invoke-Schema }
& psql $databaseUrl --no-psqlrc --set ON_ERROR_STOP=1 --tuples-only --command "SELECT 'Database='||current_database(), 'Schema='||(to_regnamespace('barber') IS NOT NULL), 'SchemaVersions='||count(*) FROM barber.schema_versions;"
if ($LASTEXITCODE -ne 0) { throw 'Schema barber/schema_versions não pôde ser validado.' }
