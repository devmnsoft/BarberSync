# Checklist Demo — BarberSync Stable Demo Release 13.0

## Build e infraestrutura

- [ ] `dotnet build` passa sem erros.
- [ ] Builds individuais da API, AdminWeb, PublicWeb e KioskWeb passam.
- [ ] `docker compose build --no-cache` passa.
- [ ] `docker compose up -d` sobe `api`, `admin-web`, `public-web`, `kiosk-web`, `postgres` e `seq`.
- [ ] `docker compose ps` mostra serviços principais `Up`.

## URLs principais

- [ ] Swagger: http://localhost:8080/swagger
- [ ] Admin: http://localhost:8081/Admin
- [ ] PublicWeb: http://localhost:8082/
- [ ] Kiosk: http://localhost:8083/Kiosk/Services

## Proxies e comunicação segura

- [ ] Browser chama `/AdminApi/...` no AdminWeb.
- [ ] Browser chama `/PublicApi/...` no PublicWeb.
- [ ] Browser chama `/KioskApi/...` no KioskWeb.
- [ ] Browser não chama `http://api:8080`.
- [ ] `ApiSettings:BaseUrl` é usado somente server-side.

## Fluxos demonstráveis

- [ ] Admin lista dados demo em dashboard, clientes, profissionais, serviços, agenda, comandas e estoque.
- [ ] CRUDs principais abrem modal, salvam demo e mostram toast.
- [ ] PublicWeb renderiza serviços/profissionais e envia agendamento com protocolo.
- [ ] Kiosk conclui serviço → cliente → profissional → confirmação → pagamento mock → sucesso → avaliação.
