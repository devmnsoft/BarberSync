# Arquitetura de demonstração — BarberSync Demo 10.0

## Componentes

- API ASP.NET Core em `http://localhost:8080`.
- AdminWeb ASP.NET Core MVC em `http://localhost:8081/Admin`.
- PublicWeb ASP.NET Core MVC em `http://localhost:8082/`.
- KioskWeb ASP.NET Core MVC em `http://localhost:8083/Kiosk/Services`.
- MobileApp React Native + Expo.
- PostgreSQL 16.
- Seq.

## Comunicação

- AdminWeb browser → `/AdminApi/...` → AdminApiController → API server-side.
- PublicWeb browser → `/PublicApi/...` → PublicApiController → API server-side.
- KioskWeb browser → `/KioskApi/...` → KioskApiController → API server-side.

## Regra de rede

`http://api:8080` é exclusivo de Docker/server-side. O navegador nunca deve chamar esse host.

## Resiliência demo

Os proxies MVC retornam fallback 200 quando a API falha ou retorna payload vazio, evitando telas vazias durante apresentação comercial.
