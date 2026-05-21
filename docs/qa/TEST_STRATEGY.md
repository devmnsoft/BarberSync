# BarberSync 2.0 - Estratégia de Qualidade e Testes

## Objetivo de Cobertura
- Cobertura mínima: **90%** para domínios críticos (agendamento, pagamentos, IA, fidelidade, notificações).
- Critérios de aceite por release:
  - Unitários: Services + Repositories.
  - Integração: API + banco + filas.
  - E2E: Admin, Mobile e Totem.

## Pirâmide de Testes
1. **Unitários (70%)**
   - Serviços de aplicação e domínio.
   - Regras de gamificação (pontos/cashback/VIP).
   - Orquestrador de IA e previsão de demanda.
2. **Integração (20%)**
   - Endpoints REST, autenticação JWT/MFA.
   - Persistência PostgreSQL e Redis.
   - Gateways de pagamento e notificadores.
3. **E2E (10%)**
   - Login, CRUD, agenda drag-and-drop, checkout PIX/cartão, fluxo totem.

## Suites mandatórias
- `Backend/Tests/BarberSync.Tests`: xUnit + cobertura.
- `tests/integration`: API + Postgres + Testcontainers.
- `tests/e2e`: Playwright com cenários:
  - Admin: login, KPIs, criação/edição de serviço.
  - Mobile: agendamento inteligente + fidelidade.
  - Totem: autoatendimento + pagamento.

## Qualidade Contínua (CI/CD)
- Pipeline valida build, testes, cobertura e artefatos.
- Bloqueio de merge se cobertura < 90%.
- Publicação de relatórios (Cobertura + E2E screenshots/videos).

## Observabilidade da qualidade
- Métricas de falha por módulo.
- Tempo médio de execução de suites.
- Alertas automáticos para regressões críticas.
