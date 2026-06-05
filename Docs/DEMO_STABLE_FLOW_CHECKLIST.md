# BarberSync — Checklist de Demonstração Estável

## Objetivo
Validar uma apresentação navegável e funcional do BarberSync com AdminWeb, PublicWeb, KioskWeb, Mobile e API trabalhando em conjunto sem chamadas de browser para `http://api:8080`.

## Arquitetura de navegação segura
- AdminWeb usa somente proxy relativo `/AdminApi/*` no navegador.
- PublicWeb usa somente proxy relativo `/PublicApi/*` no navegador.
- KioskWeb usa somente proxy relativo `/KioskApi/*` no navegador.
- A comunicação com `http://api:8080` permanece server-side via configuração Docker dos projetos MVC.
- Mobile consome a API configurada por `EXPO_PUBLIC_API_URL` e mantém fallback local para demonstração offline.

## Fluxo FullServiceFlow vertical
1. Cliente criado/selecionado.
2. Agendamento criado e confirmado.
3. Check-in registrado.
4. Atendimento iniciado e finalizado.
5. Comanda aberta com serviço e produto.
6. Pagamento mock aprovado.
7. Recibo visual gerado.
8. Estoque movimentado/reposto.
9. Cashback gerado.
10. Avaliação registrada.
11. Dashboard, Cliente 360, Agenda, Comandas, Estoque, Cashback e Avaliações atualizados.

## Checklist de demo por canal
### AdminWeb
- Abrir `/Admin/Dashboard` e confirmar KPIs preenchidos.
- Abrir `/Admin/FullServiceFlow` e executar todas as 11 etapas até Dashboard, confirmando a baixa de estoque antes do cashback.
- Abrir `/Admin/Clients`, `/Admin/Appointments`, `/Admin/ServiceOrders`, `/Admin/Stock`, `/Admin/Loyalty` e `/Admin/Reviews` para confirmar reflexo do DemoStore.
- Abrir `/Admin/DemoExperience` para visualizar histórico do EventBus.

### PublicWeb
- Abrir `/` no PublicWeb.
- Confirmar serviços e profissionais carregados via `/PublicApi/services` e `/PublicApi/professionals`.
- Enviar o formulário de proposta/agendamento.
- Confirmar protocolo, persistência em `barbersync.public.demo.store.v1` e eventos em `barbersync.public.demo.events.v1`.

### KioskWeb
- Abrir `/Kiosk/Services`.
- Selecionar serviço, informar cliente, escolher profissional, confirmar e pagar.
- Confirmar que o pagamento mock chama `/KioskApi/payment/mock` apenas uma vez.
- Enviar avaliação e validar persistência em `barbersync.kiosk.demo.store.v1` e eventos em `barbersync.kiosk.demo.events.v1`.

### Mobile
- Iniciar Expo com `EXPO_PUBLIC_API_URL` apontando para a API exposta.
- Confirmar carregamento de `/api/mobile/summary`.
- Validar fallback local quando a API estiver indisponível.

## Endpoints HTTP 200 esperados
- `GET /health`
- `GET /swagger`
- `GET /api/full-service-flow/snapshot`
- `POST /api/full-service-flow/run`
- `GET /api/mobile/summary`
- `GET /api/loyalty/accounts`
- `GET /AdminApi/dashboard`
- `GET /AdminApi/full-service-flow/snapshot`
- `POST /AdminApi/full-service-flow/run`
- `GET /AdminApi/audit-events`
- `GET /PublicApi/services`
- `GET /KioskApi/services?deviceCode=KIOSK-DEMO-001`

## Critérios de aceite
- Nenhuma tela vazia nos fluxos principais.
- Nenhum botão crítico sem ação demonstrável.
- Nenhum modal/formulário crítico sem submit.
- Assets CSS, JS e SVG carregando por `~/`/static files.
- Docker Compose mantendo a API como serviço interno e proxies relativos no browser.
- EventBus e DemoStore registrando ações de Admin, PublicWeb e KioskWeb.
- Eventos esperados no FullServiceFlow: `flow:receiptGenerated`, `flow:stockUpdated`, `flow:cashbackGenerated`, `flow:reviewCreated`, `flow:apiSynced` e `flow:completed`.
