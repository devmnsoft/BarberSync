# Auditoria rápida BarberSync (2026-05-27)

## Inventário técnico levantado
- Controllers API mapeados (auth, dashboard, clients, professionals, services, appointments, stock, payments, loyalty, reviews, copilot, recognition, etc.).
- Controllers AdminWeb: Account, Dashboard, Help, Home, CrudPages.
- Controllers PublicWeb: Home.
- Controllers KioskWeb: Home, Kiosk.
- Views presentes para Admin, PublicWeb e Kiosk, com componentes compartilhados já existentes.
- Seed SQL principal confirmado em `ScriptsSQL/barber_full_database_postgresql.sql`.

## Principais achados
1. Swagger já tinha correções de upload (`ServiceRecognitionUploadRequest`, `Consumes`, `FileUploadOperationFilter`).
2. Swagger estava condicionado apenas ao ambiente Development, com risco de indisponibilidade em cenários de demo/container.
3. Endpoints mínimos de campanhas/cupons ainda não estavam explicitamente expostos em `api/campaigns` e `api/coupons`.

## Evoluções aplicadas neste ciclo
1. Swagger habilitado independentemente de ambiente para garantir abertura em demos.
2. Endpoint de campanhas criado com GET/POST e payload amigável.
3. Endpoint de cupons criado com GET/POST e payload amigável.

## Próximos passos (backlog recomendado)
- Expandir CRUDs de Admin com forms avançados por entidade.
- Fortalecer dashboards com gráficos/consultas agregadas reais.
- Validar pipeline Docker + testes e testes de smoke E2E nas URLs de demonstração.
- Completar cobertura mobile para telas de histórico/cashback/notificações com dados reais.
