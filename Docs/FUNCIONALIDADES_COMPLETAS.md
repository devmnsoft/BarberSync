# Funcionalidades completas — BarberSync Hardening + Demo Comercial 11.0

## Backend/API

- Swagger habilitado.
- Middleware global de exceção.
- Serviços de aplicação e infraestrutura registrados.
- `IConfigurationService` registrado para configuração Kiosk.
- Endpoints Kiosk demo resilientes para serviços, profissionais, cliente, pagamento e avaliação.

## AdminWeb

- Layout administrativo profissional com sidebar, topbar, toasts, modais, command palette, tour e help drawer.
- Proxies `/AdminApi` com GETs e ações para dashboard, clientes, profissionais, serviços, agenda, comandas, estoque, campanhas, cupons, reviews, fidelidade, Copilot, Kiosk, financeiro e relatórios.
- Fallbacks demo com resposta JSON 200.
- DemoStore/localStorage e EventBus para ações visuais sem quebrar a apresentação.

## PublicWeb

- Landing comercial SaaS.
- Serviços e profissionais renderizados dinamicamente por `/PublicApi`.
- Formulário com protocolo demo.
- Calculadora ROI.

## KioskWeb

- Rotas `Services`, `Client`, `Professional`, `Confirm`, `Payment`, `Success`, `Review` e `Summary`.
- Fluxo completo com sessionStorage.
- Pagamento e avaliação mock via proxy `/KioskApi`.

## MobileApp

- App Expo/React Native com smoke test local.
- Telas de home, login, serviços, agendamento, cashback, histórico, gamificação e perfil.
