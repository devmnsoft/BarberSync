# Arquitetura demo BarberSync

## Componentes

- API ASP.NET Core em `localhost:8080`.
- AdminWeb MVC em `localhost:8081`.
- PublicWeb MVC em `localhost:8082`.
- KioskWeb MVC em `localhost:8083`.
- PostgreSQL.
- Seq.

## Comunicação

O navegador nunca chama a URL interna Docker. As aplicações Web expõem proxies relativos:

- Admin: `/AdminApi/...`
- PublicWeb: `/PublicApi/...`
- KioskWeb: `/KioskApi/...`

`ApiSettings:BaseUrl` e `ApiBaseUrl` são usados apenas server-side pelos controllers/proxies MVC. No Docker, esses valores apontam para o serviço interno da API; em execução local, usam `localhost:8080`.

## Estratégia de estabilidade

- Controllers MVC chamam a API via `IHttpClientFactory`.
- Falhas da API retornam fallback demo com status 200.
- Static files são servidos antes do roteamento MVC.
- JavaScript usa rotas relativas, tratamento de erro e estado local demo.
