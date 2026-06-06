# Quality Gate BarberSync

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
7. Rotas visuais Admin/Public/Kiosk.
8. Assets principais CSS/JS/SVG.
9. POST smoke de agendamento público e pagamento mock do totem.
10. Busca por URLs proibidas em arquivos browser-side.
11. Logs Docker com padrões críticos.
12. Relatório final em `Docs/quality-gate-last-run.md`.

## Como interpretar falhas

- **Pré-requisito falhou:** instale SDK .NET, Docker Engine/Desktop ou PowerShell.
- **Endpoint 404/500:** confirme rota MVC/API, fallback demo e proxy correto.
- **Asset 404:** confirme `wwwroot`, nomes de arquivo e publicação Docker.
- **URL proibida no front:** troque chamada direta por `/AdminApi`, `/PublicApi` ou `/KioskApi`.
- **Logs críticos:** investigue antes de qualquer demo comercial.

## Como corrigir endpoints

- Browser Admin deve chamar `/AdminApi/...`.
- Browser PublicWeb deve chamar `/PublicApi/...`.
- Browser Kiosk deve chamar `/KioskApi/...`.
- `ApiSettings:BaseUrl` é permitido somente no server-side MVC/Docker.

## Como validar Docker

```powershell
docker compose build --no-cache
docker compose up -d
docker compose ps
docker compose logs --tail=300
```

## Como validar browser

Abra com Ctrl+F5:

- `http://localhost:8080/swagger`
- `http://localhost:8081/Admin/Diagnostics`
- `http://localhost:8081/Admin/FullServiceFlow`
- `http://localhost:8082/`
- `http://localhost:8083/Kiosk/Services`

O console não deve exibir `ERR_NAME_NOT_RESOLVED`, `ReferenceError` do BarberSync, assets principais 404 ou chamadas para host interno Docker.
