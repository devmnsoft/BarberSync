# Correções atuais - BarberSync Demo Ready Stabilization 18.0

## Auditoria inicial

- Busca browser-side por `http://api:8080`, `api:8080`, `localhost:8083/api` e `8083/api` em `Web/**/*.js`, `Web/**/*.cshtml` e `Web/**/*.html`: sem ocorrências.
- `dotnet build`, `dotnet test`, Docker e PowerShell foram tentados, mas o container não possui `dotnet`, `docker` nem `pwsh`.
- `MobileApp`: `npm install && npm test` passou com `Mobile smoke test passed`.
- JS crítico de Admin/Public/Kiosk validado com `node --check`.

## Correções aplicadas

1. `Scripts/quality-gate.ps1` revisado para BarberSync Demo Ready Stabilization 18.0, com validação explícita de pré-requisitos, `docker compose build --no-cache`, endpoints, rotas, assets, POST smoke, auditoria de URLs proibidas e logs críticos.
2. Diagnostics Admin atualizado para 18.0 com painel de documentação/Quality Gate, links operacionais, resultados visuais e estado inicial não vazio.
3. FullServiceFlow atualizado com botão **Validar fluxo completo automaticamente**.
4. EventBus passou a classificar `flow:autoValidated` como evento de sucesso.
5. Dashboard passou a escutar eventos do FullServiceFlow para refletir execução automática e conclusão.
6. Documentação de readiness e qualidade atualizada com comandos, critérios e limitações reais do ambiente.

## Pendências controladas

- Rodar build/test/Docker/Quality Gate em máquina com ferramentas instaladas.
- Validar console browser real após subir Docker.
