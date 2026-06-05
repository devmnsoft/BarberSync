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
