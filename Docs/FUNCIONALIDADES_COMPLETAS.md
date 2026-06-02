# Funcionalidades Completas — BarberSync Demo

## Admin

- Dashboard executivo com KPIs, painéis e gráficos visuais.
- Clientes com CRUD visual, filtros, detalhes, agendamento e comanda.
- Profissionais com cadastro, agenda, comissão, serviços e performance.
- Serviços com catálogo multicanal para site, totem e mobile.
- Agenda com timeline, status coloridos e ações de ciclo de atendimento.
- Comandas com kanban, itens, pagamento mock e fechamento.
- Estoque com produtos, críticos, barras e movimentos.
- Campanhas, cupons, fidelidade e avaliações.
- Copilot com sugestões, perguntas rápidas e histórico local.

## PublicWeb

- Landing premium comercial com CTA, serviços, profissionais, planos, depoimentos, FAQ e formulário de agendamento.

## Totem

- Fluxo completo de autoatendimento com serviço, cliente, profissional, confirmação, pagamento mock, sucesso e avaliação.

## Mobile

- Identidade visual base em telas de home, serviços, agendamentos, cashback e perfil.

## Integração segura

- Browser usa `/AdminApi`, `/PublicApi` e `/KioskApi`.
- Proxies MVC usam `IHttpClientFactory`, `IConfiguration` e `ILogger` para chamar a API server-side.
- Fallback demo evita telas quebradas durante apresentações.
