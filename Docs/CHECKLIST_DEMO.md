# Checklist de demonstração — BarberSync Demo 10.0

## Build e containers

- [ ] `dotnet build`
- [ ] `dotnet build Backend/Presentation/BarberSync.Api/BarberSync.Api.csproj`
- [ ] `dotnet build Web/BarberSync.AdminWeb/BarberSync.AdminWeb.csproj`
- [ ] `dotnet build Web/BarberSync.PublicWeb/BarberSync.PublicWeb.csproj`
- [ ] `dotnet build Web/BarberSync.KioskWeb/BarberSync.KioskWeb.csproj`
- [ ] `docker compose build --no-cache`
- [ ] `docker compose up -d`
- [ ] `docker compose ps`

## URLs

- [ ] `http://localhost:8080/swagger`
- [ ] `http://localhost:8081/Admin`
- [ ] `http://localhost:8081/Admin/Dashboard`
- [ ] `http://localhost:8081/Admin/Clients`
- [ ] `http://localhost:8081/Admin/Professionals`
- [ ] `http://localhost:8081/Admin/Services`
- [ ] `http://localhost:8081/Admin/Appointments`
- [ ] `http://localhost:8081/Admin/ServiceOrders`
- [ ] `http://localhost:8081/Admin/Stock`
- [ ] `http://localhost:8081/Admin/Copilot`
- [ ] `http://localhost:8082/`
- [ ] `http://localhost:8083/Kiosk/Services`

## Endpoints 200

- [ ] `/AdminApi/dashboard`
- [ ] `/AdminApi/clients`
- [ ] `/AdminApi/professionals`
- [ ] `/AdminApi/services`
- [ ] `/AdminApi/appointments`
- [ ] `/AdminApi/service-orders`
- [ ] `/PublicApi/services`
- [ ] `/PublicApi/professionals`
- [ ] `/KioskApi/services?deviceCode=KIOSK-DEMO-001`
- [ ] `/KioskApi/professionals?serviceId=demo&deviceCode=KIOSK-DEMO-001`
- [ ] `/api/kiosk/services?deviceCode=KIOSK-DEMO-001`

## DevTools

- [ ] Nenhuma chamada browser-side para `http://api:8080`.
- [ ] Nenhum asset principal 404.
- [ ] Nenhum `ReferenceError` crítico do BarberSync.
- [ ] Kiosk sem 409 em consultas.
