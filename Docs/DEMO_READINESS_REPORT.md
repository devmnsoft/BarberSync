# BarberSync Demo Ready Stabilization 18.0 - Readiness Report

- **Última validação:** 2026-06-06 UTC
- **Status geral:** **Atenção controlada no ambiente do agente**. O repositório foi consolidado para demo, mas a validação executável completa de .NET/Docker não pôde ser concluída neste container porque `dotnet`, `docker` e `pwsh` não estão instalados.
- **Regra crítica:** nenhuma chamada browser-side em `Web/**/*.js`, `Web/**/*.cshtml` ou `Web/**/*.html` aponta para `http://api:8080`, `api:8080`, `localhost:8083/api` ou `8083/api`. O endereço interno Docker permanece permitido apenas em `ApiSettings:BaseUrl` server-side.

## Status por área

| Área | Status | Evidência / observação |
|---|---|---|
| API | Atenção | Endpoints alvo documentados no Quality Gate; execução bloqueada por ausência de SDK .NET/Docker no container. |
| Swagger | Atenção | `/swagger` e `/swagger/v1/swagger.json` são validados por `Scripts/quality-gate.ps1`; Admin também consulta `/AdminApi/swagger.json`. |
| AdminWeb | Revisado | Diagnostics 18.0, Dashboard 18.0, FullServiceFlow 18.0 e assets principais revisados. |
| PublicWeb | Revisado | `public.js` usa `/PublicApi/services`, `/PublicApi/professionals` e POST `/PublicApi/appointments`, com fallback local. |
| KioskWeb | Revisado | `kiosk.js`/`kiosk-flow.js` persistem sessão em `sessionStorage`, usam `/KioskApi/*` e fallback demo. |
| Mobile | OK | `npm install && npm test` executado em `MobileApp`; smoke test passou. |
| Docker | Atenção | `docker compose build --no-cache`, `up -d`, `ps` e logs estão no Quality Gate, mas Docker não existe neste container. |
| Proxies | Revisado | `/AdminApi`, `/PublicApi` e `/KioskApi` permanecem como fronteiras browser-side. |
| Assets | Revisado | Quality Gate valida CSS/JS/logos principais de Admin, Public e Kiosk. |
| FullServiceFlow | Revisado | Botão **Validar fluxo completo automaticamente** executa cliente → agenda → check-in → atendimento → comanda → pagamento → recibo → estoque → cashback → avaliação → dashboard. |
| DemoStore | Revisado | Mantém CRUD genérico, `barbersync.fullServiceFlow.v14` e funções do fluxo completo sem depender de banco real. |
| EventBus | Revisado | Inclui `on`, `off`, `emit`, `history`, `clearHistory`, wildcard e evento `flow:autoValidated`. |

## Auditoria realizada

1. Busca estática de URLs proibidas em arquivos expostos ao browser.
2. Tentativa de `dotnet build` geral e builds individuais de API/Admin/Public/Kiosk.
3. Tentativa de validação Docker e Quality Gate.
4. Validação sintática com `node --check` dos JS críticos.
5. Smoke check MobileApp com `npm install && npm test`.
6. Revisão manual de Diagnostics, FullServiceFlow, DemoStore, EventBus, PublicWeb, Kiosk e Quality Gate.

## Resultado real dos comandos neste container

| Comando | Resultado |
|---|---|
| `dotnet build` | Bloqueado: `dotnet: command not found`. |
| `dotnet test` | Bloqueado: `dotnet: command not found`. |
| Builds individuais `.csproj` | Bloqueados: `dotnet: command not found`. |
| `docker compose build --no-cache` | Bloqueado: `docker: command not found`. |
| `docker compose up -d` / `docker compose ps` / logs | Bloqueados: `docker: command not found`. |
| `pwsh -File ./Scripts/quality-gate.ps1` | Bloqueado: `pwsh: command not found`. |
| `node --check` JS crítico | OK. |
| `cd MobileApp && npm install && npm test` | OK: `Mobile smoke test passed`. |

## Como demonstrar

1. Subir a stack: `docker compose build --no-cache && docker compose up -d`.
2. Abrir Swagger: `http://localhost:8080/swagger`.
3. Abrir Admin: `http://localhost:8081/Admin`.
4. Abrir Diagnostics: `http://localhost:8081/Admin/Diagnostics` e clicar **Rodar diagnóstico**.
5. Abrir FullServiceFlow: `http://localhost:8081/Admin/FullServiceFlow` e clicar **Validar fluxo completo automaticamente**.
6. Conferir Dashboard: `http://localhost:8081/Admin/Dashboard`.
7. Abrir PublicWeb: `http://localhost:8082/`, preencher agendamento e conferir protocolo demo.
8. Abrir Kiosk: `http://localhost:8083/Kiosk/Services` e concluir o fluxo do totem.

## Como resetar a demo

- Admin Diagnostics: botão **Resetar DemoStore**.
- Dashboard: botão **Resetar DemoStore**.
- FullServiceFlow: botão **Reiniciar fluxo**.
- Kiosk: botão de reinício/cancelamento do fluxo quando disponível.
- Manualmente no browser: limpar `localStorage` e `sessionStorage` das origens `localhost:8081`, `localhost:8082` e `localhost:8083`.

## Como rodar o Quality Gate

```powershell
pwsh -NoLogo -NoProfile -File .\Scripts\quality-gate.ps1
```

O relatório de execução é salvo em `Docs/quality-gate-last-run.md` e o script retorna exit code diferente de zero se houver falha crítica.

## Pendências reais

- Executar a validação final em ambiente com SDK .NET, PowerShell e Docker instalados.
- Após a execução real, anexar o conteúdo atualizado de `Docs/quality-gate-last-run.md` à preparação da demo.
- Validar visualmente o console do browser com Ctrl+F5 nas URLs principais.
