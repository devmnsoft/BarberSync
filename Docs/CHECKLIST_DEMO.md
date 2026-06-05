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

## Checklist Atendimento Completo 14.0

- [ ] Acessar `/Admin/FullServiceFlow`.
- [ ] Criar ou selecionar cliente.
- [ ] Criar agendamento com serviço e profissional.
- [ ] Realizar check-in.
- [ ] Iniciar e finalizar atendimento.
- [ ] Abrir comanda, adicionar serviço e produto.
- [ ] Aplicar desconto/cupom/cashback usado quando necessário.
- [ ] Pagar a comanda e confirmar baixa de estoque.
- [ ] Gerar recibo mock.
- [ ] Confirmar cashback.
- [ ] Salvar avaliação.
- [ ] Concluir o fluxo e validar Dashboard, Cliente 360, Agenda, Comandas, Estoque, Avaliações e Operação.
- [ ] Confirmar que o browser usa somente proxies relativos (`/AdminApi`, `/PublicApi`, `/KioskApi`).

## Checklist Fluxo Comercial Completo 15.0

- [ ] Acessar `/Admin/CommercialFlow`.
- [ ] Selecionar origem Admin, PublicWeb, Totem ou Mobile.
- [ ] Criar cliente rápido ou selecionar cliente existente.
- [ ] Criar agendamento por `/AdminApi/services` e `/AdminApi/professionals` com fallback.
- [ ] Realizar check-in, iniciar e finalizar atendimento.
- [ ] Abrir comanda, adicionar serviço/produto, aplicar desconto, cupom e cashback.
- [ ] Pagar a comanda e confirmar baixa de estoque.
- [ ] Gerar recibo visual, PDF mock e compartilhamento mock.
- [ ] Confirmar cashback e salvar avaliação.
- [ ] Validar Dashboard, Cliente 360, Agenda, Comandas, Estoque, Avaliações e Operação ao Vivo.
- [ ] Confirmar eventos `commercialFlow:*` em auditoria/notificações/demo center via EventBus.
