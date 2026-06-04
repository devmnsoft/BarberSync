# Checklist de demonstração — BarberSync Demo Comercial 11.0

## Pré-demo

- [ ] Executar `dotnet build` na raiz.
- [ ] Executar builds individuais da API, AdminWeb, PublicWeb e KioskWeb.
- [ ] Executar `docker compose build --no-cache`.
- [ ] Executar `docker compose up -d`.
- [ ] Confirmar `docker compose ps` com `api`, `admin-web`, `public-web`, `kiosk-web`, `postgres` e `seq` em estado Up.
- [ ] Abrir Swagger em `http://localhost:8080/swagger`.
- [ ] Validar que o browser não chama `http://api:8080`.

## Endpoints mínimos

- [ ] `http://localhost:8081/AdminApi/dashboard` retorna 200.
- [ ] `http://localhost:8081/AdminApi/clients` retorna 200.
- [ ] `http://localhost:8081/AdminApi/professionals` retorna 200.
- [ ] `http://localhost:8081/AdminApi/services` retorna 200.
- [ ] `http://localhost:8081/AdminApi/appointments` retorna 200.
- [ ] `http://localhost:8081/AdminApi/service-orders` retorna 200.
- [ ] `http://localhost:8082/PublicApi/services` retorna 200.
- [ ] `http://localhost:8082/PublicApi/professionals` retorna 200.
- [ ] `http://localhost:8083/KioskApi/services?deviceCode=KIOSK-DEMO-001` retorna 200.
- [ ] `http://localhost:8083/KioskApi/professionals?serviceId=demo&deviceCode=KIOSK-DEMO-001` retorna 200.
- [ ] `http://localhost:8080/api/kiosk/services?deviceCode=KIOSK-DEMO-001` retorna 200.

## Telas principais

- [ ] Admin Dashboard mostra KPIs e cards.
- [ ] Clients, Professionals, Services, Appointments, ServiceOrders e Stock têm listas e ações visuais.
- [ ] PublicWeb mostra hero, serviços, profissionais, formulário, CTAs e rodapé.
- [ ] Kiosk executa Serviço → Cliente → Profissional → Confirmação → Pagamento mock → Sucesso → Avaliação.
- [ ] CSS/JS/logos principais retornam 200.

## Console/logs

- [ ] Sem `ReferenceError` de arquivos BarberSync.
- [ ] Sem `ERR_NAME_NOT_RESOLVED`.
- [ ] Sem asset principal 404.
- [ ] Sem 409 no fluxo Kiosk.
- [ ] Sem DI error ou exception crítica nos logs.
