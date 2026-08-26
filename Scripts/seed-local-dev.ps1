[CmdletBinding()]
param(
  [string]$ConnectionString,
  [string]$SqlPath = '.\ScriptsSQL\local_development_seed.sql'
)

$ErrorActionPreference = 'Stop'
if ($env:ASPNETCORE_ENVIRONMENT -and $env:ASPNETCORE_ENVIRONMENT -ne 'Development') { throw 'O seed local só pode ser executado com ASPNETCORE_ENVIRONMENT=Development.' }
$apiProject = Join-Path $PSScriptRoot '..\Backend\Presentation\BarberSync.Api\BarberSync.Api.csproj'
if (-not (Get-Command psql -ErrorAction SilentlyContinue)) { throw 'psql não encontrado no PATH.' }
if (-not $ConnectionString) {
  $secret = dotnet user-secrets list --project $apiProject 2>$null | Where-Object { $_ -match '^ConnectionStrings:DefaultConnection\s*=' } | Select-Object -First 1
  if ($secret) { $ConnectionString = ($secret -split '=', 2)[1].Trim() }
}
if (-not $ConnectionString) { throw 'ConnectionStrings:DefaultConnection não foi configurada. Execute setup-api-local-dev.ps1.' }

# ASP.NET Identity PasswordHasher V3: PRF HMAC-SHA256, 100000 iterations, 16-byte salt, 32-byte subkey.
$salt = [Text.Encoding]::UTF8.GetBytes('BarberSyncLocal!') # deterministic only because this is a documented local-only credential
$derive = [Security.Cryptography.Rfc2898DeriveBytes]::new('Dev@123456', $salt, 100000, [Security.Cryptography.HashAlgorithmName]::SHA256)
$subkey = $derive.GetBytes(32); $derive.Dispose()
$payload = [byte[]]::new(61); $payload[0] = 1
[Array]::Copy([byte[]](0,0,0,1), 0, $payload, 1, 4); [Array]::Copy([byte[]](0,1,134,160), 0, $payload, 5, 4); [Array]::Copy([byte[]](0,0,0,16), 0, $payload, 9, 4)
[Array]::Copy($salt, 0, $payload, 13, 16); [Array]::Copy($subkey, 0, $payload, 29, 32)
$passwordHash = [Convert]::ToBase64String($payload)

# Reuse the connection parsing without ever echoing the resulting URI.
. (Join-Path $PSScriptRoot 'local-database-common.ps1')
$databaseUrl = Convert-ToLocalPostgresUri $ConnectionString
& psql $databaseUrl --no-psqlrc --set ON_ERROR_STOP=1 --set seed_environment=Development --set password_hash=$passwordHash --file (Resolve-Path -LiteralPath $SqlPath).Path
if ($LASTEXITCODE -ne 0) { throw 'Falha ao aplicar o seed local.' }
Write-Host 'Seed local aplicado: BarberSync Local / Unidade Local.'
Write-Host 'Admin: admin@barbersync.local | Kiosk: KIOSK-LOCAL-001 (senha documentada apenas para Development).'
