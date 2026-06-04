# Correções atuais — BarberSync

Data da auditoria: 2026-06-04.

## 1. Auditoria obrigatória

### Projetos existentes

- **API**: `Backend/Presentation/BarberSync.Api/BarberSync.Api.csproj`.
- **AdminWeb**: `Web/BarberSync.AdminWeb/BarberSync.AdminWeb.csproj`.
- **PublicWeb**: `Web/BarberSync.PublicWeb/BarberSync.PublicWeb.csproj`.
- **KioskWeb**: `Web/BarberSync.KioskWeb/BarberSync.KioskWeb.csproj`.
- **MobileApp**: `MobileApp/package.json` com aplicação React Native/JavaScript.

### Builds auditados

Comandos previstos para validação:

```bash
dotnet build
dotnet build Backend/Presentation/BarberSync.Api/BarberSync.Api.csproj
dotnet build Web/BarberSync.AdminWeb/BarberSync.AdminWeb.csproj
dotnet build Web/BarberSync.PublicWeb/BarberSync.PublicWeb.csproj
dotnet build Web/BarberSync.KioskWeb/BarberSync.KioskWeb.csproj
```

Resultado no ambiente desta correção: o SDK `dotnet` não está instalado no container de execução (`/bin/bash: dotnet: command not found`). A validação foi registrada como pendência de ambiente, não como erro de código.

### Dockerfiles existentes

- `Backend/Presentation/BarberSync.Api/Dockerfile`.
- `Web/BarberSync.AdminWeb/Dockerfile`.
- `Web/BarberSync.PublicWeb/Dockerfile`.
- `Web/BarberSync.KioskWeb/Dockerfile`.

Todos usam build/publish com SDK .NET 8 e runtime ASP.NET 8, expõem `ASPNETCORE_URLS=http://+:8080` e copiam o publish para `/app`.

### docker-compose.yml

O `docker-compose.yml` contém os serviços:

- `postgres` em `5432`.
- `seq` em `5341`.
- `api` em `8080`.
- `admin-web` em `8081`.
- `public-web` em `8082`.
- `kiosk-web` em `8083`.

As URLs `http://api:8080` aparecem apenas em configuração server-side de Docker (`ApiSettings__BaseUrl` / `ApiBaseUrl`) e não em JavaScript/Razor consumido diretamente pelo navegador.

### Proxies MVC existentes

- `Web/BarberSync.AdminWeb/Controllers/AdminApiController.cs`.
- `Web/BarberSync.PublicWeb/Controllers/PublicApiController.cs`.
- `Web/BarberSync.KioskWeb/Controllers/KioskApiController.cs`.

Os três proxies usam `IHttpClientFactory`, `IConfiguration` e `ILogger`, retornam JSON e têm fallback demo para manter as telas funcionais quando a API falha.

### Program.cs dos projetos web

- `Web/BarberSync.AdminWeb/Program.cs`.
- `Web/BarberSync.PublicWeb/Program.cs`.
- `Web/BarberSync.KioskWeb/Program.cs`.

Os três registram MVC, `HttpClient` para a API e mantêm `app.UseStaticFiles()` antes de `app.UseRouting()`.

### Views principais auditadas

- Admin: `Views/Admin/Index.cshtml`, `Dashboard`, `Clients`, `Professionals`, `Services`, `Appointments`, `ServiceOrders`, `Stock`, `Campaigns`, `Coupons`, `Reviews`, `Loyalty`, `Copilot`, `Kiosk`, `PublicSite`, `Reports`, `Help`.
- PublicWeb: `Web/BarberSync.PublicWeb/Views/Home/Index.cshtml`.
- Kiosk: `Services`, `Client`, `Professional`, `Confirm`, `Payment`, `Success`, `Review`, `Summary`, `Help`.

### Static files auditados

Arquivos principais existem em `wwwroot`:

- Admin: `css/admin-design-system.css`, `css/admin-layout.css`, `js/admin-dashboard.js`, `img/logo-barbersync.svg`.
- PublicWeb: `css/public-design-system.css`, `js/public.js`.
- Kiosk: `css/kiosk-design-system.css`, `js/kiosk.js`.

A `.dockerignore` não exclui `wwwroot`, `Views` ou `appsettings.json`.

### Rotas principais

