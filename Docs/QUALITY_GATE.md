# BarberSync Quality Gate + Demo Ready 16.0

## Como rodar

No Windows PowerShell, a partir da raiz do repositório:

```powershell
.\Scripts\quality-gate.ps1
```

Opções úteis:

```powershell
.\Scripts\quality-gate.ps1 -SkipDockerBuild
.\Scripts\quality-gate.ps1 -SkipDotnetBuild -WarmupSeconds 35
```

## O que o script valida

1. `dotnet build`.
2. `docker compose build`.
3. `docker compose up -d`.
4. `docker compose ps`.
5. Endpoints principais da API, AdminApi, PublicApi e KioskApi.
6. Static files principais de AdminWeb, PublicWeb e KioskWeb.
7. Ausência de chamadas browser-side para o host interno Docker da API.
8. Logs recentes do Docker sem padrões críticos: `ERR`, `ERROR`, `Exception`, `500`, `404`, `409`, `ERR_NAME_NOT_RESOLVED`.
9. Relatório final com falha explícita quando qualquer URL não retorna HTTP 200.

## Como interpretar falhas

- **Falha em build .NET**: execute `dotnet build` isoladamente e corrija erros de compilação.
- **Falha em Docker build**: execute `docker compose build --no-cache` para reproduzir sem cache.
- **Endpoint diferente de 200**: abra a URL indicada no relatório e valide se o controller existe ou se o proxy MVC possui fallback demo.
- **Asset 404**: confirme se o arquivo existe em `wwwroot` e se o layout referencia o caminho correto.
- **URL proibida no front**: substituir por `/AdminApi`, `/PublicApi` ou `/KioskApi` conforme o web app.
- **Logs críticos**: rode `docker compose logs --tail=300` e corrija exceções reais; warnings controlados de fallback demo não devem bloquear a apresentação.

## Como corrigir endpoints

- API direta: criar/ajustar controller em `Backend/Presentation/BarberSync.Api/Controllers`.
- Admin: preferir proxy em `Web/BarberSync.AdminWeb/Controllers/AdminApiController.cs`.
- PublicWeb: preferir proxy em `Web/BarberSync.PublicWeb/Controllers/PublicApiController.cs`.
- KioskWeb: preferir proxy em `Web/BarberSync.KioskWeb/Controllers/KioskApiController.cs`.

## Como validar Docker

```powershell
docker compose build --no-cache
docker compose up -d
docker compose ps
docker compose logs --tail=300
```

## Como validar browser

Abrir com Ctrl+F5:

- <http://localhost:8080/swagger>
- <http://localhost:8081/Admin>
- <http://localhost:8081/Admin/Diagnostics>
- <http://localhost:8081/Admin/FullServiceFlow>
- <http://localhost:8082/>
- <http://localhost:8083/Kiosk/Services>

O navegador deve usar apenas rotas relativas/proxies do web app em execução; nunca deve chamar o host interno Docker da API diretamente.
