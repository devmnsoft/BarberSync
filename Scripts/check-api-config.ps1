[CmdletBinding()]
param(
  [string]$PgHost = $(if ($env:PGHOST) { $env:PGHOST } else { 'localhost' }),
  [int]$PgPort = $(if ($env:PGPORT) { [int]$env:PGPORT } else { 5432 })
)

$Root = Split-Path -Parent $PSScriptRoot
$failed = $false
function Report([bool]$Ok, [string]$Message) {
  $script:failed = $script:failed -or -not $Ok
  Write-Host "[$(if ($Ok) {'OK'} else {'ERRO'})] $Message" -ForegroundColor $(if ($Ok) {'Green'} else {'Red'})
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
Report ($null -ne $dotnet) $(if ($dotnet) { ".NET SDK $(& dotnet --version) encontrado" } else { '.NET SDK não encontrado' })
Report (Test-Path (Join-Path $Root 'ScriptsSQL/script_completo.sql')) 'ScriptsSQL/script_completo.sql encontrado'

$standard = [Environment]::GetEnvironmentVariable('ConnectionStrings__DefaultConnection')
$prefixed = [Environment]::GetEnvironmentVariable('BARBERSYNC_ConnectionStrings__DefaultConnection')
$configured = -not [string]::IsNullOrWhiteSpace($prefixed) -or -not [string]::IsNullOrWhiteSpace($standard)
Report $configured 'ConnectionStrings:DefaultConnection configurada (valor protegido)'
Write-Host "[$(if ($prefixed) {'OK'} else {'INFO'})] BARBERSYNC_ConnectionStrings__DefaultConnection $(if ($prefixed) {'configurada' } else {'não configurada'})"

$postgres = Test-NetConnection -ComputerName $PgHost -Port $PgPort -InformationLevel Quiet -WarningAction SilentlyContinue
Report $postgres "PostgreSQL acessível em ${PgHost}:${PgPort}"
foreach ($endpoint in @(
  @{ Name = 'API'; Port = 5080 }, @{ Name = 'Admin'; Port = 5081 },
  @{ Name = 'PublicWeb'; Port = 5082 }, @{ Name = 'Totem'; Port = 5083 })) {
  $busy = Test-NetConnection -ComputerName localhost -Port $endpoint.Port -InformationLevel Quiet -WarningAction SilentlyContinue
  Write-Host "[INFO] Porta $($endpoint.Port) ($($endpoint.Name)): $(if ($busy) {'em uso'} else {'livre'})"
}

if ($failed) { Write-Error 'Diagnóstico encontrou itens pendentes.'; exit 1 }
Write-Host '[OK] API pronta para iniciar' -ForegroundColor Green
