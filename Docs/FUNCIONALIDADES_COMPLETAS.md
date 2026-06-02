# Funcionalidades completas BarberSync

## AdminWeb

- Layout enterprise com sidebar, topbar, cards, tabelas, modais, toasts e responsividade.
- Dashboard executivo com KPIs, agenda, comandas, estoque, alertas e Copilot.
- CRUD visual de clientes, profissionais e serviços com criar, editar, excluir, detalhes, validação e fallback demo.
- Agenda, comandas, estoque, campanhas, cupons, avaliações, fidelidade e Copilot visíveis para demonstração.
- Proxies `/AdminApi/*` com fallback padronizado `{ success, message, data }`.

## PublicWeb

- Landing premium com serviços e profissionais carregados via `/PublicApi/*`.
- Formulário público de agendamento com validação e toast.

## KioskWeb

- Fluxo completo de autoatendimento: serviços, cliente, profissional, confirmação, pagamento mock, sucesso e avaliação.
- Estado local em `sessionStorage` e proxies `/KioskApi/*`.

## MobileApp

- Tema BarberSync com cores, tipografia, botões, cards, empty state e telas de home, serviços, agendamentos, cashback e perfil alinhadas ao produto.

## Docker e infraestrutura

- Static files habilitados em AdminWeb, PublicWeb e KioskWeb.
- Controllers MVC usam `ApiSettings:BaseUrl` server-side.
- `.dockerignore` preserva `wwwroot`, `Views` e `appsettings.json`.
