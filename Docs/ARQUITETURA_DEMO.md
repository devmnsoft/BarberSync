# Arquitetura Demo BarberSync 8.0

## Componentes

- API ASP.NET Core em `http://localhost:8080`.
- AdminWeb ASP.NET Core MVC em `http://localhost:8081`.
- PublicWeb ASP.NET Core MVC em `http://localhost:8082`.
- KioskWeb ASP.NET Core MVC em `http://localhost:8083`.
- MobileApp React Native + Expo.
- PostgreSQL e Seq via Docker Compose.

## Proxies

O navegador não chama a API Docker diretamente. Cada aplicação MVC expõe proxy local:

- Admin: `/AdminApi/...`.
- PublicWeb: `/PublicApi/...`.
- Kiosk: `/KioskApi/...`.

`ApiSettings:BaseUrl` e `ApiBaseUrl` são usados server-side para falar com a API. No Docker o valor interno é resolvido pela rede do compose; localmente usa localhost quando configurado.

## Estado demo

- `barbersync.demo.state.v8`: DemoStore operacional e configurações.
- `barbersync.saas8.config`: preferências visuais SaaS 8.0.
- Eventos demo são mantidos pelo EventBus/localStorage para auditoria e notificações.

## Execução

```bash
dotnet build
docker compose build --no-cache
docker compose up -d
docker compose ps
```

## Validação

Valide Swagger, Admin, PublicWeb, Totem, Kiosk Help, assets CSS e proxies. Também pesquise JS/CSHTML/HTML renderizados para garantir ausência de chamadas diretas do browser à API interna.
