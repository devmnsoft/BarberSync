# BarberSync Hardening + Demo Comercial 11.0 — Correções atuais

Data da consolidação: 2026-06-04.

## Diagnóstico executado

- A raiz do repositório não possuía uma solution principal para o comando `dotnet build` solicitado na rotina de aceite.
- Docker Compose aponta corretamente os serviços MVC para `http://api:8080` apenas em variáveis server-side; nenhuma ocorrência browser-side foi encontrada em JS/CSHTML/HTML.
- `Program.cs` dos projetos AdminWeb, PublicWeb e KioskWeb mantém `app.UseStaticFiles()` antes de `app.UseRouting()`.
- API possui Swagger, middleware global de exceção e `IConfigurationService` registrado para o fluxo Kiosk.
- Proxies MVC `/AdminApi`, `/PublicApi` e `/KioskApi` possuem fallbacks demo para evitar tela vazia quando API/infra falhar.
- PublicWeb tinha JS para renderizar serviços/profissionais, mas não tinha os containers `#services` e `#pros`; a vitrine dinâmica foi adicionada.

## Correções aplicadas

1. Criada `BarberSync.sln` na raiz com API, Application, Domain, Infrastructure, AdminWeb, PublicWeb, KioskWeb e Tests.
2. PublicWeb recebeu seções dinâmicas de serviços e profissionais alimentadas por `/PublicApi/services` e `/PublicApi/professionals`.
3. Âncora de agendamento do PublicWeb alinhada para `#agendamento`.
4. Identidade visual textual atualizada para `Demo Comercial 11.0` no Admin e PublicWeb.
5. Documentação de demo, operação e checklist atualizada para a etapa 11.0.

## Comunicação browser/API

- Browser AdminWeb: usar somente `/AdminApi/...`.
- Browser PublicWeb: usar somente `/PublicApi/...`.
- Browser KioskWeb: usar somente `/KioskApi/...`.
- `http://api:8080` permanece restrito ao Docker/server-side via `ApiSettings:BaseUrl` e variáveis de ambiente do Compose.

## Validações realizadas neste ambiente

- `node --check` em scripts JS dos projetos AdminWeb, PublicWeb e KioskWeb.
- `npm test` no MobileApp.
- Auditoria de ocorrências de `http://api:8080`, `api:8080`, `localhost:8083/api` e `8083/api`.

## Limitações reais do ambiente

- `dotnet` não está instalado no container de execução, impedindo validação local de `dotnet build`.
- `docker` não está instalado no container de execução, impedindo `docker compose build`, `up`, `ps` e logs.
- Endpoints HTTP em `localhost:8080-8083` dependem de Docker/.NET indisponíveis neste ambiente.
