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
if (-not $WebOnly) { $apps += @{ Name='API'; Project='Backend/Presentation/BarberSync.Api/BarberSync.Api.csproj'; Profile='https'; Url='https://localhost:7088/health' } }
if (-not $ApiOnly) {
  $apps += @{ Name='AdminWeb'; Project='Web/BarberSync.AdminWeb/BarberSync.AdminWeb.csproj'; Profile='https'; Url='https://localhost:7188/Account/Login' }
  $apps += @{ Name='PublicWeb'; Project='Web/BarberSync.PublicWeb/BarberSync.PublicWeb.csproj'; Profile='https'; Url='https://localhost:7288' }
  $apps += @{ Name='KioskWeb'; Project='Web/BarberSync.KioskWeb/BarberSync.KioskWeb.csproj'; Profile='https'; Url='https://localhost:7388/Kiosk?deviceCode=KIOSK-LOCAL-001' }
}
$processes = @()
try {
  foreach ($app in $apps) {
    $logDir = Join-Path $root '.local-logs'; New-Item -ItemType Directory -Force $logDir | Out-Null
    $stdout = Join-Path $logDir "$($app.Name).log"; $stderr = Join-Path $logDir "$($app.Name).error.log"
    $arguments = @('run','--no-launch-profile','--project',(Join-Path $root $app.Project),'--configuration',$Configuration,'--urls',($app.Url -replace '/health$|/Account/Login$|/Kiosk.*$',''))
    $processes += Start-Process dotnet -ArgumentList $arguments -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
    Write-Host "$($app.Name): $($app.Url) (logs: $stdout)"
  }
  Start-Sleep -Seconds 3
  if (-not $NoBrowser) { foreach ($app in $apps) { Start-Process $app.Url } }
  Write-Host 'Stack iniciada. Pressione Ctrl+C para encerrar todos os processos.'
  while ($true) { Start-Sleep -Seconds 1; if (($processes | Where-Object { -not $_.HasExited }).Count -eq 0) { throw 'Todos os processos locais foram encerrados; consulte .local-logs.' } }
} finally {
  foreach ($process in $processes) { if (-not $process.HasExited) { Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue } }
}
