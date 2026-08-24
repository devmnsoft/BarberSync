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
