# BarberSync Demo Ready Fix & Validate 20.0 - Readiness Report

- **Última validação:** 2026-06-07 UTC.
- **Status geral:** **Atenção controlada no ambiente do agente**. A auditoria 20.0 consolidou API, AdminWeb, PublicWeb, KioskWeb, MobileApp, Docker Compose, proxies MVC, Diagnostics, Quality Gate, DemoStore, EventBus e FullServiceFlow. A validação .NET/Docker completa não pôde ser concluída neste container porque `dotnet`, `docker` e `pwsh` não estão instalados.
- **Regra crítica:** a busca browser-side em `Web/**/*.js`, `Web/**/*.cshtml` e `Web/**/*.html` não encontrou chamadas para `http://api:8080`, `api:8080`, `localhost:8083/api` ou `8083/api`. O host interno Docker permanece somente em configuração server-side (`appsettings.Docker.json`/`docker-compose.yml`).

## Status por área

| Área | Status | Evidência / observação |
|---|---|---|
| API | Revisado | `Program.cs` registra Swagger, controllers, CORS, health checks, DI de Application/Infrastructure e `IConfigurationService`. Endpoints `/api/services`, `/api/professionals`, `/api/kiosk/services`, `/api/kiosk/professionals` e `/api/dashboard/summary` estão cobertos pelo Quality Gate. |
| Swagger | Revisado | `/swagger` e `/swagger/v1/swagger.json` são validados pelo Quality Gate; o Admin também expõe `/AdminApi/swagger.json` com fallback OpenAPI demo. |
| Admin | Revisado | Rotas `/Admin`, `/Admin/Dashboard`, `/Admin/Diagnostics`, `/Admin/FullServiceFlow`, `/Admin/Clients`, `/Admin/Appointments`, `/Admin/ServiceOrders` e `/Admin/Stock` permanecem no roteiro de validação. |
| PublicWeb | Revisado | Landing/agendamento público usa `/PublicApi/services`, `/PublicApi/professionals`, `/PublicApi/appointments` e `/PublicApi/leads`, com fallback demo e sem host Docker no navegador. |
| Kiosk | Revisado | Fluxo usa `/KioskApi/*`, `sessionStorage`, fallback visual e endpoints demo que não retornam 409 para consultas. |
| Mobile | Revisado | `MobileApp/package.json`, `App.js`, smoke test e estrutura mobile foram inspecionados; validação automatizada completa depende do ambiente Node/Expo local. |
| Docker | Atenção | `docker compose build --no-cache`, `docker compose up -d` e `docker compose ps` devem ser reexecutados em host com Docker; neste container `docker` não existe. |
| Proxies | Revisado | Browser chama somente `/AdminApi`, `/PublicApi` e `/KioskApi`; `ApiSettings:BaseUrl` é server-side. |
| Assets | Revisado | Quality Gate valida CSS/JS/logos principais de Admin, PublicWeb e Kiosk, incluindo Diagnostics, FullServiceFlow e testes JS. |
| Diagnostics | Atualizado | `/Admin/Diagnostics` mostra cards de API, Swagger, proxies, assets, DemoStore, EventBus, localStorage, FullServiceFlow, PublicWeb, Kiosk e Docker/Quality Gate; inclui botões de diagnóstico, reset, exportação e abertura das principais telas. |
| FullServiceFlow | Revisado | Fluxo manual e automático cobrem cliente, agendamento, check-in, atendimento, comanda, serviço, produto, pagamento, recibo, estoque, cashback, avaliação e evento `flow:autoValidated`. |
| DemoStore | Atualizado | CRUD genérico e funções do FullServiceFlow usam fallback/localStorage; gravação agora possui `try/catch`; chave do fluxo `barbersync.fullServiceFlow.v14`. |
| EventBus | Revisado | `on`, `off`, `emit`, `history`, `clearHistory` e eventos do FullServiceFlow disponíveis para Dashboard e Diagnostics. |

## Pendências reais

- Instalar SDK .NET, Docker/Compose e PowerShell no ambiente de validação e executar a validação final completa.
- Atualizar `Docs/quality-gate-last-run.md` com execução real em ambiente Docker.
- Fazer validação visual com Ctrl+F5 nas URLs principais e conferir console browser.

## Como demonstrar

1. Rodar `docker compose build --no-cache`.
2. Rodar `docker compose up -d`.
3. Abrir `http://localhost:8080/swagger`.
4. Abrir `http://localhost:8081/Admin/Diagnostics` e clicar **Rodar diagnóstico**.
5. Abrir `http://localhost:8081/Admin/FullServiceFlow` e clicar **Rodar fluxo automático de teste**.
6. Conferir Dashboard, Cliente 360, Agenda, Comandas, Estoque e Avaliações.
7. Agendar no PublicWeb em `http://localhost:8082/`.
8. Concluir autoatendimento no Kiosk em `http://localhost:8083/Kiosk/Services`.

## Como resetar demo

- Diagnostics: **Resetar DemoStore**.
- FullServiceFlow: **Reiniciar fluxo**.
- Browser: limpar `localStorage`/`sessionStorage` das origens `localhost:8081`, `localhost:8082` e `localhost:8083`.

## Como rodar Quality Gate

```powershell
pwsh -NoLogo -NoProfile -File .\Scripts\quality-gate.ps1
```

O relatório final será salvo em `Docs/quality-gate-last-run.md`.
