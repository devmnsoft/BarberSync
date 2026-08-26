[CmdletBinding()]
param(
  [string]$ProjectPath = ".\Web\BarberSync.KioskWeb\BarberSync.KioskWeb.csproj",
  [string]$DeviceCode = "KIOSK-LOCAL-001",
  [string]$ApiBaseUrl
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $ProjectPath -PathType Leaf)) { throw "Projeto Kiosk não encontrado: $ProjectPath" }
if ($DeviceCode -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]{4,63}$') { throw 'DeviceCode inválido: use 5 a 64 letras, números, ponto, hífen ou sublinhado, sem espaços.' }
if ($ApiBaseUrl -and -not [Uri]::IsWellFormedUriString($ApiBaseUrl, [UriKind]::Absolute)) { throw 'ApiBaseUrl deve ser uma URL absoluta.' }

dotnet user-secrets init --project $ProjectPath 2>$null | Out-Null
dotnet user-secrets set 'Kiosk:DeviceCode' $DeviceCode --project $ProjectPath | Out-Null
if ($ApiBaseUrl) { dotnet user-secrets set 'ApiSettings:BaseUrl' $ApiBaseUrl.TrimEnd('/') --project $ProjectPath | Out-Null }

$masked = if ($DeviceCode.Length -le 4) { '****' } else { $DeviceCode.Substring(0, 4) + ('*' * ($DeviceCode.Length - 4)) }
Write-Host "Kiosk:DeviceCode: $masked (user-secrets)"
if ($ApiBaseUrl) { Write-Host "API..............: $($ApiBaseUrl.TrimEnd('/'))" }
Write-Host 'Totem............: http://localhost:5083/Kiosk'
