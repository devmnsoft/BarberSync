# BarberSync — Funcionalidades Completas Demo Comercial 1.0

## Admin
- Onboarding com progresso em `localStorage`.
- Ajuda contextual e manual.
- Dashboard executivo com KPIs, insights, plano de ação e ações rápidas.
- Clientes com CRUD visual e Cliente 360.
- Profissionais com performance, comissão, ranking, ocupação e meta.
- Serviços com CRUD e canais PublicWeb/Totem/Mobile.
- Agenda com transição de status.
- Comandas com pagamento e recibo visual.
- Estoque com alertas, barras e reposição demo.
- Campanhas, cupons, fidelidade/cashback e avaliações/NPS.
- Copilot com chat, perguntas rápidas, prioridades e ações.

## PublicWeb
Landing comercial com CTA, planos, diferenciais e agendamento via `/PublicApi/appointments`.

## Kiosk
Fluxo completo com progresso, ajuda, acessibilidade, resumo lateral, pagamento mock, sucesso e avaliação.

## Mobile
Home demonstrável com saudação, próximo agendamento, serviços em destaque, cashback, promoções e CTA de agendamento.

## Proxies
O browser deve consumir somente `/AdminApi`, `/PublicApi` e `/KioskApi`. Configurações internas de API são usadas server-side pelos projetos MVC.
