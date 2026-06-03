# Arquitetura demo BarberSync 7.0

## Componentes
- API ASP.NET Core: Swagger em `http://localhost:8080/swagger`.
- AdminWeb MVC: `http://localhost:8081/Admin`.
- PublicWeb MVC: `http://localhost:8082/`.
- KioskWeb MVC: `http://localhost:8083/Kiosk/Services`.
- MobileApp React Native + Expo.
- PostgreSQL, Docker Compose e Seq.

## Proxies
- AdminWeb expõe `/AdminApi/...` e faz proxy server-side para a API.
- PublicWeb expõe `/PublicApi/...` e faz proxy server-side para a API.
- KioskWeb expõe `/KioskApi/...` e faz proxy server-side para a API.
- O browser não deve chamar `http://api:8080`.

## Como rodar Docker
1. `docker compose build --no-cache`
2. `docker compose up -d`
3. `docker compose ps`
4. Validar logs críticos com filtro de erro.

## Como rodar Windows/local
1. `dotnet build`
2. Subir API, AdminWeb, PublicWeb e KioskWeb nas portas 8080, 8081, 8082 e 8083.
3. Executar `Invoke-WebRequest` nos endpoints de validação.

## Demonstrações-chave
- Multiunidade: `/Admin/Branches`.
- Canais: `/Admin/PublicSite` e `/Admin/Kiosk`.
- Auditoria: `/Admin/Audit`.
- Notificações: `/Admin/Notifications`.
