# Correções Atuais — BarberSync Stable Demo Release 13.0

## Diagnóstico inicial

- Projeto auditado como **BarberSync**; não houve consulta nem alteração em módulos InovaGED/GED.
- `dotnet`, `docker` e `pwsh` não estão instalados neste container, então as validações dependentes dessas CLIs ficaram bloqueadas pelo ambiente.
- A busca por URLs proibidas encontrou `http://api:8080` apenas em configuração Docker/server-side e documentação; não há chamada browser-side direta para `http://api:8080` em JS/CSHTML/HTML da aplicação.
- `Program.cs` dos três MVCs mantém `UseStaticFiles()` antes de `UseRouting()`.
- API registra `IConfigurationService` e expõe `/api/kiosk/services`, `/api/kiosk/professionals` e `/api/kiosk/status` com fallback demo.
- Proxies MVC usam `IHttpClientFactory`, `IConfiguration`, `ILogger`, `ApiSettings:BaseUrl` server-side e fallback com HTTP 200.

## Ajustes realizados nesta etapa

- Versão visual atualizada para **Stable Demo Release 13.0** em Admin e PublicWeb.
- Proxies Admin/Public/Kiosk passaram a usar o client nomeado `BarberSyncApi`, mantendo comunicação server-side com a API.
- PublicWeb agora exibe o protocolo retornado por `/PublicApi/appointments` quando disponível e mantém protocolo local como fallback.
- JavaScript principal de Admin, PublicWeb e Kiosk foi validado com `node --check`.

## Validações pendentes por ambiente

Executar em ambiente com .NET SDK, Docker e PowerShell:

```bash
dotnet build
docker compose build --no-cache
docker compose up -d
docker compose ps
```

```powershell
Invoke-WebRequest http://localhost:8081/AdminApi/dashboard
Invoke-WebRequest http://localhost:8082/PublicApi/services
Invoke-WebRequest "http://localhost:8083/KioskApi/services?deviceCode=KIOSK-DEMO-001"
Invoke-WebRequest "http://localhost:8080/api/kiosk/services?deviceCode=KIOSK-DEMO-001"
```
