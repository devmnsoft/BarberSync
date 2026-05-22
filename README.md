# BarberSync 2.0 - Futuristic Intelligent Platform

This monorepo now includes a **futuristic, automated and AI-driven foundation** for salon operations.

## Core Modules
- **Backend (.NET API):** Clean Architecture + DDD with advanced modules for AI analytics, loyalty, smart notifications, platform command center and futuristic automation endpoints.
- **Mobile (React Native):** Base screens + gamification/AI snapshot screen for real-time KPIs.
- **Totem (React kiosk):** Self-service data orchestrator with loyalty progress and AI-powered operations snapshot.
- **ML:** Structured integration points for service recognition and intelligent metrics.
- **ScriptsSQL:** Expanded database scripts for AI model registry, predictive demand, professional metrics and smart notification logs.

## New Futuristic Automation Endpoints
- `GET /api/futuristic-automation/operations-snapshot`
- `GET /api/futuristic-automation/bi-export`

## Highlights Delivered
- Real-time AI operational snapshot for dashboards and totems
- Predictive demand feed by hour and service profile
- Smart notification queue model (push/WhatsApp/Telegram)
- Professional KPI metrics ready for BI export (Power BI/Qlik)
- Gamified mobile experience scaffold with intelligent KPI visualization

## Quick Start
1. Configure environment values (JWT, DB, Redis, broker and external integrations).
2. Run SQL scripts from `ScriptsSQL` including `007_futuristic_ai_automation.sql`.
3. Start backend API with Swagger.
4. Start Mobile and Totem applications.

## Status
Production-oriented scaffold for BarberSync 2.0 advanced evolution, ready for integration and iterative hardening.

## BarberSync SaaS 2.0 (Evolução comercial)
- Multiempresa com módulos SaaS, onboarding, dashboard e avaliações.
- Landing page comercial em `Frontend/LandingPage/index.html`.
- Novas rotas: `/api/saas/*`, `/api/onboarding/*`, `/api/dashboard/*`, `/api/reviews`, `/api/nps`, `/api/reports/satisfaction/{tenantId}`.
- Documentação detalhada em `Docs/`.

