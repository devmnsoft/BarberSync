# Evidência Release Candidate BarberSync

## Ambiente
- OS:
- Docker:
- Docker Compose:
- dotnet:
- psql:
- node:
- npm:
- gh auth:

## PRs
- PR #201 fechado? sim/não
- PRs abertos restantes:

## Gate
- run local ou Actions:
- commit:
- status final:

## Build
- dotnet restore:
- build Debug:
- build Release:

## SQL
- primeira aplicação:
- segunda aplicação:
- schema validation:

## Runtime
- API:
- /health:
- production-smoke:

## Frontend
- node --check:
- Mobile smoke:
- Totem smoke:
- Totem build:

## Demo/Fallback
- scan:
- pendências operacionais:

## GO/NO-GO
- decisão:
- motivo:

## Readiness seed and authenticated HTTP evidence

Required POS lines: `EVIDENCE:AUTH_SMOKE_SERVICE_ORDER:PASS`, `EVIDENCE:AUTH_SMOKE_PAYMENT:PASS`,
`EVIDENCE:AUTH_SMOKE_STOCK_MOVEMENT:PASS`, `EVIDENCE:AUTH_SMOKE_CASH_MOVEMENT:PASS`,
`EVIDENCE:AUTH_SMOKE_FINANCIAL_ENTRY:PASS`, `EVIDENCE:AUTH_SMOKE_COMMISSION:PASS`, and
`EVIDENCE:AUTH_SMOKE_POS:PASS`. Paste the unmodified gate log; absence and any non-PASS value are NO-GO.

- `EVIDENCE:READINESS_SEED:PASS`
- `EVIDENCE:AUTHENTICATED_PRODUCTION_SMOKE:PASS`
- `EVIDENCE:AUTH_SMOKE_LOGIN:PASS`
- `EVIDENCE:AUTH_SMOKE_DASHBOARD:PASS`
- `EVIDENCE:AUTH_SMOKE_MOBILE_CLIENT:PASS`
- `EVIDENCE:AUTH_SMOKE_MOBILE_PROFESSIONAL:PASS`
- `EVIDENCE:AUTH_SMOKE_NOTIFICATIONS:PASS`
- `EVIDENCE:AUTH_SMOKE_STOCK:PASS`
- `EVIDENCE:AUTH_SMOKE_CASH_REGISTER:PASS`

Every exact PASS marker and its log is mandatory. Missing logs, `SKIPPED`, and environment errors never qualify as GO.

## Contratos estáticos de readiness

- Comando: `./scripts/validate-readiness-contracts.sh`
- Evidência obrigatória: `EVIDENCE:READINESS_CONTRACTS_STATIC:PASS`
- Resultado: `AUSENTE/REPROVADO` até execução bem-sucedida. A validação textual não substitui build .NET ou PostgreSQL real.

## Evidência hospedada — Sprint 29

- URL do run de `Production Readiness`:
- artifact `production-readiness-<run-id>` baixado: sim/não
- `artifacts/release-evidence/go-no-go.md` presente: sim/não
- GitHub Step Summary presente: sim/não
- markers passados:
- markers ausentes:
- markers falhos:

O Step Summary e o artifact publicado mesmo em falha são o caminho oficial de diagnóstico. Artifact, resumo ou log obrigatório ausente implica **NO-GO**. O estado permanece **NO-GO** até GitHub Actions ou outro executor Docker real produzir todos os markers PASS.

## Pacote local de evidência

- Arquivo ZIP:
- Máquina:
- Data/hora:
- Branch:
- Commit:
- Resultado:
- Primeiro marker ausente/falho:
- Próxima correção:
