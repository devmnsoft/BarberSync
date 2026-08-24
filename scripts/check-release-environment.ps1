$ErrorActionPreference = "Continue"
Set-StrictMode -Version Latest
$Ok = 0; $Warn = 0; $Fail = 0
function Write-Result([string]$Level, [string]$Message) {
    Write-Host "${Level}: $Message"
    switch ($Level) { "OK" { $script:Ok++ } "WARN" { $script:Warn++ } "FAIL" { $script:Fail++ } }
}
function Has-Command([string]$Name) { return [bool](Get-Command $Name -ErrorAction SilentlyContinue) }
$Available = @{}
foreach ($Tool in @("git", "gh", "docker", "dotnet", "psql", "node", "npm", "curl")) {
    $Available[$Tool] = Has-Command $Tool
    if ($Available[$Tool]) { Write-Result "OK" "$Tool encontrado" } else { Write-Result "FAIL" "$Tool não encontrado" }
}
$ComposeOk = $false
if ($Available.docker) { docker compose version *> $null; $ComposeOk = ($LASTEXITCODE -eq 0) }
if ($ComposeOk) { Write-Result "OK" "docker compose encontrado" } else { Write-Result "FAIL" "docker compose não encontrado ou indisponível" }
$GhAuthOk = $false
if ($Available.gh) { gh auth status *> $null; $GhAuthOk = ($LASTEXITCODE -eq 0) }
if ($GhAuthOk) { Write-Result "OK" "gh autenticado" } elseif ($Available.gh) { Write-Result "WARN" "gh encontrado, mas não autenticado" } else { Write-Result "WARN" "gh auth status indisponível" }
$TokenOk = -not [string]::IsNullOrWhiteSpace($env:GH_TOKEN) -or -not [string]::IsNullOrWhiteSpace($env:GITHUB_TOKEN)
if ($TokenOk) { Write-Result "OK" "GH_TOKEN/GITHUB_TOKEN disponível (valor oculto)" } else { Write-Result "WARN" "GH_TOKEN/GITHUB_TOKEN não definido" }
$DatabaseOk = -not [string]::IsNullOrWhiteSpace($env:DATABASE_URL)
if ($DatabaseOk) { Write-Result "OK" "DATABASE_URL disponível (valor oculto)" } else { Write-Result "FAIL" "DATABASE_URL não definido" }
$DockerGate = $Available.docker -and $ComposeOk -and $Available.node -and $Available.npm -and $Available.curl
$HostGate = $Available.dotnet -and $Available.psql -and $Available.node -and $Available.npm -and $Available.curl -and $DatabaseOk
$ActionsGate = $Available.gh -and ($GhAuthOk -or $TokenOk)
Write-Host "`nResumo: $Ok OK, $Warn WARN, $Fail FAIL"
Write-Host "Rota Docker local: $(if ($DockerGate) {'DISPONÍVEL'} else {'INDISPONÍVEL'})"
Write-Host "Rota local sem Docker: $(if ($HostGate) {'DISPONÍVEL'} else {'INDISPONÍVEL'})"
Write-Host "Rota GitHub Actions: $(if ($ActionsGate) {'DISPONÍVEL'} else {'INDISPONÍVEL'})"
if ($DockerGate -or $HostGate -or $ActionsGate) { Write-Host "OK: ao menos uma rota de gate real pode ser executada."; exit 0 }
Write-Host "FAIL: nenhuma rota de gate real pode ser executada neste ambiente."; exit 1
