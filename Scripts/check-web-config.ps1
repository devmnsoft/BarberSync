[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$projects = [ordered]@{
  AdminWeb = '.\Web\BarberSync.AdminWeb\BarberSync.AdminWeb.csproj'
  PublicWeb = '.\Web\BarberSync.PublicWeb\BarberSync.PublicWeb.csproj'
  KioskWeb = '.\Web\BarberSync.KioskWeb\BarberSync.KioskWeb.csproj'
}
$failed = $false
$baseUrls = @{}
foreach ($entry in $projects.GetEnumerator()) {
  if (-not (Test-Path -LiteralPath $entry.Value -PathType Leaf)) { Write-Error "Projeto não encontrado: $($entry.Value)"; $failed = $true; continue }
  $secrets = @(dotnet user-secrets list --project $entry.Value 2>$null)
  $line = $secrets | Where-Object { $_ -match '^ApiSettings:BaseUrl\s*=' } | Select-Object -First 1
  if (-not $line) { Write-Warning "$($entry.Key): ApiSettings:BaseUrl não configurada em user-secrets."; $failed = $true; continue }
  $baseUrl = ($line -split '=', 2)[1].Trim().TrimEnd('/')
  $baseUrls[$entry.Key] = $baseUrl
  try { $uri = [Uri]$baseUrl; Write-Host "$($entry.Key): API $($uri.Scheme)://$($uri.Host):$($uri.Port)" }
  catch { Write-Warning "$($entry.Key): BaseUrl inválida."; $failed = $true }
}

$kioskSecrets = @(dotnet user-secrets list --project $projects.KioskWeb 2>$null)
if (-not ($kioskSecrets | Where-Object { $_ -match '^Kiosk:DeviceCode\s*=\s*\S+' })) {
  Write-Warning 'KioskWeb: Kiosk:DeviceCode não configurado.'; $failed = $true
} else { Write-Host 'KioskWeb: DeviceCode configurado (valor oculto).' }

$apiBaseUrl = $baseUrls.Values | Select-Object -First 1
if ($apiBaseUrl) {
  $apiUri = [Uri]$apiBaseUrl
  $tcp = [System.Net.Sockets.TcpClient]::new()
  try {
    $connect = $tcp.ConnectAsync($apiUri.Host, $apiUri.Port)
    if (-not $connect.Wait(3000) -or -not $tcp.Connected) { throw "Porta $($apiUri.Port) não está ouvindo." }
    Write-Host "Porta API: $($apiUri.Host):$($apiUri.Port) está ouvindo."
    $response = Invoke-WebRequest -Uri "$apiBaseUrl/health" -Method Get -TimeoutSec 5 -SkipCertificateCheck
    Write-Host "API health: HTTP $($response.StatusCode)"
  } catch {
    Write-Warning "A API configurada em $apiBaseUrl não respondeu. Verifique se BarberSync.Api está rodando nessa porta."
    $failed = $true
  } finally {
    $tcp.Dispose()
  }
}
if ($failed) { exit 1 }
Write-Host 'Configuração Web válida.'
