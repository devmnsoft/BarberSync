# BarberSync Quality Gate - Última execução

- Executado em UTC: 2026-06-05 23:11:30
- Resultado final: REPROVADO
- Passos executados: 5
- Falhas críticas: 3
- Alertas: 0

| Validação | Alvo | Status | Detalhe |
|---|---|---|---|
| PowerShell disponível | `pwsh -NoLogo -NoProfile -File Scripts/quality-gate.ps1` | FAIL | `/bin/bash: line 1: pwsh: command not found` neste container. |
| dotnet disponível | `dotnet --version` | FAIL | `/bin/bash: line 1: dotnet: command not found` neste container. |
| Docker disponível | `docker --version && docker compose version` | FAIL | `/bin/bash: line 1: docker: command not found` neste container. |
| Mobile smoke | `cd MobileApp && npm install && npm test` | OK | `Mobile smoke test passed.` |
| Front syntax/browser URL audit | `node --check` em `Web/**/wwwroot/js/*.js` e `rg` de URLs proibidas no front | OK | Sem erro de sintaxe JS e sem chamadas diretas browser para host interno Docker. |

## Falhas encontradas

- Ambiente local do agente não possui PowerShell (`pwsh`), .NET SDK (`dotnet`) nem Docker (`docker`).
- Por limitação de ambiente, não foi possível executar build .NET, testes .NET, Docker Compose, endpoints HTTP reais, Swagger real e logs Docker.

## Como corrigir

- Executar `pwsh -NoLogo -NoProfile -File Scripts/quality-gate.ps1` em uma máquina de demo/CI com PowerShell, .NET SDK e Docker instalados.
- Se endpoints/proxies falharem: confira `docker compose up -d`, health da API e fallbacks MVC.
- Se assets falharem: confira publicação dos diretórios `wwwroot` e referências nos layouts.
- Se aparecer URL proibida no front: remover chamadas diretas a `http://api:8080`, `api:8080`, `localhost:8083/api` ou `8083/api` de `.js`, `.cshtml` e `.html`.

## Resultado final

**REPROVADO no container atual por limitação de ferramentas**, com validações estáticas e Mobile smoke aprovados. Reexecutar o Quality Gate completo no ambiente com .NET/Docker/PowerShell antes da apresentação comercial.
