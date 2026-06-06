# Correções atuais - BarberSync Demo Ready Guided Release 19.0

## Auditoria realizada

- Revisados API/Swagger/controladores/proxies por inspeção estática dos arquivos principais.
- Revisados AdminWeb, PublicWeb, KioskWeb, MobileApp, Docker Compose e Quality Gate.
- Busca browser-side por URLs proibidas em `Web/**/*.js`, `Web/**/*.cshtml` e `Web/**/*.html`: **sem ocorrências**.
- `dotnet`, `docker` e `pwsh` foram tentados, mas não existem neste container.
- `MobileApp`: `npm install && npm test` passou.
- JS crítico Admin/Public/Kiosk validado com `node --check`.

## Correções aplicadas

1. Criada rota MVC `/Admin/DemoWizard`.
2. Criada tela `Views/Admin/DemoWizard.cshtml` com 15 etapas guiadas.
3. Criados `admin-demo-wizard.js` e `admin-demo-wizard.css`.
4. Adicionado menu **Sistema > Demo Wizard**.
5. Layout Admin passou a carregar CSS/JS do Demo Wizard.
6. Dashboard ganhou atalho **Abrir Demo Wizard** no painel Status da Demonstração.
7. `Scripts/quality-gate.ps1` atualizado para Guided Release 19.0 e para validar rota/assets do Demo Wizard.
8. Documentação de readiness, checklist, roteiro, diagnostics, FullServiceFlow e Quality Gate atualizada.

## Pendências controladas

- Reexecutar validação .NET/Docker completa em ambiente com SDK .NET, Docker e PowerShell.
- Validar visualmente console browser após subir Docker.
