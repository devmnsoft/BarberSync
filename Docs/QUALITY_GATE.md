# BarberSync Quality Gate + Demo Ready 16.0

## Como rodar

```powershell
.\Scripts\quality-gate.ps1
```

Parâmetros úteis:

- `-SkipDotnetBuild`: pula `dotnet build` quando a solução já foi compilada.
- `-SkipDockerBuild`: pula `docker compose build` quando as imagens já estão atualizadas.
- `-WarmupSeconds 30`: aumenta o tempo de aquecimento dos containers.

## O que valida

1. `dotnet build`.
2. `docker compose build`.
3. `docker compose up -d`.
4. `docker compose ps`.
5. Swagger UI e `swagger/v1/swagger.json`.
6. Endpoints API `/api/services`, `/api/professionals` e kiosk.
7. Proxies MVC `/AdminApi`, `/PublicApi` e `/KioskApi`.
8. Assets principais de AdminWeb, PublicWeb e KioskWeb.
9. Ausência de chamadas browser para hosts internos Docker em arquivos web.
10. Logs Docker sem padrões críticos.

O relatório da última execução é salvo em `Docs/quality-gate-last-run.md`.

## Como interpretar falhas

- **Endpoint 404/500**: confira rota, controller e fallback demo do respectivo proxy.
- **Asset 404**: confira caminho em `wwwroot`, layout e Dockerfile/publicação.
- **Browser URL proibida**: substitua chamadas diretas por `/AdminApi`, `/PublicApi` ou `/KioskApi`.
- **Logs críticos**: revise exceções, falhas de DI, Swagger quebrado, assets 404 e erros de proxy.

## Como corrigir endpoints

- API direta deve responder em `http://localhost:8080`.
- AdminWeb deve usar `/AdminApi/*` no browser.
- PublicWeb deve usar `/PublicApi/*` no browser.
- KioskWeb deve usar `/KioskApi/*` no browser.
- `ApiSettings:BaseUrl` é server-side e pode apontar para o host interno do Docker Compose.

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
- `http://localhost:8081/Admin`
- `http://localhost:8081/Admin/Diagnostics`
- `http://localhost:8081/Admin/FullServiceFlow`
- `http://localhost:8082/`
- `http://localhost:8083/Kiosk/Services`
