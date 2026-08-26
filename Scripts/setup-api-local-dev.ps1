[CmdletBinding()]
param(
  [Parameter(Mandatory)] [string]$ConnectionString,
  [string]$JwtSigningKey
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot '..\Backend\Presentation\BarberSync.Api\BarberSync.Api.csproj'
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'dotnet SDK não encontrado no PATH.' }
if (-not $JwtSigningKey) {
  $bytes = [byte[]]::new(48)
  [Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
  $JwtSigningKey = [Convert]::ToBase64String($bytes)
}
if ($JwtSigningKey.Length -lt 32) { throw 'Jwt:SigningKey deve possuir pelo menos 32 caracteres.' }
dotnet user-secrets set 'ConnectionStrings:DefaultConnection' $ConnectionString --project $project | Out-Null
dotnet user-secrets set 'Jwt:SigningKey' $JwtSigningKey --project $project | Out-Null
Write-Host 'API local configurada em user-secrets (valores protegidos).'
Write-Host 'API HTTPS: https://localhost:7088 | HTTP: http://localhost:5080'
Write-Host 'Próximo passo: .\Scripts\apply-local-database.ps1'
