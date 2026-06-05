# Correções atuais — estabilização demo BarberSync

## Diagnóstico

- O ambiente local desta execução não possui `dotnet` nem `docker`, então a validação de build e containers foi registrada como pendência ambiental.
- A auditoria encontrou `http://api:8080` apenas em configurações Docker/server-side e documentação; não houve ocorrência em JS/CSHTML/HTML renderizado ao navegador.
- O proxy legado `wwwroot/js/api-client.js` não tinha fallback resiliente e poderia lançar erro no browser se usado por telas antigas.
- O `AdminApiClient` aceitava URLs absolutas e não normalizava caminhos `/api/...` quando recebidos como URL completa.
- O `KioskApiController` devolvia payload da API mesmo quando vazio, podendo deixar a tela do totem sem cards em caso de API com resposta vazia.
- Scripts PublicWeb/Kiosk possuíam leituras de `localStorage`/`sessionStorage` que podiam quebrar com JSON corrompido.

## Correções aplicadas

- Proxy Admin legado reescrito para usar somente `/AdminApi`, com `try/catch`, fallback demo e toast de aviso.
- `AdminApiClient` passou a normalizar URLs absolutas com path `/api/...` para proxy relativo `/AdminApi/...`.
- `KioskApiController` passou a detectar respostas vazias e retornar fallback demo 200 para serviços e profissionais.
- PublicWeb passou a ler leads do `localStorage` com parse seguro e manter protocolo demo.
- Kiosk passou a registrar fallback de pagamento/review, proteger parse de sessão e manter fluxo demo navegável.

## Validação realizada nesta execução

- Auditoria de rotas MVC Admin, Kiosk e proxies.
- Auditoria de static files esperados nos projetos Web.
- Auditoria de ocorrências browser-side proibidas: `http://api:8080`, `api:8080`, `localhost:8083/api`, `8083/api`.
- Validação sintática dos JavaScripts alterados com Node.js.

## Pendências reais

- Executar `dotnet build` em ambiente com SDK .NET 8 instalado.
- Executar `docker compose build --no-cache`, `docker compose up -d`, `docker compose ps` e validação HTTP em ambiente com Docker disponível.
- Validar visualmente Swagger, Admin, PublicWeb e Kiosk no navegador após subir os containers.
