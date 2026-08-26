# Execução local ponta a ponta

Este fluxo é exclusivo para **Development**. Ele preserva autenticação, autorização, `ValidateOnStart` e dados reais do PostgreSQL. Não use as credenciais locais em produção, não versione segredos e não substitua as validações de segurança por valores padrão.

## Pré-requisitos

- .NET SDK indicado pelo repositório, PostgreSQL 14+ e `psql` no `PATH`.
- Banco vazio ou já inicializado (os scripts são aditivos e idempotentes).
- Certificado local: `dotnet dev-certs https --trust`.

## Fluxo único

```powershell
cd C:\MNSOFT\BarberSync
$env:ASPNETCORE_ENVIRONMENT = "Development"

.\Scripts\setup-api-local-dev.ps1 -ConnectionString "Host=localhost;Port=5432;Database=barbersync;Username=postgres;Password=SUA_SENHA"
.\Scripts\setup-web-local-dev.ps1 -ApiBaseUrl "https://localhost:7088" -KioskDeviceCode "KIOSK-LOCAL-001"
.\Scripts\apply-local-database.ps1 -ValidateIdempotency
.\Scripts\seed-local-dev.ps1
.\Scripts\run-local-stack.ps1 -NoBrowser
# Em outro terminal:
.\Scripts\check-local-stack.ps1
```

`apply-local-database.ps1` e `seed-local-dev.ps1` leem `ConnectionStrings:DefaultConnection` dos user-secrets da API quando o parâmetro é omitido. Senhas e connection strings nunca são impressas. O hash da senha local é produzido pelo formato ASP.NET Identity V3 usado pelo backend, e não por SQL ou por um hash inventado.

## URLs oficiais

| Aplicação | HTTPS | HTTP alternativo |
|---|---|---|
| API / health | `https://localhost:7088/health` | `http://localhost:5080/health` |
| AdminWeb / login | `https://localhost:7188/Account/Login` | `http://localhost:5081/Account/Login` |
| PublicWeb | `https://localhost:7288` | `http://localhost:5082` |
| KioskWeb | `https://localhost:7388/Kiosk` | `http://localhost:5083/Kiosk` |

A chave canônica dos três gateways é `ApiSettings:BaseUrl`. O Kiosk obtém sua identidade exclusivamente do user-secret `Kiosk:DeviceCode`; o runner não injeta código de dispositivo por query string nem mantém fallback implícito.

## Fluxo validado em máquina com .NET SDK + PostgreSQL

Em uma máquina que tenha PowerShell 7, .NET SDK e PostgreSQL/`psql`, execute cada gate abaixo na ordem. As duas execuções de schema e seed são intencionais e validam a idempotência real antes de iniciar os processos:

```powershell
dotnet restore .\BarberSync.sln
dotnet build .\BarberSync.sln --configuration Debug
dotnet build .\BarberSync.sln --configuration Release
.\Scripts\apply-local-database.ps1
.\Scripts\apply-local-database.ps1
.\Scripts\seed-local-dev.ps1
.\Scripts\seed-local-dev.ps1
.\Scripts\run-local-stack.ps1 -NoBrowser
# Em outro terminal:
.\Scripts\check-local-stack.ps1
```

O runner só abre o navegador depois que os endpoints básicos respondem, informa o PID de cada aplicação e grava stdout/stderr em `artifacts/local-stack/logs`. Para validação parcial, use `-ApiOnly` ou `-WebOnly`.

## Dados locais

| Item | Valor |
|---|---|
| Tenant | BarberSync Local (`barbersync-local`) |
| Branch | Unidade Local (`LOCAL`) |
| Admin | `admin@barbersync.local` / `Dev@123456` |
| Caixa | `caixa@barbersync.local` / `Dev@123456` |
| Profissional | `profissional@barbersync.local` / `Dev@123456` |
| Cliente | `cliente@barbersync.local` |
| Totem | `KIOSK-LOCAL-001` |

Esses valores existem apenas no seed local explicitamente marcado como Development. O script redefine somente as três contas locais de UUID reservado, não remove dados e não toca no `production_readiness_seed.sql`.

## Comportamento degradado

- Sem API, Admin informa indisponibilidade no login e Public/Kiosk exibem erro operacional amigável.
- Sem `Kiosk:DeviceCode`, o Kiosk mostra **Totem não configurado**, sem identidade implícita.
- Um `401` no Admin informa que a sessão expirou ou que as credenciais são inválidas; `503` informa que a API/banco deve ser iniciado.

Consulte [LOCAL_TROUBLESHOOTING.md](LOCAL_TROUBLESHOOTING.md) para diagnósticos detalhados.
