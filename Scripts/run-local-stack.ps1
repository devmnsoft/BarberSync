[CmdletBinding()]
param(
  [switch]$NoBrowser,
  [switch]$ApiOnly,
  [switch]$WebOnly,
  [string]$Configuration = 'Debug',
  [int]$StartupTimeoutSeconds = 60
)

$ErrorActionPreference = 'Stop'
if ($ApiOnly -and $WebOnly) { throw 'Use somente um entre -ApiOnly e -WebOnly.' }
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'dotnet SDK não encontrado no PATH.' }

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$logDir = Join-Path $root 'artifacts/local-stack/logs'
New-Item -ItemType Directory -Force $logDir | Out-Null
$apps = @()
if (-not $WebOnly) { $apps += @{ Name='API'; Project='Backend/Presentation/BarberSync.Api/BarberSync.Api.csproj'; ListenUrl='https://localhost:7088'; ProbeUrl='https://localhost:7088/health'; BrowserUrl='https://localhost:7088/health' } }
if (-not $ApiOnly) {
  $apps += @{ Name='AdminWeb'; Project='Web/BarberSync.AdminWeb/BarberSync.AdminWeb.csproj'; ListenUrl='https://localhost:7188'; ProbeUrl='https://localhost:7188/Account/Login'; BrowserUrl='https://localhost:7188/Account/Login' }
  $apps += @{ Name='PublicWeb'; Project='Web/BarberSync.PublicWeb/BarberSync.PublicWeb.csproj'; ListenUrl='https://localhost:7288'; ProbeUrl='https://localhost:7288'; BrowserUrl='https://localhost:7288' }
  $apps += @{ Name='KioskWeb'; Project='Web/BarberSync.KioskWeb/BarberSync.KioskWeb.csproj'; ListenUrl='https://localhost:7388'; ProbeUrl='https://localhost:7388/Kiosk'; BrowserUrl='https://localhost:7388/Kiosk' }
}

function Test-ApplicationReady($App, $Process) {
  if ($Process.HasExited) { return $false }
  try {
    $response = Invoke-WebRequest $App.ProbeUrl -TimeoutSec 3 -SkipCertificateCheck -MaximumRedirection 5
    return $response.StatusCode -ge 200 -and $response.StatusCode -lt 400
  } catch { return $false }
}

$processes = @()
$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
try {
  # User-secrets are loaded only in Development. The runner must not silently start
  # the applications in Production while expecting setup-*-local-dev secrets.
  $env:ASPNETCORE_ENVIRONMENT = 'Development'
  foreach ($app in $apps) {
    $stdout = Join-Path $logDir "$($app.Name).log"
    $stderr = Join-Path $logDir "$($app.Name).error.log"
    $arguments = @('run','--no-launch-profile','--project',(Join-Path $root $app.Project),'--configuration',$Configuration,'--urls',$app.ListenUrl)
    $process = Start-Process dotnet -ArgumentList $arguments -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
    $processes += @{ App=$app; Process=$process; Stdout=$stdout; Stderr=$stderr }
    Write-Host "$($app.Name): PID $($process.Id), $($app.ListenUrl) (logs: $stdout)"
  }

  foreach ($entry in $processes) {
    $deadline = [DateTime]::UtcNow.AddSeconds($StartupTimeoutSeconds)
    while (-not (Test-ApplicationReady $entry.App $entry.Process) -and [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Seconds 1 }
    if (-not (Test-ApplicationReady $entry.App $entry.Process)) {
      Write-Host "Falha ao iniciar $($entry.App.Name). Últimas linhas do log de erro:" -ForegroundColor Red
      if (Test-Path $entry.Stderr) { Get-Content $entry.Stderr -Tail 20 | Write-Host }
      throw "$($entry.App.Name) não ficou disponível em $StartupTimeoutSeconds segundos."
    }
    Write-Host "[OK] $($entry.App.Name) respondeu em $($entry.App.ProbeUrl)" -ForegroundColor Green
  }

  if (-not $NoBrowser) { foreach ($entry in $processes) { Start-Process $entry.App.BrowserUrl } }
  Write-Host 'Stack iniciada. Pressione Ctrl+C para encerrar todos os processos.'
  while ($true) {
    Start-Sleep -Seconds 1
    $exited = @($processes | Where-Object { $_.Process.HasExited })
    if ($exited.Count -gt 0) { throw "Processo(s) encerrado(s): $($exited.App.Name -join ', '). Consulte $logDir." }
  }
} finally {
  foreach ($entry in $processes) { if (-not $entry.Process.HasExited) { Stop-Process -Id $entry.Process.Id -Force -ErrorAction SilentlyContinue } }
  $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
}
