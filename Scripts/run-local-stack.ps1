[CmdletBinding()]
param(
  [switch]$NoBrowser,
  [switch]$ApiOnly,
  [switch]$WebOnly,
  [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
if ($ApiOnly -and $WebOnly) { throw 'Use somente um entre -ApiOnly e -WebOnly.' }
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'dotnet SDK não encontrado no PATH.' }
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$apps = @()
if (-not $WebOnly) { $apps += @{ Name='API'; Project='Backend/Presentation/BarberSync.Api/BarberSync.Api.csproj'; BaseUrl='https://localhost:7088'; HealthUrl='https://localhost:7088/health'; BrowserUrl='https://localhost:7088/health' } }
if (-not $ApiOnly) {
  $apps += @{ Name='AdminWeb'; Project='Web/BarberSync.AdminWeb/BarberSync.AdminWeb.csproj'; BaseUrl='https://localhost:7188'; HealthUrl='https://localhost:7188/Account/Login'; BrowserUrl='https://localhost:7188/Account/Login' }
  $apps += @{ Name='PublicWeb'; Project='Web/BarberSync.PublicWeb/BarberSync.PublicWeb.csproj'; BaseUrl='https://localhost:7288'; HealthUrl='https://localhost:7288'; BrowserUrl='https://localhost:7288' }
  $apps += @{ Name='KioskWeb'; Project='Web/BarberSync.KioskWeb/BarberSync.KioskWeb.csproj'; BaseUrl='https://localhost:7388'; HealthUrl='https://localhost:7388/Kiosk'; BrowserUrl='https://localhost:7388/Kiosk' }
}
$processes = @()
$logDir = Join-Path $root 'artifacts/local-stack/logs'
New-Item -ItemType Directory -Force $logDir | Out-Null

function Wait-ForEndpoint([hashtable]$App, [System.Diagnostics.Process]$Process) {
  $deadline = (Get-Date).AddSeconds(45)
  do {
    if ($Process.HasExited) { throw "$($App.Name) encerrou com código $($Process.ExitCode). Consulte $logDir." }
    try {
      $response = Invoke-WebRequest $App.HealthUrl -TimeoutSec 3 -SkipCertificateCheck -MaximumRedirection 5
      if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 400) {
        Write-Host "[OK] $($App.Name) respondeu HTTP $($response.StatusCode): $($App.HealthUrl)" -ForegroundColor Green
        return
      }
    } catch { Start-Sleep -Milliseconds 500 }
  } while ((Get-Date) -lt $deadline)
  throw "$($App.Name) não ficou pronto em 45 segundos. Consulte $logDir."
}

try {
  foreach ($app in $apps) {
    $stdout = Join-Path $logDir "$($app.Name).log"; $stderr = Join-Path $logDir "$($app.Name).error.log"
    $arguments = @('run','--no-launch-profile','--project',(Join-Path $root $app.Project),'--configuration',$Configuration,'--urls',$app.BaseUrl)
    $process = Start-Process dotnet -ArgumentList $arguments -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
    $processes += $process
    Write-Host "$($app.Name): PID $($process.Id), $($app.BaseUrl) (logs: $stdout)"
  }
  for ($index = 0; $index -lt $apps.Count; $index++) { Wait-ForEndpoint $apps[$index] $processes[$index] }
  if (-not $NoBrowser) { foreach ($app in $apps) { Start-Process $app.BrowserUrl } }
  Write-Host 'Stack iniciada. Pressione Ctrl+C para encerrar todos os processos.'
  while ($true) { Start-Sleep -Seconds 1; if (($processes | Where-Object { -not $_.HasExited }).Count -eq 0) { throw "Todos os processos locais foram encerrados; consulte $logDir." } }
} finally {
  foreach ($process in $processes) { if (-not $process.HasExited) { Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue } }
}
