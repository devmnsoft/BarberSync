# Growth Engine

Módulo inicial de inteligência comercial autônoma com:
- oportunidades detectadas;
- ações recomendadas com aprovação;
- execução controlada;
- base para aprendizado e medição de impacto.

## Endpoints iniciais
- `GET /api/growth-engine/opportunities`
- `GET /api/growth-engine/opportunities/{id}`
- `GET /api/growth-engine/recommended-actions`
- `POST /api/growth-engine/actions/{id}/approve`
- `POST /api/growth-engine/actions/{id}/reject`
- `POST /api/growth-engine/actions/{id}/execute`

## Integrações já conectadas
- Prescriptive AI: `GET /api/prescriptive-ai/recommendations`
- Revenue slots: `GET /api/revenue-management/slots`
- A/B experiments: `GET /api/ab-testing/experiments`
- Retention risk: `GET /api/retention/risk-scores`
- Dashboard summary: `GET /api/autonomous-growth-dashboard/summary`
