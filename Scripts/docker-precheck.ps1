param(
    [string]$DotNetVersion = "8.0",
    [string]$RegistryHost = "mcr.microsoft.com"
)

$ErrorActionPreference = "Continue"

function Write-Section {
    param([string]$Message)
    Write-Host ""
    Write-Host "==================================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "==================================================" -ForegroundColor Cyan
}

function Fail-NetworkRegistry {
    param([string]$Message)
    Write-Host ""
    Write-Host "FALHA: $Message" -ForegroundColor Red
    Write-Host "Diagnostico: o pre-check falhou ao acessar o registry/base images do Docker." -ForegroundColor Yellow
    Write-Host "Este problema e de rede, proxy, firewall, DNS, Docker Desktop ou indisponibilidade do registry; nao e erro de C#, Razor ou codigo do BarberSync." -ForegroundColor Yellow
    Write-Host "Sugestoes: reinicie o Docker Desktop, verifique internet/proxy/firewall/DNS, teste sem VPN e abra https://mcr.microsoft.com no navegador." -ForegroundColor Yellow
    exit 1
}

$sdkImage = "mcr.microsoft.com/dotnet/sdk:$DotNetVersion"
$aspnetImage = "mcr.microsoft.com/dotnet/aspnet:$DotNetVersion"

Write-Section "BarberSync Docker pre-check"
Write-Host "Versao .NET Docker: $DotNetVersion"
Write-Host "Registry: $RegistryHost"

Write-Section "1. Testando conectividade HTTPS com ${RegistryHost}:443"
$connection = Test-NetConnection $RegistryHost -Port 443 -WarningAction SilentlyContinue
if (-not $connection.TcpTestSucceeded) {
    Fail-NetworkRegistry "Nao foi possivel conectar em $RegistryHost na porta 443."
}
Write-Host "OK: conexao TCP com ${RegistryHost}:443 funcionando." -ForegroundColor Green

Write-Section "2. Verificando Docker CLI"
docker version
if ($LASTEXITCODE -ne 0) {
    Fail-NetworkRegistry "Docker CLI/daemon nao esta funcional. Confirme se o Docker Desktop esta iniciado e logado quando aplicavel."
}
Write-Host "OK: Docker CLI/daemon respondeu." -ForegroundColor Green

Write-Section "3. Baixando imagem SDK: $sdkImage"
docker pull $sdkImage
if ($LASTEXITCODE -ne 0) {
    Fail-NetworkRegistry "Falha ao executar docker pull $sdkImage."
}
Write-Host "OK: imagem SDK disponivel localmente." -ForegroundColor Green

Write-Section "4. Baixando imagem ASP.NET runtime: $aspnetImage"
docker pull $aspnetImage
if ($LASTEXITCODE -ne 0) {
    Fail-NetworkRegistry "Falha ao executar docker pull $aspnetImage."
}
Write-Host "OK: imagem ASP.NET runtime disponivel localmente." -ForegroundColor Green

Write-Section "Pre-check concluido com sucesso"
Write-Host "As imagens base foram resolvidas e baixadas. Agora rode:" -ForegroundColor Green
Write-Host "docker compose build --no-cache --progress=plain"
Write-Host "docker compose up -d --build"