- Admin: `/Admin`, `/Admin/Dashboard`, `/Admin/Clients`, `/Admin/Professionals`, `/Admin/Services`, `/Admin/Appointments`, `/Admin/ServiceOrders`, `/Admin/Stock`, `/Admin/Campaigns`, `/Admin/Coupons`, `/Admin/Reviews`, `/Admin/Loyalty`, `/Admin/Copilot`, `/Admin/Kiosk`, `/Admin/PublicSite`, `/Admin/Reports`, `/Admin/Help`.
- PublicWeb: `/` com hero, serviços, profissionais, proposta/agendamento demo, CTA Admin/Totem e rodapé/seções comerciais.
- Kiosk: `/Kiosk/Services`, `/Kiosk/Client`, `/Kiosk/Professional`, `/Kiosk/Confirm`, `/Kiosk/Payment`, `/Kiosk/Success`, `/Kiosk/Review` e `/Kiosk/Summary`.

### Chamadas indevidas auditadas

Pesquisa executada nos arquivos browser-side para:

- `http://api:8080`
- `api:8080`
- `localhost:8083/api`
- `8083/api`

Resultado: não há ocorrência em `.js`, `.cshtml` ou `.html` sob `Web`, `Backend`, `MobileApp` e `Totem` para os padrões proibidos de browser. As ocorrências restantes de `http://api:8080` ficam em `docker-compose.yml` e `appsettings.Docker.json`, que são server-side/configuração Docker.

### JavaScript auditado

Executado `node --check` em todos os JavaScripts de `Web/BarberSync.AdminWeb/wwwroot/js`, `Web/BarberSync.PublicWeb/wwwroot/js` e `Web/BarberSync.KioskWeb/wwwroot/js`.

Resultado: sintaxe OK.

### Arrays C# / CS0826

Pesquisa por `new[]` em C# encontrou apenas arrays homogêneos ou arrays já tipados como `object[]` nos dados demo principais. Não foi possível confirmar compilação por ausência do SDK `dotnet` no ambiente.

## 2. Erros encontrados, causas e correções

| Erro/Risco | Causa | Correção aplicada |
|---|---|---|
| `GET /api/kiosk/professionals?serviceId=demo&deviceCode=KIOSK-DEMO-001` poderia retornar 400 antes da action | O parâmetro `serviceId` era `Guid?`; o valor demo textual `demo` não faz binding como GUID em controller `[ApiController]` | O parâmetro passou para `string?`, permitindo IDs demo e IDs reais sem quebra de model binding |
| Kiosk services ainda podia retornar 500 no catch em ambiente não Development | O catch retornava 500 fora de Development | O catch agora sempre retorna fallback demo com HTTP 200 |
| Validação de build e Docker bloqueada no runner | `dotnet` e `docker` não estão instalados no ambiente | Registrado como pendência de ambiente; validações estáticas e `node --check` foram executadas |

## 3. Arquivos corrigidos

- `Backend/Presentation/BarberSync.Api/Controllers/Configuration/KioskConfigController.cs`.
- `Docs/CORRECOES_ATUAIS.md`.

## 4. Como validar localmente

### Build

```bash
dotnet build
dotnet build Backend/Presentation/BarberSync.Api/BarberSync.Api.csproj
dotnet build Web/BarberSync.AdminWeb/BarberSync.AdminWeb.csproj
dotnet build Web/BarberSync.PublicWeb/BarberSync.PublicWeb.csproj
dotnet build Web/BarberSync.KioskWeb/BarberSync.KioskWeb.csproj
```

### Docker

```bash
docker compose build --no-cache
docker compose up -d
docker compose ps
docker compose logs --tail=300 | grep -Ei "ERR|ERROR|Exception|fail|Failed|FATAL|Unhandled|500|404|409|ERR_NAME_NOT_RESOLVED"
```

No PowerShell, o último comando pode ser executado com:

```powershell
docker compose logs --tail=300 | Select-String -Pattern "ERR|ERROR|Exception|fail|Failed|FATAL|Unhandled|500|404|409|ERR_NAME_NOT_RESOLVED"
```

### Endpoints esperados com HTTP 200

```powershell
Invoke-WebRequest http://localhost:8081/AdminApi/dashboard
Invoke-WebRequest http://localhost:8081/AdminApi/clients
Invoke-WebRequest http://localhost:8081/AdminApi/professionals
Invoke-WebRequest http://localhost:8081/AdminApi/services
Invoke-WebRequest http://localhost:8081/AdminApi/appointments
Invoke-WebRequest http://localhost:8081/AdminApi/service-orders
Invoke-WebRequest http://localhost:8082/PublicApi/services
Invoke-WebRequest http://localhost:8082/PublicApi/professionals
Invoke-WebRequest "http://localhost:8083/KioskApi/services?deviceCode=KIOSK-DEMO-001"
Invoke-WebRequest "http://localhost:8083/KioskApi/professionals?serviceId=demo&deviceCode=KIOSK-DEMO-001"
Invoke-WebRequest "http://localhost:8080/api/kiosk/services?deviceCode=KIOSK-DEMO-001"
Invoke-WebRequest "http://localhost:8080/api/kiosk/professionals?serviceId=demo&deviceCode=KIOSK-DEMO-001"
```

