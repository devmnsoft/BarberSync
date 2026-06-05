# Arquitetura Demo — BarberSync Stable Demo Release 13.0

## Componentes

- `Backend/Presentation/BarberSync.Api`: API ASP.NET Core com Swagger.
- `Web/BarberSync.AdminWeb`: Admin MVC com proxy `/AdminApi`.
- `Web/BarberSync.PublicWeb`: site público MVC com proxy `/PublicApi`.
- `Web/BarberSync.KioskWeb`: Totem MVC com proxy `/KioskApi`.
- `MobileApp`: React Native + Expo para canal mobile.
- `docker-compose.yml`: orquestra API, web apps, PostgreSQL e Seq.

## Comunicação

```text
Browser Admin    -> /AdminApi/*  -> AdminWeb server-side  -> ApiSettings:BaseUrl
Browser Public   -> /PublicApi/* -> PublicWeb server-side -> ApiSettings:BaseUrl
Browser Kiosk    -> /KioskApi/*  -> KioskWeb server-side  -> ApiSettings:BaseUrl
Docker MVC -> http://api:8080
Local MVC  -> http://localhost:8080
```

`http://api:8080` é endereço interno Docker e não deve aparecer em fetch/browser-side.

## Estratégia de estabilidade

- Proxies MVC envolvem chamadas em fallback demo.
- GET usado por tela não deixa payload vazio.
- Mutação demo responde sucesso controlado.
- Static files são servidos antes de routing nos MVCs.
