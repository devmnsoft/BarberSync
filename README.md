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

\n## Strategic Business Modules 2.0\n- Franchise & network management\n- Commercial goals & progress\n- Professional ranking and performance\n- CRM reactivation and VIP intelligence\n- Campaign management\n- Business insights and executive dashboard\n- Financial planning and reputation\n- Management report job pipeline

## Strategic Business Modules 2.0
- Franchise & network management
- Commercial goals & progress
- Professional ranking and performance
- CRM reactivation and VIP intelligence
- Campaign management
- Business insights and executive dashboard
- Financial planning and reputation
- Management report job pipeline

## Módulos Financeiro/Fiscal (v2.1)
Inclui módulo fiscal inicial, recibos, contas a pagar e dashboard financeiro básico com endpoints em `Backend/Presentation/Finance` e scripts em `ScriptsSQL/46-57`.

## Strategic Growth 2.0
Inclui inteligência competitiva, precificação dinâmica, fornecedores, compras, reposição, consumo por serviço, expansão, franquias avançadas, benchmarking e dashboard estratégico.

## Customer Experience Ecosystem (Phase 1)

Added SQL foundations and docs for subscriptions, benefit club, wallet, family accounts, referrals, e-commerce, customer preferences, visual history, journeys and CX dashboard.

See ScriptsSQL 105-115 and Docs/CUSTOMER_* modules.


## Super App Ecosystem Foundation (Phase 2)

Adicionados scripts SQL 129-141 e documentação dos novos domínios:
- Conta digital empresarial e reconciliação financeira
- Repasses de profissionais
- Marketplace B2B e compras coletivas
- IA multimodal, assistente de voz e totem acessível
- Super App cliente e app do dono
- Ranking público e centro de oportunidades
- Monetização da plataforma

## Enterprise Open Platform (2.0+)
- Public API versioned + Developer Portal
- Partner Integrators + Governance + Enterprise Risks
- Incidents/SLA + Command Center + Autonomous AI Agents
- Change/Release Management + Business Continuity + Immutable Audit + Advanced Compliance
- Enterprise Dashboard and SQL seeds/views for demos


## Plataforma 360º (Consolidação)
- Scripts SQL consolidados em `ScriptsSQL/Consolidated` com ordem de execução 001-020.
- Documentação consolidada em `Docs/` para arquitetura, API, deploy, testes, operação, mobile e totem.
- Roteiro comercial completo em `Docs/DEMO_SCRIPT.md`.

## Banco de Dados PostgreSQL Consolidado

Execute o script único consolidado:

```bash
psql -U postgres -d barbersync -f ScriptsSQL/barbersync_full_database_postgresql.sql
```

Validação pós-criação:

```bash
psql -U postgres -d barbersync -f ScriptsSQL/validate_barbersync_database.sql
```