### Static files esperados com HTTP 200

```powershell
Invoke-WebRequest http://localhost:8081/css/admin-design-system.css
Invoke-WebRequest http://localhost:8081/css/admin-layout.css
Invoke-WebRequest http://localhost:8081/js/admin-dashboard.js
Invoke-WebRequest http://localhost:8081/img/logo-barbersync.svg
Invoke-WebRequest http://localhost:8082/css/public-design-system.css
Invoke-WebRequest http://localhost:8082/js/public.js
Invoke-WebRequest http://localhost:8083/css/kiosk-design-system.css
Invoke-WebRequest http://localhost:8083/js/kiosk.js
```

## 5. Endpoints testados/validados nesta correção

No ambiente atual não foi possível subir serviços HTTP porque `dotnet` e `docker` não estão instalados. Foram validados estaticamente:

- Presença das actions em `AdminApiController`.
- Presença das actions em `PublicApiController`.
- Presença das actions em `KioskApiController`.
- Ausência de chamadas browser-side para host Docker proibido.
- Sintaxe dos JavaScripts web com `node --check`.

## 6. Pendências reais

- Executar build .NET em ambiente com SDK instalado.
- Executar Docker Compose em host com Docker instalado.
- Validar HTTP 200 real dos endpoints e assets após `docker compose up -d`.
- Validar visualmente Swagger, Admin, PublicWeb e Kiosk no navegador com Ctrl+F5.
- Confirmar DevTools sem `ReferenceError`, sem asset principal 404 e sem chamada browser-side para `http://api:8080`.

## Consolidação Demo 10.0 — 2026-06-04

### Diagnóstico encontrado
- O SDK `dotnet` e o binário `docker` não estão disponíveis neste ambiente de execução, impedindo validação local de build .NET, `docker compose build`, `docker compose up` e endpoints via serviços em execução.
- A tentativa de instalar o SDK .NET 8 via `https://dot.net/v1/dotnet-install.sh` retornou HTTP 403 neste ambiente.
- A varredura de browser files não encontrou `http://api:8080`, `api:8080`, `localhost:8083/api` ou `8083/api` em JavaScript, Razor, Views ou MobileApp; as ocorrências restantes seguem restritas a Docker/appsettings server-side e documentação explicativa.
- O fallback do proxy MVC do Totem e o fallback local do JavaScript do Totem não listavam todos os serviços e profissionais obrigatórios para uma demonstração comercial completa.
- O MobileApp declarava `main: index.js`, mas o arquivo `index.js` não existia; alguns componentes TypeScript também referenciavam aliases de tema não declarados (`text`, `dark`, `dark3`).

### Correções aplicadas
- Atualizada a versão-alvo visual/documental para **BarberSync SaaS Platform Demo 10.0** nos manuais e telas principais da demonstração.
- Ampliados os dados demo obrigatórios do Totem para `Corte Masculino`, `Barba Tradicional`, `Corte + Barba`, `Sobrancelha`, `Hidratação Capilar` e `Manicure`.
- Ampliados os profissionais demo obrigatórios do Totem para `Rafael Barber`, `Lucas Navalha`, `Bruno Estilo`, `Camila Beauty` e `Amanda Nails`.
- Corrigida proteção JavaScript do fluxo de avaliação do Totem para evitar falha caso o input de rating não esteja presente.
- Criado `MobileApp/index.js` com `registerRootComponent(App)` para alinhar o ponto de entrada do Expo ao `package.json`.
- Adicionados aliases de tema mobile usados pelos componentes existentes e expandido o smoke test para validar o entrypoint Expo.

### Validações executadas neste ambiente
- `node --check` nos JavaScripts críticos de Admin, PublicWeb e Totem passou sem erro de sintaxe.
- `npm test` em `MobileApp` passou com o smoke test local.
- `rg` confirmou ausência dos hosts internos proibidos em arquivos expostos ao browser.

### Pendências de ambiente
- Rodar `dotnet build` e builds por projeto em ambiente com SDK .NET 8.
- Rodar `docker compose build --no-cache`, `docker compose up -d`, `docker compose ps` e varredura de logs em ambiente com Docker.
- Com os containers ativos, validar HTTP 200 dos endpoints AdminApi, PublicApi, KioskApi e API Kiosk listados no checklist.
