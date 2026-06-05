# Checklist demo BarberSync

## Build e containers

- [ ] `dotnet build`
- [ ] `dotnet build Backend/Presentation/BarberSync.Api/BarberSync.Api.csproj`
- [ ] `dotnet build Web/BarberSync.AdminWeb/BarberSync.AdminWeb.csproj`
- [ ] `dotnet build Web/BarberSync.PublicWeb/BarberSync.PublicWeb.csproj`
- [ ] `dotnet build Web/BarberSync.KioskWeb/BarberSync.KioskWeb.csproj`
- [ ] `docker compose build --no-cache`
- [ ] `docker compose up -d`
- [ ] `docker compose ps`

## URLs principais

- [ ] API Swagger: `http://localhost:8080/swagger`
- [ ] Admin: `http://localhost:8081/Admin`
- [ ] PublicWeb: `http://localhost:8082/`
- [ ] Kiosk: `http://localhost:8083/Kiosk/Services`

## Proxies

- [ ] AdminWeb usa `/AdminApi/...`
- [ ] PublicWeb usa `/PublicApi/...`
- [ ] KioskWeb usa `/KioskApi/...`
- [ ] Browser não chama `http://api:8080`

## Static files principais

- [ ] Admin CSS/JS/logo retornam 200.
- [ ] Public CSS/JS/logo retornam 200.
- [ ] Kiosk CSS/JS/logo retornam 200.

## Fluxos

- [ ] Admin mostra dashboard, cadastros, agenda, comanda, estoque e Copilot.
- [ ] PublicWeb renderiza serviços/profissionais e agenda com protocolo.
- [ ] Kiosk conclui serviço → cliente → profissional → confirmação → pagamento mock → sucesso → avaliação.
