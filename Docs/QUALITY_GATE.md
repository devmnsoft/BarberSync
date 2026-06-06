# Quality Gate BarberSync 19.0

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
3. `dotnet test BarberSync.sln --no-build` quando há projetos de teste.
4. `docker compose build --no-cache`.
5. `docker compose up -d` e `docker compose ps`.
6. Swagger, API, AdminApi, PublicApi e KioskApi.
7. Rotas visuais, incluindo `/Admin/DemoWizard`.
8. Assets principais CSS/JS/SVG, incluindo `admin-demo-wizard.js` e `admin-demo-wizard.css`.
9. POST smoke de agendamento público e pagamento mock do totem.
10. Busca por URLs proibidas em arquivos browser-side.
11. Logs Docker com padrões críticos.
12. Geração de `Docs/quality-gate-last-run.md`.

## Resultado esperado

- OK em verde para validações aprovadas.
- Alertas em amarelo para pontos controlados.
- Falhas em vermelho para bloqueios críticos.
- Exit code diferente de zero se houver falha crítica.
