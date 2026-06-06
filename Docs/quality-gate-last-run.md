# BarberSync Quality Gate - Última execução

- Versão alvo: BarberSync Demo Ready Guided Release 19.0
- Executado em UTC: 2026-06-06
- Resultado final: REPROVADO NO AMBIENTE DO AGENTE POR PRÉ-REQUISITOS AUSENTES
- Falhas críticas ambientais: SDK .NET ausente, Docker ausente, PowerShell (`pwsh`) ausente.
- Validações estáticas/JS/Mobile executadas com sucesso.

| Validação | Alvo | Status | Detalhe |
|---|---|---|---|
| Pré-requisito dotnet | `dotnet` | FAIL | `/bin/bash: dotnet: command not found` |
| Pré-requisito docker | `docker` | FAIL | `/bin/bash: docker: command not found` |
| Pré-requisito pwsh | `pwsh` | FAIL | `/bin/bash: pwsh: command not found` |
| Auditoria URL proibida no front | `Web/**/*.{js,cshtml,html}` | OK | Sem chamadas diretas para `http://api:8080`, `api:8080`, `localhost:8083/api` ou `8083/api`. |
| JS syntax check | JS crítico Admin/Public/Kiosk | OK | `node --check` passou nos arquivos críticos. |
| Mobile smoke | `MobileApp` | OK | `npm install && npm test` passou com `Mobile smoke test passed`. |

## Falhas encontradas

- O Quality Gate completo precisa ser executado em ambiente com SDK .NET, Docker e PowerShell instalados.

## Como corrigir

- Instale SDK .NET compatível, Docker Engine/Desktop e PowerShell 7+.
- Rode `pwsh -NoLogo -NoProfile -File .\Scripts\quality-gate.ps1` na raiz do repositório.
- Corrija qualquer endpoint, asset, proxy ou log crítico reportado pelo script.
