# BarberSync Demo Ready Guided Release 19.0 - Readiness Report

- **Última validação:** 2026-06-06 UTC.
- **Status geral:** **Atenção controlada no ambiente do agente**. A consolidação de código, JS, Mobile, documentação, Quality Gate e Demo Wizard foi realizada. A validação .NET/Docker completa não pôde ser executada neste container porque `dotnet`, `docker` e `pwsh` não estão instalados.
- **Regra crítica:** a busca browser-side em `Web/**/*.js`, `Web/**/*.cshtml` e `Web/**/*.html` não encontrou chamadas para `http://api:8080`, `api:8080`, `localhost:8083/api` ou `8083/api`. O host interno Docker permanece somente em configuração server-side.

## Status por área

| Área | Status | Evidência / observação |
|---|---|---|
| API | Atenção | Endpoints `/api/services`, `/api/professionals`, `/api/kiosk/services`, `/api/kiosk/professionals` e `/api/dashboard/summary` constam no Quality Gate; execução bloqueada por ausência de SDK .NET/Docker. |
| Swagger | Atenção | `/swagger` e `/swagger/v1/swagger.json` são validados por `Scripts/quality-gate.ps1`; execução real depende de Docker/.NET. |
| Admin | Revisado | Rotas `/Admin`, `/Admin/Dashboard`, `/Admin/Diagnostics`, `/Admin/DemoWizard` e `/Admin/FullServiceFlow` existem no controlador MVC. |
| PublicWeb | Revisado | Fluxo usa `/PublicApi/services`, `/PublicApi/professionals` e `/PublicApi/appointments`, com fallback visual. |
| Kiosk | Revisado | Fluxo usa `/KioskApi/*`, `sessionStorage`, botões de navegação e fallback visual. |
| Mobile | OK | `npm install && npm test` executado em `MobileApp`; smoke test passou. |
| Docker | Atenção | Comandos obrigatórios foram tentados e falharam por `docker: command not found` neste container. |
| Proxies | Revisado | `/AdminApi`, `/PublicApi` e `/KioskApi` são a fronteira browser-side. |
| Assets | Revisado | Quality Gate valida CSS/JS/logos principais, incluindo `admin-demo-wizard.js` e `admin-demo-wizard.css`. |
| FullServiceFlow | Revisado | Fluxo automático cria cliente, agenda, check-in, atendimento, comanda, pagamento, recibo, estoque, cashback, avaliação e evento `flow:autoValidated`. |
| DemoWizard | Criado | Nova tela `/Admin/DemoWizard` com 15 etapas, estado persistido e menu em Sistema. |
| DemoStore | Revisado | CRUD genérico e funções do FullServiceFlow usam fallback/localStorage; chave do fluxo `barbersync.fullServiceFlow.v14`. |
| EventBus | Revisado | `on`, `off`, `emit`, `history`, `clearHistory`, wildcard e eventos do FullServiceFlow disponíveis. |

## Pendências reais

- Executar `dotnet build`, `dotnet test`, `docker compose build --no-cache`, `docker compose up -d`, `docker compose ps` e `pwsh -File .\Scripts\quality-gate.ps1` em ambiente com ferramentas instaladas.
- Fazer validação visual com Ctrl+F5 nas URLs principais e conferir console browser.
- Atualizar `Docs/quality-gate-last-run.md` com o resultado real do ambiente Docker.

## Como demonstrar

1. Subir a stack.
2. Abrir `http://localhost:8080/swagger`.
3. Abrir `http://localhost:8081/Admin/DemoWizard` e seguir as 15 etapas.
4. Rodar Diagnostics.
5. Executar FullServiceFlow automático.
6. Conferir Dashboard, Cliente 360, Agenda, Comandas, Estoque e Avaliações.
7. Agendar no PublicWeb.
8. Concluir autoatendimento no Kiosk.

## Como resetar demo

- Diagnostics: **Resetar DemoStore**.
- Demo Wizard: **Resetar progresso**.
- FullServiceFlow: **Reiniciar fluxo**.
- Browser: limpar `localStorage`/`sessionStorage` das origens `localhost:8081`, `localhost:8082` e `localhost:8083`.

## Como rodar Quality Gate

```powershell
pwsh -NoLogo -NoProfile -File .\Scripts\quality-gate.ps1
```
