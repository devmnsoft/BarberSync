[CmdletBinding()]
param(
  [string]$ApiBaseUrl = 'https://localhost:7088',
  [string]$KioskDeviceCode = 'KIOSK-LOCAL-001'
)

$ErrorActionPreference = 'Stop'
if (-not [Uri]::IsWellFormedUriString($ApiBaseUrl, [UriKind]::Absolute)) { throw 'ApiBaseUrl deve ser uma URL absoluta.' }
$projects = [ordered]@{
  AdminWeb = '.\Web\BarberSync.AdminWeb\BarberSync.AdminWeb.csproj'
  PublicWeb = '.\Web\BarberSync.PublicWeb\BarberSync.PublicWeb.csproj'
  KioskWeb = '.\Web\BarberSync.KioskWeb\BarberSync.KioskWeb.csproj'
}
foreach ($entry in $projects.GetEnumerator()) {
  if (-not (Test-Path -LiteralPath $entry.Value -PathType Leaf)) { throw "Projeto não encontrado: $($entry.Value)" }
  dotnet user-secrets init --project $entry.Value 2>$null | Out-Null
  dotnet user-secrets set 'ApiSettings:BaseUrl' $ApiBaseUrl.TrimEnd('/') --project $entry.Value | Out-Null
}
& "$PSScriptRoot\setup-kiosk-local-dev.ps1" -ProjectPath $projects.KioskWeb -DeviceCode $KioskDeviceCode -ApiBaseUrl $ApiBaseUrl

Write-Host "API........: $($ApiBaseUrl.TrimEnd('/'))"
Write-Host "AdminWeb...: configurado para API $($ApiBaseUrl.TrimEnd('/')) (http://localhost:5081)"
Write-Host "PublicWeb..: configurado para API $($ApiBaseUrl.TrimEnd('/')) (http://localhost:5082)"
Write-Host "KioskWeb...: DeviceCode configurado (http://localhost:5083/Kiosk)"
Write-Host "Diagnóstico: .\Scripts\check-web-config.ps1"
