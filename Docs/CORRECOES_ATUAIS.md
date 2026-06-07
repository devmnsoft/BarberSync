# Correções atuais - BarberSync Demo Ready Fix & Validate 20.0

## Auditoria realizada

- Revisados API/Swagger/controladores/DI, incluindo `Program.cs`, `ServicesController`, `ProfessionalsController`, `DashboardController` e `KioskConfigController`.
- Revisados AdminWeb, `AdminController`, `AdminApiController`, layouts, partials, views principais, Diagnostics e FullServiceFlow.
- Revisados PublicWeb (`Program.cs`, `PublicApiController`, landing e `public.js`).
- Revisados KioskWeb (`Program.cs`, `KioskController`, `KioskApiController`, views e scripts `kiosk.js`/`kiosk-flow.js`).
- Revisados MobileApp (`package.json`, `App.js`, smoke test e estrutura existente).
- Revisados Docker Compose, Dockerfiles e `.dockerignore`.
- Busca browser-side por URLs proibidas em `Web/**/*.js`, `Web/**/*.cshtml` e `Web/**/*.html`: **sem ocorrências**.
- `dotnet`, `docker` e `pwsh` foram tentados, mas não existem neste container.
- JS crítico Admin/Public/Kiosk validado com `node --check`.

## Correções aplicadas

1. Atualizado `Scripts/quality-gate.ps1` para a versão-alvo 20.0.
2. Quality Gate passou a executar também builds individuais de API, AdminWeb, PublicWeb e KioskWeb quando o SDK .NET estiver disponível.
3. Quality Gate passou a validar mais proxies críticos, POSTs demo e assets de Diagnostics/FullServiceFlow/testes JS.
4. Diagnostics atualizado para 20.0, com validação ampliada de AdminApi, PublicApi, KioskApi, Swagger e assets.
5. Diagnostics ganhou botões explícitos para abrir FullServiceFlow, PublicWeb, Kiosk e Swagger.
6. AdminApi ganhou rota `/AdminApi/swagger` redirecionando para o contrato Swagger JSON proxy/fallback.
7. DemoStore recebeu `try/catch` na persistência local para evitar quebra quando localStorage estiver indisponível ou sem quota.
8. Documentação de readiness, checklist, roteiro, diagnostics, FullServiceFlow e Quality Gate atualizada para 20.0.

## Pendências controladas

- Reexecutar validação .NET/Docker completa em ambiente com SDK .NET, Docker e PowerShell.
- Validar visualmente console browser após subir Docker.
- Registrar o resultado real em `Docs/quality-gate-last-run.md`.
