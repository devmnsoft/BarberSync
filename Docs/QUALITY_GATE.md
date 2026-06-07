# Quality Gate BarberSync 20.0

## Como rodar

```powershell
pwsh -NoLogo -NoProfile -File .\Scripts\quality-gate.ps1
```

Opções úteis:

```powershell
pwsh -File .\Scripts\quality-gate.ps1 -SkipDockerBuild
pwsh -File .\Scripts\quality-gate.ps1 -WarmupSeconds 40
```

## O que o script valida

1. Pré-requisitos `dotnet` e `docker`.
2. `dotnet build BarberSync.sln`.
3. Builds individuais de API, AdminWeb, PublicWeb e KioskWeb.
4. `dotnet test BarberSync.sln --no-build` quando há projetos de teste.
5. `docker compose build --no-cache`.
6. `docker compose up -d` e `docker compose ps`.
7. Swagger, API, AdminApi, PublicApi e KioskApi.
8. Rotas visuais principais: Admin, Diagnostics, FullServiceFlow, Dashboard, Clientes, Agenda, Comandas, Estoque, PublicWeb e Kiosk.
9. Assets principais CSS/JS/SVG de Admin, PublicWeb e Kiosk, incluindo Diagnostics, FullServiceFlow e testes JS.
10. POST smoke de Copilot Admin, agendamento público, lead público, cliente Kiosk, pagamento mock e avaliação Kiosk.
11. Busca por URLs proibidas em arquivos browser-side.
12. Logs Docker com padrões críticos.
13. Geração de `Docs/quality-gate-last-run.md`.

## Resultado esperado

- OK em verde para validações aprovadas.
- Alertas em amarelo para pontos controlados.
- Falhas em vermelho para bloqueios críticos.
- Exit code diferente de zero se houver falha crítica.
