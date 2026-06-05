# Funcionalidades Completas — Stable Demo Release 13.0

## API

- Swagger habilitado.
- Controllers de dashboard, serviços, profissionais, clientes, agenda, fidelidade, reviews, campanhas, cupons e kiosk.
- Kiosk API demo com serviços, profissionais, status, cliente rápido, pagamento mock e avaliação.

## AdminWeb

- Rotas administrativas principais sem tela vazia.
- Proxies `/AdminApi` para GETs e mutações demo.
- DemoStore/localStorage, EventBus, toasts, modais, tabelas e CRUD visual.

## PublicWeb

- Landing SaaS comercial.
- Serviços/profissionais por proxy.
- Agendamento com protocolo demo.

## KioskWeb

- Fluxo completo de autoatendimento.
- SessionStorage para estado do atendimento.
- Pagamento mock e avaliação.

## Docker

- Compose com API, AdminWeb, PublicWeb, KioskWeb, PostgreSQL e Seq.
- MVCs acessam API internamente por `ApiSettings:BaseUrl`.

## Vertical Slice Demo 14.0 — Atendimento Completo

A versão 14.0 adiciona a tela `/Admin/FullServiceFlow`, um roteiro visual premium que integra Cliente, Agenda, Check-in, Atendimento, Comanda, Pagamento, Recibo, Estoque, Cashback, Avaliação e Dashboard. O fluxo usa DemoStore/localStorage, proxies relativos `/AdminApi` para dados de serviço/profissional quando possível, e eventos `flow:*` no EventBus para histórico, notificações demo, auditoria demo e atualização dos módulos administrativos.

## Vertical Slice + Produto Comercial 15.0 — Fluxo Comercial Completo

A versão 15.0 adiciona `/Admin/CommercialFlow`, uma tela central premium com stepper visual de 11 etapas: Origem, Cliente, Agendamento, Check-in, Atendimento, Comanda, Pagamento, Recibo, Cashback, Avaliação e Resultado. O fluxo persiste em `barbersync.commercialFlow.v15`, usa proxies relativos `/AdminApi`, integra DemoStore, EventBus `commercialFlow:*`, Dashboard, Cliente 360, Agenda, Operação ao Vivo, Comandas, Estoque, Avaliações, PublicWeb e Totem.
