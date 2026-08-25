param(
  [switch]$SkipDockerGate,
  [switch]$OnlyPreflight,
  [switch]$PackageOnly,
  [string]$DatabaseUrl,
  [string]$OutputDir = "artifacts/release-evidence-package"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$LogDir = Join-Path $Root "artifacts/release-evidence"
$Log = Join-Path $LogDir "local-production-readiness.log"
$FinalizeOnFailure = $false
New-Item -ItemType Directory -Force -Path $LogDir | Out-Null

function Write-Log([string]$Message) {
  $safe = $Message -replace '(?i)(DATABASE_URL|Password|Token|Secret|Jwt|ApiKey|ConnectionStrings)(\s*[=:]\s*)\S+', '$1$2***REDACTED***'
  $line = "[$(Get-Date -Format o)] $safe"
  $line | Add-Content -Encoding UTF8 $Log
  Write-Host $line
}
function Require-Command([string]$Name, [string]$Hint = '') {
  if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
    throw "Pré-requisito ausente: $Name. $Hint"
  }
  Write-Log "OK: $Name disponível."
}
function Invoke-Step([string]$Name, [scriptblock]$Command, [switch]$AllowFailure) {
  Write-Log "Iniciando: $Name"
  & $Command 2>&1 | ForEach-Object { Write-Log $_.ToString() }
  $code = $LASTEXITCODE
  if ($code -ne 0 -and -not $AllowFailure) { throw "$Name falhou (exit $code). Consulte $Log." }
  Write-Log "Finalizado: $Name (exit $code)."
  return $code
}

Push-Location $Root
try {
  Write-Log "Production Readiness local iniciado."
  if (-not (Test-Path (Join-Path $Root "BarberSync.sln")) -or -not (Test-Path (Join-Path $Root "scripts"))) {
    throw "Execute a partir de uma cópia válida da raiz do repositório BarberSync."
  }
  if ($PackageOnly) {
    Invoke-Step "empacotamento" { & (Join-Path $PSScriptRoot "package-release-evidence.ps1") -OutputDir $OutputDir }
    exit 0
  }

  if ($PSVersionTable.PSVersion.Major -lt 5) { throw "PowerShell 5.1 ou superior é obrigatório (PowerShell 7 recomendado)." }
  Write-Log "OK: PowerShell $($PSVersionTable.PSVersion)."
  Require-Command git "Instale Git for Windows."
  Require-Command dotnet "Instale o .NET SDK compatível com o projeto."
  Require-Command node "Instale Node.js 20 ou superior."
  Require-Command npm "O npm deve acompanhar a instalação do Node.js."
  $nodeMajor = [int]((node --version).TrimStart('v').Split('.')[0])
  if ($nodeMajor -lt 20) { throw "Node.js 20+ é obrigatório; detectado $(node --version)." }
  if (-not $SkipDockerGate) {
    Require-Command docker "Instale e inicie o Docker Desktop."
    docker info *> $null; if ($LASTEXITCODE -ne 0) { throw "Docker foi encontrado, mas o daemon não está disponível." }
    docker compose version *> $null; if ($LASTEXITCODE -ne 0) { throw "Docker Compose v2 é obrigatório." }
    Write-Log "OK: Docker daemon e Docker Compose v2 disponíveis."
  } else { Write-Log "AVISO: gate Docker desabilitado explicitamente; o resultado não poderá ser GO." }
  if ($DatabaseUrl) {
    Require-Command psql "Instale as ferramentas cliente do PostgreSQL para banco externo."
    $env:DATABASE_URL = $DatabaseUrl
    Write-Log "DATABASE_URL recebida: ***REDACTED***"
  }
  $dirty = git status --porcelain
  if ($dirty) { Write-Log "AVISO: working tree contém alterações; commit/branch serão registrados no pacote." }
  else { Write-Log "OK: working tree limpa." }

  Invoke-Step "contratos estáticos" { & (Join-Path $PSScriptRoot "validate-readiness-contracts.ps1") }
  if ($OnlyPreflight) { Write-Log "Preflight concluído com sucesso."; exit 0 }
  $FinalizeOnFailure = $true
  if (-not $SkipDockerGate) { Invoke-Step "gate Production Readiness" { & (Join-Path $PSScriptRoot "run-production-readiness.ps1") } }
  Invoke-Step "coleta de evidências" { & (Join-Path $PSScriptRoot "collect-release-evidence.ps1") }
  $summaryExit = Invoke-Step "resumo GO/NO-GO" { & (Join-Path $PSScriptRoot "summarize-release-evidence.ps1") } -AllowFailure
  Invoke-Step "empacotamento" { & (Join-Path $PSScriptRoot "package-release-evidence.ps1") -OutputDir $OutputDir }
  $FinalizeOnFailure = $false
  if ($summaryExit -ne 0) { throw "Resultado NO-GO. O pacote foi criado; consulte go-no-go.md e o primeiro marker falho/ausente." }
  Write-Log "Resultado final: GO."
} catch {
  Write-Log "ERRO: $($_.Exception.Message)"
  if ($FinalizeOnFailure) {
    Write-Log "Preservando evidências da falha e gerando pacote NO-GO."
    try { & (Join-Path $PSScriptRoot "collect-release-evidence.ps1") 2>&1 | ForEach-Object { Write-Log $_.ToString() } } catch { Write-Log "AVISO: coleta falhou: $($_.Exception.Message)" }
    try { & (Join-Path $PSScriptRoot "summarize-release-evidence.ps1") 2>&1 | ForEach-Object { Write-Log $_.ToString() } } catch { Write-Log "AVISO: resumo falhou: $($_.Exception.Message)" }
    try { & (Join-Path $PSScriptRoot "package-release-evidence.ps1") -OutputDir $OutputDir 2>&1 | ForEach-Object { Write-Log $_.ToString() } } catch { Write-Log "AVISO: empacotamento falhou: $($_.Exception.Message)" }
  }
  exit 1
} finally {
  Remove-Item Env:DATABASE_URL -ErrorAction SilentlyContinue
  Pop-Location
}
