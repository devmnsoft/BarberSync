# BarberSync Quality Gate - Última execução

- Versão alvo: BarberSync Demo Ready Fix & Validate 20.0
- Executado em UTC: 2026-06-07
- Resultado final: ATENÇÃO CONTROLADA NO AMBIENTE DO AGENTE
- Observação: a execução completa depende de SDK .NET, Docker/Compose e PowerShell. Neste container os comandos `dotnet`, `docker` e `pwsh` não estão disponíveis.

| Validação | Alvo | Status | Detalhe |
|---|---|---|---|
| Auditoria URL proibida no front | `Web/**/*.{js,cshtml,html}` | OK | Sem chamadas diretas para host interno Docker/API em arquivos expostos ao navegador. |
| JavaScript Admin DemoStore | `node --check Web/BarberSync.AdminWeb/wwwroot/js/admin-demo-store.js` | OK | Sintaxe válida. |
| JavaScript Admin EventBus | `node --check Web/BarberSync.AdminWeb/wwwroot/js/admin-event-bus.js` | OK | Sintaxe válida. |
| JavaScript Admin FullServiceFlow | `node --check Web/BarberSync.AdminWeb/wwwroot/js/admin-full-service-flow.js` | OK | Sintaxe válida. |
| JavaScript Admin Diagnostics | `node --check Web/BarberSync.AdminWeb/wwwroot/js/admin-diagnostics.js` | OK | Sintaxe válida. |
| JavaScript PublicWeb | `node --check Web/BarberSync.PublicWeb/wwwroot/js/public.js` | OK | Sintaxe válida. |
| JavaScript Kiosk | `node --check Web/BarberSync.KioskWeb/wwwroot/js/kiosk.js` e `kiosk-flow.js` | OK | Sintaxe válida. |
| dotnet build | `dotnet build` | BLOQUEADO | `/bin/bash: dotnet: command not found`. |
| dotnet test | `dotnet test` | BLOQUEADO | SDK .NET indisponível neste container. |
| docker compose | `docker compose build --no-cache`, `docker compose up -d`, `docker compose ps` | BLOQUEADO | `/bin/bash: docker: command not found`. |
| PowerShell Quality Gate | `pwsh -File ./Scripts/quality-gate.ps1` | BLOQUEADO | `/bin/bash: pwsh: command not found`. |

## Próxima execução obrigatória

Em ambiente com ferramentas instaladas, executar:

```powershell
dotnet build
dotnet test
docker compose build --no-cache
docker compose up -d
docker compose ps
pwsh -NoLogo -NoProfile -File .\Scripts\quality-gate.ps1
docker compose logs --tail=300 | Select-String -Pattern "ERR|ERROR|Exception|fail|Failed|FATAL|Unhandled|500|404|409|ERR_NAME_NOT_RESOLVED"
```
